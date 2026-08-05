using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Main.main._Outside_Building;

namespace Main.main.packages.items;

public partial class Blueprint : Sprite2D
{
    public Area2D Area = new();
    public int DisplayOffset { get; private set; }
    public const int MaxOffset = 30;
    public Vector2 LastPos;

    public bool ValidPlacement = false;

    public Color InvalidPlacementColor = new Color(1, .1f, .1f, .5f);
    public Color ValidPlacementColor = new Color(.0f, .5f, 1, .5f);

    public override void _Ready()
    {
        base._Ready();
        LastPos = GlobalPosition;
        Area.Monitoring = true;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (GlobalPosition != LastPos)
        {
            //TODO this is a really intensive way to accomplish this. Should only check on done moving or something.

            ValidPlacement = true;
            foreach (Area2D area in Area.GetOverlappingAreas())
            {
                if (area.GetParent() is Player player)
                {
                    if (player.GetBase() == area)
                    {
                        ValidPlacement = false;
                        break;
                    }

                    continue; // I mean this is amateur hour work
                }

                if (area is not Boundaries.Boundary boundary)
                {
                    ValidPlacement = false;
                    break;
                }
            }

            if (ValidPlacement)
            {
                Set("modulate", ValidPlacementColor);
            }
            else
            {
                Set("modulate", InvalidPlacementColor);
            }

            LastPos = GlobalPosition;
        }
    }

    public int ChangeDisplayOffset(int change, bool set = false)
    {
        int newValue = set ? change : DisplayOffset + change;

        if (newValue > MaxOffset || newValue < 0)
            return DisplayOffset;
        DisplayOffset = newValue;
        return DisplayOffset;
    }
}