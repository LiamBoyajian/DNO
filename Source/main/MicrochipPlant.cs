using System;
using Dapper;
using Godot;
using Microsoft.Data.Sqlite;

namespace Main.Source.main;

public partial class MicrochipPlant : AbstractPlant
{
    [Export] private PlantGui _guiManager;

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

        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();

        var query = $"SELECT * FROM genes g JOIN dna_strands d WHERE g.strand_id = d.id";


        var result = connection.Query(query, new { plant_id = DbId });

        foreach (var n in result)
            Console.WriteLine($"n.id: {n.id}, n.name: {n.name}, n.thing: {n.code}");

        //RunGene();
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

        //THESE VALUES ARE TESTING CONSTANTS :: SHOULD BE REPLACED WITH SOME SET CONSTANT LATER
        DrawWater(MyResources[Rt.Health].Max * .1); //SHOULD BE CONTROLLED BY GENES
        EnergyHp(.1); //ENERGY CONSUMPTION IS CONSISTENT BUT CHANGED BY HORMONES (ADDITION REQUIRED)


        Console.Write($"\nHEALTH REMAINING: {MyResources[Rt.Health].Amount}");
        Console.Write($"\nENERGY REMAINING: {MyResources[Rt.Energy].Amount}");


        IsDeadThenDeadFrame();
        FrameSum = 0.0;
    }

    protected override bool GetAtmosphRatio()
    {
        throw new NotImplementedException();
    }

    protected override void Store()
    {
        throw new NotImplementedException();
    }

    protected override void Retrieve()
    {
        throw new NotImplementedException();
    }

    protected override void Consume()
    {
        throw new NotImplementedException();
    }

    protected override void Grow()
    {
        throw new NotImplementedException();
    }

    protected override void Perform()
    {
        throw new NotImplementedException();
    }

    protected override void Cycle()
    {
        throw new NotImplementedException();
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
     * TODO: STUB
     */
    protected void RunAll()
    {
        return;
    }

    /**
     *
     * EX: ENERGY::10<80::CONSUME
     */
    protected void RunGene(string gene)
    {
        var components = gene.Split("::");

        Rt head = Rt.DamagedCells;
        int first = -1;
        int second = -1;
        foreach (var component in components)
        {
            //Rt
            Enum.TryParse(component, true, out head);
            //Context
            if (component.Contains("<"))
                foreach (var value in component.Split("<"))
                {
                    if (first == -1)
                    {
                        int.TryParse(value, out first);
                    }
                    else
                    {
                        int.TryParse(value, out second);
                    }
                }
            //Action
        }

        if (head != Rt.DamagedCells)
            Console.WriteLine($"\n{head} - {first} - {second}");
    }
}