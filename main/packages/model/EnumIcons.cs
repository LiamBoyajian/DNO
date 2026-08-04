using System;
using System.Collections.Generic;
using Godot;
using Main.main.scripts.core.plants;

namespace Main.main.scripts.model;

[GlobalClass]
public partial class EnumIcons : Resource
{
    Type EnumType { get; set; }
    protected Dictionary<Enum, Texture2D> IconMapping = new();

    //public Texture2D GetIcon(Enum key)
    //{
    //    return GetValueOrDefault(IconMapping, key.GetHashCode());
    //}
}