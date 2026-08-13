using MediaDownloader.Services.Tests.Constants;
using MediaDownloader.Services.Parsing;
using System.Text.Json;
using Xunit;

namespace MediaDownloader.Services.Tests.Parsing;

public sealed class YtDlpMediaInfoParserTests
{
    [Fact]
    public void Parse_maps_metadata_and_video_qualities()
    {
        var result = YtDlpMediaInfoParser.Parse(TestConstants.ExampleWatchUrl, TestConstants.YtDlpJson.MediaInfo);

        Assert.Equal(TestConstants.ExampleWatchUrl, result.Url);
        Assert.Equal(TestConstants.Media.Title, result.Title);
        Assert.Equal(TestConstants.Media.Channel, result.Author);
        Assert.Equal(TestConstants.Media.ThumbnailUrl, result.ThumbnailUrl);
        Assert.Equal(TestConstants.Durations.Seconds3661, result.Duration);
        Assert.Equal(TestConstants.YtDlpJson.DisplayUploadDate, result.UploadDate);
        Assert.Equal(TestConstants.Media.Mp4, result.SelectedMediaType);
        Assert.Equal(TestConstants.Media.VideoQuality1080p, result.SelectedVideoQuality);
        Assert.Equal(
            [
                TestConstants.Media.VideoQuality1080p,
                TestConstants.Media.VideoQuality720p,
                TestConstants.Media.VideoQuality480p
            ],
            result.AvailableVideoQualities.Select(option => option.Name));
        Assert.True(result.AvailableVideoQualities.Single(option => option.Name == TestConstants.Media.VideoQuality1080p).IsSelected);
        Assert.All(
            result.AvailableVideoQualities.Where(option => option.Name != TestConstants.Media.VideoQuality1080p),
            option => Assert.False(option.IsSelected));
    }

    [Theory]
    [InlineData(0, TestConstants.Empty)]
    [InlineData(-1, TestConstants.Empty)]
    [InlineData(59, TestConstants.Durations.Seconds59)]
    [InlineData(125, TestConstants.Durations.Seconds125)]
    [InlineData(3661, TestConstants.Durations.Seconds3661)]
    public void FormatDuration_formats_supported_ranges(double seconds, string expected)
    {
        Assert.Equal(expected, YtDlpMediaInfoParser.FormatDuration(seconds));
    }

    [Theory]
    [InlineData(TestConstants.Dates.YtDlpUploadDate, TestConstants.Dates.DisplayUploadDate)]
    [InlineData(TestConstants.Dates.InvalidDate, TestConstants.Dates.InvalidDate)]
    [InlineData(TestConstants.Empty, TestConstants.Empty)]
    public void FormatUploadDate_formats_yt_dlp_date_or_returns_original(string value, string expected)
    {
        Assert.Equal(expected, YtDlpMediaInfoParser.FormatUploadDate(value));
    }

    [Fact]
    public void GetVideoQualities_returns_empty_list_when_formats_is_missing()
    {
        using var document = JsonDocument.Parse(TestConstants.YtDlpJson.WithoutFormats);

        var result = YtDlpMediaInfoParser.GetVideoQualities(document.RootElement);

        Assert.Empty(result);
    }
}
