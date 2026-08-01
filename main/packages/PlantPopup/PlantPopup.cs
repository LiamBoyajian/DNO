using System.Collections;
using System.Collections.Generic;
using Godot;
using Main.main.packages.ResourceDisplay;
using Main.main.scripts.core.plants.interfaces;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.packages.PlantPopup;

public partial class PlantPopup : PanelContainer
{
    public static PlantPopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }

    public List<IResourceDisplay<Node>> DisplayTo { get; set; } = [];
    //[Export] public ResourceDisplay MaterialResourcesContainer;
    //[Export] public ResourceDisplay InfiniteResourcesContainer;

    protected IAttributeDictionary LastAttributeDictionary { get; private set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (var child in FindChildren("*"))
        {
            if (child is not IResourceDisplay<Node> resourceDisplay) continue;

            DisplayTo.Add(resourceDisplay);
        }

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
        //PopupCentered();

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
                    //Can later use suffixes to distinguish between nodes
                    if (display is ProgressBars)
                    {
                        //Testing: hardcoded
                        if (item1 is not (AbstractPlant.Rt.Health or AbstractPlant.Rt.H2O
                            or AbstractPlant.Rt.Glucose))
                        {
                            continue;
                        }
                    }

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
            .Split(ResourceDisplayTools.Delimiter);
        bool refresh = false;

        //TODO
        //Future implementation: use suffixes to identify button presses.
        //if (temp[0].Contains(UpgradeButtons.ClassNamePrefix) && LastAttributeDictionary is IObtainable obtainable)
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
    }
}