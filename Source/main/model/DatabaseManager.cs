using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Source.main.model;

using Godot;
using Microsoft.Data.Sqlite;
using Dapper;

public partial class DatabaseManager : Node
{
    private string _databasePath = ProjectSettings.GlobalizePath("user://greenhouse.db");
    private string _schemaPath = "Source/main/model/schema.sql";

    private void InitializeDatabase()
    {
        if (!FileAccess.FileExists(_schemaPath))
        {
            GD.PrintErr($"Database initialization failed: Schema file not found at {_schemaPath}");
            return;
        }

        try
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.ExecuteNonQuery();

            GD.Print("Database initialized successfully from schema.sql.");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to execute database schema: {ex.Message}");
        }
    }

    public override void _Ready()
    {
        InitializeDatabase();
    }

    public bool AddPlant(string name)
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO plants (name) VALUES (@name)";
        command.Parameters.Add("@name", SqliteType.Text).Value = name;
        //command.ExecuteNonQuery();


        return command.ExecuteNonQuery() > 0;
    }

    private Godot.Collections.Array<Godot.Collections.Dictionary> GetPlantGenes(int plantId)
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath}");

        var query = """
                    SELECT id, plant_id, gene_name 
                    FROM dna_strands 
                    WHERE plant_id = @plant_id 
                    ORDER BY id ASC;
                    """;

        connection.Open();
        using var command = connection.CreateCommand();

        command.CommandText = query;
        command.Parameters.Add("@plant_id", SqliteType.Integer).Value = plantId;

        //command.ExecuteNonQuery();


        var dnaList = connection.Query(query, new { plant_id = plantId });
        var godotArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
        foreach (var row in dnaList)
        {
            var godotDict = new Godot.Collections.Dictionary();
            godotDict.Add("id", row["id"]);
            godotDict.Add("gene_name", row["gene_name"]);
        }


        return godotArray;
    }
}

public partial class GeneDataContainer : Node
{
    public string Name;

    public List<SubGeneDataContainer> SubGenes;
}

public class SubGeneDataContainer
{
    public string Name;
    public float Value;
}