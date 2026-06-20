using MediaDownloader.Extensions;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MediaDownloader;

/// <summary>
/// Configures and starts the WPF application.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddMediaDownloader();

        serviceProvider = services.BuildServiceProvider();
        serviceProvider
            .GetRequiredService<ILoggerWrapper>()
            .Write(LogConstants.ApplicationStarted);
        serviceProvider.GetRequiredService<MainPage>().Show();
    }

    /// <inheritdoc />
    protected override void OnExit(ExitEventArgs e)
    {
        var logger = serviceProvider?.GetService<ILoggerWrapper>();
        logger?.Write(LogConstants.ApplicationStopped);
        logger?.CloseAndFlush();
        serviceProvider?.Dispose();

        base.OnExit(e);
    }
}
