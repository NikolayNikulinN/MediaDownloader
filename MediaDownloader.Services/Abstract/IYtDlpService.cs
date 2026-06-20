using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Retrieves media metadata through yt-dlp.
/// </summary>
public interface IYtDlpService
{
    /// <summary>
    /// Retrieves metadata for the specified media URL.
    /// </summary>
    /// <param name="url">Media URL to analyze.</param>
    /// <param name="cancellationToken">Token used to cancel analysis.</param>
    /// <returns>Analyzed media metadata.</returns>
    Task<UrlMediaItem> GetMediaInfoAsync(string url, CancellationToken cancellationToken = default);
}
