using Godot;
using System;

public partial class LabSignalManager : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var player = GetNode<CharacterBody2D>("CharacterBody2D");
        var pc = GetNode<AnimatedSprite2D>("PC");

        player.Connect(Main.LabAssetts.MainCharacter.SignalName.OpenedSignal,
            new Callable(pc, Pc.MethodName.OnOpenedSignal));
        pc.Connect(Pc.SignalName.OpenPc, new Callable(player, Main.LabAssetts.MainCharacter.MethodName.OpenScene));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}