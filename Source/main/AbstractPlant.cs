using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using Main.Package;

namespace Main.Source.main;

public abstract partial class AbstractPlant : Node
{
    /**
     * ResourceTypes
     */
    public enum Rt
    {
        //Abstract:
        Health,
        MaxHealth,
        Chlorophyll,
        Energy,

        //Definite attributes:
        Glucose,
        H2O,
        Co2,
        Oxygen,

        //hormones
        //circadian rhythm
        //injury types:
        DamagedCells, //maybe add types of cells or damage idk (types of broken proteins.)
    }

    protected Dictionary<Rt, MaterialResource> Resources = new()
    {
        //Arbitrary base values
        { Rt.Health, new MaterialResource(10.0, 100.0) },
        //size attribute needed
        { Rt.Chlorophyll, new MaterialResource(10.0, 100.0) },
        { Rt.Energy, new MaterialResource(10.0, 100.0) },
        { Rt.Glucose, new MaterialResource(10.0, 1000.0) },
        { Rt.H2O, new MaterialResource(10.0, 200.0) },
        { Rt.Co2, new MaterialResource(10.0, 100.0) },
        { Rt.Oxygen, new MaterialResource(10.0, 100.0) },

        { Rt.DamagedCells, new MaterialResource(50.0, 100.0) },
    };

    //-----------------------------
    public IReadOnlyDictionary<Rt, MaterialResource> MyResources => Resources;

    protected double FrameSum = 0.0;


    //-----------------------------


    public override void _Ready()
    {
    }


    public override void _Process(double delta)
    {
    }

    abstract public void Tick(double delta);


    /**
     * TODO: STUB
     */
    public float GetSunLevel()
    {
        var TESTSUM = .8f;
        return TESTSUM;
    }

    /**
     * ACTIONS: Make changes to a plant's resources
     *
     **/

    //Trade: swap one resource for another at a specific rate
    private void _trade()
    {
    }

    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    private void _photosynthesize(float sunlevel)
    {
        const float oxygenByproductRatio = 6.0f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            (int)((Math.Max(Resources[Rt.H2O].Amount, Resources[Rt.Co2].Amount) * sunlevel) / 6.0f);
        Resources[Rt.Glucose].Increment();
        Resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        Resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        Resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);
    }

    //Clean: remove a resource permanently
    public void _clean(Enum resource)
    {
        if (resource is not Rt)
            throw new ArgumentException(resource.ToString() + " is not an Rt.");
        //stub not sure if I want here yet
    }


    //Store: store specific resources in an organelle or plant structure
    public void _store()
    {
    }

    //retrieve: retrieve specific resources in an organelle or plant structure
    public void _retrieve()
    {
    }

    //Consume: Use glucose for energy (no energy = lose hp)
    public void _consume()
    {
        if (Resources[Rt.Glucose].Decrement())
        {
            Resources[Rt.Energy].Give(10.0);
        }
        else
        {
            Resources[Rt.Health].Take(10.0);
        }
    }

    //Grow: Use resources to increase an attribute maximum
    public void _grow()
    {
        //size attribute needed: size creates a need for higher upkeep cost
    }

    //Perform: Use resources to use an organ
    public void _perform()
    {
    }

    //Cycle: Tell the plant to change its hormonal state
    public void _cycle()
    {
    }

    public bool IsAlive()
    {
        return Resources[Rt.Health].Amount > 0.0;
    }

    abstract protected bool GrowthUpdateFrame();

    abstract protected bool IsDeadThenDeadFrame();
}