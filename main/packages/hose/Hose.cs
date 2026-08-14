using System;
using Godot;
using Main.Source.main;

namespace Main.main.packages.hose;

public partial class Hose : AnimatedSprite2D
{
    protected MaterialResource Water = new MaterialResource(1000, 1000);
    protected int OutputSpeed = 100;
    protected int InputSpeed = 100;

    public override void _Ready()
    {
        base._Ready();
    }

    private double _deltaSum = 0;
    private double _tickSpeed = 5;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _deltaSum += delta;
        if (_deltaSum < _tickSpeed) return;
        _deltaSum = 0;

        if (Water.IsMaxed())
        {
            Animation = "drip";
        }
        else
        {
            Animation = "default";
            Water.Give(InputSpeed);
        }

        GD.Print(Water.Amount);
    }

    public double OutputWater()
    {
        GD.Print(Water.Amount);
        return Water.Take(OutputSpeed);
    }
}