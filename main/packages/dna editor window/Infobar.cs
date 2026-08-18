using System;
using Godot;

namespace Main.main.packages.dna_editor_window;

public partial class Infobar : PanelContainer
{
    [Export] public TextureButton Back;
    [Export] public TextureButton Add;
    [Export] public TextureButton Delete;
    [Export] public TextEdit NameDisplay;
    [Export] public TextEdit Id;
    [Export] public Label SelectedElements;

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
        Id.TextChanged += () => EmitSignal(nameof(IdChanged));
    }

    public int GetId()
    {
        return Convert.ToInt32(Id.Text);
    }

    public void SetNameTitle(string s)
    {
        NameDisplay.Text = s;
    }
}