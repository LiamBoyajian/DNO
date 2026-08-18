using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.plants.enums;
using Main.Source.main;
using SQLite;
using SQLiteNetExtensions.Attributes;
using AbstractPlant = Main.main.scripts.core.plants.AbstractPlant;

namespace Main.main.scripts.model;

public partial class GeneDb : GodotObject
{
    [PrimaryKey, AutoIncrement, Unique] public int Id { get; set; }

    [ForeignKey(typeof(StrandDb))] public int StrandId { get; set; }

    [ManyToOne(CascadeOperations = CascadeOperation.All)]
    public StrandDb Parent { get; set; }

    public EnumLibrary.Rt Input { get; set; }
    public EnumLibrary.Rt Output { get; set; }

    public double Amount { get; set; }

    public string PlantAction { get; set; }

    public override string ToString()
    {
        return $"{PlantAction}:{Amount};{(int)Input}?{(int)Output}";
    }

    public StrandDb GetParent()
    {
        return Parent;
    }

    public GeneDb Clone()
    {
        GeneDb result = new GeneDb();
        result.Id = Id;
        result.StrandId = StrandId;
        result.Parent = Parent;
        result.Input = Input;
        result.Output = Output;
        result.Amount = Amount;
        result.PlantAction = PlantAction;

        return result;
    }
}