using System;
using System.Reflection.PortableExecutable;
using Godot;
using Main.InventoryAssets;

namespace Main.Package;

/**
 * Extend this class for each machine and place that script onto the machine directly.
 * Sprite size should be uniform across all frames
 */
public abstract partial class AbstractMachine(Vector2 size, int slots, TextureButton button)
    : InventoryContainer(size, slots, button)
{
    //protected AnimatedSprite2D Sprite;
    //protected InventoryContainer Inventory;
    public AnimatedSprite2D AnimatedSprite;

    public Vector2 GetSpriteSize()
    {
        if (AnimatedSprite.GetSpriteFrames() == null)
            return new Vector2(0, 0);
        return AnimatedSprite.SpriteFrames.GetFrameTexture(AnimatedSprite.Animation, 0).GetSize();
    }

    /**
     * Returns the machine's former buffer item and puts the item argument into its buffer slot
     */
    public ItemTexture SwapBufferItem(ItemTexture item)
    {
        return TakeBufferItem(item);
    }


    public override void _Ready()
    {
    }
}