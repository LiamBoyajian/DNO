using System;
using Godot;

namespace Main.main.packages.dna_editor_window;

public partial class Infobar : PanelContainer
{
    [Export] protected TextureButton Back;
    [Export] protected TextureButton Add;
    [Export] protected TextureButton Delete;
    [Export] protected TextEdit Name;
    [Export] protected TextEdit Id;
    [Export] protected Label SelectedElements;

    [Signal]
    public delegate void BackPressedEventHandler();

    [Signal]
    public delegate void AddPressedEventHandler();

    [Signal]
    public delegate void DeletePressedEventHandler();

    [Signal]
    public delegate void NameChangedEventHandler();

    [Signal]
    public delegate void IdChangedEventHandler();

    public bool IdUnupdated { get; private set; }
    public bool NameUnupdated { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        if (Back == null)
            throw new Exception("Back not set");
        if (Add == null)
            throw new Exception("Add not set");
        if (Delete == null)
            throw new Exception("Delete not set");
        if (Name == null)
            throw new Exception("Name not set");
        if (Id == null)
            throw new Exception("Id not set");
        if (SelectedElements == null)
            throw new Exception("SelectedElements not set");
        Back.Pressed += () => EmitSignal(nameof(BackPressed));
        Add.Pressed += () => EmitSignal(nameof(AddPressed));
        Delete.Pressed += () => EmitSignal(nameof(DeletePressed));
        Id.TextChanged += () => IdUnupdated = true;
        Name.TextChanged += () => NameUnupdated = true;
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        base._UnhandledKeyInput(@event);
        if (@event.IsAction("update_window"))
        {
            if (IdUnupdated)
                EmitSignal(nameof(IdChanged));
            if (NameUnupdated)
                EmitSignal(nameof(NameChanged));
        }
    }

    public int GetId()
    {
        return Convert.ToInt32(Id.Text);
    }
}