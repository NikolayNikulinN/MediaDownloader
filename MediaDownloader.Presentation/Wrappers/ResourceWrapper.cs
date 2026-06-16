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

        public string this[string key] => GetString(key);

        private string GetString(string key)
        {
            return resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
        }
    }
}
