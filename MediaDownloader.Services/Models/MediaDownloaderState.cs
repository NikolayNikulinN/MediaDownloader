using MediaDownloader.Services.Constants;
using System.Xml.Serialization;

namespace MediaDownloader.Services.Models;

/// <summary>
/// Represents application state persisted between sessions.
/// </summary>
[XmlRoot(ServiceConstants.Xml.RootName)]
public sealed class MediaDownloaderState
{
    public string SaveFolderPath { get; set; } = string.Empty;

    [XmlArray(ServiceConstants.Xml.UrlItemsArrayName)]
    [XmlArrayItem(ServiceConstants.Xml.UrlItemName)]
    public List<UrlMediaItem> UrlItems { get; set; } = [];
}
