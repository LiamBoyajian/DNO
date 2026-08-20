using System.Collections.Generic;

namespace Main.main.packages.model.Dna;

/**
 * Holds strands of DNA.
 * Can be shared across multiple Nucleus instances (many-to-many).
 * ParentId removed — a Chromosome may belong to any number of parent Nuclei;
 * that relationship is recorded in the NucleusChromosome junction table.
 */
public class Chromosome
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<DnaStrand> DnaStrands { get; set; } = new List<DnaStrand>();
}