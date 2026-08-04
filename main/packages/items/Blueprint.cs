using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace Main.main.packages.items;

public partial class Blueprint : TextureRect
{
    public Area2D Area = new();
    public int DisplayOffset = 10;
    public Vector2 lastPos;

    public override void _Ready()
    {
        base._Ready();
        lastPos = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GlobalPosition != lastPos)
        {
            if (Area.GetOverlappingAreas().Count > 0 &&
                !(Area.GetOverlappingAreas().Count == 1 && String.Compare(Area.GetOverlappingAreas()[0].Name.ToString(),
                    "Boundaries", StringComparison.Ordinal) == 0))
            {
                Set("modulate", new Color(1, .1f, .1f));
            }
            else
            {
                Set("modulate", new Color(1, 1, 1));
            }

            lastPos = GlobalPosition;
        }
    }
}