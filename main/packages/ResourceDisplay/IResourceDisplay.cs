using System;
using System.Collections.Generic;
using Godot;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public static class ResourceDisplayTools
{
    public static char Delimiter { get; set; } = '_';
}

public interface IResourceDisplay<out TNode> where TNode : Node
{
    //Not 100% needed?
    public ButtonGroup Buttons { get; }
    public bool ClearChildren();
    //public string ClassNamePrefix { get; set; }

    public bool AddElement((Enum, IMaterialResource) item, string suffix = "");

    /**
     * returns found progressbar; otherwise null
     */
    public TNode Find(string key);

    /**
     * Attempts to update a progressbar with this key
     * returns updated progressbar; otherwise null
     */
    public TNode Update(string key, IMaterialResource material);

    //public void UpdateAll(IEnumerable<(string, IMaterialResource)> getMaterialEnumerable);
}