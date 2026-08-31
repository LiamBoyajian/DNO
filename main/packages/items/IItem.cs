using System;
using Godot;

namespace Main.main.packages.items;

public partial interface IItem
{
    public Vector2 Position { get; set; }
    public Texture2D DragIcon { get; }
    public Texture2D Icon { get; }

    public Texture2D HeldIcon { get; }

    public void Hide();
    public void Show();

    public bool Use(Node target = null);


    public void Reparent(Node newParent, bool keepGlobalTransform = false);
}

public interface IDeployable
{
    public bool Deploy(Blueprint blueprint);

    public Blueprint GetBlueprint();

    public bool CanCarry();

    public void Collisions(bool enable);
    //TODO ensure these are each needed
}

public partial class WrapperIItem : TextureRect
{
    //Static methods
    public static WrapperIItem From(IItem item)
    {
        return new WrapperIItem(item);
    }

    public static WrapperIItem From(Node node)
    {
        WrapperIItem result = node as WrapperIItem;
        if (node is IItem item && result == null)
        {
            result = WrapperIItem.From(item);
        }

        return result;
    }

    //Constructors
    public WrapperIItem()
    {
    }

    public WrapperIItem(IItem item)
    {
        if (item is not Node node) throw new Exception("Item is not Node");

        if (node.GetParent() == null)
        {
            AddChild(node);
        }
        else
        {
            node.Reparent(this);
        }

        Texture = item.Icon;
        item.Hide();
    }

    // Default
    public bool HasIItem()
    {
        return GetChildCount() > 0;
    }

    /**
     * Unsafe reference
     */
    public IItem GetItem()
    {
        if (GetChildCount() == 0) return null;
        return GetChild(0) as IItem;
    }

    /**
     * Withdraw item, queuefree() this object
     */
    public void Decompose(out IItem item)
    {
        var result = GetItem();
        result.Show();
        RemoveChild(result as Node);
        item = result;
        QueueFree();
    }

    public void Initialize()
    {
        LayoutMode = 1;
        AnchorsPreset = (int)LayoutPreset.Center;
        Show();
    }

    public void UpdateTexture()
    {
        Texture = GetItem()?.Icon ?? Texture;
    }
}

public partial class ManagerIItem(IItem item) : WrapperIItem(item)
{
    //Static
    public new static ManagerIItem From(IItem item)
    {
        return new ManagerIItem(item);
    }

    public new static ManagerIItem From(Node node)
    {
        ManagerIItem result = node as ManagerIItem;
        if (node is IItem item && result == null)
        {
            result = ManagerIItem.From(item);
        }

        return result;
    }


    private IItem _item = item;

    /**
     * Stores a reference to the item
     * Returns the item reference without a parent
     */
    public IItem BorrowItem()
    {
        var result = GetItem();
        RemoveChild(result as Node);
        return result;
    }

    /**
     * If this object has a valid iitem reference,
     * reparent to this object.
     */
    public bool HasItemLink()
    {
        return _item != null;
    }

    public bool ReturnItem()
    {
        if (_item == null) return false;
        if (_item is not Node n) return false;
        if (n.GetParent() == null)
        {
            AddChild(n);
        }
        else
        {
            n.Reparent(this);
        }

        return true;
    }
}