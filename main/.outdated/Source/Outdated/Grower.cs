using Main.Package;

namespace Main.Source;

public partial class Grower : AbstractMachine
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        RunOnReady();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}