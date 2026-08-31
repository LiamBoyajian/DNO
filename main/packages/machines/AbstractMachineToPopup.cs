using System;
using Godot;

namespace Main.main.packages.machines;

public abstract partial class AbstractMachineToPopup : AnimatedSprite2D
{
    [Export] public PackedScene PopupScene;
    [Export] public Area2D Area;

    public override void _Ready()
    {
        base._Ready();

        if (Area == null)
        {
            Area = GetChild<Area2D>(0);
            if (Area == null) throw new Exception("Click area is null");
        }

        if (PopupScene == null) throw new Exception("PopupScene is null");

        Area.InputEvent += AreaOnInputEvent;
    }

    private void AreaOnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            var scene = PopupScene.Instantiate();

            if (InstantiateFromPopup(scene))
                AddChild(scene);
        }
    }

    /**
     * Returns whether the passed node was valid
     */
    protected abstract bool InstantiateFromPopup(Node node);
}