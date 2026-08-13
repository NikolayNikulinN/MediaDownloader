using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;
using MediaDownloader.ViewModels;

namespace MediaDownloader.Tests.Support;

internal sealed class ViewModelTestContext
{
    public ViewModelTestContext(params UrlMediaItem[] savedItems)
    {
        XmlService.Settings = new ApplicationSettings { SaveFolderPath = TestConstants.OutputFolder };
        XmlService.State = new MediaDownloaderState { UrlItems = savedItems.ToList() };
    }

    public FakeYtDlpService YtDlpService { get; } = new();

    public TestLogger Logger { get; } = new();

    public ResourceWrapper ResourceWrapper { get; } = new();

    public AppConstants AppConstants { get; } = new();

    public FakeFolderDialogUtil FolderDialog { get; } = new();

    public FakeShellUtil Shell { get; } = new();

    public FakeUiPopupUtil UiPopup { get; } = new();

    public FakeXmlService XmlService { get; } = new();

    public FakeMediaDownloadService MediaDownloadService { get; } = new();

    public FakeToolUpdateService ToolUpdateService { get; } = new();

    public FakeToolBootstrapService ToolBootstrapService { get; } = new() { ToolsDirectoryPath = TestConstants.ToolsFolder };

    public HomePageViewModel CreateHomePageViewModel() =>
        new(
            YtDlpService,
            Logger,
            ResourceWrapper,
            AppConstants,
            FolderDialog,
            Shell,
            UiPopup,
            XmlService,
            MediaDownloadService);

    public MainPageViewModel CreateMainPageViewModel(HomePageViewModel homePageViewModel) =>
        new(
            AppConstants,
            ResourceWrapper,
            homePageViewModel,
            ToolUpdateService,
            ToolBootstrapService,
            UiPopup,
            Shell,
            Logger);
}
