using System.Linq;
using Godot;
using Godot.Collections;

namespace Main.Source.main;

public partial class ContainPlant : Sprite2D
{
    public MaterialResource Water = new MaterialResource(5.0, 100.0);

    // Called when the node enters the scene tree for the first time.
    protected const uint HealthCapacity = 100;

    protected bool Broken = false;

    public override void _Ready()
    {
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
}