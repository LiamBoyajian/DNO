using System;
using Godot;
using Main.main.packages.model.Dna;

namespace Main.main.packages.dna_editor_window;

public partial class DnaEditorWindow : Window
{
    [Export] protected NucleusDisplay NucleusDisplay { get; set; }
    [Export] protected DnaStrandDisplay DnaStrandDisplay { get; set; }
    [Export] protected Infobar InfobarDisplay { get; set; }

    [Export] protected int CurrentNucleus { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (NucleusDisplay == null) throw new Exception("NucleusDisplay is null");
        if (DnaStrandDisplay == null) throw new Exception("DnaStrandDisplay is null");
        if (InfobarDisplay == null) throw new Exception("InfobarDisplay is null");
        InfobarDisplay.BackPressed += () => NucleusDisplay.CloseOpenedElement();
        InfobarDisplay.IdChanged += PopulateItemList;

        NucleusDisplay.DnaStrandSelected += PopulateDnaDisplay;
    }

    public void PopulateItemList()
    {
        var requestedNucleusId = InfobarDisplay.GetId();
        if (CurrentNucleus == requestedNucleusId) return;
        var nucleus = DnaDb.Instance.GetNucleus(requestedNucleusId);
        if (nucleus == null) return;
        InfobarDisplay.SetName(nucleus.Name);
        NucleusDisplay.Populate(nucleus);
    }

    public void PopulateDnaDisplay()
    {
        InfobarDisplay.SetName(NucleusDisplay.SelectedDna.Name);
        DnaStrandDisplay.DisplayDna(NucleusDisplay.SelectedDna);
    }
}