using System;
using System.Collections.Generic;
using Godot;
using Main.Source.main;

namespace Main.main.scripts.core.plants.interfaces;

public partial interface IAttributeDictionary
{
}

public partial interface IAttributeEnumerable : IAttributeDictionary
{
    public IEnumerable<(string, double)> GetAttributeEnumerable();
}

public partial interface IMaterialEnumerable : IAttributeDictionary
{
    public IEnumerable<(string, IMaterialResource)> GetMaterialEnumerable();
}

/**
 * Can increase resource max
 */
public partial interface IUpgradable : IAttributeDictionary
{
    /**
     * where string corresponds to an enum key
     * if valid the plant will attempt to spend glucose to increase the "s" type of resource max
     */
    public bool ParseUpgrade(string s);
}

/**
 * Can obtain resources manually
 */
public partial interface IObtainable : IAttributeDictionary
{
    /**
     * where string corresponds to an enum key
     * if valid the plant will attempt to spend glucose to obtain the "s" type of resource
     */
    public bool ParseObtain(string s);
}

public partial interface IBroadcastsUpdate : IAttributeDictionary
{
    public event Action Updated;
}