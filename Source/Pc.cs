using Godot;
using System;

public partial class Pc : AnimatedSprite2D
{
    // Called when the node enters the scene tree for the first time.
    //private Area2D _area2D;

    [Signal]
    public delegate void OpenPcEventHandler(string scene);

    public override void _Ready()
    {
        //_area2D = GetNode<Area2D>("Area2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnOpenedSignal(Vector2 position)
    {
        var posDifference = this.Position - position;
        var temp = this.SpriteFrames.GetFrameTexture("On", 0).GetSize();
        if (posDifference.Length() > temp.Length()) //TODO positions are relative or something
        {
            EmitSignalOpenPc("res://Source/DnaSequence.tscn");
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
    }
}