namespace MediaDownloader.Services.Constants;

public static class ServiceConstants
{
    public static class Tools
    {
        public const string FolderName = "Tools";

        public const string YtDlpFileName = "yt-dlp.exe";

        public const string FfmpegFileName = "ffmpeg.exe";

        public const string FfprobeFileName = "ffprobe.exe";

        public const string VersionArgument = "-version";
    }

    public static class YtDlp
    {
        public const string DumpJsonArgument = "--dump-json";

        public const string SkipDownloadArgument = "--skip-download";

        public const string NoPlaylistArgument = "--no-playlist";
    }

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

    public static class Media
    {
        public const string Mp4 = "MP4";

        public const string DownloadStatusReady = "Ready";

        public const string VideoQualityNameFormat = "{0}p";
    }

    public static class DateTimeFormats
    {
        public const string DurationWithHours = @"h\:mm\:ss";

        public const string DurationWithoutHours = @"m\:ss";

        public const string YtDlpUploadDate = "yyyyMMdd";

        public const string DisplayDate = "dd.MM.yyyy";
    }

    public static class Xml
    {
        public const string RootName = "MediaDownloaderState";

        public const string UrlItemsArrayName = "UrlItems";

        public const string UrlItemName = "UrlItem";
    }
}
