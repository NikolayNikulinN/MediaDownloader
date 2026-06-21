namespace MediaDownloader.Services.Models;

/// <summary>
/// Describes a requested media download.
/// </summary>
public sealed class MediaDownloadRequest
{
    public required string Url { get; init; }

    public required string OutputFolder { get; init; }

    public required bool IsAudioOnly { get; init; }

    public string VideoQuality { get; init; } = string.Empty;
}
