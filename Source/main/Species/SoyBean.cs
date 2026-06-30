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
    }

    protected override void Clean(Enum resource)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        Clean(resource, TESTVALUE);
    }
}