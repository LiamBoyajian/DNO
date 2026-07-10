using System;
using Godot;
using Main.main.scripts.model;

namespace Main.main.scripts.core.util;

public partial class ComputerSceneData : SceneData<ComputerSceneData>
{
    [Export] public int PlantId = 1;
    [Export] public int StrandId = -1;
    [Export] public int GeneId = -1;
    protected GeneDb Gene = null;

    public static ComputerSceneData Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PushWarning($"Multiple instances of ComputerSceneData detected. Destroying extra.");
            QueueFree();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetData(int plant, int strand, int gene)
    {
        PlantId = plant;
        StrandId = strand;
        GeneId = gene;
        Updated();
    }

    public bool HasPlantId()
    {
        return PlantId > 0;
    }

    public bool HasStrandId()
    {
        return StrandId > 0;
    }

    public bool HasGeneId()
    {
        return GeneId > 0;
    }

    public string GetPlantStringData()
    {
        return $"{PlantId}, {StrandId}, {GeneId}";
    }


    public GeneDb GetGeneDb()
    {
        SilentUpdateSelectedGene();
        return Gene; //TODO make this a clone
    }

    protected bool SilentUpdateSelectedGene()
    {
        if (!HasPlantId() || !HasStrandId() || !HasGeneId())
            return false; //throw new InvalidOperationException("Not all ids are set");

        Gene = DbManager.Instance?.GetGene(GeneId);
        return true;
    }

    protected bool UpdateSelectedGene()
    {
        bool result = SilentUpdateSelectedGene();
        base.Updated();
        return true;
    }

    protected bool UpdateSelectedGene(GeneDb gene)
    {
        if (gene == null)
            return false;
        var result = DbManager.Instance.ReplaceGene(gene);
        base.Updated();
        return result;
    }
}