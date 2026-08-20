using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Main.main.packages.model.Dna;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Main.tests;

[TestClass]
public class DnaHelperMethodsTests
{
    private static string _tempDirPath;
    private static string _tempDbPath;
    private static SqliteConnection _connection;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _tempDirPath = Path.Combine(Path.GetTempPath(), "DnaHelperMethodsTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirPath);

        _tempDbPath = Path.Combine(_tempDirPath, Path.GetRandomFileName());
        _connection = new SqliteConnection($"Data Source={_tempDbPath};");
        _connection.Open();

        // Check root BaseDirectory first, fallback to subfolder path if necessary
        string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dna.sql");
        if (!File.Exists(schemaPath))
        {
            schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages", "model", "Dna", "dna.sql");
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = File.ReadAllText(schemaPath);
            command.ExecuteNonQuery();
        }

        // DnaHelperMethods keeps one shared static connection open across all calls
        // (see DnaHelperMethods.Connection). Point it at this test's temp file and the
        // resolved schema path, reset any connection a prior test class may have left
        // open, then Initialize() to build/verify the schema against the temp file -
        // same call DnaDb makes in production.
        DnaHelperMethods.ConnectionPath = _tempDbPath;
        DnaHelperMethods.SchemaPath = schemaPath;
        DnaHelperMethods.ResetConnection();
        DnaHelperMethods.Initialize();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        using var command = _connection.CreateCommand();
        // Junction tables first (defensive - entity deletes below would cascade
        // to these anyway via ON DELETE CASCADE, but being explicit keeps this
        // resilient to schema changes), then entities, then reset autoincrement.
        command.CommandText = """
                              DELETE FROM DnaGene;
                              DELETE FROM ChromosomeDna;
                              DELETE FROM NucleusChromosome;
                              DELETE FROM Gene;
                              DELETE FROM Dna;
                              DELETE FROM Chromosome;
                              DELETE FROM Nucleus;
                              DELETE FROM sqlite_sequence WHERE name IN ('Gene', 'Dna', 'Chromosome', 'Nucleus');
                              """;
        command.ExecuteNonQuery();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        DnaHelperMethods.ResetConnection();

        _connection?.Close();
        _connection?.Dispose();
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(_tempDirPath))
            Directory.Delete(_tempDirPath, recursive: true);
    }


    // ENTITY INSERT HELPERS --------
    // Insert a bare entity row only - no parent link. Used directly by tests
    // that need to wire up sharing (multiple links) explicitly via the Link*
    // helpers below.

    private static long InsertNucleusEntity(int? parentId, string name)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Nucleus (ParentId, Name) VALUES (@ParentId, @Name);";
            command.Parameters.AddWithValue("@ParentId", (object)parentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        return GetLastInsertRowId();
    }

    private static long InsertChromosomeEntity(string name)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Chromosome (Name) VALUES (@Name);";
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        return GetLastInsertRowId();
    }

    private static long InsertDnaEntity(string name, string enumType, int ordinal, string comparisonType)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = """
                                  INSERT INTO Dna (Name, EnumType, Ordinal, ComparisonType)
                                  VALUES (@Name, @EnumType, @Ordinal, @ComparisonType);
                                  """;
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.Parameters.AddWithValue("@EnumType", (object)enumType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ordinal", ordinal);
            command.Parameters.AddWithValue("@ComparisonType", (object)comparisonType ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        return GetLastInsertRowId();
    }

    private static long InsertGeneEntity()
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Gene DEFAULT VALUES;";
            command.ExecuteNonQuery();
        }

        return GetLastInsertRowId();
    }

    private static long GetLastInsertRowId()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT last_insert_rowid();";
        return (long)command.ExecuteScalar();
    }


    // RAW VERIFICATION HELPERS --------
    // Used by the Add*/Update* tests below to inspect DB state directly,
    // independent of DnaHelperMethods' own Get* read path.

    private static object GetColumnValue(string table, string column, int id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT {column} FROM {table} WHERE Id = @Id;";
        command.Parameters.AddWithValue("@Id", id);
        return command.ExecuteScalar();
    }

    private static long CountRows(string table)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";
        return (long)command.ExecuteScalar();
    }

    private static bool LinkExists(string junctionTable, string col1, int val1, string col2, int val2)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {junctionTable} WHERE {col1} = @Val1 AND {col2} = @Val2;";
        command.Parameters.AddWithValue("@Val1", val1);
        command.Parameters.AddWithValue("@Val2", val2);
        return (long)command.ExecuteScalar() > 0;
    }

    // Only used to exercise Promoter.Target's round trip into the EnumType
    // column (AssemblyQualifiedName) on Add/Update - not tied to any real
    // game enum.
    private enum TestPromoterEnum
    {
        Alpha,
        Beta
    }


    // JUNCTION LINK HELPERS --------
    // Insert a single link row. Call more than once against the same child Id
    // to simulate that child being shared across multiple parents.

    private static void LinkNucleusChromosome(int nucleusId, int chromosomeId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
            "INSERT INTO NucleusChromosome (NucleusId, ChromosomeId) VALUES (@NucleusId, @ChromosomeId);";
        command.Parameters.AddWithValue("@NucleusId", nucleusId);
        command.Parameters.AddWithValue("@ChromosomeId", chromosomeId);
        command.ExecuteNonQuery();
    }

    private static void LinkChromosomeDna(int chromosomeId, int dnaId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
            "INSERT INTO ChromosomeDna (ChromosomeId, DnaId) VALUES (@ChromosomeId, @DnaId);";
        command.Parameters.AddWithValue("@ChromosomeId", chromosomeId);
        command.Parameters.AddWithValue("@DnaId", dnaId);
        command.ExecuteNonQuery();
    }

    private static void LinkDnaGene(int dnaId, int geneId)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "INSERT INTO DnaGene (DnaId, GeneId) VALUES (@DnaId, @GeneId);";
        command.Parameters.AddWithValue("@DnaId", dnaId);
        command.Parameters.AddWithValue("@GeneId", geneId);
        command.ExecuteNonQuery();
    }


    // COMBO INSERT HELPERS --------
    // Create an entity AND link it to exactly one parent in a single call.
    // These match the pre-M2M helper shape (parentId, ...) so most existing
    // single-parent test bodies below are unchanged - only tests that
    // exercise sharing/orphan behavior go through the entity + Link* helpers
    // directly.

    private static long InsertNucleus(int? parentId, string name) => InsertNucleusEntity(parentId, name);

    private static long InsertChromosome(int nucleusId, string name)
    {
        long id = InsertChromosomeEntity(name);
        LinkNucleusChromosome(nucleusId, (int)id);
        return id;
    }

    private static long InsertDna(int chromosomeId, string name, string enumType, int ordinal, string comparisonType)
    {
        long id = InsertDnaEntity(name, enumType, ordinal, comparisonType);
        LinkChromosomeDna(chromosomeId, (int)id);
        return id;
    }

    private static long InsertGene(int dnaId)
    {
        long id = InsertGeneEntity();
        LinkDnaGene(dnaId, (int)id);
        return id;
    }


    [TestMethod]
    public void Test()
    {
    }

    // GET NUCLEUS - BASIC --------

    [TestMethod]
    public void TestGetNucleusNotFoundReturnsNull()
    {
        Assert.IsNull(DnaHelperMethods.GetNucleus(9999));
    }

    [TestMethod]
    public void TestGetNucleusBasic()
    {
        long id = InsertNucleus(null, "Plant Root");

        var nucleus = DnaHelperMethods.GetNucleus((int)id);

        Assert.IsNotNull(nucleus);
        Assert.AreEqual((int)id, nucleus.Id);
        Assert.AreEqual("Plant Root", nucleus.Name);
        Assert.AreEqual(0, nucleus.ParentId);
        Assert.IsNull(nucleus.Parent);
        Assert.IsNotNull(nucleus.Chromosomes);
        Assert.AreEqual(0, nucleus.Chromosomes.Count);
    }

    [TestMethod]
    public void TestGetNucleusNullNameDefaultsToEmptyString()
    {
        long id = InsertNucleus(null, null);

        var nucleus = DnaHelperMethods.GetNucleus((int)id);

        Assert.AreEqual("", nucleus.Name);
    }

    // GET NUCLEUS - CHROMOSOMES --------

    [TestMethod]
    public void TestGetNucleusWithChromosomes()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        InsertChromosome((int)nucleusId, "ChromosomeA");
        InsertChromosome((int)nucleusId, "ChromosomeB");

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);

        Assert.AreEqual(2, nucleus.Chromosomes.Count);
        Assert.AreEqual("ChromosomeA", nucleus.Chromosomes[0].Name);
        Assert.AreEqual("ChromosomeB", nucleus.Chromosomes[1].Name);
    }

    [TestMethod]
    public void TestGetNucleusOnlyReturnsOwnChromosomes()
    {
        long firstNucleusId = InsertNucleus(null, "First");
        long secondNucleusId = InsertNucleus(null, "Second");
        InsertChromosome((int)firstNucleusId, "ChromosomeA");
        InsertChromosome((int)secondNucleusId, "ChromosomeB");

        var nucleus = DnaHelperMethods.GetNucleus((int)firstNucleusId);

        Assert.AreEqual(1, nucleus.Chromosomes.Count);
        Assert.AreEqual("ChromosomeA", nucleus.Chromosomes[0].Name);
    }

    // GET NUCLEUS - DNA STRANDS --------

    [TestMethod]
    public void TestGetNucleusChromosomeHasDnaStrands()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);
        var strand = nucleus.Chromosomes[0].DnaStrands[0];

        Assert.AreEqual(1, nucleus.Chromosomes[0].DnaStrands.Count);
        Assert.AreEqual("StrandA", strand.Name);
        Assert.IsNotNull(strand.Promoter);
        Assert.AreEqual("GreaterThan", strand.Promoter.PromoterText);
        // Target is a System.Enum built from the EnumType column via reflection - not wired
        // up yet in DnaHelperMethods, so it is expected to stay unset for now.
        Assert.IsNull(strand.Promoter.Target);
    }

    [TestMethod]
    public void TestGetNucleusDnaStrandNullNameAndComparisonTypeDefaultToEmptyString()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        InsertDna((int)chromosomeId, null, "SomeEnumType", 1, null);

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);
        var strand = nucleus.Chromosomes[0].DnaStrands[0];

        Assert.AreEqual("", strand.Name);
        Assert.AreEqual("", strand.Promoter.PromoterText);
    }

    // GET NUCLEUS - GENES --------

    [TestMethod]
    public void TestGetNucleusDnaStrandHasGenes()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);
        var genes = nucleus.Chromosomes[0].DnaStrands[0].Genes;

        Assert.AreEqual(1, genes.Count);
        Assert.AreEqual((int)geneId, genes[0].Id);
        // ProteinName has no matching column in the Gene table (Id, Protein only,
        // read into ProteinName), so it stays unset unless Protein was written.
        Assert.IsNull(genes[0].ProteinName);
    }

    [TestMethod]
    public void TestGetNucleusDnaStrandWithNoGenesReturnsEmptyList()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);
        var genes = nucleus.Chromosomes[0].DnaStrands[0].Genes;

        Assert.IsNotNull(genes);
        Assert.AreEqual(0, genes.Count);
    }

    // GET NUCLEUS - PARENT RECURSION --------
    // Nucleus.ParentId remains one-to-one (not part of the M2M refactor), so
    // this section is unchanged in behavior.

    [TestMethod]
    public void TestGetNucleusIncludeParentFalseLeavesParentNull()
    {
        long parentId = InsertNucleus(null, "Root");
        long childId = InsertNucleus((int)parentId, "Child");

        var nucleus = DnaHelperMethods.GetNucleus((int)childId);

        Assert.AreEqual((int)parentId, nucleus.ParentId);
        Assert.IsNull(nucleus.Parent);
    }

    [TestMethod]
    public void TestGetNucleusIncludeParentTrueLoadsParent()
    {
        long parentId = InsertNucleus(null, "Root");
        long childId = InsertNucleus((int)parentId, "Child");

        var nucleus = DnaHelperMethods.GetNucleus((int)childId, true);

        Assert.IsNotNull(nucleus.Parent);
        Assert.AreEqual((int)parentId, nucleus.Parent.Id);
        Assert.AreEqual("Root", nucleus.Parent.Name);
        Assert.IsNull(nucleus.Parent.Parent);
    }

    [TestMethod]
    public void TestGetNucleusIncludeParentTrueWithNoParentTerminates()
    {
        long nucleusId = InsertNucleus(null, "Root");

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId, true);

        Assert.IsNull(nucleus.Parent);
    }

    [TestMethod]
    public void TestGetNucleusIncludeParentTrueThreeLevelsDeep()
    {
        long grandparentId = InsertNucleus(null, "Grandparent");
        long parentId = InsertNucleus((int)grandparentId, "Parent");
        long childId = InsertNucleus((int)parentId, "Child");

        var nucleus = DnaHelperMethods.GetNucleus((int)childId, true);

        Assert.AreEqual("Child", nucleus.Name);
        Assert.AreEqual("Parent", nucleus.Parent.Name);
        Assert.AreEqual("Grandparent", nucleus.Parent.Parent.Name);
        Assert.IsNull(nucleus.Parent.Parent.Parent);
    }

    // GET CHROMOSOME --------

    [TestMethod]
    public void TestGetChromosomeNotFoundReturnsNull()
    {
        Assert.IsNull(DnaHelperMethods.GetChromosome(9999));
    }

    [TestMethod]
    public void TestGetChromosomeBasic()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");

        var chromosome = DnaHelperMethods.GetChromosome((int)chromosomeId);

        Assert.IsNotNull(chromosome);
        Assert.AreEqual((int)chromosomeId, chromosome.Id);
        // No ParentId assertion - Chromosome no longer carries one under M2M;
        // ownership is read via GetNucleus(...).Chromosomes instead.
        Assert.AreEqual("ChromosomeA", chromosome.Name);
        Assert.IsNotNull(chromosome.DnaStrands);
        Assert.AreEqual(0, chromosome.DnaStrands.Count);
    }

    [TestMethod]
    public void TestGetChromosomeNullNameDefaultsToEmptyString()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, null);

        var chromosome = DnaHelperMethods.GetChromosome((int)chromosomeId);

        Assert.AreEqual("", chromosome.Name);
    }

    [TestMethod]
    public void TestGetChromosomeIncludesDnaStrandsAndGenes()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        var chromosome = DnaHelperMethods.GetChromosome((int)chromosomeId);

        Assert.AreEqual(1, chromosome.DnaStrands.Count);
        Assert.AreEqual("StrandA", chromosome.DnaStrands[0].Name);
        Assert.AreEqual(1, chromosome.DnaStrands[0].Genes.Count);
        Assert.AreEqual((int)geneId, chromosome.DnaStrands[0].Genes[0].Id);
    }

    // GET DNA STRAND --------

    [TestMethod]
    public void TestGetDnaStrandNotFoundReturnsNull()
    {
        Assert.IsNull(DnaHelperMethods.GetDnaStrand(9999));
    }

    [TestMethod]
    public void TestGetDnaStrandBasic()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        var strand = DnaHelperMethods.GetDnaStrand((int)dnaId);

        Assert.IsNotNull(strand);
        Assert.AreEqual((int)dnaId, strand.Id);
        Assert.AreEqual("StrandA", strand.Name);
        Assert.IsNotNull(strand.Promoter);
        Assert.AreEqual("GreaterThan", strand.Promoter.PromoterText);
        Assert.IsNotNull(strand.Genes);
        Assert.AreEqual(0, strand.Genes.Count);
    }

    [TestMethod]
    public void TestGetDnaStrandNullNameAndComparisonTypeDefaultToEmptyString()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, null, "SomeEnumType", 1, null);

        var strand = DnaHelperMethods.GetDnaStrand((int)dnaId);

        Assert.AreEqual("", strand.Name);
        Assert.AreEqual("", strand.Promoter.PromoterText);
    }

    [TestMethod]
    public void TestGetDnaStrandIncludesGenes()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        var strand = DnaHelperMethods.GetDnaStrand((int)dnaId);

        Assert.AreEqual(1, strand.Genes.Count);
        Assert.AreEqual((int)geneId, strand.Genes[0].Id);
    }

    // GET GENE --------

    [TestMethod]
    public void TestGetGeneNotFoundReturnsNull()
    {
        Assert.IsNull(DnaHelperMethods.GetGene(9999));
    }

    [TestMethod]
    public void TestGetGeneBasic()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        var gene = DnaHelperMethods.GetGene((int)geneId);

        Assert.IsNotNull(gene);
        Assert.AreEqual((int)geneId, gene.Id);
        Assert.IsNull(gene.ProteinName);
    }

    // -------------------------------------------------------------------------
    // INSTANCE SHARING (identity map) --------
    // Verifies that within a single top-level Get* call, a child reached via
    // more than one path is returned as the exact same C# instance, not a
    // separate copy per path.
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestGetNucleusSharesSameDnaStrandInstanceAcrossChromosomes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeA = InsertChromosome((int)nucleusId, "ChromA");
        long chromosomeB = InsertChromosome((int)nucleusId, "ChromB");
        long dnaId = InsertDnaEntity("SharedStrand", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeA, (int)dnaId);
        LinkChromosomeDna((int)chromosomeB, (int)dnaId);

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);

        var strandFromA = nucleus.Chromosomes.Single(c => c.Id == (int)chromosomeA).DnaStrands.Single();
        var strandFromB = nucleus.Chromosomes.Single(c => c.Id == (int)chromosomeB).DnaStrands.Single();

        Assert.AreSame(strandFromA, strandFromB);
    }

    [TestMethod]
    public void TestGetNucleusSharesSameGeneInstanceAcrossDnaStrands()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Chrom");
        long strandA = InsertDnaEntity("StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        var nucleus = DnaHelperMethods.GetNucleus((int)nucleusId);
        var chromosome = nucleus.Chromosomes.Single();

        var geneFromA = chromosome.DnaStrands.Single(s => s.Id == (int)strandA).Genes.Single();
        var geneFromB = chromosome.DnaStrands.Single(s => s.Id == (int)strandB).Genes.Single();

        Assert.AreSame(geneFromA, geneFromB);
    }

    [TestMethod]
    public void TestGetNucleusDoesNotShareInstancesAcrossSeparateTopLevelCalls()
    {
        // The identity map is scoped to a single Get* call, not a session-wide
        // cache (see DnaHelperMethods.GetNucleus). Two independent calls that
        // happen to reach the same shared Chromosome must NOT return the same
        // C# instance - only IDs match, not references.
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Shared");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        var fetchedA = DnaHelperMethods.GetNucleus((int)nucleusA);
        var fetchedB = DnaHelperMethods.GetNucleus((int)nucleusB);

        Assert.AreEqual(fetchedA.Chromosomes.Single().Id, fetchedB.Chromosomes.Single().Id);
        Assert.AreNotSame(fetchedA.Chromosomes.Single(), fetchedB.Chromosomes.Single());
    }

    // REMOVE NUCLEUS --------

    [TestMethod]
    public void TestRemoveNucleusReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.RemoveNucleus(9999));
    }

    [TestMethod]
    public void TestRemoveNucleusReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");

        Assert.IsTrue(DnaHelperMethods.RemoveNucleus((int)nucleusId));
        Assert.IsNull(DnaHelperMethods.GetNucleus((int)nucleusId));
    }

    [TestMethod]
    public void TestRemoveNucleusCascadesToChromosomeDnaAndGene()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveNucleus((int)nucleusId);

        Assert.IsNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
        Assert.IsNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveNucleusDoesNotAffectSiblingNucleus()
    {
        long firstNucleusId = InsertNucleus(null, "First");
        long secondNucleusId = InsertNucleus(null, "Second");

        DnaHelperMethods.RemoveNucleus((int)firstNucleusId);

        Assert.IsNotNull(DnaHelperMethods.GetNucleus((int)secondNucleusId));
    }

    [TestMethod]
    public void TestRemoveNucleusDoesNotAffectAncestorNucleus()
    {
        // Nucleus.ParentId cascade only ever runs downward (toward children) -
        // deleting a child must never remove its parent chain.
        long parentId = InsertNucleus(null, "Parent");
        long childId = InsertNucleus((int)parentId, "Child");

        DnaHelperMethods.RemoveNucleus((int)childId);

        Assert.IsNotNull(DnaHelperMethods.GetNucleus((int)parentId));
    }

    [TestMethod]
    public void TestRemoveNucleusCascadesThroughDescendantNucleusSubtree()
    {
        long grandparentId = InsertNucleus(null, "Grandparent");
        long parentId = InsertNucleus((int)grandparentId, "Parent");
        long childId = InsertNucleus((int)parentId, "Child");
        long chromosomeId = InsertChromosome((int)childId, "ChromosomeA");

        DnaHelperMethods.RemoveNucleus((int)grandparentId);

        Assert.IsNull(DnaHelperMethods.GetNucleus((int)grandparentId));
        Assert.IsNull(DnaHelperMethods.GetNucleus((int)parentId));
        Assert.IsNull(DnaHelperMethods.GetNucleus((int)childId));
        // The chromosome was only ever linked to the (now-deleted) grandchild
        // nucleus, so it should be cleaned up as an orphan too.
        Assert.IsNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    [TestMethod]
    public void TestRemoveNucleusPreservesChromosomeSharedWithAnotherNucleus()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Shared");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        DnaHelperMethods.RemoveNucleus((int)nucleusA);

        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusB).Chromosomes.Count);
    }

    // REMOVE CHROMOSOME --------

    [TestMethod]
    public void TestRemoveChromosomeReturnsFalseWhenNotFound()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");

        Assert.IsFalse(DnaHelperMethods.RemoveChromosome((int)nucleusId, 9999));
    }

    [TestMethod]
    public void TestRemoveChromosomeReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");

        Assert.IsTrue(DnaHelperMethods.RemoveChromosome((int)nucleusId, (int)chromosomeId));
        Assert.IsNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    [TestMethod]
    public void TestRemoveChromosomeCascadesToDnaAndGene()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveChromosome((int)nucleusId, (int)chromosomeId);

        Assert.IsNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveChromosomeDoesNotAffectParentNucleusOrSiblings()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long siblingId = InsertChromosome((int)nucleusId, "ChromosomeB");

        DnaHelperMethods.RemoveChromosome((int)nucleusId, (int)chromosomeId);

        Assert.IsNotNull(DnaHelperMethods.GetNucleus((int)nucleusId));
        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)siblingId));
    }

    [TestMethod]
    public void TestRemoveChromosomeUnlinksButPreservesChromosomeSharedWithAnotherNucleus()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Shared");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        bool unlinked = DnaHelperMethods.RemoveChromosome((int)nucleusA, (int)chromosomeId);

        Assert.IsTrue(unlinked);
        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusB).Chromosomes.Count);
    }

    [TestMethod]
    public void TestRemoveChromosomeDeletesOnlyAfterLastLinkRemoved()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Shared");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        DnaHelperMethods.RemoveChromosome((int)nucleusA, (int)chromosomeId);
        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));

        DnaHelperMethods.RemoveChromosome((int)nucleusB, (int)chromosomeId);
        Assert.IsNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    [TestMethod]
    public void TestRemoveChromosomePreservesDnaStrandSharedWithAnotherChromosome()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeA = InsertChromosome((int)nucleusId, "ChromA");
        long chromosomeB = InsertChromosome((int)nucleusId, "ChromB");
        long dnaId = InsertDnaEntity("SharedStrand", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeA, (int)dnaId);
        LinkChromosomeDna((int)chromosomeB, (int)dnaId);

        DnaHelperMethods.RemoveChromosome((int)nucleusId, (int)chromosomeA);

        Assert.IsNotNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
        Assert.AreEqual(1, DnaHelperMethods.GetChromosome((int)chromosomeB).DnaStrands.Count);
    }

    // REMOVE DNA STRAND --------

    [TestMethod]
    public void TestRemoveDnaStrandReturnsFalseWhenNotFound()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");

        Assert.IsFalse(DnaHelperMethods.RemoveDnaStrand((int)chromosomeId, 9999));
    }

    [TestMethod]
    public void TestRemoveDnaStrandReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        Assert.IsTrue(DnaHelperMethods.RemoveDnaStrand((int)chromosomeId, (int)dnaId));
        Assert.IsNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
    }

    [TestMethod]
    public void TestRemoveDnaStrandCascadesToGene()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveDnaStrand((int)chromosomeId, (int)dnaId);

        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveDnaStrandDoesNotAffectParentChromosome()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        DnaHelperMethods.RemoveDnaStrand((int)chromosomeId, (int)dnaId);

        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    [TestMethod]
    public void TestRemoveDnaStrandUnlinksButPreservesStrandSharedWithAnotherChromosome()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeA = InsertChromosome((int)nucleusId, "ChromA");
        long chromosomeB = InsertChromosome((int)nucleusId, "ChromB");
        long dnaId = InsertDnaEntity("SharedStrand", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeA, (int)dnaId);
        LinkChromosomeDna((int)chromosomeB, (int)dnaId);

        bool unlinked = DnaHelperMethods.RemoveDnaStrand((int)chromosomeA, (int)dnaId);

        Assert.IsTrue(unlinked);
        Assert.IsNotNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
        Assert.AreEqual(1, DnaHelperMethods.GetChromosome((int)chromosomeB).DnaStrands.Count);
    }

    [TestMethod]
    public void TestRemoveDnaStrandPreservesGeneSharedWithAnotherStrand()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Chrom");
        long strandA = InsertDnaEntity("StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        DnaHelperMethods.RemoveDnaStrand((int)chromosomeId, (int)strandA);

        Assert.IsNotNull(DnaHelperMethods.GetGene((int)geneId));
        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand((int)strandB).Genes.Count);
    }

    // REMOVE GENE --------

    [TestMethod]
    public void TestRemoveGeneReturnsFalseWhenNotFound()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        Assert.IsFalse(DnaHelperMethods.RemoveGene((int)dnaId, 9999));
    }

    [TestMethod]
    public void TestRemoveGeneReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        Assert.IsTrue(DnaHelperMethods.RemoveGene((int)dnaId, (int)geneId));
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveGeneDoesNotAffectParentDnaStrand()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveGene((int)dnaId, (int)geneId);

        var strand = DnaHelperMethods.GetDnaStrand((int)dnaId);
        Assert.IsNotNull(strand);
        Assert.AreEqual(0, strand.Genes.Count);
    }

    [TestMethod]
    public void TestRemoveGeneUnlinksButPreservesGeneSharedWithAnotherStrand()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Chrom");
        long strandA = InsertDnaEntity("StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        bool unlinked = DnaHelperMethods.RemoveGene((int)strandA, (int)geneId);

        Assert.IsTrue(unlinked);
        Assert.IsNotNull(DnaHelperMethods.GetGene((int)geneId));
        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand((int)strandB).Genes.Count);
    }

    [TestMethod]
    public void TestRemoveGeneDeletesOnlyAfterLastLinkRemoved()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Chrom");
        long strandA = InsertDnaEntity("StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        DnaHelperMethods.RemoveGene((int)strandA, (int)geneId);
        Assert.IsNotNull(DnaHelperMethods.GetGene((int)geneId));

        DnaHelperMethods.RemoveGene((int)strandB, (int)geneId);
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    // -------------------------------------------------------------------------
    // ADD NUCLEUS --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestAddNucleusInsertsNewRowAndAssignsId()
    {
        var nucleus = new Nucleus { Name = "Root" };

        int id = DnaHelperMethods.AddNucleus(nucleus, cascade: false);

        Assert.IsTrue(id > 0);
        Assert.AreEqual(id, nucleus.Id);
        var fetched = DnaHelperMethods.GetNucleus(id);
        Assert.IsNotNull(fetched);
        Assert.AreEqual("Root", fetched.Name);
    }

    [TestMethod]
    public void TestAddNucleusWritesParentIdColumn()
    {
        long parentId = InsertNucleusEntity(null, "Parent");

        var child = new Nucleus { Name = "Child" };
        int childId = DnaHelperMethods.AddNucleus(child, (int)parentId, cascade: false);

        var fetched = DnaHelperMethods.GetNucleus(childId, true);
        Assert.AreEqual((int)parentId, fetched.ParentId);
        Assert.IsNotNull(fetched.Parent);
        Assert.AreEqual("Parent", fetched.Parent.Name);
    }

    [TestMethod]
    public void TestAddNucleusWithExistingIdDoesNotReinsertOrChangeName()
    {
        long existingId = InsertNucleusEntity(null, "Original");

        var duplicate = new Nucleus { Id = (int)existingId, Name = "Changed" };
        int returnedId = DnaHelperMethods.AddNucleus(duplicate, cascade: false);

        Assert.AreEqual((int)existingId, returnedId);
        Assert.AreEqual(1, CountRows("Nucleus"));
        Assert.AreEqual("Original", DnaHelperMethods.GetNucleus((int)existingId).Name);
    }

    [TestMethod]
    public void TestAddNucleusCascadeAddsChromosomesDnaAndGenes()
    {
        var nucleus = new Nucleus
        {
            Name = "Root",
            Chromosomes = new List<Chromosome>
            {
                new Chromosome
                {
                    Name = "ChromA",
                    DnaStrands = new List<DnaStrand>
                    {
                        new DnaStrand
                        {
                            Name = "StrandA",
                            Promoter = new Promoter { PromoterText = "GreaterThan" },
                            Genes = new List<Gene> { new Gene { ProteinName = "Chlorophyll Synthase" } }
                        }
                    }
                }
            }
        };

        int nucleusId = DnaHelperMethods.AddNucleus(nucleus);

        var fetched = DnaHelperMethods.GetNucleus(nucleusId);
        Assert.AreEqual(1, fetched.Chromosomes.Count);
        Assert.AreEqual("ChromA", fetched.Chromosomes[0].Name);
        Assert.AreEqual(1, fetched.Chromosomes[0].DnaStrands.Count);
        Assert.AreEqual("StrandA", fetched.Chromosomes[0].DnaStrands[0].Name);
        Assert.AreEqual(1, fetched.Chromosomes[0].DnaStrands[0].Genes.Count);
        Assert.AreEqual("Chlorophyll Synthase", fetched.Chromosomes[0].DnaStrands[0].Genes[0].ProteinName);
    }

    [TestMethod]
    public void TestAddNucleusCascadeFalseDoesNotAddChildren()
    {
        var nucleus = new Nucleus
        {
            Name = "Root",
            Chromosomes = new List<Chromosome> { new Chromosome { Name = "ChromA" } }
        };

        int nucleusId = DnaHelperMethods.AddNucleus(nucleus, cascade: false);

        Assert.AreEqual(0, DnaHelperMethods.GetNucleus(nucleusId).Chromosomes.Count);
        Assert.AreEqual(0, CountRows("Chromosome"));
    }

    [TestMethod]
    public void TestAddNucleusDedupLinksExistingChromosomeInsteadOfDuplicating()
    {
        long existingChromosomeId = InsertChromosomeEntity("Original");

        var nucleus = new Nucleus
        {
            Name = "Root",
            Chromosomes = new List<Chromosome>
            {
                new Chromosome { Id = (int)existingChromosomeId, Name = "Changed" }
            }
        };

        int nucleusId = DnaHelperMethods.AddNucleus(nucleus);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual("Original", DnaHelperMethods.GetChromosome((int)existingChromosomeId).Name);
        Assert.IsTrue(
            LinkExists("NucleusChromosome", "NucleusId", nucleusId, "ChromosomeId", (int)existingChromosomeId));
    }

    // -------------------------------------------------------------------------
    // ADD CHROMOSOME --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestAddChromosomeInsertsNewRowAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome { Name = "ChromA" };

        int chromosomeId = DnaHelperMethods.AddChromosome(chromosome, (int)nucleusId, cascade: false);

        Assert.IsTrue(chromosomeId > 0);
        Assert.AreEqual(chromosomeId, chromosome.Id);
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusId).Chromosomes.Count);
    }

    [TestMethod]
    public void TestAddChromosomeWithNullParentIdDoesNotLink()
    {
        var chromosome = new Chromosome { Name = "Unlinked" };

        int chromosomeId = DnaHelperMethods.AddChromosome(chromosome, null, cascade: false);

        Assert.IsNotNull(DnaHelperMethods.GetChromosome(chromosomeId));
        Assert.AreEqual(0, CountRows("NucleusChromosome"));
    }

    [TestMethod]
    public void TestAddChromosomeExistingIdOnlyLinksDoesNotReinsert()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long existingId = InsertChromosomeEntity("Original");

        var duplicate = new Chromosome { Id = (int)existingId, Name = "Changed" };
        DnaHelperMethods.AddChromosome(duplicate, (int)nucleusId, cascade: false);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual("Original", DnaHelperMethods.GetChromosome((int)existingId).Name);
        Assert.IsTrue(LinkExists("NucleusChromosome", "NucleusId", (int)nucleusId, "ChromosomeId", (int)existingId));
    }

    [TestMethod]
    public void TestAddChromosomeCascadeAddsDnaStrandsAndGenes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome
        {
            Name = "ChromA",
            DnaStrands = new List<DnaStrand>
            {
                new DnaStrand
                {
                    Name = "StrandA",
                    Promoter = new Promoter { PromoterText = "GreaterThan" },
                    Genes = new List<Gene> { new Gene { ProteinName = "ProteinA" } }
                }
            }
        };

        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusId);

        var fetched = DnaHelperMethods.GetChromosome(chromosome.Id);
        Assert.AreEqual(1, fetched.DnaStrands.Count);
        Assert.AreEqual(1, fetched.DnaStrands[0].Genes.Count);
        Assert.AreEqual("ProteinA", fetched.DnaStrands[0].Genes[0].ProteinName);
    }

    [TestMethod]
    public void TestAddChromosomeCascadeFalseDoesNotAddDnaStrands()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome
        {
            Name = "ChromA",
            DnaStrands = new List<DnaStrand> { new DnaStrand { Name = "StrandA" } }
        };

        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusId, cascade: false);

        Assert.AreEqual(0, DnaHelperMethods.GetChromosome(chromosome.Id).DnaStrands.Count);
        Assert.AreEqual(0, CountRows("Dna"));
    }

    [TestMethod]
    public void TestAddChromosomeCanLinkSameChromosomeToMultipleNuclei()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        var chromosome = new Chromosome { Name = "Shared" };

        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusA, cascade: false);
        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusB, cascade: false);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusA).Chromosomes.Count);
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusB).Chromosomes.Count);
    }

    [TestMethod]
    public void TestAddChromosomeLinkIfMissingDoesNotDuplicateExistingLink()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome { Name = "ChromA" };

        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusId, cascade: false);
        DnaHelperMethods.AddChromosome(chromosome, (int)nucleusId, cascade: false);

        using var command = _connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM NucleusChromosome WHERE NucleusId = @NucleusId AND ChromosomeId = @ChromosomeId;";
        command.Parameters.AddWithValue("@NucleusId", (int)nucleusId);
        command.Parameters.AddWithValue("@ChromosomeId", chromosome.Id);
        Assert.AreEqual(1L, (long)command.ExecuteScalar());
    }

    // -------------------------------------------------------------------------
    // ADD DNA STRAND --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestAddDnaStrandInsertsNewRowAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand { Name = "StrandA", Promoter = new Promoter { PromoterText = "GreaterThan" } };

        int dnaId = DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.IsTrue(dnaId > 0);
        Assert.AreEqual(1, DnaHelperMethods.GetChromosome((int)chromosomeId).DnaStrands.Count);
    }

    [TestMethod]
    public void TestAddDnaStrandWritesPromoterTextToComparisonTypeColumn()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand { Name = "StrandA", Promoter = new Promoter { PromoterText = "GreaterThan" } };

        int dnaId = DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.AreEqual("GreaterThan", GetColumnValue("Dna", "ComparisonType", dnaId));
    }

    [TestMethod]
    public void TestAddDnaStrandWritesTargetEnumTypeAssemblyQualifiedName()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand
        {
            Name = "StrandA",
            Promoter = new Promoter { Target = TestPromoterEnum.Alpha, PromoterText = "GreaterThan" }
        };

        int dnaId = DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.AreEqual(typeof(TestPromoterEnum).AssemblyQualifiedName, GetColumnValue("Dna", "EnumType", dnaId));
    }

    [TestMethod]
    public void TestAddDnaStrandOrdinalDefaultsToZeroOnInsert()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand { Name = "StrandA" };

        int dnaId = DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.AreEqual(0L, Convert.ToInt64(GetColumnValue("Dna", "Ordinal", dnaId)));
    }

    [TestMethod]
    public void TestAddDnaStrandExistingIdOnlyLinksDoesNotReinsert()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long existingId = InsertDnaEntity("Original", "SomeEnumType", 1, "GreaterThan");

        var duplicate = new DnaStrand { Id = (int)existingId, Name = "Changed" };
        DnaHelperMethods.AddDnaStrand(duplicate, (int)chromosomeId, cascade: false);

        Assert.AreEqual(1, CountRows("Dna"));
        Assert.AreEqual("Original", DnaHelperMethods.GetDnaStrand((int)existingId).Name);
        Assert.IsTrue(LinkExists("ChromosomeDna", "ChromosomeId", (int)chromosomeId, "DnaId", (int)existingId));
    }

    [TestMethod]
    public void TestAddDnaStrandCascadeAddsGenes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand
        {
            Name = "StrandA",
            Genes = new List<Gene> { new Gene { ProteinName = "ProteinA" } }
        };

        DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId);

        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand(strand.Id).Genes.Count);
    }

    [TestMethod]
    public void TestAddDnaStrandCascadeFalseDoesNotAddGenes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand
        {
            Name = "StrandA",
            Genes = new List<Gene> { new Gene { ProteinName = "ProteinA" } }
        };

        DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.AreEqual(0, DnaHelperMethods.GetDnaStrand(strand.Id).Genes.Count);
        Assert.AreEqual(0, CountRows("Gene"));
    }

    [TestMethod]
    public void TestAddDnaStrandCanLinkSameStrandToMultipleChromosomes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeA = InsertChromosome((int)nucleusId, "ChromA");
        long chromosomeB = InsertChromosome((int)nucleusId, "ChromB");
        var strand = new DnaStrand { Name = "Shared" };

        DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeA, cascade: false);
        DnaHelperMethods.AddDnaStrand(strand, (int)chromosomeB, cascade: false);

        Assert.AreEqual(1, CountRows("Dna"));
        Assert.AreEqual(1, DnaHelperMethods.GetChromosome((int)chromosomeA).DnaStrands.Count);
        Assert.AreEqual(1, DnaHelperMethods.GetChromosome((int)chromosomeB).DnaStrands.Count);
    }

    // -------------------------------------------------------------------------
    // ADD GENE --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestAddGeneInsertsNewRowAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        var gene = new Gene { ProteinName = "ProteinA" };

        int geneId = DnaHelperMethods.AddGene(gene, (int)dnaId);

        Assert.IsTrue(geneId > 0);
        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand((int)dnaId).Genes.Count);
        Assert.AreEqual("ProteinA", DnaHelperMethods.GetGene(geneId).ProteinName);
    }

    [TestMethod]
    public void TestAddGeneWithNullParentIdDoesNotLink()
    {
        var gene = new Gene { ProteinName = "Unlinked" };

        int geneId = DnaHelperMethods.AddGene(gene, null);

        Assert.IsNotNull(DnaHelperMethods.GetGene(geneId));
        Assert.AreEqual(0, CountRows("DnaGene"));
    }

    [TestMethod]
    public void TestAddGeneExistingIdOnlyLinksDoesNotReinsert()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long existingId = InsertGeneEntity();

        var duplicate = new Gene { Id = (int)existingId, ProteinName = "Changed" };
        DnaHelperMethods.AddGene(duplicate, (int)dnaId);

        Assert.AreEqual(1, CountRows("Gene"));
        Assert.IsNull(DnaHelperMethods.GetGene((int)existingId).ProteinName);
        Assert.IsTrue(LinkExists("DnaGene", "DnaId", (int)dnaId, "GeneId", (int)existingId));
    }

    [TestMethod]
    public void TestAddGeneCanLinkSameGeneToMultipleStrands()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long strandA = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        var gene = new Gene { ProteinName = "Shared" };

        DnaHelperMethods.AddGene(gene, (int)strandA);
        DnaHelperMethods.AddGene(gene, (int)strandB);

        Assert.AreEqual(1, CountRows("Gene"));
        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand((int)strandA).Genes.Count);
        Assert.AreEqual(1, DnaHelperMethods.GetDnaStrand((int)strandB).Genes.Count);
    }

    // -------------------------------------------------------------------------
    // UPDATE NUCLEUS --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestUpdateNucleusChangesNameAndParentId()
    {
        long parentId = InsertNucleus(null, "Parent");
        long nucleusId = InsertNucleus(null, "Original");

        bool updated = DnaHelperMethods.UpdateNucleus(
            new Nucleus { Id = (int)nucleusId, Name = "Updated", ParentId = (int)parentId });

        Assert.IsTrue(updated);
        var fetched = DnaHelperMethods.GetNucleus((int)nucleusId);
        Assert.AreEqual("Updated", fetched.Name);
        Assert.AreEqual((int)parentId, fetched.ParentId);
    }

    [TestMethod]
    public void TestUpdateNucleusReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.UpdateNucleus(new Nucleus { Id = 9999, Name = "Nope" }));
    }

    [TestMethod]
    public void TestUpdateNucleusZeroParentIdClearsToNull()
    {
        long parentId = InsertNucleus(null, "Parent");
        long nucleusId = InsertNucleus((int)parentId, "Child");

        DnaHelperMethods.UpdateNucleus(new Nucleus { Id = (int)nucleusId, Name = "Child", ParentId = 0 });

        Assert.AreEqual(0, DnaHelperMethods.GetNucleus((int)nucleusId).ParentId);
    }

    // -------------------------------------------------------------------------
    // UPDATE CHROMOSOME --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestUpdateChromosomeChangesName()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Original");

        bool updated = DnaHelperMethods.UpdateChromosome(new Chromosome { Id = (int)chromosomeId, Name = "Updated" });

        Assert.IsTrue(updated);
        Assert.AreEqual("Updated", DnaHelperMethods.GetChromosome((int)chromosomeId).Name);
    }

    [TestMethod]
    public void TestUpdateChromosomeReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.UpdateChromosome(new Chromosome { Id = 9999, Name = "Nope" }));
    }

    [TestMethod]
    public void TestUpdateChromosomeVisibleToAllParentsWhenShared()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Original");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        DnaHelperMethods.UpdateChromosome(new Chromosome { Id = (int)chromosomeId, Name = "Updated" });

        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)nucleusA).Chromosomes.Single().Name);
        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)nucleusB).Chromosomes.Single().Name);
    }

    // -------------------------------------------------------------------------
    // UPDATE DNA STRAND --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestUpdateDnaStrandChangesNameEnumTypeAndComparisonType()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "Original", "SomeEnumType", 1, "GreaterThan");

        bool updated = DnaHelperMethods.UpdateDnaStrand(new DnaStrand
        {
            Id = (int)dnaId,
            Name = "Updated",
            Promoter = new Promoter { Target = TestPromoterEnum.Beta, PromoterText = "LessThan" }
        });

        Assert.IsTrue(updated);
        var fetched = DnaHelperMethods.GetDnaStrand((int)dnaId);
        Assert.AreEqual("Updated", fetched.Name);
        Assert.AreEqual("LessThan", fetched.Promoter.PromoterText);
        Assert.AreEqual(typeof(TestPromoterEnum).AssemblyQualifiedName, GetColumnValue("Dna", "EnumType", (int)dnaId));
    }

    [TestMethod]
    public void TestUpdateDnaStrandReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.UpdateDnaStrand(new DnaStrand { Id = 9999, Name = "Nope" }));
    }

    [TestMethod]
    public void TestUpdateDnaStrandDoesNotModifyOrdinal()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "Original", "SomeEnumType", 7, "GreaterThan");

        DnaHelperMethods.UpdateDnaStrand(new DnaStrand { Id = (int)dnaId, Name = "Updated" });

        Assert.AreEqual(7L, Convert.ToInt64(GetColumnValue("Dna", "Ordinal", (int)dnaId)));
    }

    // -------------------------------------------------------------------------
    // UPDATE GENE --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestUpdateGeneChangesProteinName()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        bool updated = DnaHelperMethods.UpdateGene(new Gene { Id = (int)geneId, ProteinName = "Updated" });

        Assert.IsTrue(updated);
        Assert.AreEqual("Updated", DnaHelperMethods.GetGene((int)geneId).ProteinName);
    }

    [TestMethod]
    public void TestUpdateGeneReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.UpdateGene(new Gene { Id = 9999, ProteinName = "Nope" }));
    }

    [TestMethod]
    public void TestUpdateGeneVisibleToAllParentsWhenShared()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "Chrom");
        long strandA = InsertDnaEntity("StrandA", "SomeEnumType", 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", "SomeEnumType", 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        DnaHelperMethods.UpdateGene(new Gene { Id = (int)geneId, ProteinName = "Updated" });

        Assert.AreEqual("Updated", DnaHelperMethods.GetDnaStrand((int)strandA).Genes.Single().ProteinName);
        Assert.AreEqual("Updated", DnaHelperMethods.GetDnaStrand((int)strandB).Genes.Single().ProteinName);
    }

    // -------------------------------------------------------------------------
    // SYNC NUCLEUS --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestSyncNucleusInsertsWhenIdUnset()
    {
        var nucleus = new Nucleus { Name = "New" };

        int id = DnaHelperMethods.SyncNucleus(nucleus, cascade: false);

        Assert.IsTrue(id > 0);
        Assert.AreEqual(id, nucleus.Id);
        Assert.AreEqual("New", DnaHelperMethods.GetNucleus(id).Name);
    }

    [TestMethod]
    public void TestSyncNucleusUpdatesWhenIdExists()
    {
        long existingId = InsertNucleusEntity(null, "Original");

        int id = DnaHelperMethods.SyncNucleus(
            new Nucleus { Id = (int)existingId, Name = "Updated" }, cascade: false);

        Assert.AreEqual((int)existingId, id);
        Assert.AreEqual(1, CountRows("Nucleus"));
        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)existingId).Name);
    }

    [TestMethod]
    public void TestSyncNucleusInsertsWhenIdSetButRowMissing()
    {
        var nucleus = new Nucleus { Id = 9999, Name = "Ghost" };

        int id = DnaHelperMethods.SyncNucleus(nucleus, cascade: false);

        Assert.AreNotEqual(9999, id);
        Assert.AreEqual(id, nucleus.Id);
        Assert.AreEqual("Ghost", DnaHelperMethods.GetNucleus(id).Name);
    }

    [TestMethod]
    public void TestSyncNucleusAppliesParentIdArgument()
    {
        long parentId = InsertNucleusEntity(null, "Parent");
        long childId = InsertNucleusEntity(null, "Child");

        DnaHelperMethods.SyncNucleus(
            new Nucleus { Id = (int)childId, Name = "Child" }, (int)parentId, cascade: false);

        Assert.AreEqual((int)parentId, DnaHelperMethods.GetNucleus((int)childId).ParentId);
    }

    [TestMethod]
    public void TestSyncNucleusCascadeUpdatesExistingChildAndInsertsNewOne()
    {
        long nucleusId = InsertNucleusEntity(null, "Root");
        long existingChromosomeId = InsertChromosomeEntity("OriginalChrom");
        LinkNucleusChromosome((int)nucleusId, (int)existingChromosomeId);

        var nucleus = new Nucleus
        {
            Id = (int)nucleusId,
            Name = "Root",
            Chromosomes = new List<Chromosome>
            {
                new Chromosome { Id = (int)existingChromosomeId, Name = "UpdatedChrom" },
                new Chromosome { Name = "BrandNewChrom" }
            }
        };

        DnaHelperMethods.SyncNucleus(nucleus);

        Assert.AreEqual(2, CountRows("Chromosome"));
        var fetched = DnaHelperMethods.GetNucleus((int)nucleusId);
        Assert.AreEqual(2, fetched.Chromosomes.Count);
        Assert.AreEqual("UpdatedChrom", DnaHelperMethods.GetChromosome((int)existingChromosomeId).Name);
        Assert.IsTrue(fetched.Chromosomes.Any(c => c.Name == "BrandNewChrom"));
    }

    [TestMethod]
    public void TestSyncNucleusCascadeFalseIgnoresChildren()
    {
        long nucleusId = InsertNucleusEntity(null, "Root");

        var nucleus = new Nucleus
        {
            Id = (int)nucleusId,
            Name = "Updated",
            Chromosomes = new List<Chromosome> { new Chromosome { Name = "ShouldNotBeInserted" } }
        };

        DnaHelperMethods.SyncNucleus(nucleus, cascade: false);

        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)nucleusId).Name);
        Assert.AreEqual(0, CountRows("Chromosome"));
    }

    [TestMethod]
    public void TestSyncNucleusCascadeSyncsFullDepthSubtree()
    {
        var nucleus = new Nucleus
        {
            Name = "Root",
            Chromosomes = new List<Chromosome>
            {
                new Chromosome
                {
                    Name = "ChromA",
                    DnaStrands = new List<DnaStrand>
                    {
                        new DnaStrand
                        {
                            Name = "StrandA",
                            Promoter = new Promoter { PromoterText = "GreaterThan" },
                            Genes = new List<Gene> { new Gene { ProteinName = "ProteinA" } }
                        }
                    }
                }
            }
        };

        int nucleusId = DnaHelperMethods.SyncNucleus(nucleus);

        var fetched = DnaHelperMethods.GetNucleus(nucleusId);
        Assert.AreEqual("ChromA", fetched.Chromosomes.Single().Name);
        Assert.AreEqual("StrandA", fetched.Chromosomes.Single().DnaStrands.Single().Name);
        Assert.AreEqual("ProteinA",
            fetched.Chromosomes.Single().DnaStrands.Single().Genes.Single().ProteinName);
    }

    [TestMethod]
    public void TestSyncNucleusDoesNotUnlinkChildRemovedFromCollection()
    {
        // Sync is additive: it never removes junction rows for children that
        // are absent from the object graph. Dropping the link is an explicit
        // Remove* call.
        long nucleusId = InsertNucleusEntity(null, "Root");
        long chromosomeId = InsertChromosomeEntity("Keeper");
        LinkNucleusChromosome((int)nucleusId, (int)chromosomeId);

        var nucleus = new Nucleus
        {
            Id = (int)nucleusId,
            Name = "Root",
            Chromosomes = new List<Chromosome>()
        };

        DnaHelperMethods.SyncNucleus(nucleus);

        Assert.IsTrue(LinkExists("NucleusChromosome", "NucleusId", (int)nucleusId, "ChromosomeId", (int)chromosomeId));
        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    // -------------------------------------------------------------------------
    // SYNC CHROMOSOME --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestSyncChromosomeInsertsAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome { Name = "ChromA" };

        int id = DnaHelperMethods.SyncChromosome(chromosome, (int)nucleusId, cascade: false);

        Assert.IsTrue(id > 0);
        Assert.IsTrue(LinkExists("NucleusChromosome", "NucleusId", (int)nucleusId, "ChromosomeId", id));
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusId).Chromosomes.Count);
    }

    [TestMethod]
    public void TestSyncChromosomeUpdatesExistingAndLinksToNewParent()
    {
        long nucleusA = InsertNucleus(null, "A");
        long nucleusB = InsertNucleus(null, "B");
        long chromosomeId = InsertChromosomeEntity("Original");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);

        DnaHelperMethods.SyncChromosome(
            new Chromosome { Id = (int)chromosomeId, Name = "Updated" }, (int)nucleusB, cascade: false);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual("Updated", DnaHelperMethods.GetChromosome((int)chromosomeId).Name);
        // Both links now present - the original was not disturbed.
        Assert.IsTrue(LinkExists("NucleusChromosome", "NucleusId", (int)nucleusA, "ChromosomeId", (int)chromosomeId));
        Assert.IsTrue(LinkExists("NucleusChromosome", "NucleusId", (int)nucleusB, "ChromosomeId", (int)chromosomeId));
    }

    [TestMethod]
    public void TestSyncChromosomeDoesNotDuplicateExistingLink()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome { Name = "ChromA" };

        DnaHelperMethods.SyncChromosome(chromosome, (int)nucleusId, cascade: false);
        DnaHelperMethods.SyncChromosome(chromosome, (int)nucleusId, cascade: false);

        using var command = _connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM NucleusChromosome WHERE NucleusId = @NucleusId AND ChromosomeId = @ChromosomeId;";
        command.Parameters.AddWithValue("@NucleusId", (int)nucleusId);
        command.Parameters.AddWithValue("@ChromosomeId", chromosome.Id);
        Assert.AreEqual(1L, (long)command.ExecuteScalar());
    }

    [TestMethod]
    public void TestSyncChromosomeCascadeFalseIgnoresDnaStrands()
    {
        long nucleusId = InsertNucleus(null, "Root");
        var chromosome = new Chromosome
        {
            Name = "ChromA",
            DnaStrands = new List<DnaStrand> { new DnaStrand { Name = "StrandA" } }
        };

        DnaHelperMethods.SyncChromosome(chromosome, (int)nucleusId, cascade: false);

        Assert.AreEqual(0, CountRows("Dna"));
    }

    [TestMethod]
    public void TestSyncChromosomeCascadeMixesUpdatesAndInserts()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosomeEntity("ChromA");
        LinkNucleusChromosome((int)nucleusId, (int)chromosomeId);
        long existingStrandId = InsertDnaEntity("OriginalStrand", null, 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)existingStrandId);

        var chromosome = new Chromosome
        {
            Id = (int)chromosomeId,
            Name = "ChromA",
            DnaStrands = new List<DnaStrand>
            {
                new DnaStrand { Id = (int)existingStrandId, Name = "UpdatedStrand" },
                new DnaStrand { Name = "NewStrand" }
            }
        };

        DnaHelperMethods.SyncChromosome(chromosome, (int)nucleusId);

        Assert.AreEqual(2, CountRows("Dna"));
        Assert.AreEqual("UpdatedStrand", DnaHelperMethods.GetDnaStrand((int)existingStrandId).Name);
        Assert.AreEqual(2, DnaHelperMethods.GetChromosome((int)chromosomeId).DnaStrands.Count);
    }

    // -------------------------------------------------------------------------
    // SYNC DNA STRAND --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestSyncDnaStrandInsertsAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand { Name = "StrandA", Promoter = new Promoter { PromoterText = "GreaterThan" } };

        int id = DnaHelperMethods.SyncDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.IsTrue(id > 0);
        Assert.IsTrue(LinkExists("ChromosomeDna", "ChromosomeId", (int)chromosomeId, "DnaId", id));
        Assert.AreEqual("GreaterThan", GetColumnValue("Dna", "ComparisonType", id));
    }

    [TestMethod]
    public void TestSyncDnaStrandUpdatesPromoterFieldsOnExistingRow()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "Original", null, 1, "GreaterThan");

        DnaHelperMethods.SyncDnaStrand(new DnaStrand
        {
            Id = (int)dnaId,
            Name = "Updated",
            Promoter = new Promoter { Target = TestPromoterEnum.Beta, PromoterText = "LessThan" }
        }, (int)chromosomeId, cascade: false);

        Assert.AreEqual(1, CountRows("Dna"));
        Assert.AreEqual("Updated", GetColumnValue("Dna", "Name", (int)dnaId));
        Assert.AreEqual("LessThan", GetColumnValue("Dna", "ComparisonType", (int)dnaId));
        Assert.AreEqual(typeof(TestPromoterEnum).AssemblyQualifiedName, GetColumnValue("Dna", "EnumType", (int)dnaId));
    }

    [TestMethod]
    public void TestSyncDnaStrandDoesNotModifyOrdinalOnUpdate()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "Original", null, 7, "GreaterThan");

        DnaHelperMethods.SyncDnaStrand(
            new DnaStrand { Id = (int)dnaId, Name = "Updated" }, (int)chromosomeId, cascade: false);

        Assert.AreEqual(7L, Convert.ToInt64(GetColumnValue("Dna", "Ordinal", (int)dnaId)));
    }

    [TestMethod]
    public void TestSyncDnaStrandCascadeFalseIgnoresGenes()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        var strand = new DnaStrand
        {
            Name = "StrandA",
            Genes = new List<Gene> { new Gene { ProteinName = "ProteinA" } }
        };

        DnaHelperMethods.SyncDnaStrand(strand, (int)chromosomeId, cascade: false);

        Assert.AreEqual(0, CountRows("Gene"));
    }

    [TestMethod]
    public void TestSyncDnaStrandCascadeMixesUpdatesAndInserts()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", null, 1, "GreaterThan");
        long existingGeneId = InsertGene((int)dnaId);

        var strand = new DnaStrand
        {
            Id = (int)dnaId,
            Name = "StrandA",
            Genes = new List<Gene>
            {
                new Gene { Id = (int)existingGeneId, ProteinName = "UpdatedProtein" },
                new Gene { ProteinName = "NewProtein" }
            }
        };

        DnaHelperMethods.SyncDnaStrand(strand, (int)chromosomeId);

        Assert.AreEqual(2, CountRows("Gene"));
        Assert.AreEqual("UpdatedProtein", DnaHelperMethods.GetGene((int)existingGeneId).ProteinName);
        Assert.AreEqual(2, DnaHelperMethods.GetDnaStrand((int)dnaId).Genes.Count);
    }

    // -------------------------------------------------------------------------
    // SYNC GENE --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestSyncGeneInsertsAndLinksToParent()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", null, 1, "GreaterThan");

        int geneId = DnaHelperMethods.SyncGene(new Gene { ProteinName = "ProteinA" }, (int)dnaId);

        Assert.IsTrue(geneId > 0);
        Assert.IsTrue(LinkExists("DnaGene", "DnaId", (int)dnaId, "GeneId", geneId));
        Assert.AreEqual("ProteinA", DnaHelperMethods.GetGene(geneId).ProteinName);
    }

    [TestMethod]
    public void TestSyncGeneUpdatesExistingRow()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", null, 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.SyncGene(new Gene { Id = (int)geneId, ProteinName = "Updated" }, (int)dnaId);

        Assert.AreEqual(1, CountRows("Gene"));
        Assert.AreEqual("Updated", DnaHelperMethods.GetGene((int)geneId).ProteinName);
    }

    [TestMethod]
    public void TestSyncGeneWithNullParentIdDoesNotLink()
    {
        int geneId = DnaHelperMethods.SyncGene(new Gene { ProteinName = "Unlinked" }, null);

        Assert.IsNotNull(DnaHelperMethods.GetGene(geneId));
        Assert.AreEqual(0, CountRows("DnaGene"));
    }

    // -------------------------------------------------------------------------
    // SYNC — SHARED CHILD INSTANCES --------
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TestSyncSharedChromosomeUpdateVisibleToAllParents()
    {
        long nucleusA = InsertNucleusEntity(null, "A");
        long nucleusB = InsertNucleusEntity(null, "B");
        long chromosomeId = InsertChromosomeEntity("Original");
        LinkNucleusChromosome((int)nucleusA, (int)chromosomeId);
        LinkNucleusChromosome((int)nucleusB, (int)chromosomeId);

        DnaHelperMethods.SyncChromosome(
            new Chromosome { Id = (int)chromosomeId, Name = "Updated" }, (int)nucleusA, cascade: false);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)nucleusA).Chromosomes.Single().Name);
        Assert.AreEqual("Updated", DnaHelperMethods.GetNucleus((int)nucleusB).Chromosomes.Single().Name);
    }

    [TestMethod]
    public void TestSyncSharedGeneUpdateVisibleToAllParentStrands()
    {
        long nucleusId = InsertNucleus(null, "Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromA");
        long strandA = InsertDnaEntity("StrandA", null, 1, "GreaterThan");
        long strandB = InsertDnaEntity("StrandB", null, 1, "GreaterThan");
        LinkChromosomeDna((int)chromosomeId, (int)strandA);
        LinkChromosomeDna((int)chromosomeId, (int)strandB);
        long geneId = InsertGeneEntity();
        LinkDnaGene((int)strandA, (int)geneId);
        LinkDnaGene((int)strandB, (int)geneId);

        DnaHelperMethods.SyncGene(new Gene { Id = (int)geneId, ProteinName = "Updated" }, (int)strandA);

        Assert.AreEqual(1, CountRows("Gene"));
        Assert.AreEqual("Updated", DnaHelperMethods.GetDnaStrand((int)strandA).Genes.Single().ProteinName);
        Assert.AreEqual("Updated", DnaHelperMethods.GetDnaStrand((int)strandB).Genes.Single().ProteinName);
    }

    [TestMethod]
    public void TestSyncNucleusCascadeUpdatesSharedChildOnceViaBothPaths()
    {
        // The same Chromosome instance attached to a Nucleus twice (shared
        // trait reused). Cascading Sync must converge on one row and one link.
        long nucleusId = InsertNucleusEntity(null, "Root");
        var shared = new Chromosome { Name = "SharedTrait" };

        var nucleus = new Nucleus
        {
            Id = (int)nucleusId,
            Name = "Root",
            Chromosomes = new List<Chromosome> { shared, shared }
        };

        DnaHelperMethods.SyncNucleus(nucleus);

        Assert.AreEqual(1, CountRows("Chromosome"));
        Assert.AreEqual(1, CountRows("NucleusChromosome"));
        Assert.AreEqual(1, DnaHelperMethods.GetNucleus((int)nucleusId).Chromosomes.Count);
    }
}