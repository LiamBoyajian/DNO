using Godot;
using Main.InventoryAssets;

namespace Main.Package;

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

    public ItemTexture TakeMyBufferItem()
    {
        return Inventory.TakeBufferItem();
    }
}