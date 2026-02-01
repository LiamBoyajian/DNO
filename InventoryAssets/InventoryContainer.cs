using System;
using Godot;
using Main.Package;

namespace Main.InventoryAssets;

public partial class InventoryContainer : Inventory
{
    private Container _container;
    private TextureButton _button;
    private int _slots;
    private ButtonGroup _playerInventoryButtons;

    public InventoryContainer(Container container, int slots, TextureButton button) : base(slots)
    {
        _slots = slots > 0 ? slots : throw new ArgumentException("slots is < 0");
        _container = container ?? throw new ArgumentNullException(nameof(container), "Container is null");
        _button = button ?? throw new ArgumentNullException(nameof(button), "button is null");

        _button.Hide();

        this.AddChild(_container);
        this.AddChild(_button);
        //this.AddChild(_buttonGroup);
        //_playerInventoryButtons.
    }

    public InventoryContainer() : this((new Container()), 1, new TextureButton())
    {
    }

    public int GenNodeGrid(Vector2 slotSize)
    {
        //NEEDS to use containers.............
        _button.Show();
        var result = CricketVisuals.GenerateNodeGrid(_button,
            slotSize, 1, _slots, _container,
            _container.Size, new int?(0));
        _button.Hide();


        foreach (var button in _container.GetChildren())
        {
            button.AddToGroup(nameof(_playerInventoryButtons));
        }


        _playerInventoryButtons = ((TextureButton)_container.GetChildren()[0]).GetButtonGroup();

        return result;
    }

    public void Show()
    {
        _container.Visible = true;
    }

    public void Hide()
    {
        _container.Visible = false;
    }

    public void ToggleVisible()
    {
        _container.Visible = !_container.Visible;
    }

    public BaseButton GetPressedButton()
    {
        return _playerInventoryButtons.GetPressedButton();
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