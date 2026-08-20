using System;
using Godot;
using Main.main.packages.plants.enums;
using Main.main.scripts.core.util;
using Main.main.scripts.model;
using Main.Source.main;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.scripts.scene;

public partial class UiGeneEdit : CloseWindow
{
    [Export] protected TextEdit GeneDisplay;
    [Export] protected TextEdit StrandDisplay;
    [Export] protected Button Push;
    private bool _stagedChanges = false;
    protected char[] ParamSeparators = { ':', ' ', '-', ';', '?' };

    public override void _Ready()
    {
        base._Ready();
        ComputerSceneData.Instance.WasUpdated += GetSceneData;
        Push.Pressed += PushChanges;
        GeneDisplay.TextChanged += () => { _stagedChanges = true; };
        StrandDisplay.TextChanged += () => { _stagedChanges = true; };
        GetSceneData();
    }

    public void GetSceneData()
    {
        var gene = ComputerSceneData.Instance.GetGeneDb();
        if (gene == null)
            return;
        GeneDisplay.Text = gene.ToString();
        StrandDisplay.Text = gene.GetParent().ToString();
    }

    public void PushChanges()
    {
        if (!_stagedChanges)
        {
            GD.Print("NO CHANGES FOUND");
            Console.Write("NO CHANGES FOUND");
            return;
        }

        var tempGene = GeneDbFromString(ComputerSceneData.Instance.GetGeneDb(), GeneDisplay.Text);
        if (tempGene != null)
            GD.Print(DbManager.Instance.ReplaceGene(tempGene));

        var tempStrand = StrandDbFromString(ComputerSceneData.Instance.GetStrandDb(), StrandDisplay.Text);
        if (tempStrand != null)
            GD.Print(DbManager.Instance.UpdateStrand(tempStrand));
    }

    /**
    *
    *
    *
    *  $"{PlantAction}:{Amount};{(int)Input}?{(int)Output}";
    */
    protected GeneDb GeneDbFromString(GeneDb gene, string s)
    {
        var param = s.Split(ParamSeparators);
        GeneDb result = gene.Clone();

        result.PlantAction = param[0];
        result.Amount = double.Parse(param[1]);
        result.Input = (EnumLibrary.Rt)int.Parse(param[2]);
        result.Input = (EnumLibrary.Rt)int.Parse(param[3]);


        return result; //STUB TODO
    }

    /**
     * return $"{(int)Type}:{Lo} {Operator} {Hi}";
     */
    protected StrandDb StrandDbFromString(StrandDb strand, string s)
    {
        var param = s.Split(ParamSeparators);
        StrandDb result = strand.Clone();

        result.Type = (EnumLibrary.Rt)int.Parse(param[0]);
        result.Lo = int.Parse(param[1]);
        result.Operator = param[2];
        result.Hi = int.Parse(param[3]);


        return result; //STUB TODO
    }
}