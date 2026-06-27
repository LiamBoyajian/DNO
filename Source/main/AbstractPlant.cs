using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using Main.Package;

namespace Main.Source.main;

public abstract partial class AbstractPlant : Node
{
    public class Resource(double amount, double max)
    {
        /**
         * Storage here?
         */


        public double Max { get; } = max;

        public double Amount { get; private set; } = amount;

        public double ReturnPercent()
        {
            if (Max == 0.0) return 0;
            return Amount / Max;
        }

        public double Give(double amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount + Amount > Max)
            {
                Amount = Max;
                return amount - (Max - Amount);
            }

            Amount += amount;
            return 0.0;
        }

        public double Take(double amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount > Amount)
            {
                var result = amount - Amount;
                Amount = 0;
                return result;
            }

            Amount -= amount;
            return 0.0;
        }

        public bool IsEmpty()
        {
            return Amount <= 0;
        }

        public bool Increment()
        {
            if (Amount + 1.0 >= Max) return false;
            Amount++;
            return true;
        }

        public bool Decrement()
        {
            if (Amount - 1.0 <= 0.0) return false;
            Amount--;
            return true;
        }
    }

    /**
     * ResourceTypes
     */
    public enum Rt
    {
        //Abstract:
        Health,
        MaxHealth,
        Chlorophyll,
        Energy,

        //Definite attributes:
        Glucose,
        H2O,
        Co2,
        Oxygen,

        //hormones
        //circadian rhythm
        //injury types:
        DamagedCells, //maybe add types of cells or damage idk (types of broken proteins.)
    }

    protected Dictionary<Rt, Resource> _resources = new()
    {
        //Arbitrary base values
        { Rt.Health, new Resource(10.0, 100.0) },
        //size attribute needed
        { Rt.Chlorophyll, new Resource(10.0, 100.0) },
        { Rt.Energy, new Resource(10.0, 100.0) },
        { Rt.Glucose, new Resource(10.0, 1000.0) },
        { Rt.H2O, new Resource(10.0, 200.0) },
        { Rt.Co2, new Resource(10.0, 100.0) },
        { Rt.Oxygen, new Resource(10.0, 100.0) },

        { Rt.DamagedCells, new Resource(50.0, 100.0) },
    };

    //-----------------------------
    public IReadOnlyDictionary<Rt, Resource> MyResources => _resources;

    private double _frameSum = 0.0;


    //-----------------------------

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (_resources[Rt.Health].Amount > 0.0)
            Tick(delta);
    }

    public void Tick(double delta)
    {
        _frameSum += delta;
        if (_frameSum < 5.0)
            return;
        _frameSum = 0.0;

        _resources[Rt.H2O].Give(25.0);
        _resources[Rt.Co2].Give(50.0);

        _consume();
        if (_resources[Rt.Health].Amount <= 0.0)
            Console.Write("\n\n PLANT DEAD \n");
        //TESTING TODO
        //if (GetSunLevel() >= 0.0)
        //    _photosynthesize(GetSunLevel());


        Console.Write($"Glucose  {_resources[Rt.Glucose].Amount}");
    }

    /**
     * TODO: STUB
     */
    public float GetSunLevel()
    {
        var TESTSUM = .8f;
        return TESTSUM;
    }

    /**
     * ACTIONS: Make changes to a plant's resources
     *
     **/

    //Trade: swap one resource for another at a specific rate
    private void _trade()
    {
    }

    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    private void _photosynthesize(float sunlevel)
    {
        const float oxygenByproductRatio = 6.0f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated =
            (int)((Math.Max(_resources[Rt.H2O].Amount, _resources[Rt.Co2].Amount) * sunlevel) / 6.0f);
        _resources[Rt.Glucose].Increment();
        _resources[Rt.Oxygen].Give(glucoseGenerated * oxygenByproductRatio);
        _resources[Rt.H2O].Take(glucoseGenerated * waterAndCo2Min);
        _resources[Rt.Co2].Take(glucoseGenerated * waterAndCo2Min);
    }

    //Clean: remove a resource permanently
    public void _clean(Enum resource)
    {
        if (resource is not Rt)
            throw new ArgumentException(resource.ToString() + " is not an Rt.");
        //stub not sure if I want here yet
    }


    //Store: store specific resources in an organelle or plant structure
    public void _store()
    {
    }

    //retrieve: retrieve specific resources in an organelle or plant structure
    public void _retrieve()
    {
    }

    //Consume: Use glucose for energy (no energy = lose hp)
    public void _consume()
    {
        if (_resources[Rt.Glucose].Decrement())
        {
            _resources[Rt.Energy].Give(10.0);
        }
        else
        {
            _resources[Rt.Health].Take(10.0);
        }
    }

    //Grow: Use resources to increase an attribute maximum
    public void _grow()
    {
        //size attribute needed: size creates a need for higher upkeep cost
    }

    //Perform: Use resources to use an organ
    public void _perform()
    {
    }

    //Cycle: Tell the plant to change its hormonal state
    public void _cycle()
    {
    }

    public bool IsAlive()
    {
        return _resources[Rt.Health].Amount > 0.0;
    }

    abstract protected bool GrowthUpdateFrame();

    abstract protected bool IsDeadThenDeadFrame();
}