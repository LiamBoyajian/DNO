using Godot;

namespace Main.main.scripts.core.util;

public partial class MarketPopup : Window
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    private void Settings()
    {
        Visible = true;
        WrapControls = false;
        Transient = true;
        TransientToFocused = true;
        Exclusive = false;
        Unresizable = true;
        Borderless = false;
        AlwaysOnTop = true;
        Transparent = true;
        Unfocusable = false;
        PopupWindow = true;
        ExtendToTitle = false;
        MousePassthrough = false;
        SharpCorners = false;
        ExcludeFromCapture = false;
        PopupWMHint = true;
        MinimizeDisabled = true;
        MaximizeDisabled = true;
        ForceNative = false;
    }
}