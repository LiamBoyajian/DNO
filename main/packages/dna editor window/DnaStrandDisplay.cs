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

        if (!GeneEditor.Indexer.ValidIndex) Hide();
    }

    public void DisplayDna(DnaStrand value)
    {
        Show();
        SelectedDna = value;
        if (value == null)
        {
            GeneEditor.Clear();
            return;
        }

        GeneEditor.DisplayGenes(value.Genes);
    }

    public void Clear()
    {
        SelectedDna = null;
        ComparisonSymbolDropdown.Selected = -1;
        EnumDropdown.Selected = -1;
        DnaValueText.Text = null;
        GeneEditor.Clear();
        Hide();
    }

    /**
     * -1 if no selection
     */
    public int IndexOfSelectedGene()
    {
        return GeneEditor.Indexer.Index;
    }

    public bool RemoveSelectedGene()
    {
        if (!GeneEditor.Indexer.ValidIndex) return false;
        var selectedGeneIndex = GeneEditor.Indexer.Index;
        var id = SelectedDna.Genes[selectedGeneIndex].Id;

        SelectedDna.Genes.RemoveAt(selectedGeneIndex);
        GeneEditor.Indexer.Deselect();

        return DnaDb.Instance.RemoveGene(SelectedDna.Id, id);
        ;
    }

    public bool HasSelectedGene()
    {
        return GeneEditor.Indexer.ValidIndex;
    }

    public void CreateGene()
    {
        var gene = new Gene();
        SelectedDna.Genes.Add(gene);

        GeneEditor.DisplayGenes(SelectedDna.Genes);
    }
}