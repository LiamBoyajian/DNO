using Godot;
using Main.main.scripts.core.util;

namespace Main.main.scripts.scene;

public partial class UiGeneEdit : CloseWindow
{
    [Export] protected TextEdit GeneDisplay;
    [Export] protected TextEdit StrandDisplay;

    public override void _Ready()
    {
        base._Ready();
        ComputerSceneData.Instance.WasUpdated += GetSceneData;
    }

    public void GetSceneData()
    {
        var gene = ComputerSceneData.Instance.GetGeneDb();
        if (gene == null)
            return;
        GeneDisplay.Text = gene.ToString();
        StrandDisplay.Text = gene.GetParent().ToString();
    }
}