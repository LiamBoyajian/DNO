using System;
using Godot;

namespace Main.InventoryAssets;

//TODO figure out if this is a terrible way of implementing this
public partial class ItemTexture(Texture2D texture) : GodotObject
{
    public readonly Texture2D Texture = texture;
}

public partial class Item<TItem>(Texture2D texture, TItem data) : ItemTexture(texture)
{
    public readonly TItem Data = data;
}