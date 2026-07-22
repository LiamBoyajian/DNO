using Godot;

namespace Main.main.scripts.core.util.inventory;

public interface IGeneral
{
}

public interface IItemCollision : IGeneral
{
    private void _itemEnteredEventHandler(Node2D node)
    {
    }
}