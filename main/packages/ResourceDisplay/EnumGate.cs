using System;
using System.Collections.Generic;
using CommandLine;

namespace Main.main.packages.ResourceDisplay;

/**
 *
 */
public class EnumGate
{
    private Dictionary<Type, int[]> _dictionary = new Dictionary<Type, int[]>();


    /**
     * Values represent the allowed ordinals of the given enum type
     */
    public void CreateGate(Type @enumType, params int[] values)
    {
        ArgumentNullException.ThrowIfNull(@enumType);
        ArgumentNullException.ThrowIfNull(values);

        if (!@enumType.IsEnum) throw new ArgumentException(nameof(@enumType));

        _dictionary.Add(@enumType, values);
    }

    public (Type, int[]) RemoveGate(Type @enumType)
    {
        if (@enumType is null) throw new ArgumentNullException(nameof(@enumType));
        if (!@enumType.IsEnum) throw new ArgumentException(nameof(@enumType));
        if (!_dictionary.TryGetValue(@enumType, out var value))
            return (@enumType, null);

        var result = (@enumType, value);
        _dictionary.Remove(@enumType);

        return result;
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Contains(Type @enumType)
    {
        return _dictionary.ContainsKey(@enumType);
    }

    /**
     * Check whether the enum is allowed through.
     * If no filters are provided, defaults to true.
     * If filter is provided only the specified elements are allowed through.
     */
    public bool Permits(Enum @enum, bool trueOnNoRule = true)
    {
        if (_dictionary.Count == 0) return trueOnNoRule;
        if (!_dictionary.ContainsKey(@enum.GetType()))
            return trueOnNoRule;

        return BinarySearch(@enum.Cast<int>(), _dictionary[@enum.GetType()]);
    }

    public static bool BinarySearch(int lookup, int[] values)
    {
        if (values is null || values.Length == 0) return false;

        var left = 0;
        var right = values.Length;

        int midpoint;
        do
        {
            midpoint = (left + right) / 2;
            if (values[midpoint] == lookup)
                return true;
            if (values[midpoint] < lookup)
            {
                left = midpoint;
                midpoint = (left + right) / 2;
            }
            else
            {
                right = midpoint;
            }
        } while (midpoint > left);

        return false;
    }
}