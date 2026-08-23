using System;
using Godot;
using Main.main.packages.model.Dna;

namespace Main.main.packages.dna_editor_window;

public partial class DnaEditorWindow : Window
{
    [Export] protected NucleusDisplay NucleusDisplay { get; set; }
    [Export] protected DnaStrandDisplay DnaStrandDisplay { get; set; }
    [Export] protected Infobar InfobarDisplay { get; set; }
    [Export] protected PackedScene WarningPopupScene { get; set; }

    protected WarningConfirmation WarnWindow;

    public override void _Ready()
    {
        base._Ready();
        if (NucleusDisplay == null) throw new Exception("NucleusDisplay is null");
        if (DnaStrandDisplay == null) throw new Exception("DnaStrandDisplay is null");
        if (InfobarDisplay == null) throw new Exception("InfobarDisplay is null");

        WarnWindow = WarningPopupScene.Instantiate() as WarningConfirmation;
        WarnWindow?.Hide();
        AddChild(WarnWindow);
        if (WarnWindow == null) throw new Exception("WarnWindow scene is invalid");

        InfobarDisplay.BackPressed += () =>
        {
            if (NucleusDisplay.SelectedNucleus == null)
            {
                Hide();
                QueueFree();
            }

            NucleusDisplay.CloseOpenedElement(true);
        };
        InfobarDisplay.IdChanged += PopulateItemList;

        InfobarDisplay.AddPressed += AddSelectedElement;
        InfobarDisplay.DeletePressed += ConfirmRemoval;
        InfobarDisplay.NameChanged += (string name) =>
        {
            InfobarDisplay.UnsavedChanges(true);
            NucleusDisplay.SetNameCurrentElement(name);
        };

        InfobarDisplay.ChangesSavedPressed += () =>
        {
            if (!InfobarDisplay.HasUnsavedChanges) return;
            InfobarDisplay.UnsavedChanges(false);
            if (NucleusDisplay.SelectedNucleus != null)
            {
                DnaDb.Instance.SyncNucleus(NucleusDisplay.SelectedNucleus);
            }
        };
        NucleusDisplay.Updated += UpdateDisplay;

        DnaStrandDisplay.ChangesMade += () => { InfobarDisplay.UnsavedChanges(true); };

        WarnWindow.ConfirmButton.Pressed += RemoveSelectedElement;
        WarnWindow.CancelButton.Pressed += () =>
        {
            WarnWindow.Clear();
            WarnWindow.Hide();
        };
    }

    private void ConfirmRemoval()
    {
        var subheadingText = "";
        if (InfobarDisplay.HasUnsavedChanges)
        {
            subheadingText += "Save changes, then ";
        }

        subheadingText += "Remove selected ";
        var text = "Name: ";
        if (DnaStrandDisplay.SelectedDna != null && !DnaStrandDisplay.HasSelectedGene())
        {
            subheadingText += "gene.";
            text += "At index: " + DnaStrandDisplay.IndexOfSelectedGene();
        }
        else if (NucleusDisplay.SelectedDna != null)
        {
            subheadingText += "dna.";
            text += NucleusDisplay.SelectedDna.Name;
        }
        else if (NucleusDisplay.SelectedChromosome != null)
        {
            subheadingText += "chromosome.";
            text += NucleusDisplay.SelectedChromosome.Name;
        }
        else if (NucleusDisplay.SelectedNucleus != null)
        {
            subheadingText += "nucleus.";
            text += NucleusDisplay.SelectedNucleus.Name;
        }

        WarnWindow.SubheadingDetails.Text = subheadingText;
        WarnWindow.WarningDetails.Text = text;
        WarnWindow.Popup();
    }

    private void RemoveSelectedElement()
    {
        if (!WarnWindow.Get("visible").AsBool()) return;

        WarnWindow.Clear();
        WarnWindow.Hide();

        if (DnaStrandDisplay.SelectedDna != null && DnaStrandDisplay.HasSelectedGene())
        {
            DnaStrandDisplay.RemoveSelectedGene();
        }
        else if (NucleusDisplay.SelectedDna != null)
        {
            RemoveSelectedDna();
        }
        else if (NucleusDisplay.SelectedChromosome != null)
        {
            RemoveSelectedChromosome();
        }
        else if (NucleusDisplay.SelectedNucleus != null)
        {
            RemoveSelectedNucleus();
        }
    }

    private void AddSelectedElement()
    {
        if (DnaStrandDisplay.SelectedDna != null && !DnaStrandDisplay.HasSelectedGene())
        {
            DnaStrandDisplay.CreateGene();
            InfobarDisplay.UnsavedChanges(true);
        }
        else
        {
            NucleusDisplay.CreateElement();
            InfobarDisplay.UnsavedChanges(true);
        }
    }

    public void PopulateItemList()
    {
        NucleusDisplay.Clear();
        DnaStrandDisplay.Hide();
        var requestedNucleusId = InfobarDisplay.GetId();
        var nucleus = DnaDb.Instance.GetNucleus(requestedNucleusId);
        InfobarDisplay.SetTitleSilent(nucleus?.Name);

        if (nucleus == null)
        {
            NucleusDisplay.Clear();
            return;
        }

        NucleusDisplay.Populate(nucleus);
    }

    public void UpdateDisplay()
    {
        var setName = "";

        if (NucleusDisplay.SelectedDna != null)
        {
            setName = NucleusDisplay.SelectedDna.Name;
            DnaStrandDisplay.DisplayDna(NucleusDisplay.SelectedDna);
        }
        else
        {
            DnaStrandDisplay.Clear();
            if (NucleusDisplay.SelectedChromosome != null)
            {
                setName = NucleusDisplay.SelectedChromosome.Name;
            }
            else if (NucleusDisplay.SelectedNucleus != null)
            {
                setName = NucleusDisplay.SelectedNucleus.Name;
                InfobarDisplay.SetIdSilent("" + NucleusDisplay.SelectedNucleus.Id);
            }
            else
            {
                setName = null;
                InfobarDisplay.SetIdSilent(null);
            }
        }

        InfobarDisplay.SetTitle(setName);
    }

    public void PushChanges()
    {
        if (NucleusDisplay.SelectedNucleus != null)
            DnaDb.Instance.SyncNucleus(NucleusDisplay.SelectedNucleus);
    }

    public void RemoveSelectedDna()
    {
        var id = NucleusDisplay.SelectedDna.Id;
        NucleusDisplay.CloseOpenedElement(true);
        DnaDb.Instance.RemoveDnaStrand(NucleusDisplay.SelectedChromosome.Id, id);
    }

    public void RemoveSelectedChromosome()
    {
        var id = NucleusDisplay.SelectedChromosome.Id;
        NucleusDisplay.CloseOpenedElement(true);
        DnaDb.Instance.RemoveChromosome(NucleusDisplay.SelectedNucleus.Id, id);
    }

    public void RemoveSelectedNucleus()
    {
        var id = NucleusDisplay.SelectedNucleus.Id;
        NucleusDisplay.CloseOpenedElement(true);
        DnaDb.Instance.RemoveNucleus(id);
    }
}