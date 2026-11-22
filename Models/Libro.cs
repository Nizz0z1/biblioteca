namespace BibliotecaPersonal.Models;

/// <summary>
/// Representa un libro de la biblioteca personal.
/// Hereda Id y FechaRegistro de EntidadBase.
/// </summary>
public class Libro : EntidadBase
{
    public const int CalificacionMaxima = 5;

    // Campos privados: el acceso se controla mediante las propiedades (encapsulamiento).
    private string _titulo = string.Empty;
    private string _autor = string.Empty;
    private string _genero = string.Empty;
    private int _calificacion;

    /// <summary>Título del libro. Se guarda sin espacios sobrantes al inicio o al final.</summary>
    public string Titulo
    {
        get => _titulo;
        set => _titulo = (value ?? string.Empty).Trim();
    }

    public string Autor
    {
        get => _autor;
        set => _autor = (value ?? string.Empty).Trim();
    }

    public string Genero
    {
        get => _genero;
        set => _genero = (value ?? string.Empty).Trim();
    }

    public int AnioPublicacion { get; set; }

    public int Paginas { get; set; }

    public EstadoLectura Estado { get; set; } = EstadoLectura.PorLeer;

    /// <summary>
    /// Calificación de 0 (sin calificar) a 5 estrellas.
    /// El set impide que el objeto quede con un valor fuera de rango.
    /// </summary>
    public int Calificacion
    {
        get => _calificacion;
        set
        {
            if (value < 0 || value > CalificacionMaxima)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"La calificación debe estar entre 0 y {CalificacionMaxima}.");
            _calificacion = value;
        }
    }

    public Libro() { }

    public Libro(string titulo, string autor, string genero, int anioPublicacion,
                 int paginas, EstadoLectura estado, int calificacion)
    {
        Titulo = titulo;
        Autor = autor;
        Genero = genero;
        AnioPublicacion = anioPublicacion;
        Paginas = paginas;
        Estado = estado;
        Calificacion = calificacion;
    }

    // ----- Propiedades calculadas que usa la interfaz -----

    public string EstadoDescripcion => Estado switch
    {
        EstadoLectura.PorLeer => "Por leer",
        EstadoLectura.Leyendo => "Leyendo",
        EstadoLectura.Leido => "Leído",
        _ => "Desconocido"
    };

    public string Estrellas => Calificacion == 0 ? string.Empty : new string('★', Calificacion);

    /// <summary>Línea de detalle que se muestra debajo del título en la lista.</summary>
    public string Detalle
    {
        get
        {
            var partes = new List<string> { Autor, AnioPublicacion.ToString(), $"{Paginas} págs." };
            if (!string.IsNullOrEmpty(Genero)) partes.Add(Genero);
            partes.Add(EstadoDescripcion);
            var texto = string.Join(" · ", partes);
            return Calificacion > 0 ? $"{texto}  {Estrellas}" : texto;
        }
    }

    public bool EstaLeido => Estado == EstadoLectura.Leido;

    public override string Descripcion() => $"\"{Titulo}\" de {Autor}";

    public override string ToString() => Descripcion();
}
