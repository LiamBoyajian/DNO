using System;
using Godot;
using Main.main.scripts.model;
using Main.Source.main;
using Microsoft.VisualBasic.CompilerServices;

namespace Main.main.scripts.core.util;

public partial class ComputerSceneData : SceneData<ComputerSceneData>
{
    [Export] public int PlantId = 1;
    [Export] public int StrandId = -1;
    [Export] public int GeneId = -1;

    protected PlantDb Plant = null;
    protected StrandDb Strand = null;
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
        if (!SilentUpdateSelectedGene())
            return null;
        return Gene;
    }

    public StrandDb GetStrandDb()
    {
        if (!SilentUpdateSelectedStrand())
            return null;
        return Strand;
    }

    public PlantDb GetPlantDb()
    {
        if (!SilentUpdateSelectedPlant())
            return null;
        return Plant;
    }

    protected bool SilentUpdateSelectedPlant()
    {
        if (!HasPlantId())
            return false;

        Plant = DbManager.Instance?.GetPlant(PlantId, true);
        return true;
    }

    protected bool SilentUpdateSelectedStrand()
    {
        if (!HasPlantId() || !HasStrandId())
            return false;

        Strand = DbManager.Instance?.GetStrand(StrandId, true);
        return true;
    }

    protected bool SilentUpdateSelectedGene()
    {
        if (!HasPlantId() || !HasStrandId() || !HasGeneId())
            return false;

        Gene = DbManager.Instance?.GetGene(GeneId);
        return true;
    }

    protected bool UpdateSelected()
    {
        var result = SilentUpdateSelectedPlant() || SilentUpdateSelectedStrand() || SilentUpdateSelectedGene();
        if (result) base.Updated();
        return result;
    }

    public bool UpdateSelectedGene(GeneDb gene)
    {
        if (gene == null)
            return false;
        var result = DbManager.Instance.ReplaceGene(gene);
        base.Updated();
        return result;
    }
}