using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Main.Source.main;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;

namespace Main.main.scripts.model;

//Do functions need parameters (only for quantity and resource type if they do): so probably yes

public partial class DatabaseManager() : Node
{
    public string DbPath { get; private set; }

    private SQLiteConnection _db;

    public static DatabaseManager Instance { get; private set; }

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

    public static PlantDb GetPlantDb(int id)
    {
        return Instance._db.GetWithChildren<PlantDb>(id, true);
    }
    ///**
    // *
    // *
    // */
    //protected void FromString(string gene)
    //{
    //    var components = gene.Split("::");
//
    //    AbstractPlant.Rt? head = null;
    //    int first = int.MinValue;
    //    int second = int.MaxValue;
    //    string funcName = null;
//
    //    foreach (var component in components)
    //    {
    //        //Rt
    //        if (Enum.TryParse(component, true, out AbstractPlant.Rt temp))
    //        {
    //            head = temp;
    //            continue;
    //        }
//
    //        //Context
    //        var bounds = component.Split('<', '-');
    //        if (component.Contains("<") || component.Contains("-"))
    //        {
    //            if (bounds.Length > 1)
    //            {
    //                int.TryParse(bounds[0], out first);
    //                int.TryParse(bounds[1], out second);
    //            }
    //            else
    //            {
    //                if (component[0] == '<')
    //                {
    //                    int.TryParse(bounds[0], out second);
    //                }
    //                else
    //                {
    //                    int.TryParse(bounds[0], out first);
    //                }
    //            }
//
    //            continue;
    //        }
//
//
    //        //Action
    //        funcName = component;
    //    }
//
    //    if (head != null)
    //    {
    //        if (MyResources[(AbstractPlant.Rt)head].Amount >= first && MyResources[(AbstractPlant.Rt)head].Amount <= second)
    //        {
    //            //INCLUSIVE EXCLUSIVE BASED ON SYMBOL '-' '<'
    //            if (funcName != null)
    //            {
    //                RunString(funcName, head, first, second);
    //            }
    //        }
    //    }
    //}
//
    //protected abstract void SubRun(string funcName);
    //protected bool RunString(string funcName, Enum rt, double first, double second)
    //{
    //    Console.WriteLine($"\nWITHIN BOUNDS:    {rt} - {first} - {second} - {funcName}");
    //    switch (funcName.ToLower())
    //    {
    //        case "grow":
    //            Grow(rt);
    //            break;
    //        case "consume":
    //            Consume();
    //            break;
    //        case "clean":
    //            Clean(rt);
    //            break;
    //        default:
    //            SubRun(funcName);
    //            //throw new InvalidOperationException($"Unknown function: {funcName}");
    //            return false;
    //            break;
    //    }
//
    //    return true;
    //}
} //