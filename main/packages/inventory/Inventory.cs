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
    public Godot.Collections.Array<Panel> Slots;
    [Export] protected Label MoneyLabel;


    //Model fields


    //Signals
    [Signal]
    public delegate void SelectedItemChangedEventHandler(Node item, int index);

    //Godot native methods
    public override void _Ready()
    {
        Instance = this;
        Slots = [];
        foreach (var childNode in FindChildren("*"))
        {
            if (childNode is not InventorySlot inventorySlot) continue;
            Slots.Add(inventorySlot);
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

        Slots[Math.Clamp(SelectedItem, 0, Count())].SelfModulate = ColorIcon;
        if (node == SelectedItem)
        {
            EmitSignal(nameof(SelectedItemChanged), (Node)null, -1);
            SelectedItem = -1;
            return false;
        }

        SelectedItem = node;
        Slots[SelectedItem].SelfModulate = ColorSelectedIcon;

        if (Slots[SelectedItem] is InventorySlot inventorySlot)
        {
            EmitSignal(nameof(SelectedItemChanged), inventorySlot.GetItem(), SelectedItem);
        }

        return true;
    }

    public bool ReturnItem(Node item, int index, bool anySlot = true)
    {
        if (item == null) return true;
        if (item is not IItem) return false;
        if (index < 0 || index >= Slots.Count) return false;
        var target = Slots[index];

        if (target.GetChildCount() == 0)
        {
            item.Reparent(target);
            return true;
        }

        if (anySlot)
        {
            foreach (var slot in Slots)
            {
                if (slot.GetChildCount() == 0)
                {
                    item.Reparent(slot);
                    return true;
                }
            }
        }

        return false;
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

    //
}