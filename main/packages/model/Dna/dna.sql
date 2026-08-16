CREATE TABLE IF NOT EXISTS Nucleus
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    Name     TEXT,
    FOREIGN KEY (ParentId) REFERENCES Nucleus (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Chromosome
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    Name     TEXT,
    FOREIGN KEY (ParentId) REFERENCES Nucleus (Id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS Dna
(
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId       INTEGER,
    Name           TEXT,
    EnumType       TEXT,
    Ordinal        INTEGER,
    ComparisonType TEXT,
    FOREIGN KEY (ParentId) REFERENCES Chromosome (Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Gene
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    Protein TEXT,
    FOREIGN KEY (ParentId) REFERENCES Dna (Id) ON DELETE CASCADE
);
    


    
    
    