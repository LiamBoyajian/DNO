using System;
using System.Collections;
using Godot;
using Godot.NativeInterop;
using Main.Package;

namespace Main.InventoryAssets;

public partial class Inventory(int max) : Node
{
    //Consider replacing with a new object; something like item or something. Maybe not needed...
    protected ArrayList Array = new ArrayList();

    protected int
        MaxItems = max > 0
            ? max
            : throw new ArgumentException(
                "max is < 0"); //create a new inventory to upgrade... or I'll make an addition system im not sure.

    public Item<TRemove> SwapAtIndex<TI, TRemove>(int index, Item<TI> item)
    {
        var result = RemoveItem(index);
        AddItem(index, item);
        return (Item<TRemove>)result;
    }

    public int AddItem<TI>(Item<TI> item)
    {
        if (_ensureCapacity()) return -1;
        return Array.Add(item);
    }

    public int AddItem<TI>(int index, Item<TI> item)
    {
        if (_ensureCapacity()) return -1;
        Array.Insert(index, item);
        return index;
    }

    public ItemSprite RemoveItem(int index)
    {
        if (Array.Count == 0) throw new ArgumentOutOfRangeException(nameof(index), "Inventory already empty.");
        if (index >= Array.Count)
            throw new IndexOutOfRangeException("Index is greater than the Inventory size or negative.");
        var result = (ItemSprite)Array[index];
        Array.RemoveAt(index);
        return result;
    }

    private bool _ensureCapacity()
    {
        return Array.Count >= MaxItems;
    }

    public int Search()
    {
        return -1; //unimplemented
    }

    public int Sort()
    {
        return -1; //unimplemented
    }

    public int Count()
    {
        return Array.Count;
    }

    public ArrayList ToArrayList()
    {
        return Array.Clone() as ArrayList;
    }
}

//My personal implementation
public partial class InventoryContainer : Inventory
{
    private Container _container;
    private TextureButton _button;
    private ButtonGroup _playerInventoryButtons;

    public InventoryContainer(Container container, int slots, TextureButton button) : base(slots)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container), "Container is null");
        _button = button ?? throw new ArgumentNullException(nameof(button), "button is null");

        _button.Hide();

        this.AddChild(_container);
        this.AddChild(_button);
        _playerInventoryButtons = new ButtonGroup();
        _playerInventoryButtons.SetName("playerInventoryButtons");
        //this.AddChild(_buttonGroup);
        //_playerInventoryButtons.
    }


    //Required by Godot
    public InventoryContainer() : this((new Container()), 1, new TextureButton())
    {
    }

    public int GenNodeGrid(Vector2 slotSize)
    {
        //NEEDS to use containers.............
        _button.Show();
        var result = CricketVisuals.GenerateNodeGrid(_button,
            slotSize, 1, MaxItems, _container,
            _container.Size, new int?(0));
        _button.Hide();

        foreach (var node in _container.GetChildren())
        {
            if (node is not (TextureButton))
                throw new ArrayTypeMismatchException();

            TextureButton textureButton = node as TextureButton;
            textureButton.ButtonGroup = _playerInventoryButtons;
        }

        return result;
    }

    public void Show()
    {
        _container.Visible = true;
        DisplayItems();
    }

    public void Hide()
    {
        _container.Visible = false;
    }

    public void ToggleVisible()
    {
        //if (_container.Visible) 
        if (_container.Visible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public BaseButton GetPressedButton()
    {
        return _playerInventoryButtons.GetPressedButton();
    }

    public void ClearPressedButtons()
    {
        if (_playerInventoryButtons.GetPressedButton() == null) return;
        _playerInventoryButtons.GetPressedButton().SetPressed(false);
    }

    private void DisplayItems()
    {
        for (int i = 0; i < MaxItems; i++)
        {
            ItemSprite temp = (ItemSprite)base.Array[i];
            if (temp == null) continue;
            Sprite2D currentSprite = new Sprite2D();
            currentSprite.Texture = temp.Sprite.Texture;
            currentSprite.Show();
            _container.GetChild(i).AddChild(currentSprite); //TODO not good
        }

        return;
    }
}