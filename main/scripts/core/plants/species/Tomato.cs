using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Main.main.scripts.core.plants.interfaces;
using Main.main.scripts.core.util.interfaces;
using Main.main.scripts.core.util.inventory;
using Main.Source.main;

namespace Main.main.scripts.core.plants.species;

public partial class Tomato(int dbId)
    : AbstractMicrochipPlant(dbId), IAttributeEnumerable, IMaterialEnumerable, IUpgradable, IObtainable,
        IBroadcastsUpdate, IShearable
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
    protected const double GLUCOSETORESOURCE = 7.14;

    public event Action Updated;
    //public event UpdatedEventHandler
    //protected int HpToStemRatio = 10;

    public override bool Tick(double delta)
    {
        if (!base.Tick(delta))
            return false;

        Updated?.Invoke();
        return true;
    }

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
        double waterCo2Max = Math.Min(Resources[Rt.H2O].HasValue(PhotoSynthAmount * GetSunLevel()),
            Resources[Rt.Co2].HasValue(PhotoSynthAmount));
        if (waterCo2Max <= 0.0)
            return 0;
        Resources[Rt.Co2].Take(PhotoSynthAmount);
        Resources[Rt.H2O].Take(PhotoSynthAmount);
        Resources[Rt.Glucose].Give(waterCo2Max);
        return waterCo2Max;
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
     *
     * Automatically runs to sustain plant
     * When energy demand is greater than supply: hp is used instead
     *
     * Result: in case of energy underflow; hp loss
     */
    protected new double EnergyHp(double maxHpToGluRatio, double gluToEnergyRatio)
    {
        GD.Print("asdpoijnasodoiasjd\n");
        double toTake = Resources[Rt.Health].Max * maxHpToGluRatio * gluToEnergyRatio; //Energy required in this run
        double missingEnergy = toTake - Resources[Rt.Energy].Take(toTake);

        double missingGlucose =
            (toTake / gluToEnergyRatio) - Resources[Rt.Glucose].Take(missingEnergy / gluToEnergyRatio);
        return Resources[Rt.Health].Take(missingGlucose / maxHpToGluRatio);
    }

    /**
     * Unsafe if given a negative
     */
    protected double GlucoseUpgradeFunction(double x)
    {
        return M * GrowthType(x) + B;
    }


    protected bool CreateOrgan(TomatoOrgans key)
    {
        //TODO CHANGE TO CHAGERESOURCEMAX()
        bool result = false;
        switch (key)
        {
            case TomatoOrgans.LeafStem:
                Organs[TomatoOrgans.LeafStem] += 1;
                result = true;
                break;

            case TomatoOrgans.FlowerStem:
                Organs[TomatoOrgans.FlowerStem] += 1;
                result = true;
                break;

            case TomatoOrgans.Leaf:
                Organs[TomatoOrgans.Leaf] += 2;
                result = true;
                break;

            case TomatoOrgans.Root:

                Organs[TomatoOrgans.Root] += 1;
                result = true;
                break;

            case TomatoOrgans.Flower:

                Organs[TomatoOrgans.Flower] += 2;
                result = true;
                break;

            case TomatoOrgans.Fruit:
                if (Organs[TomatoOrgans.Flower] <= 0)
                    break;
                Organs[TomatoOrgans.Flower] -= 1;
                Organs[TomatoOrgans.Fruit] += 1;
                result = true;
                break;
        }

        if (result)
            Resources[Rt.Health].ChangeMax(10);

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
            var tempCost = GlucoseUpgradeFunction(Resources[rtKey].Max);

            if (tempCost <= Resources[Rt.Glucose].Amount)
            {
                Resources[Rt.Glucose].Take(tempCost);
                Resources[rtKey].ChangeMax(STANDARDUPGRADEVAL);
                result = true;
            }
        }
        else if (Enum.TryParse(s, out TomatoOrgans tomatoKey))
        {
            GD.Print("asdojaosijdasoipjd\n");
            var tempCost = GlucoseUpgradeFunction(Organs[tomatoKey] * ORGANPURCHASEMULTIPLIER);
            if (tempCost <= Resources[Rt.Glucose].Amount)
            {
                Resources[Rt.Glucose].Take(tempCost);
                result = CreateOrgan(tomatoKey);
            }
        }

        return result;
    }

    public virtual bool ParseObtain(string s)
    {
        bool result = false;
        if (Enum.TryParse(s, out Rt rtKey))
        {
            if (rtKey == Rt.Glucose)
            {
                //Do nothing
            }
            else if (rtKey == Rt.H2O)
            {
                if (MyContainer.HasWater())
                    DrawWater(GLUCOSETORESOURCE * Resources[Rt.Glucose].Take(STANDARDUPGRADEVAL));
            }
            else
            {
                Resources[rtKey].Give(GLUCOSETORESOURCE * Resources[Rt.Glucose].Take(STANDARDUPGRADEVAL));
            }

            result = true;
        }

        return result;
    }

    public int GetShear()
    {
        return Organs[TomatoOrgans.Fruit];
    }

    public int Shear(int sheared)
    {
        if (sheared <= 0) return 0;

        int result;
        if (sheared <= Organs[TomatoOrgans.Fruit])
        {
            result = sheared;
            Organs[TomatoOrgans.Fruit] -= sheared;
        }
        else
        {
            result = Organs[TomatoOrgans.Fruit];
            Organs[TomatoOrgans.Fruit] = 0;
        }

        Updated?.Invoke();
        return result;
    }
}