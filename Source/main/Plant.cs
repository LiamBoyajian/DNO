using System;
using System.Collections.Generic;
using Godot;

namespace Main.Source.main;

public partial class Plant : Node
{
    /**
     * ResourceTypes
     */
    public enum Rt
    {
    //Abstract:
    Health,
    MaxHealth,
    Chlorophyll,
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


    private Godot.Collections.Dictionary<Rt, double> _resources = new()
    {
        //Arbitrary base values
        { Rt.Health, 10.0 },
        { Rt.MaxHealth, 10.0 },
        { Rt.Chlorophyll, 100.0 },
        { Rt.Glucose, 0 },
        { Rt.H2O, 100.0 },
        { Rt.Co2, 100.0 },
        { Rt.Oxygen, 0 },

        { Rt.DamagedCells, 0 },

    };
    
    
    //-----------------------------
    public IReadOnlyDictionary<Rt, double> Resources => _resources;
    
    
    
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
        
        _resources[Rt.H2O] += 25.0;
        _resources[Rt.H2O] += 50.0;
        
        if(GetSunLevel() >= 0.0)
            _photosynthesize(GetSunLevel());

        
        Console.Write($"Glucose {_resources[Rt.Glucose]}");

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
    private void _trade(){
    
    }
    
    //Photosynthesize: yk what that is
    //should soon be exponential 
    //co2 one to one with water; sun is idk and idc rn
    private void _photosynthesize(float sunlevel)
    {
        const float oxygenByproductRatio = 6.0f;
        const float waterAndCo2Min = 6f;

        var glucoseGenerated = (int) ((Math.Max(_resources[Rt.H2O], _resources[Rt.Co2]) * sunlevel) / 6.0f); 
        _resources[Rt.Glucose] += glucoseGenerated;
        _resources[Rt.Oxygen] += glucoseGenerated * oxygenByproductRatio;
        _resources[Rt.H2O] -= glucoseGenerated * waterAndCo2Min;
        _resources[Rt.Co2] -= glucoseGenerated * waterAndCo2Min;
    }
    //Clean: remove a resource permanently
    private void _clean(Enum resource){
        if (resource is not Rt)
            throw new ArgumentException(resource.ToString() + " is not an Rt.");
        //stub not sure if I want here yet
        return; 
    }
    
    
    //Store: store specific resources in an organelle or plant structure
    private void _store(){
    
    }
    //retrieve: retrieve specific resources in an organelle or plant structure
    private void _retrieve(){
    
    }
    //Consume: Use resources to increase an attribute value
    private void _consume(){
    
    }
    //Grow: Use resources to increase an attribute maximum
    private void _grow(){
    
    }
    //Perform: Use resources to use an organ
    private void _perform(){
    
    }
    //Cycle: Tell the plant to change its hormonal state
    private void _cycle()
    {


    }
    
    

    
}