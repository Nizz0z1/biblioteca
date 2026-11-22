namespace BibliotecaPersonal.Models;

/// <summary>
/// Clase abstracta con los datos comunes a cualquier entidad que se guarde
/// en la base de datos (identificador y fecha de registro).
/// Si en el futuro se agregan otras entidades (por ejemplo, Autor o Prestamo),
/// heredan de esta clase y reutilizan estos miembros.
/// </summary>
public abstract class EntidadBase
{
    /// <summary>
    /// Identificador asignado por SQLite. El set es "internal": solo las clases
    /// del propio proyecto (el DAO y el servicio) pueden asignarlo; la interfaz no lo modifica.
    /// </summary>
    public int Id { get; internal set; }

    public DateTime FechaRegistro { get; internal set; } = DateTime.Now;

    /// <summary>Indica si la entidad aún no ha sido guardada en la base de datos.</summary>
    public bool EsNueva => Id == 0;

    /// <summary>Cada entidad define cómo se describe a sí misma (polimorfismo).</summary>
    public abstract string Descripcion();
}
