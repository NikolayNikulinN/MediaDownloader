using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Utils;
using System.IO.Compression;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Verifies and downloads external media tools.
/// </summary>
public sealed class ToolBootstrapService : IToolBootstrapService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBootstrapService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used for tool downloads.</param>
    public ToolBootstrapService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public string ToolsDirectoryPath => ToolPathResolver.ToolsDirectoryPath;

    /// <inheritdoc />
    public IReadOnlyList<string> GetMissingTools() => ServiceConstants.Tools.RequiredFileNames
                                                                                               .Where(fileName => !File.Exists(ToolPathResolver.Resolve(fileName)))
                                                                                               .ToArray();

    /// <inheritdoc />
    public async Task DownloadMissingToolsAsync(
                                                IReadOnlyCollection<string> missingTools,
                                                bool userConfirmed,
                                                IProgress<string>? progress = null,
                                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(missingTools);
        if (!userConfirmed)
        {
            throw new InvalidOperationException(ServiceConstants.Tools.DownloadRequiresConfirmation);
        }

        if (missingTools.Count == 0)
        {
            return;
        }

        Directory.CreateDirectory(ToolPathResolver.ToolsDirectoryPath);
        if (missingTools.Contains(ServiceConstants.Tools.YtDlpFileName, StringComparer.OrdinalIgnoreCase))
        {
            progress?.Report(ServiceConstants.Tools.YtDlpFileName);
            await DownloadFileAsync(
                                    ServiceConstants.Tools.YtDlpDownloadUrl,
                                    ServiceConstants.Tools.YtDlpFileName,
                                    cancellationToken);
        }

        if (missingTools.Any(IsFfmpegTool))
        {
            await DownloadFfmpegToolsAsync(
                                            missingTools,
                                            progress,
                                            cancellationToken);
        }
    }

    private static bool IsFfmpegTool(string fileName) => string.Equals(
                                                                       fileName,
                                                                       ServiceConstants.Tools.FfmpegFileName,
                                                                       StringComparison.OrdinalIgnoreCase)

                                                         || string.Equals(
                                                                          fileName,
                                                                          ServiceConstants.Tools.FfprobeFileName,
                                                                          StringComparison.OrdinalIgnoreCase);

    private async Task DownloadFileAsync(
                                         string sourceUrl,
                                         string destinationFileName,
                                         CancellationToken cancellationToken)
    {
        var destinationPath = Path.Combine(ToolPathResolver.ToolsDirectoryPath, destinationFileName);
        var temporaryPath = destinationPath + ServiceConstants.Tools.TemporaryFileExtension;

        try
        {
            await using (var sourceStream = await httpClient.GetStreamAsync(sourceUrl, cancellationToken))
            await using (var destinationStream = File.Create(temporaryPath))
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            }

            File.Move(temporaryPath, destinationPath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private async Task DownloadFfmpegToolsAsync(
                                                IReadOnlyCollection<string> missingTools,
                                                IProgress<string>? progress,
                                                CancellationToken cancellationToken)
    {
        progress?.Report(missingTools.First(IsFfmpegTool));
        var archivePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{ServiceConstants.Tools.ZipFileExtension}");

        try
        {
            await using (var sourceStream = await httpClient.GetStreamAsync(ServiceConstants.Tools.FfmpegDownloadUrl,cancellationToken))
            await using (var archiveStream = File.Create(archivePath))
            {
                await sourceStream.CopyToAsync(archiveStream, cancellationToken);
            }

            using var archive = ZipFile.OpenRead(archivePath);
            foreach (var fileName in missingTools.Where(IsFfmpegTool))
            {
                progress?.Report(fileName);
                var entry = archive.Entries.FirstOrDefault(candidate => candidate.FullName.EndsWith(
                                                                                                    $"/bin/{fileName}",
                                                                                                    StringComparison.OrdinalIgnoreCase))
                                                                                                    ?? 
                                                                                                    throw new InvalidDataException(string.Format(
                                                                                                                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                                                                                                                 ServiceConstants.Tools.ArchiveEntryNotFound,
                                                                                                                                                 fileName));

                var destinationPath = Path.Combine(ToolPathResolver.ToolsDirectoryPath, fileName);
                var temporaryPath = destinationPath + ServiceConstants.Tools.TemporaryFileExtension;

                try
                {
                    entry.ExtractToFile(temporaryPath, overwrite: true);
                    File.Move(temporaryPath, destinationPath, overwrite: true);
                }
                finally
                {
                    File.Delete(temporaryPath);
                }
            }
        }
        finally
        {
            File.Delete(archivePath);
        }
    }
}
