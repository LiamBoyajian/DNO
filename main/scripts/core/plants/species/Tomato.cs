using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Main.main.scripts.core.plants.interfaces;
using Main.main.scripts.core.util.inventory;
using Main.Source.main;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;

namespace Main.main.scripts.core.plants.species;

public partial class Tomato(int dbId)
    : AbstractMicrochipPlant(dbId), IConcatEnumerable, IUpgradable, IObtainable,
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
        //Resources[Rt.Health].ChangeMax(20);
        //Resources[Rt.Health].Give(100);
        //Resources[Rt.Energy].Give(100);
        UpdatePlantGuiFrame();
        Updated?.Invoke(); //
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

    protected Dictionary<TomatoOrgans, MaterialResource> Organs = new()
    {
        { TomatoOrgans.LeafStem, new MaterialResource(1, 5) },
        { TomatoOrgans.FlowerStem, new MaterialResource(1, 5) },
        { TomatoOrgans.Leaf, new MaterialResource(1, 5) },
        { TomatoOrgans.Root, new MaterialResource(1, 5) },
        { TomatoOrgans.Flower, new MaterialResource(1, 5) },
        { TomatoOrgans.Fruit, new MaterialResource(1, 5) },
    };

    public IReadOnlyDictionary<TomatoOrgans, IMaterialResource> MyOrgans => ConvertToReadOnlyDictionary(Organs);

    public override void _Ready()
    {
        base._Ready();
        DbId = -1;
        GuiManager.NoGrowth = "no_growth";
        GuiManager.Dead = "no_growth";
        GuiManager.Default = "default";
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
        //GD.Print("asdpoijnasodoiasjd\n");
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
                Organs[TomatoOrgans.LeafStem].Give(1);
                result = true;
                break;

            case TomatoOrgans.FlowerStem:
                Organs[TomatoOrgans.FlowerStem].Give(1);
                result = true;
                break;

            case TomatoOrgans.Leaf:
                Organs[TomatoOrgans.Leaf].Give(2);
                result = true;
                break;

            case TomatoOrgans.Root:

                Organs[TomatoOrgans.Root].Give(1);
                result = true;
                break;

            case TomatoOrgans.Flower:

                Organs[TomatoOrgans.Flower].Give(2);
                result = true;
                break;

            case TomatoOrgans.Fruit:
                if (Organs[TomatoOrgans.Flower].Amount <= 0)
                    break;
                Organs[TomatoOrgans.Flower].Take(1);
                Organs[TomatoOrgans.Fruit].Give(1);
                result = true;
                break;
        }

        if (result)
            Resources[Rt.Health].ChangeMax(10);

        return result;
    }


    //public IEnumerable<(string, IMaterialResource)> GetMaterialEnumerable()
    //{
    //    foreach (var r in Resources)
    //    {
    //        if (r.Key is Rt.H2O or Rt.Glucose or Rt.Health)
    //            yield return (r.Key.ToString(), (IMaterialResource)r.Value);
    //    }
    //}
    //
    //public IEnumerable<(string, double)> GetAttributeEnumerable()
    //{
    //    foreach (var r in Organs)
    //    {
    //        yield return (r.Key.ToString(), r.Value);
    //    }
    //}

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
            var tempCost = GlucoseUpgradeFunction(Organs[tomatoKey].Amount * ORGANPURCHASEMULTIPLIER);
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

    /**
     * returns the current frame and updates
     * -2 == dead
     * -1 == no growth
     * 0 == first growth frame
     * n = current growth frame
     *
     */
    public int UpdatePlantGuiFrame()
    {
        //TODO Horrible method/design -- replacing later surely 
        var result = -2;
        double healthMax = Resources[Rt.Health].Max;
        if (healthMax <= 0) return -2; //dead
        GD.Print("HEALTH MAX: " + healthMax);
        switch (healthMax)
        {
            case < 0:
                GuiManager.Animation = "no_growth";
                break;
            case < 10:
                GuiManager.Animation = "no_growth";
                break;
            case >= 500:
                GuiManager.Animation = "fruiting";
                GuiManager.Stop();
                GuiManager.Frame = (int)Mathf.Clamp(Organs[TomatoOrgans.Fruit].Amount, 0, 8);
                break;
            case >= 200:
                GuiManager.Frame = 4;
                break;
            case >= 100:
                GuiManager.Frame = 3;
                break;
            case >= 50:
                GuiManager.Frame = 2;
                break;
            case >= 20:
                GuiManager.Frame = 1;
                break;
            case >= 10:
                GuiManager.Frame = 0;
                break;
        }

        return GuiManager.Frame;
    }


    public int GetShear()
    {
        return (int)Organs[TomatoOrgans.Fruit].Amount;
    }

    public int Shear(int sheared)
    {
        if (sheared <= 0) return 0;

        int result;
        if (sheared <= Organs[TomatoOrgans.Fruit].Amount)
        {
            result = sheared;
            Organs[TomatoOrgans.Fruit].Take(sheared);
        }
        else
        {
            result = (int)Organs[TomatoOrgans.Fruit].Amount;
            Organs[TomatoOrgans.Fruit].SetEmpty();
        }

        Updated?.Invoke();
        return result;
    }

    public IEnumerable<(Enum, IMaterialResource)> GetDictionaryConcatEnumerable()
    {
        foreach (var r in MyResources)
        {
            yield return (r.Key, r.Value);
        }

        foreach (var o in Organs)
        {
            yield return (o.Key, o.Value);
        }
    }
}