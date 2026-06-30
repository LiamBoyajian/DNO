using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using Main.Package;
using Microsoft.Data.Sqlite;
using Dapper;


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

    protected int DbId;
    protected string DatabasePath = ProjectSettings.GlobalizePath("user://greenhouse.db");
    //-----------------------------


    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    abstract public void Tick(double delta);

    public bool ConnectPlantToDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();

        var query = "SELECT id, name FROM plants WHERE id = @plant_id;";
        var foundPlants = connection.Query(query, new { plant_id = DbId });

        var count = 0;
        foreach (var plant in foundPlants)
        {
            Console.WriteLine($"\nPlants: {plant.id} - {plant.name}");
            ++count;
        }

        if (count > 1)
            throw new InvalidOperationException($"Database found two identical plant_ids - {DbId}");


        return count != 1;
    }

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

    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    private double _photosynthesize(float sunlevel)
    {
        const float oxygenByproductRatio = 6f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            ((Math.Max(Resources[Rt.H2O].Amount, Resources[Rt.Co2].Amount) * sunlevel) / waterAndCo2Min);
        Resources[Rt.Glucose].Increment();
        Resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        Resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        Resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);

        return glucoseGenerated;
    }

    //Clean: remove a resource permanently
    public double Clean(Enum resource, double amount)
    {
        if (resource is not Rt rt)
            throw new ArgumentException(resource + " is not an Rt.");

        return Resources[rt].Take(amount);
    }

    /**
     * Consume: create energy from resource
     * Param (double): amount of input to use (1:1)
     */
    protected double Consume(double amount)
    {
        return Resources[Rt.Energy].Give(amount);
    }

    /**
     * Grow: Use resources to increase an attribute maximum
     * Params: resource to use
     */
    public void Grow(Rt attributeType, double amount)
    {
        Resources[attributeType].ChangeMax(amount);
    }


    public bool IsAlive()
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
    protected abstract void Consume();
    protected abstract void Grow(Enum resource);
    protected abstract void Clean(Enum resource);
}