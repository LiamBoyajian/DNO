using System.Diagnostics;
using Godot;
using Main.main.packages.items;

// Ensure this inherits from the exact C# name of your base class
namespace Main.main.packages.util;

public partial class ItemSeed : TextureRect, IPlantable, IItem
{
    // SHOULD NOT BE EXPORTS; CURRENTLY USED FOR TESTING
    [Export] protected Script PlantType;
    [Export] protected SpriteFrames Frames;
    [Export] protected PackedScene PlantScene;
    [Export] protected int PlantDbId = -1;

    public bool AssignSpecies(Script species)
    {
        if (PlantType.Call("is_ancestor_of_count", species).AsBool())
        {
            PlantType = species;
            return true;
        }

        Debug.Assert(false, "species (param) is not child of abstract plant");
        return false;
    }


    public bool Use(Node target = null)
    {
        throw new System.NotImplementedException();
    }

    public override void _Ready()
    {
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

    public Texture2D DragIcon => Texture;
    public Texture2D Icon => Texture;

    public Texture2D HeldIcon
    {
        get => Texture;
    }
}