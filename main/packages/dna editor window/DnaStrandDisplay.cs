using System;
using System.Collections.Generic;
using System.Linq;
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

    [Signal]
    public delegate void ChangesMadeEventHandler();

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

                var keyName = @enum.ToString();

                //In case duplicate enum names
                var i = 2;
                while (StringToEnum.ContainsKey(keyName))
                {
                    keyName = @enum.ToString() + i;
                    ++i;
                }

                EnumDropdown.AddIconItem(MemoryToDb.GetTextureFromEntry(new Entry(@enum)), keyName);
                StringToEnum.Add($"{keyName}", @enum);
            }
        }

        foreach (var comparisonType in Promoter.ComparisonKeys)
        {
            ComparisonSymbolDropdown.AddItem(comparisonType);
        }


        if (!GeneEditor.Indexer.ValidIndex) Hide();
        ComparisonSymbolDropdown.ItemSelected += (index) =>
        {
            if (SelectedDna == null) return;
            if (!Promoter.ComparisonKeys.Contains(ComparisonSymbolDropdown.GetItemText((int)index))) return;
            SelectedDna.Promoter.ComparisonSymbol = ComparisonSymbolDropdown.GetItemText((int)index);
            EmitSignal(nameof(ChangesMade));
        };
        EnumDropdown.ItemSelected += (index) =>
        {
            if (SelectedDna == null) return;
            if (!StringToEnum.TryGetValue(EnumDropdown.GetItemText((int)index), out var @enum)) return;
            GD.Print(SelectedDna.Promoter);
            SelectedDna.Promoter.Target = @enum;
            EmitSignal(nameof(ChangesMade));
        };
        DnaValueText.TextChanged += () =>
        {
            if (SelectedDna == null) return;
            var newText = DnaValueText.Text;
            SelectedDna.Promoter.IsPercent = newText.Contains('%');
            newText = newText.Replace("%", "");
            try
            {
                SelectedDna.Promoter.ComparisonValue = Convert.ToInt32(newText);
            }
            catch (Exception e)
            {
                SelectedDna.Promoter.ComparisonValue = 0;
            }

            EmitSignal(nameof(ChangesMade));
        };
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

        var index = 0;
        var promotersSymbol = SelectedDna?.Promoter.ComparisonSymbol;
        foreach (var comparisonType in Promoter.ComparisonKeys)
        {
            if (String.CompareOrdinal(promotersSymbol, comparisonType) == 0)
            {
                ComparisonSymbolDropdown.Selected = index;
                break;
            }

            ++index;
        }

        index = 0;
        foreach (var pair in StringToEnum)
        {
            if (Equals(SelectedDna?.Promoter.Target, pair.Value))
            {
                EnumDropdown.Selected = index;
                break;
            }

            ++index;
        }

        DnaValueText.Text = SelectedDna?.Promoter.ComparisonValue +
                            ((SelectedDna?.Promoter.IsPercent ?? false) ? "%" : "");

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