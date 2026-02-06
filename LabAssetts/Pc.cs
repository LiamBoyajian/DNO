using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class Pc : AbstractMachine
{
    Pc()
    {
        Inventory = new InventoryContainer();
        Sprite = new AnimatedSprite2D();
    }
    
}