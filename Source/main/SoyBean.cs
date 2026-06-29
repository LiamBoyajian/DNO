using System;
using Godot;

namespace Main.Source.main;

public partial class SoyBean : AbstractPlant
{
    [Export] private PlantGui _guiManager;

    protected ContainPlant
        MyContainer; //TODO this should be replaced with some new implementation. Only used for testing and simple environmental control

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _guiManager ??= GetChild<PlantGui>(0);
        Console.Write("GuiManger set to: " + _guiManager);

        if (GetParent() is ContainPlant)
            MyContainer = (ContainPlant)GetParent();
        else
            throw new InvalidOperationException($"{this} is not in a ContainPlant object.");

        ConnectPlantToDatabase();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //Console.Write(FrameSum);
        //
        if (IsAlive())
            Tick(delta);
    }

    public override void Tick(double delta)
    {
        FrameSum += delta;
        if (FrameSum < 5.0)
            return;

        //THESE VALUES ARE TESTING CONSTANTS :: SHOULD BE REPLACED WITH SOME SET CONSTANT
        DrawWater(Resources[Rt.Health].Max * .1); //SHOULD BE CONTROLLED BY GENES
        EnergyHp(Resources[Rt.Health].Max *
                 .2); //ENERGY CONSUMPTION IS CONSISTENT BUT CHANGED BY HORMONES (ADDITION REQUIRED)

        FrameSum = 0.0;

        Resources[Rt.H2O].Give(25.0);
        Resources[Rt.Co2].Give(50.0);

        Console.Write($"\nHEALTH REMAINING: {MyResources[Rt.Health].Amount}");
        Console.Write($"\nENERGY REMAINING: {MyResources[Rt.Energy].Amount}");

        IsDeadThenDeadFrame();
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

    protected double DrawWater(double amount)
    {
        if (!MyContainer.HasWater()) return 0;

        return MyContainer.Water.Take(amount);
    }


    protected double EnergyHp(double energyAmount)
    {
        double result = Resources[Rt.Energy].Take(energyAmount);
        Resources[Rt.Health].Take(result);
        //COULD POSSIBLY CHECK IF DEAD AFTERWORDS--
        return result;
    }
}