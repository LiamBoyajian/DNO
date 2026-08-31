using System;
using System.Collections.Generic;
using System.Linq;
using Main.Source.main;

namespace Main.main.packages.model.Dna;

public class Promoter
{
    public Enum Target { get; set; }

    public string ComparisonSymbol { get; set; }
    public bool IsPercent { get; set; } = false;
    public int ComparisonValue { get; set; } = 0;


    /**
     * Display string in the form {Target}{ComparisonSymbol}{ComparisonValue}{%}
     * e.g. "WaterAmount>=75%". Composed from the fields on demand - it is not
     * stored and cannot be assigned. Set Target / ComparisonSymbol /
     * ComparisonValue / IsPercent directly instead.
     */
    public string PromoterText
    {
        get
        {
            var result = "";
            result += Target?.ToString();
            result += ComparisonSymbol;
            result += ComparisonValue;
            result += IsPercent ? "%" : "";
            return result;
        }
    }

    private static readonly double Tolerance = 0.0001;

    public static IReadOnlyList<string> ComparisonKeys
    {
        get => Comparators.Keys.ToList();
    }

    private static readonly Dictionary<string, Func<double, double, bool>> Comparators = new()
    {
        ["=="] = (a, b) => Math.Abs(a - b) < Tolerance,
        ["<="] = (a, b) => a <= b,
        [">="] = (a, b) => a >= b,
        ["<"] = (a, b) => a < b,
        [">"] = (a, b) => a > b,
        ["!="] = (a, b) => Math.Abs(a - b) > Tolerance,
        ["*="] = (a, b) => true,
    };
    //equal to 
    //greater/less than
    // percent vs value?


    //Create the comparison type to compare etc etc

    public bool Compare(IMaterialResource materialResource)
    {
        if (materialResource == null) return false;

        double input;
        if (IsPercent)
        {
            // ComparisonValue is a whole percentage (75 means 75%), so the
            // ratio is scaled to the same 0-100 range before comparing.
            if (materialResource.Max == 0) return false;
            input = materialResource.Amount / materialResource.Max * 100.0;
        }
        else
        {
            input = materialResource.Amount;
        }

        // Keyed on ComparisonSymbol, not PromoterText: PromoterText is the
        // composed display string and never equals a comparator key.
        if (ComparisonSymbol != null &&
            Comparators.TryGetValue(ComparisonSymbol, out Func<double, double, bool> comparator))
        {
            return comparator(input, ComparisonValue);
        }

        return false;
    }
}