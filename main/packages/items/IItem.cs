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
    public void Deploy(Node viewport, Vector2 pos);

    public Blueprint GetBlueprint();
    public bool CanCarry();
    //TODO ensure these are each needed
}