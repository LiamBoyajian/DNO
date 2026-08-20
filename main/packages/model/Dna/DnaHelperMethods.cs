using System;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.IO;

namespace Main.main.packages.model.Dna;

public static class DnaHelperMethods
{
    public static string ConnectionPath = "main/packages/model/Dna/Nuclei.db";
    public static string SchemaPath = "main/packages/model/Dna/dna.sql";

    private static SqliteConnection _connection;

    /**
     * Shared connection reused across all helper method calls. Opened lazily
     * on first use and kept open for the lifetime of the process. Test code
     * is expected to manage its own per-call connections rather than go
     * through this.
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
     * Explicit initialization hook — call this once when the owning Godot
     * node is instantiated (see DnaDb._Ready()). Safe to call more than once.
     */
    public static void Initialize()
    {
        _ = Connection;
    }

    /**
     * Runs dna.sql against the connection to create any missing tables. Every
     * CREATE TABLE uses IF NOT EXISTS, so this is a no-op on an already-built
     * database.
     */
    private static void EnsureSchema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = File.ReadAllText(SchemaPath);
        command.ExecuteNonQuery();
    }

    /**
     * Closes and disposes the current shared connection so the next call
     * opens a fresh one against whatever ConnectionPath is currently set to.
     */
    public static void ResetConnection()
    {
        _connection?.Dispose();
        _connection = null;
    }

    // -------------------------------------------------------------------------
    // GET — public entry points
    // -------------------------------------------------------------------------

    /**
     * Loads a full Nucleus → Chromosome → DnaStrand → Gene tree by Id.
     * Pass includeParent = true to also walk upward through Nucleus.ParentId.
     *
     * Within a single call, any Chromosome/DnaStrand/Gene that is shared
     * across multiple parents is returned as the same C# instance (identity
     * map scoped to this call). Edits to that instance will therefore be
     * visible from every parent that references it in the returned graph.
     */
    public static Nucleus GetNucleus(int id, bool includeParent = false)
    {
        var chromosomeCache = new Dictionary<int, Chromosome>();
        var strandCache = new Dictionary<int, DnaStrand>();
        var geneCache = new Dictionary<int, Gene>();

        return GetNucleus(Connection, id, includeParent, chromosomeCache, strandCache, geneCache);
    }

    /**
     * Loads a single Chromosome by Id, with its DnaStrands (and their Genes)
     * populated. Shared children within this call are the same instance.
     */
    public static Chromosome GetChromosome(int id)
    {
        var strandCache = new Dictionary<int, DnaStrand>();
        var geneCache = new Dictionary<int, Gene>();

        return GetChromosome(Connection, id, strandCache, geneCache);
    }

    /**
     * Loads a single DnaStrand by Id, with its Genes populated.
     * Shared genes within this call are the same instance.
     */
    public static DnaStrand GetDnaStrand(int id)
    {
        var geneCache = new Dictionary<int, Gene>();

        return GetDnaStrand(Connection, id, geneCache);
    }

    /**
     * Loads a single Gene by Id.
     */
    public static Gene GetGene(int id)
    {
        return GetGene(Connection, id);
    }

    // -------------------------------------------------------------------------
    // GET — private implementation
    // -------------------------------------------------------------------------

    private static Nucleus GetNucleus(
        SqliteConnection connection,
        int id,
        bool includeParent,
        Dictionary<int, Chromosome> chromosomeCache,
        Dictionary<int, DnaStrand> strandCache,
        Dictionary<int, Gene> geneCache)
    {
        var nucleus = GetNucleusRow(connection, id);
        if (nucleus == null) return null;

        nucleus.Chromosomes = GetChromosomes(connection, nucleus.Id, chromosomeCache, strandCache, geneCache);

        if (includeParent && nucleus.ParentId > 0)
        {
            // Recurse upward. Cycle prevention is enforced on the write side.
            nucleus.Parent = GetNucleus(connection, nucleus.ParentId, true, chromosomeCache, strandCache, geneCache);
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

    private static Chromosome GetChromosome(
        SqliteConnection connection,
        int id,
        Dictionary<int, DnaStrand> strandCache,
        Dictionary<int, Gene> geneCache)
    {
        if (!TryGetChromosomeRow(connection, id, out var chromosome)) return null;

        chromosome.DnaStrands = GetDnaStrands(connection, chromosome.Id, strandCache, geneCache);

        return chromosome;
    }

    private static bool TryGetChromosomeRow(SqliteConnection connection, int id, out Chromosome chromosome)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name FROM Chromosome WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            chromosome = null;
            return false;
        }

        chromosome = new Chromosome
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? "" : reader.GetString(1)
        };
        return true;
    }

    private static DnaStrand GetDnaStrand(
        SqliteConnection connection,
        int id,
        Dictionary<int, Gene> geneCache)
    {
        if (!TryGetDnaStrandRow(connection, id, out var strand)) return null;

        strand.Genes = GetGenes(connection, strand.Id, geneCache);

        return strand;
    }

    private static bool TryGetDnaStrandRow(SqliteConnection connection, int id, out DnaStrand strand)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, EnumType, Ordinal, ComparisonSymbol, ComparisonValue, IsPercent " +
                              "FROM Dna WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            strand = null;
            return false;
        }

        strand = BuildDnaStrandFromReader(reader);
        return true;
    }

    private static Gene GetGene(SqliteConnection connection, int id)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Protein FROM Gene WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new Gene
        {
            Id = reader.GetInt32(0),
            ProteinName = reader.IsDBNull(1) ? null : reader.GetString(1)
        };
    }

    // -------------------------------------------------------------------------
    // GET — list fetchers (join through junction tables)
    // -------------------------------------------------------------------------

    /**
     * Returns all Chromosomes linked to a given Nucleus via NucleusChromosome.
     * Chromosomes already seen within this call (via the cache) are reused as
     * the same instance rather than constructed again.
     */
    private static List<Chromosome> GetChromosomes(
        SqliteConnection connection,
        int nucleusId,
        Dictionary<int, Chromosome> chromosomeCache,
        Dictionary<int, DnaStrand> strandCache,
        Dictionary<int, Gene> geneCache)
    {
        var ids = GetLinkedIds(connection,
            "SELECT ChromosomeId FROM NucleusChromosome WHERE NucleusId = @ParentId;",
            nucleusId);

        var chromosomes = new List<Chromosome>(ids.Count);

        foreach (var id in ids)
        {
            if (chromosomeCache.TryGetValue(id, out var cached))
            {
                chromosomes.Add(cached);
                continue;
            }

            if (!TryGetChromosomeRow(connection, id, out var chromosome)) continue;

            chromosome.DnaStrands = GetDnaStrands(connection, chromosome.Id, strandCache, geneCache);
            chromosomeCache[id] = chromosome;
            chromosomes.Add(chromosome);
        }

        return chromosomes;
    }

    /**
     * Returns all DnaStrands linked to a given Chromosome via ChromosomeDna.
     * Strands already seen within this call are reused as the same instance.
     */
    private static List<DnaStrand> GetDnaStrands(
        SqliteConnection connection,
        int chromosomeId,
        Dictionary<int, DnaStrand> strandCache,
        Dictionary<int, Gene> geneCache)
    {
        var ids = GetLinkedIds(connection,
            "SELECT DnaId FROM ChromosomeDna WHERE ChromosomeId = @ParentId;",
            chromosomeId);

        var strands = new List<DnaStrand>(ids.Count);

        foreach (var id in ids)
        {
            if (strandCache.TryGetValue(id, out var cached))
            {
                strands.Add(cached);
                continue;
            }

            if (!TryGetDnaStrandRow(connection, id, out var strand)) continue;

            strand.Genes = GetGenes(connection, strand.Id, geneCache);
            strandCache[id] = strand;
            strands.Add(strand);
        }

        return strands;
    }

    /**
     * Returns all Genes linked to a given DnaStrand via DnaGene.
     * Genes already seen within this call are reused as the same instance.
     */
    private static List<Gene> GetGenes(
        SqliteConnection connection,
        int dnaId,
        Dictionary<int, Gene> geneCache)
    {
        var ids = GetLinkedIds(connection,
            "SELECT GeneId FROM DnaGene WHERE DnaId = @ParentId;",
            dnaId);

        var genes = new List<Gene>(ids.Count);

        foreach (var id in ids)
        {
            if (geneCache.TryGetValue(id, out var cached))
            {
                genes.Add(cached);
                continue;
            }

            var gene = GetGene(connection, id);
            if (gene == null) continue;

            geneCache[id] = gene;
            genes.Add(gene);
        }

        return genes;
    }

    /**
     * Executes a query that returns a single integer column and collects the
     * results into a list. Used by all three list-fetchers above to read the
     * child IDs from a junction table. The query must use @ParentId as its
     * sole parameter.
     */
    private static List<int> GetLinkedIds(SqliteConnection connection, string sql, int parentId,
        SqliteTransaction transaction = null)
    {
        var ids = new List<int>();

        using var command = CreateCommand(connection, transaction);
        command.CommandText = sql;
        command.Parameters.AddWithValue("@ParentId", parentId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
            ids.Add(reader.GetInt32(0));

        return ids;
    }

    // -------------------------------------------------------------------------
    // GET — DnaStrand construction helper
    // -------------------------------------------------------------------------

    /**
     * Builds a DnaStrand from the current reader row. Shared by the single-row
     * fetch (TryGetDnaStrandRow) and future list-fetchers that may SELECT
     * multiple Dna rows in one query.
     * Reader column order: 0=Id, 1=Name, 2=EnumType, 3=Ordinal,
     * 4=ComparisonSymbol, 5=ComparisonValue, 6=IsPercent.
     *
     * Each Promoter field is read straight into its property - PromoterText is
     * a composed display string with no setter, so nothing is parsed here.
     * Target is rebuilt from EnumType (assembly-qualified type name) plus
     * Ordinal (the member's numeric value).
     */
    private static DnaStrand BuildDnaStrandFromReader(SqliteDataReader reader)
    {
        Enum @enum = null;
        if (!reader.IsDBNull(2))
        {
            var enumType = Type.GetType(reader.GetString(2));
            if ((enumType?.IsEnum ?? false) && !reader.IsDBNull(3))
                @enum = (Enum)Enum.ToObject(enumType, reader.GetInt32(3));
        }

        return new DnaStrand
        {
            Id = reader.GetInt32(0),
            Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
            Promoter = new Promoter
            {
                Target = @enum,
                ComparisonSymbol = reader.IsDBNull(4) ? "" : reader.GetString(4),
                ComparisonValue = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                IsPercent = !reader.IsDBNull(6) && reader.GetInt32(6) != 0
            }
        };
    }

    // -------------------------------------------------------------------------
    // REMOVE — public entry points
    // -------------------------------------------------------------------------

    /**
     * Removes a Nucleus row and its entire descendant-Nucleus subtree.
     * Nucleus.ParentId keeps its original ON DELETE CASCADE, which fires
     * automatically at the SQLite level and only ever runs downward: deleting
     * `id` cascades to every Nucleus row whose ParentId (transitively) points
     * at it, and — via the junction table's own ON DELETE CASCADE — to every
     * NucleusChromosome row belonging to any nucleus in that subtree. It never
     * touches `id`'s own ancestors.
     *
     * Because that cascade happens inside SQLite, our C# code has no chance to
     * inspect the junction rows once RemoveRow runs. So we walk the subtree
     * and collect every linked Chromosome ID *before* deleting, then run the
     * usual orphan-check/cascade logic on each of those chromosomes afterward.
     * Returns true if the Nucleus row existed and was deleted.
     */
    public static bool RemoveNucleus(int id)
    {
        return RemoveNucleus(Connection, id);
    }

    private static bool RemoveNucleus(SqliteConnection connection, int id, SqliteTransaction transaction = null)
    {
        var subtreeNucleusIds = GetNucleusSubtreeIds(connection, id, transaction);

        var linkedChromosomeIds = new HashSet<int>();
        foreach (var nucleusId in subtreeNucleusIds)
        {
            foreach (var chromId in GetLinkedIds(connection,
                         "SELECT ChromosomeId FROM NucleusChromosome WHERE NucleusId = @ParentId;",
                         nucleusId, transaction))
            {
                linkedChromosomeIds.Add(chromId);
            }
        }

        bool deleted = RemoveRow(connection, "Nucleus", id, transaction);
        // SQLite's cascade has now removed the whole Nucleus subtree and every
        // NucleusChromosome row for it. Orphan-check each chromosome that was
        // touched, cascading down through its DnaStrands/Genes as needed.
        foreach (var chromId in linkedChromosomeIds)
            DeleteIfOrphaned(connection, chromId,
                "SELECT COUNT(*) FROM NucleusChromosome WHERE ChromosomeId = @Id;",
                "Chromosome",
                chromId,
                () => CascadeOrphanChromosomeChildren(connection, chromId, transaction),
                transaction);

        return deleted;
    }

    /**
     * Recursively collects `rootId` plus every Nucleus Id transitively
     * reachable via ParentId (i.e. the full downward subtree of children,
     * grandchildren, etc.). Used before a delete so the cascade's effects on
     * NucleusChromosome links can be reasoned about afterward.
     */
    private static List<int> GetNucleusSubtreeIds(SqliteConnection connection, int rootId,
        SqliteTransaction transaction = null)
    {
        var ids = new List<int> { rootId };

        foreach (var childId in GetLinkedIds(connection,
                     "SELECT Id FROM Nucleus WHERE ParentId = @ParentId;", rootId, transaction))
        {
            ids.AddRange(GetNucleusSubtreeIds(connection, childId, transaction));
        }

        return ids;
    }

    /**
     * Unlinks the Chromosome from one of its parent Nuclei. If the Chromosome
     * becomes orphaned (no remaining NucleusChromosome rows), deletes it and
     * cascades down through its DnaStrands and Genes via the same orphan logic.
     * Returns true if the junction row existed and was removed (or if the
     * chromosome entity was consequently deleted).
     */
    public static bool RemoveChromosome(int nucleusId, int chromosomeId)
    {
        return RemoveChromosome(Connection, nucleusId, chromosomeId);
    }

    private static bool RemoveChromosome(SqliteConnection connection, int nucleusId, int chromosomeId,
        SqliteTransaction transaction = null)
    {
        bool unlinked = RemoveJunctionRow(connection, "NucleusChromosome",
            "NucleusId", nucleusId, "ChromosomeId", chromosomeId, transaction);

        DeleteIfOrphaned(connection, chromosomeId,
            "SELECT COUNT(*) FROM NucleusChromosome WHERE ChromosomeId = @Id;",
            "Chromosome",
            chromosomeId,
            () => CascadeOrphanChromosomeChildren(connection, chromosomeId, transaction),
            transaction);

        return unlinked;
    }

    /**
     * Runs orphan-check deletion on every DnaStrand still linked to a
     * Chromosome that is about to be deleted, and (via DeleteIfOrphaned's own
     * cascade callback) on every Gene linked to each of those strands.
     * Shared by RemoveChromosome and RemoveNucleus so both paths apply the
     * exact same downward-cascade logic once a Chromosome is confirmed orphaned.
     */
    private static void CascadeOrphanChromosomeChildren(SqliteConnection connection, int chromosomeId,
        SqliteTransaction transaction = null)
    {
        var linkedStrandIds = GetLinkedIds(connection,
            "SELECT DnaId FROM ChromosomeDna WHERE ChromosomeId = @ParentId;",
            chromosomeId, transaction);

        // This callback runs BEFORE RemoveRow deletes the Chromosome entity
        // (see DeleteIfOrphaned), so the ChromosomeDna rows for this chromosome
        // still exist right now. Sever them explicitly first — otherwise each
        // strand's remaining-link count below would still include the link back
        // to this (about-to-be-deleted) chromosome, and a strand that's only
        // used by this one chromosome would be missed as "not orphaned".
        DeleteAllJunctionRowsForParent(connection, "ChromosomeDna", "ChromosomeId", chromosomeId, transaction);

        foreach (var strandId in linkedStrandIds)
            DeleteIfOrphaned(connection, strandId,
                "SELECT COUNT(*) FROM ChromosomeDna WHERE DnaId = @Id;",
                "Dna",
                strandId,
                () => CleanupOrphanedGenesForStrand(connection, strandId, transaction),
                transaction);
    }

    /**
     * Unlinks a DnaStrand from one of its parent Chromosomes. If the strand
     * becomes orphaned, deletes it and cascades to its Genes.
     * Returns true if the junction row existed and was removed.
     */
    public static bool RemoveDnaStrand(int chromosomeId, int dnaId)
    {
        return RemoveDnaStrand(Connection, chromosomeId, dnaId);
    }

    private static bool RemoveDnaStrand(SqliteConnection connection, int chromosomeId, int dnaId,
        SqliteTransaction transaction = null)
    {
        bool unlinked = RemoveJunctionRow(connection, "ChromosomeDna",
            "ChromosomeId", chromosomeId, "DnaId", dnaId, transaction);

        DeleteIfOrphaned(connection, dnaId,
            "SELECT COUNT(*) FROM ChromosomeDna WHERE DnaId = @Id;",
            "Dna",
            dnaId,
            () => CleanupOrphanedGenesForStrand(connection, dnaId, transaction),
            transaction);

        return unlinked;
    }

    /**
     * Unlinks a Gene from one of its parent DnaStrands. If the Gene becomes
     * orphaned, deletes it.
     * Returns true if the junction row existed and was removed.
     */
    public static bool RemoveGene(int dnaId, int geneId)
    {
        return RemoveGene(Connection, dnaId, geneId);
    }

    private static bool RemoveGene(SqliteConnection connection, int dnaId, int geneId,
        SqliteTransaction transaction = null)
    {
        bool unlinked = RemoveJunctionRow(connection, "DnaGene",
            "DnaId", dnaId, "GeneId", geneId, transaction);

        DeleteIfOrphaned(connection, geneId,
            "SELECT COUNT(*) FROM DnaGene WHERE GeneId = @Id;",
            "Gene",
            geneId,
            null,
            transaction);

        return unlinked;
    }

    // -------------------------------------------------------------------------
    // REMOVE — helpers
    // -------------------------------------------------------------------------

    /**
     * Counts remaining junction rows for a child entity. If none remain,
     * deletes the entity row and optionally executes a cascade callback so
     * the caller can apply the same orphan logic to the next level down.
     */
    private static void DeleteIfOrphaned(
        SqliteConnection connection,
        int entityId,
        string countSql,
        string tableName,
        int id,
        Action cascadeCallback = null,
        SqliteTransaction transaction = null)
    {
        long remaining = CountRows(connection, countSql, entityId, transaction);
        if (remaining > 0) return;

        cascadeCallback?.Invoke();
        RemoveRow(connection, tableName, id, transaction);
    }

    /**
     * Convenience wrapper: collects all gene IDs still linked to a strand and
     * runs orphan-check deletion on each. Used when a strand is being deleted
     * as part of a parent-cascade.
     */
    private static void CleanupOrphanedGenesForStrand(SqliteConnection connection, int strandId,
        SqliteTransaction transaction = null)
    {
        var geneIds = GetLinkedIds(connection,
            "SELECT GeneId FROM DnaGene WHERE DnaId = @ParentId;",
            strandId, transaction);

        // Same reasoning as CascadeOrphanChromosomeChildren: this runs before
        // the Dna row (and its auto-cascaded DnaGene rows) are actually
        // deleted, so sever this strand's gene links explicitly first.
        DeleteAllJunctionRowsForParent(connection, "DnaGene", "DnaId", strandId, transaction);

        foreach (var geneId in geneIds)
            DeleteIfOrphaned(connection, geneId,
                "SELECT COUNT(*) FROM DnaGene WHERE GeneId = @Id;",
                "Gene",
                geneId,
                null,
                transaction);
    }

    /**
     * Deletes every junction row for a given parent column/value — i.e. all of
     * one entity's outgoing links to its children. Used right before checking
     * those children's orphan status, so the check reflects the parent's
     * imminent deletion rather than its still-live junction rows.
     * junctionTable/parentColumn are only ever passed as hardcoded literals.
     */
    private static void DeleteAllJunctionRowsForParent(
        SqliteConnection connection, string junctionTable, string parentColumn, int parentId,
        SqliteTransaction transaction = null)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = $"DELETE FROM {junctionTable} WHERE {parentColumn} = @Id;";
        command.Parameters.AddWithValue("@Id", parentId);
        command.ExecuteNonQuery();
    }

    private static long CountRows(SqliteConnection connection, string sql, int id, SqliteTransaction transaction = null)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Id", id);
        return (long)command.ExecuteScalar();
    }

    private static bool RemoveRow(SqliteConnection connection, string tableName, int id,
        SqliteTransaction transaction = null)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = $"DELETE FROM {tableName} WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteNonQuery() > 0;
    }

    /**
     * Deletes a single junction row identified by two FK columns.
     * Column and table names are only ever passed as hardcoded literals from
     * the Remove* methods above — never user input — so string interpolation
     * here is safe (FK column names cannot be SQL parameters).
     */
    private static bool RemoveJunctionRow(
        SqliteConnection connection,
        string junctionTable,
        string col1, int val1,
        string col2, int val2,
        SqliteTransaction transaction = null)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText =
            $"DELETE FROM {junctionTable} WHERE {col1} = @Val1 AND {col2} = @Val2;";
        command.Parameters.AddWithValue("@Val1", val1);
        command.Parameters.AddWithValue("@Val2", val2);
        return command.ExecuteNonQuery() > 0;
    }

    /**
     * Executes a SELECT that returns a single long scalar. Pass the active
     * transaction (if any) so this participates in the same atomic operation
     * as the INSERT that preceded it.
     */
    /**
     * The EnumType column value for a Promoter: the assembly-qualified name of
     * Target's type, or DBNull when there's no Target. Paired with
     * PromoterOrdinalValue so Type.GetType + Enum.ToObject can rebuild it.
     */
    private static object PromoterEnumTypeValue(Promoter promoter)
    {
        return (object)promoter?.Target?.GetType().AssemblyQualifiedName ?? DBNull.Value;
    }

    /**
     * The Ordinal column value for a Promoter: Target's underlying numeric
     * value, or DBNull when there's no Target. Convert.ToInt32 is used rather
     * than a direct (int) cast because Target is typed as System.Enum (a
     * reference type - no direct numeric cast exists) and because it also
     * handles enums whose underlying type isn't int.
     *
     * NOTE: this stores the enum's *value*, not its name, so renumbering or
     * reordering members of a stored enum will remap previously-saved rows.
     */
    private static object PromoterOrdinalValue(Promoter promoter)
    {
        return promoter?.Target == null ? DBNull.Value : Convert.ToInt32(promoter.Target);
    }

    /** The ComparisonSymbol column value (">", ">=", "*=", ...). */
    private static object PromoterSymbolValue(Promoter promoter)
    {
        return (object)promoter?.ComparisonSymbol ?? DBNull.Value;
    }

    /** The ComparisonValue column value (the numeric threshold). */
    private static object PromoterComparisonValue(Promoter promoter)
    {
        return promoter == null ? DBNull.Value : promoter.ComparisonValue;
    }

    /** The IsPercent column value, stored as SQLite 0/1. */
    private static object PromoterIsPercentValue(Promoter promoter)
    {
        return promoter == null ? DBNull.Value : (promoter.IsPercent ? 1 : 0);
    }

    private static long GetLastInsertRowId(SqliteConnection connection, SqliteTransaction transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT last_insert_rowid();";
        return (long)command.ExecuteScalar();
    }

    /**
     * Creates a command bound to the given connection and (possibly null)
     * transaction. Used by the Add* path so every statement in a cascade
     * participates in the same transaction as the top-level call.
     */
    private static SqliteCommand CreateCommand(SqliteConnection connection, SqliteTransaction transaction)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        return command;
    }

    /**
     * Returns true if a row with this Id already exists in tableName.
     * tableName is only ever passed as one of the hardcoded literals in the
     * Add* methods below, never user input.
     */
    private static bool EntityExists(SqliteConnection connection, SqliteTransaction transaction, string tableName,
        int id)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = $"SELECT COUNT(*) FROM {tableName} WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);
        return (long)command.ExecuteScalar() > 0;
    }

    /**
     * Writes a junction row linking parent to child, unless that exact link
     * already exists (INSERT OR IGNORE against the junction table's composite
     * primary key) - this is the "link the existing record instead of
     * duplicating it" dedup behavior for Add*. junctionTable/col1/col2 are
     * only ever passed as hardcoded literals from the Add* methods below.
     */
    private static void LinkIfMissing(
        SqliteConnection connection, SqliteTransaction transaction,
        string junctionTable, string col1, int val1, string col2, int val2)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText =
            $"INSERT OR IGNORE INTO {junctionTable} ({col1}, {col2}) VALUES (@Val1, @Val2);";
        command.Parameters.AddWithValue("@Val1", val1);
        command.Parameters.AddWithValue("@Val2", val2);
        command.ExecuteNonQuery();
    }

    // -------------------------------------------------------------------------
    // ADD — public entry points
    // -------------------------------------------------------------------------

    /**
     * Inserts (or, if nucleus.Id already exists in the DB, reuses) a Nucleus
     * row. Nucleus.ParentId is a direct column (the Nucleus-to-Nucleus
     * relationship stays one-to-one, not M2M) — parentId, if given, becomes
     * that column's value at insert time. It only applies on insert: calling
     * AddNucleus again for an already-existing Id does not re-parent it
     * (use UpdateNucleus for that).
     *
     * cascade = true additionally adds/links every Chromosome in
     * nucleus.Chromosomes (and, transitively, their DnaStrands and Genes).
     * cascade = false writes only this Nucleus row.
     *
     * Returns the Nucleus's Id (new or existing). On insert, nucleus.Id and
     * nucleus.ParentId are updated in place to reflect what was written.
     */
    public static int AddNucleus(Nucleus nucleus, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = AddNucleus(Connection, transaction, nucleus, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int AddNucleus(
        SqliteConnection connection, SqliteTransaction transaction,
        Nucleus nucleus, int? parentId, bool cascade)
    {
        bool isNew = nucleus.Id <= 0 || !EntityExists(connection, transaction, "Nucleus", nucleus.Id);

        if (isNew)
        {
            int resolvedParentId = parentId ?? nucleus.ParentId;

            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Nucleus (ParentId, Name) VALUES (@ParentId, @Name);";
            command.Parameters.AddWithValue("@ParentId",
                resolvedParentId > 0 ? resolvedParentId : (object)DBNull.Value);
            command.Parameters.AddWithValue("@Name", (object)nucleus.Name ?? DBNull.Value);
            command.ExecuteNonQuery();

            nucleus.Id = (int)GetLastInsertRowId(connection, transaction);
            nucleus.ParentId = resolvedParentId;
        }

        if (cascade)
        {
            foreach (var chromosome in nucleus.Chromosomes ?? new List<Chromosome>())
                AddChromosome(connection, transaction, chromosome, nucleus.Id, true);
        }

        return nucleus.Id;
    }

    /**
     * Inserts (or reuses, if chromosome.Id already exists) a Chromosome row,
     * then links it to parentId via NucleusChromosome (skipped if parentId is
     * null). If the link already exists, it is left as-is (no duplicate).
     *
     * cascade = true additionally adds/links every DnaStrand in
     * chromosome.DnaStrands (and, transitively, their Genes).
     *
     * Returns the Chromosome's Id (new or existing).
     */
    public static int AddChromosome(Chromosome chromosome, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = AddChromosome(Connection, transaction, chromosome, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int AddChromosome(
        SqliteConnection connection, SqliteTransaction transaction,
        Chromosome chromosome, int? parentId, bool cascade)
    {
        bool isNew = chromosome.Id <= 0 || !EntityExists(connection, transaction, "Chromosome", chromosome.Id);

        if (isNew)
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Chromosome (Name) VALUES (@Name);";
            command.Parameters.AddWithValue("@Name", (object)chromosome.Name ?? DBNull.Value);
            command.ExecuteNonQuery();

            chromosome.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "NucleusChromosome",
                "NucleusId", parentId.Value, "ChromosomeId", chromosome.Id);

        if (cascade)
        {
            foreach (var strand in chromosome.DnaStrands ?? new List<DnaStrand>())
                AddDnaStrand(connection, transaction, strand, chromosome.Id, true);
        }

        return chromosome.Id;
    }

    /**
     * Inserts (or reuses, if strand.Id already exists) a DnaStrand row, then
     * links it to parentId via ChromosomeDna (skipped if parentId is null).
     *
     * On insert, strand.Promoter.Target and .PromoterText are written to the
     * EnumType/Ordinal and ComparisonType columns respectively - Target's
     * assembly-qualified type name plus its numeric value, which together let
     * the read path rebuild the exact enum member via Type.GetType +
     * Enum.ToObject. A null Target writes NULL to both.
     *
     * cascade = true additionally adds/links every Gene in strand.Genes.
     *
     * Returns the DnaStrand's Id (new or existing).
     */
    public static int AddDnaStrand(DnaStrand strand, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = AddDnaStrand(Connection, transaction, strand, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int AddDnaStrand(
        SqliteConnection connection, SqliteTransaction transaction,
        DnaStrand strand, int? parentId, bool cascade)
    {
        bool isNew = strand.Id <= 0 || !EntityExists(connection, transaction, "Dna", strand.Id);

        if (isNew)
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = """
                                  INSERT INTO Dna (Name, EnumType, Ordinal, ComparisonSymbol, ComparisonValue, IsPercent)
                                  VALUES (@Name, @EnumType, @Ordinal, @ComparisonSymbol, @ComparisonValue, @IsPercent);
                                  """;
            command.Parameters.AddWithValue("@Name", (object)strand.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("@EnumType", PromoterEnumTypeValue(strand.Promoter));
            command.Parameters.AddWithValue("@Ordinal", PromoterOrdinalValue(strand.Promoter));
            command.Parameters.AddWithValue("@ComparisonSymbol", PromoterSymbolValue(strand.Promoter));
            command.Parameters.AddWithValue("@ComparisonValue", PromoterComparisonValue(strand.Promoter));
            command.Parameters.AddWithValue("@IsPercent", PromoterIsPercentValue(strand.Promoter));
            command.ExecuteNonQuery();

            strand.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "ChromosomeDna",
                "ChromosomeId", parentId.Value, "DnaId", strand.Id);

        if (cascade)
        {
            foreach (var gene in strand.Genes ?? new List<Gene>())
                AddGene(connection, transaction, gene, strand.Id, true);
        }

        return strand.Id;
    }

    /**
     * Inserts (or reuses, if gene.Id already exists) a Gene row, then links
     * it to parentId via DnaGene (skipped if parentId is null).
     * cascade has no effect — Gene is a leaf with no children in the
     * hierarchy — it's accepted only for signature uniformity with the other
     * Add* methods.
     *
     * Returns the Gene's Id (new or existing).
     */
    public static int AddGene(Gene gene, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = AddGene(Connection, transaction, gene, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int AddGene(
        SqliteConnection connection, SqliteTransaction transaction,
        Gene gene, int? parentId, bool cascade)
    {
        bool isNew = gene.Id <= 0 || !EntityExists(connection, transaction, "Gene", gene.Id);

        if (isNew)
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Gene (Protein) VALUES (@Protein);";
            command.Parameters.AddWithValue("@Protein", (object)gene.ProteinName ?? DBNull.Value);
            command.ExecuteNonQuery();

            gene.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "DnaGene",
                "DnaId", parentId.Value, "GeneId", gene.Id);

        return gene.Id;
    }

    // -------------------------------------------------------------------------
    // UPDATE — in-place edits by Id
    // -------------------------------------------------------------------------
    // Update* methods write directly to the entity's own row, so a shared
    // child (referenced by multiple parents) is changed for every parent that
    // references it — there's only ever one row. None of these touch junction
    // rows; relationships are left exactly as they were unless a caller uses
    // Add*/Remove* to change them explicitly.

    /**
     * Updates a Nucleus's Name and ParentId columns by Id. Returns true if a
     * row with that Id existed and was updated.
     */
    public static bool UpdateNucleus(Nucleus nucleus)
    {
        return UpdateNucleus(Connection, null, nucleus, nucleus.ParentId);
    }

    private static bool UpdateNucleus(
        SqliteConnection connection, SqliteTransaction transaction, Nucleus nucleus, int resolvedParentId)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = "UPDATE Nucleus SET ParentId = @ParentId, Name = @Name WHERE Id = @Id;";
        command.Parameters.AddWithValue("@ParentId", resolvedParentId > 0 ? resolvedParentId : (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", (object)nucleus.Name ?? DBNull.Value);
        command.Parameters.AddWithValue("@Id", nucleus.Id);
        return command.ExecuteNonQuery() > 0;
    }

    /**
     * Updates a Chromosome's Name column by Id. Returns true if a row with
     * that Id existed and was updated.
     */
    public static bool UpdateChromosome(Chromosome chromosome)
    {
        return UpdateChromosome(Connection, null, chromosome);
    }

    private static bool UpdateChromosome(
        SqliteConnection connection, SqliteTransaction transaction, Chromosome chromosome)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = "UPDATE Chromosome SET Name = @Name WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Name", (object)chromosome.Name ?? DBNull.Value);
        command.Parameters.AddWithValue("@Id", chromosome.Id);
        return command.ExecuteNonQuery() > 0;
    }

    /**
     * Updates a DnaStrand's Name, EnumType, Ordinal, and ComparisonType
     * columns by Id (from Name, Promoter.Target, and Promoter.PromoterText).
     * Ordinal now carries Target's numeric value, so it IS written here -
     * leaving it stale would desync it from EnumType and rebuild the wrong
     * enum member on read.
     * Returns true if a row with that Id existed and was updated.
     */
    public static bool UpdateDnaStrand(DnaStrand strand)
    {
        return UpdateDnaStrand(Connection, null, strand);
    }

    private static bool UpdateDnaStrand(
        SqliteConnection connection, SqliteTransaction transaction, DnaStrand strand)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText =
            "UPDATE Dna SET Name = @Name, EnumType = @EnumType, Ordinal = @Ordinal, " +
            "ComparisonSymbol = @ComparisonSymbol, ComparisonValue = @ComparisonValue, " +
            "IsPercent = @IsPercent WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Name", (object)strand.Name ?? DBNull.Value);
        command.Parameters.AddWithValue("@EnumType", PromoterEnumTypeValue(strand.Promoter));
        command.Parameters.AddWithValue("@Ordinal", PromoterOrdinalValue(strand.Promoter));
        command.Parameters.AddWithValue("@ComparisonSymbol", PromoterSymbolValue(strand.Promoter));
        command.Parameters.AddWithValue("@ComparisonValue", PromoterComparisonValue(strand.Promoter));
        command.Parameters.AddWithValue("@IsPercent", PromoterIsPercentValue(strand.Promoter));
        command.Parameters.AddWithValue("@Id", strand.Id);
        return command.ExecuteNonQuery() > 0;
    }

    /**
     * Updates a Gene's Protein column (from ProteinName) by Id. Returns true
     * if a row with that Id existed and was updated.
     */
    public static bool UpdateGene(Gene gene)
    {
        return UpdateGene(Connection, null, gene);
    }

    private static bool UpdateGene(
        SqliteConnection connection, SqliteTransaction transaction, Gene gene)
    {
        using var command = CreateCommand(connection, transaction);
        command.CommandText = "UPDATE Gene SET Protein = @Protein WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Protein", (object)gene.ProteinName ?? DBNull.Value);
        command.Parameters.AddWithValue("@Id", gene.Id);
        return command.ExecuteNonQuery() > 0;
    }

    // -------------------------------------------------------------------------
    // SYNC — upsert (update-or-insert) + link
    // -------------------------------------------------------------------------
    // Sync* is the "align the DB to this object graph" entry point. For each
    // element it either UPDATEs the existing row (Id > 0 and present) or
    // INSERTs a new one, then ensures the parent-child junction link exists.
    // With cascade = true it walks the whole attached subtree doing the same.
    //
    // IMPORTANT — Sync is additive with respect to relationships. It creates
    // and updates, but never unlinks: a child that exists in the DB but is no
    // longer present in the object's collection keeps its junction row. If you
    // remove a Chromosome from nucleus.Chromosomes and Sync, that link
    // survives - call Remove*(parentId, childId) explicitly to drop it. This
    // keeps Sync non-destructive; a partially-populated graph (e.g. one built
    // with cascade = false reads) can be synced without silently deleting the
    // relationships it simply doesn't know about.

    /**
     * Upserts a Nucleus and, with cascade = true, its whole attached subtree.
     * parentId (or, if null, nucleus.ParentId) becomes the Nucleus's ParentId
     * column - unlike the other levels this is a direct column, not a
     * junction, since Nucleus-to-Nucleus stays one-to-one.
     * Returns the Nucleus's Id (existing or newly assigned).
     */
    public static int SyncNucleus(Nucleus nucleus, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = SyncNucleus(Connection, transaction, nucleus, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int SyncNucleus(
        SqliteConnection connection, SqliteTransaction transaction,
        Nucleus nucleus, int? parentId, bool cascade)
    {
        int resolvedParentId = parentId ?? nucleus.ParentId;
        bool exists = nucleus.Id > 0 && EntityExists(connection, transaction, "Nucleus", nucleus.Id);

        if (exists)
        {
            UpdateNucleus(connection, transaction, nucleus, resolvedParentId);
            nucleus.ParentId = resolvedParentId;
        }
        else
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Nucleus (ParentId, Name) VALUES (@ParentId, @Name);";
            command.Parameters.AddWithValue("@ParentId",
                resolvedParentId > 0 ? resolvedParentId : (object)DBNull.Value);
            command.Parameters.AddWithValue("@Name", (object)nucleus.Name ?? DBNull.Value);
            command.ExecuteNonQuery();

            nucleus.Id = (int)GetLastInsertRowId(connection, transaction);
            nucleus.ParentId = resolvedParentId;
        }

        if (cascade)
        {
            foreach (var chromosome in nucleus.Chromosomes ?? new List<Chromosome>())
                SyncChromosome(connection, transaction, chromosome, nucleus.Id, true);
        }

        return nucleus.Id;
    }

    /**
     * Upserts a Chromosome, links it to parentId via NucleusChromosome (if
     * parentId is given), and with cascade = true syncs its DnaStrands and
     * their Genes. Returns the Chromosome's Id.
     */
    public static int SyncChromosome(Chromosome chromosome, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = SyncChromosome(Connection, transaction, chromosome, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int SyncChromosome(
        SqliteConnection connection, SqliteTransaction transaction,
        Chromosome chromosome, int? parentId, bool cascade)
    {
        bool exists = chromosome.Id > 0 && EntityExists(connection, transaction, "Chromosome", chromosome.Id);

        if (exists)
        {
            UpdateChromosome(connection, transaction, chromosome);
        }
        else
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Chromosome (Name) VALUES (@Name);";
            command.Parameters.AddWithValue("@Name", (object)chromosome.Name ?? DBNull.Value);
            command.ExecuteNonQuery();

            chromosome.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "NucleusChromosome",
                "NucleusId", parentId.Value, "ChromosomeId", chromosome.Id);

        if (cascade)
        {
            foreach (var strand in chromosome.DnaStrands ?? new List<DnaStrand>())
                SyncDnaStrand(connection, transaction, strand, chromosome.Id, true);
        }

        return chromosome.Id;
    }

    /**
     * Upserts a DnaStrand, links it to parentId via ChromosomeDna (if
     * parentId is given), and with cascade = true syncs its Genes.
     * Ordinal carries Target's numeric value and is written on both the
     * insert and update paths - see the AddDnaStrand note.
     * Returns the DnaStrand's Id.
     */
    public static int SyncDnaStrand(DnaStrand strand, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = SyncDnaStrand(Connection, transaction, strand, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int SyncDnaStrand(
        SqliteConnection connection, SqliteTransaction transaction,
        DnaStrand strand, int? parentId, bool cascade)
    {
        bool exists = strand.Id > 0 && EntityExists(connection, transaction, "Dna", strand.Id);

        if (exists)
        {
            UpdateDnaStrand(connection, transaction, strand);
        }
        else
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = """
                                  INSERT INTO Dna (Name, EnumType, Ordinal, ComparisonSymbol, ComparisonValue, IsPercent)
                                  VALUES (@Name, @EnumType, @Ordinal, @ComparisonSymbol, @ComparisonValue, @IsPercent);
                                  """;
            command.Parameters.AddWithValue("@Name", (object)strand.Name ?? DBNull.Value);
            command.Parameters.AddWithValue("@EnumType", PromoterEnumTypeValue(strand.Promoter));
            command.Parameters.AddWithValue("@Ordinal", PromoterOrdinalValue(strand.Promoter));
            command.Parameters.AddWithValue("@ComparisonSymbol", PromoterSymbolValue(strand.Promoter));
            command.Parameters.AddWithValue("@ComparisonValue", PromoterComparisonValue(strand.Promoter));
            command.Parameters.AddWithValue("@IsPercent", PromoterIsPercentValue(strand.Promoter));
            command.ExecuteNonQuery();

            strand.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "ChromosomeDna",
                "ChromosomeId", parentId.Value, "DnaId", strand.Id);

        if (cascade)
        {
            foreach (var gene in strand.Genes ?? new List<Gene>())
                SyncGene(connection, transaction, gene, strand.Id, true);
        }

        return strand.Id;
    }

    /**
     * Upserts a Gene and links it to parentId via DnaGene (if parentId is
     * given). cascade has no effect - Gene is a leaf - and is accepted only
     * for signature uniformity. Returns the Gene's Id.
     */
    public static int SyncGene(Gene gene, int? parentId = null, bool cascade = true)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            int id = SyncGene(Connection, transaction, gene, parentId, cascade);
            transaction.Commit();
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int SyncGene(
        SqliteConnection connection, SqliteTransaction transaction,
        Gene gene, int? parentId, bool cascade)
    {
        bool exists = gene.Id > 0 && EntityExists(connection, transaction, "Gene", gene.Id);

        if (exists)
        {
            UpdateGene(connection, transaction, gene);
        }
        else
        {
            using var command = CreateCommand(connection, transaction);
            command.CommandText = "INSERT INTO Gene (Protein) VALUES (@Protein);";
            command.Parameters.AddWithValue("@Protein", (object)gene.ProteinName ?? DBNull.Value);
            command.ExecuteNonQuery();

            gene.Id = (int)GetLastInsertRowId(connection, transaction);
        }

        if (parentId.HasValue && parentId.Value > 0)
            LinkIfMissing(connection, transaction, "DnaGene",
                "DnaId", parentId.Value, "GeneId", gene.Id);

        return gene.Id;
    }
}