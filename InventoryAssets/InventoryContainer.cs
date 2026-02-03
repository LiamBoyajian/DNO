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
        _playerInventoryButtons = new ButtonGroup();
        _playerInventoryButtons.SetName("playerInventoryButtons");
        //this.AddChild(_buttonGroup);
        //_playerInventoryButtons.
    }


    //Required by Godot
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

        foreach (var node in _container.GetChildren())
        {
            if (node is not (TextureButton))
                throw new ArrayTypeMismatchException();

            TextureButton textureButton = node as TextureButton;
            textureButton.ButtonGroup = _playerInventoryButtons;
        }

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

    public void ClearPressedButtons()
    {
        if (_playerInventoryButtons.GetPressedButton() == null) return;
        _playerInventoryButtons.GetPressedButton().SetPressed(false);
    }
}