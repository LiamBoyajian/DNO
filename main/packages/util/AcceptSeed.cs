using System.Diagnostics;
using Godot;
using Main.main.packages.items;
using Main.main.scripts.core.util;

// Adjust "DisappearSlot" to match the actual C# class name of your base script
namespace Main.main.packages.util;

public partial class AcceptSeed : DisappearSlot
{
    protected packages.containers.ContainPlant ParentContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        if (GetParent() is not packages.containers.ContainPlant parentContainPlant)
        {
            Debug.Assert(false, "parent is not type: ContainPlant");
        }
        else
        {
            ParentContainer = (packages.containers.ContainPlant)parentContainPlant;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override bool AfterAccepted(WrapperIItem wrapperIItem)
    {
        if (ParentContainer == null)
            return false;

        wrapperIItem.Decompose(out IItem item);

        if (item is not ItemSeed seed)
            return false;


        var plant = seed.GetPlantScene().Instantiate();

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

        var currentAnimation = animatedSprite.Animation;
        var currentFrameIndex = animatedSprite.Frame;
        var texture = animatedSprite.SpriteFrames.GetFrameTexture(currentAnimation, currentFrameIndex);

        animatedSprite.Position =
            new Vector2(0, -(texture.GetSize().Y / 2f)) + ParentContainer.Offset * 2 + new Vector2(0, 1);


        var newPlant = ParentContainer.AcceptSeed(plant, seed.GetPlantDbId());

        plant.Call("Init");

        return true;
    }
}