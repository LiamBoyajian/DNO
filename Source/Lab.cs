using Godot;
using System;

public partial class Lab : Node2D
{
	
	private Node _inventory;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_inventory = ResourceLoader.Load<PackedScene>("res://Source/Inventory.tscn").Instantiate();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
