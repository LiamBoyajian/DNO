namespace Main.main.packages.model.Dna;

public class Gene
{
    // Gene identity is tied to a unique gene type (ProteinName), not instance
    // data — so there is no ParentId. Parent links live in the DnaGene junction
    // table. A Gene may belong to any number of DnaStrands.
    public int Id { get; set; }
    public string ProteinName { get; set; }
}