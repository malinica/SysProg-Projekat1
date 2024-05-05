using System;
using System.Collections.Generic;
using System.Threading;

public struct FileAndDate
{
    public byte[] bytes;
    public DateTime created;
};
public class Kes
{
    private readonly ReaderWriterLockSlim _kesLock; 
    private readonly Dictionary<string, FileAndDate> _kes;
    private const int kesKapacitet = 100;

    public Kes()
    {
        _kesLock = new ReaderWriterLockSlim();
        _kes = new Dictionary<string, FileAndDate>(kesKapacitet);
    }

    public void DodajUKes(string key, byte[] value, int timeout)
    {
        if (!_kesLock.TryEnterWriteLock(timeout)) return; 

        if (_kes.ContainsKey(key))
        {
            throw new Exception("Element je vec u kesu.\n");
        }
        FileAndDate fileAndDate = new FileAndDate()
        {
            bytes = value,
            created = DateTime.UtcNow
        };
        _kes.Add(key, fileAndDate);
        Console.WriteLine("UPISANO U KES\n");
        _kesLock.ExitWriteLock();
    }

    public void StampajSadrzajKesa()
    {
        _kesLock.EnterReadLock();
        try
        {
            Console.WriteLine("Sadrzaj kesa:\n");
            foreach (var k in _kes)
            {
                Console.WriteLine($"Kljuc: {k.Key}, Vrednost: {BitConverter.ToString(k.Value.bytes)}, Kreirano: {k.Value.created}\n");
            }
        }
        finally
        {
            _kesLock.ExitReadLock();
        }
    }
}

