using MediaDownloader.Logger.Constants;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Utils;
using System.Diagnostics;
using System.Globalization;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Provides access to bundled FFmpeg and FFprobe tools.
/// </summary>
public sealed class FfmpegService : IFfmpegService
{
    public string FfmpegPath => GetToolPath(ServiceConstants.Tools.FfmpegFileName);

    public string FfprobePath => GetToolPath(ServiceConstants.Tools.FfprobeFileName);

    public bool AreToolsAvailable() =>
        File.Exists(FfmpegPath) && File.Exists(FfprobePath);

    public Task<string> GetFfmpegVersionAsync(CancellationToken cancellationToken = default) =>
        GetToolVersionAsync(FfmpegPath, cancellationToken);

    public Task<string> GetFfprobeVersionAsync(CancellationToken cancellationToken = default) =>
        GetToolVersionAsync(FfprobePath, cancellationToken);

    private static async Task<string> GetToolVersionAsync(string toolPath, CancellationToken cancellationToken)
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

        startInfo.ArgumentList.Add(ServiceConstants.Tools.VersionArgument);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, LogConstants.ToolStartFailed, toolPath));

        using var cancellationRegistration = cancellationToken.Register(
            () => ProcessUtil.TryKill(process));

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

    private static string GetToolPath(string fileName) => ToolPathResolver.Resolve(fileName);
}
