using System;
using System.Collections.Generic;
using CommandLine;
using Godot;
using Main.Source.main;

namespace Main.main.packages.ResourceDisplay;

public static class ResourceDisplayTools
{
    public static char Delimiter { get; private set; } = '_';
    public static char DelimiterAlternate { get; private set; } = '-';

    public static string DelimiterIdName(string className, Enum @enum, string suffix)
    {
        return
            $"{className}{DelimiterAlternate}{@enum.GetType().FullName?.Replace('.', '_')}{DelimiterAlternate}{@enum.Cast<int>()}{Delimiter}{suffix}";
    }

    public static Enum ConvertStringToEnum(string enumPath, int ordinal)
    {
        Type type = Type.GetType(enumPath ?? "");
        if (type is null || !type.IsEnum) throw new Exception(enumPath);

        return (Enum)Enum.ToObject(type, ordinal);
    }
}

public interface IResourceDisplay<out TNode> where TNode : Node
{
    //Not 100% needed?
    public ButtonGroup Buttons { get; }
    public bool ClearChildren();
    //public string ClassNamePrefix { get; set; }

    public bool AddElement((Enum, IMaterialResource) item, string suffix = "");

    /**
     * returns found progressbar; otherwise null
     */
    public TNode Find(Enum @enum, string suffix = "");

    /**
     * Attempts to update a progressbar with this key
     * returns updated progressbar; otherwise null
     */
    public TNode Update(Enum @enum, IMaterialResource material, string suffix = "*");

    //public void UpdateAll(IEnumerable<(string, IMaterialResource)> getMaterialEnumerable);
}