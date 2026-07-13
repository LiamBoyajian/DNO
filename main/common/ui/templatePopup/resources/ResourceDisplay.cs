using System;
using System.Collections.Generic;
using Godot;
using Main.main.scripts.core.plants;
using Main.Source.main;

namespace Main.main.common.ui.templatePopup.resources;

public partial class ResourceDisplay : HBoxContainer
{
    [Export] public PackedScene ProgressBarTemplate { get; set; }
    [Export] public PackedScene ButtonTemplate { get; set; }
    [Export] protected ButtonGroup Buttons { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;
        if (pB == null)
        {
            System.Diagnostics.Debug.Assert(false, "ProgressBarTemplate is not type ProgressBar");
        }

        var tB = ButtonTemplate.Instantiate() as Button;
        if (pB == null)
        {
            System.Diagnostics.Debug.Assert(false, "ButtonTemplate is not type ProgressBar");
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void CreateMaterialBars(Dictionary<AbstractPlant.Rt, MaterialResource> resources)
    {
        foreach (var r in resources)
        {
            DisplayMaterialResource(r.Key, r.Value);
        }
    }

    public bool DisplayMaterialResource(AbstractPlant.Rt key, MaterialResource material)
    {
        if (material == null)
            return false;

        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;
        AddChild(pB);
        pB.MaxValue = material.Max;
        pB.Value = material.Amount;
        pB.Name = $"Bar:{key}";

        var bT = ButtonTemplate.Instantiate() as Button;
        AddChild(bT);
        bT.Name = $"BarButton:{key}";
        bT.ButtonGroup = Buttons;

        return true;
    }

    public bool DisplayAttributes(Enum key, double value)
    {
        var bT = ButtonTemplate.Instantiate() as Button;
        bT.Name = $"Button:{key}";
        bT.Text = $"{key}:{value}";
        bT.ButtonGroup = Buttons;


        return true;
    }

    public ProgressBar GetProgressBar(AbstractPlant.Rt key)
    {
        return FindChild($"Bar:{key}", false) as ProgressBar;
    }

    public ProgressBar GetProgressBarButton(AbstractPlant.Rt key)
    {
        return FindChild($"BarButton:{key}", false) as ProgressBar;
    }

    /**
     * returns found button; otherwise null
     */
    public Button GetButton(Enum key)
    {
        return FindChild($"Button:{key}", false) as Button; //TODO
    }

    public Button UpdateButton(Enum key, double value)
    {
        if (GetButton(key) is not { } b)
            return null;

        b.Text = $"{key}:{value}";

        return b;
    }

    public ProgressBar UpdateBar(AbstractPlant.Rt key, MaterialResource material)
    {
        if (GetProgressBar(key) is not { } b)
            return null;

        b.MaxValue = material.Max;
        b.Value = material.Amount;

        return b;
    }
}