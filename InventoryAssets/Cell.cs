using System;
using Godot;
namespace Main.InventoryAssets;

public abstract class Cell
{
    public DNA dna;


    public String getDNAString()
    {
        return dna.ToString();
    }

    /**
     * Returns a clone of the DNA.
     * testing this dumb commit thing because it's using the wrong account for some reason test2
     */
    public DNA getDNA()
    {
        return dna;
    }
}