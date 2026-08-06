using GdUnit4;
using Main.main.packages.inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Main.tests;

[TestSuite, RequireGodotRuntime]
public class TestInventory
{
    public Inventory Inventory;

    [Before]
    public void ClassInit()
    {
        Inventory = new Inventory(5);
    }

    [After]
    public void ClassCleanup()
    {
    }

    [BeforeTest]
    public void TestInit()
    {
    }

    [AfterTest]
    public void TestCleanup()
    {
    }


    //Initialize inventory


    //Add item
    [TestCase]
    public void AddNullItem()
    {
    }

    [TestCase]
    public void AddOneItem()
    {
    }

    //Removing items

    //Dropping items

    //Picking up items

    //Holding/ Selecting items in inventory


    //Displaying held items
}