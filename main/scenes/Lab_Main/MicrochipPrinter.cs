using System;
using Godot;
using Main.main.packages.inventory;
using Main.main.packages.items;
using Main.main.packages.items.tools.microchip_injector;

namespace Main.main.scenes.Lab_Main;

public partial class MicrochipPrinter : AnimatedSprite2D
{
    //[Export] public PackedScene MicrochipScene;
    public int DbId { get; set; }

    public enum MachineState
    {
        Empty,
        Running,
        Complete
    }

    public Area2D Area { get; set; }

    public MachineState State { get; private set; } = MachineState.Empty;

    public override void _Ready()
    {
        base._Ready();
        //if (MicrochipScene == null) throw new Exception("MicrochipScene is null");
        if (GetChild(0) is not Area2D area2D) throw new Exception("Child 0 is not Area2d");
        Area = area2D;
        area2D.AreaEntered += AreaEnteredHandler;
        area2D.InputEvent += AreaClickedHandler;
    }

    private void AreaClickedHandler(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            Interact();
        }
    }

    //terrible method I just need to redo the inventory system completely.
    private void PickupItem()
    {
        if (State == MachineState.Complete)
        {
            State = MachineState.Empty;

            Inventory.Instance.RequestItemReturn();
            foreach (var n in Inventory.Instance.GetItems())
            {
                if (n is not ManagerIItem manager) continue;
                var item = manager.BorrowItem();

                if (item is not Injector injector)
                {
                    manager.ReturnItem();
                    continue;
                }

                injector.SetMicrochipId(DbId);
                Animation = "default";
                DbId = -1;

                manager.ReturnItem();
                manager.UpdateTexture();
                break;
            }
        }
    }

    public void PrintMicrochip()
    {
        if (DbId < 0) return;
        if (State == MachineState.Complete) return;
        Animation = "dispense";
        State = MachineState.Complete;
    }


    private void AreaEnteredHandler(Area2D area)
    {
        Interact();
    }

    private void Interact()
    {
        if (State == MachineState.Empty && DbId >= 0)
        {
            PrintMicrochip();
            return;
        }

        PickupItem();
    }
}