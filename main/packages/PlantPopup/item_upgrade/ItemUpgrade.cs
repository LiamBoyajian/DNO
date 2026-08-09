using System;
using Godot;
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
    [Export] protected Label GlucoseCost;
    [Export] protected Label AmountLabel;

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
        if (GlucoseCost == null)
            GD.PrintErr("maxLabel is null");
        if (AmountLabel == null)
            GD.PrintErr("amountLabel is null");

        UpgradeButton?.Pressed += () => EmitSignal(nameof(UpgradePressed), this);
        PurchaseButton?.Pressed += () => EmitSignal(nameof(PurchasePressed), this);
    }

    public void InitializeIcon()
    {
        if (IconTexture.Texture == null)
        {
            IconTexture.Texture = GD.Load<Texture2D>(AccessIconsDb.GetEntry(new Entry(Enum))?.Data);
        }
    }

    public void SetValueDisplays(double cost, double amount)
    {
        GlucoseCost.Text = "" + (cost < 0 ? "XXX" : cost);


        AmountLabel.Text = "" + (amount < 0 ? "XXX" : amount);
    }
}