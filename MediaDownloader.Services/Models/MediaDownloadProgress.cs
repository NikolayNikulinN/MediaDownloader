namespace MediaDownloader.Services.Models;

/// <summary>
/// Reports progress and processing stage for a media download.
/// </summary>
public sealed class MediaDownloadProgress
{
    public int Percentage { get; init; }

    public bool IsPostProcessing { get; init; }
}
