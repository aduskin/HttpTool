using System.Windows;
using HttpTool.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HttpTool.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    public static IServiceProvider Services { get; private set; } = null!;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpToolServices();
                services.AddSingleton<MainWindow>();
            })
            .Build();

        Services = _host.Services;

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _host?.Dispose();
    }
}
