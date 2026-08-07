using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.Boundaries;
using Main.main.packages.inventory;
using Main.main.packages.items;
using Main.main.packages.mc;
using Main.main.scripts.core.util;
using Main.main.scripts.scene;

namespace Main.main._Outside_Building;

public enum MovementType
{
    Walk,
    Run,
    Stand,
    Carrying,
    Jump,
    Climb,
    Crouch,
    Prone,
    Squeeze,
    Roll,
}

public partial class Player : AnimatedSprite2D
{
    [Export] protected float MovementSpeed = .02f;
    public Vector2 ItemPositon { get; protected set; }
    protected BasicScene ParentScene;


    protected IDeployable Deployable = null;
    protected Blueprint Blueprint = null;

    protected Node HeldItem = null;


    protected Vector2 FacingUnitVector = Vector2.Zero;

    protected Area2D Proximity;
    protected Area2D Hitbox;
    protected Area2D Base;
    protected ItemUsage ItemRange;

    protected Dictionary<MovementType, float> MovementSpeedRatio = new Dictionary<MovementType, float>()
    {
        { MovementType.Walk, 1f },
        { MovementType.Run, 1.25f },
        { MovementType.Stand, 0f },
        { MovementType.Carrying, .5f },
    };

    private MovementType _movementType = MovementType.Walk;

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

        ItemRange = GetNode("ItemUsage") as ItemUsage;
        if (ItemRange == null)
            GD.PrintErr("ItemUsage not found");

        Base = GetNode("Base") as Area2D;
        if (Base == null)
            GD.PrintErr("Player base not found");

        Inventory.Instance.SelectedItemChanged += PulloutItem;
        Inventory.Instance.NoSelectedItem += ClearItem;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");


        if (direction != Vector2.Zero)
        {
            Movement(direction * MovementSpeed * MovementSpeedRatio[_movementType]); // * (float)delta);
            FacingUnitVector = direction.Normalized();

            if (FacingUnitVector.Y > 0)
            {
                direction.Y += MovementSpeed * (float)delta;
                Animation = "front";
            }
            else if (FacingUnitVector.Y < 0)
            {
                direction.Y -= MovementSpeed * (float)delta;
                Animation = "back";
            }
            else if (FacingUnitVector.X > 0)
            {
                direction.X += MovementSpeed * (float)delta;
                Animation = "side";
                FlipH = false;
            }
            else if (FacingUnitVector.X < 0)
            {
                direction.X -= MovementSpeed * (float)delta;
                Animation = "side";
                FlipH = true;
            }
        }

        //Inefficient: checked every frame, but bugged otherwise.
        if (Blueprint is { Visible: true })
        {
            Blueprint.GlobalPosition = GlobalPosition.Floor() + (FacingUnitVector.Floor() * Blueprint.DisplayOffset);
            Blueprint.CheckPlacement();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);


        //Carrying an object
        if (Deployable != null)
        {
            if (@event.IsAction("left_click"))
            {
                if (Blueprint is { ValidPlacement: true })
                {
                    PutDown();
                }
            }
            else if (@event.IsActionPressed("right_click"))
            {
                if (Blueprint == null)
                {
                    Blueprint = Deployable.GetBlueprint();
                    GetTree().CurrentScene.AddChild(Blueprint);
                    Blueprint.SetVisibility(false);
                }

                Blueprint.SetVisibility();
                _movementType = (Blueprint.Visible) ? MovementType.Stand : MovementType.Carrying;
            }

            if (Blueprint != null)
            {
                if (@event.IsAction("distance_increase"))
                {
                    Blueprint.ChangeDisplayOffset(1);
                }

                if (@event.IsAction("distance_decrease"))
                {
                    Blueprint.ChangeDisplayOffset(-1);
                }
            }
        }

        if (@event.IsAction("use_item"))
        {
            if (HeldItem is IItem item)
            {
                if (@event.IsActionPressed("use_item"))
                {
                    ItemRange.SetDirection(FacingUnitVector);
                    ItemRange.Enable(true);
                    var overlappingAreas = ItemRange.GetOverlappingAreas();
                    if (overlappingAreas.Count > 0)
                        item.Use(overlappingAreas[0].GetParent());
                }
            }
        }
        //if (@event.IsActionPressed(""))
    }

    protected void Movement(Vector2 change)
    {
        var targetPosition = GlobalPosition + change;

        var baseShape = Base.GetChild(0);
        if (baseShape is not CollisionShape2D shape) throw new Exception();

        //ai slop:

        // 1. Take the actual global transform of the CollisionShape2D (preserving its exact scale & rotation)
        Transform2D queryTransform = shape.GlobalTransform;

        // 2. Calculate the position offset relative to the Player's position
        Vector2 shapeOffset = shape.GlobalPosition - GlobalPosition;

        // 3. Set the query transform's origin to the target position + local shape offset
        queryTransform.Origin = targetPosition + shapeOffset;

        using var query = new PhysicsShapeQueryParameters2D();
        query.Shape = shape.GetShape();
        query.Transform = queryTransform;
        query.CollisionMask = Base.CollisionMask;
        query.CollideWithAreas = true; //CollideWithBodies =  true,
        query.Exclude = [Proximity.GetRid(), Hitbox.GetRid(), Base.GetRid()];

        var hits = GetWorld2D().DirectSpaceState.IntersectShape(query);

        bool blocked = false;
        bool withinBoundary = false;

        foreach (var hit in hits)
        {
            if (hit["collider"].AsGodotObject() is CollisionObject2D col)
            {
                if (col.GetCollisionLayerValue(1)) blocked = true;
                if (col.GetCollisionLayerValue(3)) withinBoundary = true;
            }
        }

        if (!blocked && withinBoundary)
        {
            GlobalPosition = targetPosition;
        }
    }


    //Items -------------------------------------------------------------
    public void PulloutItem(Node node)
    {
        if (_movementType == MovementType.Carrying) return;
        if (node == null) return;

        HeldItem = node;
        AddChild(node);
        if (node is IItem item)
        {
            item.Position = ItemPositon;
        }
    }

    private void ClearItem()
    {
        HeldItem = null;
        ItemRange.Enable(false);
    }

    //Deployables
    public void PickedUp(IDeployable deployable)
    {
        if (!deployable.CanCarry() || HeldItem != null)
            return;

        Deployable = deployable;
        Deployable.Collisions(false);

        if (Deployable is Node2D n)
        {
            n.GetParent().RemoveChild(n);
            AddChild(n);
            n.Position = new Vector2(0, -50);

            n.GlobalScale = new Vector2(1, 1);
        }

        _movementType = MovementType.Carrying;
    }

    /**
     * returns whether Deployable was deployed
     */
    public bool PutDown()
    {
        Blueprint.Visible = false;
        _movementType = MovementType.Walk;

        if (Deployable is Node2D n)
        {
            RemoveChild(n);
            n.GlobalScale = new Vector2(1, 1);
        }

        Deployable.Collisions(true);
        var result = Deployable.Deploy(Blueprint);
        if (result)
        {
            Blueprint = null;
            Deployable = null;
        }

        return result;
    }

    public Area2D GetBase()
    {
        return Base;
    }
}