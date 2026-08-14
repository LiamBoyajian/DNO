using Godot;
using Main.main._Outside_Building;

namespace Main.main.scripts.scene;

public partial class SceneTransitionTemplate : Area2D
{
    [Export(PropertyHint.File, "*.tscn")] private string _targetScenePath = "";
    private Viewport _root;
    [Export] public bool Enabled { get; set; } = true;
    [Export] public bool TransitionOnCollision { get; set; } = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _root = GetTree().Root;
        if (string.IsNullOrEmpty(_targetScenePath))
            GD.PrintErr("No target scene path set in " + Name);


        //Short-term solution
        var persistentNode = GetTree().Root.GetNodeOrNull("InventoryDisplay");
        if (persistentNode != null && persistentNode.GetParent() is null)
        {
            GetTree().Root.CallDeferred(Node.MethodName.RemoveChild, persistentNode);
            AddChild(persistentNode);
        }


        InputEvent += InputEventHandler;
        AreaEntered += AreaEnteredHandler;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void InputEventHandler(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            foreach (var area in GetOverlappingAreas())
            {
                if (area.GetParent() is Player p)
                {
                    TriggerSceneTransition(p, true);
                }
            }
        }
    }

    private void AreaEnteredHandler(Area2D area)
    {
        if (!TransitionOnCollision) return;
        if (area.GetParent() is not Player p) return;
        TriggerSceneTransition(p);
    }

    public bool TriggerSceneTransition(Player player, bool clearMe = true)
    {
        if (!Enabled)
            return false;

        if (string.IsNullOrEmpty(_targetScenePath))
        {
            GD.PrintErr("No target scene in " + this);
            return false;
        }

        player.PrepareTransition();

        SceneTree tree = GetTree();
        string targetPath = _targetScenePath;

        //Short-term solution
        var persistentNode = GetTree().Root.GetNodeOrNull("InventoryDisplay");
        persistentNode.GetParent()?.RemoveChild(persistentNode);
        GetTree().Root.AddChild(persistentNode);


        Callable.From(() => { tree?.ChangeSceneToFile(targetPath); }).CallDeferred();

        return true;
    }
}