using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeXmlService : IXmlService
{
    public ApplicationSettings Settings { get; set; } = new();

    public MediaDownloaderState State { get; set; } = new();

    public List<ApplicationSettings> SavedSettings { get; } = [];

    public List<MediaDownloaderState> SavedStates { get; } = [];

    public ApplicationSettings LoadSettings() => Settings;

    public void SaveSettings(ApplicationSettings settings) => SavedSettings.Add(Clone(settings));

    public MediaDownloaderState LoadData() => State;

    public void SaveData(MediaDownloaderState state) => SavedStates.Add(Clone(state));

    private static ApplicationSettings Clone(ApplicationSettings settings) =>
        new()
        {
            SaveFolderPath = settings.SaveFolderPath
        };

    private static MediaDownloaderState Clone(MediaDownloaderState state) =>
        new()
        {
            UrlItems = state.UrlItems.Select(Clone).ToList()
        };

    private static UrlMediaItem Clone(UrlMediaItem item) =>
        new()
        {
            Url = item.Url,
            Title = item.Title,
            Author = item.Author,
            ThumbnailUrl = item.ThumbnailUrl,
            Duration = item.Duration,
            UploadDate = item.UploadDate,
            SelectedMediaType = item.SelectedMediaType,
            SelectedVideoQuality = item.SelectedVideoQuality,
            AvailableVideoQualities = item.AvailableVideoQualities
                .Select(option => new VideoQualityOption { Name = option.Name, IsSelected = option.IsSelected })
                .ToList(),
            DownloadProgress = item.DownloadProgress,
            DownloadStatus = item.DownloadStatus,
            DownloadDate = item.DownloadDate,
            DownloadFilePath = item.DownloadFilePath
        };
}
