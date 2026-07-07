using Godot;

namespace Main.main.scripts.core.util;

public partial class Computer : SceneData
{
    [Export] public int PlantId = 1;
    [Export] public int StrandId = -1;
    [Export] public int GeneId = -1;


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
}