using System;
using System.Collections.Generic;
using Godot;
using Main.main.packages.model.Dna;
using Main.main.packages.ResourceDisplay;

namespace Main.main.packages.dna_editor_window;

public partial class NucleusDisplay : PanelContainer
{
    [Export] protected Texture2D ChromosomeTexture;
    [Export] protected Texture2D DnaTexture;
    protected ButtonGroup Buttons;
    public Nucleus SelectedNucleus { get; private set; } = null;
    public Chromosome SelectedChromosome { get; private set; } = null;
    public DnaStrand SelectedDna { get; private set; } = null;
    public System.Collections.Generic.Dictionary<TextureButton, Chromosome> ChromosomeDictionary = new();
    public System.Collections.Generic.Dictionary<TextureButton, DnaStrand> DnaStrandDictionary = new();

    [Export] public Container ElementContainer;


    [Signal]
    public delegate void DnaStrandSelectedEventHandler();

    public override void _Ready()
    {
        base._Ready();
        if (ElementContainer == null)
        {
            ElementContainer = GetChild<Container>(0);
            if (ElementContainer == null)
                throw new Exception("ElementContainer is null");
        }

        Buttons = new();
        Buttons.Pressed += PressHandler;
    }

    public void Populate(Nucleus nucleus)
    {
        if (nucleus == null) return;
        if (nucleus == SelectedNucleus) return;
        Initialize();
        SelectedNucleus = nucleus;
        foreach (var chromosome in nucleus.Chromosomes)
        {
            var textureButton = new TextureButton();
            textureButton.ToggleMode = true;
            textureButton.TextureNormal = ChromosomeTexture;
            textureButton.ButtonGroup = Buttons;
            textureButton.Name = chromosome.Id + chromosome.Name;
            ChromosomeDictionary.Add(textureButton, chromosome);
            ElementContainer.AddChild(textureButton);
        }
    }

    public void Populate(Chromosome chromosome)
    {
        ClearAllChildren();
        foreach (var dnaStrand in chromosome.DnaStrands)
        {
            var textureButton = new TextureButton();
            textureButton.ToggleMode = true;
            textureButton.TextureNormal = DnaTexture;
            textureButton.ButtonGroup = Buttons;
            textureButton.Name = dnaStrand.Id + dnaStrand.Name;
            DnaStrandDictionary.Add(textureButton, dnaStrand);
            ElementContainer.AddChild(textureButton);
        }
    }

    private void PressHandler(BaseButton button)
    {
        if (button is not TextureButton textureButton) return;
        if (ChromosomeDictionary.TryGetValue(textureButton, out Chromosome chromosome))
        {
            SelectedChromosome = chromosome;
            Populate(SelectedChromosome);
        }
        else if (DnaStrandDictionary.TryGetValue(textureButton, out DnaStrand dnStrand))
        {
            SelectedDna = dnStrand;
            EmitSignal(nameof(DnaStrandSelected));
        }

        button.SelfModulate = new Color(5f, 5f, 5f);
    }

    protected void ClearAllChildren()
    {
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }
    }

    protected void Initialize()
    {
        SelectedNucleus = null;
        SelectedChromosome = null;
        SelectedDna = null;
        ChromosomeDictionary.Clear();
        DnaStrandDictionary.Clear();
        ClearAllChildren();
    }

    public void CloseOpenedElement(bool ifNucleusClear = false)
    {
        if (SelectedDna != null)
        {
            if (Buttons.GetPressedButton() is TextureButton textureButton)
                textureButton.SelfModulate = new Color(1f, 1f, 1f);

            SelectedDna = null;
            EmitSignal(nameof(DnaStrandSelected));
        }
        else if (SelectedChromosome != null)
        {
            SelectedChromosome = null;
            DnaStrandDictionary.Clear();
            Populate(SelectedNucleus);
        }
        else if (SelectedNucleus != null && ifNucleusClear)
        {
            Initialize();
        }
    }
}