using System;
using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class Pc : AbstractMachine
{
    Pc()
    {
        Inventory = new InventoryContainer();
    }


    public override void _Ready()
    {
        Sprite = this.Get("AnimatedSprite2D").As<AnimatedSprite2D>();
        Console.WriteLine("My sprite: " + Sprite);
    }
}