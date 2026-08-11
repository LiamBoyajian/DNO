using System;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.main.packages.ResourceDisplay;
using Main.Source.main;

namespace Main.main.packages.PlantPopup.item_upgrade;

public partial class ItemUpgrade : Panel, IResourceElement
{
    public Enum Enum { get; set; }
    [Export] protected TextureRect IconTexture;
    [Export] protected TextureButton UpgradeButton;
    [Export] protected TextureButton PurchaseButton;
    [Export] protected Label ObtainCost;
    [Export] protected Label UpgradeCost;

    [Signal]
    public delegate void UpgradePressedEventHandler(Node node);

    [Signal]
    public delegate void PurchasePressedEventHandler(Node node);

    //Declaration
    public ItemUpgrade() : this(null)
    {
    }

    public ItemUpgrade(Enum @enum)
    {
        Enum = @enum;
    }
    //Default


    //GODOT
    public override void _Ready()
    {
        base._Ready();

        if (UpgradeButton == null)
            GD.PrintErr("upgradeButton is null");
        if (PurchaseButton == null)
            GD.PrintErr("purchaseButton is null");
        if (ObtainCost == null)
            GD.PrintErr("ObtainCost is null");
        if (UpgradeCost == null)
            GD.PrintErr("UpgradeCost is null");

        UpgradeButton?.Pressed += () => EmitSignal(nameof(UpgradePressed), this);
        PurchaseButton?.Pressed += () => EmitSignal(nameof(PurchasePressed), this);
    }

    public void InitializeIcon()
    {
        if (IconTexture.Texture == null && MemoryToDb.GetTextureFromEntry(new Entry(Enum)) is { } texture2D)
        {
            IconTexture.Texture = texture2D;
        }
    }

    public void SetUpgradeCostDisplay(double upgradeCost, bool roundToInt = true)
    {
        var upgradeCostDisplay = roundToInt ? Math.Ceiling(upgradeCost) : upgradeCost;
        UpgradeCost.Text = "" + (upgradeCostDisplay < 0 ? "XXX" : upgradeCostDisplay);
    }

    public void SetObtainCostDisplay(double obtainCost, bool roundToInt = true)
    {
        var obtainCostDisplay = roundToInt ? Math.Ceiling(obtainCost) : obtainCost;
        ObtainCost.Text = "" + (obtainCostDisplay < 0 ? "XXX" : obtainCostDisplay);
    }
}