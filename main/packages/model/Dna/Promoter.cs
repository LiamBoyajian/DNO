using System;
using System.Collections.Generic;
using Main.Source.main;

namespace Main.main.packages.model.Dna;

public class Promoter
{
    public Enum Target { get; set; }

    public string ComparisonType { get; private set; }

    public string PromoterText
    {
        get => ComparisonType;
        set => SetComparisonType(value);
    }

    private void SetComparisonType(string input)
    {
        IsPercent = false;
        ComparisonType = input;
        ComparisonValue = 0;
        if (input == null) return;

        string tempType = "";
        IsPercent = input.Contains("%");

        string tempValue = "";
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] >= '0' && input[i] <= '9')
            {
                tempValue += input[i];
            }
            else
            {
                tempType += input[i];
            }
        }

        if (tempValue.Length > 0)
            ComparisonValue = Convert.ToInt32(tempValue);

        if (IsPercent)
            tempType = tempType.Replace("%", "");

        ComparisonType = tempType;
    }

    public bool IsPercent { get; private set; } = false;
    public int ComparisonValue { get; private set; } = 0;

    private static readonly double Tolerance = 0.0001;

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
        if (PromoterText.Contains("%"))
        {
            input = materialResource.Amount / materialResource.Max;
        }
        else
        {
            input = materialResource.Amount;
        }

        if (Comparators.TryGetValue(PromoterText, out Func<double, double, bool> comparator))
        {
            return comparator(input, ComparisonValue);
        }

        return false;
    }
}