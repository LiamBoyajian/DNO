using System.ComponentModel;
using Godot;
using Main.main.packages.containers;
using Main.main.packages.inventory;
using Main.main.packages.plants.interfaces;

namespace Main.main.packages.items.tools.shears;

public partial class Shears : AnimatedSprite2D, IItem
{
    public const int FruitPerShear = 1;

    //public Vector2 Position { get; set; }
    public Texture2D DragIcon => GetCurrentTexture();
    public Texture2D Icon => GetCurrentTexture();
    public Texture2D HeldIcon => GetCurrentTexture();
    public new void Hide() => ((AnimatedSprite2D)this).Hide();

    public new void Show() => ((AnimatedSprite2D)this).Show();

    public bool Use(Node target = null)
    {
        if (target == null) return false;
        if (target is not ContainPlant container) return false;
        if (container.GetPlants()[0] is not IShearable shearable) return false;

        int result = shearable.Shear(FruitPerShear);
        Inventory.Instance.AddMoney(result, true);
        return result > 0;
    }

    public new void Reparent(Node newParent, bool keepGlobalTransform = false) =>
        ((Node2D)this).Reparent(newParent, keepGlobalTransform);

    public Texture2D GetCurrentTexture()
    {
        return GetSpriteFrames().GetFrameTexture(Animation, Frame);
    }
}