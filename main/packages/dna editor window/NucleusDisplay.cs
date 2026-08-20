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

    public TextureButton ChromosomeButton { get; private set; }

    public TextureButton DnaStrandButton { get; private set; }


    public Nucleus SelectedNucleus { get; private set; } = null;

    public Chromosome SelectedChromosome
    {
        get
        {
            if (ChromosomeButton == null) return null;
            ChromosomeDictionary.TryGetValue(ChromosomeButton, out Chromosome chromosome);
            return chromosome;
        }
        set
        {
            if (ChromosomeButton == null) return;
            if (!ChromosomeDictionary.ContainsKey(ChromosomeButton)) return;
            ChromosomeDictionary[ChromosomeButton] = value;
        }
    }

    public DnaStrand SelectedDna
    {
        get
        {
            if (DnaStrandButton == null) return null;
            DnaStrandDictionary.TryGetValue(DnaStrandButton, out DnaStrand dnaStrand);
            return dnaStrand;
        }
        set
        {
            if (DnaStrandButton == null) return;
            if (!DnaStrandDictionary.ContainsKey(DnaStrandButton)) return;
            DnaStrandDictionary[DnaStrandButton] = value;
        }
    }


    protected System.Collections.Generic.Dictionary<TextureButton, Chromosome> ChromosomeDictionary = new();
    protected System.Collections.Generic.Dictionary<TextureButton, DnaStrand> DnaStrandDictionary = new();

    [Export] public Container ElementContainer;


    [Signal]
    public delegate void UpdatedEventHandler();

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
            InitializeChromosome(chromosome);
        }
    }


    public void Populate(Chromosome chromosome)
    {
        ChildrenVisibility(false);
        DnaStrandDictionary.Clear();
        foreach (var dnaStrand in chromosome.DnaStrands)
        {
            InitializeDna(dnaStrand);
        }
    }

    protected void InitializeDna(DnaStrand dnaStrand)
    {
        var textureButton = new TextureButton();
        textureButton.ToggleMode = true;
        textureButton.TextureNormal = DnaTexture;
        textureButton.ButtonGroup = Buttons;
        textureButton.Name = dnaStrand.Id + dnaStrand.Name;
        DnaStrandDictionary.Add(textureButton, dnaStrand);
        ElementContainer.AddChild(textureButton);
    }

    protected void InitializeChromosome(Chromosome chromosome)
    {
        var textureButton = new TextureButton();
        textureButton.ToggleMode = true;
        textureButton.TextureNormal = ChromosomeTexture;
        textureButton.ButtonGroup = Buttons;
        textureButton.Name = chromosome.Id + chromosome.Name;
        ChromosomeDictionary.Add(textureButton, chromosome);
        ElementContainer.AddChild(textureButton);
    }

    private void PressHandler(BaseButton button)
    {
        if (button is not TextureButton textureButton) return;
        if (ChromosomeDictionary.TryGetValue(textureButton, out Chromosome chromosome))
        {
            //SelectedChromosome = chromosome;
            Populate(chromosome);
        }
        else if (DnaStrandDictionary.TryGetValue(textureButton, out DnaStrand dnStrand))
        {
            //SelectedDna = dnStrand;
            button.SelfModulate = new Color(5f, 5f, 5f);
        }

        EmitSignal(nameof(Updated));
    }

    protected void Initialize()
    {
        SelectedNucleus = null;
        Clear();
    }

    public void CloseOpenedElement(bool ifNucleusClear = false)
    {
        if (DnaStrandButton != null)
        {
            DnaStrandButton.SelfModulate = new Color(1.0f, 1.0f, 1.0f);
            DnaStrandButton = null;
            GD.Print("tests");
        }
        else if (ChromosomeButton != null)
        {
            ChromosomeButton = null;
            DnaStrandDictionary.Clear();
            ClearChildren(true);
            ChildrenVisibility(true);
        }
        else if (SelectedNucleus != null && ifNucleusClear)
        {
            Initialize();
        }

        EmitSignal(nameof(Updated));
    }

    public void Clear()
    {
        foreach (var child in ElementContainer.GetChildren())
        {
            child.QueueFree();
        }

        DnaStrandDictionary.Clear();
        ChromosomeDictionary.Clear();
        ChromosomeButton = null;
        DnaStrandButton = null;
    }

    /**
     * Unsafe, dictionary may still have references
     */
    private void ClearChildren(bool ignoreHidden = false)
    {
        foreach (var child in ElementContainer.GetChildren())
        {
            if (child.Get("visible").AsBool())
                child.QueueFree();
        }
    }

    private void ChildrenVisibility(bool show = false)
    {
        foreach (var child in ElementContainer.GetChildren())
        {
            if (child is not TextureButton textureButton) continue;
            if (show)
            {
                textureButton.Show();
            }
            else
            {
                textureButton.Hide();
            }
        }
    }

    public void CreateElement()
    {
        if (DnaStrandButton != null) return;
        if (ChromosomeButton != null)
        {
            var dnaStrand = new DnaStrand();
            SelectedChromosome.DnaStrands.Add(dnaStrand);

            InitializeDna(dnaStrand);
        }
        else if (SelectedNucleus != null)
        {
            var chromosome = new Chromosome();
            SelectedNucleus.Chromosomes.Add(chromosome);

            InitializeChromosome(chromosome);
        }
        else if (SelectedNucleus == null)
        {
            var nucleus = new Nucleus();
            nucleus.Name = "Nucleus draft";
            Populate(nucleus);
        }

        EmitSignal(nameof(Updated));
    }

    public void SetNameCurrentElement(string name = "unnamed")
    {
        if (DnaStrandButton != null)
        {
            SelectedDna.Name = name;
        }
        else if (ChromosomeButton != null)
        {
            SelectedChromosome.Name = name;
        }
        else if (SelectedNucleus != null)
        {
            SelectedNucleus.Name = name;
        }
    }

    public void RemoveSelectedElement()
    {
        if (DnaStrandButton != null)
        {
            var dnaButton = DnaStrandButton;
            DnaStrandDictionary.Remove(DnaStrandButton);
            dnaButton.QueueFree();
        }
        else if (ChromosomeButton != null)
        {
            var chromosomeButton = ChromosomeButton;
            ChromosomeDictionary.Remove(ChromosomeButton);
            chromosomeButton.QueueFree();
        }
        else if (SelectedNucleus != null)
        {
            Initialize();
        }
    }
}