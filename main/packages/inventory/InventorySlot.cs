using System;
using Godot;
using Main.main.packages.items;

namespace Main.main.packages.inventory;

public partial class InventorySlot : Panel
{
    [Signal]
    public delegate void ItemReturnedEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        if (GetChildCount() > 0)
        {
            var child = GetChild<Node>(0);
            var wrapper = ManagerIItem.From(child);
            if (wrapper == null) throw new Exception("Child is not valid item");
            AddChild(wrapper);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return GetChildCount() <= 1;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var dataAsObject = data.AsGodotObject();
        var manager = dataAsObject as ManagerIItem;
        if (dataAsObject is IItem item)
        {
            //AddItem(dataAsObject);
            manager = ManagerIItem.From(item);
        }

        if (manager != null)
        {
            if (GetChildCount() >= 1 && HasItem())
            {
                var currentChild = GetManager();
                currentChild?.Reparent(manager.GetParent());
                currentChild?.Initialize();
            }

            manager.Reparent(this);
            manager.Initialize();
        }
    }

    public void AddItem(ManagerIItem manager)
    {
        if (manager != null)
        {
            if (GetChildCount() >= 1 && HasItem())
            {
                var currentChild = GetManager();
                currentChild?.Reparent(manager.GetParent());
                currentChild?.Initialize();
            }
            else
            {
                if (manager.GetParent() != null)
                {
                    manager.Reparent(this);
                }
                else
                {
                    AddChild(manager);
                }
            }

            manager.Initialize();
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        if (GetChildCount() == 0)
        {
            return default;
        }

        if (GetChild<Node>(0) is not ManagerIItem managerIItem)
        {
            return default;
        }


        if (managerIItem.ReturnItem())
            EmitSignal(nameof(ItemReturned));


        using var textureRectPreview = new TextureRect();
        textureRectPreview.Texture = managerIItem.GetItem().DragIcon;
        SetDragPreview(textureRectPreview);
        return Variant.From(managerIItem);
    }

    public ManagerIItem GetManager()
    {
        if (GetChildCount() == 0) return null;
        return GetChild<Node>(0) as ManagerIItem;
    }

    public bool HasItem()
    {
        return GetManager() != null;
    }
}