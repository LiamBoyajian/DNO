using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.items;

namespace Main.main.packages.inventory;

public partial class Inventory(int size = 1) : Node
{
    public Inventory() : this(1)
    {
    }

    //Global self
    public static Inventory Instance { get; private set; }

    //Fields
    private int _money = 0;
    public int SelectedItem { get; private set; } = 0;

    [Export] protected string MoneyLabelUnit = "kr";

    [Export] public Color ColorSelectedIcon = new Color(.5f, .5f, .5f);
    [Export] public Color ColorIcon = new Color(1, 1, 1);


    //References to scene nodes
    public Godot.Collections.Array<InventorySlot> Slots;
    [Export] protected Label MoneyLabel;


    //Model fields


    //Signals
    [Signal]
    public delegate void SelectedItemChangedEventHandler(Node item);

    [Signal]
    public delegate void NoSelectedItemEventHandler();

    //Godot native methods
    public override void _Ready()
    {
        Instance = this;
        Slots = [];
        foreach (var childNode in FindChildren("*"))
        {
            if (childNode is not InventorySlot inventorySlot) continue;
            Slots.Add(inventorySlot);
            inventorySlot.ItemReturned += SlotReturnedItemHandler;
        }
    }

    public override void _Process(double delta)
    {
    }

    //


    //Signals and updates --------------------

    public bool UpdateAll()
    {
        return UpdateMoney();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("inventory_slot") && @event is InputEventKey eventKey)
        {
            var keycodeString = OS.GetKeycodeString(eventKey.PhysicalKeycode);
            SetSelectedItem(Convert.ToInt32(keycodeString) - 1);
        }
    }


    //Getters / Setters -------------------

    public bool SetSelectedItem(int node)
    {
        if (node < 0 || node >= Slots.Count) return false;

        var previousSlot = Slots[Math.Clamp(SelectedItem, 0, Count())];
        previousSlot.SelfModulate = ColorIcon;
        if (previousSlot.HasItem())
        {
            var previousSlotManager = previousSlot.GetManager();
            previousSlotManager?.ReturnItem();
            EmitSignal(nameof(NoSelectedItem));
            if (node == SelectedItem)
            {
                SelectedItem = -1;
                return false;
            }
        }

        SelectedItem = node;
        var currentSlot = Slots[SelectedItem];
        currentSlot.SelfModulate = ColorSelectedIcon;

        IItem signalItem = currentSlot.GetManager()?.BorrowItem();
        signalItem?.Show();

        EmitSignal(nameof(SelectedItemChanged), (Node)signalItem);

        return true;
    }

    public int Count()
    {
        return Slots.Count;
    }

    public void AddMoney(int amount, bool update = false)
    {
        if (amount > 0)
            _money += amount;

        if (update)
            UpdateMoney();
    }

    public int GetMoney()
    {
        return _money;
    }

    public bool UpdateMoney()
    {
        if (MoneyLabel == null)
        {
            GD.PrintErr("Money label is null");
            return false;
        }

        MoneyLabel.Text = GetMoney().ToString();
        return true;
    }

    protected void SlotReturnedItemHandler()
    {
        EmitSignal(nameof(NoSelectedItem));
    }

    //
    public void RequestItemReturn()
    {
        SetSelectedItem(SelectedItem);
    }
}