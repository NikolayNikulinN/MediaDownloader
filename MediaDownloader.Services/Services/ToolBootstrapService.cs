using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Utils;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Globalization;
using System.IO.Compression;
using System.Net;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Verifies and downloads external media tools.
/// </summary>
public sealed class ToolBootstrapService : IToolBootstrapService
{
    private readonly HttpClient httpClient;
    private readonly ILoggerWrapper loggerWrapper;
    private readonly ResiliencePipeline downloadPipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolBootstrapService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used for tool downloads.</param>
    /// <param name="loggerWrapper">Application logger.</param>
    public ToolBootstrapService(
                                HttpClient httpClient,
                                ILoggerWrapper loggerWrapper)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(loggerWrapper);

        this.httpClient = httpClient;
        this.loggerWrapper = loggerWrapper;
        downloadPipeline = CreateDownloadPipeline();
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
            throw new InvalidOperationException(
                ServiceConstants.Tools.DownloadRequiresConfirmation);
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

    private ResiliencePipeline CreateDownloadPipeline() => new ResiliencePipelineBuilder()
                                                                                          .AddRetry(
                                                                                              new RetryStrategyOptions
                                                                                              {
                                                                                                  ShouldHandle = new PredicateBuilder()
                                                                                                                                       .Handle<HttpRequestException>(IsTransientHttpException)
                                                                                                                                       .Handle<IOException>()
                                                                                                                                       .Handle<InvalidDataException>()
                                                                                                                                       .Handle<TimeoutRejectedException>(),
                                                                                                  MaxRetryAttempts = ServiceConstants.Tools.DownloadMaxRetryAttempts,
                                                                                                  Delay = TimeSpan.FromSeconds(ServiceConstants.Tools.DownloadRetryDelaySeconds),
                                                                                                  BackoffType = DelayBackoffType.Exponential,
                                                                                                  UseJitter = true,
                                                                                                  OnRetry = arguments =>
                                                                                                  {
                                                                                                      loggerWrapper.Write(
                                                                                                                          string.Format(
                                                                                                                                         CultureInfo.InvariantCulture,
                                                                                                                                         LogConstants.ToolDownloadRetry,
                                                                                                                                         arguments.AttemptNumber + 1,
                                                                                                                                         arguments.RetryDelay.TotalSeconds,
                                                                                                                                         arguments.Outcome.Exception?.Message),
                                                                                                                          LogLevel.Warning);
                                                                                                      return default;
                                                                                                  }
                                                                                              })
                                                                                          .AddTimeout(TimeSpan.FromMinutes(ServiceConstants.Tools.DownloadAttemptTimeoutMinutes))
                                                                                          .Build();

    private static bool IsTransientHttpException(HttpRequestException exception) =>
                                                                                    exception.StatusCode is null
                                                                                    or HttpStatusCode.RequestTimeout
                                                                                    or HttpStatusCode.TooManyRequests
                                                                                    || (int)exception.StatusCode >=
                                                                                    (int)HttpStatusCode.InternalServerError;

    private async Task DownloadFileAsync(
                                         string sourceUrl,
                                         string destinationFileName,
                                         CancellationToken cancellationToken)
    {
        var destinationPath = Path.Combine(ToolPathResolver.ToolsDirectoryPath, destinationFileName);
        var temporaryPath = destinationPath + ServiceConstants.Tools.TemporaryFileExtension;

        await DownloadToTemporaryFileAsync(
                                            sourceUrl,
                                            temporaryPath,
                                            ValidateWindowsExecutable,
                                            cancellationToken);
        try
        {
            File.Move(temporaryPath, destinationPath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private async Task DownloadToTemporaryFileAsync(
                                                    string sourceUrl,
                                                    string temporaryPath,
                                                    Action<string>? validateFile,
                                                    CancellationToken cancellationToken)
    {
        try
        {
            await downloadPipeline.ExecuteAsync(
                                                async attemptToken =>
                                                {
                                                    File.Delete(temporaryPath);
                                                    await DownloadOnceAsync(
                                                                            sourceUrl,
                                                                            temporaryPath,
                                                                            attemptToken);
                                                    validateFile?.Invoke(temporaryPath);
                                                },
                                                cancellationToken);
        }
        catch
        {
            File.Delete(temporaryPath);
            throw;
        }
    }

    private async Task DownloadOnceAsync(
                                         string sourceUrl,
                                         string destinationPath,
                                         CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
        using var response = await httpClient.SendAsync(
                                                         request,
                                                         HttpCompletionOption.ResponseHeadersRead,
                                                         cancellationToken);
        response.EnsureSuccessStatusCode();

        var expectedLength = response.Content.Headers.ContentLength;
        await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destinationStream = new FileStream(
                                                            destinationPath,
                                                            FileMode.Create,
                                                            FileAccess.Write,
                                                            FileShare.None,
                                                            bufferSize: 81920,
                                                            useAsync: true);

        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        await destinationStream.FlushAsync(cancellationToken);

        var downloadedLength = destinationStream.Length;
        if (downloadedLength < ServiceConstants.Tools.MinimumDownloadedFileSizeBytes)
        {
            throw new IOException(ServiceConstants.Tools.DownloadedFileIsEmpty);
        }

        if (expectedLength.HasValue && downloadedLength != expectedLength.Value)
        {
            throw new IOException(
                                    string.Format(
                                                   CultureInfo.InvariantCulture,
                                                   ServiceConstants.Tools.DownloadedFileSizeMismatch,
                                                   expectedLength.Value,
                                                   downloadedLength));
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
            await DownloadToTemporaryFileAsync(
                                                ServiceConstants.Tools.FfmpegDownloadUrl,
                                                archivePath,
                                                path => ValidateFfmpegArchive(path, missingTools),
                                                cancellationToken);

            using var archive = ZipFile.OpenRead(archivePath);
            var extractedTools = missingTools
                                            .Where(IsFfmpegTool)
                                            .Select(fileName =>
                                            {
                                                progress?.Report(fileName);
                                                return ExtractToolToTemporaryFile(archive, fileName);
                                            })
                                            .ToArray();

            try
            {
                ReplaceToolFiles(extractedTools);
            }
            finally
            {
                foreach (var tool in extractedTools)
                {
                    File.Delete(tool.TemporaryPath);
                    File.Delete(tool.BackupPath);
                }
            }
        }
        finally
        {
            File.Delete(archivePath);
        }
    }

    private static ExtractedToolFile ExtractToolToTemporaryFile(ZipArchive archive, string fileName)
    {
        var entry = archive.Entries.FirstOrDefault(candidate => 
                                                                candidate.FullName.EndsWith($"/bin/{fileName}", 
                                                                StringComparison.OrdinalIgnoreCase))
                                                             ?? throw new InvalidDataException(string.Format(
                                                                                                              CultureInfo.InvariantCulture,
                                                                                                              ServiceConstants.Tools.ArchiveEntryNotFound,
                                                                                                              fileName));

        var destinationPath = Path.Combine(ToolPathResolver.ToolsDirectoryPath, fileName);
        var temporaryPath = destinationPath + ServiceConstants.Tools.TemporaryFileExtension;
        var backupPath = destinationPath + ServiceConstants.Tools.BackupFileExtension;

        entry.ExtractToFile(temporaryPath, overwrite: true);
        if (new FileInfo(temporaryPath).Length < ServiceConstants.Tools.MinimumDownloadedFileSizeBytes)
        {
            throw new InvalidDataException(ServiceConstants.Tools.DownloadedFileIsEmpty);
        }

        return new ExtractedToolFile(destinationPath, temporaryPath, backupPath);
    }

    private static void ValidateWindowsExecutable(string filePath)
    {
        Span<byte> header = stackalloc byte[ServiceConstants.Tools.PortableExecutableHeaderSize];
        using var stream = File.OpenRead(filePath);

        if (stream.Read(header) != header.Length
            || header[0] != ServiceConstants.Tools.PortableExecutableFirstByte
            || header[1] != ServiceConstants.Tools.PortableExecutableSecondByte)
        {
            throw new InvalidDataException(ServiceConstants.Tools.DownloadedExecutableIsInvalid);
        }
    }

    private static void ValidateFfmpegArchive(string archivePath, IReadOnlyCollection<string> requiredFiles)
    {
        using var archive = ZipFile.OpenRead(archivePath);
        foreach (var fileName in requiredFiles.Where(IsFfmpegTool))
        {
            if (!archive.Entries.Any(entry => entry.FullName.EndsWith($"/bin/{fileName}", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDataException(string.Format(
                                                             CultureInfo.InvariantCulture,
                                                             ServiceConstants.Tools.ArchiveEntryNotFound,
                                                             fileName));
            }
        }
    }

    private static void ReplaceToolFiles(IReadOnlyCollection<ExtractedToolFile> tools)
    {
        var replacedTools = new List<ExtractedToolFile>();

        try
        {
            foreach (var tool in tools)
            {
                File.Delete(tool.BackupPath);
                if (File.Exists(tool.DestinationPath))
                {
                    File.Move(tool.DestinationPath, tool.BackupPath, overwrite: true);
                }

                try
                {
                    File.Move(tool.TemporaryPath, tool.DestinationPath, overwrite: true);
                }
                catch
                {
                    if (File.Exists(tool.BackupPath))
                    {
                        File.Move(tool.BackupPath, tool.DestinationPath, overwrite: true);
                    }

                    throw;
                }

                replacedTools.Add(tool);
            }
        }
        catch
        {
            foreach (var tool in replacedTools.AsEnumerable().Reverse())
            {
                File.Delete(tool.DestinationPath);
                if (File.Exists(tool.BackupPath))
                {
                    File.Move(tool.BackupPath, tool.DestinationPath, overwrite: true);
                }
            }

            throw;
        }
    }
}
