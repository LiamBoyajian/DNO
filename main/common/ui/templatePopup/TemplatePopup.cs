using Godot;
using Main.Source.main;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.common.ui.templatePopup;

public partial class TemplatePopup : Window
{
    public static TemplatePopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }
    [Export] public PackedScene ResourceDisplayTemplate;
    [Export] public BoxContainer ResourcesContainer;
    [Export] public ButtonGroup ResourceUpgradeButtonGroup;


    [Export] public PackedScene LimitlessResourceDisplayTemplate;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        FocusExited += OnClose;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnClose()
    {
        Hide();
    }

    public void Popup()
    {
        Show();
        PopupCentered();
    }
}