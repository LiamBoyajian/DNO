using System;
using Godot;
using Main.Package;

namespace Main.Source;

public partial class AcidDisplay : Control
{
    // Called when the node enters the scene tree for the first time.
    private Container _container;
    private AnimatedSprite2D _acidLetter;
    private TextureButton _buttonBase;
    public DNA dna;


    public override void _Ready()
    {
        _container = GetNode<Container>("Container");
        _acidLetter = GetNode<AnimatedSprite2D>("AcidLetterBase");
        _buttonBase = GetNode<TextureButton>("ButtonBase");
        //Console.WriteLine(_container);
        dna = new DNA(new RandomNumberGenerator());
        //Console.WriteLine(_container.GetChildren()); //acidletter in by here
        _buttonBase.Show();
        CricketVisuals.GenerateNodeGrid(_buttonBase,
            _acidLetter.SpriteFrames.GetFrameTexture("A", 0).GetSize(), 1, dna.GetLength(), _container,
            _container.Size, null);
        _buttonBase.Hide();

        var index = 0;
        var containerChildren = _container.GetChildren();

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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}