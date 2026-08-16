using Main.main.packages.model.Dna;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Main.tests;

[TestClass]
public class DnaModelTests
{
    // GENE --------

    [TestMethod]
    public void TestGeneDefaults()
    {
        var gene = new Gene();

        Assert.AreEqual(0, gene.Id);
        Assert.IsNull(gene.ProteinName);
    }

    [TestMethod]
    public void TestGeneSetProperties()
    {
        var gene = new Gene { Id = 5, ProteinName = "Chlorophyll Synthase" };

        Assert.AreEqual(5, gene.Id);
        Assert.AreEqual("Chlorophyll Synthase", gene.ProteinName);
    }

    // PROMOTER --------

    [TestMethod]
    public void TestPromoterDefaults()
    {
        var promoter = new Promoter();

        Assert.AreEqual(0, promoter.Id);
        Assert.IsNull(promoter.Target);
        Assert.IsNull(promoter.ComparisonType);
    }

    [TestMethod]
    public void TestPromoterSetProperties()
    {
        var promoter = new Promoter { Id = 3, ComparisonType = "GreaterThan" };

        Assert.AreEqual(3, promoter.Id);
        Assert.AreEqual("GreaterThan", promoter.ComparisonType);
    }

    // DNASTRAND --------

    [TestMethod]
    public void TestDnaStrandDefaults()
    {
        var strand = new DnaStrand();

        Assert.AreEqual(-1, strand.Id);
        Assert.AreEqual("", strand.Name);
        Assert.IsNull(strand.Promoter);
        Assert.IsNotNull(strand.Genes);
        Assert.AreEqual(0, strand.Genes.Count);
    }

    [TestMethod]
    public void TestDnaStrandGenesListIsIndependentPerInstance()
    {
        var first = new DnaStrand();
        var second = new DnaStrand();

        first.Genes.Add(new Gene { Id = 1 });

        Assert.AreEqual(1, first.Genes.Count);
        Assert.AreEqual(0, second.Genes.Count);
    }

    [TestMethod]
    public void TestDnaStrandSetProperties()
    {
        var strand = new DnaStrand { Id = 7, Name = "StrandA", Promoter = new Promoter { Id = 1 } };
        strand.Genes.Add(new Gene { Id = 2 });

        Assert.AreEqual(7, strand.Id);
        Assert.AreEqual("StrandA", strand.Name);
        Assert.AreEqual(1, strand.Promoter.Id);
        Assert.AreEqual(1, strand.Genes.Count);
    }

    // CHROMOSOME --------

    [TestMethod]
    public void TestChromosomeDefaults()
    {
        var chromosome = new Chromosome();

        Assert.AreEqual(0, chromosome.Id);
        Assert.AreEqual(0, chromosome.ParentId);
        Assert.IsNull(chromosome.Name);
        Assert.IsNotNull(chromosome.DnaStrands);
        Assert.AreEqual(0, chromosome.DnaStrands.Count);
    }

    [TestMethod]
    public void TestChromosomeDnaStrandsListIsIndependentPerInstance()
    {
        var first = new Chromosome();
        var second = new Chromosome();

        first.DnaStrands.Add(new DnaStrand());

        Assert.AreEqual(1, first.DnaStrands.Count);
        Assert.AreEqual(0, second.DnaStrands.Count);
    }

    [TestMethod]
    public void TestChromosomeSetProperties()
    {
        var chromosome = new Chromosome { Id = 4, ParentId = 2, Name = "ChromosomeA" };
        chromosome.DnaStrands.Add(new DnaStrand());

        Assert.AreEqual(4, chromosome.Id);
        Assert.AreEqual(2, chromosome.ParentId);
        Assert.AreEqual("ChromosomeA", chromosome.Name);
        Assert.AreEqual(1, chromosome.DnaStrands.Count);
    }

    // NUCLEUS --------

    [TestMethod]
    public void TestNucleusDefaults()
    {
        var nucleus = new Nucleus();

        Assert.AreEqual(0, nucleus.Id);
        Assert.AreEqual(0, nucleus.ParentId);
        Assert.IsNull(nucleus.Name);
        Assert.IsNull(nucleus.Parent);
        Assert.IsNotNull(nucleus.Chromosomes);
        Assert.AreEqual(0, nucleus.Chromosomes.Count);
    }

    [TestMethod]
    public void TestNucleusChromosomesListIsIndependentPerInstance()
    {
        var first = new Nucleus();
        var second = new Nucleus();

        first.Chromosomes.Add(new Chromosome());

        Assert.AreEqual(1, first.Chromosomes.Count);
        Assert.AreEqual(0, second.Chromosomes.Count);
    }

    [TestMethod]
    public void TestNucleusSetProperties()
    {
        var parent = new Nucleus { Id = 1, Name = "Root" };
        var nucleus = new Nucleus { Id = 2, ParentId = 1, Name = "Child", Parent = parent };
        nucleus.Chromosomes.Add(new Chromosome());

        Assert.AreEqual(2, nucleus.Id);
        Assert.AreEqual(1, nucleus.ParentId);
        Assert.AreEqual("Child", nucleus.Name);
        Assert.AreEqual("Root", nucleus.Parent.Name);
        Assert.AreEqual(1, nucleus.Chromosomes.Count);
    }

    // EXECUTEPROTEINS --------

    [TestMethod]
    public void TestExecuteProteinsCanBeInstantiated()
    {
        var executeProteins = new ExecuteProteins();

        Assert.IsNotNull(executeProteins);
    }

    // IPROTEINS --------
    // IProteins / IDirigent have no concrete implementation in the codebase yet,
    // so there is nothing to instantiate or exercise. Revisit once a class implements them.
}