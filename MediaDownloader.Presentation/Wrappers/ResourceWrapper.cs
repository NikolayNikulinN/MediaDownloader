using System.Globalization;
using System.Resources;

namespace MediaDownloader.Presentation.Resources
{
    public sealed class ResourceWrapper
    {
        private readonly ResourceManager resourceManager = AppResource.ResourceManager;

        public string AppTitle => GetString(nameof(AppTitle));

        public string MinimizeToolTip => GetString(nameof(MinimizeToolTip));

        public string MaximizeToolTip => GetString(nameof(MaximizeToolTip));

        public string RestoreToolTip => GetString(nameof(RestoreToolTip));

        public string CloseToolTip => GetString(nameof(CloseToolTip));

        public string AnalyzeUrlButtonText => GetString(nameof(AnalyzeUrlButtonText));

        public string YouTubeUrlToolTip => GetString(nameof(YouTubeUrlToolTip));

        public string UrlPlaceholder => GetString(nameof(UrlPlaceholder));

        public string SaveToLabel => GetString(nameof(SaveToLabel));

        public string DownloadFolderToolTip => GetString(nameof(DownloadFolderToolTip));

        public string BrowseButtonText => GetString(nameof(BrowseButtonText));

        public string OpenFolderToolTip => GetString(nameof(OpenFolderToolTip));

        public string DownloadToolTip => GetString(nameof(DownloadToolTip));

        public string MediaModeToolTip => GetString(nameof(MediaModeToolTip));

        public string Mp4Label => GetString(nameof(Mp4Label));

        public string Mp3Label => GetString(nameof(Mp3Label));

        public string SelectDownloadFolderTitle => GetString(nameof(SelectDownloadFolderTitle));

        public string AnalyzingUrlStatus => GetString(nameof(AnalyzingUrlStatus));

        public string ReadyStatus => GetString(nameof(ReadyStatus));

        public string NoDataText => GetString(nameof(NoDataText));

        public string this[string key] => GetString(key);

        private string GetString(string key)
        {
            return resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
    }
}
