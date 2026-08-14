using System.Numerics;
using Godot;
using Vector2 = Godot.Vector2;

namespace Main.main.packages.mc;

public partial class ItemUsage : Area2D
{
    public static Vector2 Side = new Vector2(14, -24);
    public static Vector2 Back = new Vector2(0, -24);
    public static Vector2 Front = new Vector2(0, -26);


    public void SetDirection(Vector2 direction)
    {
        if (direction == Vector2.Right)
        {
            Position = Side;
        }
        else if (direction == Vector2.Left)
        {
            Position = Side * new Vector2(-1, 1);
        }
        else if (direction == Vector2.Up)
        {
            Position = Back;
        }
        else if (direction == Vector2.Down)
        {
            Position = Front;
        }
    }

    public void Enable(bool? enable)
    {
        var collisionShape = (CollisionShape2D)GetChild(0);
        if (enable is null)
        {
            Visible = !Visible;
            Monitoring = !Monitoring;
            Monitorable = !Monitorable;
            collisionShape.Disabled = !collisionShape.Disabled;
        }
        else if ((bool)enable)
        {
            Visible = true;
            Monitoring = true;
            Monitorable = true;
            collisionShape.Disabled = false;
        }
        else
        {
            Visible = false;
            Monitoring = false;
            Monitorable = false;
            collisionShape.Disabled = true;
        }
    }
}