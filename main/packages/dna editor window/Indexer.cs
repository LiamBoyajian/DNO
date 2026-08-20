using Godot;
using System;
using System.Collections.Generic;

public partial class Indexer : PanelContainer
{
    [Export] private BaseButton _up;
    [Export] private BaseButton _down;
    [Export] private Container _elementHolder;
    [Export] private PackedScene _dotScene;
    private ButtonGroup _buttonGroup;

    public bool ValidIndex => Index >= 0 && Index < MaxSize;


    [Signal]
    public delegate void IndexChangedEventHandler(int index);

    private List<BaseButton> _buttons = [];
    public int Index { get; private set; } = -1;

    protected int MaxSize = 0;

    public void SetMax(int value)
    {
        MaxSize = 0;
        Clear();
        if (value < 1)
            return;

        if (_buttonGroup == null) _buttonGroup = new ButtonGroup();

        for (int i = 0; i < value; i++)
        {
            var node = _dotScene.Instantiate();
            if (node is not BaseButton button) throw new Exception("Button scene is not basebutton");
            button.ButtonGroup = _buttonGroup;
            button.ButtonPressed = false;
            button.Name = "" + (i + 1);
            _elementHolder.AddChild(button);
            _buttons.Add(button);
        }

        MaxSize = value;
    }


    public override void _Ready()
    {
        base._Ready();
        _buttonGroup = new ButtonGroup();

        if (_up == null) throw new NullReferenceException("_up is null");
        if (_down == null) throw new NullReferenceException("_down is null");
        if (_dotScene == null) throw new NullReferenceException("_dotScene is null");
        if (_elementHolder == null)
        {
            _elementHolder = FindChild("ElementHolder", true) as Container;
            if (_elementHolder == null)
                throw new NullReferenceException("_elementHolder is null");
        }

        _up.Pressed += () => ChangeIndex(-1);
        _down.Pressed += () => ChangeIndex(1);
        _buttonGroup.Pressed += PressedHandler;
    }

    public void ChangeIndex(int change)
    {
        if (change + Index >= MaxSize || change + Index < 0) return;
        _buttons[Index + change].ButtonPressed = true; //Will reach PressedHandler
    }

    private void PressedHandler(BaseButton button)
    {
        if (button == null) return;
        var index = _buttons.IndexOf(button);
        if (index < 0) return;
        if (Index == index)
        {
            GD.Print("some bs");
            Index = -1;
            button.ButtonPressed = false;
        }
        else
        {
            Index = index;
        }

        EmitSignal(nameof(IndexChanged), Index);
    }

    public void Clear()
    {
        foreach (var button in _buttons)
        {
            button.QueueFree();
        }

        _buttons.Clear();
        MaxSize = 0;
    }

    public void Deselect()
    {
        Index = -1;
    }
}