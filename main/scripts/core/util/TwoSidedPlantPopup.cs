using Godot;
using Main.main.scripts.core.plants.interfaces;
using Main.main.scripts.core.util.interfaces;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.scripts.core.util;

public partial class TwoSidedPlantPopup : Window, IPlantPopup
{
    public static TwoSidedPlantPopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }

    [Export] public ResourceDisplay MaterialResourcesContainer;
    [Export] public ResourceDisplay InfiniteResourcesContainer;

    protected IUpgradable LastUpgradable { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        FocusExited += OnClose;

        MaterialResourcesContainer.Buttons.Pressed += ButtonGroupWasPressed;
        InfiniteResourcesContainer.Buttons.Pressed += ButtonGroupWasPressed;


        OnClose();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnClose()
    {
        Hide();
        ClearElements();
    }

    /**
     *  Can display:
     *  IAttributeEnumerable, IMaterialEnumerable
     *
     */
    public void Popup(Node parent)
    {
        if (parent is IUpgradable u)
            LastUpgradable = u;


        ClearElements();
        Show();
        PopupCentered();
        if (parent is IAttributeEnumerable attributeEnumerable)
        {
            InfiniteResourcesContainer.CreateAttributeButtons(attributeEnumerable.GetAttributeEnumerable());
        }

        if (parent is IMaterialEnumerable enumerable)
        {
            MaterialResourcesContainer.CreateMaterialBars(enumerable.GetMaterialEnumerable());
        }
        //ResourcesContainer.CreateMaterialBars();
        //ResourcesContainer.CreateAttributeButtons();
    }

    public bool ClearElements()
    {
        bool result = false;
        //ensure "|| result" comes at the end
        result = InfiniteResourcesContainer.ClearChildren();
        result = MaterialResourcesContainer.ClearChildren() || result;
        return result;
    }

    public void ButtonGroupWasPressed(BaseButton button)
    {
        if (LastUpgradable == null) return;

        var temp = button.Name.ToString().Split('_')[1]; //hardcoded based on name from ResourceDisplay

        if (LastUpgradable.ParseUpgrade(temp))
            Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsAction("ui_filedialog_refresh")) return;
        Refresh();
    }

    public bool Refresh()
    {
        ClearElements();
        Popup(LastUpgradable as Node);
        return LastUpgradable != null;
    }
}