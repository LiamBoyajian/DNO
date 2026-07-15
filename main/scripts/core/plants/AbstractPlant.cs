using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Main.Source.main;

namespace Main.main.scripts.core.plants;

[GlobalClass]
public abstract partial class AbstractPlant : Node
{
    /**
     * ResourceTypes
     */
    public enum Rt
    {
        //Abstract:
        Health,
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
        Null,
    }

    protected Dictionary<Rt, MaterialResource> Resources = new()
    {
        //Arbitrary base values -- should be removed outside of testing
        { Rt.Health, new MaterialResource(14.0, 100.0) },
        { Rt.Chlorophyll, new MaterialResource(10.0, 100.0) },
        { Rt.Energy, new MaterialResource(10.0, 100.0) },
        { Rt.Glucose, new MaterialResource(10.0, 1000.0) },
        { Rt.H2O, new MaterialResource(10.0, 200.0) },
        { Rt.Co2, new MaterialResource(10.0, 100.0) },
        { Rt.Oxygen, new MaterialResource(10.0, 100.0) },

        { Rt.DamagedCells, new MaterialResource(50.0, 100.0) },
    };

    //-----------------------------
    public IReadOnlyDictionary<Rt, IMaterialResource> MyResources =>
        Resources.ToDictionary(k => k.Key, IMaterialResource (v) => v.Value);

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
    public abstract float GetSunLevel();

    //Clean: remove a resource permanently
    protected virtual double Clean(Enum resource, double amount)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        return Resources[rt].Take(amount);
    }

    /**
     * Consume: create energy from resource
     * Param (double): amount of input to use (1:1)
     */
    protected virtual double Consume(double amount)
    {
        return Resources[Rt.Energy].Give(amount);
    }

    /**
     * Grow: Use resources to increase an attribute maximum
     * Params: resource to use
     */
    protected virtual double Grow(Enum resource, double amount)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        return Resources[rt].ChangeMax(amount);
    }


    public virtual bool IsAlive()
    {
        return Resources[Rt.Health].Amount > 0.0;
    }

    /**
     *
     * Automatically runs to sustain plant
     * When energy demand is greater than supply: hp is used instead
     *
     * Param: units of energy needed per health point (.1 == 10:1; health * .1 == current demand)
     * Param: Ratio when health is converted to energy (10 == 1:10; health * 10 = output energy)
     * Result: in case of energy underflow; missing energy taken from hp
     */
    protected double EnergyHp(double hpToEnergyRatio, double valueOfHealth)
    {
        double toTake = Resources[Rt.Health].Max * hpToEnergyRatio; //Energy required in this run
        double underflow = toTake - Resources[Rt.Energy].Take(toTake); //Energy # not met
        Resources[Rt.Health].Take(underflow / valueOfHealth);
        return underflow;
    }

    /**
     * Takes water.
     */
    protected double AcceptWater(double waterAmount)
    {
        return Resources[Rt.H2O].Give(waterAmount);
    }

    protected abstract bool GrowthUpdateFrame();
    protected abstract bool IsDeadThenDeadFrame();
    protected abstract bool GetAtmosphRatio();

    protected abstract double ObtainGlucose();
}