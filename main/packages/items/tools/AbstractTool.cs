using Godot;
using Main.main.scripts.core.util.inventory;

namespace Main.main.scripts.core.util.items.tools;

public enum ToolType
{
    NaN,
    Shovel,
    PruningShears,
    DnaSampler,
    WateringCan,
}

public partial class AbstractTool : AnimatedSprite2D
{
    protected bool IsEquipped = false;
    [Export] public ToolType Type { get; private set; }
    [Export] protected Area2D ClickArea;

    public override void _Ready()
    {
        base._Ready();
        ClickArea.InputEvent += Equip;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (IsEquipped)
        {
            GlobalPosition = GetGlobalMousePosition();
        }
    }

    protected void Equip(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsAction("left_click"))
        {
            IsEquipped = !IsEquipped;
        }
    }

    public bool HasType()
    {
        return Type != ToolType.NaN;
    }

    public bool IsType(ToolType type)
    {
        return Type == type;
    }

    public bool IsSameType(AbstractTool tool)
    {
        return Type == tool.Type;
    }
}