using Godot;

namespace Main.Source.main;

public partial class Plant : Node
{
    //Abstract
    private float health;
    private float maxHealth;

    //Definite attributes
    private double _glucose;
    private double _h2O;
    private double _cO2;
    private double _oxygen;
    //I think sunlight shouldn't be a field. it isn't a component of the plant and has no reason to be stored.

    //hormones
    //circadian rhythm
    //injury

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void Tick()
    {
    }
}