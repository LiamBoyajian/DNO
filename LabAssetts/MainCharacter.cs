using System;
using Godot;
using Main.InventoryAssets;

namespace Main.LabAssetts;

public partial class MainCharacter : CharacterBody2D
{
    private const float Speed = 150.0f;


    //public const float JumpVelocity = -400.0f;
    private AnimatedSprite2D _mainChar;
    public DNA dna = new DNA(new RandomNumberGenerator());
    private InventoryContainer _inventory;
    private Node _sceneDna;
    private Sprite2D _spriteOnMouse;

    //private object _mouseSlot;

    [Signal]
    public delegate void OpenedSignalEventHandler(Vector2 position);

    public override void _Ready()
    {
        _mainChar = GetNode<AnimatedSprite2D>("mainChar");
        GenerateInventory();
        this.CallDeferred(Node.MethodName.AddChild, _inventory);
        
        _spriteOnMouse = new Sprite2D();
        _spriteOnMouse.Name = "SpriteOnMouse";
        this.AddChild(_spriteOnMouse);
        
        _inventory.Hide();
        _inventory.UpdatedBufferSlot += ShowHoldingItem;
    }


    public override void _PhysicsProcess(double delta)
    {
        //Update mouseSprite pos
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

        //if (@event.IsActionPressed("ui_text_clear_carets_and_selection"))
        //{
        //    _inventory.ClearPressedButtons();
        //    return;
        //}

        if (@event.IsActionPressed("Click"))
        {
            //CallDeferred("OnLeftClick");
        }

        if (this.Velocity.Equals(new Vector2(0, 0)) && @event.IsActionPressed("Open Nearest Object"))
        {
            _mainChar.SetAnimation("IdleBack");
            if (_sceneDna == null)
            {
                EmitSignal(MainCharacter.SignalName.OpenedSignal,
                    _mainChar
                        .GlobalPosition); //I don't know why using 'this.' returns 0,0 so might be something to watch
            }
            else
            {
                GetTree().Root.RemoveChild(_sceneDna);
                _sceneDna.Free();
                _sceneDna = null;
            }

            return;
        }
    }

    private void ShowHoldingItem()
    {
        _spriteOnMouse.Texture = _inventory.GetBufferSlot()?.Texture;
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

        _inventory = new InventoryContainer(tempContainer, 10, tempTextureButton);


        Texture2D temp = GetNode<AnimatedSprite2D>("Vial").SpriteFrames.GetFrameTexture("DNA", 0);
        Texture2D temp2 = GetNode<AnimatedSprite2D>("Vial").SpriteFrames.GetFrameTexture("Plant", 0);

        _inventory.AddItem(null);
        _inventory.AddItem(null);
        _inventory.AddItem(null);
        _inventory.AddItem(null);
        _inventory.AddItem(new Item<DNA>(temp2, new DNA(new RandomNumberGenerator())));
        while (-1 != _inventory.AddItem(new Item<DNA>(temp, new DNA(new RandomNumberGenerator()))))
            ; //Remove after testing

        _inventory.GenNodeGrid(tempTextureButton.TextureNormal.GetSize());
    }
}