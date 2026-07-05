using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Main.main.scripts.model;

public class PlantDb()
{
    [PrimaryKey, AutoIncrement, Unique] public int Id { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<StrandDb> Children { get; set; }

    public string Species { get; set; }

    //public IEnumerable<Plant> Where()
    //{
    //    return null;
    //}
    public override string ToString()
    {
        return Id + Species;
    }
}