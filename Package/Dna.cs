using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;

namespace Main.Package;

public enum NucleotideBase : byte
{
    [Display(Name = "Adenine")] A,
    [Display(Name = "Thymine")] T,
    [Display(Name = "Guanine")] G,
    [Display(Name = "Cytosine")] C
}

public class Dna : IEnumerable<NucleotideBase>
{
    protected readonly byte[]
        DnaBinary; //four bases per byte -- might be good to swap to a NucleotideBase[] (opting for ram here)

    public readonly int Length; //total bases; excluding trailing bases from final byte

    public Dna(Random random, int acidCount)
    {
        if (acidCount < 3) throw new ArgumentOutOfRangeException(nameof(acidCount));
        ArgumentOutOfRangeException.ThrowIfNegative(acidCount);
        Length = acidCount;
        DnaBinary = new byte[acidCount / 4];
        random.NextBytes(DnaBinary); //fill array
        DnaBinary[0] &= 3;
        DnaBinary[0] |= ((byte)NucleotideBase.T << 4) | ((byte)NucleotideBase.G << 2); //Coding ATG(AUG) to the start.
    }


    public object Clone()
    {
        throw new NotImplementedException();
    }

    public IEnumerator<NucleotideBase> GetEnumerator()
    {
        for (var i = 0; i < Length; i++)
        {
            //first step: place in the byte
            //2: 

            //final: get the value using & 3 and convert that number to nucBase
            yield return (NucleotideBase)(DnaBinary[i / 4] >> (6 - (2 * (i % 4))) & 3);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}