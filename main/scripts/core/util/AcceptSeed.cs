using System.Diagnostics;
using Godot;

// Adjust "DisappearSlot" to match the actual C# class name of your base script
namespace Main.main.scripts.core.util;

public partial class AcceptSeed : DisappearSlot
{
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

    public override bool AfterAccepted(Variant seedVariant)
    {
        // 1. Parent validation
        //var parent = GetParent() as ContainPlant;
        if (GetParent() is not ContainPlant parentContainPlant)
        {
            Debug.Assert(false, "parent is not type: ContainPlant");
            return false;
        }

        if (seedVariant.Obj is not ItemSeed seed)
        {
            Debug.Assert(false, "param is not type: Item_Seed");
            return false;
        }

        var plant = seed.GetPlantScene().Instantiate();
        //templatePlant.Name = "soybean - id:" + seed.GetPlantDbId().ToString();

        var plantId = plant.GetInstanceId();
        plant.SetScript(seed.GetPlantType());

        plant = GodotObject.InstanceFromId(plantId) as Node;
        if (plant == null)
        {
            GD.PrintErr("Couldn't find plant: " + plantId);
            return false;
        }


        var animatedSprite = plant.GetChild<AnimatedSprite2D>(0);
        animatedSprite.SpriteFrames = seed.GetFrames();

        animatedSprite.Scale *= 4;
        animatedSprite.Position = new Vector2(0, -78); //TODO bad hardcoded


        var newPlant = parentContainPlant.AcceptSeed(plant, seed.GetPlantDbId());

        plant.Call("Init");

        return true;
    }
}