using System;
using System.Diagnostics;
using Godot;
using Main.main.packages.items;

// Ensure this inherits from the exact C# name of your base class
namespace Main.main.scripts.core.util;

public partial class ItemSeed : TextureRect, IPlantable, IItem<Texture>
{
    // SHOULD NOT BE EXPORTS; CURRENTLY USED FOR TESTING
    [Export] protected Script PlantType;
    [Export] protected SpriteFrames Frames;
    [Export] protected PackedScene PlantScene;
    [Export] protected int PlantDbId = -1;
    [Export] protected Area2D Area;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        if (Area is null)
        {
            Area = new Area2D();
            var collisionShape2D = new CollisionShape2D();
            var shape = new RectangleShape2D();
            shape.Size = Texture.GetSize();
            collisionShape2D.Shape = shape;

            Area.AddChild(collisionShape2D);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

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