using Godot;
using System;
using Main.Package;
using Main.Source;


/**
 * Should be on the head node
 */
public partial class LabSignalManager : Node2D
{
    // Called when the node enters the scene tree for the first time.

    public Machines MachinesStruct;

    [Signal]
    public delegate void NearestMachineEventHandler(Node machine);
    //TODO make a machine class


    public override void _Ready()
    {
        //get a reference to everything in the scene.
        //** I could make an array to get everything but there might be trash in there or I would need to enforce some rule about what can go in there.
        //Otherwise I need to find everything manually. Or I can do the array and check the type of the object and also check the nearest object which I am also in bounds of.

        var arrayOfNodesInScene = GetChildren();
        var player =
            GetNode<CharacterBody2D>(
                "CharacterBody2D"); //Can leave separate since it's useful to have this particular reference
        //arrayOfNodesInScene.Remove(player);

        //Should catch a signal from player being the request for the nearest object
        MachinesStruct = new Machines(arrayOfNodesInScene);
        foreach (ref readonly var machine in MachinesStruct.Elements.AsSpan())
        {
            Console.WriteLine(machine);
        }
        //var pc = GetNode<AnimatedSprite2D>("Pc"); //TODO replace
        //arrayOfNodesInScene;
        //player.Connect(Main.LabAssetts.MainCharacter.SignalName.OpenedSignal,
        //    new Callable(pc, antiquated1.MethodName.OnOpenedSignal));
        //pc.Connect(antiquated1.SignalName.OpenPc, new Callable(player, Main.LabAssetts.MainCharacter.MethodName.OpenScene));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void HandlePlayerOpen(Vector2 playerPos)
    {
    }
}