using Godot;
using System;

public partial class MicrochipPrinter : AnimatedSprite2D
{
    public enum MachineState
    {
        Empty,
        Running,
        Complete
    }

    public Area2D Area { get; set; }

    public MachineState State { get; private set; } = MachineState.Empty;

    public override void _Ready()
    {
        base._Ready();
        if (GetChild(0) is not Area2D area2D) throw new Exception("Child 0 is not Area2d");
        Area = area2D;
        area2D.AreaEntered += AreaEnteredHandler;
        area2D.InputEvent += AreaClickedHandler;
    }

    private void AreaClickedHandler(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("click"))
        {
            if ()
        }
    }

    private void AreaEnteredHandler(Area2D area)
    {
        throw new NotImplementedException();
    }
}