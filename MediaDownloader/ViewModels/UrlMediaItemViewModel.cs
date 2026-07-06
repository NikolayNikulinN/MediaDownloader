using CommunityToolkit.Mvvm.ComponentModel;
using MediaDownloader.Services.Models;
using System.Collections.ObjectModel;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Represents analyzed media and its mutable download state in the user interface.
/// </summary>
public partial class UrlMediaItemViewModel : ObservableObject
{
    private readonly string mp3Label;
    private readonly string mp4Label;
    private readonly string downloadIconPath;
    private readonly string downloadToolTip;
    private readonly string stopDownloadIconPath;
    private readonly string stopDownloadToolTip;

    [ObservableProperty]
    private string downloadButtonIconPath;

    [ObservableProperty]
    private string downloadButtonToolTip;

    [ObservableProperty]
    private string downloadDate;

    [ObservableProperty]
    private string downloadFilePath;

    [ObservableProperty]
    private int downloadProgress;

    [ObservableProperty]
    private string downloadStatus;

    [ObservableProperty]
    private bool isAudioSelected;

    [ObservableProperty]
    private bool isDownloading;

    [ObservableProperty]
    private string selectedMediaType;

    [ObservableProperty]
    private string selectedVideoQuality;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlMediaItemViewModel"/> class.
    /// </summary>
    /// <param name="model">Source service model.</param>
    /// <param name="readyStatus">Localized initial download status.</param>
    /// <param name="mp4Label">Localized MP4 label.</param>
    /// <param name="mp3Label">Localized MP3 label.</param>
    /// <param name="downloadIconPath">Download button icon path.</param>
    /// <param name="stopDownloadIconPath">Stop button icon path.</param>
    /// <param name="downloadToolTip">Download button tooltip.</param>
    /// <param name="stopDownloadToolTip">Stop button tooltip.</param>
    public UrlMediaItemViewModel(
        UrlMediaItem model,
        string readyStatus,
        string mp4Label,
        string mp3Label,
        string downloadIconPath,
        string stopDownloadIconPath,
        string downloadToolTip,
        string stopDownloadToolTip)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(readyStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(mp4Label);
        ArgumentException.ThrowIfNullOrWhiteSpace(mp3Label);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadIconPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(stopDownloadIconPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadToolTip);
        ArgumentException.ThrowIfNullOrWhiteSpace(stopDownloadToolTip);

        this.mp4Label = mp4Label;
        this.mp3Label = mp3Label;
        this.downloadIconPath = downloadIconPath;
        this.stopDownloadIconPath = stopDownloadIconPath;
        this.downloadToolTip = downloadToolTip;
        this.stopDownloadToolTip = stopDownloadToolTip;
        downloadButtonIconPath = downloadIconPath;
        downloadButtonToolTip = downloadToolTip;
        downloadDate = model.DownloadDate;
        downloadFilePath = model.DownloadFilePath;
        Url = model.Url;
        Title = model.Title;
        Author = model.Author;
        ThumbnailUrl = model.ThumbnailUrl;
        Duration = model.Duration;
        UploadDate = model.UploadDate;
        selectedMediaType = model.SelectedMediaType;
        selectedVideoQuality = model.SelectedVideoQuality;
        isAudioSelected = string.Equals(
                                        model.SelectedMediaType,
                                        mp3Label,
                                        StringComparison.OrdinalIgnoreCase);
        downloadProgress = model.DownloadProgress;
        downloadStatus = string.IsNullOrWhiteSpace(model.DownloadStatus)
                                                                        ? readyStatus
                                                                        : model.DownloadStatus;
        AvailableVideoQualities = new(
            model.AvailableVideoQualities.Select(
                option => new VideoQualityOptionViewModel(option, OnVideoQualitySelected)));
    }

    public string Url { get; }

    public string Title { get; }

    public string Author { get; }

    public string ThumbnailUrl { get; }

    public string Duration { get; }

    public string UploadDate { get; }

    public ObservableCollection<VideoQualityOptionViewModel> AvailableVideoQualities { get; }

    public bool AreVideoQualitiesVisible => !IsAudioSelected;

    public bool CanChangeDownloadOptions => !IsDownloading;

    /// <summary>
    /// Converts this view model to its persistence model.
    /// </summary>
    /// <returns>Persistable media data.</returns>
    public UrlMediaItem ToModel() =>
        new()
        {
            Url = Url,
            Title = Title,
            Author = Author,
            ThumbnailUrl = ThumbnailUrl,
            Duration = Duration,
            UploadDate = UploadDate,
            SelectedMediaType = SelectedMediaType,
            SelectedVideoQuality = SelectedVideoQuality,
            AvailableVideoQualities =
                AvailableVideoQualities.Select(option => option.ToModel()).ToList(),
            DownloadProgress = DownloadProgress,
            DownloadStatus = DownloadStatus,
            DownloadDate = DownloadDate,
            DownloadFilePath = DownloadFilePath
        };

    partial void OnIsAudioSelectedChanged(bool value)
    {
        SelectedMediaType = value ? mp3Label : mp4Label;
        OnPropertyChanged(nameof(AreVideoQualitiesVisible));
    }

    partial void OnIsDownloadingChanged(bool value)
    {
        DownloadButtonIconPath = value
            ? stopDownloadIconPath
            : downloadIconPath;
        DownloadButtonToolTip = value
            ? stopDownloadToolTip
            : downloadToolTip;
        OnPropertyChanged(nameof(CanChangeDownloadOptions));
    }

    private void OnVideoQualitySelected(string quality) =>
        SelectedVideoQuality = quality;
}
