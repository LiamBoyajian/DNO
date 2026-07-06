using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Main.Source.main;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;

namespace Main.main.scripts.model;

//Do functions need parameters (only for quantity and resource type if they do): so probably 
public partial class DbManager : Node
{
    public string DbPath { get; private set; }

    private SQLiteConnection _db;

    public static DbManager Instance { get; private set; }

    public override void _Ready()
    {
        InitDb();
        DbPath = Path.Combine(Godot.ProjectSettings.GlobalizePath("user://"), "greenhouse.db");
        using var db = new SQLite.SQLiteConnection(DbPath);

        IEnumerable<PlantDb> iePlants = db.Table<PlantDb>();

        foreach (var p in iePlants)
        {
            Console.WriteLine("\r\n" + p);
        }

        Instance = this;
    }

    public void InitDb()
    {
        DbPath = Path.Combine(Godot.ProjectSettings.GlobalizePath("user://"), "greenhouse.db");

        _db = new SQLiteConnection(DbPath);

        _db.CreateTable<PlantDb>();
        _db.CreateTable<StrandDb>();
        _db.CreateTable<GeneDb>();
    }

    public PlantDb GetPlant(int id)
    {
        return _db.GetWithChildren<PlantDb>(id, true);
    }

    public List<PlantDb> GetPlantDbList(int start, int end)
    {
        var allPlants = _db.Table<PlantDb>().Where(p => (p.Id >= start && end > p.Id)).ToList();
        foreach (var p in allPlants)
        {
            _db.GetChildren(p);
        }

        return allPlants;
    }

    public List<PlantDb> GetPlantDbList()
    {
        return _db.GetAllWithChildren<PlantDb>();
    }

    public PlantDb[] GetPlantDbArray()
    {
        return GetPlantDbList().ToArray();
    }
}