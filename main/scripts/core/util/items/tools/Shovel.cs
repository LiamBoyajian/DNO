using Godot;

namespace Main.main.scripts.core.util.items.tools;

public partial class Shovel : Node2D
{
    [Export] protected RigidBody2D CollisionArea2D;
    [Export] protected StaticBody2D Body2D;

    protected bool Follow = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CollisionArea2D.InputEvent += EquipShovel;
        Body2D.InputEvent += EquipShovel;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Follow)
        {
            //GD.Print(GetGlobalMousePosition());
            Body2D.GlobalPosition = GetGlobalMousePosition();
        }
    }

    private void EquipShovel(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsAction("Click"))
        {
            Follow = !Follow;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (Follow && @event.IsAction("Click"))
            Follow = false;
    }
}