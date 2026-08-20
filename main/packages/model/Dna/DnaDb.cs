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
     * Removes a Nucleus and its entire descendant-Nucleus subtree (Nucleus
     * remains one-to-one, so this still cascades downward through ParentId).
     * Any Chromosome that becomes orphaned as a result — no longer linked to
     * any Nucleus — is deleted too, cascading down through its DnaStrands and
     * Genes wherever those are also left orphaned. Returns true if the
     * Nucleus row existed and was deleted.
     */
    public bool RemoveNucleus(int id)
    {
        return DnaHelperMethods.RemoveNucleus(id);
    }

    /**
     * Unlinks a Chromosome from one parent Nucleus. If that was the
     * Chromosome's last remaining parent, deletes the Chromosome and cascades
     * down through any of its DnaStrands/Genes that become orphaned too.
     * Returns true if the link existed and was removed.
     */
    public bool RemoveChromosome(int nucleusId, int chromosomeId)
    {
        return DnaHelperMethods.RemoveChromosome(nucleusId, chromosomeId);
    }

    /**
     * Unlinks a DnaStrand from one parent Chromosome. If that was its last
     * remaining parent, deletes the DnaStrand and cascades down through any
     * Genes that become orphaned too. Returns true if the link existed and
     * was removed.
     */
    public bool RemoveDnaStrand(int chromosomeId, int dnaId)
    {
        return DnaHelperMethods.RemoveDnaStrand(chromosomeId, dnaId);
    }

    /**
     * Unlinks a Gene from one parent DnaStrand. If that was its last
     * remaining parent, deletes the Gene. Returns true if the link existed
     * and was removed.
     */
    public bool RemoveGene(int dnaId, int geneId)
    {
        return DnaHelperMethods.RemoveGene(dnaId, geneId);
    }

    /**
     * Inserts (or, if nucleus.Id already exists, reuses) a Nucleus row.
     * cascade = true also adds/links every Chromosome in nucleus.Chromosomes
     * (and transitively their DnaStrands/Genes). Returns the Nucleus's Id.
     */
    public int AddNucleus(Nucleus nucleus, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.AddNucleus(nucleus, parentId, cascade);
    }

    /**
     * Inserts (or reuses) a Chromosome row and links it to parentId.
     * cascade = true also adds/links every DnaStrand in
     * chromosome.DnaStrands (and transitively their Genes). Returns the
     * Chromosome's Id.
     */
    public int AddChromosome(Chromosome chromosome, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.AddChromosome(chromosome, parentId, cascade);
    }

    /**
     * Inserts (or reuses) a DnaStrand row and links it to parentId.
     * cascade = true also adds/links every Gene in strand.Genes. Returns the
     * DnaStrand's Id.
     */
    public int AddDnaStrand(DnaStrand strand, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.AddDnaStrand(strand, parentId, cascade);
    }

    /**
     * Inserts (or reuses) a Gene row and links it to parentId. Returns the
     * Gene's Id.
     */
    public int AddGene(Gene gene, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.AddGene(gene, parentId, cascade);
    }

    /**
     * Updates a Nucleus's Name and ParentId by Id. Returns true if it existed.
     */
    public bool UpdateNucleus(Nucleus nucleus)
    {
        return DnaHelperMethods.UpdateNucleus(nucleus);
    }

    /**
     * Updates a Chromosome's Name by Id. Returns true if it existed.
     */
    public bool UpdateChromosome(Chromosome chromosome)
    {
        return DnaHelperMethods.UpdateChromosome(chromosome);
    }

    /**
     * Updates a DnaStrand's Name/EnumType/ComparisonType by Id. Returns true
     * if it existed.
     */
    public bool UpdateDnaStrand(DnaStrand strand)
    {
        return DnaHelperMethods.UpdateDnaStrand(strand);
    }

    /**
     * Updates a Gene's ProteinName by Id. Returns true if it existed.
     */
    public bool UpdateGene(Gene gene)
    {
        return DnaHelperMethods.UpdateGene(gene);
    }

    /**
     * Upserts a Nucleus (update if it exists, insert if not) and, with
     * cascade = true, its whole attached subtree. Never unlinks children that
     * were removed from the object's collections. Returns the Nucleus's Id.
     */
    public int SyncNucleus(Nucleus nucleus, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.SyncNucleus(nucleus, parentId, cascade);
    }

    /**
     * Upserts a Chromosome, ensures its link to parentId, and with
     * cascade = true syncs its DnaStrands and Genes. Returns the Id.
     */
    public int SyncChromosome(Chromosome chromosome, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.SyncChromosome(chromosome, parentId, cascade);
    }

    /**
     * Upserts a DnaStrand, ensures its link to parentId, and with
     * cascade = true syncs its Genes. Returns the Id.
     */
    public int SyncDnaStrand(DnaStrand strand, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.SyncDnaStrand(strand, parentId, cascade);
    }

    /**
     * Upserts a Gene and ensures its link to parentId. Returns the Id.
     */
    public int SyncGene(Gene gene, int? parentId = null, bool cascade = true)
    {
        return DnaHelperMethods.SyncGene(gene, parentId, cascade);
    }
}