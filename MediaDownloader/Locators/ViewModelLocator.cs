using MediaDownloader.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MediaDownloader.Locators
{
    public sealed class ViewModelLocator
    {
        public MainPageViewModel MainPageViewModel =>
            App.Services.GetRequiredService<MainPageViewModel>();

        public HomePageViewModel HomePageViewModel =>
            App.Services.GetRequiredService<HomePageViewModel>();
    }
}
