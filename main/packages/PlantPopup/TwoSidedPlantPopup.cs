using System.Collections;
using System.Collections.Generic;
using Godot;
using Main.main.packages.ResourceDisplay;
using Main.main.scripts.core.plants.interfaces;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.packages.PlantPopup;

public partial class TwoSidedPlantPopup : Window
{
    public static TwoSidedPlantPopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }

    public List<IResourceDisplay<Node>> DisplayTo { get; set; } = [];
    //[Export] public ResourceDisplay MaterialResourcesContainer;
    //[Export] public ResourceDisplay InfiniteResourcesContainer;

    protected IAttributeDictionary LastAttributeDictionary { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
        FocusExited += OnClose;

        foreach (var display in DisplayTo)
        {
            display.Buttons.Pressed += ButtonGroupWasPressed;
        }

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

        if (parent is IConcatEnumerable parentAsDictionaryEnumerable)
        {
            using var enumerable = parentAsDictionaryEnumerable.GetDictionaryConcatEnumerable().GetEnumerator();
            while (enumerable.MoveNext())
            {
                var item1 = enumerable.Current.Item1;
                var item2 = enumerable.Current.Item2;

                if (item1 == null || item2 == null)
                    continue;

                foreach (var display in DisplayTo)
                {
                    display.AddElement(enumerable.Current);
                }
            }
        }
    }

    public bool ClearElements()
    {
        bool result = false;
        //ensure "|| result" comes at the end
        foreach (var display in DisplayTo)
        {
            result = display.ClearChildren() || result;
        }

        return result;
    }

    public void ButtonGroupWasPressed(BaseButton button)
    {
        if (LastAttributeDictionary == null) return;


        var temp = button.Name.ToString()
            .Split(ResourceDisplay.Delimiter); //hardcoded based on name from ResourceDisplay
        bool refresh = false;


        //if (temp[0].Contains("Amount") && LastAttributeDictionary is IObtainable obtainable)
        //{
        //    refresh = obtainable.ParseObtain(temp[1]);
        //}
        //else if ((temp[0].Contains("Max") || temp[0].Contains("Infinite")) &&
        //         LastAttributeDictionary is IUpgradable upgradable)
        //{
        //    refresh = upgradable.ParseUpgrade(temp[1]);
        //}

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
            return;
        }

        if (LastAttributeDictionary is not IConcatEnumerable dictionaryEnumerator)
            return;

        using var enumerable = dictionaryEnumerator.GetDictionaryConcatEnumerable().GetEnumerator();
        while (enumerable.MoveNext())
        {
            foreach (var display in DisplayTo)
            {
                var item1 = enumerable.Current.Item1;
                var item2 = enumerable.Current.Item2;


                display.Update(item1.GetType().Name, item2);
            }
        }
        //TODO a way to update the buttons with my new dictionaryenumerator implementation
        //if (LastAttributeDictionary is IAttributeEnumerable attributeEnumerable)
        //{
        //    InfiniteResourcesContainer.UpdateAttributeButtons(attributeEnumerable.GetAttributeEnumerable());
        //}
//
        //if (LastAttributeDictionary is IMaterialEnumerable materialEnumerable)
        //{
        //    MaterialResourcesContainer.UpdateMaterialBars(materialEnumerable.GetMaterialEnumerable());
        //}
    }
}