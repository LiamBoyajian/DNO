using Godot;
using Main.main.packages.containers;
using Main.Source.main;

namespace Main.main.packages.items.tools.watering_can;

public partial class WateringCan : AnimatedSprite2D, IItem
{
    protected MaterialResource Water = new MaterialResource(1000, 1000);


    public override void _Ready()
    {
        base._Ready();
    }


    //Access methods
    public void Fill(int delta)
    {
        if (delta < 0) return;
        Water.Give(delta);
    }

    public bool Use(Node target = null)
    {
        if (target == null) return false;
        if (Water.Amount == 0) return false;
        if (target is not IWaterable waterable) return false;
        return Water.Take(waterable.GiveWater(Water.Amount)) > 0;
    }

    //IItem
    public Texture2D DragIcon
    {
        get => GetCurrentFrame();
    }

    public Texture2D Icon
    {
        get => GetCurrentFrame();
    }

    public Texture2D HeldIcon
    {
        get => GetCurrentFrame();
    }

    public Texture2D GetCurrentFrame()
    {
        return GetSpriteFrames().GetFrameTexture(Animation, GetFrame());
    }
}