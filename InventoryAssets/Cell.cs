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
     */
    public DNA getDNA()
    {
        return dna;
    }
}