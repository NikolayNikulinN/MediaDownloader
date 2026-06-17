namespace MediaDownloader.Services.Models;

public sealed class UrlMediaItem
{
    public string Url { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ThumbnailUrl { get; set; } = string.Empty;

    public string Duration { get; set; } = string.Empty;

    public string UploadDate { get; set; } = string.Empty;

    public string SelectedMediaType { get; set; } = string.Empty;

    public string SelectedVideoQuality { get; set; } = string.Empty;

    public List<string> AvailableVideoQualities { get; set; } = [];
}
