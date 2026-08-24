using System;
using Godot;
using Main.main.packages.chatbubble;
using Main.main.packages.inventory;
using Main.main.packages.items;
using Main.main.packages.util;

namespace Main.main.packages.Seller;

public class TradeOffer(string moneyUnit = "cr", float price = 0, ManagerIItem item = null)
{
    public string MoneyUnit { get; private set; } = "";
    public float Price { get; private set; } = 1;

    public ManagerIItem ItemManager { get; private set; } = item;

    public override string ToString()
    {
        return Price + MoneyUnit;
    }

    public event Action OnPurchaseMade;
}

public partial class Seller : AnimatedSprite2D
{
    [Export] protected PackedScene ChatBubbleScene;
    public TradeOffer CurrentOffer;

    [Export] protected Godot.Collections.Array<PackedScene> ItemInstances = new();

    public override void _Ready()
    {
        base._Ready();

        if (ItemInstances.Count < 0) throw new Exception("No item instances");

        GenerateOffer();
    }

    public void GenerateOffer()
    {
        var currentItem = ItemInstances.PickRandom().Instantiate();
        if (currentItem is not IItem item) throw new Exception("ItemScene is not IItem");


        CurrentOffer = new TradeOffer("kr", 5, ManagerIItem.From(item));

        var node = ChatBubbleScene.Instantiate();
        if (node is ChatBubble chatBubble)
        {
            chatBubble.SetText(CurrentOffer.ToString());
            chatBubble.Modulate = new Color(180, 180, 255);
        }

        AddChild(node);
    }

    public void DisplayTradeOffer()
    {
    }

    public void Purchased()
    {
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        //base._UnhandledInput(@event);
        if (@event.IsActionPressed("accept"))
        {
            if (CurrentOffer != null)
            {
                if (Inventory.Instance.Purchase((int)CurrentOffer.Price))
                {
                    //if (CurrentOffer.Item is not IItem) throw new Exception("Gay man here");
                    Inventory.Instance.AddItem(CurrentOffer.ItemManager);
                    CurrentOffer = null;
                }
            }
        }
    }
}