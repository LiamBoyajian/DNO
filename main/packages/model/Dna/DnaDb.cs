using Godot;

namespace Main.main.packages.model.Dna;

/**
 * Singleton (autoload) wrapper around DnaHelperMethods for runtime access
 * from other nodes. Mirrors the MemoryToDb pattern: static Instance set in
 * _Ready(), simple pass-through methods, exported config for the Inspector.
 */
public partial class DnaDb : Node
{
    public static DnaDb Instance { get; private set; }

    [Export] public string ConnectionPath = DnaHelperMethods.ConnectionPath;
    [Export] public string SchemaPath = DnaHelperMethods.SchemaPath;

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        DnaHelperMethods.ConnectionPath = ConnectionPath;
        DnaHelperMethods.SchemaPath = SchemaPath;
        DnaHelperMethods.ResetConnection();
        DnaHelperMethods.Initialize();
    }

    public override void _ExitTree()
    {
        DnaHelperMethods.ResetConnection();
        base._ExitTree();
    }

    /**
     * Loads a full NucleusDisplay -> Chromosome -> DnaStrand -> Gene tree by Id.
     * Set includeParent to true to also walk upward through ParentId.
     */
    public Nucleus GetNucleus(int id, bool includeParent = false)
    {
        return DnaHelperMethods.GetNucleus(id, includeParent);
    }

    /**
     * Loads a single Chromosome by Id, with its DnaStrands (and their
     * Genes) populated.
     */
    public Chromosome GetChromosome(int id)
    {
        return DnaHelperMethods.GetChromosome(id);
    }

    /**
     * Loads a single DnaStrand by Id, with its Genes populated.
     */
    public DnaStrand GetDnaStrand(int id)
    {
        return DnaHelperMethods.GetDnaStrand(id);
    }

    /**
     * Loads a single Gene by Id.
     */
    public Gene GetGene(int id)
    {
        return DnaHelperMethods.GetGene(id);
    }

    /**
     * Removes a NucleusDisplay and, via cascade, all of its Chromosomes,
     * DnaStrands, and Genes. Returns true if a row was deleted.
     */
    public bool RemoveNucleus(int id)
    {
        return DnaHelperMethods.RemoveNucleus(id);
    }

    /**
     * Removes a Chromosome and, via cascade, all of its DnaStrands and
     * Genes. Returns true if a row was deleted.
     */
    public bool RemoveChromosome(int id)
    {
        return DnaHelperMethods.RemoveChromosome(id);
    }

    /**
     * Removes a DnaStrand and, via cascade, all of its Genes. Returns true
     * if a row was deleted.
     */
    public bool RemoveDnaStrand(int id)
    {
        return DnaHelperMethods.RemoveDnaStrand(id);
    }

    /**
     * Removes a single Gene. Returns true if a row was deleted.
     */
    public bool RemoveGene(int id)
    {
        return DnaHelperMethods.RemoveGene(id);
    }
}