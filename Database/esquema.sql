-- Esquema de la base de datos de Biblioteca Personal (SQLite).
--
-- NO es necesario ejecutar este archivo manualmente: la aplicación ejecuta
-- este mismo CREATE TABLE IF NOT EXISTS al iniciar (Data/ConexionBD.cs).
-- Se incluye como referencia y por si se quiere crear la base a mano con:
--     sqlite3 biblioteca.db < Database/esquema.sql

CREATE TABLE IF NOT EXISTS Libros (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Titulo          TEXT    NOT NULL,
    Autor           TEXT    NOT NULL,
    Genero          TEXT    NOT NULL DEFAULT '',
    AnioPublicacion INTEGER NOT NULL,
    Paginas         INTEGER NOT NULL CHECK (Paginas > 0),
    Estado          TEXT    NOT NULL CHECK (Estado IN ('PorLeer', 'Leyendo', 'Leido')),
    Calificacion    INTEGER NOT NULL DEFAULT 0 CHECK (Calificacion BETWEEN 0 AND 5),
    FechaRegistro   TEXT    NOT NULL
);

-- Datos de ejemplo opcionales (descomentar para cargarlos):
-- INSERT INTO Libros (Titulo, Autor, Genero, AnioPublicacion, Paginas, Estado, Calificacion, FechaRegistro)
-- VALUES ('Cien años de soledad', 'Gabriel García Márquez', 'Novela', 1967, 471, 'Leido', 5, datetime('now', 'localtime')),
--        ('El principito', 'Antoine de Saint-Exupéry', 'Fábula', 1943, 96, 'PorLeer', 0, datetime('now', 'localtime'));
