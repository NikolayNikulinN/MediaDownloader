using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;

namespace MediaDownloader.Tests.Support;

internal static class TestModels
{
    public static UrlMediaItem CreateMediaItem(
        string url = TestConstants.VideoUrl,
        string title = TestConstants.VideoTitle,
        params string[] qualities)
    {
        if (qualities.Length == 0)
        {
            qualities = [TestConstants.Media.Quality1080p, TestConstants.Media.Quality720p];
        }

        return new UrlMediaItem
        {
            Url = url,
            Title = title,
            Author = TestConstants.Author,
            ThumbnailUrl = TestConstants.ThumbnailUrl,
            Duration = TestConstants.Duration,
            UploadDate = TestConstants.UploadDate,
            SelectedMediaType = TestConstants.Media.Mp4,
            SelectedVideoQuality = qualities[0],
            AvailableVideoQualities = qualities
                .Select((quality, index) => new VideoQualityOption { Name = quality, IsSelected = index == 0 })
                .ToList()
        };
    }
}
