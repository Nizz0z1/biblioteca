using Avalonia;

namespace BibliotecaPersonal;

/// <summary>
/// Punto de entrada de la aplicación. Configura e inicia Avalonia.
/// </summary>
internal class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
