using System;
using Godot;
using Main.main.scripts.core.util;
using Main.main.scripts.model;

// Adjust based on your actual DbManager namespace

namespace Main.main.scripts.scene;

public partial class UiGeneDisplay : CloseWindow // Assuming base class is CloseWindow
{
    [Export] public VBoxContainer DnaStrandContainer { get; set; }
    [Export] public PackedScene StrandPanelBg { get; set; }
    [Export] public PackedScene GeneContainerTemplate { get; set; }
    [Export] public PackedScene GeneTemplate { get; set; }
    public ComputerSceneData SceneData { get; set; } // We can use the strong type directly in C#!
    [Export] public ButtonGroup GeneButtonGroup { get; set; }

    public override void _Ready()
    {
        base._Ready();


        SceneData = ComputerSceneData.Instance;


        // Reference validations
        if (DnaStrandContainer == null) GD.PushError("DnaStrandContainer has no reference.");
        if (StrandPanelBg == null) GD.PushError("StrandPanelBg has no reference.");
        if (GeneContainerTemplate == null) GD.PushError("GeneContainerTemplate has no reference.");
        if (GeneTemplate == null) GD.PushError("GeneTemplate has no reference.");

        RefreshStrands();


        //Signals
        if (GeneButtonGroup != null)
        {
            GeneButtonGroup.Pressed += OnGenePressed;
        }

        ComputerSceneData.Instance.WasUpdated += RefreshStrands;
    }

    public void RefreshStrands()
    {
        foreach (Node child in DnaStrandContainer.GetChildren())
        {
            child.QueueFree();
        }

        // Fetching plant via DbManager instance
        var tempHeadRoot = ComputerSceneData.Instance;
        if (tempHeadRoot != null)
        {
            var plant = DbManager.Instance.GetPlant(tempHeadRoot.PlantId, true);
            if (plant != null)
            {
                DisplayStrandsToEditor(plant.GetChildren());
            }
        }
    }

    private void OnWindowCloseRequested()
    {
        Hide();
    }

    private void DisplayStrandsToEditor(StrandDb[] strands)
    {
        float colorWeight = 0f;

        foreach (var strand in strands)
        {
            if (strand == null) continue;

            var tempPanel = StrandPanelBg.Instantiate<Control>();
            var tempGeneCont = GeneContainerTemplate.Instantiate<Control>();

            tempPanel.AddChild(tempGeneCont);
            DnaStrandContainer.AddChild(tempPanel);

            var genes = strand.GetChildren();
            foreach (var gene in genes)
            {
                if (gene == null) continue;

                var tempGene = GeneTemplate.Instantiate<Button>(); // Assumed Button or BaseButton based on toggle_mode

                tempGene.ThemeTypeVariation = "GeneButton";

                int plantId = ComputerSceneData.Instance.PlantId;
                string identifier = $"{plantId}.{strand.Id}.{gene.Id}";

                tempGene.Name = identifier;
                tempGene.EditorDescription = identifier;

                tempGeneCont.AddChild(tempGene);
                tempGene.ButtonGroup = GeneButtonGroup;
                tempGene.ToggleMode = true;

                float alphaC = 0.9f;
                Color firstColor = new Color(0.78431374f, 0.21568628f, 0.21568628f, alphaC);
                Color secondColor = new Color(0.15686275f, 0.3137255f, 0.5882353f, alphaC);

                // Duplicate stylebox and cast it safely
                if (tempGene.GetThemeStylebox("normal") is StyleBoxFlat normalStyle)
                {
                    var ttt = (StyleBoxFlat)normalStyle.Duplicate();
                    ttt.BgColor = firstColor.Lerp(secondColor, colorWeight);
                    tempGene.AddThemeStyleboxOverride("normal", ttt);
                }

                if (tempGene.GetThemeStylebox("hover") is StyleBoxFlat hoverStyle)
                {
                    hoverStyle.BgColor = new Color(1f, 1f, 1f, 0.8f);
                }

                colorWeight += 0.33f;
            }
        }
    }

    private void OnGenePressed(BaseButton button)
    {
        string name = button.EditorDescription;
        string[] pieces = name.Split('.');

        if (pieces.Length >= 3 && int.TryParse(pieces[0], out int p0) && int.TryParse(pieces[1], out int p1) &&
            int.TryParse(pieces[2], out int p2))
        {
            var headNode = ComputerSceneData.Instance;
            headNode.SetData(p0, p1, p2); // Assuming SetData maps to these fields

            GD.Print(headNode.GetPlantStringData());
        }
    }
}