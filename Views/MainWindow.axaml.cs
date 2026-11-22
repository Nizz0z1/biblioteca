using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using BibliotecaPersonal.Data;
using BibliotecaPersonal.Models;
using BibliotecaPersonal.Services;

namespace BibliotecaPersonal.Views;

/// <summary>
/// Ventana principal. Solo maneja la interfaz: lee el formulario, responde a los eventos
/// y muestra resultados. Las validaciones y el acceso a datos están en LibroServicio y LibroDAO.
/// </summary>
public partial class MainWindow : Window
{
    private readonly LibroServicio _servicio = new();

    /// <summary>Libro elegido en la lista (null cuando se está creando uno nuevo).</summary>
    private Libro? _libroSeleccionado;

    public MainWindow()
    {
        InitializeComponent();

        TxtRutaBD.Text = $"Base de datos SQLite: {ConexionBD.RutaBaseDatos}";
        CargarLibros();
        ActualizarModoFormulario();
    }

    // =============================================================== Eventos

    private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
    {
        var resultado = _servicio.Registrar(LeerFormulario());
        MostrarResultado(resultado);

        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLibros(TxtBuscar.Text);
        }
    }

    private void BtnActualizar_Click(object? sender, RoutedEventArgs e)
    {
        if (_libroSeleccionado is null)
        {
            MostrarMensaje("Seleccione un libro de la lista para modificarlo.", esExito: false);
            return;
        }

        var resultado = _servicio.Actualizar(_libroSeleccionado.Id, LeerFormulario());
        MostrarResultado(resultado);

        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLibros(TxtBuscar.Text);
        }
    }

    private async void BtnEliminar_Click(object? sender, RoutedEventArgs e)
    {
        if (_libroSeleccionado is null)
        {
            MostrarMensaje("Seleccione un libro de la lista para eliminarlo.", esExito: false);
            return;
        }

        var dialogo = new DialogoConfirmacion(
            $"¿Seguro que desea eliminar el libro {_libroSeleccionado.Descripcion()}?\nEsta acción no se puede deshacer.");
        var confirmado = await dialogo.ShowDialog<bool>(this);
        if (!confirmado) return;

        var resultado = _servicio.Eliminar(_libroSeleccionado.Id);
        MostrarResultado(resultado);

        if (resultado.Exito)
        {
            LimpiarFormulario();
            CargarLibros(TxtBuscar.Text);
        }
    }

    private void BtnLimpiar_Click(object? sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
        BrdMensaje.IsVisible = false;
    }

    private void BtnBuscar_Click(object? sender, RoutedEventArgs e) => CargarLibros(TxtBuscar.Text);

    private void BtnMostrarTodos_Click(object? sender, RoutedEventArgs e)
    {
        TxtBuscar.Text = string.Empty;
        CargarLibros();
    }

    private void TxtBuscar_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) CargarLibros(TxtBuscar.Text);
    }

    /// <summary>Al elegir un libro de la lista, sus datos pasan al formulario para editarlo.</summary>
    private void LstLibros_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _libroSeleccionado = LstLibros.SelectedItem as Libro;

        if (_libroSeleccionado is not null)
        {
            TxtTitulo.Text = _libroSeleccionado.Titulo;
            TxtAutor.Text = _libroSeleccionado.Autor;
            TxtGenero.Text = _libroSeleccionado.Genero;
            TxtAnio.Text = _libroSeleccionado.AnioPublicacion.ToString();
            TxtPaginas.Text = _libroSeleccionado.Paginas.ToString();
            CmbEstado.SelectedIndex = (int)_libroSeleccionado.Estado;
            CmbCalificacion.SelectedIndex = _libroSeleccionado.Calificacion;
        }

        ActualizarModoFormulario();
    }

    // ============================================================ Auxiliares

    /// <summary>Consulta los libros (Read) y los muestra en la lista.</summary>
    private void CargarLibros(string? filtro = null)
    {
        try
        {
            var libros = _servicio.Listar(filtro);
            LstLibros.ItemsSource = libros;

            TxtListaVacia.IsVisible = libros.Count == 0;
            TxtTotal.Text = string.IsNullOrWhiteSpace(filtro)
                ? $"{libros.Count} libro(s) en la biblioteca"
                : $"{libros.Count} resultado(s) para \"{filtro.Trim()}\"";
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error al consultar la base de datos: {ex.Message}", esExito: false);
        }
    }

    private DatosFormularioLibro LeerFormulario() => new(
        TxtTitulo.Text,
        TxtAutor.Text,
        TxtGenero.Text,
        TxtAnio.Text,
        TxtPaginas.Text,
        CmbEstado.SelectedIndex,
        CmbCalificacion.SelectedIndex);

    private void LimpiarFormulario()
    {
        TxtTitulo.Text = string.Empty;
        TxtAutor.Text = string.Empty;
        TxtGenero.Text = string.Empty;
        TxtAnio.Text = string.Empty;
        TxtPaginas.Text = string.Empty;
        CmbEstado.SelectedIndex = 0;
        CmbCalificacion.SelectedIndex = 0;

        LstLibros.SelectedItem = null; // dispara SelectionChanged → _libroSeleccionado = null
        _libroSeleccionado = null;
        ActualizarModoFormulario();
        TxtTitulo.Focus();
    }

    /// <summary>
    /// Habilita los botones según el modo: "Guardar" para un libro nuevo,
    /// "Actualizar" y "Eliminar" cuando hay un libro seleccionado.
    /// </summary>
    private void ActualizarModoFormulario()
    {
        var hayLibro = _libroSeleccionado is not null;

        BtnGuardar.IsEnabled = !hayLibro;
        BtnActualizar.IsEnabled = hayLibro;
        BtnEliminar.IsEnabled = hayLibro;

        TxtModo.Text = hayLibro
            ? $"Editando: {_libroSeleccionado!.Titulo} (Id {_libroSeleccionado.Id})"
            : "Nuevo registro";
        TxtModo.Foreground = new SolidColorBrush(Color.Parse(hayLibro ? "#1565C0" : "#2E7D32"));
    }

    private void MostrarResultado(ResultadoOperacion resultado) =>
        MostrarMensaje(resultado.Mensaje, resultado.Exito);

    /// <summary>Muestra un aviso verde (éxito) o rojo (error) en la parte inferior.</summary>
    private void MostrarMensaje(string mensaje, bool esExito)
    {
        TxtMensaje.Text = mensaje;
        TxtMensaje.Foreground = new SolidColorBrush(Color.Parse(esExito ? "#1B5E20" : "#B71C1C"));
        BrdMensaje.Background = new SolidColorBrush(Color.Parse(esExito ? "#E8F5E9" : "#FFEBEE"));
        BrdMensaje.BorderBrush = new SolidColorBrush(Color.Parse(esExito ? "#A5D6A7" : "#EF9A9A"));
        BrdMensaje.IsVisible = true;
    }
}
