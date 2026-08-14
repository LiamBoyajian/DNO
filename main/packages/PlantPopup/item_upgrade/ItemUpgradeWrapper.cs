using Godot;
using Main.main.packages.ResourceDisplay;

namespace Main.main.packages.PlantPopup.item_upgrade;

public partial class ItemUpgradeWrapper : FlowContainer, IResourceDisplay<ItemUpgrade>
{
    [Export] public PackedScene Scene { get; set; }
    public EnumGate EnumGate { get; set; }
}