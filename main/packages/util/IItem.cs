using Godot;

namespace Main.main.scripts.core.util;

public partial interface IItem<out TType> where TType : GodotObject
{
    public TType DragIcon { get; }
    public TType Icon { get; }
}