using System;
using System.Collections.Generic;
using Godot;
using Main.Source.main;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Main.main.scripts.model;

public partial class GeneDb : GodotObject
{
    [PrimaryKey, AutoIncrement, Unique] public int Id { get; set; }

    [ForeignKey(typeof(StrandDb))] public int StrandId { get; set; }

    [ManyToOne(CascadeOperations = CascadeOperation.All)]
    public StrandDb Parent { get; set; }

    public AbstractPlant.Rt Input { get; set; }
    public AbstractPlant.Rt Output { get; set; }

    public double Amount { get; set; }

    public string PlantAction { get; set; }
}