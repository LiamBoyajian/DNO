using System.Collections.Generic;

namespace Main.main.packages.model.Dna;

/**
 * Holds strand of dna
 * Can be cloned and shared across plants
 */
public class Chromosome
{
    public int Id { get; set; }
    public int ParentId { get; set; }
    public string Name { get; set; }
    public List<DnaStrand> DnaStrands { get; set; } = new List<DnaStrand>();
}