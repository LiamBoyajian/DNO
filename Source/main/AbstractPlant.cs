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

    private Dictionary<Rt, MaterialResource> _resources = new()
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
        _resources.ToDictionary(k => k.Key, IMaterialResource (v) => v.Value);

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

    //Trade: swap one resource for another at a specific rate
    private void _trade()
    {
    }

    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    private double _photosynthesize(float sunlevel)
    {
        const float oxygenByproductRatio = 6f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            ((Math.Max(_resources[Rt.H2O].Amount, _resources[Rt.Co2].Amount) * sunlevel) / waterAndCo2Min);
        _resources[Rt.Glucose].Increment();
        _resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        _resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        _resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);

        return glucoseGenerated;
    }

    //Clean: remove a resource permanently
    public void _clean(Enum resource)
    {
        if (resource is not Rt)
            throw new ArgumentException(resource + " is not an Rt.");
        //stub not sure if I want here yet
    }


    //Store: store specific resources in an organelle or plant structure
    //Implementing later because I'm unsure how I will implement this
    // new class?: storableResource?
    private void _store()
    {
    }

    //retrieve: retrieve specific resources in an organelle or plant structure
    private void _retrieve()
    {
    }

    /**
     * Consume: Use glucose to create energy
     */
    private double _consume(double glucoseAmount)
    {
        return _resources[Rt.Glucose].Take(glucoseAmount);
    }

    /**
     * Grow: Use resources to increase an attribute maximum
     * Params: glucose directed to growing; ratio of glucose to health
     */
    private void _grow(double glucoseAmount, double modifier)
    {
        _resources[Rt.Health].ChangeMax(glucoseAmount * modifier);
    }

    /**
     * TODO
     * Perform: Use resources to use an organ
     *
     */
    private void _perform()
    {
    }

    /**
     * TODO
     * Cycle: Tell the plant to change its hormonal state
     */
    private void _cycle()
    {
    }

    public bool IsAlive()
    {
        return _resources[Rt.Health].Amount > 0.0;
    }

    protected double EnergyHp(double hpToEnergyRatio)
    {
        double toTake = _resources[Rt.Health].Max * hpToEnergyRatio;
        double result = _resources[Rt.Energy].Take(toTake);
        _resources[Rt.Health].Take(toTake - result);
        return result;
    }

    /**
     * Takes water.
     */
    protected double AcceptWater(double waterAmount)
    {
        return _resources[Rt.H2O].Give(waterAmount);
    }

    abstract protected bool GrowthUpdateFrame();

    abstract protected bool IsDeadThenDeadFrame();

    abstract protected bool GetAtmosphRatio();

    //Versioned used by the plants
    protected abstract void Store();
    protected abstract void Retrieve();
    protected abstract void Consume();
    protected abstract void Grow();
    protected abstract void Perform();
    protected abstract void Cycle();
}