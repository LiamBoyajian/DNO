using System;
using Godot;

namespace Main.InventoryAssets;

//TODO figure out if this is a terrible way of implementing this
public partial class ItemSprite(Sprite2D sprite)
{
    public readonly Sprite2D Sprite = sprite;
}

public class Item<TItem>(Sprite2D sprite, TItem data) : ItemSprite(sprite)
{
    public readonly TItem Data = data;
}