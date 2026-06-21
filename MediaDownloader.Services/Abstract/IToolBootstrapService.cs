namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Verifies and installs external tools required by the application.
/// </summary>
public interface IToolBootstrapService
{
    /// <summary>
    /// Gets the directory containing external tools.
    /// </summary>
    string ToolsDirectoryPath { get; }

    /// <summary>
    /// Gets the file names of all required tools that are not available.
    /// </summary>
    /// <returns>Missing tool file names.</returns>
    IReadOnlyList<string> GetMissingTools();

    /// <summary>
    /// Downloads the specified missing tools after explicit user confirmation.
    /// </summary>
    /// <param name="missingTools">Tool file names to download.</param>
    /// <param name="userConfirmed">Whether the user explicitly approved the download.</param>
    /// <param name="progress">Receives the file name currently being downloaded.</param>
    /// <param name="cancellationToken">Token used to cancel downloads.</param>
    Task DownloadMissingToolsAsync(
                                    IReadOnlyCollection<string> missingTools,
                                    bool userConfirmed,
                                    IProgress<string>? progress = null,
                                    CancellationToken cancellationToken = default);
}
