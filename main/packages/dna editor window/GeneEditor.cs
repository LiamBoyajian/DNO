using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.model.Dna;

namespace Main.main.packages.dna_editor_window;

public partial class GeneEditor : PanelContainer
{
    [Export] public Indexer Indexer;
    [Export] public TextEdit GeneTextEditor;
    public List<Gene> Genes = null; //Live/unsafe references

    public override void _Ready()
    {
        base._Ready();
        if (Indexer == null) throw new Exception("Indexer is null");
        if (GeneTextEditor == null) throw new Exception("GeneTextEditor is null");
        Indexer.IndexChanged += DisplayGene;
        GeneTextEditor.TextChanged += () =>
        {
            if (Indexer.ValidIndex)
                Genes[Indexer.Index].ProteinName = GeneTextEditor.Text;
        };
    }

    private void DisplayGene(int index)
    {
        if (!Indexer.ValidIndex) return;
        Gene gene = Genes[index];
        GeneTextEditor.Text = gene.ProteinName;
    }


    //PUBLIC METHODS
    public void DisplayGenes(List<Gene> genes)
    {
        Indexer.SetMax(genes.Count);
        Genes = genes;
        GeneTextEditor.Text = "Select a gene to display its value";
    }

    public void CloseGenes()
    {
        Genes = null;
        Indexer.Clear();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event.IsAction("update_window"))
        {
        }
    }
}