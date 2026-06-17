using System;
using System.Reflection.PortableExecutable;
using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class MainCharacter : CharacterBody2D
{
    private const float Speed = 150.0f;


    //public const float JumpVelocity = -400.0f;
    private AnimatedSprite2D _mainChar;
    public outdated_DNA OutdatedDna = new outdated_DNA(new RandomNumberGenerator());
    private InventoryContainer _inventory;
    private AbstractMachine _openMachine;
    private Node _sceneDna;
    private Sprite2D _spriteOnMouse;
    private Node _sceneHeadNode;
    //private object _mouseSlot;


    [Signal]
    public delegate void RequestNearestDeviceEventHandler(Vector2 position);

    public override void _Ready()
    {
        _mainChar = GetNode<AnimatedSprite2D>("mainChar");
        GenerateInventory();
        this.CallDeferred(Node.MethodName.AddChild, _inventory);

        _spriteOnMouse = new Sprite2D();
        _spriteOnMouse.Name = "SpriteOnMouse";
        this.AddChild(_spriteOnMouse);
        _spriteOnMouse.ZIndex = 10;
        _inventory.Hide();
        _inventory.UpdatedBufferSlot += ShowHoldingItem;

        _sceneHeadNode = GetTree().Root.GetChild<Node>(0); //should be "main" atm
    }


    public override void _PhysicsProcess(double delta)
    {
        //Update mouseSprite pos
        if (_spriteOnMouse.Texture != null) //TODO make only when holding left click down.
            _spriteOnMouse.GlobalPosition = GetViewport().GetMousePosition();

        Vector2 velocity = Velocity;

        Vector2 direction = new Vector2(Input.GetAxis("ui_left", "ui_right"), Input.GetAxis("ui_up", "ui_down"));
        if (direction != Vector2.Zero)
        {
            velocity.X = direction.X * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        }

        if (velocity.X != 0f)
        {
            _mainChar.Play("Walk");
            if (velocity.X > 0f)
            {
                _mainChar.FlipH = false;
            }
            else
            {
                _mainChar.FlipH = true;
            }
        }
        else
        {
            if (velocity.Y == 0f && _mainChar.Animation != "IdleBack")
            {
                //_mainChar.SetAnimation("IdleBack");
                _mainChar.SetAnimation("IdleFront");
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        //TODO implement a dictionary for inputs to methods
        if (@event.IsActionPressed("Inventory"))
        {
            _inventory.ClearPressedButtons();
            _setInventory();
            return;
        }

        if (@event.IsActionPressed("Click"))
        {
            //CallDeferred("OnLeftClick");
        }

        if (@event.IsActionPressed("Open Nearest Object") && this.Velocity.Equals(new Vector2(0, 0)))
        {
            _mainChar.SetAnimation("IdleBack");
            if (_openMachine == null)
            {
                EmitSignal(MainCharacter.SignalName.RequestNearestDevice,
                    GlobalPosition); //I don't know why using 'this.' returns 0,0 so might be something to watch
            }
            else
            {
                CloseMachine();
            }

            return;
        }
    }

    private void ShowHoldingItem(Texture2D texture, bool bufferFull)
    {
        _spriteOnMouse.Texture = texture;
    }

    private void _setInventory()
    {
        _inventory.ToggleVisible();
    }

    private void OpenScene(string scene)
    {
        _sceneDna = ResourceLoader.Load<PackedScene>(scene).Instantiate();
        GetTree().Root.AddChild(_sceneDna);
    }

    private void GenerateInventory()
    {
        //Just called near the start and I put all my testing code in here. 
        var tempTextureButton = new TextureButton();
        tempTextureButton.TextureNormal = GetNode<AnimatedSprite2D>("_box").SpriteFrames.GetFrameTexture("Black", 0);
        tempTextureButton.TextureHover = GetNode<AnimatedSprite2D>("_box").SpriteFrames.GetFrameTexture("Selected", 0);
        tempTextureButton.TexturePressed =
            GetNode<AnimatedSprite2D>("_box").SpriteFrames.GetFrameTexture("Selected", 0);

        tempTextureButton.ToggleMode = true;

        var tempContainer = new Container();
        tempContainer.Size = new Vector2(410, 360);
        tempContainer.GlobalPosition = new Vector2(0, 0);

        _inventory = new InventoryContainer(new Vector2(410, 50), 10, tempTextureButton);
        _inventory.TopLevel = true;
        //Removable:

        //Texture2D temp = GetNode<AnimatedSprite2D>("Vial").SpriteFrames.GetFrameTexture("outdated_DNA", 0);
        //Texture2D temp2 = GetNode<AnimatedSprite2D>("Vial").SpriteFrames.GetFrameTexture("Plant", 0);

        //_inventory.AddItem(null);
        //_inventory.AddItem(null);
        //_inventory.AddItem(null);
        //_inventory.AddItem(null);
        //_inventory.AddItem(new Item<outdated_DNA>(temp2, new outdated_DNA(new RandomNumberGenerator())));
        //while (-1 != _inventory.AddItem(new Item<outdated_DNA>(temp, new outdated_DNA(new RandomNumberGenerator()))))
        //    ; //Remove after testing

        _inventory.GenNodeGrid(tempTextureButton.TextureNormal.GetSize());
    }

    public void CatchMachine(AbstractMachine machine)
    {
        _openMachine = machine;

        _inventory.UpdatedBufferSlot += HandleItemEventInventory;
        _openMachine.UpdatedBufferSlotWrapper += HandleItemEventMachine;
        _inventory.UpdatedBufferSlot -= ShowHoldingItem;
        _openMachine.ShowInventoryWrapper();
    }

    /**
     * Called when the inv was updated
     */
    private void HandleItemEventInventory(Texture2D texture, bool bufferFull)
    {
        ShowHoldingItem(texture, bufferFull);
        if (!_openMachine.HasBufferItemWrapper()) return;
        _openMachine.PlaceBufferItemWrapper(_inventory.SlotSwap(_openMachine.TakeBufferItemWrapper()));
        _inventory.ClearPressedButtons();
    }

    /**
     * Called when the machine was updated
     */
    private void HandleItemEventMachine(Texture2D texture, bool bufferFull)
    {
        ShowHoldingItem(texture, bufferFull);
        if (!_inventory.HasBufferItem()) return;
        _inventory.PlaceBufferItem(_openMachine.SlotSwapWrapper(_inventory.TakeBufferItem()));
        _openMachine.ClearPressedButtonsWrapper();
    }

    public void CloseMachine()
    {
        if (_openMachine == null) return;

        _inventory.UpdatedBufferSlot -= HandleItemEventInventory;
        _openMachine.UpdatedBufferSlotWrapper -= HandleItemEventMachine;

        _openMachine.HideInventoryWrapper();
        _openMachine.ClearPressedButtonsWrapper();
        ShowHoldingItem(_inventory.GetBufferSlotTexture(), false);

        _openMachine = null;
        _inventory.UpdatedBufferSlot += ShowHoldingItem;
    }
}