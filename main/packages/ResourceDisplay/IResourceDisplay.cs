using System;
using System.Collections.Generic;
using CommandLine;
using Godot;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public interface IResourceDisplay<TNode> where TNode : Node, IResourceElement
{
    [Export] public PackedScene Scene { get; set; }
    public EnumGate EnumGate { get; set; }

    public void ClearChildren()
    {
        if (this is not Container c) throw new Exception("this is not a node");
        foreach (var child in c.GetChildren())
        {
            child.QueueFree();
        }
    }

    public void AddElement(TNode item)
    {
        if (this is not Container c) throw new Exception("this is not a node");
        c.AddChild(item);
    }

    //public TNode Find(Enum @enum, string suffix = "");

    public TNode Get(Enum @enum)
    {
        if (this is not Container c) throw new Exception("this is not a node");
        foreach (var child in c.GetChildren())
        {
            if (child is not TNode itemUpgrade) continue;
            if (Equals(itemUpgrade.Enum, @enum))
            {
                return itemUpgrade;
            }
        }

        return null;
    }

    public bool Contains(Enum @enum)
    {
        //TODO reimplement later, search per enum
        if (this is not Container c) throw new Exception("this is not a node");
        return Get(@enum) != null;
    }

    //public void UpdateAll(IEnumerable<(string, IMaterialResource)> getMaterialEnumerable);
    public IEnumerable<TNode> GetAll()
    {
        if (this is not Container c) throw new Exception("this is not a node");
        foreach (var child in c.GetChildren())
            yield return (TNode)child;
    }
}