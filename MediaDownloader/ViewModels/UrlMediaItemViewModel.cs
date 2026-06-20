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

    [ObservableProperty]
    private int downloadProgress;

    [ObservableProperty]
    private string downloadStatus;

    [ObservableProperty]
    private bool isAudioSelected;

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
    public UrlMediaItemViewModel(
        UrlMediaItem model,
        string readyStatus,
        string mp4Label,
        string mp3Label)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(readyStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(mp4Label);
        ArgumentException.ThrowIfNullOrWhiteSpace(mp3Label);

        this.mp4Label = mp4Label;
        this.mp3Label = mp3Label;
        Url = model.Url;
        Title = model.Title;
        Author = model.Author;
        ThumbnailUrl = model.ThumbnailUrl;
        Duration = model.Duration;
        UploadDate = model.UploadDate;
        selectedMediaType = model.SelectedMediaType;
        selectedVideoQuality = model.SelectedVideoQuality;
        downloadProgress = model.DownloadProgress;
        downloadStatus = readyStatus;
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

    partial void OnIsAudioSelectedChanged(bool value) =>
        SelectedMediaType = value ? mp3Label : mp4Label;

    private void OnVideoQualitySelected(string quality) =>
        SelectedVideoQuality = quality;
}
