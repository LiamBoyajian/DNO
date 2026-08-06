using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using Main.main.packages.ResourceDisplay;
using Main.main.scripts.core.plants.interfaces;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.packages.PlantPopup;

public partial class PlantPopup : Window
{
    public static PlantPopup Instance { get; private set; }
    public AbstractPlant SelectedPlant { get; set; }

    public Dictionary<IResourceDisplay<Node>, EnumGate> DisplayTo { get; set; } = [];

    //[Export] public ResourceDisplay MaterialResourcesContainer;
    //[Export] public ResourceDisplay InfiniteResourcesContainer;
    //public EnumGate @EnumGate { get; set; }
    protected IAttributeDictionary LastAttributeDictionary { get; private set; }

    private string _purchaseSuffix = "purchase";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //@EnumGate = new EnumGate();


        foreach (var child in FindChildren("*"))
        {
            if (child is not IResourceDisplay<Node> resourceDisplay) continue;

            if (resourceDisplay is ProgressBars progressBar)
            {
                var temp = new EnumGate();
                temp.CreateGate(typeof(AbstractPlant.Rt), 0, 3, 4);
                DisplayTo.Add(progressBar, temp);
            }
            else
            {
                DisplayTo.Add(resourceDisplay, new EnumGate());
            }
        }

        Instance = this;
        FocusExited += OnClose;

        foreach (var display in DisplayTo)
        {
            display.Key.Buttons.Pressed += ButtonGroupWasPressed;
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
                    var suffix = "";

                    if (display.Key is ProgressBars && !display.Value.Permits(item1, false))
                    {
                        continue;
                    }

                    if (display.Key is UpgradeButtons ub)
                    {
                        suffix = _purchaseSuffix;
                    }

                    display.Key.AddElement(enumerable.Current, suffix);
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
            result = display.Key.ClearChildren() || result;
        }

        return result;
    }

    public void ButtonGroupWasPressed(BaseButton button)
    {
        if (LastAttributeDictionary == null) return;


        var alternateSplitDelimiter = button.Name.ToString()
            .Split(ResourceDisplayTools.DelimiterAlternate);
        var primarySplitDelimiter = alternateSplitDelimiter[2]
            .Split(ResourceDisplayTools.Delimiter);


        var @enum = ResourceDisplayTools.ConvertStringToEnum(alternateSplitDelimiter[1].Replace('_', '.'),
            Convert.ToInt32(primarySplitDelimiter[0]));
        if (@enum == null) throw new Exception("Button pressed has no valid enum");

        bool refresh = false;

        //TODO
        //Future implementation: use suffixes to identify button presses.
        if (string.CompareOrdinal(primarySplitDelimiter[1], _purchaseSuffix) == 0)
        {
            if (Input.IsKeyPressed(Key.Ctrl))
            {
                if (LastAttributeDictionary is IUpgradable upgradable)
                    refresh = upgradable.ParseUpgrade(@enum);
            }
            else
            {
                if (LastAttributeDictionary is IObtainable obtainable)
                    refresh = obtainable.ParseObtain(@enum);
            }
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
        //TODO overload refresh specific instance
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

                display.Key.Update(item1,
                    item2); //TODO need to be able to differentiate between duplicate values if identical names are present
            }
        }
    }
}