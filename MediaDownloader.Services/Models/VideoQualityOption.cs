namespace MediaDownloader.Services.Models;

/// <summary>
/// Describes an available media quality.
/// </summary>
public sealed class VideoQualityOption
{
    public string Name { get; set; } = string.Empty;

    public bool IsSelected { get; set; }
}
