using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Downloads video or audio media through yt-dlp.
/// </summary>
public interface IMediaDownloadService
{
    /// <summary>
    /// Downloads media using the requested format and reports progress.
    /// </summary>
    /// <param name="request">Download parameters.</param>
    /// <param name="progress">Progress receiver.</param>
    /// <param name="cancellationToken">Token used to stop the download.</param>
    /// <returns>The completed download result.</returns>
    Task<MediaDownloadResult> DownloadAsync(MediaDownloadRequest request, IProgress<MediaDownloadProgress>? progress = null, CancellationToken cancellationToken = default);
}
