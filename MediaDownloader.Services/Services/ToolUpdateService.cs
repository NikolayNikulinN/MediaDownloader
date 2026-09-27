using MediaDownloader.Logger.Constants;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Updates;
using MediaDownloader.Services.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Checks local external-tool versions against their release endpoints.
/// </summary>
public sealed class ToolUpdateService : IToolUpdateService
{
    private readonly IFfmpegService ffmpegService;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolUpdateService"/> class.
    /// </summary>
    public ToolUpdateService(HttpClient httpClient, IFfmpegService ffmpegService)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(ffmpegService);

        this.httpClient = httpClient;
        this.ffmpegService = ffmpegService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ToolUpdateInfo>> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(ServiceConstants.Tools.UpdateCheckTimeoutSeconds));
        var results = await Task.WhenAll(CheckYtDlpAsync(timeout.Token), CheckFfmpegAsync(timeout.Token), CheckDenoAsync(timeout.Token));
        return results
                       .Where(update => update is not null)
                       .Cast<ToolUpdateInfo>()
                       .ToArray();
    }

    private async Task<ToolUpdateInfo?> CheckYtDlpAsync(CancellationToken cancellationToken)
    {
        var currentVersion = await GetToolOutputAsync(
                                                       ToolPathResolver.Resolve(ServiceConstants.Tools.YtDlpFileName),
                                                       ServiceConstants.Tools.YtDlpVersionArgument,
                                                       cancellationToken);

        var response = await httpClient.GetStringAsync(ServiceConstants.Tools.YtDlpLatestReleaseApiUrl, cancellationToken);
        using var document = JsonDocument.Parse(response);
        var latestVersion = document.RootElement.GetProperty(ServiceConstants.Tools.GitHubTagNameProperty).GetString()?.Trim() ?? string.Empty;

        return ToolVersionComparer.VersionsMatch(currentVersion, latestVersion)
                                                            ? null
                                                            : new ToolUpdateInfo
                                                            {
                                                                DisplayName = ServiceConstants.Tools.YtDlpDisplayName,
                                                                CurrentVersion = currentVersion,
                                                                LatestVersion = latestVersion,
                                                                FileNames = [ServiceConstants.Tools.YtDlpFileName]
                                                            };
    }

    private async Task<ToolUpdateInfo?> CheckDenoAsync(CancellationToken cancellationToken)
    {
        var output = await GetToolOutputAsync(ToolPathResolver.Resolve(ServiceConstants.Tools.DenoFileName),
            ServiceConstants.Tools.YtDlpVersionArgument, cancellationToken);
        var currentVersion = output.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1) ?? output;
        var response = await httpClient.GetStringAsync(ServiceConstants.Tools.DenoLatestReleaseApiUrl, cancellationToken);
        using var document = JsonDocument.Parse(response);
        var latestVersion = document.RootElement.GetProperty(ServiceConstants.Tools.GitHubTagNameProperty).GetString()?.TrimStart('v') ?? string.Empty;
        return ToolVersionComparer.VersionsMatch(currentVersion, latestVersion) ? null : new ToolUpdateInfo
        {
            DisplayName = ServiceConstants.Tools.DenoDisplayName,
            CurrentVersion = currentVersion,
            LatestVersion = latestVersion,
            FileNames = [ServiceConstants.Tools.DenoFileName]
        };
    }

    private async Task<ToolUpdateInfo?> CheckFfmpegAsync(CancellationToken cancellationToken)
    {
        var versionOutput = await ffmpegService.GetFfmpegVersionAsync(cancellationToken);
        var currentVersion = ToolVersionComparer.ParseFfmpegVersion(versionOutput);
        var latestVersion = (await httpClient.GetStringAsync(ServiceConstants.Tools.FfmpegLatestVersionUrl, cancellationToken)).Trim();

        return ToolVersionComparer.VersionsMatch(currentVersion, latestVersion)
                                                            ? null
                                                            : new ToolUpdateInfo
                                                            {
                                                                DisplayName = ServiceConstants.Tools.FfmpegDisplayName,
                                                                CurrentVersion = currentVersion,
                                                                LatestVersion = latestVersion,
                                                                FileNames =
                                                                [
                                                                    ServiceConstants.Tools.FfmpegFileName,
                                                                    ServiceConstants.Tools.FfprobeFileName
                                                                ]
                                                            };
    }

    private static async Task<string> GetToolOutputAsync(
                                                         string toolPath,
                                                         string argument,
                                                         CancellationToken cancellationToken)
    {
        if (!File.Exists(toolPath))
        {
            throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, LogConstants.ToolNotFound, toolPath), toolPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(argument);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, LogConstants.ToolStartFailed, toolPath));
        using var cancellationRegistration = cancellationToken.Register(() => ProcessUtil.TryKill(process));

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(error);
        }
        return output.Split('\n')[0].Trim();
    }

}
