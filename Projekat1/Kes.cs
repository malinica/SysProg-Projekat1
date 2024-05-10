using System;
using System.Collections.Generic;
using System.Threading;

namespace Projekat1
{
    public class Kes
    {
        private ReaderWriterLockSlim _kesLock;
        private Dictionary<string, Stavka> _kes;
        private const int kesKapacitet = 2;
        private int mestoCitanja, mestoPisanja;
        private string[] red;
        public Kes()
        {
            _kesLock = new ReaderWriterLockSlim();
            _kes = new Dictionary<string, Stavka>(kesKapacitet);
            mestoCitanja = mestoPisanja = 0;
            red = new string[kesKapacitet];
        }

        public void DodajUKes(string key, int ukupno1, string podaci1, int timeout)
        {
            try
            {
                if (!_kesLock.TryEnterWriteLock(timeout))
                    return;
                if (_kes.ContainsKey(key))
                    throw new Exception("Element je vec u kesu.\n");
                Stavka stavka = new Stavka(ukupno1, podaci1);


                if (_kes.Count != kesKapacitet)
                {
                    red[mestoPisanja] = key;
                    mestoPisanja = (++mestoPisanja) % kesKapacitet;
                }
                else
                {
                    string kljucZaBrisanje = red[mestoCitanja];
                    mestoPisanja = mestoCitanja++;
                    red[mestoPisanja++] = key;
                    mestoPisanja = mestoPisanja % kesKapacitet;
                    mestoCitanja = mestoCitanja % kesKapacitet;
                    _kes.Remove(kljucZaBrisanje);
                }
                _kes.Add(key, stavka);
               // TrenutnoStanje();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _kesLock.ExitWriteLock();
            }
        }
        public void TrenutnoStanje()
        {
            Console.WriteLine("Kljucevi koji se nalaze u kesu su:");
            foreach (var keys in _kes.Keys)
            {
                Console.WriteLine($" {keys} ");
            }
            Console.WriteLine("Redosled za izbacivanje iz kesa:");
            if (_kes.Count == kesKapacitet)
            {
                int brojac = 0;
                while (brojac < kesKapacitet)
                {
                    Console.WriteLine($"{brojac}. {red[(mestoCitanja + brojac) % kesKapacitet]}");
                    brojac++;
                }

                Console.WriteLine("\n\n");
            }
            else
            {
                for (int i = 0; i != mestoPisanja; i++)
                    Console.WriteLine($"{i}. {red[(i + mestoCitanja % kesKapacitet)]} \n");
                Console.WriteLine("\n\n");
            }
        }

        public void StampajStavkuKesa(string Key)
        {
            _kesLock.EnterReadLock();
            try
            {
                Stavka k = _kes[Key];
                Console.WriteLine($"Kljuc je: {Key}, {k.ToString()} \n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    Console.WriteLine($"Kljuc je: {k.Key}, {k.ToString()} \n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _kesLock.ExitReadLock();
            }
        }

        public void ObrisiIzKesa(string key)
        {
            _kesLock.EnterWriteLock();
            try
            {
                if (_kes.Count != 0 && ImaKljuc(key))
                {
                    for (int i = mestoCitanja; i < mestoPisanja - 1; i++)
                    {
                        red[i] = red[i + 1];
                    }
                    red[mestoPisanja--] = null;
                    _kes.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _kesLock.ExitWriteLock();
            }
        }

        public void ObrisiCeoKes()
        {
            _kesLock.EnterWriteLock();
            try
            {
                _kes.Clear();
                mestoPisanja = mestoCitanja = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                _kesLock.ExitWriteLock();
            }
        }

        public Stavka CitajIzKesa(string key)
        {
            _kesLock.EnterReadLock();
            try
            {
                return _kes[key];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                _kesLock.ExitReadLock();
            }
        }

        public bool ImaKljuc(string key)
        {
            _kesLock.EnterReadLock();
            try
            {
                if (_kes.ContainsKey(key))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                _kesLock.ExitReadLock();
            }
        }
    }

}