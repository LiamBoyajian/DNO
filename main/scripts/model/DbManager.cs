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


    public List<PlantDb> GetPlantDbList(int start, int end)
    {
        var allPlants = _db.Table<PlantDb>().Where(p => (p.Id >= start && end > p.Id)).ToList();
        foreach (var p in allPlants)
        {
            _db.GetChildren(p);
        }

        return allPlants;
    }

    /**
     * returns all plants in the database; (TODO might return null)
     */
    public List<PlantDb> GetPlantDbList(bool recursive)
    {
        if (recursive)
            return _db.GetAllWithChildren<PlantDb>();
        return _db.Table<PlantDb>().ToList();
    }


    /**
     * returns all plants in the database as an array; (TODO might return null)
     */
    public PlantDb[] GetPlantDbArray()
    {
        return GetPlantDbList(false)?.ToArray();
    }

    /**
    * returns the plant with that id; otherwise null
    * includes children
    */
    public PlantDb GetPlant(int plantId)
    {
        return _db.FindWithChildren<PlantDb>(plantId, true);
    }

    /**
    * returns the strand with that id; otherwise null
    * includes children
    */
    public StrandDb GetStrand(int strandId)
    {
        return _db.FindWithChildren<StrandDb>(strandId, true);
    }

    /**
    * returns the gene with that id; otherwise null
    * includes children
    */
    public GeneDb GetGene(int geneId)
    {
        return _db.FindWithChildren<GeneDb>(geneId, true);
    }

    /**
     * returns the plant with that id; otherwise null
     * does not include children
     */
    public PlantDb HasPlant(int plantId)
    {
        return _db.Find<PlantDb>(plantId);
    }

    /**
     * returns the strand with that id; otherwise null
     * does not include children
     */
    public StrandDb HasStrand(int strandtId)
    {
        return _db.Find<StrandDb>(strandtId);
    }

    /**
     * returns the gene with that id; otherwise null
     * does not include children
     */
    public GeneDb HasGene(int geneId)
    {
        return _db.Find<GeneDb>(geneId);
    }


    /**
     * Primary key must match target gene
     *
     * return: replaced gene original gene if target is not found
     */
    public bool ReplaceGene(GeneDb gene)
    {
        if (_db.Find<GeneDb>(gene.Id) != null)
            throw new InvalidOperationException("No gene with that id");

        return _db.Update(gene) > 0;
    }
}