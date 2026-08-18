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
        //Abstract:
        Health,
        Chlorophyll,
        Energy,

        //Definite attributes:
        Glucose,
        H2O,
        Co2,
        Oxygen,

        //hormones
        //circadian rhythm
        //injury types:
        DamagedCells, //maybe add types of cells or damage idk (types of broken proteins.)
        Null,
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