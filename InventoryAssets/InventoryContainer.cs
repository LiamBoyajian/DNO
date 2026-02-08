using System;
using System.Collections;
using System.Text.RegularExpressions;
using Godot;
using Godot.NativeInterop;
using Main.Package;

namespace Main.InventoryAssets;

public partial class Inventory<[MustBeVariant] TI>(int max) : Container
{
    //Consider replacing with a new object; something like item or something. Maybe not needed...

    protected TI[] Array = new TI[max];
    protected int _count = 0;

    protected int
        MaxItems = max > 0
            ? max
            : throw new ArgumentException(
                "max is < 0"); //create a new inventory to upgrade... or I'll make an addition system im not sure.

    protected bool Place(int index, TI item)
    {
        if (index >= MaxItems || index < 0 || _count >= MaxItems || Array[index] != null) return false;
        Array[index] = item;
        ++_count;
        return true;
    }

    protected int Add(TI item)
    {
        if (_count >= MaxItems) return -1;
        for (var i = 0; i < MaxItems; i++)
            if (Place(i, item))
                return i;
        throw new InvalidOperationException(); //Not possible
    }

    protected TI RemoveAt(int index)
    {
        if (_count == 0) return default;
        var result = Array[index];
        Array[index] = default;
        --_count;
        return result; //not possible
    }

    /**
     * Slow
     */
    protected bool Add(int index, TI item)
    {
        if (_count >= MaxItems) return false;
        if (_count == 0) return Place(index, item);
        var temp = RemoveAt(index);
        Place(index, item);
        for (var i = 0; Place(i, temp); i++) ;
        return true;
    }

    protected TI SwapAtIndex(int index, TI item)
    {
        if (index < 0 || index >= Array.Length)
            return item;
        var result = Array[index];
        Array[index] = item;
        return result;
    }

    //todo make protected
    public int AddItem(TI item)
    {
        if (_count >= Array.Length) return -1;
        return Add(item);
    }

    protected int AddItem(int index, TI item)
    {
        return Add(index, item) ? index : -1;
    }

    protected TI RemoveItem(int index)
    {
        if (_count >= MaxItems) throw new ArgumentOutOfRangeException(nameof(index), "Inventory already empty.");
        if (index >= _count)
            throw new IndexOutOfRangeException("Index is greater than the Inventory size or negative.");
        return RemoveAt(index);
        ;
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
        return _count;
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
    //private Container _container;
    private TextureButton _button;
    private ButtonGroup _playerInventoryButtons;

    private ItemTexture
        _bufferSlot =
            null; //Basically a free storage slot that also lets outside users interact with it. manipulated based on the clicked button.  

    public InventoryContainer(Vector2 size, int slots, TextureButton button) : base(slots)
    {
        Size = size;
        GlobalPosition = new Vector2(0, 0);

        _button = button ?? throw new ArgumentNullException(nameof(button), "button is null");

        _button.Hide();

        //this.AddChild(_container);
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
    public InventoryContainer() : this(Vector2.One, 1, new TextureButton())
    {
    }

    public int GenNodeGrid(Vector2 slotSize)
    {
        //NEEDS to use containers.............
        _button.Show();
        var result = CricketVisuals.GenerateNodeGrid(_button,
            slotSize, 1, MaxItems, this,
            this.Size, new int?(0));
        _button.Hide();

        foreach (var node in this.GetChildren())
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

    public new void Show()
    {
        Visible = true;
    }

    public new void Hide()
    {
        Visible = false;
    }

    public void ToggleVisible()
    {
        if (Visible)
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
        for (var i = 0; i < MaxItems; i++)
        {
            ItemTexture temp = Array[i];

            Sprite2D currentItemSprite = GetChild(i).GetNode<Sprite2D>("SpriteHolder");

            if (currentItemSprite == null) throw new Exception("SpriteHolder is null");
            //Console.WriteLine("uhhh " + temp);
            //something in this slot
            //nothing in this slot 
            currentItemSprite.SetTexture(temp?.Texture);
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
        _bufferSlot = item; //nullable
        return result;
    }

    /**
     * I thought it is nice to make containers in the editor to see the size in scene and wanted to make a way for the inventory to "steal" those stats.
     */
    public void CopyValues(Container container, bool destroyContainer)
    {
        if (container == null) return;
        Position = container.Position;
        Size = container.Size;
        this.TopLevel = container.TopLevel;
        if (destroyContainer)
            container.QueueFree();
    }
}