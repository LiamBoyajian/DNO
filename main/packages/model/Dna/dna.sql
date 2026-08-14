CREATE TABLE IF NOT EXISTS DNA
(
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId       INTEGER,
    Name           TEXT,
    EnumType       TEXT,
    Ordinal        INTEGER,
    ComparisonType TEXT
);

CREATE TABLE IF NOT EXISTS GENE
(
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    ParentId INTEGER,
    FOREIGN KEY (ParentId) REFERENCES DNA (Id) ON DELETE CASCADE
);
    

    
    
    
    