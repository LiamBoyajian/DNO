using System;
using System.Collections.Generic;
using Godot;
using Main.InventoryAssets;

//using AcidBases = System.;
namespace Main.Package;

public partial class DNA : IEnumerable<AcidBases>
{
    private ulong _binaryString;
    private bool _realDNA;


    public const int
        DnaLength = 32; //im hard coding this because if I increase the dna size I'll need a different structure meaning any solution here will immediately become obsolete. 


    public DNA(RandomNumberGenerator hi)
    {
        _realDNA = true;
        _binaryString = ((ulong)hi.Randi()) << 32;
        _binaryString += (ulong)hi.Randi();
    }

    private DNA(ulong binaryString)
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
 * Used for dna segments
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
        DNA snapshot = this.Clone();
        for (var i = 0; i < DnaLength; i++)
            yield return GetDnaAtIndex(snapshot, i);
    }

    public DNA Clone()
    {
        return new DNA(_binaryString);
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

    public AcidBases GetDnaAtIndex(DNA givenDna, int index)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "index is less than zero");
        if (index >= DnaLength)
            throw new ArgumentOutOfRangeException(nameof(index), "index is greater than the max index");

        ulong temp = 3ul << (index * 2); //something like 0000000000000000000000110000000000000000
        temp &= givenDna._binaryString;
        return (AcidBases)(temp >> (index * 2)); //I love this system so much
    }

    public int GetLength()
    {
        return DnaLength;
    }
}