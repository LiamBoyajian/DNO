-- DNA schema — many-to-many edition
-- Every CREATE TABLE uses IF NOT EXISTS so EnsureSchema() is safe to re-run.
-- Junction tables carry ON DELETE CASCADE on both FK sides: removing either
-- endpoint of a link removes the junction row, which then triggers the
-- orphan-check logic in DnaHelperMethods before deciding whether to delete
-- the child entity.

-- ---------------------------------------------------------------------------
-- Entity tables
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Nucleus
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    Name     TEXT,
    FOREIGN KEY (ParentId) REFERENCES Nucleus (Id) ON DELETE CASCADE
);
-- ON DELETE CASCADE here only ever fires downward: deleting a Nucleus row
-- deletes any Nucleus rows whose ParentId points at it (its children), and
-- theirs in turn, recursively. It never reaches upward to an ancestor —
-- a row's own ParentId is untouched by its own deletion.

-- Chromosome, Dna, and Gene no longer carry a ParentId column.
-- All parent-child relationships live exclusively in the junction tables below.

CREATE TABLE IF NOT EXISTS Chromosome
(
    Id   INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NULL
);

CREATE TABLE IF NOT EXISTS Dna
(
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    Name           TEXT,
    EnumType       TEXT,
    Ordinal        INTEGER,
    ComparisonType TEXT
);

CREATE TABLE IF NOT EXISTS Gene
(
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
    Protein TEXT
);

-- ---------------------------------------------------------------------------
-- Junction tables
-- ---------------------------------------------------------------------------

-- Nucleus → Chromosome  (one nucleus can own many chromosomes;
--                        one chromosome can belong to many nuclei)
CREATE TABLE IF NOT EXISTS NucleusChromosome
(
    NucleusId    INTEGER NOT NULL REFERENCES Nucleus (Id) ON DELETE CASCADE,
    ChromosomeId INTEGER NOT NULL REFERENCES Chromosome (Id) ON DELETE CASCADE,
    PRIMARY KEY (NucleusId, ChromosomeId)
);

-- Chromosome → Dna  (one chromosome can own many strands;
--                    one strand can belong to many chromosomes)
CREATE TABLE IF NOT EXISTS ChromosomeDna
(
    ChromosomeId INTEGER NOT NULL REFERENCES Chromosome (Id) ON DELETE CASCADE,
    DnaId        INTEGER NOT NULL REFERENCES Dna (Id) ON DELETE CASCADE,
    PRIMARY KEY (ChromosomeId, DnaId)
);

-- Dna → Gene  (one strand can own many genes;
--              one gene can belong to many strands)
CREATE TABLE IF NOT EXISTS DnaGene
(
    DnaId  INTEGER NOT NULL REFERENCES Dna (Id) ON DELETE CASCADE,
    GeneId INTEGER NOT NULL REFERENCES Gene (Id) ON DELETE CASCADE,
    PRIMARY KEY (DnaId, GeneId)
);