using Godot;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Main.Package;


public partial class MyInventory : Container
{
    private Container _me;
    private Sprite2D _boxBase;
    private int _boxCount = 0;
    private int _boxPadding = 4; //The texture has a build in 1 pixel padding

    private int
        paddingLR; //The (Container X % (boxDiameter + padding)) / 2 : extra space that cannot fit a box; split between the left and right sides

    private int _boxDiameter;

    public MyInventory(Container parentContainer, Sprite2D boxSprite)
    {
        _me = parentContainer;
        _boxBase = boxSprite;

        //public Transform2D ScreenDimensions = 

        _boxDiameter = _boxBase.Texture.GetWidth();
        //_containerVector2 = _me.Size;

        //Console.WriteLine(_boxBase.Texture.GetWidth());
        //Console.WriteLine("container size: " + _me.Size.X);

        int paddingLR = (int)(_me.Size.X % (_boxBase.Texture.GetWidth() + _boxPadding) / 2);
        //Console.WriteLine("paddingLR: " + paddingLR);

        //Console.WriteLine("Max X Boxes:"+GetMaxXBoxes());
        //Console.WriteLine("Max Y Boxes:"+GetMaxYBoxes());

        //_generateBoxesVisual(1014);

        //Console.WriteLine("child count of me: "+_me.GetChildCount());
        CricketVisuals.GenerateSpriteGrid<Sprite2D>(_boxBase, _boxBase.Texture.GetSize(), _boxPadding, 9999, _me,
            _me.Size);

        const int boxRequestCount = 1000;
        _generateBoxesVisual(
            boxRequestCount); //TODO make a nice version using a more accurate count instead of a random int.
    }

    /**
     * Returns null if no valid box is found
     */
    public Sprite2D MouseCoordToBox(Vector2 coord)
    {
        int someValue = 2; //TODO make this look nice
        Vector2 boxDiscreteCoord = new Vector2((int)(coord.X / (_boxDiameter + someValue)),
            (int)(coord.Y / (_boxDiameter + someValue))); //TODO needs a limit on the click location

        if (boxDiscreteCoord.X > GetMaxXBoxes())
            boxDiscreteCoord.X = GetMaxXBoxes();

        if (boxDiscreteCoord.Y > GetMaxYBoxes())
            boxDiscreteCoord.Y = GetMaxYBoxes();

        int flattenedIndex = (int)(boxDiscreteCoord.X + boxDiscreteCoord.Y * GetMaxXBoxes());
        Node clickedBox = _me.FindChild("box" + flattenedIndex, false, false); //must be a sprite2D

        return (Sprite2D)clickedBox;
    }

    /**
         * Function used to generate inventory box sprites in a grid pattern (left to right descending).
         * Argument [int count] boxes to be added to the scene.
         * Return [int result] value that holds the number of boxes which could not be created. Total requested - Total made.
         * Uses _boxDiameter, paddingLR, _boxPadding, _me, _boxCount : used for formatting
         * Adds new boxes to parent=_me.
         *
         */
    [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    private int _generateBoxesVisual(int count)
    {
        var result = count;
        var nextBoxCoordinate =
            new Vector2((float)(_boxDiameter / 2 + paddingLR), (float)(_boxDiameter / 2 + _boxPadding)); //Shift 

        for (var i = 0; i < count; i++)
        {
            //TODO-Bug one extra horizontal line of boxes is being created

            if (nextBoxCoordinate.X + _boxDiameter + (_boxDiameter / 2) >
                (_me.Size.X - paddingLR)) //if the next box's X coordinate is out of bounds.
            {
                nextBoxCoordinate.X = (float)(_boxDiameter / 2 + paddingLR);
                nextBoxCoordinate.Y += _boxDiameter + _boxPadding;
            }

            if (nextBoxCoordinate.Y + (_boxDiameter / 2) >
                _me.Size.Y) //if the next box's Y coordinate is out of bounds.
                return result; // No space remaining

            Sprite2D newBox = new Sprite2D();
            newBox.Name = "box" + _boxCount;

            ++_boxCount;
            --result;

            newBox.Position = nextBoxCoordinate;
            nextBoxCoordinate.X += _boxDiameter;

            _me.AddChild(newBox);
            newBox.Texture = _boxBase.Texture;
        }

        return result;
    }

    public int GetMaxXBoxes()
    {
        return (int)(_me.Size.X / (_boxBase.Texture.GetWidth() + _boxPadding));
    }

    public int GetMaxYBoxes()
    {
        return (int)(_me.Size.Y / (_boxBase.Texture.GetHeight() + _boxPadding));
    }
}

public partial class MainLogic : Container
{
    // Called when the node enters the scene tree for the first time.

    public MyInventory PlayerInventory; //TODO see if I can make it private

    // Vector2 _containerVector2; TODO: remove if unused but i might want it for later im unsure right now
    public override void _Ready()
    {
        PlayerInventory = new MyInventory(GetNode<Container>("."), GetNode<Sprite2D>("_boxBase"));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButtonDown)
        {
            var box = PlayerInventory.MouseCoordToBox(eventMouseButtonDown.Position);
            if (box == null) return;
            box.Scale = new Vector2(2, 2);
        }
    }
}