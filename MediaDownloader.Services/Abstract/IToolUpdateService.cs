using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Checks whether newer versions of external tools are available.
/// </summary>
public interface IToolUpdateService
{
    /// <summary>
    /// Checks all supported tools for newer versions.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the check.</param>
    /// <returns>Available tool updates.</returns>
    Task<IReadOnlyList<ToolUpdateInfo>> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
}
