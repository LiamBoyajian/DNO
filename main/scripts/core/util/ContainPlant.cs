using System;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using Main.main.scripts.core.plants;
using Main.main.scripts.core.plants.species;

namespace Main.Source.main;

[GlobalClass]
public partial class ContainPlant : Sprite2D
{
    public MaterialResource Water = new MaterialResource(12.0, 100.0);
    protected Node Plant = null;

    // Called when the node enters the scene tree for the first time.
    protected const uint HealthCapacity = 100;

    protected bool Broken = false;

    public override void _Ready()
    {
        Console.Write($"\n {GetTotalLoad()}");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public bool HasWater()
    {
        return Water.Amount >= 0;
    }

    public Array<AbstractPlant> GetPlants()
    {
        Array<AbstractPlant> result = new Array<AbstractPlant>();

        foreach (var node in GetChildren())
        {
            if (node is not AbstractPlant child)
                continue;

            result.Add((AbstractPlant)node);
        }

        return result;
    }

    /**
     * Returns the total plants managed by this container
     */
    public int TotalPlants()
    {
        int result = 0;
        foreach (var node in GetChildren())
        {
            if (node is not AbstractPlant child)
                continue;
            ++result;
        }

        return result;
    }

    public int GetTotalLoad()
    {
        double result = GetPlants().Sum(plant => plant.MyResources[AbstractPlant.Rt.Health].Amount);
        return (int)result;
    }

    protected bool BreakOnNoCapacity()
    {
        if (GetTotalLoad() < HealthCapacity) return false;
        Broken = true;
        Water.SetEmpty();
        return true;
    }

    public static bool GetAtmosphRatio()
    {
        throw new NotImplementedException();
    }

    /**
     * TODO: STUB
     * Get environmental sun level
     *
     */
    public float GetSunlevel()
    {
        return 1;
    }

    /**
     * Addchild
     *
     * returns: reference to the child node (null if unassigned)
     */
    public Node AcceptSeed(Node plant, int plantId)
    {
        if (plant == null) return null;
        if (plant is not AbstractPlant p)
            throw new ArgumentException("Node does not contain an AbstractPlant script");

        Plant = p;
        if (p is Main.main.scripts.core.plants.AbstractMicrochipPlant temp)
        {
            temp.LinkParentContainer(this);
            temp.SetDbId(plantId);
        }


        AddChild(plant);

        return plant;
    }
}