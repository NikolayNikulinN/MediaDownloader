namespace MediaDownloader.Logger.Constants;

/// <summary>
/// Provides logging configuration and message templates.
/// </summary>
public static class LogConstants
{
    public const int RetainedFileCountLimit = 10;

    public const string ApplicationFolderName = "MediaDownloader";

    public const string LogsFolderName = "Logs";

    public const string LogFileName = "MediaDownloader-.log";

    public const string WriteProbeFileNameFormat = ".write-test-{0:N}.tmp";

    public const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public const string ApplicationStarted = "Application started.";

    public const string ApplicationStopped = "Application stopped.";

    public const string XmlSaveFailed = "Failed to save XML data to '{0}'. {1}";

    public const string XmlLoadFailed = "Failed to load XML data from '{0}'. {1}";

    public const string ToolNotFound = "Tool was not found: '{0}'.";

    public const string ToolStartFailed = "Failed to start tool: '{0}'.";

    public const string YtDlpStartFailed = "Failed to start yt-dlp.";

    public const string YtDlpAnalyzeFailed = "Failed to analyze URL '{0}'. {1}";

    public const string AnalyzeUrlFailed = "Failed to analyze URL. {0}";

    public const string ToolBootstrapFailed = "Required tool verification or download failed. {0}";

    public const string ToolUpdateCheckFailed = "Tool update check failed. {0}";

    public const string ToolUpdateFailed = "Tool update failed. {0}";

    public const string ToolDownloadRetry = "Tool download retry {0} will start in {1:F1} seconds. {2}";

    public const string MediaDownloadFailed = "Media download failed for URL '{0}'. {1}";

    public const string MediaItemsDownloadBulkStarted = "Bulk media download started. Items requested: {0}. Each item will use its current selected media type and quality.";

    public const string MediaItemDeletedFromList = "Media item deleted from list only. Url: '{0}'. Title: '{1}'. Media type: '{2}'. Download date: '{3}'.";

    public const string MediaItemDeletedFromListAndFileSystem = "Media item deleted from list and file system. Url: '{0}'. Title: '{1}'. Media type: '{2}'. Download date: '{3}'. File path: '{4}'.";

    public const string MediaItemDeletedFromListFilePathMissing = "Media item deleted from list. File deletion was requested, but no downloaded file path is stored. Url: '{0}'. Title: '{1}'. Media type: '{2}'. Download date: '{3}'.";

    public const string MediaItemDeletedFromListFileNotFound = "Media item deleted from list. File deletion was requested, but the downloaded file was not found. Url: '{0}'. Title: '{1}'. Media type: '{2}'. Download date: '{3}'. File path: '{4}'.";

    public const string MediaItemDeleteFileFailed = "Media item file deletion failed. The item was kept in the list. Url: '{0}'. Title: '{1}'. Media type: '{2}'. Download date: '{3}'. File path: '{4}'. {5}";

    public const string MediaItemsDeleteBulkStarted = "Bulk media item deletion started. Items requested: {0}. File deletion requested: {1}.";

    public const string MediaItemsDeletedBulk = "Media items deleted in bulk. Removed from list: {0}. Deleted from file system: {1}. Failed file deletions: {2}. File deletion requested: {3}.";
}
