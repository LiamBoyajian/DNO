using Godot;
using System;
using Main.InventoryAssets;
using static System.Security.Cryptography.RandomNumberGenerator;
//using AcidBases = System.;
public partial class DNA : GodotObject
{
    private ulong _binaryString;
    
    public DNA(RandomNumberGenerator hi)
    {
        _binaryString = ((ulong)hi.Randi()) << 32;
        _binaryString += (ulong)hi.Randi();
    }

    public String GetDna(ulong binaryString)
    {
        String result = "";
        ulong temp = _binaryString;
        while (temp != 0)
        {
            result += ((AcidBases)(temp & 3)).ToString();
            temp >>= 2;
        }

        return result;
    }
    
    
}
