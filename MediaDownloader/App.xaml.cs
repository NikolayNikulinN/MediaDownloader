using MediaDownloader.Logger;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.ViewModels;
using MediaDownloader.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MediaDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? serviceProvider;

        public static IServiceProvider Services =>
            ((App)Current).serviceProvider
            ?? throw new InvalidOperationException("Application services are not initialized.");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);

            serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<MainPage>().Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            serviceProvider?.Dispose();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<AppConstants>();
            services.AddSingleton<ResourceWrapper>();
            services.AddSingleton<ILoggerWrapper, LoggerWrapper>();
            services.AddSingleton<HomePageViewModel>();
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<MainPage>();
        }
    }
}
