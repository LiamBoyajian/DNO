using System.Collections.Generic;
using Godot;
using Main.Source.main;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Main.main.scripts.model;

public partial class StrandDb : GodotObject
{
    [PrimaryKey, AutoIncrement, Unique] public int Id { get; set; }

    [ForeignKey(typeof(PlantDb))] public int PlantDbId { get; set; }

    [ManyToOne(CascadeOperations = CascadeOperation.All)]
    public PlantDb Parent { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<GeneDb> Children { get; set; }

    public int Lo { get; set; }
    public int Hi { get; set; }

    public AbstractPlant.Rt Type { get; set; }
    public string Operator { get; set; }

    public GeneDb[] GetChildren()
    {
        return Children.ToArray();
    }

    public override string ToString()
    {
        return $"{Type}: {Lo} {Operator} {Hi}";
    }
}