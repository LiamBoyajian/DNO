using System;
using System.IO;
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
        command.CommandText = """
                              DELETE FROM Gene;
                              DELETE FROM Dna;
                              DELETE FROM Chromosome;
                              DELETE FROM NucleusDisplay;
                              DELETE FROM sqlite_sequence WHERE name IN ('Gene', 'Dna', 'Chromosome', 'NucleusDisplay');
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


    // INSERT HELPERS --------

    private static long InsertNucleus(int? parentId, string name)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO NucleusDisplay (ParentId, Name) VALUES (@ParentId, @Name);";
            command.Parameters.AddWithValue("@ParentId", (object)parentId ?? DBNull.Value);
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT last_insert_rowid();";
            return (long)command.ExecuteScalar();
        }
    }

    private static long InsertChromosome(int parentId, string name)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Chromosome (ParentId, Name) VALUES (@ParentId, @Name);";
            command.Parameters.AddWithValue("@ParentId", parentId);
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT last_insert_rowid();";
            return (long)command.ExecuteScalar();
        }
    }

    private static long InsertDna(int parentId, string name, string enumType, int ordinal, string comparisonType)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = """
                                  INSERT INTO Dna (ParentId, Name, EnumType, Ordinal, ComparisonType)
                                  VALUES (@ParentId, @Name, @EnumType, @Ordinal, @ComparisonType);
                                  """;
            command.Parameters.AddWithValue("@ParentId", parentId);
            command.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
            command.Parameters.AddWithValue("@EnumType", (object)enumType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Ordinal", ordinal);
            command.Parameters.AddWithValue("@ComparisonType", (object)comparisonType ?? DBNull.Value);
            command.ExecuteNonQuery();
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT last_insert_rowid();";
            return (long)command.ExecuteScalar();
        }
    }

    private static long InsertGene(int parentId)
    {
        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Gene (ParentId) VALUES (@ParentId);";
            command.Parameters.AddWithValue("@ParentId", parentId);
            command.ExecuteNonQuery();
        }

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = "SELECT last_insert_rowid();";
            return (long)command.ExecuteScalar();
        }
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
        Assert.AreEqual("GreaterThan", strand.Promoter.ComparisonType);
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
        Assert.AreEqual("", strand.Promoter.ComparisonType);
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
        // ProteinName has no backing column in the Gene table yet, so it stays unset.
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
        Assert.AreEqual((int)nucleusId, chromosome.ParentId);
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
        Assert.AreEqual("GreaterThan", strand.Promoter.ComparisonType);
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
        Assert.AreEqual("", strand.Promoter.ComparisonType);
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
        // ProteinName has no backing column in the Gene table yet, so it stays unset.
        Assert.IsNull(gene.ProteinName);
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

    // REMOVE CHROMOSOME --------

    [TestMethod]
    public void TestRemoveChromosomeReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.RemoveChromosome(9999));
    }

    [TestMethod]
    public void TestRemoveChromosomeReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");

        Assert.IsTrue(DnaHelperMethods.RemoveChromosome((int)chromosomeId));
        Assert.IsNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    [TestMethod]
    public void TestRemoveChromosomeCascadesToDnaAndGene()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveChromosome((int)chromosomeId);

        Assert.IsNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveChromosomeDoesNotAffectParentNucleusOrSiblings()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long siblingId = InsertChromosome((int)nucleusId, "ChromosomeB");

        DnaHelperMethods.RemoveChromosome((int)chromosomeId);

        Assert.IsNotNull(DnaHelperMethods.GetNucleus((int)nucleusId));
        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)siblingId));
    }

    // REMOVE DNA STRAND --------

    [TestMethod]
    public void TestRemoveDnaStrandReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.RemoveDnaStrand(9999));
    }

    [TestMethod]
    public void TestRemoveDnaStrandReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        Assert.IsTrue(DnaHelperMethods.RemoveDnaStrand((int)dnaId));
        Assert.IsNull(DnaHelperMethods.GetDnaStrand((int)dnaId));
    }

    [TestMethod]
    public void TestRemoveDnaStrandCascadesToGene()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveDnaStrand((int)dnaId);

        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveDnaStrandDoesNotAffectParentChromosome()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");

        DnaHelperMethods.RemoveDnaStrand((int)dnaId);

        Assert.IsNotNull(DnaHelperMethods.GetChromosome((int)chromosomeId));
    }

    // REMOVE GENE --------

    [TestMethod]
    public void TestRemoveGeneReturnsFalseWhenNotFound()
    {
        Assert.IsFalse(DnaHelperMethods.RemoveGene(9999));
    }

    [TestMethod]
    public void TestRemoveGeneReturnsTrueAndDeletesRow()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        Assert.IsTrue(DnaHelperMethods.RemoveGene((int)geneId));
        Assert.IsNull(DnaHelperMethods.GetGene((int)geneId));
    }

    [TestMethod]
    public void TestRemoveGeneDoesNotAffectParentDnaStrand()
    {
        long nucleusId = InsertNucleus(null, "Plant Root");
        long chromosomeId = InsertChromosome((int)nucleusId, "ChromosomeA");
        long dnaId = InsertDna((int)chromosomeId, "StrandA", "SomeEnumType", 1, "GreaterThan");
        long geneId = InsertGene((int)dnaId);

        DnaHelperMethods.RemoveGene((int)geneId);

        var strand = DnaHelperMethods.GetDnaStrand((int)dnaId);
        Assert.IsNotNull(strand);
        Assert.AreEqual(0, strand.Genes.Count);
    }
}