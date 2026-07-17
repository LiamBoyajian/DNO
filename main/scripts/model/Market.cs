using Godot;

namespace Main.main.scripts.model;

public partial class Market : Node
{
    public static Market Instance { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        Instance = this;
    }
}