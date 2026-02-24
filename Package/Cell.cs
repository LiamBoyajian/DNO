using System;
using Godot;
using Main.Package;

namespace Main.InventoryAssets;

public class Cell
{
    public outdated_DNA OutdatedDna;
    private byte promote; //indicates the quantity of the genes or smth like that
    private byte operate; //indicates the start of the gene segment

    //main-stats (my current thought is to instantiate these within the plant)
    private uint health;
    private float growthPercent;
    private uint flower; //product result 

    //sub-stats (while these are instantiated and influenced by the grower)
    private int temperature;
    private uint hydration;
    private uint light;

    /** <remarks>
     * Goal: i need to be able to encode all of these, or at least one initially, into the outdated_DNA's structure. I then should be able to manipulate each field's delta by changing the OutdatedDna.
     *      So the main things I need to do for the outdated_DNA encoding and decoding is first to build the functions of promoters and operators.
     *      I also need to develop some inherit system to determine the 'intensity' of the promoter.
     *      The operator should be easy since it just acts like a bookmark.
     *      Then I need some system for turning an mRNA strand into a protein (arguably not needed), but it could be cool for adding a research layer to OutdatedDna to protein.
     *
     *
     *      After these are done I would like to have some high level system for labeling any set of proteins, mrna strands, cells broadly etc. I would like it to end up as a research system.
     *      The issue even if I am able to successfully implement everything here is that I need a function and plant diversity because soy plants are mega boring on their own.
     *      Past that too there needs to be some use for this system that is interesting enough to drive players who don't care about the OutdatedDna part to engage with it.
     *      It should be natural and allow for depth where needed. I want every system to have a shortcut that forces you to cut through a thick jungle to discover the path.
     *
     *      My main objective was to use the plants to experiment with proteins to then apply it to other things like bacteria or animals or something.
     *      I really want some form of multiplayer too, but I don't know what I could do other than maybe coop.
     *      Online trading would be insanely cool. I could also do wipes like rust for an online mode. Still I need a mechanic that entices people to play from combat games.
     *      Maybe like clash of clans where you sabotage another player's base? Then the server work would be relatively easy too.
     *      Later I could add something like chess or Pokémon where you fight with the creature's you've designed?
     *      I could do something like Stardew where the combat part is required but usually one person in particular prefers to do it.
     *      In terms of combat it can't be crazy live since I'm doing 2d and don't have server experience yet. Which makes me lean towards something more like COC or Pokémon.
     *
     *      There is also the resource gathering element which I have not yet thought about. I like the idea of going into raids or something, Tarkov style.
     *      Links well with a market and items having value. I want it to be very diverse maybe like MMO, but I can't plan that far ahead yet. Resource gathering could be going to the location then processing the materials
     * </remarks>>
     */
    private void GenerateCellStat()
    {
        promote = 150;
        operate = 95;
    }

    public Cell()
    {
        RandomNumberGenerator random = new RandomNumberGenerator();
        OutdatedDna = new outdated_DNA(random);
        promote = (byte)random.RandfRange(0, 255);
        operate = (byte)random.RandfRange(0, 255);
        GenerateCellStat();
    }

    public String GetDnaString()
    {
        return OutdatedDna.ToString();
    }

    public outdated_DNA GetDna()
    {
        return OutdatedDna;
    }

    /**
     * Reads the outdated_DNA and returns an array of tuples with the first element [byte] being the mrna strand and the second [int] being the quantity;
     */
#nullable enable
    public (byte, int)[]? SendRibosome()
    {
        foreach (AcidBases b in OutdatedDna)
        {
        }

        byte rnaPolymerase;

        return null;
    }
}