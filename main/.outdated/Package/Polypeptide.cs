using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Godot;

namespace Main.Package;

public enum AminoAcid : byte
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

public static class AminoAcidExtensions
{
    public enum AminoAttributes
    {
        Positive,
        Negative,
        Polar,
        Aromatic,
        NonPolar
    }

    public enum AminoHydro
    {
        Hydrophobic,
        Hydrophilic,
        Neutral
    }

    public static AminoAttributes? GetCharge(this AminoAcid aa) => aa switch
    {
        // Positively Charged
        AminoAcid.Arg or AminoAcid.His or AminoAcid.Lys => AminoAttributes.Positive,

        // Negatively Charged
        AminoAcid.Asp or AminoAcid.Glu => AminoAttributes.Negative,

        //Polar 
        AminoAcid.Cys or AminoAcid.Gln or AminoAcid.Asn or AminoAcid.Ser or AminoAcid.Thr or AminoAcid.Tyr =>
            AminoAttributes
                .Polar,

        //Aromatic
        AminoAcid.Phe or AminoAcid.Trp => AminoAttributes.Aromatic,

        //Non-polar
        AminoAcid.Ala or AminoAcid.Gly or AminoAcid.Ile or AminoAcid.Leu or AminoAcid.Pro or AminoAcid.Val
            or AminoAcid.Met => AminoAttributes.NonPolar,

        AminoAcid.Stop => null,

        _ => throw new Exception($"Unknown AminoAcid {aa}"),
    };

    /**
     * Most uncertain about this one scientifically but I never claimed to be a biochemist.
     */
    public static AminoHydro? GetHydrophobicity(this AminoAcid aa) => aa switch
    {
        // Hydrophobic (Non-polar and Aromatic groups)
        AminoAcid.Ala or AminoAcid.Gly or AminoAcid.Ile or
            AminoAcid.Leu or AminoAcid.Pro or AminoAcid.Val or
            AminoAcid.Phe => AminoHydro.Hydrophobic,

        // Hydrophilic (Polar, Acidic, and Basic groups)
        AminoAcid.Cys or AminoAcid.Gln or AminoAcid.Asn or
            AminoAcid.Ser or AminoAcid.Thr or
            AminoAcid.Arg or AminoAcid.His or
            AminoAcid.Asp or AminoAcid.Glu => AminoHydro.Hydrophilic,

        // Neutral
        AminoAcid.Tyr or AminoAcid.Lys or AminoAcid.Trp or AminoAcid.Met => AminoHydro.Neutral,

        AminoAcid.Stop => null,

        _ => throw new Exception("Unknown AminoAcid"),
    };
}

public class Polypeptide : IEnumerable<AminoAcid>
{
    protected AminoAcid[] Residues; //six bits per residue

    public int Length;

    // Using a static array for O(1) lookup with zero hashing overhead
    private static readonly AminoAcid[] CodonLookup = new AminoAcid[64];

    static Polypeptide()
    {
        // A=0, T=1, G=2, C=3

        // Family: Axx (0-15)
        CodonLookup[0] = AminoAcid.Lys; // AAA (00 00 00)
        CodonLookup[1] = AminoAcid.Asn; // AAT (00 00 01)
        CodonLookup[2] = AminoAcid.Lys; // AAG (00 00 10)
        CodonLookup[3] = AminoAcid.Asn; // AAC (00 00 11)
        CodonLookup[4] = AminoAcid.Ile; // ATA (00 01 00)
        CodonLookup[5] = AminoAcid.Ile; // ATT (00 01 01)
        CodonLookup[6] = AminoAcid.Met; // ATG (00 01 10)
        CodonLookup[7] = AminoAcid.Ile; // ATC (00 01 11)
        CodonLookup[8] = AminoAcid.Arg; // AGA (00 10 00)
        CodonLookup[9] = AminoAcid.Ser; // AGT (00 10 01)
        CodonLookup[10] = AminoAcid.Arg; // AGG (00 10 10)
        CodonLookup[11] = AminoAcid.Ser; // AGC (00 10 11)
        CodonLookup[12] = AminoAcid.Thr; // ACA (00 11 00)
        CodonLookup[13] = AminoAcid.Thr; // ACT (00 11 01)
        CodonLookup[14] = AminoAcid.Thr; // ACG (00 11 10)
        CodonLookup[15] = AminoAcid.Thr; // ACC (00 11 11)

        // Family: Txx (16-31)
        CodonLookup[16] = AminoAcid.Stop; // TAA (01 00 00)
        CodonLookup[17] = AminoAcid.Tyr; // TAT (01 00 01)
        CodonLookup[18] = AminoAcid.Stop; // TAG (01 00 10)
        CodonLookup[19] = AminoAcid.Tyr; // TAC (01 00 11)
        CodonLookup[20] = AminoAcid.Leu; // TTA (01 01 00)
        CodonLookup[21] = AminoAcid.Phe; // TTT (01 01 01)
        CodonLookup[22] = AminoAcid.Leu; // TTG (01 01 10)
        CodonLookup[23] = AminoAcid.Phe; // TTC (01 01 11)
        CodonLookup[24] = AminoAcid.Stop; // TGA (01 10 00)
        CodonLookup[25] = AminoAcid.Cys; // TGT (01 10 01)
        CodonLookup[26] = AminoAcid.Trp; // TGG (01 10 10)
        CodonLookup[27] = AminoAcid.Cys; // TGC (01 10 11)
        CodonLookup[28] = AminoAcid.Ser; // TCA (01 11 00)
        CodonLookup[29] = AminoAcid.Ser; // TCT (01 11 01)
        CodonLookup[30] = AminoAcid.Ser; // TCG (01 11 10)
        CodonLookup[31] = AminoAcid.Ser; // TCC (01 11 11)

        // Family: Gxx (32-47)
        CodonLookup[32] = AminoAcid.Glu; // GAA (10 00 00)
        CodonLookup[33] = AminoAcid.Asp; // GAT (10 00 01)
        CodonLookup[34] = AminoAcid.Glu; // GAG (10 00 10)
        CodonLookup[35] = AminoAcid.Asp; // GAC (10 00 11)
        CodonLookup[36] = AminoAcid.Val; // GTA (10 01 00)
        CodonLookup[37] = AminoAcid.Val; // GTT (10 01 01)
        CodonLookup[38] = AminoAcid.Val; // GTG (10 01 10)
        CodonLookup[39] = AminoAcid.Val; // GTC (10 01 11)
        CodonLookup[40] = AminoAcid.Gly; // GGA (10 10 00)
        CodonLookup[41] = AminoAcid.Gly; // GGT (10 10 01)
        CodonLookup[42] = AminoAcid.Gly; // GGG (10 10 10)
        CodonLookup[43] = AminoAcid.Gly; // GGC (10 10 11)
        CodonLookup[44] = AminoAcid.Ala; // GCA (10 11 00)
        CodonLookup[45] = AminoAcid.Ala; // GCT (10 11 01)
        CodonLookup[46] = AminoAcid.Ala; // GCG (10 11 10)
        CodonLookup[47] = AminoAcid.Ala; // GCC (10 11 11)

        // Family: Cxx (48-63)
        CodonLookup[48] = AminoAcid.Gln; // CAA (11 00 00)
        CodonLookup[49] = AminoAcid.His; // CAT (11 00 01)
        CodonLookup[50] = AminoAcid.Gln; // CAG (11 00 10)
        CodonLookup[51] = AminoAcid.His; // CAC (11 00 11)
        CodonLookup[52] = AminoAcid.Leu; // CTA (11 01 00)
        CodonLookup[53] = AminoAcid.Leu; // CTT (11 01 01)
        CodonLookup[54] = AminoAcid.Leu; // CTG (11 01 10)
        CodonLookup[55] = AminoAcid.Leu; // CTC (11 01 11)
        CodonLookup[56] = AminoAcid.Arg; // CGA (11 10 00)
        CodonLookup[57] = AminoAcid.Arg; // CGT (11 10 01)
        CodonLookup[58] = AminoAcid.Arg; // CGG (11 10 10)
        CodonLookup[59] = AminoAcid.Arg; // CGC (11 10 11)
        CodonLookup[60] = AminoAcid.Pro; // CCA (11 11 00)
        CodonLookup[61] = AminoAcid.Pro; // CCT (11 11 01)
        CodonLookup[62] = AminoAcid.Pro; // CCG (11 11 10)
        CodonLookup[63] = AminoAcid.Pro; // CCC (11 11 11)
    }


    private void _transcriptionTranslation(Dna dna)
    {
        var peptideLength = 0;
        var indexCodon = -1;
        var codonVal = (byte)0;

        foreach (var b in dna)
        {
            ++indexCodon;
            codonVal <<= 2;
            codonVal |= (byte)b;
            if (indexCodon < 2) continue;
            //turn codon into an acid
            if (CodonLookup[codonVal] is AminoAcid.Stop) break;

            Residues[peptideLength] = CodonLookup[codonVal];
            ++peptideLength;
            indexCodon = -1;
            codonVal = 0;
        }

        if (peptideLength != Residues.Length)
        {
            //maybe not the best way. My reasoning was that copying values would be faster than doing the math twice.
            var result = new AminoAcid[peptideLength];
            for (var i = 0; i < peptideLength; i++)
            {
                result[i] = Residues[i];
            }

            Residues = result;
        }
    }

    public Polypeptide(Dna dna)
    {
        Residues = new AminoAcid[dna.Length / 3];
        _transcriptionTranslation(dna);
    }

    public IEnumerator<AminoAcid> GetEnumerator()
    {
        return ((IEnumerable<AminoAcid>)Residues).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int GetLength() => Residues.Length;
}