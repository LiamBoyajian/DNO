using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Main.main.scripts.model;
using Main.Source.main;
using PlantGui = Main.main.scripts.core.util.PlantGui;
using System.Linq;

namespace Main.main.scripts.core.plants;

public abstract partial class AbstractMicrochipPlant(int dbId) : AbstractPlant
{
    [Export] protected PlantGui GuiManager;

    protected double HpToEnergyRatio;
    protected double HpEnergyValue;
    protected double GlucoseToEnergyRatio;
    protected PlantDb PlantInstance;
    protected int MaxStrands;

    [Export] protected int DbId = dbId;
    protected string DatabasePath = ProjectSettings.GlobalizePath("user://greenhouse.db");

    protected ContainPlant
        MyContainer; //TODO this should be replaced with some new implementation. Only used for testing and simple environmental control

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GuiManager ??= GetChild<PlantGui>(0);
    }

    public void Init()
    {
        ConnectToPlant();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //Console.Write(FrameSum);
        //
        if (IsAlive())
            Tick(delta);
    }

    public override void Tick(double delta)
    {
        FrameSum += delta;
        if (FrameSum < 2.0)
            return;

        ObtainGlucose();
        CheckHeadGenes();
        //THESE VALUES ARE TESTING CONSTANTS :: SHOULD BE REPLACED WITH SOME SET CONSTANT LATER
        Console.Write($"\nHEALTH REMAINING: {MyResources[Rt.Health].Amount} - ");
        Console.Write($"GLUCOSE REMAINING: {MyResources[Rt.Glucose].Amount} - ");
        Console.Write($"ENERGY REMAINING: {MyResources[Rt.Energy].Amount}");

        EnergyHp(HpToEnergyRatio, HpEnergyValue);


        Console.Write($"\nHEALTH REMAINING: {MyResources[Rt.Health].Amount} - ");
        Console.Write($"GLUCOSE REMAINING: {MyResources[Rt.Glucose].Amount} - ");
        Console.Write($"ENERGY REMAINING: {MyResources[Rt.Energy].Amount}");


        IsDeadThenDeadFrame();
        FrameSum = 0.0;
    }

    protected bool CheckHeadGenes()
    {
        if (PlantInstance == null)
            return false;
        if (ConnectToPlant() == null)
            return false;

        bool result = false;
        foreach (var strand in PlantInstance.Children)
        {
            var resourceAmount = Resources[strand.Type].Amount;

            var lo = strand.Lo;
            var hi = strand.Hi;

            bool headGeneActivated = strand.Operator switch
            {
                "==" => (int)resourceAmount == hi,
                "!=" => (int)resourceAmount != hi,

                "<" => lo < resourceAmount && resourceAmount < hi,
                "<=" => lo <= resourceAmount && resourceAmount <= hi,
                ">" => lo > resourceAmount && resourceAmount > hi,
                ">=" => lo >= resourceAmount && resourceAmount >= hi,
                _ => false
            };

            if (headGeneActivated)
            {
                result = true;

                foreach (var gene in strand.Children)
                {
                    this.StringToPlantAction(gene.PlantAction);
                    var del = this.StringToPlantAction(gene.PlantAction);
                    if (del is Func<Enum, double, double> temp)
                    {
                        temp(gene.Input, gene.Amount);
                    }
                    else if (del is Func<double, double> temp2)
                    {
                        temp2(gene.Amount);
                    }
                    else
                    {
                        throw new Exception($"Unknown PlantAction {gene.PlantAction}"); //idk 
                    }
                }
            }
        }

        return result;
    }

    public PlantDb ConnectToPlant()
    {
        if (DbId < 0)
            return null;

        return PlantInstance = DbManager.Instance?.GetPlant(DbId, true);
    }

    public virtual Delegate StringToPlantAction(string plantAction)
    {
        switch (plantAction)
        {
            case "Grow":
                return new Func<Enum, double, double>(Grow);
            case "Clean":
                return new Func<Enum, double, double>(Clean);
            case "Consume":
                return new Func<double, double>(Consume);
            default:
                return null;
        }
    }

    protected override bool GrowthUpdateFrame()
    {
        if (IsDeadThenDeadFrame())
            return false; //plant died :(

        return true;
    }

    protected override bool IsDeadThenDeadFrame()
    {
        if (IsAlive())
            return false;
        GuiManager.DeadFrame();
        return true;
    }

    protected double DrawWater(double amount)
    {
        if (!MyContainer.HasWater()) return 0;
        return AcceptWater(MyContainer.Water.Take(amount));
    }

    public ContainPlant LinkParentContainer(ContainPlant container)
    {
        MyContainer = container;
        return container;
    }


    protected override bool GetAtmosphRatio()
    {
        return ContainPlant.GetAtmosphRatio();
    }

    public void SetDbId(int id)
    {
        if (DbId < 0)
            DbId = id;
    }

    public override float GetSunLevel()
    {
        return MyContainer.GetSunlevel();
    }

    protected abstract double Photosynthesize();

    protected double ChangeResourceMax(Rt key, double change)
    {
        return Resources[key].ChangeMax(change);
    }
}