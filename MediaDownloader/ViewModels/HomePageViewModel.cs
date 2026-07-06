using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Coordinates URL analysis and the home page user interface.
/// </summary>
public partial class HomePageViewModel : ObservableObject
{
    private readonly IFolderDialogUtil folderDialogService;
    private readonly ILoggerWrapper loggerWrapper;
    private readonly IMediaDownloadService mediaDownloadService;
    private readonly IShellUtil shellService;
    private readonly IUiPopupUtil uiPopupService;
    private readonly IXmlService xmlService;
    private readonly IYtDlpService ytDlpService;
    private readonly ApplicationSettings settings;
    private readonly Dictionary<UrlMediaItemViewModel, CancellationTokenSource>
        downloadCancellationSources = [];

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
                             IXmlService xmlService,
                             IMediaDownloadService mediaDownloadService)
    {
        ArgumentNullException.ThrowIfNull(ytDlpService);
        ArgumentNullException.ThrowIfNull(loggerWrapper);
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(folderDialogService);
        ArgumentNullException.ThrowIfNull(shellService);
        ArgumentNullException.ThrowIfNull(uiPopupService);
        ArgumentNullException.ThrowIfNull(xmlService);
        ArgumentNullException.ThrowIfNull(mediaDownloadService);

        this.ytDlpService = ytDlpService;
        this.loggerWrapper = loggerWrapper;
        this.folderDialogService = folderDialogService;
        this.shellService = shellService;
        this.uiPopupService = uiPopupService;
        this.xmlService = xmlService;
        this.mediaDownloadService = mediaDownloadService;
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
        ToggleDownloadCommand = new RelayCommand<UrlMediaItemViewModel>(ToggleDownload);
        DeleteUrlItemCommand = new RelayCommand<UrlMediaItemViewModel>(DeleteUrlItem, CanDeleteUrlItem);
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

    public IRelayCommand<UrlMediaItemViewModel> ToggleDownloadCommand { get; }

    public IRelayCommand<UrlMediaItemViewModel> DeleteUrlItemCommand { get; }

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
            ResourceWrapper.Mp3Label,
            AppConstants.Images.DownloadPath,
            AppConstants.Images.StopDownloadPath,
            ResourceWrapper.DownloadToolTip,
            ResourceWrapper.StopDownloadToolTip);

    private void ToggleDownload(UrlMediaItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        if (item.IsDownloading)
        {
            StopDownload(item);
            return;
        }

        _ = DownloadMediaAsync(item);
    }

    private void StopDownload(UrlMediaItemViewModel item)
    {
        if (!uiPopupService.ConfirmStopDownload())
        {
            return;
        }

        item.DownloadStatus = ResourceWrapper.StoppingDownloadStatus;
        if (downloadCancellationSources.TryGetValue(item, out var cancellationSource))
        {
            cancellationSource.Cancel();
        }
    }

    private async Task DownloadMediaAsync(UrlMediaItemViewModel item)
    {
        using var cancellationSource = new CancellationTokenSource();
        if (!downloadCancellationSources.TryAdd(item, cancellationSource))
        {
            return;
        }

        try
        {
            item.IsDownloading = true;
            DeleteUrlItemCommand.NotifyCanExecuteChanged();
            item.DownloadProgress = 0;
            item.DownloadDate = string.Empty;
            UpdateDownloadStatus(item, 0);
            SaveUrlItems();

            var progress = new Progress<MediaDownloadProgress>(value =>
            {
                    item.DownloadProgress = value.Percentage;
                    item.DownloadStatus = value.IsPostProcessing
                                                                 ? ResourceWrapper.ProcessingDownloadStatus
                                                                 : string.Format(
                                                                                 CultureInfo.CurrentUICulture,
                                                                                 ResourceWrapper.DownloadingStatusFormat,
                                                                                 value.Percentage);
            });

            var result = await mediaDownloadService.DownloadAsync( new MediaDownloadRequest {
                                                                                              Url = item.Url,
                                                                                              OutputFolder = OutputFolder,
                                                                                              IsAudioOnly = item.IsAudioSelected,
                                                                                              VideoQuality = item.SelectedVideoQuality
                                                                                            },
                                                                                            progress,
                                                                                            cancellationSource.Token);

            item.DownloadProgress = 100;
            item.DownloadStatus = ResourceWrapper.DownloadCompletedStatus;
            item.DownloadDate = DateTime.Now.ToString(ServiceConstants.DateTimeFormats.DownloadDate, CultureInfo.CurrentCulture);
            item.DownloadFilePath = result.FilePath;
        }
        catch (OperationCanceledException)
        {
            item.DownloadProgress = 0;
            item.DownloadStatus = ResourceWrapper.DownloadCancelledStatus;
            item.DownloadDate = string.Empty;
            item.DownloadFilePath = string.Empty;
        }
        catch (Exception exception)
        {
            item.DownloadProgress = 0;
            item.DownloadStatus = ResourceWrapper.DownloadFailedStatus;
            item.DownloadDate = string.Empty;
            item.DownloadFilePath = string.Empty;
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.MediaDownloadFailed,
                                               item.Url,
                                               exception.Message),
                                 LogLevel.Error);
            uiPopupService.ShowError(exception.Message);
        }
        finally
        {
            item.IsDownloading = false;
            DeleteUrlItemCommand.NotifyCanExecuteChanged();
            downloadCancellationSources.Remove(item);
            SaveUrlItems();
        }
    }

    private void UpdateDownloadStatus(
        UrlMediaItemViewModel item,
        int percentage) => item.DownloadStatus = string.Format(
                                                                CultureInfo.CurrentUICulture,
                                                                ResourceWrapper.DownloadingStatusFormat,
                                                                percentage);

    private void DeleteUrlItem(UrlMediaItemViewModel? item)
    {
        if (item is null
            || item.IsDownloading)
        {
            return;
        }

        var confirmation = uiPopupService.ConfirmMediaItemDelete(HasDownloadedFilePath(item));
        if (!confirmation.IsConfirmed)
        {
            return;
        }

        var wasDeletedFromFileSystem = false;
        if (confirmation.DeleteDownloadedFile
            && !TryDeleteDownloadedFile(item, out wasDeletedFromFileSystem))
        {
            return;
        }

        UrlItems.Remove(item);
        SaveUrlItems();
        LogMediaItemDeleted(item, wasDeletedFromFileSystem);
    }

    private static bool CanDeleteUrlItem(UrlMediaItemViewModel? item) =>
        item is { IsDownloading: false };

    private bool TryDeleteDownloadedFile(
                                         UrlMediaItemViewModel item,
                                         out bool wasDeletedFromFileSystem)
    {
        wasDeletedFromFileSystem = false;

        if (string.IsNullOrWhiteSpace(item.DownloadFilePath))
        {
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.MediaItemDeletedFromListFilePathMissing,
                                               item.Url,
                                               item.Title,
                                               item.SelectedMediaType,
                                               item.DownloadDate),
                                LogLevel.Warning);
            return true;
        }

        if (!File.Exists(item.DownloadFilePath))
        {
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.MediaItemDeletedFromListFileNotFound,
                                               item.Url,
                                               item.Title,
                                               item.SelectedMediaType,
                                               item.DownloadDate,
                                               item.DownloadFilePath),
                                LogLevel.Warning);
            return true;
        }

        try
        {
            File.Delete(item.DownloadFilePath);
            wasDeletedFromFileSystem = true;
            return true;
        }
        catch (Exception exception) when (exception is IOException
                                             or UnauthorizedAccessException
                                             or System.Security.SecurityException)
        {
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.MediaItemDeleteFileFailed,
                                               item.Url,
                                               item.Title,
                                               item.SelectedMediaType,
                                               item.DownloadDate,
                                               item.DownloadFilePath,
                                               exception.Message),
                                LogLevel.Error);
            uiPopupService.ShowError(string.Format(
                                                   CultureInfo.CurrentUICulture,
                                                   ResourceWrapper.DeleteDownloadedFileFailedMessage,
                                                   exception.Message));
            return false;
        }
    }

    private static bool HasDownloadedFilePath(UrlMediaItemViewModel item) =>
        !string.IsNullOrWhiteSpace(item.DownloadFilePath);

    private void LogMediaItemDeleted(
                                     UrlMediaItemViewModel item,
                                     bool wasDeletedFromFileSystem)
    {
        var message = wasDeletedFromFileSystem
            ? string.Format(
                            CultureInfo.InvariantCulture,
                            LogConstants.MediaItemDeletedFromListAndFileSystem,
                            item.Url,
                            item.Title,
                            item.SelectedMediaType,
                            item.DownloadDate,
                            item.DownloadFilePath)
            : string.Format(
                            CultureInfo.InvariantCulture,
                            LogConstants.MediaItemDeletedFromList,
                            item.Url,
                            item.Title,
                            item.SelectedMediaType,
                            item.DownloadDate);

        loggerWrapper.Write(message, LogLevel.Info);
    }

    private void SaveUrlItems() => xmlService.SaveData(
                                                        new MediaDownloaderState
                                                        {
                                                            UrlItems = UrlItems.Select(item => item.ToModel()).ToList()
                                                        });
}
