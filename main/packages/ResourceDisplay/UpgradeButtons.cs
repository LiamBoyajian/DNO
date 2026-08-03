using System;
using System.Collections.Generic;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public partial class UpgradeButtons : FlowContainer, IResourceDisplay<TextureButton>
{
    public ButtonGroup Buttons { get; private set; }
    [Export] public PackedScene TextureButtonTemplate { get; set; }
    public static string ClassNamePrefix { get; set; } = "UpgradeButton";

    public override void _Ready()
    {
        base._Ready();
        Buttons = new ButtonGroup();
        MouseFilter = MouseFilterEnum.Pass;
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

    public bool AddElement((Enum, IMaterialResource) item, string suffix = "")
    {
        var item1 = item.Item1;
        var item2 = item.Item2;

        //var pendingVBox = new VBoxContainer();
        //AddChild(pendingVBox);

        var pendingTextureButton = TextureButtonTemplate.Instantiate() as TextureButton;
        if (pendingTextureButton == null)
            return false;
        AddChild(pendingTextureButton);


        pendingTextureButton.StretchMode = TextureButton.StretchModeEnum.Scale;
        var icon = MemoryToDb.GetTextureFromEntry(new Entry(item1));
        if (icon != null)
            pendingTextureButton.SetTextureNormal(icon);
        pendingTextureButton.Name =
            $"{ClassNamePrefix}{ResourceDisplayTools.Delimiter}{item1.GetType().FullName}{ResourceDisplayTools.Delimiter}{suffix}";
        pendingTextureButton.TooltipText = $"{item2.Amount}/{item2.Max}";

        pendingTextureButton.ButtonGroup = Buttons;

        return true;
    }

    /**
     * returns found progressbar; otherwise null
     */
    public TextureButton Find(string key)
    {
        return FindChild(ClassNamePrefix + key, true, false) as TextureButton;
    }

    /**
     * Attempts to update a progressbar with this key
     * returns updated progressbar; otherwise null
     */
    public TextureButton Update(string key, IMaterialResource material)
    {
        if (Find(key) is not { } b)
            return null;

        b.TooltipText = $"{material.Amount}/{material.Max}";

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