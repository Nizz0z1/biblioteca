namespace BibliotecaPersonal.Services;

/// <summary>
/// Resultado que el servicio devuelve a la interfaz: si la operación fue exitosa
/// y el mensaje que se debe mostrar al usuario.
/// </summary>
public class ResultadoOperacion
{
    public bool Exito { get; }
    public string Mensaje { get; }
    public IReadOnlyList<string> Errores { get; }

    private ResultadoOperacion(bool exito, string mensaje, IReadOnlyList<string>? errores = null)
    {
        Exito = exito;
        Mensaje = mensaje;
        Errores = errores ?? Array.Empty<string>();
    }

    public static ResultadoOperacion Ok(string mensaje) => new(true, mensaje);

    public static ResultadoOperacion Error(string mensaje) => new(false, mensaje);

    public static ResultadoOperacion ConErrores(IReadOnlyList<string> errores) =>
        new(false,
            "Revise los datos del formulario:\n• " + string.Join("\n• ", errores),
            errores);
}
