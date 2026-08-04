using System;
using Godot;
using Main.main.scripts.scene;

namespace Main.main._Outside_Building;

public enum MovementTypes
{
    Walk,
    Run,
    Jump,
    Climb,
    Crouch,
    Prone,
    Squeeze,
    Roll,
}

public partial class Player : AnimatedSprite2D
{
    [Export] private float _movementSpeed = 50f;
    protected BasicScene ParentScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ParentScene = GetParent() as BasicScene;
        if (ParentScene == null)
            GD.PrintErr("Player node not a child of parent scene");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var direction = new Vector2();
        if (Input.IsActionPressed("ui_down"))
        {
            direction.Y += _movementSpeed * (float)delta;
            Animation = "Front";
        }

        if (Input.IsActionPressed("ui_up"))
        {
            direction.Y -= _movementSpeed * (float)delta;
            Animation = "Front";
        }

        if (Input.IsActionPressed("ui_right"))
        {
            direction.X += _movementSpeed * (float)delta;
            Animation = "Side";
            FlipH = false;
        }

        if (Input.IsActionPressed("ui_left"))
        {
            direction.X -= _movementSpeed * (float)delta;
            Animation = "Side";
            FlipH = true;
        }

        Movement(direction, MovementTypes.Walk);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);


        //if (@event.IsActionPressed(""))
    }

    public bool Movement(Vector2 direction, MovementTypes movementType)
    {
        if (movementType is MovementTypes.Run or MovementTypes.Walk)
        {
            if (!ParentScene.IsInBoundaries(GlobalPosition + direction))
                GlobalPosition += direction;
        }

        return true;
    }
}