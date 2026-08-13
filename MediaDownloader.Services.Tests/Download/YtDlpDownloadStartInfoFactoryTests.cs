using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Download;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Tests.Constants;
using Xunit;

namespace MediaDownloader.Services.Tests.Download;

public sealed class YtDlpDownloadStartInfoFactoryTests
{
    [Fact]
    public void Create_adds_video_quality_selector_for_selected_height()
    {
        var request = new MediaDownloadRequest
        {
            Url = TestConstants.ExampleVideoUrl,
            OutputFolder = TestConstants.DownloadsPath,
            IsAudioOnly = false,
            VideoQuality = TestConstants.Media.VideoQuality720p
        };

        var startInfo = YtDlpDownloadStartInfoFactory.Create(TestConstants.YtDlpFileName, request, TestConstants.TempPath);
        var arguments = startInfo.ArgumentList.ToArray();

        Assert.Contains(ServiceConstants.Download.FormatArgument, arguments);
        Assert.Equal(
            TestConstants.Download.VideoFormatSelector720p,
            arguments[Array.IndexOf(arguments, ServiceConstants.Download.FormatArgument) + 1]);
        AssertContainsInOrder(
            arguments,
            ServiceConstants.Download.MergeOutputFormatArgument,
            ServiceConstants.Download.Mp4Format,
            ServiceConstants.Download.RemuxVideoArgument,
            ServiceConstants.Download.Mp4Format);
        Assert.Equal(request.Url, arguments.Last());
    }

    [Fact]
    public void Create_uses_best_video_selector_when_quality_is_invalid()
    {
        var request = new MediaDownloadRequest
        {
            Url = TestConstants.ExampleVideoUrl,
            OutputFolder = TestConstants.DownloadsPath,
            IsAudioOnly = false,
            VideoQuality = TestConstants.Media.InvalidVideoQuality
        };

        var startInfo = YtDlpDownloadStartInfoFactory.Create(TestConstants.YtDlpFileName, request, TestConstants.TempPath);
        var arguments = startInfo.ArgumentList.ToArray();

        Assert.Equal(
            ServiceConstants.Download.BestVideoFormatSelector,
            arguments[Array.IndexOf(arguments, ServiceConstants.Download.FormatArgument) + 1]);
    }

    [Fact]
    public void Create_adds_audio_arguments_for_audio_only_request()
    {
        var request = new MediaDownloadRequest
        {
            Url = TestConstants.ExampleVideoUrl,
            OutputFolder = TestConstants.DownloadsPath,
            IsAudioOnly = true
        };

        var startInfo = YtDlpDownloadStartInfoFactory.Create(TestConstants.YtDlpFileName, request, TestConstants.TempPath);
        var arguments = startInfo.ArgumentList.ToArray();

        AssertContainsInOrder(
            arguments,
            ServiceConstants.Download.ExtractAudioArgument,
            ServiceConstants.Download.AudioFormatArgument,
            ServiceConstants.Download.Mp3Format,
            ServiceConstants.Download.AudioQualityArgument,
            ServiceConstants.Download.BestAudioQuality,
            ServiceConstants.Download.FormatArgument,
            ServiceConstants.Download.BestAudioFormatSelector);
        Assert.DoesNotContain(ServiceConstants.Download.MergeOutputFormatArgument, arguments);
    }

    private static void AssertContainsInOrder(IReadOnlyList<string> values, params string[] expectedValues)
    {
        var searchStart = 0;
        foreach (var expectedValue in expectedValues)
        {
            var index = Array.FindIndex(values.ToArray(), searchStart, value => value == expectedValue);
            Assert.True(
                index >= 0,
                string.Format(
                    TestConstants.Download.MissingOrderedArgumentMessageFormat,
                    expectedValue,
                    searchStart - 1));
            searchStart = index + 1;
        }
    }
}
