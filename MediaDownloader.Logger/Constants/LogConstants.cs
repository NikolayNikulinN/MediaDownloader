namespace MediaDownloader.Logger.Constants
{
    public static class LogConstants
    {
        public const int RetainedFileCountLimit = 10;

        public const string ApplicationFolderName = "MediaDownloader";

        public const string LogsFolderName = "Logs";

        public const string LogFileName = "MediaDownloader-.log";

        public const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        public const string ApplicationStarted = "Application started.";

        public const string ApplicationStopped = "Application stopped.";

        public const string XmlSaveFailed = "Failed to save XML data to '{0}'. {1}";

        public const string XmlLoadFailed = "Failed to load XML data from '{0}'. {1}";

        public const string ToolNotFound = "Tool was not found: '{0}'.";

        public const string ToolStartFailed = "Failed to start tool: '{0}'.";

        public const string YtDlpStartFailed = "Failed to start yt-dlp.";

        public const string YtDlpAnalyzeFailed = "Failed to analyze URL '{0}'. {1}";

        public const string AnalyzeUrlFailed = "Failed to analyze URL. {0}";
    }
}
