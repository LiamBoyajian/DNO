using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.main.scripts.core.plants.species;

public partial class Tomato(int dbId) : AbstractMicrochipPlant(dbId)
{
    // GROWTH VALUES
    protected double M = 5;
    protected double B = 5;
    protected Func<double, double> GrowthType = Math.Sqrt;
    protected double PhotoSynthAmount = 100;

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
}