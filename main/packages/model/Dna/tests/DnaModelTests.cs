using Main.main.packages.model.Dna;
using Main.Source.main;
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

        Assert.IsNull(promoter.Target);
        Assert.IsNull(promoter.ComparisonSymbol);
        Assert.IsFalse(promoter.IsPercent);
        Assert.AreEqual(0, promoter.ComparisonValue);
    }

    [TestMethod]
    public void TestPromoterSetProperties()
    {
        var promoter = new Promoter { ComparisonSymbol = ">=", ComparisonValue = 40 };

        Assert.AreEqual(">=", promoter.ComparisonSymbol);
        Assert.AreEqual(40, promoter.ComparisonValue);
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
        var strand = new DnaStrand { Id = 7, Name = "StrandA", Promoter = new Promoter { } };
        strand.Genes.Add(new Gene { Id = 2 });

        Assert.AreEqual(7, strand.Id);
        Assert.AreEqual("StrandA", strand.Name);
        Assert.AreEqual(1, strand.Genes.Count);
    }

    // CHROMOSOME --------

    [TestMethod]
    public void TestChromosomeDefaults()
    {
        var chromosome = new Chromosome();

        Assert.AreEqual(0, chromosome.Id);
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
        var chromosome = new Chromosome { Id = 4, Name = "ChromosomeA" };
        chromosome.DnaStrands.Add(new DnaStrand());

        Assert.AreEqual(4, chromosome.Id);
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

    // IPROTEINS --------
    // IProtein / IDirigent have no concrete implementation in the codebase yet,
    // so there is nothing to instantiate or exercise. Revisit once a class implements them.

    [TestClass]
    public class PromoterTests
    {
        private enum TestTarget
        {
            WaterAmount
        }

        private class FakeMaterialResource : IMaterialResource
        {
            public double Amount { get; set; }

            public double Max { get; set; }
        }

        // Promoter fields are assigned directly - PromoterText has no setter.
        // It is a read-only string composed as
        // {Target}{ComparisonSymbol}{ComparisonValue}{%}.

        [TestMethod]
        public void TestPromoterDefaults()
        {
            var promoter = new Promoter();

            Assert.IsNull(promoter.Target);
            Assert.IsNull(promoter.ComparisonSymbol);
            Assert.IsFalse(promoter.IsPercent);
            Assert.AreEqual(0, promoter.ComparisonValue);
        }

        [TestMethod]
        public void TestPromoterTextComposesSymbolAndValue()
        {
            var promoter = new Promoter { ComparisonSymbol = ">", ComparisonValue = 50 };

            Assert.AreEqual(">50", promoter.PromoterText);
        }

        [TestMethod]
        public void TestPromoterTextAppendsPercentSign()
        {
            var promoter = new Promoter
            {
                ComparisonSymbol = ">=", ComparisonValue = 75, IsPercent = true
            };

            Assert.AreEqual(">=75%", promoter.PromoterText);
        }

        [TestMethod]
        public void TestPromoterTextPrefixesTarget()
        {
            var promoter = new Promoter
            {
                Target = TestTarget.WaterAmount,
                ComparisonSymbol = ">=",
                ComparisonValue = 75,
                IsPercent = true
            };

            Assert.AreEqual("WaterAmount>=75%", promoter.PromoterText);
        }

        [TestMethod]
        public void TestPromoterTextOnDefaultsIsZero()
        {
            // No Target, no symbol; ComparisonValue is 0 and is always
            // appended, so the composed string is "0".
            Assert.AreEqual("0", new Promoter().PromoterText);
        }

        [TestMethod]
        public void TestCompareStandardGreaterThan()
        {
            var promoter = new Promoter { ComparisonSymbol = ">", ComparisonValue = 50 };
            var resource = new FakeMaterialResource { Amount = 60, Max = 100 };

            Assert.IsTrue(promoter.Compare(resource));

            resource.Amount = 40;
            Assert.IsFalse(promoter.Compare(resource));
        }

        [TestMethod]
        public void TestComparePercentBased()
        {
            var promoter = new Promoter { ComparisonSymbol = ">=", ComparisonValue = 50, IsPercent = true };
            var resource = new FakeMaterialResource { Amount = 50, Max = 100 };

            Assert.IsTrue(promoter.Compare(resource));

            resource.Amount = 49;
            Assert.IsFalse(promoter.Compare(resource));
        }

        [TestMethod]
        public void TestCompareEqualsWithTolerance()
        {
            var promoter = new Promoter { ComparisonSymbol = "==", ComparisonValue = 100 };
            var resource = new FakeMaterialResource { Amount = 100.00001, Max = 200 };

            Assert.IsTrue(promoter.Compare(resource));
        }

        [TestMethod]
        public void TestCompareWildcardAlwaysTrue()
        {
            var promoter = new Promoter { ComparisonSymbol = "*=" };
            var resource = new FakeMaterialResource { Amount = 10, Max = 100 };

            Assert.IsTrue(promoter.Compare(resource));
        }

        [TestMethod]
        public void TestCompareNullResourceReturnsFalse()
        {
            var promoter = new Promoter { ComparisonSymbol = ">", ComparisonValue = 0 };

            Assert.IsFalse(promoter.Compare(null));
        }
    }
}