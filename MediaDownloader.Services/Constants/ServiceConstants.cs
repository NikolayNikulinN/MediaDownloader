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
        public const string DenoFileName = "deno.exe";
        public const string DenoDownloadUrl = "https://github.com/denoland/deno/releases/latest/download/deno-x86_64-pc-windows-msvc.zip";
        public const string DenoLatestReleaseApiUrl = "https://api.github.com/repos/denoland/deno/releases/latest";
        public const string DenoDisplayName = "Deno";
        public const int UpdateCheckTimeoutSeconds = 60;
        public const int DownloadIdleTimeoutSeconds = 30;
        public const int DownloadBufferSize = 81920;
        public const string DownloadStalled = "Tool download stopped receiving data.";

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

        public const int DownloadMaxRetryAttempts = 2;

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
            FfprobeFileName,
            DenoFileName
        ];
    }

    /// <summary>
    /// Provides yt-dlp command-line constants.
    /// </summary>
    public static class YtDlp
    {
        public const string JsRuntimesArgument = "--js-runtimes";
        public const string DenoRuntimePrefix = "deno:";
        public const string DumpJsonArgument = "--dump-json";

        public const string SkipDownloadArgument = "--skip-download";

        public const string NoPlaylistArgument = "--no-playlist";
    }

    /// <summary>
    /// Provides media download command-line and parsing constants.
    /// </summary>
    public static class Download
    {
        public const string TemporaryFolderName = ".MediaDownloaderTemp";

        public const string GuidFormat = "N";

        public const string NoColorArgument = "--no-color";

        public const string IgnoreConfigArgument = "--ignore-config";

        public const string NoSimulateArgument = "--no-simulate";

        public const string NoUpdateArgument = "--no-update";

        public const string NewLineArgument = "--newline";

        public const string WindowsFileNamesArgument = "--windows-filenames";

        public const string ProgressArgument = "--progress";

        public const string ProgressTemplateArgument = "--progress-template";

        public const string ProgressTemplate = "download:PROGRESS:%(progress._percent_str)s";

        public const string PostProcessTemplateArgument = "--progress-template";

        public const string PostProcessTemplate = "postprocess:POSTPROCESS:active";

        public const string PrintArgument = "--print";

        public const string AfterMoveFilePathTemplate = "after_move:FILEPATH:%(filepath)s";

        public const string FfmpegLocationArgument = "--ffmpeg-location";

        public const string PathsArgument = "--paths";

        public const string TempPathPrefix = "temp:";

        public const string HomePathPrefix = "home:";

        public const string OutputArgument = "--output";

        public const string OutputTemplate = "%(title)s [%(id)s].%(ext)s";

        public const string ForceOverwriteArgument = "--force-overwrites";

        public const string ExtractAudioArgument = "--extract-audio";

        public const string AudioFormatArgument = "--audio-format";

        public const string AudioQualityArgument = "--audio-quality";

        public const string BestAudioQuality = "0";

        public const string FormatArgument = "--format";

        public const string BestAudioFormatSelector = "bestaudio/best";

        public const string VideoFormatSelector = "best[height<={0}][ext=mp4]/best[height<={0}]/bestvideo[height<={0}]+bestaudio/best";

        public const string BestVideoFormatSelector = "best[ext=mp4]/best/bestvideo+bestaudio";

        public const string MergeOutputFormatArgument = "--merge-output-format";

        public const string RemuxVideoArgument = "--remux-video";

        public const string Mp3Format = "mp3";

        public const string Mp4Format = "mp4";

        public const string Mp4FileExtension = ".mp4";

        public const string Mp3FileExtension = ".mp3";

        public const string YouTubeVideoIdQueryParameter = "v";

        public const string YouTubeVideoIdFileNameFormat = "[{0}]";

        public const int FileSearchClockSkewSeconds = 5;

        public const string ProgressPrefix = "PROGRESS:";

        public const string PostProcessPrefix = "POSTPROCESS:";

        public const string FilePathPrefix = "FILEPATH:";

        public const string PercentSymbol = "%";

        public const string CompletedFileNotFound =
            "The completed download file was not found.";
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

        public const string DownloadDate = "dd.MM.yyyy HH:mm";
    }

    /// <summary>
    /// Provides XML element names.
    /// </summary>
    public static class Xml
    {
        public const string ConfigurationFolderName = "Configuration";

        public const string SettingsFileName = "settings.xml";

        public const string DataFileName = "data.xml";

        public const string SettingsRootName = "Settings";

        public const string DataRootName = "Data";

        public const string DefaultDownloadsFolderName = "Downloads";

        public const string TemporaryFileExtension = ".tmp";

        public const string UrlItemsArrayName = "UrlItems";

        public const string UrlItemName = "UrlItem";
    }
}
