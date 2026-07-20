using Godot;

namespace Main.main.scripts.scene;

public partial class BasicScene : Node2D
{
    [Export] protected Area2D Boundaries;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Boundaries == null)
        {
            Boundaries = GetNode<Area2D>("Boundaries");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public bool IsInBoundaries(Vector2 position)
    {
        if (Boundaries == null)
        {
            GD.PrintErr("boundaries are null in " + this);
            return false;
        }

        //This might be slow idk
        foreach (var node in Boundaries.GetChildren())
        {
            if (node is not CollisionShape2D shape) continue;
            if (shape.Shape.GetRect().HasPoint(shape.ToLocal(position)))
                return false;
        }


        return true;
    }
}