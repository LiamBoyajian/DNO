using Godot;

namespace Main.main.scripts.core.util.inventory;

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
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dragNode = data.AsGodotObject() as Node2D;
        if (dragNode != null)
        {
            dragNode.Reparent(this);
            dragNode.Position = Vector2.Zero;
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (GetChildCount() == 0)
        {
            return default; // Equivalent to returning null/void in C# Variant
        }

        Node temp = GetChild(0);
        if (temp == null)
        {
            return default;
        }

        // Duplicate the child node to act as the visual drag preview
        var previewNode = temp.Duplicate();
        if (previewNode is Control previewControl)
        {
            SetDragPreview(previewControl);
        }
        else if (previewNode is Node2D previewNode2D)
        {
            // If the child is a Node2D, wrap it in a Control so SetDragPreview accepts it cleanly
            var previewWrapper = new Control();
            previewWrapper.AddChild(previewNode2D);
            SetDragPreview(previewWrapper);
        }

        // Return the actual node as the drag data Variant
        return Variant.From(temp);
    }
}