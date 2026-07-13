using Godot;
using Main.main.common.ui.templatePopup;

namespace Main.main.scripts.core.util;

public partial class PlantGui : AnimatedSprite2D
{
    //requires specifically named frames to run 
    [Export] protected Area2D ClickArea;

    public override void _Ready()
    {
        Play("default");
        ClickArea.InputEvent += OnClickMe;
        if (ClickArea is null)
            ClickArea = GetNode<Area2D>("PopupArea");
    }

    public override void _Process(double delta)
    {
    }

    /**
     * Plays the plant's frame for a dead plant
     */
    public void DeadFrame()
    {
        Play("dead");
    }

    /**
     * Plays the frame for no growth (e.g. a pile of dirt)
     */
    public void NoGrowthFrame()
    {
        Play("ground");
    }

    /**
     * Switches to the next frame in the current animation (represents the plant growing)
     * Does not support looping animations
     *
     */
    public bool NextGrowthFrame()
    {
        if (IsFinalGrowthFrame()) return false;

        Frame += 1;
        return true;
    }

    /**
     * Play the frame-loop for the current animation (e.g. to show wind or some other looping animation)
     *
     */
    public void CurrentGrowthPlayAnimation()
    {
        return;
    }

    /**
     * Switches to the next animation
     */
    public void NextGrowthAnimation()
    {
    }

    public bool IsFinalGrowthFrame()
    {
        return Frame >= SpriteFrames.GetFrameCount(Animation);
    }

    public bool IsShowingDead()
    {
        return string.Compare(Animation, "dead") == 0;
    }

    public void OnClickMe(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (!@event.IsAction("alt_click"))
            return;
        TemplatePopup.Instance.Popup();
    }
}