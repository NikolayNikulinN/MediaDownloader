using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Wrappers;
using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Presentation.Services;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Services;
using MediaDownloader.ViewModels;
using MediaDownloader.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace MediaDownloader.Extensions;

/// <summary>
/// Registers application dependencies.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all MediaDownloader services, view models, and views.
    /// </summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddMediaDownloader(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<AppConstants>();
        services.AddSingleton<ResourceWrapper>();
        services.AddSingleton<ILoggerWrapper, LoggerWrapper>();
        var httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{ServiceConstants.Tools.HttpUserAgentProductName}/{ServiceConstants.Tools.HttpUserAgentProductVersion}");
        services.AddSingleton(httpClient);
        services.AddSingleton<IXmlService, XmlService>();
        services.AddSingleton<IYtDlpService, YtDlpService>();
        services.AddSingleton<IFfmpegService, FfmpegService>();
        services.AddSingleton<IToolBootstrapService, ToolBootstrapService>();
        services.AddSingleton<IToolUpdateService, ToolUpdateService>();
        services.AddSingleton<IFolderDialogUtil, FolderDialogUtil>();
        services.AddSingleton<IShellUtil, ShellUtil>();
        services.AddSingleton<IUiPopupUtil, UiPopupUtil>();
        services.AddSingleton<HomePageViewModel>();
        services.AddSingleton<MainPageViewModel>();
        services.AddSingleton<MainPage>();

        return services;
    }
}
