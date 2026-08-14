using System;
using System.Collections.Generic;
using Main.Source.main;

namespace Main.main.packages.plants.interfaces;

public partial interface IAttributeDictionary
{
}

public partial interface IConcatEnumerable : IAttributeDictionary
{
    public IEnumerable<(Enum, IMaterialResource)> GetDictionaryConcatEnumerable();

    public Dictionary<Enum, IMaterialResource> GetDictionary()
    {
        var dict = new Dictionary<Enum, IMaterialResource>();
        foreach (var entry in GetDictionaryConcatEnumerable())
        {
            dict.Add(entry.Item1, entry.Item2);
        }

        return dict;
    }

    public IMaterialResource GetIMaterialResource(Enum @enum);
}

public partial interface IUpgradable : IAttributeDictionary
{
    public bool ParseUpgrade(Enum @enum);

    public double UpgradeCost(Enum @enum);
}

public partial interface IObtainable : IAttributeDictionary
{
    public bool ParseObtain(Enum @enum);

    public double ObtainCost(Enum @enum);
}

public partial interface IBroadcastsUpdate : IAttributeDictionary
{
    public event Action Updated;
}