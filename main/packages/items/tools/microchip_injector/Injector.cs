using System;
using Godot;
using Main.main.packages.containers;
using Main.main.scripts.core.plants;

namespace Main.main.packages.items.tools.microchip_injector;

public partial class Injector : AnimatedSprite2D, IItem
{
    public int MicrochipId = -1;

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

    public void SetMicrochipId(int microchipId)
    {
        GD.Print("Setting microchip id to " + microchipId);
        if (microchipId < 0) return;
        MicrochipId = microchipId;
        Animation = "full";
    }

    public bool Use(Node target = null)
    {
        if (MicrochipId < 0) return false;
        if (target is not IInjectable injectable)
        {
            GD.PrintErr("Not an IInjectable");
            return false;
        }

        injectable.InjectDbId(MicrochipId);
        GD.Print("Setting microchip id to " + MicrochipId);

        MicrochipId = -1;
        Animation = "default";
        return true;
    }
}