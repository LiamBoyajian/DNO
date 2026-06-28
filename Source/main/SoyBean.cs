using System;
using Godot;

namespace Main.Source.main;

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
        //Console.Write(FrameSum);
        //
        Tick(delta);
    }

    public override void Tick(double delta)
    {
        IsDeadThenDeadFrame();
        FrameSum += delta;
        if (FrameSum < 5.0)
            return;
        MyResources[Rt.Health].Take(10.0);
        FrameSum = 0.0;
        Console.Write($"\nHEALTH REMAINING: {MyResources[Rt.Health].Amount}");

        Resources[Rt.H2O].Give(25.0);
        Resources[Rt.Co2].Give(50.0);

        _consume();
        if (Resources[Rt.Health].Amount <= 0.0)
            Console.Write("\n\n PLANT DEAD \n");
        //TESTING TODO
        //if (GetSunLevel() >= 0.0)
        //    _photosynthesize(GetSunLevel());


        Console.Write($"Glucose  {Resources[Rt.Glucose].Amount}");
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