using Godot;

namespace Main.main.packages.items.tools.microchip_injector;

public partial class Injector : AnimatedSprite2D, IItem
{
    public Texture2D DragIcon
    {
        get => GetSpriteFrames().GetFrameTexture(Animation, Frame);
    }

    public Texture2D Icon
    {
        get => GetSpriteFrames().GetFrameTexture(Animation, Frame);
    }

    public Texture2D HeldIcon
    {
        get => GetSpriteFrames().GetFrameTexture(Animation, Frame);
    }

    public bool Use(Node target = null)
    {
        throw new System.NotImplementedException();
    }
}