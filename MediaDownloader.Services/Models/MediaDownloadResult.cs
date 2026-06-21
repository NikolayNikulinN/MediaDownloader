namespace MediaDownloader.Services.Models;

/// <summary>
/// Contains the result of a completed media download.
/// </summary>
public sealed class MediaDownloadResult
{
    public required string FilePath { get; init; }
}
