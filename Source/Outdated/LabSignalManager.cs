using Godot;
using System;
using Main.LabAssetts;
using Main.Package;
using Main.Source;


/**
 * Should be on the head node
 */
public partial class LabSignalManager : Node2D
{
    // Called when the node enters the scene tree for the first time.

    public Machines MachinesStruct;
    private MainCharacter _player;

    public override void _Ready()
    {
        _player =
            GetNode<CharacterBody2D>(
                    "CharacterBody2D") as
                MainCharacter; //Can leave separate since it's useful to have this particular reference

        if (_player == null) throw new MissingFieldException("Player not found in scene. ", nameof(_player));
        _player.RequestNearestDevice += HandlePlayerOpen;

        var arrayOfNodesInScene = GetChild(0).GetChildren();
        MachinesStruct = new Machines(arrayOfNodesInScene);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void HandlePlayerOpen(Vector2 playerPos)
    {
        var i = 0;
        foreach (ref readonly var machine in MachinesStruct.Elements.AsSpan())
        {
            var posDifference = machine.Position - playerPos;
            if (Math.Abs(posDifference.X) < machine.Size.X / 2 && Math.Abs(posDifference.Y) < machine.Size.Y / 2)
            {
                //can't use index, can't use name, can't use reference.
                Console.WriteLine(posDifference);
                _player.CatchMachine(GetChild(0).GetChildren()[i] as AbstractMachine);
            }

            ++i;
        }
    }
}