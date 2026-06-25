CREATE TABLE plants
(
    id   INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
);

CREATE TABLE genes
(
    id        INTEGER PRIMARY KEY AUTOINCREMENT,
    plant_id  INTEGER NOT NULL,
    gene_name TEXT    NOT NULL,
    FOREIGN KEY (plant_id) REFERENCES plants (id) ON DELETE CASCADE
);

CREATE TABLE sub_genes
(
    id               INTEGER PRIMARY KEY AUTOINCREMENT,
    gene_id          INTEGER NOT NULL,
    expression_value REAL,
    FOREIGN KEY (gene_id) REFERENCES genes (id) ON DELETE CASCADE
);

