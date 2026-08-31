using System;
using System.Collections.Generic;
using System.Reflection;

namespace Main.main.packages.model.Dna;

public interface IProtein
{
    //void RequestProteinSynthesis();

    public static readonly Dictionary<string, Type> StringToProtein = new()
    {
        ["Dirigent"] = typeof(IDirigent),
        ["Florigen"] = typeof(IFlorigen),
        ["Gibberellin"] = typeof(IGibberellin),
    };

    private static readonly Dictionary<(Type ConcreteType, Type InterfaceType), MethodInfo> MethodCache = new();

    public static void RunGene(IProtein organism, Gene gene)
    {
        if (!StringToProtein.TryGetValue(gene.ProteinName ?? "", out var protein)) return;
        if (!protein.IsInstanceOfType(organism)) return;

        //AI changed this implementation and I'm not 100% on it right now but it works
        var concreteType = organism.GetType();
        var key = (concreteType, protein);

        if (!MethodCache.TryGetValue(key, out var method))
        {
            // Resolve explicit interface implementation mapping
            var map = concreteType.GetInterfaceMap(protein);
            var targetIndex = Array.FindIndex(map.InterfaceMethods, m => m.Name == nameof(IProtein.RunProtein));

            if (targetIndex >= 0)
            {
                method = map.TargetMethods[targetIndex];
                MethodCache[key] = method;
            }
        }

        method?.Invoke(organism, null);
    }

    public double RunProtein() => -1;
}

public interface IGibberellin : IProtein
{
}

public interface IFlorigen : IProtein
{
}

public interface IDirigent : IProtein
{
}