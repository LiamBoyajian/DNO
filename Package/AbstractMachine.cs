using System;
using System.Reflection.PortableExecutable;
using Godot;
using Main.InventoryAssets;

namespace Main.Package;

/**
 * Extend this class for each machine and place that script onto the machine directly.
 * Sprite size should be uniform across all frames
 */
public abstract partial class AbstractMachine()
    : AnimatedSprite2D
{
    //protected AnimatedSprite2D Sprite;
    //protected InventoryContainer Inventory;
    protected InventoryContainer Inventory;

    public Vector2 GetSpriteSize()
    {
        if (SpriteFrames == null)
            return new Vector2(0, 0);
        return SpriteFrames.GetFrameTexture(Animation, 0).GetSize();
    }

    /**
     * Returns the machine's former buffer item and puts the item argument into its buffer slot
     */
    public ItemTexture SwapBufferItem(ItemTexture item)
    {
        return Inventory?.TakeBufferItem(item);
    }
    //I need a method that reemits the signal from inventory and also i need to somehow handle the bufferslot swapping.
    public override void _Ready()
    {
    }
}