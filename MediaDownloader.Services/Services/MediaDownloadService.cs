using MediaDownloader.Logger.Constants;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Download;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Utils;
using System.Diagnostics;
using System.Globalization;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Downloads media with yt-dlp and removes temporary data when cancelled.
/// </summary>
public sealed class MediaDownloadService : IMediaDownloadService
{
    /// <inheritdoc />
    public async Task<MediaDownloadResult> DownloadAsync(
                                                         MediaDownloadRequest request,
                                                         IProgress<MediaDownloadProgress>? progress = null,
                                                         CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Url);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputFolder);

        var ytDlpPath = ToolPathResolver.Resolve(ServiceConstants.Tools.YtDlpFileName);
        if (!File.Exists(ytDlpPath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, LogConstants.ToolNotFound, ytDlpPath), ytDlpPath);
        }

        Directory.CreateDirectory(request.OutputFolder);
        var temporaryDirectoryPath = Path.Combine(
                                                  request.OutputFolder,
                                                  ServiceConstants.Download.TemporaryFolderName,
                                                  Guid.NewGuid().ToString(ServiceConstants.Download.GuidFormat));

        Directory.CreateDirectory(temporaryDirectoryPath);

        var finalFilePath = string.Empty;
        var processStartUtc = DateTime.UtcNow;
        var errors = new List<string>();
        using var process = new Process
            {
                StartInfo = YtDlpDownloadStartInfoFactory.Create(ytDlpPath, request, temporaryDirectoryPath),
                EnableRaisingEvents = true
            };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(LogConstants.YtDlpStartFailed);
            }

            using var cancellationRegistration = cancellationToken.Register(() => ProcessUtil.TryKill(process));
            void ProcessOutputLine(string line)
            {
                if (line.StartsWith(ServiceConstants.Download.ProgressPrefix, StringComparison.Ordinal))
                {
                    progress?.Report(new MediaDownloadProgress
                                                              {
                                                                 Percentage = MediaDownloadOutputParser.ParsePercentage(line)
                                                              });
                }
                else if (line.StartsWith(ServiceConstants.Download.PostProcessPrefix, StringComparison.Ordinal))
                {
                    progress?.Report(new MediaDownloadProgress
                                                               {
                                                                  Percentage = 100,
                                                                  IsPostProcessing = true
                                                               });
                }
                else if (line.StartsWith(ServiceConstants.Download.FilePathPrefix, StringComparison.Ordinal))
                {
                    finalFilePath = line[ServiceConstants.Download.FilePathPrefix.Length..];
                }
            }

            var outputTask = ReadOutputAsync(process.StandardOutput, ProcessOutputLine, cancellationToken);
            var errorTask = ReadOutputAsync(
                                            process.StandardError,
                                            line =>
                                            {
                                                if (line.StartsWith(
                                                        ServiceConstants.Download.ProgressPrefix,
                                                        StringComparison.Ordinal)
                                                    || line.StartsWith(
                                                        ServiceConstants.Download.PostProcessPrefix,
                                                        StringComparison.Ordinal)
                                                    || line.StartsWith(
                                                        ServiceConstants.Download.FilePathPrefix,
                                                        StringComparison.Ordinal))
                                                {
                                                    ProcessOutputLine(line);
                                                }
                                                else if (!string.IsNullOrWhiteSpace(line))
                                                {
                                                    errors.Add(line);
                                                }
                                            },
                                            cancellationToken);

            try
            {
                await process.WaitForExitAsync(cancellationToken);
                await Task.WhenAll(outputTask, errorTask);
            }
            catch (OperationCanceledException)
            {
                ProcessUtil.TryKill(process);
                await process.WaitForExitAsync(CancellationToken.None);
                try
                {
                    await Task.WhenAll(outputTask, errorTask);
                }
                catch (OperationCanceledException)
                {
                }

                throw;
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine, errors));
            }

            if (string.IsNullOrWhiteSpace(finalFilePath)
                || !File.Exists(MediaDownloadOutputParser.ResolveFinalFilePath(finalFilePath, request.OutputFolder)))
            {
                finalFilePath = MediaDownloadOutputParser.FindCompletedFilePath(request, processStartUtc);
                if (string.IsNullOrWhiteSpace(finalFilePath))
                {
                    throw new FileNotFoundException(
                        ServiceConstants.Download.CompletedFileNotFound);
                }
            }
            else
            {
                finalFilePath = MediaDownloadOutputParser.ResolveFinalFilePath(finalFilePath, request.OutputFolder);
            }

            return new MediaDownloadResult
            {
                FilePath = finalFilePath
            };
        }
        finally
        {
            DeleteTemporaryDirectory(temporaryDirectoryPath);
        }
    }

    private static async Task ReadOutputAsync(
                                               StreamReader reader,
                                               Action<string> processLine,
                                               CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            processLine(line);
        }
    }

    private static void DeleteTemporaryDirectory(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }

            var parentDirectory = Path.GetDirectoryName(directoryPath);
            if (parentDirectory is not null
                && Directory.Exists(parentDirectory)
                && !Directory.EnumerateFileSystemEntries(parentDirectory).Any())
            {
                Directory.Delete(parentDirectory);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
