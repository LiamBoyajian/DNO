using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Main.main.packages.plants.util;

public partial class FruitSpawns : Node2D
{
    private Dictionary<Vector2, Node2D> FruitPositions { get; } = new();

    public IReadOnlyDictionary<Vector2, Node2D> FruitPositionsReadOnly => FruitPositions.AsReadOnly();
    public int Total { get; private set; } = 0;
    public int Max => FruitPositions.Count;

    public override void _Ready()
    {
        base._Ready();
        foreach (var node in GetChildren())
        {
            if (node is not Node2D node2D) continue;
            FruitPositions.Add(node2D.GetPosition(), null);
            node.QueueFree();
        }
    }

    public bool CanAdd()
    {
        return Total < FruitPositions.Count;
    }

    public bool Add(Node2D node)
    {
        if (node is null) return false;
        if (!CanAdd()) return false;

        foreach (var pair in FruitPositions)
        {
            if (pair.Value != null) continue;
            FruitPositions[pair.Key] = node;


            node.Position = pair.Key;
            if (node.GetParent() != null)
                Reparent(this);
            else AddChild(node);

            ++Total;
            break;
        }

        return true;
    }

    public Node2D Remove(int index = -1)
    {
        if (Total == 0) return null;
        if (index > FruitPositions.Count) return null;

        Node2D result = null;
        if (index > 0)
        {
            result = FruitPositions.ElementAt(index).Value;
            FruitPositions[FruitPositions.ElementAt(index).Key] = null;
            RemoveChild(result);
            --Total;
            return result;
        }

        foreach (var pair in FruitPositions)
        {
            if (pair.Value == null) continue;
            result = pair.Value;
            FruitPositions[pair.Key] = null;
            RemoveChild(pair.Value);
            --Total;
            break;
        }

        return result;
    }

    public Node2D Get(int index = -1)
    {
        if (Total == 0) return null;
        if (index > FruitPositions.Count) return null;
        if (index > 0) return FruitPositions.Values.ElementAt(index);

        foreach (var pair in FruitPositions)
        {
            if (pair.Value == null) continue;
            return pair.Value;
        }

        return null;
    }

    public List<Node2D> RemoveAll()
    {
        if (Total == 0) return null;
        List<Node2D> result = new List<Node2D>();
        foreach (var pair in FruitPositions)
        {
            if (pair.Value == null) continue;
            result.Add(pair.Value);
            RemoveChild(pair.Value);
        }

        return result;
    }
}