using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Main.main.scripts.model;
using Main.Source.main;
using PlantGui = Main.main.scripts.core.util.PlantGui;
using System.Linq;
using Main.main.packages.model.Dna;
using Main.main.packages.plants.enums;
using Main.main.packages.plants.interfaces;
using ContainPlant = Main.main.packages.containers.ContainPlant;

namespace Main.main.scripts.core.plants;

public abstract partial class AbstractMicrochipPlant(int dbId)
    : AbstractPlant, IConcatEnumerable, IDirigent, IBroadcastsUpdate
{
    protected AbstractMicrochipPlant() : this(-1)
    {
    }

    [Export] public PlantGui GuiManager { get; set; }
    [Export] protected double ConvertHpToGluRatio = .1;
    [Export] protected double ConvertGluToEnergyRatio = .05;

    /**
     * Seconds
     */
    [Export] protected double SecondsPerTick = 10;

    protected int MaxStrands;
    public event Action Updated;

    [Export] protected int DbId = dbId;
    protected Nucleus Nucleus;
    protected string DatabasePath = ProjectSettings.GlobalizePath("user://greenhouse.db");

    protected ContainPlant
        MyContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GuiManager ??= GetChild<PlantGui>(0);
        GuiManager.DugUp += DigUp;
    }

    public void Init()
    {
        if (DbId >= 0)
        {
            ConnectToPlant();
        }
        else
        {
            Nucleus = null;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //Console.Write(FrameSum);
        //
        if (IsAlive())
            Tick(delta);
    }

    public override bool Tick(double delta)
    {
        FrameSum += delta;
        if (FrameSum < SecondsPerTick)
            return false;

        ObtainGlucose();

        EnergyHp(ConvertHpToGluRatio, ConvertGluToEnergyRatio);

        CheckPromoters();

        IsDeadThenDeadFrame();
        FrameSum = 0.0;
        return true;
    }

    public void CheckPromoters()
    {
        if (Nucleus == null) return;
        var completeEnumDictionary = ((IConcatEnumerable)this).GetDictionary();
        foreach (var chromosome in Nucleus.Chromosomes)
        {
            foreach (var dna in chromosome.DnaStrands)
            {
                if (dna.Promoter.Target == null) continue;
                if (!completeEnumDictionary.ContainsKey(dna.Promoter.Target)) continue;
                if (dna.Promoter.Compare(completeEnumDictionary[dna.Promoter.Target]))
                    RunGenes(dna);
            }
        }
    }

    private void RunGenes(DnaStrand dna)
    {
        foreach (var gene in dna.Genes)
        {
            IProtein.RunGene(this, gene);
        }
    }

    public bool ConnectToPlant()
    {
        if (DbId < 0)
            return false;
        Nucleus = DnaDb.Instance.GetNucleus(DbId);
        return Nucleus != null;
    }

    protected override bool GrowthUpdateFrame()
    {
        if (IsDeadThenDeadFrame())
            return false; //plant died :(

        return true;
    }

    protected override bool IsDeadThenDeadFrame()
    {
        if (IsAlive())
            return false;
        GuiManager.DeadFrame();
        return true;
    }

    protected double DrawWater(double amount)
    {
        if (!MyContainer.HasWater()) return 0;
        return Resources[EnumLibrary.Rt.H2O].Give(MyContainer.Water.Take(amount));
    }

    public ContainPlant LinkParentContainer(ContainPlant container)
    {
        MyContainer = container;
        return container;
    }


    protected override bool GetAtmosphRatio()
    {
        return ContainPlant.GetAtmosphRatio();
    }

    public void SetDbId(int id)
    {
        if (id >= 0)
            DbId = id;
    }

    public override float GetSunLevel()
    {
        return MyContainer.GetSunlevel();
    }

    protected abstract double Photosynthesize();

    protected double ChangeResourceMax(EnumLibrary.Rt key, double change)
    {
        return Resources[key].ChangeMax(change);
    }

    public void PopupPlant()
    {
        packages.PlantPopup.PlantPopup.Instance.InitializeNode(this);
    }

    public abstract double GlucoseUpgradeFunction(double x);
    public abstract IEnumerable<(Enum, IMaterialResource)> GetDictionaryConcatEnumerable();
    public abstract IMaterialResource GetIMaterialResource(Enum @enum);


    double IDirigent.RunProtein()
    {
        var waterUptakeAmount = 100;
        var result = DrawWater(waterUptakeAmount);
        Updated?.Invoke();
        return result;
    }
}