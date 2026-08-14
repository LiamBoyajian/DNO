using Godot;
using Main.main.packages.items;

namespace Main.main.scripts.core.util;

public abstract partial class DisappearSlot : Panel
{
    protected bool Accepting = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return Accepting;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        // Convert Variant to a Godot Object/Node so we can manipulate its transform

        if (data.Obj is not WrapperIItem wrapper) return;

        if (!AfterAccepted(wrapper)) return;
        if (wrapper.GetParent() != null) wrapper.GetParent().RemoveChild(wrapper);
        IsAccepting(false);
    }

    public void IsAccepting(bool value = true)
    {
        Accepting = value;
        if (Accepting)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    public abstract bool AfterAccepted(WrapperIItem item);
}