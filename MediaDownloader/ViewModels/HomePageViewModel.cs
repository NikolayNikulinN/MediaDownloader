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
using System.ComponentModel;
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
    private bool isBulkAudioSelected;

    [ObservableProperty]
    private bool isDownloadingAll;

    [ObservableProperty]
    private bool isDeletingAll;

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
        foreach (var item in UrlItems)
        {
            SubscribeUrlItem(item);
        }

        BulkAvailableVideoQualities = [];
        UpdateBulkActionsState();
        UrlItems.CollectionChanged += UrlItemsCollectionChanged;
        AddUrlCommand = new AsyncRelayCommand(AddUrlAsync, CanAddUrl);
        BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
        OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);
        ToggleDownloadCommand = new RelayCommand<UrlMediaItemViewModel>(ToggleDownload);
        DeleteUrlItemCommand = new RelayCommand<UrlMediaItemViewModel>(DeleteUrlItem, CanDeleteUrlItem);
        DownloadAllUrlItemsCommand = new AsyncRelayCommand(DownloadAllUrlItemsAsync, CanDownloadAllUrlItems);
        DeleteAllUrlItemsCommand = new AsyncRelayCommand(DeleteAllUrlItemsAsync, CanDeleteAllUrlItems);
    }

    public bool IsUrlPlaceholderVisible =>
        string.IsNullOrWhiteSpace(Url);

    public bool IsNoDataVisible => UrlItems.Count == 0;

    public ObservableCollection<UrlMediaItemViewModel> UrlItems { get; }

    public ObservableCollection<VideoQualityOptionViewModel> BulkAvailableVideoQualities { get; }

    public ResourceWrapper ResourceWrapper { get; }

    public AppConstants AppConstants { get; }

    public IAsyncRelayCommand AddUrlCommand { get; }

    public IRelayCommand BrowseOutputFolderCommand { get; }

    public IRelayCommand OpenOutputFolderCommand { get; }

    public IRelayCommand<UrlMediaItemViewModel> ToggleDownloadCommand { get; }

    public IRelayCommand<UrlMediaItemViewModel> DeleteUrlItemCommand { get; }

    public IAsyncRelayCommand DownloadAllUrlItemsCommand { get; }

    public IAsyncRelayCommand DeleteAllUrlItemsCommand { get; }

    public bool IsBulkActionsVisible => UrlItems.Count > 1;

    public bool CanUseBulkActions => UrlItems.Count > 1 && !IsDeletingAll && UrlItems.All(item => !item.IsDownloading);

    public bool CanUsePageActions => !IsDeletingAll;

    public bool HasActiveOperation => IsDeletingAll || UrlItems.Any(item => item.IsDownloading);

    public bool AreBulkVideoQualitiesVisible => !IsBulkAudioSelected;

    partial void OnUrlChanged(string value)
    {
        OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
        AddUrlCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsAnalyzingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
        AddUrlCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDeletingAllChanged(bool value)
    {
        OnPropertyChanged(nameof(CanUseBulkActions));
        OnPropertyChanged(nameof(CanUsePageActions));
        OnPropertyChanged(nameof(HasActiveOperation));
        OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
        AddUrlCommand.NotifyCanExecuteChanged();
        DeleteUrlItemCommand.NotifyCanExecuteChanged();
        DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();
        DeleteAllUrlItemsCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsDownloadingAllChanged(bool value) => DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();

    partial void OnIsBulkAudioSelectedChanged(bool value) =>
        ApplyBulkAudioSelection();

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
        !IsAnalyzing && !IsDeletingAll && !string.IsNullOrWhiteSpace(Url);

    private void UrlItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (UrlMediaItemViewModel item in e.OldItems)
            {
                UnsubscribeUrlItem(item);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (UrlMediaItemViewModel item in e.NewItems)
            {
                SubscribeUrlItem(item);
            }
        }

        OnPropertyChanged(nameof(IsNoDataVisible));
        OnPropertyChanged(nameof(HasActiveOperation));
        UpdateBulkActionsState();
        DeleteAllUrlItemsCommand.NotifyCanExecuteChanged();
        DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();
    }

    private async Task AddUrlAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsAnalyzing = true;
            AnalysisStatus = ResourceWrapper.AnalyzingUrlStatus;

            var mediaItem = await ytDlpService.GetMediaInfoAsync(Url, cancellationToken);
            if (UrlItems.Any(item => string.Equals(
                                                    item.Url,
                                                    mediaItem.Url,
                                                    StringComparison.OrdinalIgnoreCase)))
            {
                Url = string.Empty;
                AnalysisStatus = string.Empty;
                return;
            }

            UrlItems.Insert(0, CreateUrlMediaItemViewModel(mediaItem));
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
            loggerWrapper.Write(string.Format(CultureInfo.InvariantCulture, LogConstants.AnalyzeUrlFailed, exception.Message), LogLevel.Error);
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
            NormalizeDownloadOptions(item);
            item.IsDownloading = true;
            DeleteUrlItemCommand.NotifyCanExecuteChanged();
            DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();
            DeleteAllUrlItemsCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanUseBulkActions));
            OnPropertyChanged(nameof(HasActiveOperation));
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
                                                                                 CultureInfo.CurrentCulture,
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
            DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();
            DeleteAllUrlItemsCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CanUseBulkActions));
            OnPropertyChanged(nameof(HasActiveOperation));
            downloadCancellationSources.Remove(item);
            SaveUrlItems();
        }
    }

    private void UpdateDownloadStatus(
        UrlMediaItemViewModel item,
        int percentage) => item.DownloadStatus = string.Format(
                                                                CultureInfo.CurrentCulture,
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

    private async Task DownloadAllUrlItemsAsync()
    {
        if (!CanDownloadAllUrlItems()
            || !uiPopupService.ConfirmAllMediaItemsDownload())
        {
            return;
        }

        var items = UrlItems.ToList();
        loggerWrapper.Write(string.Format(
                                           CultureInfo.InvariantCulture,
                                           LogConstants.MediaItemsDownloadBulkStarted,
                                           items.Count),
                            LogLevel.Info);

        IsDownloadingAll = true;
        await Task.Yield();

        try
        {
            foreach (var item in items)
            {
                _ = DownloadMediaAsync(item);
            }
        }
        finally
        {
            IsDownloadingAll = false;
        }
    }

    private bool CanDownloadAllUrlItems() =>
        UrlItems.Count > 1 && !IsDeletingAll && !IsDownloadingAll && UrlItems.All(item => !item.IsDownloading);

    private void ApplyBulkDownloadOptions(UrlMediaItemViewModel item)
    {
        item.IsAudioSelected = IsBulkAudioSelected;
        if (IsBulkAudioSelected)
        {
            return;
        }

        var selectedQuality = BulkAvailableVideoQualities.FirstOrDefault(option => option.IsSelected)?.Name;
        if (string.IsNullOrWhiteSpace(selectedQuality))
        {
            return;
        }

        foreach (var quality in item.AvailableVideoQualities)
        {
            quality.IsSelected = string.Equals(
                                               quality.Name,
                                               selectedQuality,
                                               StringComparison.OrdinalIgnoreCase);
        }

        item.SelectedVideoQuality = selectedQuality;
    }

    private static void NormalizeDownloadOptions(UrlMediaItemViewModel item)
    {
        if (item.IsAudioSelected)
        {
            return;
        }

        var selectedQuality = item.AvailableVideoQualities.FirstOrDefault(option => option.IsSelected)?.Name;
        if (string.IsNullOrWhiteSpace(selectedQuality))
        {
            selectedQuality = item.SelectedVideoQuality;
        }

        if (string.IsNullOrWhiteSpace(selectedQuality))
        {
            selectedQuality = item.AvailableVideoQualities.FirstOrDefault()?.Name ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(selectedQuality))
        {
            return;
        }

        foreach (var quality in item.AvailableVideoQualities)
        {
            quality.IsSelected = string.Equals(
                                               quality.Name,
                                               selectedQuality,
                                               StringComparison.OrdinalIgnoreCase);
        }

        item.SelectedVideoQuality = selectedQuality;
    }

    private async Task DeleteAllUrlItemsAsync()
    {
        if (!CanDeleteAllUrlItems())
        {
            return;
        }

        var items = UrlItems.ToList();
        var confirmation = uiPopupService.ConfirmAllMediaItemsDelete(items.Any(HasDownloadedFilePath));
        if (!confirmation.IsConfirmed)
        {
            return;
        }

        IsDeletingAll = true;
        await Task.Yield();

        loggerWrapper.Write(string.Format(
                                           CultureInfo.InvariantCulture,
                                           LogConstants.MediaItemsDeleteBulkStarted,
                                           items.Count,
                                           confirmation.DeleteDownloadedFile),
                            LogLevel.Info);

        var removedFromListCount = 0;
        var deletedFromFileSystemCount = 0;
        var failedFileDeletionCount = 0;
        var failedItems = new List<string>();

        try
        {
            foreach (var item in items)
            {
                var deleteFileResult = (CanRemoveItem: true, WasDeletedFromFileSystem: false);
                if (confirmation.DeleteDownloadedFile
                    && !(deleteFileResult = await TryDeleteDownloadedFileAsync(item, showErrorPopup: false)).CanRemoveItem)
                {
                    failedFileDeletionCount++;
                    failedItems.Add(CreateDeletedItemDescription(item));
                    continue;
                }

                UrlItems.Remove(item);
                removedFromListCount++;
                if (deleteFileResult.WasDeletedFromFileSystem)
                {
                    deletedFromFileSystemCount++;
                }

                LogMediaItemDeleted(item, deleteFileResult.WasDeletedFromFileSystem);
            }

            SaveUrlItems();
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.MediaItemsDeletedBulk,
                                               removedFromListCount,
                                               deletedFromFileSystemCount,
                                               failedFileDeletionCount,
                                               confirmation.DeleteDownloadedFile),
                                LogLevel.Info);

            if (failedFileDeletionCount > 0)
            {
                uiPopupService.ShowWarning(string.Format(
                                                         CultureInfo.CurrentCulture,
                                                         ResourceWrapper.DeleteAllIncompleteWarningMessage,
                                                         string.Join(", ", failedItems)));
            }
        }
        finally
        {
            IsDeletingAll = false;
        }
    }

    private bool CanDeleteAllUrlItems() =>
        UrlItems.Count > 1 && !IsDeletingAll && UrlItems.All(item => !item.IsDownloading);

    private Task<(bool CanRemoveItem, bool WasDeletedFromFileSystem)> TryDeleteDownloadedFileAsync(
                                                                                                   UrlMediaItemViewModel item,
                                                                                                   bool showErrorPopup = true) => Task.Run(
                                                                                                       () =>
                                                                                                       {
                                                                                                           var canRemoveItem = TryDeleteDownloadedFile(
                                                                                                                                                       item,
                                                                                                                                                       out var wasDeletedFromFileSystem,
                                                                                                                                                       showErrorPopup);
                                                                                                           return (canRemoveItem, wasDeletedFromFileSystem);
                                                                                                       });

    private bool TryDeleteDownloadedFile(
                                         UrlMediaItemViewModel item,
                                         out bool wasDeletedFromFileSystem,
                                         bool showErrorPopup = true)
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
            if (showErrorPopup)
            {
                uiPopupService.ShowError(string.Format(
                                                       CultureInfo.CurrentCulture,
                                                       ResourceWrapper.DeleteDownloadedFileFailedMessage,
                                                       exception.Message));
            }

            return false;
        }
    }

    private static bool HasDownloadedFilePath(UrlMediaItemViewModel item) =>
        !string.IsNullOrWhiteSpace(item.DownloadFilePath);

    private static string CreateDeletedItemDescription(UrlMediaItemViewModel item) =>
        string.IsNullOrWhiteSpace(item.Title) ? item.Url : item.Title;

    private void UpdateBulkActionsState()
    {
        OnPropertyChanged(nameof(IsBulkActionsVisible));
        OnPropertyChanged(nameof(CanUseBulkActions));
        UpdateBulkAvailableVideoQualities();
    }

    private void SubscribeUrlItem(UrlMediaItemViewModel item) =>
        item.PropertyChanged += UrlItemPropertyChanged;

    private void UnsubscribeUrlItem(UrlMediaItemViewModel item) =>
        item.PropertyChanged -= UrlItemPropertyChanged;

    private void UrlItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(UrlMediaItemViewModel.IsDownloading))
        {
            return;
        }

        OnPropertyChanged(nameof(HasActiveOperation));
        OnPropertyChanged(nameof(CanUseBulkActions));
        DownloadAllUrlItemsCommand.NotifyCanExecuteChanged();
        DeleteAllUrlItemsCommand.NotifyCanExecuteChanged();
    }

    private void UpdateBulkAvailableVideoQualities()
    {
        BulkAvailableVideoQualities.Clear();
        if (UrlItems.Count < 2)
        {
            return;
        }

        var commonQualityNames = UrlItems.Select(item => item.AvailableVideoQualities.Select(option => option.Name).ToHashSet(StringComparer.OrdinalIgnoreCase))
                                         .Aggregate((left, right) =>
                                         {
                                             left.IntersectWith(right);
                                             return left;
                                         })
                                         .OrderByDescending(ParseQualityHeight)
                                         .ToArray();

        foreach (var qualityName in commonQualityNames)
        {
            BulkAvailableVideoQualities.Add(
                new VideoQualityOptionViewModel(
                                                new VideoQualityOption
                                                {
                                                    Name = qualityName,
                                                    IsSelected = BulkAvailableVideoQualities.Count == 0
                                                },
                                                ApplyBulkQualitySelection));
        }
    }

    private void ApplyBulkAudioSelection()
    {
        OnPropertyChanged(nameof(AreBulkVideoQualitiesVisible));
        foreach (var item in UrlItems)
        {
            if (!item.IsDownloading)
            {
                item.IsAudioSelected = IsBulkAudioSelected;
            }
        }

        SaveUrlItems();
    }

    private void ApplyBulkQualitySelection(string qualityName)
    {
        if (IsBulkAudioSelected
            || string.IsNullOrWhiteSpace(qualityName))
        {
            return;
        }

        foreach (var item in UrlItems.Where(item => !item.IsDownloading))
        {
            foreach (var quality in item.AvailableVideoQualities)
            {
                quality.IsSelected = string.Equals(
                                                   quality.Name,
                                                   qualityName,
                                                   StringComparison.OrdinalIgnoreCase);
            }

            item.SelectedVideoQuality = qualityName;
        }

        SaveUrlItems();
    }

    private static int ParseQualityHeight(string qualityName)
    {
        var normalizedQuality = qualityName.TrimEnd('p', 'P');
        return int.TryParse(
                            normalizedQuality,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out var height)
                            ? height
                            : 0;
    }

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
