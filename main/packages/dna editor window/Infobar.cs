using System;
using Godot;

namespace Main.main.packages.dna_editor_window;

public partial class Infobar : PanelContainer
{
    [Export] public TextureButton Back;
    [Export] public TextureButton Add;
    [Export] public TextureButton Delete;
    [Export] public TextureButton PushChanges;
    [Export] public TextureButton Print;
    [Export] public TextEdit NameDisplay;
    [Export] public TextEdit Id;
    [Export] public Label SelectedElements;

    public bool HasUnsavedChanges => !PushChanges.Disabled;

    [Signal]
    public delegate void BackPressedEventHandler();

    [Signal]
    public delegate void AddPressedEventHandler();

    [Signal]
    public delegate void DeletePressedEventHandler();

    [Signal]
    public delegate void NameChangedEventHandler(string name);

    [Signal]
    public delegate void IdChangedEventHandler();

    [Signal]
    public delegate void ChangesSavedPressedEventHandler();

    [Signal]
    public delegate void PrintPressedEventHandler();

    public override void _Ready()
    {
        base._Ready();
        if (Back == null)
            throw new Exception("Back not set");
        if (Add == null)
            throw new Exception("Add not set");
        if (Delete == null)
            throw new Exception("Delete not set");
        if (PushChanges == null)
            throw new Exception("PushChanges not set");
        if (Name == null)
            throw new Exception("Name not set");
        if (Id == null)
            throw new Exception("Id not set");
        if (SelectedElements == null)
            throw new Exception("SelectedElements not set");

        Back.Pressed += () => EmitSignal(nameof(BackPressed));
        Add.Pressed += () => EmitSignal(nameof(AddPressed));
        PushChanges.Pressed += () => EmitSignal(nameof(ChangesSavedPressed));
        Print.Pressed += () => EmitSignal(nameof(PrintPressed));

        NameDisplay.TextChanged += EmitNameChanged;
        Delete.Pressed += () => EmitSignal(nameof(DeletePressed));
        Id.TextChanged += EmitIdChanged;
        UnsavedChanges(false);
    }

    public int GetId()
    {
        if (string.IsNullOrEmpty(Id.Text)) return -1;
        try
        {
            return Convert.ToInt32(Id.Text);
        }
        catch (Exception e)
        {
            return -1;
        }
    }

    public void SetTitle(string s)
    {
        NameDisplay.Text = s;
    }

    public void UnsavedChanges(bool unsavedChanges = true)
    {
        PushChanges.Disabled = !unsavedChanges;
    }

    /**
     * do not emit a signal and change id
     */
    public void SetIdSilent(string id)
    {
        Id.TextChanged -= EmitIdChanged;
        Id.Text = id;
        Id.TextChanged += EmitIdChanged;
    }

    public void SetTitleSilent(string name)
    {
        NameDisplay.TextChanged -= EmitNameChanged;
        NameDisplay.Text = name;
        NameDisplay.TextChanged += EmitNameChanged;
    }

    public void EmitNameChanged()
    {
        EmitSignal(nameof(NameChanged), NameDisplay.Text);
    }

    public void EmitIdChanged()
    {
        EmitSignal(nameof(IdChanged));
    }
}