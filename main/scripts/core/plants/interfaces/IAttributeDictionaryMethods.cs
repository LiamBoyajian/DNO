using System;
using System.Collections.Generic;
using Main.Source.main;

namespace Main.main.scripts.core.plants.interfaces;

public partial interface IAttributeEnumerable
{
    public IEnumerable<(string, double)> GetAttributeEnumerable();
}

public partial interface IMaterialEnumerable
{
    public IEnumerable<(string, IMaterialResource)> GetMaterialEnumerable();
}

public partial interface IUpgradable
{
    public bool ParseUpgrade(string s);
}