using System;
using System.Collections.Generic;
using System.Threading;

namespace Projekat1
{
    public class Kes
    {
        private readonly ReaderWriterLockSlim _kesLock;
        private readonly Dictionary<string, Stavka> _kes;
        private const int kesKapacitet = 10;

        public Kes()
        {
            _kesLock = new ReaderWriterLockSlim();
            _kes = new Dictionary<string, Stavka>(kesKapacitet);
        }

        public void DodajUKes(string key, int ukupno1, string podaci1, int timeout)
        {
            if (!_kesLock.TryEnterWriteLock(timeout)) return;

            if (_kes.ContainsKey(key))
            {
                throw new Exception("Element je vec u kesu.\n");
            }
            Stavka stavka = new Stavka()
            {
                Ukupno = ukupno1,
                Podaci = podaci1,
                VremeKreiranja = DateTime.UtcNow
            };
            _kes.Add(key, stavka);
            Console.WriteLine("UPISANO U KES!\n");
            _kesLock.ExitWriteLock();
            StampajStavkuKesa(key);
            //            StampajSadrzajKesa();
        }
        public void StampajStavkuKesa(string Key)
        {
            _kesLock.EnterReadLock();

            try
            {
                Stavka k = _kes[Key];
                Console.WriteLine($"Kljuc je: {Key}, kreirano: {k.VremeKreiranja}, ima ukupno {k.Ukupno} podataka: {k.Podaci}, \n");
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                _kesLock.ExitReadLock();
            }


        }
        public void StampajSadrzajKesa()
        {
            _kesLock.EnterReadLock();
            try
            {
                Console.WriteLine("Sadrzaj kesa:\n");
                foreach (var k in _kes)
                {
                    Console.WriteLine($"Kljuc je: {k.Key}, kreirano: {k.Value.VremeKreiranja}, ima ukupno {k.Value.Ukupno} podataka: {k.Value.Podaci}, \n");
                }
            }
            finally
            {
                _kesLock.ExitReadLock();
            }
        }

        public void ObrisiIzKesa(string key)
        {
            _kesLock.EnterReadLock();
            if (!_kes.Remove(key))
            {
                throw new Exception("Element ne postoji u kesu.");
            }
            _kesLock.ExitReadLock();
        }

        public Stavka CitajIzKesa(string key)
        {
            _kesLock.EnterReadLock();
            try
            {
                return _kes[key];
            }
            finally
            {
                _kesLock.ExitReadLock();
            }
        }

        public bool ImaKljuc(string key)
        {
            if (_kes.ContainsKey(key))
            {
                if (_kes[key].VremeKreiranja.AddMinutes(10) >= DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    ObrisiIzKesa(key);
                }
            }
            return false;
        }
    }

}