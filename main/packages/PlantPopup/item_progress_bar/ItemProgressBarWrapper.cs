using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.ResourceDisplay;

namespace Main.main.packages.PlantPopup.item_progress_bar;

public partial class ItemProgressBarWrapper : HBoxContainer, IResourceDisplay<ItemProgressBar>
{
    [Export] public PackedScene Scene { get; set; }
    public EnumGate EnumGate { get; set; }
}