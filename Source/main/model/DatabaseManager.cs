using System;
using System.Collections.Generic;

namespace Main.Source.main.model;

using Godot;
using Microsoft.Data.Sqlite;

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
        command.Parameters.AddWithValue("@name", name);
        //command.ExecuteNonQuery();


        return command.ExecuteNonQuery() > 0;
    }
}

public class GeneDataContainer
{
    public string Name;
    public List<SubGeneDataContainer> SubGenes;
}

public class SubGeneDataContainer
{
    public string Name;
    public float Value;
}