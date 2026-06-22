using Godot;

namespace Main.Source.main;

public partial class Plant : Node
{
    //Abstract
    private float _health;
    private float _maxHealth;

    //Definite attributes
    private double _glucose;
    private double _h2O;
    private double _cO2;
    private double _oxygen;
    //I think sunlight shouldn't be a field. it isn't a component of the plant and has no reason to be stored.

    //hormones
    
    
    //circadian rhythm
    //injury
    private float _damagedCells; //maybe add types of cells or damage idk (types of broken proteins.)
    
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void Tick()
    {
        if(GetSunLevel() >= 0.0)_photosynthesize();
    }

    /**
     * TODO: STUB
     */
    public float GetSunLevel()
    {
        return 0.0f;
    }
    /**
     * ACTIONS: Make changes to a plant's resources
     *
     **/
    
    //Trade: swap one resource for another at a specific rate
    private void _trade(){
    
    }
    
    //Photosynthesize: yk what that is
    private void _photosynthesize(){
        //exponential but tapering
    }
    //Clean: remove a resource permanently
    private void _clean(){
    
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
    private void _cycle(){
    
    }
    
}