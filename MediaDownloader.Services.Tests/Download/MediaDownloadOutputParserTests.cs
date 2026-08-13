using MediaDownloader.Services.Download;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Tests.Constants;
using Xunit;

namespace MediaDownloader.Services.Tests.Download;

public sealed class MediaDownloadOutputParserTests
{
    [Theory]
    [InlineData(TestConstants.Download.Progress12Point6, 13)]
    [InlineData(TestConstants.Download.Progress12Point4, 12)]
    [InlineData(TestConstants.Download.Progress150, 100)]
    [InlineData(TestConstants.Download.ProgressNegative5, 0)]
    [InlineData(TestConstants.Download.ProgressInvalid, 0)]
    public void ParsePercentage_rounds_and_clamps_values(string line, int expected)
    {
        Assert.Equal(expected, MediaDownloadOutputParser.ParsePercentage(line));
    }

    [Fact]
    public void ResolveFinalFilePath_combines_relative_path_with_output_folder()
    {
        var result = MediaDownloadOutputParser.ResolveFinalFilePath(
            TestConstants.Download.QuotedVideoFileName,
            TestConstants.DownloadsPath);

        Assert.Equal(Path.Combine(TestConstants.DownloadsPath, TestConstants.Media.VideoFileName), result);
    }

    [Fact]
    public void TryGetYouTubeVideoId_returns_decoded_v_parameter()
    {
        var result = MediaDownloadOutputParser.TryGetYouTubeVideoId(TestConstants.Download.YouTubeUrlWithEncodedVideoId);

        Assert.Equal(TestConstants.Download.DecodedVideoId, result);
    }

    [Fact]
    public void TryGetYouTubeVideoId_returns_empty_string_for_invalid_url()
    {
        var result = MediaDownloadOutputParser.TryGetYouTubeVideoId(TestConstants.InvalidUrl);

        Assert.Equal(TestConstants.Empty, result);
    }

    [Fact]
    public void FindCompletedFilePath_prefers_matching_youtube_id_and_newest_write_time()
    {
        var outputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(TestConstants.GuidFormat));
        Directory.CreateDirectory(outputFolder);

        try
        {
            var processStartUtc = DateTime.UtcNow;
            var oldMatchingFile = Path.Combine(outputFolder, TestConstants.Download.OldMatchingFileName);
            var newestMatchingFile = Path.Combine(outputFolder, TestConstants.Download.NewestMatchingFileName);
            var unrelatedFile = Path.Combine(outputFolder, TestConstants.Download.UnrelatedFileName);

            File.WriteAllText(oldMatchingFile, TestConstants.Download.OldFileContent);
            File.WriteAllText(newestMatchingFile, TestConstants.Download.NewFileContent);
            File.WriteAllText(unrelatedFile, TestConstants.Download.UnrelatedFileContent);
            File.SetLastWriteTimeUtc(oldMatchingFile, processStartUtc.AddSeconds(1));
            File.SetLastWriteTimeUtc(newestMatchingFile, processStartUtc.AddSeconds(3));
            File.SetLastWriteTimeUtc(unrelatedFile, processStartUtc.AddSeconds(5));

            var request = new MediaDownloadRequest
            {
                Url = TestConstants.Download.YouTubeUrlWithVideoId,
                OutputFolder = outputFolder,
                IsAudioOnly = false
            };

            var result = MediaDownloadOutputParser.FindCompletedFilePath(request, processStartUtc);

            Assert.Equal(newestMatchingFile, result);
        }
        finally
        {
            Directory.Delete(outputFolder, recursive: true);
        }
    }
}
