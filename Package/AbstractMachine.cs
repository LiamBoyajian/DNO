using System;
using System.Reflection.PortableExecutable;
using Godot;
using Main.InventoryAssets;

namespace Main.Package;

/**
 * Extend this class for each machine and place that script onto the machine directly.
 * Sprite size should be uniform across all frames
 */
public abstract partial class AbstractMachine : AnimatedSprite2D
{
    //protected AnimatedSprite2D Sprite;
    protected InventoryContainer Inventory;


    public Vector2 GetSpriteSize()
    {
        if (base.GetSpriteFrames() == null)
            return new Vector2(0, 0);
        return base.SpriteFrames.GetFrameTexture(base.Animation, 0).GetSize();
    }

    /**
     * Returns the machine's former buffer item and puts the item argument into its buffer slot
     */
    public ItemTexture SwapBufferItem(ItemTexture item)
    {
        return Inventory.TakeBufferItem(item);
    }

    public void ToggleInventory()
    {
        Inventory.ToggleVisible();
    }

    public void ShowInventory()
    {
        Inventory.Show();
    }

    public void HideInventory()
    {
        Inventory.Hide();
    }

    public override void _Ready()
    {
        this.AddChild(Inventory);
    }
}