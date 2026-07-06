using MediaDownloader.Services.Constants;

namespace MediaDownloader.Services.Models;

/// <summary>
/// Contains media metadata returned by the analysis service.
/// </summary>
public sealed class UrlMediaItem
{
    public string Url { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ThumbnailUrl { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public string UploadDate { get; set; } = string.Empty;

    public string SelectedMediaType { get; set; } = string.Empty;

    public string SelectedVideoQuality { get; set; } = string.Empty;

    public List<VideoQualityOption> AvailableVideoQualities { get; set; } = [];

    public int DownloadProgress { get; set; }

    public string DownloadStatus { get; set; } = ServiceConstants.Media.DownloadStatusReady;

    public string DownloadDate { get; set; } = string.Empty;

    public string DownloadFilePath { get; set; } = string.Empty;
}
