using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Main.main._Outside_Building;
using Main.main.packages.items;
using Main.main.packages.plants.enums;
using Main.main.scripts.core.plants;
using Main.main.scripts.core.util;
using Main.Source.main;
using AcceptSeed = Main.main.packages.util.AcceptSeed;

namespace Main.main.packages.containers;

[GlobalClass]
public partial class ContainPlant : Sprite2D, IDeployable, IWaterable
{
    public MaterialResource Water = new MaterialResource(500.0, 1000.0);

    protected AbstractPlant Plant = null;

    //public Vector2 SpawnPosition { get; private set; }
    [Export] private AcceptSeed _acceptSeed;

    // Called when the node enters the scene tree for the first time.
    protected const uint HealthCapacity = 100;

    protected bool Broken = false;

    protected Area2D Base;
    protected Area2D Interactable;

    public override void _Ready()
    {
        Base = GetChild<Area2D>(0);
        if (Base == null) throw new Exception("Base is null in: " + this);
        Interactable = GetChild<Area2D>(1);
        if (Interactable == null) throw new Exception("Interactable is null in: " + this);

        _acceptSeed = GetChild<AcceptSeed>(2);
        if (_acceptSeed == null) throw new Exception("Accept seed is null in: " + this);

        Base.InputEvent += InputHandler;
        Interactable.InputEvent += InputHandler;

        Base.AreaEntered += BaseEnteredHandler;
    }

    private void BaseEnteredHandler(Area2D area)
    {
        if (area.GetParent() is IPlantable p)
        {
        }
    }

    private void InputHandler(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event.IsActionPressed("left_click"))
        {
            if (Plant is AbstractMicrochipPlant mp)
            {
                mp.PopupPlant();
            }
        }

        if (@event.IsAction("lift_object"))
        {
            foreach (var area in Base.GetOverlappingAreas())
            {
                if (area.GetParent() is Player p)
                {
                    p.PickedUp(this);
                    return;
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public bool HasWater()
    {
        return Water.Amount > 0;
    }

    public Array<AbstractPlant> GetPlants()
    {
        Array<AbstractPlant> result =
            new Array<AbstractPlant>();

        foreach (var node in GetChildren())
        {
            if (node is not AbstractPlant child)
                continue;

            result.Add((AbstractPlant)node);
        }

        return result;
    }

    /**
     * Returns the total plants managed by this container
     */
    public int TotalPlants()
    {
        int result = 0;
        foreach (var node in GetChildren())
        {
            if (node is not AbstractPlant child)
                continue;
            ++result;
        }

        return result;
    }

    public int GetTotalLoad()
    {
        double result = GetPlants().Sum(plant =>
            plant.MyResources[EnumLibrary.Rt.Health].Amount);
        return (int)result;
    }

    public static bool GetAtmosphRatio()
    {
        throw new NotImplementedException();
    }

    /**
     * TODO: STUB
     * Get environmental sun level
     *
     */
    public float GetSunlevel()
    {
        return 1;
    }

    /**
     * Addchild
     *
     * returns: reference to the child node (null if unassigned)
     */
    public Node AcceptSeed(Node plant, int plantId)
    {
        if (plant == null) return null;
        if (plant is not AbstractPlant p)
            throw new ArgumentException("Node does not contain an AbstractPlant script");

        Plant = p;
        if (p is AbstractMicrochipPlant microchipPlant)
        {
            microchipPlant.LinkParentContainer(this);
            microchipPlant.SetDbId(plantId);
            microchipPlant.Init();
        }

        AddChild(plant);
        p.DugUp += DugUpEventHandler;

        return plant;
    }

    public void DugUpEventHandler()
    {
        Plant.DugUp -= DugUpEventHandler;
    }

    public bool Deploy(Blueprint blueprint)
    {
        if (blueprint == null) return false;
        blueprint.GetParent().AddChild(this);

        GlobalPosition =
            blueprint.GlobalPosition.Floor() -
            new Vector2(0, -1); //Old bug temp solution. Placement causes object to be one pixel higher

        blueprint.Hide();
        blueprint.QueueFree();

        return true;
    }

    public Blueprint GetBlueprint()
    {
        var result = new Blueprint();
        result.Texture = Texture;
        result.Area = (Area2D)GetChild<Area2D>(0).Duplicate();
        result.AddChild(result.Area);
        result.Offset = Offset;
        result.Centered = Centered;

        return result;
    }

    public bool CanCarry()
    {
        return true;
    }

    public void Collisions(bool enable)
    {
        Base.Monitorable = enable;
    }

    public Texture GetCarriedTexture()
    {
        return Texture;
    }

    public double GiveWater(double amount)
    {
        return Water.Give(amount);
    }
}