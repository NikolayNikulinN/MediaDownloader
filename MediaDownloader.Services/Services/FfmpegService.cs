using MediaDownloader.Logger.Constants;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Utils;
using System.Diagnostics;

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
            throw new FileNotFoundException(string.Format(LogConstants.ToolNotFound, toolPath), toolPath);
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
            ?? throw new InvalidOperationException(string.Format(LogConstants.ToolStartFailed, toolPath));

        using var cancellationRegistration = cancellationToken.Register(
            () => ProcessUtil.TryKill(process));

        var output = await process.StandardOutput.ReadLineAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return output ?? string.Empty;
    }

    private static string GetToolPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, ServiceConstants.Tools.FolderName, fileName);
}
