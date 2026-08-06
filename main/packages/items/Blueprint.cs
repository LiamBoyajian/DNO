using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Main.main._Outside_Building;

namespace Main.main.packages.items;

public static class LayerControl
{
    public const int Object = 1;
    public const int Interactable = 2;
    public const int Bounds = 3;
    public const int Placeable = 4;
}

public partial class Blueprint : Sprite2D
{
    public Area2D Area = new();

    public int DisplayOffset { get; private set; } = 15;
    public const int MaxOffset = 30;

    public bool? ValidPlacement { get; private set; } = null; //null when unchecked for pos

    public Color InvalidPlacementColor = new Color(1, .1f, .1f, .5f);
    public Color ValidPlacementColor = new Color(.0f, .5f, 1, .5f);

    public override void _Ready()
    {
        base._Ready();

        SetVisibility(false);
        Name = "blueprint";
        Area.InputPickable = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        //ValidPlacement = CheckPlacement();
    }

    public int ChangeDisplayOffset(int change, bool set = false)
    {
        int newValue = set ? change : DisplayOffset + change;

        if (newValue > MaxOffset || newValue < 0)
            return DisplayOffset;
        DisplayOffset = newValue;
        return DisplayOffset;
    }

    public bool? CheckPlacement()
    {
        if (Visible)
        {
            bool withinBoundary = false;
            ValidPlacement = true;
            foreach (Area2D area in Area.GetOverlappingAreas())
            {
                uint collisionLayer = area.GetCollisionLayer();
                if (collisionLayer == LayerControl.Object)
                {
                    ValidPlacement = false;
                    break;
                }
                else if (collisionLayer == LayerControl.Bounds)
                {
                    withinBoundary = true;
                }
                else if (collisionLayer == LayerControl.Placeable)
                {
                    withinBoundary = true;
                }
            }

            ValidPlacement &= withinBoundary;

            if (ValidPlacement ?? false)
            {
                Set("modulate", ValidPlacementColor);
            }
            else
            {
                Set("modulate", InvalidPlacementColor);
            }
        }

        return ValidPlacement;
    }

    public void SetVisibility(bool? visible = null)
    {
        var collisionShape = (CollisionShape2D)Area.GetChild(0);
        if (visible is null)
        {
            Visible = !Visible;
            Area.Monitoring = !Area.Monitoring;
            Area.Monitorable = !Area.Monitorable;
            collisionShape.Disabled = !collisionShape.Disabled;
        }
        else if ((bool)visible)
        {
            Visible = true;
            Area.Monitoring = true;
            Area.Monitorable = true;
            collisionShape.Disabled = false;
        }
        else
        {
            Visible = false;
            Area.Monitoring = false;
            Area.Monitorable = false;
            collisionShape.Disabled = true;
        }
    }
}