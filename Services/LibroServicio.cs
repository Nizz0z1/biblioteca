using BibliotecaPersonal.Data;
using BibliotecaPersonal.Models;
using Microsoft.Data.Sqlite;

namespace BibliotecaPersonal.Services;

/// <summary>
/// Lógica del programa: valida los datos, aplica las reglas de negocio
/// y llama al DAO. La interfaz gráfica solo se comunica con esta clase.
/// </summary>
public class LibroServicio
{
    private const int LongitudMaximaTitulo = 150;
    private const int LongitudMaximaAutor = 100;
    private const int LongitudMaximaGenero = 50;
    private const int AnioMinimo = 1000;
    private const int PaginasMaximas = 20000;

    private readonly LibroDAO _dao;

    public LibroServicio() : this(new LibroDAO()) { }

    public LibroServicio(LibroDAO dao)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
    }

    // ------------------------------------------------------------------ READ

    public List<Libro> Listar(string? filtro = null) => _dao.ObtenerTodos(filtro);

    // ---------------------------------------------------------------- CREATE

    public ResultadoOperacion Registrar(DatosFormularioLibro datos)
    {
        var errores = ValidarYConstruir(datos, out var libro);
        if (errores.Count > 0) return ResultadoOperacion.ConErrores(errores);

        try
        {
            if (_dao.Existe(libro!.Titulo, libro.Autor))
                return ResultadoOperacion.Error($"Ya existe el libro {libro.Descripcion()} en la biblioteca.");

            var id = _dao.Insertar(libro);
            return ResultadoOperacion.Ok($"Libro {libro.Descripcion()} registrado correctamente (Id {id}).");
        }
        catch (SqliteException ex)
        {
            return ResultadoOperacion.Error($"No se pudo guardar en la base de datos: {ex.Message}");
        }
    }

    // ---------------------------------------------------------------- UPDATE

    public ResultadoOperacion Actualizar(int id, DatosFormularioLibro datos)
    {
        if (id <= 0) return ResultadoOperacion.Error("Seleccione un libro de la lista para modificarlo.");

        var errores = ValidarYConstruir(datos, out var libro);
        if (errores.Count > 0) return ResultadoOperacion.ConErrores(errores);

        try
        {
            if (_dao.Existe(libro!.Titulo, libro.Autor, excluirId: id))
                return ResultadoOperacion.Error($"Ya existe otro libro {libro.Descripcion()}.");

            libro.Id = id;
            return _dao.Actualizar(libro)
                ? ResultadoOperacion.Ok($"Libro {libro.Descripcion()} actualizado correctamente.")
                : ResultadoOperacion.Error("El libro ya no existe en la base de datos. Actualice la lista.");
        }
        catch (SqliteException ex)
        {
            return ResultadoOperacion.Error($"No se pudo actualizar en la base de datos: {ex.Message}");
        }
    }

    // ---------------------------------------------------------------- DELETE

    public ResultadoOperacion Eliminar(int id)
    {
        if (id <= 0) return ResultadoOperacion.Error("Seleccione un libro de la lista para eliminarlo.");

        try
        {
            var libro = _dao.ObtenerPorId(id);
            if (libro is null)
                return ResultadoOperacion.Error("El libro ya no existe en la base de datos. Actualice la lista.");

            return _dao.Eliminar(id)
                ? ResultadoOperacion.Ok($"Libro {libro.Descripcion()} eliminado.")
                : ResultadoOperacion.Error("No se pudo eliminar el libro.");
        }
        catch (SqliteException ex)
        {
            return ResultadoOperacion.Error($"No se pudo eliminar en la base de datos: {ex.Message}");
        }
    }

    // ------------------------------------------------------------ Validación

    /// <summary>
    /// Valida cada campo del formulario. Si no hay errores, construye el objeto Libro.
    /// Devuelve la lista de errores encontrados (vacía si todo es correcto).
    /// </summary>
    private static List<string> ValidarYConstruir(DatosFormularioLibro datos, out Libro? libro)
    {
        libro = null;
        var errores = new List<string>();

        var titulo = (datos.Titulo ?? string.Empty).Trim();
        var autor = (datos.Autor ?? string.Empty).Trim();
        var genero = (datos.Genero ?? string.Empty).Trim();

        if (titulo.Length == 0)
            errores.Add("El título es obligatorio.");
        else if (titulo.Length > LongitudMaximaTitulo)
            errores.Add($"El título no puede superar {LongitudMaximaTitulo} caracteres.");

        if (autor.Length == 0)
            errores.Add("El autor es obligatorio.");
        else if (autor.Length > LongitudMaximaAutor)
            errores.Add($"El autor no puede superar {LongitudMaximaAutor} caracteres.");
        else if (autor.Any(char.IsDigit))
            errores.Add("El nombre del autor no debe contener números.");

        if (genero.Length > LongitudMaximaGenero)
            errores.Add($"El género no puede superar {LongitudMaximaGenero} caracteres.");

        var anioActual = DateTime.Now.Year;
        if (!int.TryParse(datos.AnioPublicacion?.Trim(), out var anio))
            errores.Add("El año de publicación debe ser un número entero (ej: 1967).");
        else if (anio < AnioMinimo || anio > anioActual)
            errores.Add($"El año de publicación debe estar entre {AnioMinimo} y {anioActual}.");

        if (!int.TryParse(datos.Paginas?.Trim(), out var paginas))
            errores.Add("El número de páginas debe ser un número entero.");
        else if (paginas < 1 || paginas > PaginasMaximas)
            errores.Add($"El número de páginas debe estar entre 1 y {PaginasMaximas}.");

        if (!Enum.IsDefined(typeof(EstadoLectura), datos.EstadoIndice))
            errores.Add("Seleccione un estado de lectura.");

        if (datos.Calificacion < 0 || datos.Calificacion > Libro.CalificacionMaxima)
            errores.Add($"La calificación debe estar entre 0 y {Libro.CalificacionMaxima}.");

        // Regla de negocio: solo se califica un libro que ya se terminó de leer.
        if (datos.Calificacion > 0 && datos.EstadoIndice != (int)EstadoLectura.Leido)
            errores.Add("Solo se pueden calificar los libros en estado \"Leído\".");

        if (errores.Count == 0)
        {
            libro = new Libro(titulo, autor, genero, anio, paginas,
                              (EstadoLectura)datos.EstadoIndice, datos.Calificacion);
        }

        return errores;
    }
}
