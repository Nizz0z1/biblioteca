using System.Globalization;
using BibliotecaPersonal.Models;
using Microsoft.Data.Sqlite;

namespace BibliotecaPersonal.Data;

/// <summary>
/// DAO (Data Access Object): única clase que ejecuta SQL sobre la tabla Libros.
/// Implementa las operaciones CRUD con ADO.NET y comandos parametrizados.
/// </summary>
public class LibroDAO
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    private const string ColumnasSelect =
        "Id, Titulo, Autor, Genero, AnioPublicacion, Paginas, Estado, Calificacion, FechaRegistro";

    // ---------------------------------------------------------------- CREATE

    /// <summary>Inserta un libro y devuelve el Id generado por SQLite.</summary>
    public int Insertar(Libro libro)
    {
        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = @"
            INSERT INTO Libros (Titulo, Autor, Genero, AnioPublicacion, Paginas, Estado, Calificacion, FechaRegistro)
            VALUES (@titulo, @autor, @genero, @anio, @paginas, @estado, @calificacion, @fecha);
            SELECT last_insert_rowid();";

        AgregarParametros(comando, libro);
        comando.Parameters.AddWithValue("@fecha",
            libro.FechaRegistro.ToString(FormatoFecha, CultureInfo.InvariantCulture));

        var id = Convert.ToInt32(comando.ExecuteScalar());
        libro.Id = id;
        return id;
    }

    // ------------------------------------------------------------------ READ

    /// <summary>
    /// Devuelve todos los libros ordenados por título.
    /// Si se indica un filtro, busca coincidencias en el título, el autor o el género.
    /// </summary>
    public List<Libro> ObtenerTodos(string? filtro = null)
    {
        var libros = new List<Libro>();

        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = $@"
            SELECT {ColumnasSelect}
            FROM Libros
            WHERE @filtro IS NULL
               OR Titulo LIKE @filtro
               OR Autor  LIKE @filtro
               OR Genero LIKE @filtro
            ORDER BY Titulo COLLATE NOCASE;";

        object valorFiltro = string.IsNullOrWhiteSpace(filtro)
            ? DBNull.Value
            : $"%{filtro.Trim()}%";
        comando.Parameters.AddWithValue("@filtro", valorFiltro);

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            libros.Add(Mapear(lector));
        }

        return libros;
    }

    /// <summary>Busca un libro por su Id. Devuelve null si no existe.</summary>
    public Libro? ObtenerPorId(int id)
    {
        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = $"SELECT {ColumnasSelect} FROM Libros WHERE Id = @id;";
        comando.Parameters.AddWithValue("@id", id);

        using var lector = comando.ExecuteReader();
        return lector.Read() ? Mapear(lector) : null;
    }

    /// <summary>
    /// Indica si ya hay un libro con el mismo título y autor (sin distinguir mayúsculas).
    /// excluirId permite ignorar el propio registro cuando se está actualizando.
    /// </summary>
    public bool Existe(string titulo, string autor, int? excluirId = null)
    {
        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = @"
            SELECT COUNT(*) FROM Libros
            WHERE Titulo = @titulo COLLATE NOCASE
              AND Autor  = @autor  COLLATE NOCASE
              AND (@excluirId IS NULL OR Id <> @excluirId);";
        comando.Parameters.AddWithValue("@titulo", titulo);
        comando.Parameters.AddWithValue("@autor", autor);
        comando.Parameters.AddWithValue("@excluirId", (object?)excluirId ?? DBNull.Value);

        return Convert.ToInt64(comando.ExecuteScalar()) > 0;
    }

    // ---------------------------------------------------------------- UPDATE

    /// <summary>Actualiza un libro existente. Devuelve true si se modificó alguna fila.</summary>
    public bool Actualizar(Libro libro)
    {
        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = @"
            UPDATE Libros
            SET Titulo = @titulo,
                Autor = @autor,
                Genero = @genero,
                AnioPublicacion = @anio,
                Paginas = @paginas,
                Estado = @estado,
                Calificacion = @calificacion
            WHERE Id = @id;";

        AgregarParametros(comando, libro);
        comando.Parameters.AddWithValue("@id", libro.Id);

        return comando.ExecuteNonQuery() > 0;
    }

    // ---------------------------------------------------------------- DELETE

    /// <summary>Elimina un libro por su Id. Devuelve true si se eliminó alguna fila.</summary>
    public bool Eliminar(int id)
    {
        using var conexion = ConexionBD.CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = "DELETE FROM Libros WHERE Id = @id;";
        comando.Parameters.AddWithValue("@id", id);

        return comando.ExecuteNonQuery() > 0;
    }

    // ------------------------------------------------------------- Auxiliares

    /// <summary>Parámetros comunes de INSERT y UPDATE (evita repetir código).</summary>
    private static void AgregarParametros(SqliteCommand comando, Libro libro)
    {
        comando.Parameters.AddWithValue("@titulo", libro.Titulo);
        comando.Parameters.AddWithValue("@autor", libro.Autor);
        comando.Parameters.AddWithValue("@genero", libro.Genero);
        comando.Parameters.AddWithValue("@anio", libro.AnioPublicacion);
        comando.Parameters.AddWithValue("@paginas", libro.Paginas);
        comando.Parameters.AddWithValue("@estado", libro.Estado.ToString());
        comando.Parameters.AddWithValue("@calificacion", libro.Calificacion);
    }

    /// <summary>Convierte la fila actual del lector en un objeto Libro.</summary>
    private static Libro Mapear(SqliteDataReader lector)
    {
        var libro = new Libro(
            titulo: lector.GetString(lector.GetOrdinal("Titulo")),
            autor: lector.GetString(lector.GetOrdinal("Autor")),
            genero: lector.GetString(lector.GetOrdinal("Genero")),
            anioPublicacion: lector.GetInt32(lector.GetOrdinal("AnioPublicacion")),
            paginas: lector.GetInt32(lector.GetOrdinal("Paginas")),
            estado: Enum.Parse<EstadoLectura>(lector.GetString(lector.GetOrdinal("Estado"))),
            calificacion: lector.GetInt32(lector.GetOrdinal("Calificacion")));

        libro.Id = lector.GetInt32(lector.GetOrdinal("Id"));

        var textoFecha = lector.GetString(lector.GetOrdinal("FechaRegistro"));
        if (DateTime.TryParseExact(textoFecha, FormatoFecha, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out var fecha))
        {
            libro.FechaRegistro = fecha;
        }

        return libro;
    }
}
