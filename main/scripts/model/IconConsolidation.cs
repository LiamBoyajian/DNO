using Godot;

namespace Main.main.scripts.model;

public partial class IconConsolidation : Node
{
    public IconConsolidation Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}