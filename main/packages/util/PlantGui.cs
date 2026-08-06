using System;
using Godot;
using Main.main.packages.inventory;
using Main.main.scripts.core.plants;
using Main.main.scripts.core.util.inventory;
using Main.main.scripts.core.util.items.tools;
using Shovel = Main.main.scripts.core.util.items.tools.Shovel;

namespace Main.main.scripts.core.util;

public partial class PlantGui : AnimatedSprite2D
{
    //requires specifically named frames to run 
    protected AbstractPlant ParentPlant;
    public string NoGrowth = "no_growth";
    public string Dead = "dead";
    public string Default = "default";
    public event Action DugUp;

    public override void _Ready()
    {
        Play(Default);
        Stop();

        if (GetParent() is not AbstractPlant)
            System.Diagnostics.Debug.Assert(false, "Parent plant is not of type: AbstractPlant");
        ParentPlant = GetParent() as AbstractPlant;
    }

    public override void _Process(double delta)
    {
    }

    /**
     * Plays the plant's frame for a dead plant
     */
    public void DeadFrame()
    {
        Play(Dead);
    }

    /**
     * Plays the frame for no growth (e.g. a pile of dirt)
     */
    public void NoGrowthFrame()
    {
        Play(NoGrowth);
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
        return string.Compare(Animation, Dead) == 0;
    }

    public bool SetGrowthFrame(int frame)
    {
        if (frame < 0 || frame >= SpriteFrames.GetFrameCount(Animation))
            return false;

        Frame = frame;

        return true;
    }

    private void BodyEnteredEventHandler(Node2D node2D)
    {
        if (node2D.GetParent() is AbstractTool tool)
        {
            switch (tool.Type)
            {
                case ToolType.PruningShears:
                    if (ParentPlant is IShearable shearable)
                    {
                        var sheared = shearable.Shear(1);
                        Inventory.Instance.AddMoney(sheared, true);
                    }

                    break;

                case ToolType.Shovel:
                    if (ParentPlant is { } abstractPlant)
                    {
                        abstractPlant.DigUp();
                    }

                    break;
            }
        }

        if (node2D is RigidBody2D rb)
        {
            if (rb.GetParent() is Shovel)
            {
                ParentPlant.QueueFree();
                DugUp?.Invoke();
            }
        }
    }
}