using System;
using Godot;
using Main.Package;

namespace Main.InventoryAssets;

public partial class InventoryContainer : Inventory
{
    private AnimatedSprite2D _animatedSprite = animatedSprite;
    private Container _container = container;

    public InventoryContainer() : base(12)
    {
        
        //implement
    }
    public InventoryContainer(int max, Container container, AnimatedSprite2D animatedSprite) : base(max)
    {
        _buttonBase.Show();
            CricketVisuals.GenerateNodeGrid(_buttonBase,
            _acidLetter.SpriteFrames.GetFrameTexture("A", 0).GetSize(), 1, dna.GetLength(), _container,
            _container.Size);
            _buttonBase.Hide();
        
            var index = 0;
            var containerChildren = _container.GetChildren();
    }
    

        foreach (var b in dna)
    {
        if (containerChildren.Count == index)
            break;

        if (containerChildren[index] is TextureButton)
        {
            var current = (TextureButton)containerChildren[index];
            //Console.WriteLine(b.ToString());
            current.TextureNormal = _acidLetter.SpriteFrames.GetFrameTexture(b.ToString(), 0);
            current.TextureHover = _acidLetter.SpriteFrames.GetFrameTexture(b.ToString(), 1);
            current.TexturePressed = _acidLetter.SpriteFrames.GetFrameTexture(b.ToString(), 2);
        }
        else
        {
            throw new ArgumentException("container children contains not AnimatedSprite2D");
        }

        //end
        ++index;
    }


    _acidLetter.Hide();
}