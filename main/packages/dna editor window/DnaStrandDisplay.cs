using System;
using System.Collections.Generic;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;
using Main.main.packages.model.Dna;
using Main.main.packages.plants.enums;

namespace Main.main.packages.dna_editor_window;

public partial class DnaStrandDisplay : Container
{
    public DnaStrand SelectedDna { get; private set; }

    public void DisplayDna(DnaStrand value)
    {
        GD.Print(value?.ToString());
        SelectedDna = value;
        if (value == null)
        {
            GeneEditor.CloseGenes();
            return;
        }

        GeneEditor.DisplayGenes(value.Genes);
    }

    [Export] public OptionButton ComparisonSymbolDropdown;
    [Export] public OptionButton EnumDropdown;
    [Export] public TextEdit DnaValueText;
    [Export] GeneEditor GeneEditor { get; set; }

    protected Dictionary<string, Enum> StringToEnum = new();


    public override void _Ready()
    {
        base._Ready();

        if (ComparisonSymbolDropdown == null) throw new Exception("ComparisonSymbolDropdown is null");
        if (EnumDropdown == null) throw new Exception("EnumDropdown is null");
        if (GeneEditor == null) throw new Exception("GeneEditor is null");
        if (DnaValueText == null) throw new Exception("DnaValueText is null");

        foreach (Type enumType in EnumLibrary.Enums)
        {
            foreach (var @object in Enum.GetValues(enumType))
            {
                if (@object is not Enum @enum) continue;
                EnumDropdown.AddItem(@enum.ToString());
                EnumDropdown.AddIconItem(MemoryToDb.GetTextureFromEntry(new Entry(@enum)), @enum.ToString());
                StringToEnum.Add($"{enumType.FullName}.{@enum}", @enum);
            }
        }

        foreach (var comparisonType in Promoter.ComparisonKeys)
        {
            ComparisonSymbolDropdown.AddItem(comparisonType);
        }
    }
}