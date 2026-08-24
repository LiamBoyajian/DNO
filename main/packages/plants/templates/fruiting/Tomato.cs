using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.model.Dna;
using Main.main.packages.plants.enums;
using Main.main.packages.plants.interfaces;
using Main.main.packages.plants.util;
using Main.main.scripts.core.plants;
using Main.Source.main;
using PlantGui = Main.main.scripts.core.util.PlantGui;

namespace Main.main.packages.plants.species;

public partial class Tomato(int dbId)
    : AbstractMicrochipPlant(dbId), IConcatEnumerable, IUpgradable, IObtainable,
        IBroadcastsUpdate, IShearable, IDirigent
{
    public Tomato() : this(-1)
    {
    }

    protected new FruitingPlantGui GuiManager => base.GuiManager as FruitingPlantGui;
    protected RandomNumberGenerator R = new RandomNumberGenerator();
    [Export] protected PackedScene Fruit;

    // GROWTH VALUES
    protected double M = 5;
    protected double B = 5;
    protected Func<double, double> GrowthType = Math.Sqrt;
    protected double PhotoSynthAmount = 5;

    protected const int STANDARDUPGRADEVAL = 10;
    protected const int ORGANPURCHASEMULTIPLIER = 20;
    protected const double GLUCOSETORESOURCE = 7.14;


    public new event Action Updated;
    //public event UpdatedEventHandler
    //protected int HpToStemRatio = 10;

    public override bool Tick(double delta)
    {
        if (!base.Tick(delta))
            return false;
        //Resources[Rt.Health].ChangeMax(20);
        //Resources[Rt.Health].Give(100);
        //Resources[Rt.Energy].Give(100);
        //Resources[Rt.H2O].Give(30);
        UpdatePlantGuiFrame();
        Updated?.Invoke(); //
        return true;
    }


    protected Dictionary<EnumLibrary.BasicOrgans, MaterialResource> Organs = new()
    {
        { EnumLibrary.BasicOrgans.LeafStem, new MaterialResource(1, 5) },
        { EnumLibrary.BasicOrgans.FlowerStem, new MaterialResource(0, 5) },
        { EnumLibrary.BasicOrgans.Leaf, new MaterialResource(1, 5) },
        { EnumLibrary.BasicOrgans.Root, new MaterialResource(1, 5) },
        { EnumLibrary.BasicOrgans.Flower, new MaterialResource(0, 5) },
        { EnumLibrary.BasicOrgans.Fruit, new MaterialResource(0, 5) },
    };

    public IReadOnlyDictionary<EnumLibrary.BasicOrgans, IMaterialResource> MyOrgans =>
        ConvertToReadOnlyDictionary(Organs);

    public override void _Ready()
    {
        base._Ready();

        GuiManager.NoGrowth = "no_growth";
        GuiManager.Dead = "no_growth";
        GuiManager.Default = "default";
        if (Fruit is null) GD.PrintErr("Could not find Fruit");
    }

    protected override double ObtainGlucose()
    {
        return Photosynthesize();
    }


    protected override double Photosynthesize()
    {
        double waterTaken = Resources[EnumLibrary.Rt.H2O]
            .HasValue(PhotoSynthAmount * Math.Clamp(GetSunLevel(), 0f, 1f));

        if (waterTaken == 0.0)
            return 0;
        Resources[EnumLibrary.Rt.H2O].Take(PhotoSynthAmount);
        Resources[EnumLibrary.Rt.Glucose].Give(waterTaken);
        return waterTaken;
    }

    protected double GlucoseUpgradeMax(EnumLibrary.Rt key, double glucose)
    {
        if (glucose <= 0.0)
            return 0.0;
        return ChangeResourceMax(key, GlucoseUpgradeFunction(glucose));
    }

    protected double GlucoseAddOrgan(EnumLibrary.Rt key, double glucose)
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
        double toTake = Resources[EnumLibrary.Rt.Health].Max * maxHpToGluRatio *
                        gluToEnergyRatio; //Energy required in this run
        double missingEnergy = toTake - Resources[EnumLibrary.Rt.Energy].Take(toTake);

        double missingGlucose =
            (toTake / gluToEnergyRatio) - Resources[EnumLibrary.Rt.Glucose].Take(missingEnergy / gluToEnergyRatio);
        return Resources[EnumLibrary.Rt.Health].Take(missingGlucose / maxHpToGluRatio);
    }

    public override double GlucoseUpgradeFunction(double x)
    {
        if (x < 0.0) return 0;
        return M * GrowthType(x) + B;
    }


    protected bool CreateOrgan(EnumLibrary.BasicOrgans key)
    {
        if (key is EnumLibrary.BasicOrgans.Flower)
        {
            if (Resources[EnumLibrary.Rt.Health].Max < 200) return false;
            AnimatedSprite2D fruit = Fruit.Instantiate() as AnimatedSprite2D;
            if (fruit is null)
                throw new Exception("Could not find valid Fruit");

            fruit.ZIndex = R.RandiRange(-3, 3);
            fruit.FlipH = (R.Randi() & 1) == 1;

            if (!GuiManager.AddFruit(fruit)) return false;
            Organs[key].Increment();
        }

        if (key is EnumLibrary.BasicOrgans.Fruit)
        {
            if (Organs[EnumLibrary.BasicOrgans.Flower].IsEmpty())
                return false;
            if (!GuiManager.ConvertFlowerToFruit()) return false;
            Organs[EnumLibrary.BasicOrgans.Flower].Decrement();
        }

        if (key is EnumLibrary.BasicOrgans.Leaf)
        {
            Organs[key].Increment();
        }

        Organs[key].Increment();
        Resources[EnumLibrary.Rt.Health].ChangeMax(10);

        return true;
    }


    //-------------------------------------------------
    public override IMaterialResource GetIMaterialResource(Enum @enum)
    {
        if (@enum is EnumLibrary.Rt rtKey)
        {
            return MyResources[rtKey];
        }
        else if (@enum is EnumLibrary.BasicOrgans tomatoKey)
        {
            return MyOrgans[tomatoKey];
        }

        return null;
    }

    public double UpgradeCost(Enum @enum)
    {
        if (@enum is EnumLibrary.Rt rt) return GlucoseUpgradeFunction(MyResources[rt].Max);
        if (@enum is EnumLibrary.BasicOrgans tomatoOrgans) return GlucoseUpgradeFunction(MyOrgans[tomatoOrgans].Max);
        return -1;
    }

    public double ObtainCost(Enum @enum)
    {
        if (@enum is EnumLibrary.Rt rt) return GlucoseUpgradeFunction(MyResources[rt].Amount);
        if (@enum is EnumLibrary.BasicOrgans tomatoOrgans) return GlucoseUpgradeFunction(MyOrgans[tomatoOrgans].Amount);
        return -1;
    }
    //-------------------------------------------------

    public virtual bool ParseObtain(Enum @enum)
    {
        bool result = false;
        if (@enum is EnumLibrary.Rt rtKey)
        {
            if (rtKey == EnumLibrary.Rt.Glucose) return false;
            if (rtKey == EnumLibrary.Rt.H2O && MyContainer.HasWater())
            {
                DrawWater(GLUCOSETORESOURCE * Resources[EnumLibrary.Rt.Glucose].Take(STANDARDUPGRADEVAL));
            }
            else
            {
                Resources[rtKey].Give(GLUCOSETORESOURCE * Resources[EnumLibrary.Rt.Glucose].Take(STANDARDUPGRADEVAL));
            }

            result = true;
        }
        else if (@enum is EnumLibrary.BasicOrgans tomatoOrgans)
        {
            CreateOrgan(tomatoOrgans);
        }

        return result;
    }

    public virtual bool ParseUpgrade(Enum @enum)
    {
        bool result = false;
        if (@enum is EnumLibrary.Rt rtKey)
        {
            var tempCost = GlucoseUpgradeFunction(Resources[rtKey].Max);

            if (tempCost <= Resources[EnumLibrary.Rt.Glucose].Amount)
            {
                Resources[EnumLibrary.Rt.Glucose].Take(tempCost);
                Resources[rtKey].ChangeMax(STANDARDUPGRADEVAL);
                result = true;
            }
        }
        else if (@enum is EnumLibrary.BasicOrgans tomatoKey)
        {
            var tempCost = GlucoseUpgradeFunction(Organs[tomatoKey].Amount * ORGANPURCHASEMULTIPLIER);
            if (tempCost <= Resources[EnumLibrary.Rt.Glucose].Amount)
            {
                Resources[EnumLibrary.Rt.Glucose].Take(tempCost);
                result = CreateOrgan(tomatoKey);
            }
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
        double healthMax = Resources[EnumLibrary.Rt.Health].Max;
        if (healthMax <= 0) return -2; //dead
        switch (healthMax)
        {
            case < 0:
                GuiManager.Animation = "no_growth";
                break;
            case < 10:
                GuiManager.Animation = "no_growth";
                break;
            case >= 220:
                GuiManager.Frame = 5;
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
        return (int)Organs[EnumLibrary.BasicOrgans.Fruit].Amount;
    }

    public int Shear(int toShear)
    {
        if (toShear <= 0) return 0;

        int sheared = 0;
        for (int i = 0; sheared < toShear && i < GuiManager.GetSlotMax(); i++)
        {
            if (GuiManager.RemoveFruit())
                ++sheared;
        }

        Organs[EnumLibrary.BasicOrgans.Fruit].Take(sheared);


        Updated?.Invoke();
        return sheared;
    }

    public override IEnumerable<(Enum, IMaterialResource)> GetDictionaryConcatEnumerable()
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