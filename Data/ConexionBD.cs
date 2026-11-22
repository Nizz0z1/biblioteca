using Microsoft.Data.Sqlite;

namespace BibliotecaPersonal.Data;

/// <summary>
/// Centraliza la configuración de la conexión con SQLite y la creación de la base de datos.
/// </summary>
public static class ConexionBD
{
    private const string NombreArchivo = "biblioteca.db";

    private static readonly string _cadenaConexion;

    /// <summary>Ruta completa del archivo .db (se muestra en la interfaz).</summary>
    public static string RutaBaseDatos { get; }

    /// <summary>
    /// SQL que crea la tabla si no existe. Es el mismo contenido de Database/esquema.sql.
    /// </summary>
    private const string SqlCrearTablas = @"
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
        );";

    static ConexionBD()
    {
        // La base se guarda en la carpeta de datos del usuario y no en bin/,
        // así no se pierde al limpiar o recompilar el proyecto.
        // Windows: %LOCALAPPDATA%\BibliotecaPersonal   Linux: ~/.local/share/BibliotecaPersonal
        // macOS:   ~/Library/Application Support/BibliotecaPersonal
        var carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BibliotecaPersonal");
        Directory.CreateDirectory(carpeta);

        RutaBaseDatos = Path.Combine(carpeta, NombreArchivo);

        _cadenaConexion = new SqliteConnectionStringBuilder
        {
            DataSource = RutaBaseDatos,
            Mode = SqliteOpenMode.ReadWriteCreate // crea el archivo si no existe
        }.ToString();
    }

    /// <summary>
    /// Devuelve una conexión nueva (sin abrir). Quien la usa debe abrirla y liberarla
    /// con "using" para que el recurso se cierre siempre, incluso si ocurre un error.
    /// </summary>
    public static SqliteConnection CrearConexion() => new(_cadenaConexion);

    /// <summary>Crea la tabla Libros si todavía no existe. Se llama al iniciar la aplicación.</summary>
    public static void InicializarBaseDatos()
    {
        using var conexion = CrearConexion();
        conexion.Open();

        using var comando = conexion.CreateCommand();
        comando.CommandText = SqlCrearTablas;
        comando.ExecuteNonQuery();
    }
}
