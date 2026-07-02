using Godot;

namespace Main.Package;

public partial class BaseMachine(AnimatedSprite2D sprite, Area2D area) : Node
{
    private AnimatedSprite2D _mySprite = sprite;

    private Area2D _myArea = area;

    //Action new
    public Vector2 GetPosition()
    {
        return _mySprite.Position;
    }

    public Vector2 GetDistance(Vector2 position)
    {
        return GetPosition() - position;
    }
}