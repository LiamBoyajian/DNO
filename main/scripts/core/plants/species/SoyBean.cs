using System;

namespace Main.Source.main.Species;

public partial class SoyBean : MicrochipPlant
{
    const double TESTVALUE = 10; //TEST VALUE


    public override void _Ready()
    {
        HpToEnergyRatio = .1; //arbitrary value per plant species
        HpEnergyValue = 15;
        GlucoseToEnergyRatio = 25;
        base._Ready();
    }

    protected override void Consume()
    {
        Consume(Resources[Rt.Glucose].Take(MyResources[Rt.Energy].Max) / GlucoseToEnergyRatio);
    }

    protected override void Grow(Enum resource)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        base.Grow(rt, TESTVALUE);
        GrowAtMilestones();
    }

    protected override void Clean(Enum resource)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        Clean(resource, TESTVALUE);
    }

    protected bool GrowAtMilestones()
    {
        switch (MyResources[Rt.Health].Max)
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

    protected override double Photosynthesize()
    {
        Resources[Rt.Health].ChangeMax(20);
        GrowAtMilestones();
        var sunlevel = (float)MyContainer.GetSunlevel() * 100;
        const float oxygenByproductRatio = 6f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            ((Math.Max(Resources[Rt.H2O].Amount, Resources[Rt.Co2].Amount) * sunlevel) / waterAndCo2Min);
        Resources[Rt.Glucose].Give(glucoseGenerated);
        Resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        Resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        Resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);

        return glucoseGenerated;
    }
}