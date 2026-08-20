using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.plants.enums;
using Main.Source.main;

namespace Main.main.scripts.core.plants.species;

public partial class SoyBean(int dbId) : AbstractMicrochipPlant(dbId)
{
    const double TESTVALUE = 10; //TEST VALUE


    SoyBean() : this(-1)
    {
    }

    public override void _Ready()
    {
        base._Ready();
    }


    protected bool GrowAtMilestones()
    {
        switch (MyResources[EnumLibrary.Rt.Health].Max)
        {
            case >= 250:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(6);
                break;
            case >= 200.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(5);
                break;
            case >= 180.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(4);
                break;
            case >= 150.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(3);
                break;
            case >= 100.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(2);
                break;
            case >= 50.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(1);
                break;
            case >= 10.0:
                GuiManager.SetAnimation("default");
                GuiManager.SetFrame(0);
                break;
            default:
                GuiManager.SetAnimation("dirt");
                break;
        }

        return false; //TODO return bool on whether the frame changed
    }


    /**
     * Temp function that should be implemented in a separate class.
     * Used to test the gameplay loop without finalizing an implementation
     */
    public override void _UnhandledInput(InputEvent input)
    {
        if (input.IsActionPressed("ui_accept"))
        {
            foreach (var pair in Resources)
            {
                Console.Write($"\r\n{pair.Key}: {pair.Value.Amount}/{pair.Value.Max}");
            }
        }
    }


    //--------------------------------------------------------------------------
    // Abstract Functions
    //--------------------------------------------------------------------------


    protected override double ObtainGlucose()
    {
        return Photosynthesize();
    }

    //--------------------------------------------------------------------------
    // Interface Functions
    //--------------------------------------------------------------------------
    protected override double Consume(double amount)
    {
        return base.Consume(Resources[EnumLibrary.Rt.Glucose].Take(MyResources[EnumLibrary.Rt.Energy].Max) /
                            ConvertGluToEnergyRatio);
    }

    protected override double Grow(Enum resource, double amount)
    {
        if (resource is not EnumLibrary.Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        var result = base.Grow(rt, TESTVALUE);
        GrowAtMilestones();
        return result;
    }

    protected override double Clean(Enum resource, double amount)
    {
        if (resource is not EnumLibrary.Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");


        return base.Clean(resource, TESTVALUE);
        ;
    }


    protected override double Photosynthesize()
    {
        Resources[EnumLibrary.Rt.Health].ChangeMax(20);
        GrowAtMilestones();
        var sunlevel = (float)MyContainer.GetSunlevel() * 100;
        const float oxygenByproductRatio = 6f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            ((Math.Max(Resources[EnumLibrary.Rt.H2O].Amount, Resources[EnumLibrary.Rt.Co2].Amount) * sunlevel) /
             waterAndCo2Min);
        Resources[EnumLibrary.Rt.Glucose].Give(glucoseGenerated);
        Resources[EnumLibrary.Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        Resources[EnumLibrary.Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        Resources[EnumLibrary.Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);

        return glucoseGenerated;
    }

    public override double GlucoseUpgradeFunction(double x)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<(Enum, IMaterialResource)> GetDictionaryConcatEnumerable()
    {
        throw new NotImplementedException();
    }

    public override IMaterialResource GetIMaterialResource(Enum @enum)
    {
        throw new NotImplementedException();
    }
}