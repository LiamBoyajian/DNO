using System.Collections.Generic;

namespace Main.main.packages.model.Dna;

public class DnaStrand
{
    public int Id { get; set; } = -1;
    public string Name { get; set; } = "";
    public Promoter Promoter { get; set; }
    public List<Gene> Genes;

    public DnaStrand()
    {
        Genes = new List<Gene>();
    }
}