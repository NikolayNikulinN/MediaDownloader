using MediaDownloader.Extensions;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.IO;
using System.Windows;

namespace MediaDownloader;

/// <summary>
/// Configures and starts the WPF application.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? serviceProvider;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var services = new ServiceCollection();
        services.AddMediaDownloader();

        serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILoggerWrapper>();
        logger.Write(LogConstants.ApplicationStarted);

        if (!await EnsureRequiredToolsAsync(logger))
        {
            Shutdown();
            return;
        }

        var mainPage = serviceProvider.GetRequiredService<MainPage>();
        MainWindow = mainPage;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainPage.Show();
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

    private async Task<bool> EnsureRequiredToolsAsync(ILoggerWrapper logger)
    {
        var services = serviceProvider!;
        var toolBootstrapService = services.GetRequiredService<IToolBootstrapService>();
        var popupUtil = services.GetRequiredService<IUiPopupUtil>();
        var resourceWrapper = services.GetRequiredService<ResourceWrapper>();
        var missingTools = toolBootstrapService.GetMissingTools();

        if (missingTools.Count == 0)
        {
            return true;
        }

        var userConfirmed = popupUtil.ConfirmToolDownload(missingTools);
        if (!userConfirmed)
        {
            return false;
        }

        try
        {
            await popupUtil.ShowToolDownloadProgressAsync(progress => toolBootstrapService.DownloadMissingToolsAsync(
                                                                                                                      missingTools,
                                                                                                                      userConfirmed,
                                                                                                                      progress));

            var toolsStillMissing = toolBootstrapService.GetMissingTools();
            if (toolsStillMissing.Count > 0)
            {
                throw new FileNotFoundException(string.Join(", ", toolsStillMissing));
            }

            popupUtil.ShowMessage(resourceWrapper.ToolDownloadSucceededMessage);
            return true;
        }
        catch (Exception exception)
        {
            logger.Write(string.Format(
                                        CultureInfo.InvariantCulture,
                                        LogConstants.ToolBootstrapFailed,
                                        exception.Message),
                         LogLevel.Error);

            popupUtil.ShowError(string.Format(
                                               CultureInfo.CurrentCulture,
                                               resourceWrapper.ToolDownloadFailedMessage,
                                               exception.Message));
            return false;
        }
    }
}
