using System;
using Main.main.packages.ResourceDisplay;
using Main.main.scripts.core.plants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Main.tests;

[TestClass]
public class TestEnumGate
{
    protected static EnumGate EnumGate;

    [ClassInitialize]
    public static void ClassInitializeEnumGate(TestContext testContext)
    {
        EnumGate = new EnumGate();
    }

    [TestCleanup]
    public void ClassCleanup()
    {
        EnumGate.Clear();
    }

    [TestMethod]
    public void Test()
    {
    }

    //CREATE GATE --------
    [TestMethod]
    public void TestCreateEmptyGate()
    {
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), new int[1]);
    }

    [TestMethod]
    public void TestCreateNullGate()
    {
        Assert.Throws<ArgumentException>(() => EnumGate.CreateGate(typeof(AbstractPlant.Rt), null));
    }

    [TestMethod]
    public void TestCreateNotEnum()
    {
        Assert.Throws<ArgumentException>(() => EnumGate.CreateGate(typeof(int), null));
    }

    [TestMethod]
    public void TestCreateNullEnum()
    {
        Assert.Throws<ArgumentNullException>(() => EnumGate.CreateGate(null, new int[1]));
    }

    [TestMethod]
    public void TestCreateGate()
    {
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), 1, 2, 3);
    }

    //REMOVE GATE --------
    [TestMethod]
    public void TestRemoveGateNull()
    {
        int[] temp = [1, 2, 3];
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.Throws<ArgumentNullException>(() => EnumGate.RemoveGate(null));
    }

    [TestMethod]
    public void TestRemoveGateNotEnum()
    {
        int[] temp = [1, 2, 3];
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.Throws<ArgumentException>(() => EnumGate.RemoveGate(null));
    }

    [TestMethod]
    public void TestRemoveGate()
    {
        int[] temp = [1, 2, 3];
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.AreEqual((typeof(AbstractPlant.Rt), temp), EnumGate.RemoveGate(typeof(AbstractPlant.Rt)));
    }

    //BINARY SEARCH --------
    [TestMethod]
    public void TestBinarySearch()
    {
        int[] temp = [1, 2, 3];

        Assert.IsTrue(EnumGate.BinarySearch(1, temp));
        Assert.IsTrue(EnumGate.BinarySearch(2, temp));
        Assert.IsTrue(EnumGate.BinarySearch(3, temp));
    }

    [TestMethod]
    public void TestBinarySearchOutOfBounds()
    {
        int[] temp = [1, 2, 3];

        Assert.IsFalse(EnumGate.BinarySearch(0, temp));
        Assert.IsFalse(EnumGate.BinarySearch(4, temp));
    }

    [TestMethod]
    public void TestBinarySearchNoVals()
    {
        int[] temp = [];

        Assert.IsFalse(EnumGate.BinarySearch(1, temp));
    }

    //CONTAINS --------

    [TestMethod]
    public void TestContains()
    {
        int[] temp = new[] { 1, 2, 3 };
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.IsTrue(EnumGate.Contains(typeof(AbstractPlant.Rt)));
        Assert.IsFalse(EnumGate.Contains(typeof(Enum)));
    }

    //PERMITS --------

    [TestMethod]
    public void TestPermitsOutOfBounds()
    {
        int[] temp = new[] { 1, 2, 3 };
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.IsFalse(EnumGate.Permits(AbstractPlant.Rt.Health));
    }

    [TestMethod]
    public void TestPermitsOnEmpty()
    {
        int[] temp = new int[3];
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.IsFalse(EnumGate.Permits(AbstractPlant.Rt.Chlorophyll));
    }

    [TestMethod]
    public void TestPermitsNoFilter()
    {
        int[] temp = new int[3];
        //EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.IsTrue(EnumGate.Permits(AbstractPlant.Rt.Chlorophyll));
    }

    [TestMethod]
    public void TestPermits()
    {
        int[] temp = new[] { 1, 2, 3 };
        EnumGate.CreateGate(typeof(AbstractPlant.Rt), temp);
        Assert.IsTrue(EnumGate.Permits(AbstractPlant.Rt.Chlorophyll));
        Assert.IsTrue(EnumGate.Permits(AbstractPlant.Rt.Energy));
        Assert.IsTrue(EnumGate.Permits(AbstractPlant.Rt.Glucose));
    }
}