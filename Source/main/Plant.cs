using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;

namespace Main.Source.main;

public partial class Plant : Node
{
    public class Resource(double amount, double max)
    {
        private double _amount = amount;
        private double _max = max;

        /**
         * Storage here?
         */


        public double Max => _max;

        public double Amount => _amount;

        public double ReturnPercent()
        {
            if (_max == 0.0) return 0;
            return _amount / _max;
        }

        public double give(double amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount + _amount > _max)
            {
                _amount = _max;
                return amount - (_max - _amount);
            }

            _amount += amount;
            return 0.0;
        }

        public double take(double amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

            if (amount > _amount)
            {
                var result = amount - _amount;
                _amount = 0;
                return result;
            }

            _amount -= amount;
            return 0.0;
        }

        public bool isEmpty()
        {
            return _amount <= 0;
        }

        public bool increment()
        {
            if (_amount + 1.0 >= _max) return false;
            _amount++;
            return true;
        }

        public bool decrement()
        {
            if (_amount - 1.0 <= 0.0) return false;
            _amount--;
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

    private Dictionary<Rt, Resource> _resources = new()
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
        Tick(delta);
    }

    public void Tick(double delta)
    {
        _frameSum += delta;
        if (_frameSum < 5.0)
            return;
        _frameSum = 0.0;

        _resources[Rt.H2O].give(25.0);
        _resources[Rt.Co2].give(50.0);

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
        _resources[Rt.Glucose].increment();
        _resources[Rt.Oxygen].give(glucoseGenerated * oxygenByproductRatio);
        _resources[Rt.H2O].take(glucoseGenerated * waterAndCo2Min);
        _resources[Rt.Co2].take(glucoseGenerated * waterAndCo2Min);
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
        if (_resources[Rt.Glucose].decrement())
        {
            _resources[Rt.Energy].give(10.0);
        }
        else
        {
            _resources[Rt.Health].take(10.0);
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
}