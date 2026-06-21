using MediaDownloader.Logger.Constants;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
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
            throw new FileNotFoundException(string.Format(LogConstants.ToolNotFound, ytDlpPath), ytDlpPath);
        }

        Directory.CreateDirectory(request.OutputFolder);
        var temporaryDirectoryPath = Path.Combine(
                                                  request.OutputFolder,
                                                  ServiceConstants.Download.TemporaryFolderName,
                                                  Guid.NewGuid().ToString(ServiceConstants.Download.GuidFormat));

        Directory.CreateDirectory(temporaryDirectoryPath);

        var finalFilePath = string.Empty;
        var errors = new List<string>();
        using var process = new Process
        {
            StartInfo = CreateStartInfo(ytDlpPath, request, temporaryDirectoryPath),
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
                                                                 Percentage = ParsePercentage(line)
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
                || !File.Exists(finalFilePath))
            {
                throw new FileNotFoundException(
                    ServiceConstants.Download.CompletedFileNotFound);
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

    private static ProcessStartInfo CreateStartInfo(
                                                    string ytDlpPath,
                                                    MediaDownloadRequest request,
                                                    string temporaryDirectoryPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        AddCommonArguments(
                            startInfo,
                            request,
                            temporaryDirectoryPath);

        if (request.IsAudioOnly)
        {
            AddAudioArguments(startInfo);
        }
        else
        {
            AddVideoArguments(startInfo, request.VideoQuality);
        }

        startInfo.ArgumentList.Add(request.Url);
        return startInfo;
    }

    private static void AddCommonArguments(
                                            ProcessStartInfo startInfo,
                                            MediaDownloadRequest request,
                                            string temporaryDirectoryPath)
    {
        startInfo.ArgumentList.Add(ServiceConstants.YtDlp.NoPlaylistArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.IgnoreConfigArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoSimulateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoUpdateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoColorArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NewLineArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.WindowsFileNamesArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressTemplateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PostProcessTemplateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PostProcessTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PrintArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AfterMoveFilePathTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.FfmpegLocationArgument);
        startInfo.ArgumentList.Add(ToolPathResolver.ToolsDirectoryPath);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PathsArgument);
        startInfo.ArgumentList.Add($"{ServiceConstants.Download.TempPathPrefix}{temporaryDirectoryPath}");
        startInfo.ArgumentList.Add(ServiceConstants.Download.PathsArgument);
        startInfo.ArgumentList.Add($"{ServiceConstants.Download.HomePathPrefix}{request.OutputFolder}");
        startInfo.ArgumentList.Add(ServiceConstants.Download.OutputArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.OutputTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ForceOverwriteArgument);
    }

    private static void AddAudioArguments(ProcessStartInfo startInfo)
    {
        startInfo.ArgumentList.Add(ServiceConstants.Download.ExtractAudioArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AudioFormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp3Format);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AudioQualityArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.BestAudioQuality);
        startInfo.ArgumentList.Add(ServiceConstants.Download.FormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.BestAudioFormatSelector);
    }

    private static void AddVideoArguments(ProcessStartInfo startInfo, string selectedQuality)
    {
        var height = ParseHeight(selectedQuality);
        var formatSelector = height > 0
                                        ? string.Format(
                                                        CultureInfo.InvariantCulture,
                                                        ServiceConstants.Download.VideoFormatSelector,
                                                        height)
                                        : ServiceConstants.Download.BestVideoFormatSelector;

        startInfo.ArgumentList.Add(ServiceConstants.Download.FormatArgument);
        startInfo.ArgumentList.Add(formatSelector);
        startInfo.ArgumentList.Add(ServiceConstants.Download.MergeOutputFormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp4Format);
        startInfo.ArgumentList.Add(ServiceConstants.Download.RemuxVideoArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp4Format);
    }

    private static int ParseHeight(string quality)
    {
        var numericValue = quality.TrimEnd('p', 'P');
        return int.TryParse(
                            numericValue,
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out var height)
                            ? height
                            : 0;
    }

    private static int ParsePercentage(string line)
    {
        var value = line[ServiceConstants.Download.ProgressPrefix.Length..]
                                                                            .Replace(
                                                                                     ServiceConstants.Download.PercentSymbol,
                                                                                     string.Empty,
                                                                                     StringComparison.Ordinal)
                                                                            .Trim();

        return double.TryParse(
                                value,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out var percentage)
                                ? Math.Clamp((int)Math.Round(percentage), 0, 100)
                                : 0;
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
