using System;
using Godot;
using Main.Package;

namespace Main.InventoryAssets;

public partial class InventoryContainer : Inventory
{

    private Container _container;
    private TextureButton _button;
    private int _slots;
    public InventoryContainer(Container container, int slots, TextureButton button) : base(12)
    {
        
        _slots = slots < 0 ? slots : throw new ArgumentException("slots is < 0");
        _container = container ?? throw new ArgumentNullException(nameof(container), "Container is null");
        _button = button ?? throw new ArgumentNullException(nameof(button), "button is null");
        //implement
    }

    public int GenNodeGrid(int max, Vector2 slotSize)
    {
        //NEEDS to use containers.............
        _button.Show();
        var result = CricketVisuals.GenerateNodeGrid(_button,
            slotSize, 1, _slots, _container,
            _container.Size);
        
        _button.Hide();
        return result;
    }

   
    //public void UpdateButtonIcons(Sprite2D normal, Sprite2D hover, Sprite2D pressed)
    //{
    //    var containerChildren = _container.GetChildren();
    //    foreach (var child in containerChildren){
    //        
    //        if (child is not TextureButton button)
    //            throw new ArgumentException("container children contains not AnimatedSprite2D");
    //
    //        //_animatedSprite.SpriteFrames.GetFrameCount(_animatedSprite.Animation);
    //            
    //            //Passing an animated sprite is convenient. 
    //            button.TextureNormal = normal.Texture;
    //            button.TextureHover = hover.Texture;
    //            button.TexturePressed = hover.Texture; 
    //    }
    //    //_animatedSprite.Hide();
    //}
    //
    //public void UpdateButtonIcons(AnimatedSprite2D animSprite)
    //{
    //    
    //}
}