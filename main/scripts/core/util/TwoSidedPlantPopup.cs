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

    protected IAttributeDictionary LastAttributeDictionary { get; private set; }


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
        if (parent is IAttributeDictionary u)
        {
            LastAttributeDictionary = u;
            if (LastAttributeDictionary is IBroadcastsUpdate broadcaster)
            {
                broadcaster.Updated += () => Refresh();
            }
        }


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
        //TODO remove hard coding this terrible (za stringz are very baad
        if (LastAttributeDictionary == null) return;

        var temp = button.Name.ToString().Split('_'); //hardcoded based on name from ResourceDisplay
        bool refresh = false;

        if (temp[0].Contains("Amount") && LastAttributeDictionary is IObtainable obtainable)
        {
            refresh = obtainable.ParseObtain(temp[1]);
        }
        else if ((temp[0].Contains("Max") || temp[0].Contains("Infinite")) &&
                 LastAttributeDictionary is IUpgradable upgradable)
        {
            refresh = upgradable.ParseUpgrade(temp[1]);
        }

        if (refresh)
            Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible || !@event.IsAction("ui_filedialog_refresh")) return;
        Refresh();
    }

    public void Refresh(bool fullRefresh = false)
    {
        if (fullRefresh)
        {
            ClearElements();
            Popup(LastAttributeDictionary as Node);
        }
        else
        {
            if (LastAttributeDictionary is IAttributeEnumerable attributeEnumerable)
            {
                InfiniteResourcesContainer.UpdateAttributeButtons(attributeEnumerable.GetAttributeEnumerable());
            }

            if (LastAttributeDictionary is IMaterialEnumerable materialEnumerable)
            {
                MaterialResourcesContainer.UpdateMaterialBars(materialEnumerable.GetMaterialEnumerable());
            }
        }
    }
}