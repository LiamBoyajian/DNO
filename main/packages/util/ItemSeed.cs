using System.Diagnostics;
using Godot;

// Ensure this inherits from the exact C# name of your base class
namespace Main.main.scripts.core.util;

public partial class ItemSeed : TextureRect, IItem<Texture>
{
    // SHOULD NOT BE EXPORTS; CURRENTLY USED FOR TESTING
    [Export] protected Script PlantType;
    [Export] protected SpriteFrames Frames;
    [Export] protected PackedScene PlantScene;
    [Export] protected int PlantDbId = -1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public bool AssignSpecies(Script species)
    {
        // Note: 'is_ancestor_of_count' is not a native Godot Script method.
        // Assuming this is a custom method you've attached via GDScript, we use .Call().
        // If it's a C# extension method you wrote, change this to: _plantType.IsAncestorOfCount(species)
        if (PlantType.Call("is_ancestor_of_count", species).AsBool())
        {
            PlantType = species;
            return true;
        }

        Debug.Assert(false, "species (param) is not child of abstract plant");
        return false;
    }

    public Script GetPlantType()
    {
        return PlantType;
    }

    public SpriteFrames GetFrames()
    {
        return Frames;
    }

    public PackedScene GetPlantScene()
    {
        return PlantScene;
    }

    public int GetPlantDbId()
    {
        return PlantDbId;
    }

    public Texture DragIcon { get; }
    public Texture Icon => Texture;
}