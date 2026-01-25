using Godot;
using System;
using System.Diagnostics;
public partial class MenuLogic : Control
{
	
	private Button _startButton;
	
	private Node _simultaneousScene;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GetTree().Root.;
		
		Debug.WriteLine("Started");
		_startButton = GetNode<Button>("Control/Start");
		if(_startButton == null) Debug.WriteLine("Start button not found");
		//Debug.WriteLine(GetNode<Button>("Control/Start"));
		
		//this method instantiates the scene
		//probably not good for use outside menus xxx
		//TODO: Check if there is a better method or if I should just do this within the method.
		_simultaneousScene = ResourceLoader.Load<PackedScene>("res://Initial.tscn").Instantiate();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_startButton.ButtonPressed)
		{
			Debug.WriteLine("Start button pressed");
			StartLogic();
		}
	}

	private void StartLogic()
	{
		GetTree().Root.AddChild(_simultaneousScene);
		GetNode("/root/Menu").Free();
	}
}