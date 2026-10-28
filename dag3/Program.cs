using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace dag3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Manuelle forsøg på at oprette noder til testformål.
            byte a = (byte)'a';
            byte b = (byte)'b';

            Node blad1 = new Node(a, 1);
            Node blad2 = new Node(b, 1);

            Console.WriteLine(blad1);
            Console.WriteLine($"{blad1.Værdi}, {blad1.Frekvens}, {blad1.Id}");
            Console.WriteLine($"{blad2.Værdi}, {blad2.Frekvens}, {blad2.Id}");

            int nodeID = 256; // bin 100000000
            Node node1 = new Node(blad1, blad2, nodeID++);

            Console.WriteLine(node1);
            Console.WriteLine($"{node1.Værdi}, {node1.Frekvens}, {node1.Id}, {node1.Venstre.Id}, {node1.Højre.Id}");

            //Selve handlingen: Hent teksten, tæl frekvenserne, byg træet og udskriv på skærmen.
            string tekst = "abracadabra";
            byte[] byteTekst = Encoding.UTF8.GetBytes(tekst);
            var frekvenser = TælFrekvenser(byteTekst);

            Node rootNode = BygTræ(frekvenser);
            Console.WriteLine("--- HUFFMAN TRÆ ---");
            UdskrivTræ(rootNode);

            //Vi opretter et tomt kodearray med 256 elementer.
            string[] koder = new string[256];

            //Vi kalder metoden med rodknuden og en tom startsti ("").
            DanKoder(rootNode, "", koder);

            //Lad os udskrive det på skærmen for at se, hvilken kode der er tildelt hvilket bogstav
            Console.WriteLine("\n--- OPGAVE 6: Bogstaver Koder ---");
            foreach(var kvp in frekvenser)
            {
                char tegn = (char)kvp.Key;
                Console.WriteLine($"'{tegn}' Kod af bogstaver: {koder[kvp.Key]}");
            }

            string komprimeret = Komprimer(byteTekst, koder);

            Console.WriteLine("\n--- OPGAVE 7: KOMPRIMERET STRENG ---");
            Console.WriteLine(komprimeret);
            Console.WriteLine($"Komprimeret størrelse: {komprimeret.Length} bits");


            string dekomprimeret = Dekomprimer(komprimeret, rootNode);
            Console.WriteLine("\n--- OPGAVE 8: DEKOMPRIMERET TEKST ---");
            Console.WriteLine(dekomprimeret);

            // Opgave 9: Test (Round-trip kontrol)
            if (tekst == dekomprimeret)
            {
                Console.WriteLine("\n[SUCCESS] Test OK! Den dekodede tekst er nøjagtig den samme som originalen.");
            }
            else
            {
                Console.WriteLine("\n[FEJL] Decompression stemmer ikke overens med originalteksten!");
            }
        }
        //opgave5
        static void UdskrivTræ(Node node, string indryk = "")
        {
            if (node == null) return; // Hvis knuden er tom, afslut

            if (node.ErBlad)
            {
                //Hvis vi er nået til slutningen af stien (bladet), udskriv bogstavet og dets frekvens.
                char tegn = (char)node.Værdi;
                Console.WriteLine($"{indryk}Blad: '{tegn}' (Frekvens: {node.Frekvens})");
            }
            else
            {
                //Hvis det er en intern knude, udskriv først dig selv, og gå derefter til venstre og højre barn.
                Console.WriteLine($"{indryk}Node [Id: {node.Id}, Frekvens: {node.Frekvens}]");
                Console.WriteLine($"{indryk}├── Venstre:");
                UdskrivTræ(node.Venstre, indryk + "│   ");
                Console.WriteLine($"{indryk}└── Højre:");
                UdskrivTræ(node.Højre, indryk + "    ");
            }
        }

        //opgave6
        static void DanKoder(Node? node, string sti, string[] koder)
        {
            if (node == null) return;

            //Hvis vi har nået bladet, gem stien (koden) på det relevante byte-indeks.
            if(node.ErBlad)
            {
                //Tip: Hvis teksten kun består af én type bogstav (f.eks. "aaaa"), kan stien forblive tom
                //(""). I dette tilfælde tildeles "0", så den ikke er tom.
                koder[node.Værdi] = string.IsNullOrEmpty(sti) ? "0" : sti;
                return;
            }
            //Tilføj "0" til koden, når du går til det venstre barn.
            DanKoder(node.Venstre, sti + "0", koder);
            //Tilføj "1" til koden, når du går til det venstre barn.
            DanKoder(node.Højre, sti + "1", koder);

        }

        //opgave7
        static string Komprimer(byte[] data, string[] koder)
        {
            StringBuilder sb = new StringBuilder();
            foreach(byte b in data)
            {
                sb.Append(koder[b]);//Tilføj Huffman-koden for hver byte.
            }
            return sb.ToString();
        }

        //opgave 8
        static string Dekomprimer(string binærStreng,Node root)
        {
            StringBuilder sb = new StringBuilder();
            Node nuværende = root;

            //ærligt tilfælde: Hvis teksten kun består af én type bogstav (Træ med ét blad)
            if (root.ErBlad)
            {
                for(int i = 0;i < binærStreng.Length; i++)
                {
                    sb.Append((char)root.Værdi);
                }
                return sb.ToString() ;
            }

            foreach(char bit in binærStreng)
            {
                if(bit == '0')
                {
                    nuværende = nuværende.Venstre!;
                }
                else if(bit == '1')
                {
                    nuværende = nuværende.Højre!;
                }

                //Hvis vi har nået bladet, gem bogstavet og vend tilbage til roden.
                if (nuværende.ErBlad)
                {
                    sb.Append((char)nuværende.Værdi) ;
                    nuværende = root;
                }
            }
            return sb.ToString();
        }

        static Node? BygTræ(Dictionary<byte, int> frekrenser)
        {
            var noder = new List<Node>();

            //For hvert bogstav i ordbogen opretter vi en bladknude (blad) og tilføjer den til listen.
            foreach ((byte k, int v) in frekrenser)
            {
                var node = new Node(k, v);
                noder.Add(node);
            }

            int næsteId = 256; // For interne knuder tildeler vi ID'er, der starter fra 256.

            // Løkken kører, indtil der kun er 1 knude (root) tilbage på listen.
            while (noder.Count > 1)
            {

                //VIGTIG REGEL: Sorter først fra mindste til største efter frekvens.
                //Hvis frekvenserne er ens, sorter efter ID (Uafgjort-regel).
                noder.Sort((a, b) => a.Frekvens != b.Frekvens
                    ? a.Frekvens.CompareTo(b.Frekvens)
                    : a.Id.CompareTo(b.Id));

                //Vælg de første to knuder med den laveste frekvens.
                Node mindst = noder[0];
                Node næstMindst = noder[1];

                //Kombiner disse to under en ny overknude.
                Node samler = new Node(mindst, næstMindst, næsteId++);

                //Tilføj den nye knude til listen.
                noder.Add(samler);
                // fjern fra liste
                noder.RemoveAt(0);
                noder.RemoveAt(0);
            }

            return noder[0];
        }

        static Dictionary<byte, int> TælFrekvenser(byte[] data)
        {
            Dictionary<byte, int> d = new Dictionary<byte, int>();
            foreach (byte b in data)
            {
                if (!d.ContainsKey(b))
                {
                    d.Add(b, 0); //Hvis bogstavet ikke er blevet tilføjet før, skal det tilføjes til ordbogen med 0.
                }

                d[b]++; //Forøg antallet af bogstaver med 1.
            }
            return d; //Eksempel på output: {'a': 5, 'b': 2, 'r': 2, 'c': 1, 'd': 1}
        }



    }
}
