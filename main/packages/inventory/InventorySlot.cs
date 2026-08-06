using Godot;
using Main.main.packages.items;

namespace Main.main.packages.inventory;

public partial class InventorySlot : Panel
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return GetChildCount() == 0;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        if (data.AsGodotObject() is IItem item)
        {
            item.Reparent(this);
            item.Initialize();
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (GetChildCount() == 0)
        {
            return default;
        }

        Node childNode = GetChild<Node>(0);
        if (childNode == null)
        {
            return default;
        }

        var previewNode = childNode.Duplicate();
        if (previewNode is IItem item)
        {
            using var textureRectPreview = new TextureRect();
            textureRectPreview.Texture = item.DragIcon;
            SetDragPreview(textureRectPreview);
        }
        else
        {
            GD.PrintErr("Node does not implement IItem");
        }

        // Return the actual node as the drag data Variant
        return Variant.From(childNode);
    }

    public Node GetItem()
    {
        if (GetChildCount() == 0) return null;
        return GetChild<Node>(0);
    }
}