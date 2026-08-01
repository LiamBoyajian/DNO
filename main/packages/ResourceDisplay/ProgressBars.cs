using System;
using System.Collections.Generic;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public partial class ProgressBars : BoxContainer, IResourceDisplay<ProgressBar>
{
    public ButtonGroup Buttons { get; private set; }
    [Export] public PackedScene ProgressBarTemplate { get; set; }
    public static string ClassNamePrefix { get; set; } = "IconProgressBar";

    public override void _Ready()
    {
        base._Ready();
        Buttons = new ButtonGroup();
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

    public bool AddElement((Enum, IMaterialResource) item)
    {
        var item1 = item.Item1;
        var item2 = item.Item2;

        var pendingVBox = new VBoxContainer();
        AddChild(pendingVBox);

        var pendingProgressBar = ProgressBarTemplate.Instantiate() as ProgressBar;
        if (pendingProgressBar == null)
            return false;
        pendingVBox.AddChild(pendingProgressBar);


        pendingProgressBar.MaxValue = item2.Max;
        pendingProgressBar.Value = item2.Amount;
        pendingProgressBar.Name = $"{ClassNamePrefix}{ResourceDisplayTools.Delimiter}{item1.GetType().FullName}";

        var pendingTextureRect = new TextureRect();
        pendingVBox.AddChild(pendingTextureRect);
        pendingTextureRect.ExpandMode = TextureRect.ExpandModeEnum.KeepSize;
        var icon = MemoryToDb.GetTextureFromEntry(new Entry(item1));
        pendingTextureRect.Texture = icon;


        return true;
    }

    public ProgressBar Find(string key)
    {
        return FindChild(ClassNamePrefix + key, true, false) as ProgressBar;
    }

    public ProgressBar Update(string key, IMaterialResource material)
    {
        if (Find(key) is not { } b)
            return null;

        b.MaxValue = material.Max;
        b.Value = material.Amount;

        return b;
    }

    public void UpdateAll(IEnumerable<(string, IMaterialResource)> getMaterialEnumerable)
    {
        foreach (var r in getMaterialEnumerable)
        {
            Update(r.Item1, r.Item2);
        }
    }
}