using System;
using Godot;

namespace Main.Package;

public partial class Debug : Node
{
    public override void _Ready()
    {
        var dna = new Dna(new Random(), 1000);
        //foreach (var a in dna)
        //{
        //    Console.Write(a + ", ");
        //    
        //}
        var polypep = new Polypeptide(new Dna("TTTCAACAATTTGACCAAGACTTTCAACAATTTGACCAAGAC"));
        int en = 0;

        String HeptadRepeatPattern = "HPPHCPC";

        int hrCount = 0;
        int hri = -1;
        //Heptad repeat: H P P H C P C
        foreach (var a in polypep)
        {
            Console.Write(a + ", ");

            var acidCharge = (int)(a.GetCharge() ?? 0);
            en += acidCharge;
            if (en != 0 && (acidCharge != 0))
                Console.WriteLine("[FOLD], ");

            //Heptad Repeat:


            var charge = a.GetCharge();
            if (HeptadRepeatPattern[hri + 1] == 'H' && !(charge == AminoAcidExtensions.AminoAttributes.Polar ||
                                                         charge == AminoAcidExtensions.AminoAttributes.Positive ||
                                                         charge == AminoAcidExtensions.AminoAttributes.Negative))
            {
                ++hri;
            }
            else if (HeptadRepeatPattern[hri + 1] == 'P' && charge == AminoAcidExtensions.AminoAttributes.Polar)
            {
                ++hri;
            }
            else if (HeptadRepeatPattern[hri + 1] == 'C' && (charge == AminoAcidExtensions.AminoAttributes.Positive ||
                                                             charge == AminoAcidExtensions.AminoAttributes.Negative))
            {
                ++hri;
            }
            else
            {
                if (hrCount > 0)
                {
                    Console.WriteLine($"END COUNT HR: {hrCount}");
                    hrCount = 0;
                }
            }

            if (hri == HeptadRepeatPattern.Length - 1)
            {
                hri = -1;
                ++hrCount;
            }
            //Console.WriteLine($"{charge} Count hr: {hri}");
        }

        if (hrCount > 0)
        {
            Console.WriteLine($"END COUNT HR: {hrCount}");
            hrCount = 0;
        }

        Console.WriteLine("\n End of Polypeptide...");
    }
}