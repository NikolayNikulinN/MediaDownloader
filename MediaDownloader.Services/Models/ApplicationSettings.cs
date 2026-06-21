using MediaDownloader.Services.Constants;
using System.Xml.Serialization;

namespace MediaDownloader.Services.Models;

/// <summary>
/// Represents user-configurable application settings.
/// </summary>
[XmlRoot(ServiceConstants.Xml.SettingsRootName)]
public sealed class ApplicationSettings
{
    public string SaveFolderPath { get; set; } = string.Empty;
}
