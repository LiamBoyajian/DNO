using Godot;

namespace Main.main.scripts.scene;

public partial class CloseWindow : Window
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // In Godot 4 C#, events/signals use standard C# event syntax (+=)
        CloseRequested += OnWindowCloseRequested;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //base._Process(delta);
    }

    private void OnWindowCloseRequested()
    {
        Hide();
    }
}