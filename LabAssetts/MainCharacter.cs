using Godot;
using System;
using Main.InventoryAssets;

public partial class MainCharacter : CharacterBody2D
{
    private const float Speed = 75.0f;


    //public const float JumpVelocity = -400.0f;
    private AnimatedSprite2D _mainChar;
    public DNA dna = new DNA(new RandomNumberGenerator());
    private Control _inventory;

    public override void _Ready()
    {
        _mainChar = GetNode<AnimatedSprite2D>("mainChar");
        _inventory = (Control)ResourceLoader.Load<PackedScene>("res://Source/Inventory.tscn").Instantiate();

        GetTree().Root.CallDeferred(Node.MethodName.AddChild, _inventory);

        _inventory.Set("z_index", 10);
        _inventory.Hide();

        //Console.WriteLine(dna.GetDnaString());
        //Console.WriteLine(dna.toString());
        //Console.WriteLine("At index 1: " + (AcidBases)dna.GetDnaAtIndex(1) + "\n");
        foreach (var b in dna)
        {
            Console.Write(b);
        }

        Console.WriteLine("\nat index 20: " + (AcidBases)dna.GetDnaAtIndex(20));
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        //Console.WriteLine(_inventory.Get("z_index"));

        if (velocity.Equals(new Vector2(0, 0)) && Input.IsKeyPressed(Key.E))
        {
            //if(!Input.IsKeyPressed(Key.E)) 
            _mainChar.SetAnimation("IdleBack");
            return;
        }


        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = new Vector2(Input.GetAxis("ui_left", "ui_right"), Input.GetAxis("ui_up", "ui_down"));
        //Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
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
            if (velocity.Y == 0f)
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
        //@event. 
        if (@event.IsActionPressed("Inventory"))
        {
            _setInventory();
            return;
        }
    }

    private void _setInventory()
    {
        //_inventory.Show();
        _inventory.Set("visible", !_inventory.Get("visible").AsBool());
    }

    // Considering for a mouse free interaction with items. More QOL than anything. Finishing later. If not implemented in months delete...
    //public static Node _getNearestNode(Tree tree, Vector2 position)
    //{
    //    if(tree == null)
    //        return null;
    //    Node nearest = null;
    //    Node main = tree.FindChild("main",false); 
    //    foreach (Node n in main.GetChildren())
    //    { //I'm like 90 
    //        if( || nearest == null)
    //        {
    //            nearest = n;
    //        }
    //    }
    //    
    //    
    //    
    //    return result;
    //}
}