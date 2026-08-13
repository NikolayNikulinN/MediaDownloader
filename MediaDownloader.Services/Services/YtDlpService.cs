using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Parsing;
using MediaDownloader.Services.Utils;
using System.Diagnostics;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Uses yt-dlp to retrieve metadata for media URLs.
/// </summary>
public sealed class YtDlpService : IYtDlpService
{
    private readonly ILoggerWrapper loggerWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="YtDlpService"/> class.
    /// </summary>
    /// <param name="loggerWrapper">Application logger.</param>
    public YtDlpService(ILoggerWrapper loggerWrapper)
    {
        ArgumentNullException.ThrowIfNull(loggerWrapper);
        this.loggerWrapper = loggerWrapper;
    }

    /// <inheritdoc />
    public async Task<UrlMediaItem> GetMediaInfoAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        var ytDlpPath = GetToolPath(ServiceConstants.Tools.YtDlpFileName);

        if (!File.Exists(ytDlpPath))
        {
            throw new FileNotFoundException(string.Format(LogConstants.ToolNotFound, ytDlpPath), ytDlpPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add(ServiceConstants.YtDlp.DumpJsonArgument);
        startInfo.ArgumentList.Add(ServiceConstants.YtDlp.SkipDownloadArgument);
        startInfo.ArgumentList.Add(ServiceConstants.YtDlp.NoPlaylistArgument);
        startInfo.ArgumentList.Add(url);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(LogConstants.YtDlpStartFailed);

        // Cancelling managed reads does not terminate yt-dlp, so explicitly stop
        // the process tree to avoid leaving a background downloader running.
        using var cancellationRegistration = cancellationToken.Register(
            () => ProcessUtil.TryKill(process));

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            loggerWrapper.Write(string.Format(LogConstants.YtDlpAnalyzeFailed, url, error), LogLevel.Error);
            throw new InvalidOperationException(error);
        }

        return YtDlpMediaInfoParser.Parse(url, output);
    }

    private static string GetToolPath(string fileName) => ToolPathResolver.Resolve(fileName);
}
