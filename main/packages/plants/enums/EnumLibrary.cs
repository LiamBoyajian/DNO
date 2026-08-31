using System;
using System.Collections.Generic;

namespace Main.main.packages.plants.enums;

public static class EnumLibrary
{
    public static readonly List<Type> Enums = new()
    {
        typeof(Rt),
        typeof(BasicOrgans)
    };

    public enum Rt
    {
        Health,
        Glucose,
        H2O,
        Co2,
        Energy
    }

    public enum Biproducts
    {
        Oxygen,
        DamagedCells,
    }

    public enum Organelles
    {
        Chlorophyll,
        Mitochondria,
    }

    public enum BasicOrgans
    {
        LeafStem,
        FlowerStem,
        Leaf,
        Root,
        Flower,
        Fruit,
    }
}