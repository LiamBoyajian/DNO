using System;
using System.Collections;
using System.Text.RegularExpressions;
using Godot;
using Godot.NativeInterop;
using Main.Package;

namespace Main.InventoryAssets;

public partial class Inventory<[MustBeVariant] TI>(int max) : Node
{
    //Consider replacing with a new object; something like item or something. Maybe not needed...

    protected Godot.Collections.Array<TI> Array = new Godot.Collections.Array<TI>();

    protected int
        MaxItems = max > 0
            ? max
            : throw new ArgumentException(
                "max is < 0"); //create a new inventory to upgrade... or I'll make an addition system im not sure.

    protected TI SwapAtIndex(int index, TI item)
    {
        var result = RemoveItem(index);
        AddItem(index, item);
        return result;
    }

    protected int AddItem(TI item)
    {
        if (EnsureCapacity()) return -1;
        Array.Add(item);
        return Array.IndexOf(item);
    }

    protected int AddItem(int index, TI item)
    {
        if (EnsureCapacity()) return -1;
        Array.Insert(index, item);
        return index;
    }

    protected TI RemoveItem(int index)
    {
        //TODO might be broken I havent tested
        if (Array.Count == 0) throw new ArgumentOutOfRangeException(nameof(index), "Inventory already empty.");
        if (index >= Array.Count)
            throw new IndexOutOfRangeException("Index is greater than the Inventory size or negative.");


        var result = Array[index];
        Array.RemoveAt(index);
        return result;
    }

    /**
     * Returns true if there is no space remaining
     */
    public bool EnsureCapacity()
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

    //public ArrayList ToArrayList()
    //{
    //    return Array.CopyTo();
    //}
}

//------------------------------------------------------------------------------------------
//My personal implementation of inventory : uses itemSprite
public partial class InventoryContainer : Inventory<ItemTexture>
{
    private Container _container;
    private TextureButton _button;
    private ButtonGroup _playerInventoryButtons;
    private ItemTexture _bufferSlot = null; //Basically a free storage slot that also lets outside users interact with it. manipulated based on the clicked button.  

    public InventoryContainer(Container container, int slots, TextureButton button) : base(slots)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container), "Container is null");
        _button = button ?? throw new ArgumentNullException(nameof(button), "button is null");

        _button.Hide();

        this.AddChild(_container);
        this.AddChild(_button);
        _playerInventoryButtons = new ButtonGroup();
        _playerInventoryButtons.SetName("playerInventoryButtons");
        _playerInventoryButtons.Pressed += OnGroupButtonPressed;
        //this.AddChild(_buttonGroup);
        //_playerInventoryButtons.
    }

    [Signal]
    public delegate void UpdatedBufferSlotEventHandler();

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

            Sprite2D spriteHolder = new Sprite2D();
            spriteHolder.Name = "SpriteHolder";
            spriteHolder.Position = new Vector2(_button.GetTextureNormal().GetSize().X / 2,
                _button.GetTextureNormal().GetSize().Y / 2); //center it
            textureButton.AddChild(spriteHolder);

            textureButton.ButtonGroup = _playerInventoryButtons;
        }

        UpdateItemsDisplay();
        return result;
    }

    private void OnGroupButtonPressed(BaseButton button)
    {
        LoadBufferSlot();
    }

    public void Show()
    {
        _container.Visible = true;
    }

    public void Hide()
    {
        _container.Visible = false;

        //No Space
        if (EnsureCapacity())
        {
            //leave the buffer full but the currently held item should immediately refresh on inventory open
        }
        //Return to previous slot or dump into open slot
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

    public void UpdateItemsDisplay()
    {
        for (int i = 0; i < MaxItems && i < Array.Count; i++)
        {
            ItemTexture temp = Array[i] as ItemTexture;
            //if (temp == null ) continue;

            Sprite2D currentItemSprite = _container.GetChild(i).GetNode<Sprite2D>("SpriteHolder");

            if (currentItemSprite == null) throw new Exception("SpriteHolder is null");
            if (temp == null || temp.Texture == null)
            {
                //nothing in this slot 
                currentItemSprite.SetTexture(null);
            }
            else
            {
                //something in this slot
                currentItemSprite.SetTexture(temp.Texture);
            }
        }

        return;
    }


    /**
     * This refreshes the slot
     */
    protected ItemTexture LoadBufferSlot()
    {
        if (GetPressedButton() is null)
            return null;
        _bufferSlot = SwapAtIndex(GetPressedButton().GetIndex(), _bufferSlot);
        UpdateItemsDisplay(); //Visual update
        EmitSignal(SignalName.UpdatedBufferSlot);
        return _bufferSlot;
    }

    public ItemTexture GetBufferSlot()
    {
        return _bufferSlot;
    }

    /**
     * Returns a references to the ItemTexture in bufferslot and removes bufferslot's own reference
     */
    public ItemTexture TakeBufferItem(ItemTexture item)
    {
        ItemTexture result = GetBufferSlot();
        _bufferSlot = item;//nullable
        return result;
    }
}