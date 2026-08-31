using Godot;
using Main.main.packages.dna_editor_window;

namespace Main.main.packages.machines;

public partial class DnaSythesizer : AbstractMachineToPopup
{
    [Export] public scenes.Lab_Main.MicrochipPrinter MicrochipPrinter;

    protected override bool InstantiateFromPopup(Node node)
    {
        if (node is not DnaEditorWindow dnaEditorWindow) return false;

        dnaEditorWindow.NucleusIdSet += (int id) => { MicrochipPrinter.DbId = id; };

        return true;
    }
}