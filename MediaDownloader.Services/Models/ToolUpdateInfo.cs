namespace MediaDownloader.Services.Models;

/// <summary>
/// Describes an available update for an external tool.
/// </summary>
public sealed class ToolUpdateInfo
{
    public required string DisplayName { get; init; }

    public required string CurrentVersion { get; init; }

    public required string LatestVersion { get; init; }

    public required IReadOnlyCollection<string> FileNames { get; init; }
}
