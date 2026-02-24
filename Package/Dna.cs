using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;

namespace Main.Package;

public enum AminoAcid
{
    // Nonpolar, Aliphatic
    [Display(Name = "Alanine")] Ala,
    [Display(Name = "Glycine")] Gly,
    [Display(Name = "Isoleucine")] Ile,
    [Display(Name = "Leucine")] Leu,
    [Display(Name = "Proline")] Pro,
    [Display(Name = "Valine")] Val,

    // Polar, Uncharged
    [Display(Name = "Cysteine")] Cys,
    [Display(Name = "Glutamine")] Gln,
    [Display(Name = "Asparagine")] Asn,
    [Display(Name = "Serine")] Ser,
    [Display(Name = "Threonine")] Thr,
    [Display(Name = "Tyrosine")] Tyr,

    // Aromatic
    [Display(Name = "Phenylalanine")] Phe,
    [Display(Name = "Tryptophan")] Trp,

    // Positively Charged
    [Display(Name = "Arginine")] Arg,
    [Display(Name = "Histidine")] His,
    [Display(Name = "Lysine")] Lys,

    // Negatively Charged
    [Display(Name = "Aspartic Acid")] Asp,
    [Display(Name = "Glutamic Acid")] Glu,

    // Special
    [Display(Name = "Methionine (Start)")] Met,
    [Display(Name = "Stop Codon")] Stop
}

public enum AcidBases
{
    [Display(Name = "Adenine")] A,
    [Display(Name = "Thymine")] T,
    [Display(Name = "Guanine")] G,
    [Display(Name = "Cytosine")] C
}

public class Dna : IEnumerable<AcidBases>
{
    private byte[] _dnaBinary; //four bases per byte
    private int _length; //total bases; excluding trailing bases from final byte

    public Dna(Random random, int acidCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(acidCount);
        _length = acidCount;
        _dnaBinary = new byte[acidCount / 4 + 1];

        random.NextBytes(_dnaBinary); //fill array
    }


    public object Clone()
    {
        throw new NotImplementedException();
    }

    public IEnumerator<AcidBases> GetEnumerator()
    {
        for (var i = 0; i < _length; i++)
        {
            yield return (AcidBases)(_dnaBinary[i / 4] >> (6 - (2 * (i % 4))) & 3);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}