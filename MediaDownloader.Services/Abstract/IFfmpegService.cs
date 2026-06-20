namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Provides information about bundled FFmpeg tools.
/// </summary>
public interface IFfmpegService
{
    /// <summary>
    /// Gets the FFmpeg executable path.
    /// </summary>
    string FfmpegPath { get; }

    /// <summary>
    /// Gets the FFprobe executable path.
    /// </summary>
    string FfprobePath { get; }

    /// <summary>
    /// Determines whether both tools are available.
    /// </summary>
    /// <returns><see langword="true"/> when both tools exist.</returns>
    bool AreToolsAvailable();

    /// <summary>
    /// Reads the FFmpeg version.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The first version output line.</returns>
    Task<string> GetFfmpegVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the FFprobe version.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The first version output line.</returns>
    Task<string> GetFfprobeVersionAsync(CancellationToken cancellationToken = default);
}
