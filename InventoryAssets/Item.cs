using System;
using Godot;

namespace Main.InventoryAssets;

public partial class Item<TItem>(Sprite2D sprite, TItem data) : Node
{
    public readonly Sprite2D Sprite = sprite;
    public readonly TItem Data = data;
}