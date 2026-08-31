using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Main.main.packages.PlantPopup.item_progress_bar;
using Main.main.packages.PlantPopup.item_upgrade;
using Main.main.packages.plants.enums;
using Main.main.packages.plants.interfaces;
using Main.main.packages.plants.species;
using Main.main.packages.ResourceDisplay;
using Main.Source.main;
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
        ProgressBarDisplay?.EnumGate.CreateGate(typeof(EnumLibrary.Rt), 0, 1, 2);
        ProgressBarDisplay?.EnumGate.CreateGate(typeof(EnumLibrary.BasicOrgans));

        ItemUpgradeDisplay?.EnumGate.CreateGate(typeof(EnumLibrary.BasicOrgans));

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
        Refresh();
        //UpdateAllOfEnum(itemUpgrade.Enum);
    }

    public void UpgradePressedHandler(Node node)
    {
        if (node is not ItemUpgrade itemUpgrade) return;
        if (LastAttributeDictionary is not IUpgradable upgradable) return;
        if (LastAttributeDictionary is not IConcatEnumerable concatEnumerable) return;
        upgradable.ParseUpgrade(itemUpgrade.Enum);
        Refresh();
        //UpdateAllOfEnum(itemUpgrade.Enum);
    }

    //IDK

    public void OnClose()
    {
        Hide();
        //ClearElements();
    }

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


        if (node is IConcatEnumerable concatEnumerable)
        {
            if (!AllEnumsRepresented(concatEnumerable))
                CreateElements(concatEnumerable);
        }

        Show();
        PopupCentered();
    }

    public void CreateElements(IConcatEnumerable concatEnumerable)
    {
        ClearElements();
        using var enumerable = concatEnumerable.GetDictionaryConcatEnumerable().GetEnumerator();
        while (enumerable.MoveNext())
        {
            var item1 = enumerable.Current.Item1;
            var item2 = enumerable.Current.Item2;
            if (item1 == null || item2 == null)
                continue;

            //These can be separated into their own methods

            //ItemUpgradeDisplay]
            if (ItemUpgradeDisplay.EnumGate.Permits(item1))
            {
                var newItemUpgradeScene = ItemUpgradeDisplay.Scene.Instantiate();
                if (newItemUpgradeScene is not ItemUpgrade newItemUpgrade)
                    throw new Exception("newItemUpgradeScene is not itemupgrade");
                if (newItemUpgradeScene is IResourceElement resourceElementUpgrade)
                    resourceElementUpgrade.SetEnum(item1);
                double upgradeCost = -1;
                double obtainCost = -1;
                if (concatEnumerable is IUpgradable upgradable)
                    upgradeCost = upgradable.UpgradeCost(item1);
                if (concatEnumerable is IObtainable obtainable)
                    obtainCost = obtainable.ObtainCost(item1);

                newItemUpgrade.SetObtainCostDisplay(obtainCost);
                newItemUpgrade.SetObtainCostDisplay(upgradeCost);
                newItemUpgrade.UpgradePressed += UpgradePressedHandler;
                newItemUpgrade.PurchasePressed += PurchasePressedHandler;

                ItemUpgradeDisplay.AddElement(newItemUpgrade);
            }

            //ProgressBarDisplay
            if (ProgressBarDisplay.EnumGate.Permits(item1))
            {
                var newItemProgressBarScene = ProgressBarDisplay.Scene.Instantiate();
                if (newItemProgressBarScene is not ItemProgressBar newItemProgressBar)
                    throw new Exception("newItemProgressBarScene is not itemprogressBar");
                if (newItemProgressBar is IResourceElement resourceElementProgressbar)
                    resourceElementProgressbar.SetEnum(item1);

                ProgressBarDisplay.AddElement(newItemProgressBar);
            }

            UpdateAllOfEnum(item1, item2);
        }
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
            UpdateAllOfEnum(enumerable.Current.Item1, enumerable.Current.Item2);
        }
    }

    public void UpdateAllOfEnum(Enum @enum, IMaterialResource resource = null)
    {
        if (LastAttributeDictionary is not IConcatEnumerable dictionaryEnumerator)
            return;

        var progressbar = ProgressBarDisplay.Get(@enum);
        var itemUpgrade = ItemUpgradeDisplay.Get(@enum);
        var resourceMaterial = resource ?? dictionaryEnumerator.GetIMaterialResource(@enum);

        if (itemUpgrade != null)
        {
            if (LastAttributeDictionary is IUpgradable upgradable)
                itemUpgrade.SetUpgradeCostDisplay(upgradable.UpgradeCost(itemUpgrade.Enum));

            if (LastAttributeDictionary is IObtainable obtainable)
                itemUpgrade.SetObtainCostDisplay(obtainable.ObtainCost(itemUpgrade.Enum));
        }

        if (progressbar != null)
        {
            progressbar.ProgressBar.Value = resourceMaterial.Amount;
            progressbar.ProgressBar.MaxValue = resourceMaterial.Max;
            progressbar.TooltipText = "" + (int)resourceMaterial.Amount + " / " + (int)resourceMaterial.Max;
        }
    }

    public bool AllEnumsRepresented(IConcatEnumerable concatEnumerable)
    {
        foreach (var item in concatEnumerable.GetDictionaryConcatEnumerable())
        {
            Enum @enum = item.Item1;
            if (ItemUpgradeDisplay.EnumGate.Permits(@enum) && !ItemUpgradeDisplay.Contains(@enum)) return false;
            if (ProgressBarDisplay.EnumGate.Permits(@enum) && !ProgressBarDisplay.Contains(@enum)) return false;
        }

        Refresh();
        return true;
    }
}