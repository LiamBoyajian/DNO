using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace Main.main.packages.model.Dna;

//Made with claude. 
public static class DnaHelperMethods
{
    public static string ConnectionPath = "main/packages/model/Dna/Nuclei.db";
    public static string SchemaPath = "main/packages/model/Dna/dna.sql";

    private static SqliteConnection _connection;

    /**
     * Shared connection reused across all helper method calls. Opened lazily
     * on first use and kept open for the lifetime of the process - helper
     * methods do NOT open/close a fresh connection per call, they all read
     * against this single connection so ConnectionPath is referenced
     * consistently. Test code (using isolated temp SQLite files) is expected
     * to manage its own per-call connections rather than go through this.
     */
    private static SqliteConnection Connection
    {
        get
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection($"Data Source={ConnectionPath};");
                _connection.Open();

                using (var pragmaCommand = _connection.CreateCommand())
                {
                    pragmaCommand.CommandText = "PRAGMA foreign_keys = ON;";
                    pragmaCommand.ExecuteNonQuery();
                }

                EnsureSchema(_connection);
            }

            return _connection;
        }
    }

    /**
     * Explicit initialization hook - call this once when the owning Godot
     * node is instantiated (see DnaDb._Ready()). Forces the shared
     * connection to open against ConnectionPath (creating an empty SQLite
     * file there if one doesn't exist yet) and builds the schema from
     * SchemaPath if it isn't already in place. Safe to call more than once.
     */
    public static void Initialize()
    {
        _ = Connection;
    }

    /**
     * Runs dna.sql against the connection to create any missing tables. Every
     * CREATE TABLE statement in dna.sql uses IF NOT EXISTS, so this doubles
     * as the "does the database exist" check - it's a no-op against a
     * database that already has the schema, and builds it from scratch
     * against a brand-new (empty) database file.
     */
    private static void EnsureSchema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = File.ReadAllText(SchemaPath);
        command.ExecuteNonQuery();
    }

    /**
     * Closes and disposes the current shared connection, if one is open, so
     * the next helper method call opens a fresh connection against whatever
     * ConnectionPath is currently set to. Call this after changing
     * ConnectionPath at runtime (e.g. switching save files) - otherwise the
     * shared connection keeps pointing at the old path.
     */
    public static void ResetConnection()
    {
        _connection?.Dispose();
        _connection = null;
    }

    public static Nucleus GetNucleus(int id, bool includeParent = false)
    {
        return GetNucleus(Connection, id, includeParent);
    }

    private static Nucleus GetNucleus(SqliteConnection connection, int id, bool includeParent)
    {
        var nucleus = GetNucleusRow(connection, id);
        if (nucleus == null) return null;

        nucleus.Chromosomes = GetChromosomes(connection, nucleus.Id);

        if (includeParent)
        {
            // Recurses upward one NucleusDisplay row at a time. Infinite recursion (e.g. a
            // ParentId cycle) is not guarded against here - that's expected to be
            // enforced wherever rows get added to the NucleusDisplay table.
            nucleus.Parent = GetNucleus(connection, nucleus.ParentId, true);
        }

        return nucleus;
    }

    private static Nucleus GetNucleusRow(SqliteConnection connection, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ParentId, Name FROM Nucleus WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Nucleus
        {
            Id = reader.GetInt32(0),
            ParentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            Name = reader.IsDBNull(2) ? "" : reader.GetString(2)
        };
    }

    public static Chromosome GetChromosome(int id)
    {
        return GetChromosome(Connection, id);
    }

    private static Chromosome GetChromosome(SqliteConnection connection, int id)
    {
        var chromosome = GetChromosomeRow(connection, id);
        if (chromosome == null) return null;

        chromosome.DnaStrands = GetDnaStrands(connection, chromosome.Id);

        return chromosome;
    }

    private static Chromosome GetChromosomeRow(SqliteConnection connection, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ParentId, Name FROM Chromosome WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Chromosome
        {
            Id = reader.GetInt32(0),
            ParentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
            Name = reader.IsDBNull(2) ? "" : reader.GetString(2)
        };
    }

    public static DnaStrand GetDnaStrand(int id)
    {
        return GetDnaStrand(Connection, id);
    }

    private static DnaStrand GetDnaStrand(SqliteConnection connection, int id)
    {
        var strand = GetDnaStrandRow(connection, id);
        if (strand == null) return null;

        strand.Genes = GetGenes(connection, strand.Id);

        return strand;
    }

    private static DnaStrand GetDnaStrandRow(SqliteConnection connection, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, EnumType, Ordinal, ComparisonType FROM Dna WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new DnaStrand
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
            Promoter = new Promoter
            {
                // See the equivalent note in GetDnaStrands below - Target/Ordinal are
                // left unset for the same reasons.
                ComparisonType = reader.IsDBNull(4) ? "" : reader.GetString(4)
            }
        };
    }

    public static Gene GetGene(int id)
    {
        return GetGene(Connection, id);
    }

    private static Gene GetGene(SqliteConnection connection, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Gene WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Gene
        {
            Id = reader.GetInt32(0)
            // ProteinName has no matching column in the Gene table - see the note in
            // GetGenes below.
        };
    }

    private static List<Chromosome> GetChromosomes(SqliteConnection connection, int nucleusId)
    {
        var chromosomes = new List<Chromosome>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT Id, ParentId, Name FROM Chromosome WHERE ParentId = @ParentId;";
            command.Parameters.AddWithValue("@ParentId", nucleusId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                chromosomes.Add(new Chromosome
                {
                    Id = reader.GetInt32(0),
                    ParentId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    Name = reader.IsDBNull(2) ? "" : reader.GetString(2)
                });
            }
        }

        foreach (var chromosome in chromosomes)
        {
            chromosome.DnaStrands = GetDnaStrands(connection, chromosome.Id);
        }

        return chromosomes;
    }

    private static List<DnaStrand> GetDnaStrands(SqliteConnection connection, int chromosomeId)
    {
        var strands = new List<DnaStrand>();

        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                "SELECT Id, Name, EnumType, Ordinal, ComparisonType FROM Dna WHERE ParentId = @ParentId;";
            command.Parameters.AddWithValue("@ParentId", chromosomeId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                strands.Add(new DnaStrand
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Promoter = new Promoter
                    {
                        // NOTE: Target is a System.Enum in Promoter.cs, but the Dna table only
                        // stores the enum's type name as a string (EnumType). Turning that
                        // string into an actual enum value needs reflection (Type.GetType +
                        // Enum.Parse), which needs "using System;" - not added here since it's
                        // a new import. Left unset for now; see note below.
                        ComparisonType = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        // Ordinal (index 3) also has no home on Promoter right now - see note below.
                    }
                });
            }
        }

        foreach (var strand in strands)
        {
            strand.Genes = GetGenes(connection, strand.Id);
        }

        return strands;
    }

    private static List<Gene> GetGenes(SqliteConnection connection, int dnaId)
    {
        var genes = new List<Gene>();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id FROM Gene WHERE ParentId = @ParentId;";
        command.Parameters.AddWithValue("@ParentId", dnaId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            genes.Add(new Gene
            {
                Id = reader.GetInt32(0)
                // ProteinName has no matching column in the Gene table (Id, ParentId only),
                // so it's left unset - see note below.
            });
        }

        return genes;
    }

    public static bool RemoveNucleus(int id)
    {
        return RemoveRow(Connection, "Nucleus", id);
    }

    public static bool RemoveChromosome(int id)
    {
        return RemoveRow(Connection, "Chromosome", id);
    }

    public static bool RemoveDnaStrand(int id)
    {
        return RemoveRow(Connection, "Dna", id);
    }

    public static bool RemoveGene(int id)
    {
        return RemoveRow(Connection, "Gene", id);
    }

    /**
     * Deletes a single row by Id from the given table. Child rows (e.g.
     * removing a Chromosome also removes its DnaStrand and Gene rows) are
     * cleaned up automatically by the schema's ON DELETE CASCADE - that only
     * takes effect because PRAGMA foreign_keys = ON is set when the shared
     * Connection is opened above. tableName is only ever passed as one of
     * the hardcoded literals in the four methods above, never user input, so
     * building the DELETE statement with string interpolation here is safe
     * (table names can't be parameterized as SQL parameters anyway).
     */
    private static bool RemoveRow(SqliteConnection connection, string tableName, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {tableName} WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        return command.ExecuteNonQuery() > 0;
    }
}