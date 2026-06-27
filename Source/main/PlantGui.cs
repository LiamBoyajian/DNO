using System.Runtime.InteropServices.JavaScript;
using Godot;

namespace Main.Source.main;

public abstract partial class PlantGui : AnimatedSprite2D
{
    //requires specifically named frames to run 


    public override void _Ready()
    {
        Play("default");
    }

    protected void _Process(float delta) //TODO idk sm wrong here should have an override
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
}