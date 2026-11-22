using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BibliotecaPersonal.Views;

/// <summary>
/// Ventana modal de confirmación. Avalonia no trae un MessageBox integrado,
/// por eso se crea esta ventana sencilla que devuelve true (Eliminar) o false (Cancelar).
/// </summary>
public partial class DialogoConfirmacion : Window
{
    // Constructor sin parámetros requerido por el cargador de XAML de Avalonia.
    public DialogoConfirmacion()
    {
        InitializeComponent();
    }

    public DialogoConfirmacion(string mensaje) : this()
    {
        TxtMensaje.Text = mensaje;
    }

    private void BtnAceptar_Click(object? sender, RoutedEventArgs e) => Close(true);

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e) => Close(false);
}
