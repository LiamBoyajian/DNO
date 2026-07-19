using Godot;

namespace Main.main.scripts.scene;

public partial class SceneTransitionTemplate : Area2D
{
    [Export] private PackedScene _targetScene;
    private Viewport _root;
    [Export] public bool Enabled { get; set; } = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _root = GetTree().Root;
        if (_targetScene == null)
            GD.PrintErr("No target scene in " + this);


        InputEvent += InputEventHandler;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void InputEventHandler(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("Click"))
        {
            TriggerSceneTransition(true);
        }
    }

    public bool TriggerSceneTransition(bool clearMe = true)
    {
        if (!Enabled)
            return false;

        if (_targetScene == null)
        {
            GD.PrintErr("No target scene in " + this);
            return false;
        }

        Node nextScene = _targetScene.Instantiate();
        _root.AddChild(nextScene);
        Node oldScene = GetTree().CurrentScene;
        GetTree().CurrentScene = nextScene;

        if (clearMe)
            oldScene.QueueFree();

        return true;
    }
}