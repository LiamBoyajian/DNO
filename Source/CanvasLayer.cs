using Godot;
using System;

public partial class CanvasLayer : Godot.CanvasLayer
{
	private Node _inventory;
	private bool _inventoryOpen = false;

	public override void _Ready()
	{
		_inventory = ResourceLoader.Load<PackedScene>("res://Source/Inventory.tscn").Instantiate();
		//_inventory.
		
		
		
		GetTree().Root.CallDeferred(Node.MethodName.AddChild, _inventory); //this shit is so stupid it's wild
		
		//_inventory.GetNode("/root/Menu").Free();
		_inventory.SetIndexed("z_index", -10);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Tab))
		{
			Console.WriteLine("HI");
			if (_inventoryOpen)
			{
				_inventory.SetIndexed("z_index", -10);
			}
			else
			{
				_inventory.SetIndexed("z_index", 10);
			}
			_inventoryOpen = !_inventoryOpen;
		}
			//Console.WriteLine(_inventory.GetChildCount());
	}
}
