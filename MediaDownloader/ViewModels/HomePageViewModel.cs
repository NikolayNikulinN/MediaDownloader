using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Coordinates URL analysis and the home page user interface.
/// </summary>
public partial class HomePageViewModel : ObservableObject
{
    private readonly IFolderDialogUtil folderDialogService;
    private readonly ILoggerWrapper loggerWrapper;
    private readonly IShellUtil shellService;
    private readonly IUiPopupUtil uiPopupService;
    private readonly IXmlService xmlService;
    private readonly IYtDlpService ytDlpService;
    private readonly ApplicationSettings settings;

    [ObservableProperty]
    private string analysisStatus = string.Empty;

    [ObservableProperty]
    private bool isAnalyzing;

    [ObservableProperty]
    private string outputFolder;

    [ObservableProperty]
    private string url = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// </summary>
    public HomePageViewModel(
                             IYtDlpService ytDlpService,
                             ILoggerWrapper loggerWrapper,
                             ResourceWrapper resourceWrapper,
                             AppConstants appConstants,
                             IFolderDialogUtil folderDialogService,
                             IShellUtil shellService,
                              IUiPopupUtil uiPopupService,
                            IXmlService xmlService)
    {
        ArgumentNullException.ThrowIfNull(ytDlpService);
        ArgumentNullException.ThrowIfNull(loggerWrapper);
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(folderDialogService);
        ArgumentNullException.ThrowIfNull(shellService);
        ArgumentNullException.ThrowIfNull(uiPopupService);
        ArgumentNullException.ThrowIfNull(xmlService);

        this.ytDlpService = ytDlpService;
        this.loggerWrapper = loggerWrapper;
        this.folderDialogService = folderDialogService;
        this.shellService = shellService;
        this.uiPopupService = uiPopupService;
        this.xmlService = xmlService;
        ResourceWrapper = resourceWrapper;
        AppConstants = appConstants;

        settings = xmlService.LoadSettings();
        outputFolder = settings.SaveFolderPath;
        var savedData = xmlService.LoadData();
        UrlItems = new(savedData.UrlItems.Select(CreateUrlMediaItemViewModel));
        UrlItems.CollectionChanged += UrlItemsCollectionChanged;
        AddUrlCommand = new AsyncRelayCommand(AddUrlAsync, CanAddUrl);
        BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);
    }

    public bool IsUrlInputEnabled => !IsAnalyzing;

    public bool IsUrlPlaceholderVisible =>
        IsUrlInputEnabled && string.IsNullOrWhiteSpace(Url);

    public bool IsNoDataVisible => UrlItems.Count == 0;

    public ObservableCollection<UrlMediaItemViewModel> UrlItems { get; }

    public ResourceWrapper ResourceWrapper { get; }

    public AppConstants AppConstants { get; }

    public IAsyncRelayCommand AddUrlCommand { get; }

    public IRelayCommand BrowseOutputFolderCommand { get; }

    public IRelayCommand OpenOutputFolderCommand { get; }

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
        AddUrlCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsAnalyzingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsUrlInputEnabled));
        OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
        AddUrlCommand.NotifyCanExecuteChanged();
    }

    private string GetDefaultOutputFolder() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            AppConstants.Common.DownloadsFolderName);

    private void BrowseOutputFolder()
    {
        var selectedFolder = folderDialogService.SelectFolder(
            ResourceWrapper.SelectDownloadFolderTitle,
            Directory.Exists(OutputFolder)
                ? OutputFolder
                : GetDefaultOutputFolder());

        if (!string.IsNullOrWhiteSpace(selectedFolder))
        {
            OutputFolder = selectedFolder;
            settings.SaveFolderPath = selectedFolder;
            xmlService.SaveSettings(settings);
        }
    }

    private void OpenOutputFolder()
    {
        if (!string.IsNullOrWhiteSpace(OutputFolder))
        {
            shellService.OpenFolder(OutputFolder);
        }
    }

    private bool CanAddUrl() =>
        !IsAnalyzing && !string.IsNullOrWhiteSpace(Url);

    private void UrlItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(IsNoDataVisible));

    private async Task AddUrlAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsAnalyzing = true;
            AnalysisStatus = ResourceWrapper.AnalyzingUrlStatus;

            var mediaItem = await ytDlpService.GetMediaInfoAsync(Url, cancellationToken);
            UrlItems.Add(CreateUrlMediaItemViewModel(mediaItem));
            SaveUrlItems();

            Url = string.Empty;
            AnalysisStatus = string.Empty;
        }
        catch (OperationCanceledException)
        {
            AnalysisStatus = string.Empty;
        }
        catch (Exception exception)
        {
            AnalysisStatus = exception.Message;
            loggerWrapper.Write(string.Format(LogConstants.AnalyzeUrlFailed, exception.Message), LogLevel.Error);
            uiPopupService.ShowError(exception.Message);
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    private UrlMediaItemViewModel CreateUrlMediaItemViewModel(
                                                               UrlMediaItem mediaItem) =>
                                                               new(
                                                                    mediaItem,
                                                                    ResourceWrapper.ReadyStatus,
                                                                    ResourceWrapper.Mp4Label,
                                                                    ResourceWrapper.Mp3Label);

    private void SaveUrlItems() => xmlService.SaveData(
                                                        new MediaDownloaderState
                                                        {
                                                            UrlItems = UrlItems.Select(item => item.ToModel()).ToList()
                                                        });
}
