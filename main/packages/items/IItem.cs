using Godot;

namespace Main.main.packages.items;

public static class ItemHelperMethods
{
    //public static 
}

public partial interface IItem
{
    public Vector2 Position { get; set; }
    public Texture2D DragIcon { get; }
    public Texture2D Icon { get; }

    public Texture2D HeldIcon { get; }

    public void Initialize();

    public void Reparent(Node newParent, bool keepGlobalTransform = false);
}

public interface IDeployable
{
    public bool Deploy(Blueprint blueprint);

    public Blueprint GetBlueprint();

    public bool CanCarry();

    public void Collisions(bool enable);
    //TODO ensure these are each needed
}