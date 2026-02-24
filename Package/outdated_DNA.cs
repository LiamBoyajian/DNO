using System;
using System.Collections.Generic;
using Godot;
using Main.InventoryAssets;

//using AcidBases = System.;
namespace Main.Package;

public partial class outdated_DNA : IEnumerable<AcidBases>
{
    private ulong _binaryString;
    private bool _realDNA;
    private byte _dnaBytes;

    public const int
        DnaLength = 32; //im hard coding this because if I increase the OutdatedDna size I'll need a different structure meaning any solution here will immediately become obsolete. 


    public outdated_DNA(RandomNumberGenerator hi)
    {
        _realDNA = true;
        _binaryString = ((ulong)hi.Randi()) << 32;
        _binaryString += (ulong)hi.Randi();
    }

    private outdated_DNA(ulong binaryString)
    {
        _realDNA = false;
        _binaryString = binaryString;
    }

    public static String GetDnaString(ulong binaryString)
    {
        String result = "";
        ulong temp = binaryString;
        while (temp != 0)
        {
            result += ((AcidBases)(temp & 3)).ToString();
            temp >>= 2;
        }

        return result;
    }

    public override string ToString()
    {
        return GetDnaString(_binaryString);
    }

    /**
 * Used for OutdatedDna segments
 */
    public static string GetDnaString(byte binaryString)
    {
        String result = "";
        byte temp = binaryString;
        while (temp != 0)
        {
            result += ((AcidBases)(temp & 3)).ToString();
            temp >>= 2;
        }

        return result;
    }

    public String GetDnaString()
    {
        return GetDnaString(_binaryString);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<AcidBases> GetEnumerator()
    {
        outdated_DNA snapshot = this.Clone();
        for (var i = 0; i < DnaLength; i++)
            yield return GetDnaAtIndex(snapshot, i);
    }

    public outdated_DNA Clone()
    {
        return new outdated_DNA(_binaryString);
    }

    public bool GetRealDna()
    {
        return _realDNA;
    }


    public AcidBases GetDnaAtIndex(int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "index is less than zero");
        if (index >= DnaLength)
            throw new ArgumentOutOfRangeException(nameof(index), "index is greater than the max index");

        ulong temp = 3ul << (index * 2); //something like 0000000000000000000000110000000000000000
        temp &= _binaryString;
        return (AcidBases)(temp >> (index * 2)); //I love this system so much
    }

    public AcidBases GetDnaAtIndex(outdated_DNA givenOutdatedDna, int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "index is less than zero");
        if (index >= DnaLength)
            throw new ArgumentOutOfRangeException(nameof(index), "index is greater than the max index");

        ulong temp = 3ul << (index * 2); //something like 0000000000000000000000110000000000000000
        temp &= givenOutdatedDna._binaryString;
        return (AcidBases)(temp >> (index * 2)); //I love this system so much
    }

    public int GetLength()
    {
        return DnaLength;
    }
}