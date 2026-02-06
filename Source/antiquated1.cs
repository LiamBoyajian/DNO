using Godot;
using System;
using Main.InventoryAssets;

public partial class antiquated1 : AnimatedSprite2D
{
    // Called when the node enters the scene tree for the first time.
    //private Area2D _area2D;

    [Signal]
    public delegate void OpenPcEventHandler(string scene);

    private InventoryContainer _myInventory;

    public override void _Ready()
    {
        _myInventory = new InventoryContainer();
        //_area2D = GetNode<Area2D>("Area2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnOpenedSignal(Vector2 position)
    {
        var posDifference = this.GlobalPosition - position;
        var pcSprite = this.SpriteFrames.GetFrameTexture("On", 0).GetSize();
        if (Math.Abs(posDifference.X) < pcSprite.X / 2 &&
            Math.Abs(posDifference.Y) < pcSprite.Y / 2) //TODO positions are relative or something
        {
            EmitSignalOpenPc("res://Source/DnaSequence.tscn");
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
    }
}