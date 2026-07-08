using System;
using Godot;
using Main.main.scripts.model;

namespace Main.main.scripts.core.util;

public partial class Computer : SceneData
{
    [Export] public int PlantId = 1;
    [Export] public int StrandId = -1;
    [Export] public int GeneId = -1;
    protected GeneDb Gene = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
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
        UpdateSelectedGene();
        return Gene; //TODO make this a clone
    }

    protected bool UpdateSelectedGene()
    {
        if (!HasPlantId() || !HasStrandId() || !HasGeneId())
            return false; //throw new InvalidOperationException("Not all ids are set");

        Gene = DbManager.Instance?.GetGene(GeneId);
        return true;
    }

    protected bool UpdateSelectedGene(GeneDb gene)
    {
        if (gene == null)
            return false;

        return DbManager.Instance.ReplaceGene(gene);
    }
}