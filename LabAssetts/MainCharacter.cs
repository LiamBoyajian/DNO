using System;
using Godot;
using Main.InventoryAssets;

namespace Main.LabAssetts;

public partial class MainCharacter : CharacterBody2D
{
    private const float Speed = 75.0f;


    //public const float JumpVelocity = -400.0f;
    private AnimatedSprite2D _mainChar;
    public DNA dna = new DNA(new RandomNumberGenerator());
    private InventoryContainer _inventory;
    private Node _sceneDna;
    //private object _mouseSlot;

    [Signal]
    public delegate void OpenedSignalEventHandler(Vector2 position);

    public override void _Ready()
    {
        _mainChar = GetNode<AnimatedSprite2D>("mainChar");

        GenerateInventory();
        this.CallDeferred(Node.MethodName.AddChild, _inventory);
        _inventory.Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
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
            _setInventory();
            return;
        }

        if (@event.IsActionPressed("ui_text_clear_carets_and_selection"))
        {
            _inventory.ClearPressedButtons();
            return;
        }

        if (@event.IsActionPressed("Click"))
        {
            //if (_inventory == null) return;
            if (_inventory.GetPressedButton() == null)
            {
                Console.WriteLine("Did not click a button");
            }
            else
            {
                Console.WriteLine("Clicked Box: " + _inventory.GetPressedButton().ToString());
            }

            return;
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
        _inventory.GenNodeGrid(tempTextureButton.TextureNormal.GetSize());
    }
}