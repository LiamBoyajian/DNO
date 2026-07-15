using System.Collections.Generic;
using Godot;
using Main.Source.main;

namespace Main.main.scripts.core.util;

public partial class ResourceDisplay : BoxContainer
{
    [Export] public PackedScene ProgressBarTemplate { get; set; }
    [Export] public PackedScene ButtonTemplate { get; set; }
    [Export] public ButtonGroup Buttons;

    /**
     * Seperates the individual elements
     */
    [Export]
    public PackedScene BoxContainer { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Buttons ??= new ButtonGroup();
        Buttons.AllowUnpress = true;


        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;
        if (pB == null)
            System.Diagnostics.Debug.Assert(false, "ProgressBarTemplate is not type ProgressBar");


        var tB = ButtonTemplate.Instantiate() as Button;
        if (tB == null)
            System.Diagnostics.Debug.Assert(false, "ButtonTemplate is not type ProgressBar");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void CreateMaterialBars(IEnumerable<(string, IMaterialResource)> resources)
    {
        foreach (var r in resources)
        {
            DisplayMaterialResource(r.Item1, r.Item2);
        }
    }

    public void CreateAttributeButtons(IEnumerable<(string, double)> resources)
    {
        foreach (var r in resources)
        {
            DisplayAttributes(r.Item1, r.Item2);
        }
    }

    public bool DisplayMaterialResource(string key, IMaterialResource material)
    {
        if (material == null)
            return false;

        var box = BoxContainer.Instantiate() as BoxContainer;
        AddChild(box);

        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;

        box.AddChild(pB);
        pB.MaxValue = material.Max;
        pB.Value = material.Amount;
        pB.Name = $"Bar_{key}";

        var bT = ButtonTemplate.Instantiate() as Button;
        box.AddChild(bT);
        bT.Name = $"BarButton_{key}";
        bT.Text = $"{key}_{material.Amount}";
        bT.ButtonGroup = Buttons;


        return true;
    }

    public bool DisplayAttributes(string key, double value)
    {
        var box = BoxContainer.Instantiate() as BoxContainer;
        AddChild(box);

        var bT = ButtonTemplate.Instantiate() as Button;
        box.AddChild(bT);
        bT.Name = $"Button_{key}";
        bT.Text = $"{key}_{value}";
        bT.ButtonGroup = Buttons;


        return true;
    }

    /**
     * returns found progressbar; otherwise null
     */
    public ProgressBar GetProgressBar(string key)
    {
        return FindChild($"Bar_{key}", false) as ProgressBar;
    }

    /**
     * returns found "progressbar"-button; otherwise null
     */
    public ProgressBar GetProgressBarButton(string key)
    {
        return FindChild($"BarButton_{key}", false) as ProgressBar;
    }

    /**
     * returns found button; otherwise null
     */
    public Button GetButton(string key)
    {
        return FindChild($"Button_{key}", false) as Button;
    }

    /**
     * Attempts to update a button with this key
     * returns updated button; otherwise null
     */
    public Button UpdateButton(string key, double value)
    {
        if (GetButton(key) is not { } b)
            return null;

        b.Text = $"{key}_{value}";

        return b;
    }

    /**
     * Attempts to update a progressbar with this key
     * returns updated progressbar; otherwise null
     */
    public ProgressBar UpdateBar(string key, IMaterialResource material)
    {
        if (GetProgressBar(key) is not { } b)
            return null;

        b.MaxValue = material.Max;
        b.Value = material.Amount;

        return b;
    }

    public bool ClearChildren()
    {
        if (GetChildren().Count <= 0)
            return false;

        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        return true;
    }
}