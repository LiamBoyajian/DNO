using System;
using Godot;
using Main.main.scripts.core.util;

namespace Main.main.packages.plants.util;

public partial class FruitingPlantGui : PlantGui
{
    [Export] protected FruitSpawns FruitSpawns;
    [Export] protected int MaxClickDistance = 10;

    public override void _Ready()
    {
        base._Ready();
        if (GetChild(0) is FruitSpawns fruitSpawns)
        {
            FruitSpawns = fruitSpawns;
        }
        else GD.PrintErr("Could not find FruitSpawns");
    }

    public virtual new bool AddFruit(Node2D node)
    {
        if (node is null) return false;
        if (!FruitSpawns.CanAdd()) return false;
        return FruitSpawns.Add(node);
    }

    public virtual new bool RemoveFruit(Vector2? pos = null)
    {
        Node2D result = null;
        if (pos == null)
        {
            result = FruitSpawns.Remove();
            if (result is null)
                return false;

            //GetTree().Root.AddChild(result);
            return true;
        }

        Vector2? closest = null;
        foreach (var pair in FruitSpawns.FruitPositionsReadOnly)
        {
            if (pair.Value == null) continue;
            if (closest == null)
            {
                closest = pair.Key;
                continue;
            }

            if (pos?.DistanceTo((Vector2)closest) > pos?.DistanceTo(pair.Key))
                closest = pair.Key;
        }

        result = FruitSpawns.Remove();
        if (result is null)
            return false;

        GetTree().Root.AddChild(result);
        return true;
    }

    public virtual bool ConvertFlowerToFruit()
    {
        var foundFlower = false;
        int i = 0;
        while (!foundFlower && i < FruitSpawns.Max)
        {
            var node2D = FruitSpawns.Get(i);
            if (node2D is not AnimatedSprite2D animatedSprite2D) return false;
            if (animatedSprite2D.Frame == 0)
            {
                ++animatedSprite2D.Frame;
                foundFlower = true;
            }

            ++i;
        }

        return foundFlower;
    }

    public int GetSlotMax()
    {
        return FruitSpawns.Max;
    }
}