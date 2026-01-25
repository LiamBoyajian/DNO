using Godot;
using System;

public partial class MainCharacter : CharacterBody2D
{
	private const float Speed = 75.0f;
	//public const float JumpVelocity = -400.0f;
	private AnimatedSprite2D _mainChar;
	
	private Node _inventory;

	public override void _Ready()
	{
		_mainChar = GetNode<AnimatedSprite2D>("mainChar");
		_inventory = ResourceLoader.Load<PackedScene>("res://Source/Inventory.tscn").Instantiate();
		//_inventory.
		_inventory.SetIndexed("z_index", 10);
		//GetTree().Root.AddChild(_inventory);
		//_inventory.SetLa
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		
		//Console.WriteLine(_inventory.Get("z_index"));

		if (velocity.Equals(new Vector2(0,0)) && Input.IsKeyPressed(Key.E))
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
		}else
		{
			if (velocity.Y == 0f)
			{
				//_mainChar.SetAnimation("IdleBack");
				_mainChar.SetAnimation("IdleFront");
			}
			
		}
		
		Velocity = velocity;
		MoveAndSlide();
		if (Input.IsKeyPressed(Key.Tab))
		{
			_setInventory();	
		}
		
	}

	private void _setInventory()
	{
		
		
		if (_inventory.GetIndex() <= 0)
		{
			
		}
		else
		{
			//_inventory.SetIndexed("z_index", -10);
		}
	}
	
	
}
