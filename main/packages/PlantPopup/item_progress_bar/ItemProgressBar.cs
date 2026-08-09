using System;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.main.packages.ResourceDisplay;

namespace Main.main.packages.PlantPopup.item_progress_bar;

public partial class ItemProgressBar : BoxContainer, IResourceElement
{
    public Enum Enum { get; set; }
    [Export] public ProgressBar ProgressBar { get; set; }
    [Export] public TextureRect IconTexture { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (ProgressBar == null)
            GD.PrintErr("ItemProgressBar: ProgressBar is null");
        if (IconTexture == null)
            GD.PrintErr("ItemProgressBar: TextureRect is null");
    }

    public void InitializeIcon()
    {
        if (IconTexture.Texture == null)
        {
            IconTexture.Texture = GD.Load<Texture2D>(AccessIconsDb.GetEntry(new Entry(Enum))?.Data);
        }
    }
}