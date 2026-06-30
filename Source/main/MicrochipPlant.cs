using System;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

namespace Main.Source.main;

public abstract partial class MicrochipPlant : AbstractPlant
{
    [Export] private PlantGui _guiManager;

    protected double HpToEnergyRatio;
    protected double HpEnergyValue;
    protected double GlucoseToEnergyRatio;

    protected ContainPlant
        MyContainer; //TODO this should be replaced with some new implementation. Only used for testing and simple environmental control

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        DbId = 1;

        _guiManager ??= GetChild<PlantGui>(0);
        Console.Write("GuiManger set to: " + _guiManager);

        if (GetParent() is ContainPlant)
            MyContainer = (ContainPlant)GetParent();
        else
            throw new InvalidOperationException($"{this} is not in a ContainPlant object.");

        ConnectPlantToDatabase();
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
        if (FrameSum < 5.0)
            return;

        Photosynthesize();
        ConnectToGenes();
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

    public void ConnectToGenes()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();

        var query = $"SELECT * FROM genes g JOIN dna_strands d WHERE g.strand_id = d.id";


        var result = connection.Query(query, new { plant_id = DbId });

        foreach (var n in result)
        {
            //Console.WriteLine($"n.id: {n.id}, n.name: {n.name}, n.thing: {n.code}");
            RunGene(n.code);
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
        _guiManager.DeadFrame();
        return true;
    }

    protected double DrawWater(double amount)
    {
        if (!MyContainer.HasWater()) return 0;
        return AcceptWater(MyContainer.Water.Take(amount));
    }

    /**
     *
     *
     */
    protected void RunGene(string gene)
    {
        var components = gene.Split("::");

        Rt? head = null;
        int first = int.MinValue;
        int second = int.MaxValue;
        string funcName = null;

        foreach (var component in components)
        {
            //Rt
            if (Enum.TryParse(component, true, out Rt temp))
            {
                head = temp;
                continue;
            }

            //Context
            var bounds = component.Split('<', '-');
            if (component.Contains("<") || component.Contains("-"))
            {
                if (bounds.Length > 1)
                {
                    int.TryParse(bounds[0], out first);
                    int.TryParse(bounds[1], out second);
                }
                else
                {
                    if (component[0] == '<')
                    {
                        int.TryParse(bounds[0], out second);
                    }
                    else
                    {
                        int.TryParse(bounds[0], out first);
                    }
                }

                continue;
            }


            //Action
            funcName = component;
        }

        if (head != null)
        {
            if (MyResources[(Rt)head].Amount >= first && MyResources[(Rt)head].Amount <= second)
            {
                if (funcName != null)
                {
                    RunString(funcName, head, first, second);
                }
            }
        }
    }

    protected bool RunString(string funcName, Enum rt, double first, double second)
    {
        Console.WriteLine($"\nWITHIN BOUNDS:    {rt} - {first} - {second} - {funcName}");
        switch (funcName.ToLower())
        {
            case "grow":
                Grow(rt);
                break;
            case "consume":
                Consume();
                break;
            case "clean":
                Clean(rt);
                break;
            default:
                throw new InvalidOperationException($"Unknown function: {funcName}");
                return false;
                break;
        }

        return true;
    }

    /**
     * TODO - get from plant container
     */
    protected override bool GetAtmosphRatio()
    {
        return ContainPlant.GetAtmosphRatio();
    }

    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    protected double Photosynthesize()
    {
        var sunlevel = (float)MyContainer.GetSunlevel();
        const float oxygenByproductRatio = 6f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            ((Math.Max(Resources[Rt.H2O].Amount, Resources[Rt.Co2].Amount) * sunlevel) / waterAndCo2Min);
        Resources[Rt.Glucose].Give(glucoseGenerated);
        Resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        Resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        Resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);

        return glucoseGenerated;
    }
}