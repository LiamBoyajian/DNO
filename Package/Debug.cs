using System;
using Godot;

namespace Main.Package;

public partial class Debug : Node
{
    public override void _Ready()
    {
        var dna = new Dna(new Random(), 1000);
        //foreach (var a in dna)
        //{
        //    Console.Write(a + ", ");
        //    
        //}
        var polypep = new Polypeptide(dna);
        foreach (var a in polypep)
        {
            Console.Write(a + ", ");
        }
    }
}