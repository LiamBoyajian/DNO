using Godot;
using Main.InventoryAssets;

namespace Main.Package;

/**
 * Extend this class for each machine and place that script onto the machine directly.
 * Sprite size should be uniform across all frames
 */
public abstract partial class AbstractMachine : Node
{
    protected AnimatedSprite2D Sprite;
    protected InventoryContainer Inventory;

    public Vector2 GetSpriteSize()
    {
        if (Sprite == null)
            return new Vector2(0, 0);
        return Sprite.SpriteFrames.GetFrameTexture(Sprite.Animation, Sprite.Frame).GetSize();
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

    public AnimatedSprite2D GetSprite()
    {
        return Sprite;
    }

    public Vector2 GetSpritePosition()
    {
        if (Sprite == null)
            return Vector2.Zero;
        return Sprite.Position;
    }
}