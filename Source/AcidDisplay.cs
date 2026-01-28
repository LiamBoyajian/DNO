using System;
using Godot;
using Main.Package;

namespace Main.Source;

public partial class AcidDisplay : Container
{
    // Called when the node enters the scene tree for the first time.
    private Container _container;
    private AnimatedSprite2D _acidLetter;
    public DNA dna;

    public override void _Ready()
    {
        _container = this;
        _acidLetter = GetTree().GetCurrentScene().GetNode<AnimatedSprite2D>("AcidLetterBase");
        _acidLetter.Show();
        dna = new DNA(new RandomNumberGenerator());
        Console.WriteLine(_container.GetChildren()); //acidletter in by here

        CricketVisuals.GenerateSpriteGrid<AnimatedSprite2D>(_acidLetter,
            _acidLetter.SpriteFrames.GetFrameTexture("A", 0).GetSize(), 1, dna.GetLength(), _container,
            _container.Size);
        CricketVisuals.SetAcidBases(_container, dna);
        _acidLetter.Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}