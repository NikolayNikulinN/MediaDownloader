using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Services;
using MediaDownloader.Services.Tests.Constants;
using Xunit;

namespace MediaDownloader.Services.Tests.Services;

public sealed class XmlServiceTests
{
    [Fact]
    public void LoadSettings_creates_default_settings_when_file_is_missing()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var logger = new TestLogger();
        var service = new XmlService(logger, temporaryDirectory.Path);

        var settings = service.LoadSettings();

        Assert.EndsWith(ServiceConstants.Xml.DefaultDownloadsFolderName, settings.SaveFolderPath);
        Assert.True(File.Exists(Path.Combine(temporaryDirectory.Path, ServiceConstants.Xml.SettingsFileName)));
    }

    [Fact]
    public void SaveData_and_LoadData_round_trip_url_items()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var logger = new TestLogger();
        var service = new XmlService(logger, temporaryDirectory.Path);
        var state = new MediaDownloaderState
        {
            UrlItems =
            [
                new UrlMediaItem
                {
                    Url = TestConstants.Xml.Url,
                    Title = TestConstants.Xml.Title,
                    SelectedMediaType = TestConstants.Media.Mp4,
                    SelectedVideoQuality = TestConstants.Media.VideoQuality720p,
                    AvailableVideoQualities =
                    [
                        new VideoQualityOption { Name = TestConstants.Media.VideoQuality720p, IsSelected = true }
                    ],
                    DownloadProgress = 42,
                    DownloadStatus = TestConstants.Media.DownloadingStatus,
                    DownloadDate = TestConstants.Media.DownloadDate,
                    DownloadFilePath = TestConstants.Media.DownloadFilePath
                }
            ]
        };

        service.SaveData(state);
        var loaded = service.LoadData();

        var item = Assert.Single(loaded.UrlItems);
        Assert.Equal(TestConstants.Xml.Url, item.Url);
        var quality = Assert.Single(item.AvailableVideoQualities);
        Assert.Equal(TestConstants.Media.VideoQuality720p, quality.Name);
        Assert.Equal(42, item.DownloadProgress);
    }

    [Fact]
    public void LoadData_replaces_corrupt_xml_with_empty_state_and_logs_error()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var logger = new TestLogger();
        var service = new XmlService(logger, temporaryDirectory.Path);
        Directory.CreateDirectory(temporaryDirectory.Path);
        File.WriteAllText(Path.Combine(temporaryDirectory.Path, ServiceConstants.Xml.DataFileName), TestConstants.Xml.CorruptData);

        var loaded = service.LoadData();

        Assert.Empty(loaded.UrlItems);
        Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    private sealed class TestLogger : ILoggerWrapper
    {
        public List<(string Message, LogLevel Level)> Entries { get; } = [];

        public void Write(string message) => Entries.Add((message, LogLevel.Info));

        public void Write(string message, LogLevel level) => Entries.Add((message, level));

        public void CloseAndFlush()
        {
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString(TestConstants.GuidFormat));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
