using System;
using System.Collections.Generic;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.main.scripts.core.plants;
using Main.Source.main;

namespace Main.main.scripts.core.util;

public partial class ResourceDisplay : Container
{
    [Export] public PackedScene ProgressBarTemplate { get; set; }
    [Export] public PackedScene ButtonTemplate { get; set; }
    [Export] public ButtonGroup Buttons;

    private string _barTag = "Bar_";
    private string _barMaxButtonTag = "MaxBarButton_";
    private string _barAmountButtonTag = "AmountBarButton_";
    private string _infiniteButtonTag = "InfiniteButton_";

    /**
     * Seperates the individual elements
     */
    [Export]
    public PackedScene SubContainer { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Buttons ??= new ButtonGroup();
        Buttons.AllowUnpress = true;


        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;
        if (pB == null)
            System.Diagnostics.Debug.Assert(false, "ProgressBarTemplate is not type ProgressBar");


        var tB = ButtonTemplate.Instantiate() as BaseButton;
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

        var box = SubContainer.Instantiate() as Container;
        AddChild(box);

        var pB = ProgressBarTemplate.Instantiate() as ProgressBar;

        box.AddChild(pB);
        pB.MaxValue = material.Max;
        pB.Value = material.Amount;
        pB.Name = _barTag + key;

        //var bT1 = ButtonTemplate.Instantiate() as BaseButton;
        //box.AddChild(bT1);
        //bT1.Name = _barAmountButtonTag + key;
        ////bT1.Text = $"{key}_{(int)material.Amount}";
        //bT1.ButtonGroup = Buttons;
        //var bT2 = ButtonTemplate.Instantiate() as BaseButton;
        //box.AddChild(bT2);
        //bT2.Name = _barMaxButtonTag + key;
        ////bT2.Text = $"{key}_{(int)material.Max}";
        //bT2.ButtonGroup = Buttons;


        return true;
    }

    public bool DisplayAttributes(string key, double value)
    {
        var box = SubContainer.Instantiate() as Container;
        AddChild(box);

        var bT = ButtonTemplate.Instantiate() as BaseButton;
        box.AddChild(bT);
        bT.Name = _infiniteButtonTag + key;
        //bT.Text = $"{key}_{value}";
        bT.ButtonGroup = Buttons;


        return true;
    }

    /**
     * returns found progressbar; otherwise null
     */
    public ProgressBar GetProgressBar(string key)
    {
        return FindChild(_barTag + key, true, false) as ProgressBar;
    }

    /**
     * returns found "progressbar"-button; otherwise null
     */
    public BaseButton GetProgressBarAmountButton(string key)
    {
        return FindChild(_barAmountButtonTag + key, true, false) as BaseButton;
    }

    /**
     * returns found "progressbar"-button; otherwise null
     */
    public BaseButton GetProgressBarMaxButton(string key)
    {
        return FindChild(_barMaxButtonTag + key, true, false) as BaseButton;
    }

    /**
     * returns found button; otherwise null
     */
    public BaseButton GetButton(string key)
    {
        return FindChild(_infiniteButtonTag + key, true, false) as BaseButton;
    }

    /**
     * Attempts to update a button with this key
     * returns updated button; otherwise null
     */
    public BaseButton UpdateButton(string key, double value)
    {
        if (GetButton(key) is not { } b)
            return null;

        //b.Text = $"{key}_{value}";

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

        if (GetProgressBarAmountButton(key) is not { } bT1)
            return null;
        if (GetProgressBarMaxButton(key) is not { } bT2)
            return null;

        b.MaxValue = material.Max;
        b.Value = material.Amount;

        //bT1.Text = $"{key}_{(int)material.Amount}";
        //bT2.Text = $"{key}_{(int)material.Max}";

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

    public void UpdateMaterialBars(IEnumerable<(string, IMaterialResource)> getMaterialEnumerable)
    {
        foreach (var r in getMaterialEnumerable)
        {
            UpdateBar(r.Item1, r.Item2);
        }
    }

    public void UpdateAttributeButtons(IEnumerable<(string, double)> getAttributeEnumerable)
    {
        foreach (var r in getAttributeEnumerable)
        {
            UpdateButton(r.Item1, r.Item2);
        }
    }
}