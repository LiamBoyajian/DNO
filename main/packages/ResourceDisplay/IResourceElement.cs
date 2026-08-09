using System;

namespace Main.main.packages.ResourceDisplay;

public interface IResourceElement
{
    public Enum Enum { get; set; }

    public bool SetEnum(Enum @enum)
    {
        if (Equals(Enum, @enum)) return true;
        if (Enum != null) return false;

        Enum = @enum;
        InitializeIcon();

        return true;
    }

    public void InitializeIcon();
}