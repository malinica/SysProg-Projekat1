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
}

