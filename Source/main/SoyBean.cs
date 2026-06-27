using Godot;
using System;
using Main.Source.main;

public partial class SoyBean : AbstractPlant
{
    [Export] private PlantGui _guiManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _guiManager ??= GetChild<PlantGui>(0);
        Console.Write("GuiManger set to: " + _guiManager);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    protected override bool GrowthUpdateFrame()
    {
        if (IsDeadThenDeadFrame())
            return false; //plant died :(

        return true;
    }

    protected override bool IsDeadThenDeadFrame()
    {
        if (IsAlive())
            return false;
        _guiManager.DeadFrame();
        return true;
    }
}