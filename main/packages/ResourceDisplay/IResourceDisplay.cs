using System;
using System.Collections.Generic;
using Godot;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public static class ResourceDisplay
{
    public static char Delimiter { get; set; } = '_';
}

public interface IResourceDisplay<TNode>
{
    //Not 100% needed?

    public List<Enum> AllowDisplay { get; }
    public ButtonGroup Buttons { get; }
    public bool ClearChildren();
    public string ClassNamePrefix { get; set; }

    public bool AddElement((Enum, IMaterialResource) item);

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