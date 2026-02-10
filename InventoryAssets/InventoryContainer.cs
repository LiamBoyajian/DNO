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
    protected TextureButton Button;
    protected ButtonGroup PlayerInventoryButtons;
    protected InventoryContainer ConnectedInventoryContainer;

    protected ItemTexture
        BufferSlot =
            null; //Basically a free storage slot that also lets outside users interact with it. manipulated based on the clicked button.  


    public InventoryContainer(Vector2 size, int slots, TextureButton button) : base(slots)
    {
        Size = size;
        GlobalPosition = new Vector2(0, 0);

        Button = button ?? throw new ArgumentNullException(nameof(button), "button is null");

        Button.Hide();

        //this.AddChild(_container);
        this.AddChild(Button); //TODO I think there is a bug with this : shouldnt make a child should just free
        PlayerInventoryButtons = new ButtonGroup();
        PlayerInventoryButtons.SetName("playerInventoryButtons");
        PlayerInventoryButtons.Pressed += OnGroupButtonPressed;
        //this.AddChild(_buttonGroup);
        //_playerInventoryButtons.
    }

    [Signal]
    public delegate void UpdatedBufferSlotEventHandler(Texture2D texture, bool bufferFull);


    //Required by Godot
    public InventoryContainer() : this(Vector2.One, 1, new TextureButton())
    {
    }

    public int GenNodeGrid(Vector2 slotSize)
    {
        //NEEDS to use containers.............
        Button.Show();
        var result = CricketVisuals.GenerateNodeGrid(Button,
            slotSize, 1, MaxItems, this,
            this.Size, new int?(0));
        Button.Hide();

        foreach (var node in GetChildren())
        {
            if (node is not (TextureButton))
                throw new ArrayTypeMismatchException();


            TextureButton textureButton = node as TextureButton;

            Sprite2D spriteHolder = new Sprite2D();
            spriteHolder.Name = "SpriteHolder";
            spriteHolder.Position = new Vector2(Button.GetTextureNormal().GetSize().X / 2,
                Button.GetTextureNormal().GetSize().Y / 2); //center it
            textureButton.AddChild(spriteHolder);

            textureButton.ButtonGroup = PlayerInventoryButtons;
        }

        UpdateItemsDisplay();
        return result;
    }

    private void OnGroupButtonPressed(BaseButton button)
    {
        //button alway not null...
        LoadBufferSlot();
        Console.WriteLine("Buffer : " + BufferSlot);
        EmitSignal(nameof(UpdatedBufferSlot), BufferSlot?.Texture,
            BufferSlot != null); //SlotSwapped Emit == on button press      
    }

    public void ShowInventory()
    {
        Visible = true;
    }

    public bool HasBufferItem()
    {
        return BufferSlot != null;
    }


    public void HideInventory()
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
        return PlayerInventoryButtons?.GetPressedButton();
    }

    public void ClearPressedButtons()
    {
        if (PlayerInventoryButtons.GetPressedButton() == null) return;
        if (BufferSlot != null)
        {
            if (!Place(GetPressedButton().GetIndex(), BufferSlot))
                AddItem(BufferSlot);
            BufferSlot = null;
        }

        PlayerInventoryButtons.GetPressedButton().SetPressed(false);
        LoadBufferSlot();
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
        BufferSlot = SwapAtIndex(GetPressedButton().GetIndex(), BufferSlot);
        UpdateItemsDisplay(); //Visual update
        //EmitSignal(SignalName.UpdatedBufferSlot);
        return BufferSlot;
    }

    public void CopyValues(Container container, bool destroyContainer)
    {
        if (container == null) return;
        Position = container.Position;
        Size = container.Size;
        this.TopLevel = container.TopLevel;
        if (destroyContainer)
            container.QueueFree();
    }

    public ItemTexture SlotSwap(ItemTexture item)
    {
        var result = BufferSlot;
        BufferSlot = item;
        LoadBufferSlot();
        return result;
    }

    public ItemTexture PlaceBufferItem(ItemTexture item)
    {
        if (BufferSlot == null)
        {
            BufferSlot = item;
            LoadBufferSlot();
            return null;
        }

        return item;
    }

    public ItemTexture TakeBufferItem()
    {
        var result = BufferSlot;
        BufferSlot = null;
        ClearPressedButtons();
        return BufferSlot;
    }

    public Texture2D GetBufferSlotTexture()
    {
        return BufferSlot?.Texture;
    }
}