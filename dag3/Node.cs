using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dag3
{
    public class Node
    {
        public byte Værdi;       // Bruges kun i blade
        public int Frekvens;
        public int Id;           // Til at afgøre uafgjort: blade = byte-værdien, indre noder = 256, 257, ...
        public Node? Venstre;
        public Node? Højre;

        public bool ErBlad
        {
            get { return Venstre == null && Højre == null; }
        }

        // Konstruktør til blade
        public Node(byte værdi, int frekvens)
        {
            Værdi = værdi;
            Frekvens = frekvens;
            Id = værdi;
        }

        // Konstruktør til indre noder
        public Node(Node venstre, Node højre, int id)
        {
            Venstre = venstre;
            Højre = højre;
            Frekvens = venstre.Frekvens + højre.Frekvens;
            Id = id;
        }

        // lav evt en ToString
    }
}
