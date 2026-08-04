using System;
using Godot;
using Main.main.packages.items;
using Main.main.scripts.core.util;
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

public enum FacingDirections
{
    Left,
    Right,
    Behind,
    Forward,
}

public partial class Player : AnimatedSprite2D
{
    [Export] private float _movementSpeed = .02f;
    public Vector2 ItemPositon { get; protected set; }
    protected BasicScene ParentScene;

    protected IDeployable Deployable = null;
    protected Blueprint Blueprint = null;


    protected Area2D Proximity;
    protected Area2D Hitbox;

    private FacingDirections _facing = FacingDirections.Forward;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ParentScene = GetParent() as BasicScene;
        if (ParentScene == null)
            GD.PrintErr("Player node not a child of parent scene");

        Proximity = GetNode("Proximity") as Area2D;
        if (Proximity == null)
            GD.PrintErr("Proximity not found");

        Hitbox = GetNode("Hitbox") as Area2D;
        if (Hitbox == null)
            GD.PrintErr("Player hitbox not found");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        if (direction.X < 0) _facing = FacingDirections.Left;
        if (direction.X > 0) _facing = FacingDirections.Right;
        if (direction.Y > 0) _facing = FacingDirections.Forward;
        if (direction.Y < 0) _facing = FacingDirections.Behind;

        if (Input.IsActionJustPressed("right_click"))
        {
            if (Blueprint == null)
            {
                Blueprint = Deployable.GetBlueprint();
                GetTree().Root.AddChild(Blueprint);
            }
            else
            {
                Blueprint = null;
            }
        }

        if (Blueprint != null)
        {
            var offset = FacingUnitVector() * Blueprint.DisplayOffset;
            Blueprint.GlobalPosition = GlobalPosition + offset;
        }

        if (direction != Vector2.Zero)
            Movement(direction * _movementSpeed, MovementTypes.Walk);

        if (_facing == FacingDirections.Forward)
        {
            direction.Y += _movementSpeed * (float)delta;
            Animation = "Front";
        }
        else if (_facing == FacingDirections.Behind)
        {
            direction.Y -= _movementSpeed * (float)delta;
            Animation = "Front";
        }
        else if (_facing == FacingDirections.Right)
        {
            direction.X += _movementSpeed * (float)delta;
            Animation = "Side";
            FlipH = false;
        }
        else if (_facing == FacingDirections.Left)
        {
            direction.X -= _movementSpeed * (float)delta;
            Animation = "Side";
            FlipH = true;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);


        //if (@event.IsActionPressed(""))
    }

    protected bool Movement(Vector2 direction, MovementTypes movementType)
    {
        if (movementType is MovementTypes.Run or MovementTypes.Walk)
        {
            if (!ParentScene.IsInBoundaries(GlobalPosition + direction))
                GlobalPosition += direction;
        }

        return true;
    }

    public void PickedUp(IDeployable deployable)
    {
        Deployable = deployable;
        if (Deployable is Node n)
        {
            n.GetParent().RemoveChild(n);
            AddChild(n);
        }
    }

    /**
     * returns whether Deployable was deployed
     */
    public bool PutDown(Node viewport, Vector2 pos)
    {
        if (Deployable is Node n)
            RemoveChild(n);
        Deployable.Deploy(viewport, pos);
        Deployable = null;
        return true;
    }

    public Vector2 FacingUnitVector()
    {
        if (_facing == FacingDirections.Forward)
            return new Vector2(0, 1);
        if (_facing == FacingDirections.Behind)
            return new Vector2(0, -1);
        if (_facing == FacingDirections.Left)
            return new Vector2(-1, 0);
        if (_facing == FacingDirections.Right)
            return new Vector2(1, 0);
        return Vector2.Zero;
    }
}