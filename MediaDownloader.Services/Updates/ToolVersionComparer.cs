namespace MediaDownloader.Services.Updates;

internal static class ToolVersionComparer
{
    public static string ParseFfmpegVersion(string output)
    {
        var parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length >= Constants.ServiceConstants.Tools.FfmpegVersionTokenCount
            ? parts[Constants.ServiceConstants.Tools.FfmpegVersionTokenIndex]
            : output.Trim();
    }

    public static bool VersionsMatch(string currentVersion, string latestVersion) =>
        currentVersion.Equals(latestVersion, StringComparison.OrdinalIgnoreCase)
        || currentVersion.StartsWith($"{latestVersion}-", StringComparison.OrdinalIgnoreCase);
}
