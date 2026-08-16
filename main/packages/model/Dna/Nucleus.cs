using System.Collections.Generic;

namespace Main.main.packages.model.Dna;

/**
 * Hold chromosomes
 */
public class Nucleus
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    public Nucleus Parent { get; set; } = null;
    public List<Chromosome> Chromosomes { get; set; } = new List<Chromosome>();
}