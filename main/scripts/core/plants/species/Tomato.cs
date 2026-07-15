using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Main.main.scripts.core.plants.interfaces;
using Main.main.scripts.core.util.interfaces;
using Main.Source.main;

namespace Main.main.scripts.core.plants.species;

public partial class Tomato(int dbId)
    : AbstractMicrochipPlant(dbId), IAttributeEnumerable, IMaterialEnumerable, IUpgradable
{
    public Tomato() : this(-1)
    {
    }

    // GROWTH VALUES
    protected double M = 5;
    protected double B = 5;
    protected Func<double, double> GrowthType = Math.Sqrt;
    protected double PhotoSynthAmount = 100;

    protected const int STANDARDUPGRADEVAL = 10;
    protected const int ORGANPURCHASEMULTIPLIER = 20;

    //protected int HpToStemRatio = 10;


    public enum TomatoOrgans
    {
        LeafStem,
        FlowerStem,
        Leaf,
        Root,
        Flower,
        Fruit,
    }


    protected Dictionary<TomatoOrgans, int> Organs = new()
    {
        { TomatoOrgans.LeafStem, 1 },
        { TomatoOrgans.FlowerStem, 0 },
        { TomatoOrgans.Leaf, 1 },
        { TomatoOrgans.Root, 0 },
        { TomatoOrgans.Flower, 0 },
        { TomatoOrgans.Fruit, 0 },
    };

    public IReadOnlyDictionary<TomatoOrgans, int> MyOrgans => Organs.ToDictionary();

    public override void _Ready()
    {
        base._Ready();
        DbId = -1;
    }

    protected override double ObtainGlucose()
    {
        return Photosynthesize();
    }

    protected override double Photosynthesize()
    {
        double waterCo2Max = Math.Max(Resources[Rt.H2O].HasValue(PhotoSynthAmount),
            Resources[Rt.Co2].HasValue(PhotoSynthAmount));
        Resources[Rt.Co2].Take(PhotoSynthAmount);
        Resources[Rt.H2O].Take(PhotoSynthAmount);
        return waterCo2Max * GetSunLevel();
    }

    protected double GlucoseUpgradeMax(Rt key, double glucose)
    {
        if (glucose <= 0.0)
            return 0.0;
        return ChangeResourceMax(key, GlucoseUpgradeFunction(glucose));
    }

    protected double GlucoseAddOrgan(Rt key, double glucose)
    {
        if (glucose <= 0.0)
            return 0.0;
        return ChangeResourceMax(key, GlucoseUpgradeFunction(glucose));
    }

    /**
     * Unsafe if given a negative
     */
    protected double GlucoseUpgradeFunction(double x)
    {
        return M * GrowthType(x) + B;
    }

    protected bool ObtainResource(Rt key)
    {
        bool result = false;
        switch (key)
        {
            case Rt.H2O:
                if (Resources[Rt.Glucose].Amount < 5)
                    break;
                Resources[Rt.H2O].Give(100);
                result = true;
                break;
        }


        return result;
    }

    protected bool CreateOrgan(TomatoOrgans key)
    {
        bool result = false;
        switch (key)
        {
            case TomatoOrgans.LeafStem:
                if (Resources[Rt.Glucose].Amount < 50)
                    break;

                Organs[TomatoOrgans.LeafStem]++;
                result = true;
                break;

            case TomatoOrgans.FlowerStem:
                if (Resources[Rt.Glucose].Amount < 60)
                    break;

                Organs[TomatoOrgans.Fruit]++;
                result = true;
                break;

            case TomatoOrgans.Leaf:
                if (Resources[Rt.Glucose].Amount < 10 || Organs[TomatoOrgans.LeafStem] <= 0)
                    break;


                Organs[TomatoOrgans.Leaf] += 2;
                result = true;
                break;

            case TomatoOrgans.Root:
                if (Resources[Rt.Glucose].Amount < 20)
                    break;

                Organs[TomatoOrgans.Root]++;
                result = true;
                break;

            case TomatoOrgans.Flower:
                if (Resources[Rt.Glucose].Amount < 10 || Organs[TomatoOrgans.FlowerStem] <= 0)
                    break;

                Organs[TomatoOrgans.Flower] += 2;
                result = true;
                break;

            case TomatoOrgans.Fruit:
                if (Resources[Rt.Glucose].Amount < 100 || Organs[TomatoOrgans.Flower] <= 0)
                    break;


                --Organs[TomatoOrgans.Flower];
                ++Organs[TomatoOrgans.Fruit];
                result = true;
                break;
        }

        if (result)
            Resources[Rt.Health].Give(10);

        return result;
    }


    public IEnumerable<(string, IMaterialResource)> GetMaterialEnumerable()
    {
        foreach (var r in Resources)
        {
            yield return (r.Key.ToString(), (IMaterialResource)r.Value);
        }
    }

    public IEnumerable<(string, double)> GetAttributeEnumerable()
    {
        foreach (var r in Organs)
        {
            yield return (r.Key.ToString(), r.Value);
        }
    }

    /**
     * Can upgrade doubles and MaterialResource
     * String can parse to Rt and TomatoOrgans
     *
     * returns true if change was made
     */
    public virtual bool ParseUpgrade(string s)
    {
        bool result = false;
        if (Enum.TryParse(s, out Rt rtKey))
        {
            var tempCost = GlucoseUpgradeFunction(Resources[rtKey].Amount);

            if (tempCost <= Resources[Rt.Glucose].Amount)
            {
                Resources[Rt.Glucose].Take(tempCost);
                Resources[rtKey].ChangeMax(STANDARDUPGRADEVAL);
                result = true;
            }
        }
        else if (Enum.TryParse(s, out TomatoOrgans tomatoKey))
        {
            var tempCost = GlucoseUpgradeFunction(Organs[tomatoKey] * ORGANPURCHASEMULTIPLIER);
            if (tempCost <= Resources[Rt.Glucose].Amount)
            {
                Resources[Rt.Glucose].Take(tempCost);
                Organs[tomatoKey] += 1;
                result = true;
            }
        }

        return result;
    }
}