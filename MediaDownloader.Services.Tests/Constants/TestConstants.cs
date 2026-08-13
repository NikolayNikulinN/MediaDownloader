namespace MediaDownloader.Services.Tests.Constants;

internal static class TestConstants
{
    public const string Empty = "";

    public const string GuidFormat = "N";

    public const string ExampleWatchUrl = "https://example.test/watch";

    public const string ExampleVideoUrl = "https://example.test/video";

    public const string InvalidUrl = "not-a-url";

    public const string DownloadsPath = @"C:\Downloads";

    public const string TempPath = @"C:\Temp";

    public const string YtDlpFileName = "yt-dlp.exe";

    public static class Media
    {
        public const string Title = "Sample video";

        public const string Channel = "Sample channel";

        public const string ThumbnailUrl = "https://example.test/thumb.jpg";

        public const string Mp4 = "MP4";

        public const string VideoQuality1080p = "1080p";

        public const string VideoQuality720p = "720p";

        public const string VideoQuality480p = "480p";

        public const string InvalidVideoQuality = "best";

        public const string VideoFileName = "video.mp4";

        public const string DownloadingStatus = "Downloading";

        public const string DownloadDate = "13.08.2026 12:00";

        public const string DownloadFilePath = @"C:\Downloads\Video.mp4";
    }

    public static class YtDlpJson
    {
        public const string DisplayUploadDate = "05.07.2026";

        public const string MediaInfo = """
        {
          "title": "Sample video",
          "uploader": "",
          "channel": "Sample channel",
          "thumbnail": "https://example.test/thumb.jpg",
          "duration": 3661,
          "upload_date": "20260705",
          "formats": [
            { "vcodec": "none", "height": 1080 },
            { "vcodec": "avc1", "height": 720 },
            { "vcodec": "vp9", "height": 1080 },
            { "vcodec": "avc1", "height": 480 }
          ]
        }
        """;

        public const string WithoutFormats = """{ "title": "No formats" }""";
    }

    public static class Dates
    {
        public const string YtDlpUploadDate = "20260813";

        public const string DisplayUploadDate = "13.08.2026";

        public const string InvalidDate = "not-a-date";
    }

    public static class Durations
    {
        public const string Seconds59 = "0:59";

        public const string Seconds125 = "2:05";

        public const string Seconds3661 = "1:01:01";
    }

    public static class Download
    {
        public const string VideoFormatSelector720p = "best[height<=720][ext=mp4]/best[height<=720]/bestvideo[height<=720]+bestaudio/best";

        public const string Progress12Point6 = "PROGRESS:12.6%";

        public const string Progress12Point4 = "PROGRESS:12.4%";

        public const string Progress150 = "PROGRESS:150%";

        public const string ProgressNegative5 = "PROGRESS:-5%";

        public const string ProgressInvalid = "PROGRESS:not-a-number";

        public const string QuotedVideoFileName = "\"video.mp4\"";

        public const string YouTubeUrlWithEncodedVideoId = "https://www.youtube.com/watch?list=abc&v=id%2D123";

        public const string DecodedVideoId = "id-123";

        public const string YouTubeUrlWithVideoId = "https://www.youtube.com/watch?v=abc123";

        public const string OldMatchingFileName = "Old [abc123].mp4";

        public const string NewestMatchingFileName = "New [abc123].mp4";

        public const string UnrelatedFileName = "Other [zzz999].mp4";

        public const string OldFileContent = "old";

        public const string NewFileContent = "new";

        public const string UnrelatedFileContent = "other";

        public const string MissingOrderedArgumentMessageFormat = "Expected '{0}' after index {1}.";
    }

    public static class Versions
    {
        public const string FfmpegOutput = "ffmpeg version 7.1.1-essentials_build-www.gyan.dev Copyright";

        public const string FfmpegVersion = "7.1.1-essentials_build-www.gyan.dev";

        public const string Unexpected = "unexpected";

        public const string UnexpectedPadded = "  unexpected  ";

        public const string CurrentVersion = "2026.08.13";

        public const string CurrentVersionCustom = "2026.08.13-custom";

        public const string LatestVersion = "2026.08.13";

        public const string OlderLatestVersion = "2026.08.12";

        public const string UpperCaseVersion = "ABC";

        public const string LowerCaseVersion = "abc";
    }

    public static class Xml
    {
        public const string Url = ExampleVideoUrl;

        public const string Title = "Video";

        public const string CorruptData = "<Data>";
    }
}
