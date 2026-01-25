using Godot;
using System;
using Main.InventoryAssets;
using static System.Security.Cryptography.RandomNumberGenerator;

//using AcidBases = System.;
public partial class DNA : GodotObject
{
    private ulong _binaryString;
    private bool _realDNA;

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

    /**
     * Used for dna segments
     */
    public static String GetDNAString(byte binaryString)
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

    public String getDNAString()
    {
        return GetDnaString(_binaryString);
    }


    public DNA Clone()
    {
        return new DNA(_binaryString);
    }

    public bool getRealDNA()
    {
        return _realDNA;
    }
}