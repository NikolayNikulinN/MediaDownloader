namespace MediaDownloader.Tests.Constants;

internal static class TestConstants
{
    public const string OutputFolder = @"C:\Downloads";

    public const string SelectedFolder = @"D:\Selected";

    public const string ToolsFolder = @"C:\Tools";

    public const string VideoUrl = "https://example.test/video";

    public const string DuplicateVideoUrlUpper = "HTTPS://EXAMPLE.TEST/VIDEO";

    public const string SecondVideoUrl = "https://example.test/second-video";

    public const string FailedVideoUrl = "https://example.test/failure";

    public const string VideoTitle = "Video";

    public const string SecondVideoTitle = "Second video";

    public const string Author = "Author";

    public const string ThumbnailUrl = "https://example.test/thumb.jpg";

    public const string Duration = "2:05";

    public const string UploadDate = "13.08.2026";

    public const string DownloadDate = "13.08.2026 12:00";

    public const string DownloadPath = @"C:\Downloads\Video.mp4";

    public const string ToolName = "yt-dlp";

    public const string CurrentToolVersion = "2026.08.01";

    public const string LatestToolVersion = "2026.08.13";

    public const string ToolFileName = "yt-dlp.exe";

    public const string AnalyzeFailureMessage = "Analysis failed";

    public const string DownloadFailureMessage = "Download failed";

    public const string UpdateFailureMessage = "Update failed";

    public const int Progress50 = 50;

    public static class Media
    {
        public const string Mp4 = "MP4";

        public const string Mp3 = "MP3";

        public const string Quality1080p = "1080p";

        public const string Quality720p = "720p";

        public const string Quality480p = "480p";
    }

    public static class Support
    {
        public const string NoFakeYtDlpResultConfigured = "No fake yt-dlp result configured.";

        public const string NoFakeDownloadResultConfigured = "No fake download result configured.";

        public const string AsyncConditionTimeout = "The expected condition was not reached before the timeout.";
    }
}
