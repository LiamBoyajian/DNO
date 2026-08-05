using Godot;

namespace Main.main.packages.items;

public static class ItemHelperMethods
{
    //public static 
}

public partial interface IItem<out TType> where TType : GodotObject
{
    public TType DragIcon { get; }
    public TType Icon { get; }
}

public interface IDeployable
{
    public bool Deploy(Blueprint blueprint);

    public Blueprint GetBlueprint();

    public bool CanCarry();

    public void Collisions(bool enable);
    //TODO ensure these are each needed
}