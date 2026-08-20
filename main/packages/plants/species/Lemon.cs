using System;
using System.Collections.Generic;
using Main.main.scripts.core.plants;
using Main.Source.main;

namespace Main.main.packages.plants.species;

public partial class Lemon : AbstractMicrochipPlant
{
    protected override double ObtainGlucose()
    {
        throw new System.NotImplementedException();
    }

    protected override double Photosynthesize()
    {
        throw new System.NotImplementedException();
    }

    public override double GlucoseUpgradeFunction(double x)
    {
        throw new System.NotImplementedException();
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