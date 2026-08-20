using System;
using Godot;

namespace Main.main.packages.dna_editor_window;

public partial class WarningConfirmation : Window
{
    [Export] public Button ConfirmButton { get; set; }
    [Export] public Button CancelButton { get; set; }

    [Export] public Label SubheadingDetails { get; set; }
    [Export] public Label WarningDetails { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (ConfirmButton == null) throw new Exception("ConfirmButton is null");
        if (CancelButton == null) throw new Exception("CancelButton is null");
        if (SubheadingDetails == null) throw new Exception("SubheadingDetails is null");
        if (WarningDetails == null) throw new Exception("TextDetails is null");
    }

    public void Clear()
    {
        SubheadingDetails.Text = null;
        WarningDetails.Text = null;
    }
}