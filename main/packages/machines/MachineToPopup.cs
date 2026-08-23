using System;
using Godot;

namespace Main.main.packages.machines;

public partial class MachineToPopup : Node
{
    [Export] public PackedScene PopupScene;
    [Export] public Area2D ClickArea;

    public override void _Ready()
    {
        base._Ready();

        if (ClickArea == null)
        {
            ClickArea = GetChild<Area2D>(0);
            if (ClickArea == null) throw new Exception("Click area is null");
        }

        if (PopupScene == null) throw new Exception("PopupScene is null");

        ClickArea.InputEvent += ClickAreaOnInputEvent;
    }

    private void ClickAreaOnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            var scene = PopupScene.Instantiate();
            GetTree().Root.AddChild(scene);
        }
    }
}