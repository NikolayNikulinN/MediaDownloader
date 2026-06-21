namespace MediaDownloader.Services.Constants;

/// <summary>
/// Provides constants used by backend media services.
/// </summary>
public static class ServiceConstants
{
    /// <summary>
    /// Provides external tool constants.
    /// </summary>
    public static class Tools
    {
        public const string FolderName = "Tools";

        public const string YtDlpFileName = "yt-dlp.exe";

        public const string FfmpegFileName = "ffmpeg.exe";

        public const string FfprobeFileName = "ffprobe.exe";

        public const string VersionArgument = "-version";

        public const string YtDlpVersionArgument = "--version";

        public const string YtDlpDisplayName = "yt-dlp";

        public const string FfmpegDisplayName = "FFmpeg";

        public const string GitHubTagNameProperty = "tag_name";

        public const int FfmpegVersionTokenIndex = 2;

        public const int FfmpegVersionTokenCount = 3;

        public const string TemporaryFileExtension = ".download";

        public const string BackupFileExtension = ".backup";

        public const string ZipFileExtension = ".zip";

        public const string YtDlpDownloadUrl = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";

        public const string YtDlpLatestReleaseApiUrl = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";

        public const string FfmpegDownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        public const string FfmpegLatestVersionUrl = "https://www.gyan.dev/ffmpeg/builds/release-version";

        public const string DownloadRequiresConfirmation = "Tool downloads require explicit user confirmation.";

        public const string ArchiveEntryNotFound = "The downloaded FFmpeg archive does not contain '{0}'.";

        public const string DownloadedFileIsEmpty = "The downloaded file is empty.";

        public const string DownloadedFileSizeMismatch = "The downloaded file is incomplete. Expected {0} bytes, received {1} bytes.";

        public const string DownloadedExecutableIsInvalid = "The downloaded executable is invalid.";

        public const int DownloadMaxRetryAttempts = 4;

        public const int DownloadRetryDelaySeconds = 2;

        public const int DownloadAttemptTimeoutMinutes = 15;

        public const int MinimumDownloadedFileSizeBytes = 1;

        public const int PortableExecutableHeaderSize = 2;

        public const byte PortableExecutableFirstByte = 0x4D;

        public const byte PortableExecutableSecondByte = 0x5A;

        public const string HttpUserAgentProductName = "MediaDownloader";

        public const string HttpUserAgentProductVersion = "1.0";

        public static IReadOnlyList<string> RequiredFileNames { get; } =
        [
            YtDlpFileName,
            FfmpegFileName,
            FfprobeFileName
        ];
    }

    /// <summary>
    /// Provides yt-dlp command-line constants.
    /// </summary>
    public static class YtDlp
    {
        public const string DumpJsonArgument = "--dump-json";

        public const string SkipDownloadArgument = "--skip-download";

        public const string NoPlaylistArgument = "--no-playlist";
    }

    /// <summary>
    /// Provides yt-dlp JSON property names.
    /// </summary>
    public static class Json
    {
        public const string Title = "title";

        public const string Uploader = "uploader";

        public const string Channel = "channel";

        public const string Creator = "creator";

        public const string Thumbnail = "thumbnail";

        public const string Duration = "duration";

        public const string UploadDate = "upload_date";

        public const string ReleaseDate = "release_date";

        public const string Formats = "formats";

        public const string VideoCodec = "vcodec";

        public const string NoCodec = "none";

        public const string Height = "height";
    }

    /// <summary>
    /// Provides media-related constants.
    /// </summary>
    public static class Media
    {
        public const string Mp4 = "MP4";

        public const string DownloadStatusReady = "Ready";

        public const string VideoQualityNameFormat = "{0}p";
    }

    /// <summary>
    /// Provides date and duration formats.
    /// </summary>
    public static class DateTimeFormats
    {
        public const string DurationWithHours = @"h\:mm\:ss";

        public const string DurationWithoutHours = @"m\:ss";

        public const string YtDlpUploadDate = "yyyyMMdd";

        public const string DisplayDate = "dd.MM.yyyy";
    }

    /// <summary>
    /// Provides XML element names.
    /// </summary>
    public static class Xml
    {
        public const string RootName = "MediaDownloaderState";

        public const string UrlItemsArrayName = "UrlItems";

        public const string UrlItemName = "UrlItem";
    }
}
