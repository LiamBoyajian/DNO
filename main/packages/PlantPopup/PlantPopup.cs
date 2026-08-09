using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Main.main.packages.PlantPopup.item_progress_bar;
using Main.main.packages.PlantPopup.item_upgrade;
using Main.main.packages.plants.interfaces;
using Main.main.packages.ResourceDisplay;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;
using ItemProgressBar = Main.main.packages.PlantPopup.item_progress_bar.ItemProgressBar;

namespace Main.main.packages.PlantPopup;

public partial class PlantPopup : Window
{
    public static PlantPopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }

    [Export] public Container ContainerItemUpgradeDisplay;
    [Export] public Container ContainerProgressBarDisplay;
    public IResourceDisplay<ItemUpgrade> ItemUpgradeDisplay;
    public IResourceDisplay<ItemProgressBar> ProgressBarDisplay;

    protected IAttributeDictionary LastAttributeDictionary { get; private set; }

    //private string _purchaseSuffix = "purchase";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (ContainerItemUpgradeDisplay is not IResourceDisplay<ItemUpgrade> itemUpgradeDisplay)
            throw new Exception("ContainerItemUpgradeDisplay is not IResourceDisplay<ItemUpgrade>");
        ItemUpgradeDisplay = itemUpgradeDisplay;

        if (ContainerProgressBarDisplay is not IResourceDisplay<ItemProgressBar> progressBarDisplay)
            throw new Exception("ContainerProgressBarDisplay is not IResourceDisplay<ItemProgressBar>");
        ProgressBarDisplay = progressBarDisplay;


        //---------------
        ItemUpgradeDisplay?.EnumGate = new EnumGate();
        ProgressBarDisplay?.EnumGate = new EnumGate();
        ProgressBarDisplay?.EnumGate.CreateGate(typeof(AbstractPlant.Rt), 0, 3, 4);

        //Default
        Instance = this;
        FocusExited += OnClose;
        OnClose();
    }

    public override void _Process(double delta)
    {
    }

    // EVENT HANDLERS ----------------------------
    private void PurchasePressedHandler(Node node)
    {
        if (node is not ItemUpgrade itemUpgrade) return;
        if (LastAttributeDictionary is not IObtainable obtainable) return;
        if (LastAttributeDictionary is not IConcatEnumerable concatEnumerable) return;
        obtainable.ParseObtain(itemUpgrade.Enum);
        itemUpgrade.SetValueDisplays(obtainable.ObtainCost(itemUpgrade.Enum),
            concatEnumerable.GetIMaterialResource(itemUpgrade.Enum).Amount);
    }

    public void UpgradePressedHandler(Node node)
    {
        if (node is not ItemUpgrade itemUpgrade) return;
        if (LastAttributeDictionary is not IUpgradable upgradable) return;
        if (LastAttributeDictionary is not IConcatEnumerable concatEnumerable) return;
        upgradable.ParseUpgrade(itemUpgrade.Enum);
        itemUpgrade.SetValueDisplays(upgradable.UpgradeCost(itemUpgrade.Enum),
            concatEnumerable.GetIMaterialResource(itemUpgrade.Enum).Amount);
    }

    //IDK

    public void OnClose()
    {
        Hide();
        ClearElements();
    }

    /**
     *  Can display:
     *  IAttributeEnumerable, IMaterialEnumerable
     *
     */
    public void InitializeNode(Node node)
    {
        if (node is IAttributeDictionary u)
        {
            LastAttributeDictionary = u;
            if (LastAttributeDictionary is IBroadcastsUpdate broadcaster)
            {
                broadcaster.Updated += () => Refresh();
            }
        }


        ClearElements();


        if (node is IConcatEnumerable concatEnumerable)
        {
            using var enumerable = concatEnumerable.GetDictionaryConcatEnumerable().GetEnumerator();
            while (enumerable.MoveNext())
            {
                var item1 = enumerable.Current.Item1;
                var item2 = enumerable.Current.Item2;
                if (item1 == null || item2 == null)
                    continue;

                //ItemUpgradeDisplay]
                var newItemUpgradeScene = ItemUpgradeDisplay.Scene.Instantiate();
                if (newItemUpgradeScene is not ItemUpgrade newItemUpgrade)
                    throw new Exception("newItemUpgradeScene is not itemupgrade");
                if (newItemUpgradeScene is IResourceElement resourceElementUpgrade)
                    resourceElementUpgrade.SetEnum(item1);
                double cost = -1;
                if (node is IUpgradable upgradable)
                    cost = (int)Math.Ceiling(upgradable.UpgradeCost(item1));

                newItemUpgrade.SetValueDisplays(cost, item2.Amount);
                newItemUpgrade.UpgradePressed += UpgradePressedHandler;
                newItemUpgrade.PurchasePressed += PurchasePressedHandler;

                ItemUpgradeDisplay.AddElement(newItemUpgrade);


                //ProgressBarDisplay
                var newItemProgressBarScene = ProgressBarDisplay.Scene.Instantiate();
                if (newItemProgressBarScene is not ItemProgressBar newItemProgressBar)
                    throw new Exception("newItemProgressBarScene is not itemprogressBar");
                if (newItemProgressBar is IResourceElement resourceElementProgressbar)
                    resourceElementProgressbar.SetEnum(item1);
                newItemProgressBar.ProgressBar.Value = item2.Amount;
                newItemProgressBar.ProgressBar.MaxValue = item2.Max;
                newItemProgressBar.TooltipText = "" + item2.Amount + " / " + item2.Max;

                ProgressBarDisplay.AddElement(newItemProgressBar);
            }
        }

        Show();
        PopupCentered();
    }

    public void ClearElements()
    {
        ItemUpgradeDisplay.ClearChildren();
        ProgressBarDisplay.ClearChildren();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsAction("ui_filedialog_refresh")) return;
        Refresh();
    }

    public void Refresh(bool fullRefresh = false)
    {
        if (fullRefresh)
        {
            ClearElements();
            InitializeNode(LastAttributeDictionary as Node);
            return;
        }

        if (LastAttributeDictionary is not IConcatEnumerable dictionaryEnumerator)
            return;

        using var enumerable = dictionaryEnumerator.GetDictionaryConcatEnumerable().GetEnumerator();
        while (enumerable.MoveNext())
        {
            var item1 = enumerable.Current.Item1;
            var item2 = enumerable.Current.Item2;
            //Only need to update the values since we know no purchases were made.
            //if (ItemUpgradeDisplay.Get(item1) is not {} itemUpgradeDisplay) GD.PrintErr("Does not contain enum");
            if (ProgressBarDisplay.Get(item1) is not { } itemProgressBar)
            {
                GD.PrintErr("Does not contain enum");
                continue;
            }

            itemProgressBar.ProgressBar.Value = item2.Amount;
            itemProgressBar.ProgressBar.MaxValue = item2.Max;
            itemProgressBar.TooltipText = "" + item2.Amount + " / " + item2.Max;
        }
    }
    //public void UpdateAllOfEnum(Enum @enum, )
    //{
    //    if (ItemUpgradeDisplay.Get(@enum) is not {} itemUpgradeDisplay) GD.PrintErr("Does not contain enum");
    //    if (ProgressBarDisplay.Get(@enum) is not {} itemProgressBar) GD.PrintErr("Does not contain enum");
    //    
    //    
    //    itemProgressBar.ProgressBar.Value = item2.Amount;
    //    itemProgressBar.ProgressBar.MaxValue = item2.Max;
    //    itemProgressBar.TooltipText = "" + item2.Amount + " / " + item2.Max;
    //}
    //
}