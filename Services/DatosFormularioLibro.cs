namespace BibliotecaPersonal.Services;

/// <summary>
/// Datos tal como el usuario los escribió en el formulario (texto sin validar).
/// La interfaz solo los recoge; el servicio se encarga de validarlos y convertirlos.
/// </summary>
public record DatosFormularioLibro(
    string? Titulo,
    string? Autor,
    string? Genero,
    string? AnioPublicacion,
    string? Paginas,
    int EstadoIndice,
    int Calificacion);
