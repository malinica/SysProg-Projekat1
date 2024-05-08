using System;

namespace Projekat1
{
    public class Stavka
    {
        public int Ukupno { get; set; }
        public string Podaci { get; set; }
        public DateTime VremeKreiranja { get; set; }

        public Stavka()
        {
            VremeKreiranja = DateTime.UtcNow;
        }
        public Stavka(int u, string p)
        {
            Ukupno = u;
            Podaci = p;
            VremeKreiranja = DateTime.UtcNow;
        }
        public override string ToString()
        {
            return $" kreirano: {VremeKreiranja}, ima ukupno {Ukupno} podataka: {Podaci}";
        }
    }
}
