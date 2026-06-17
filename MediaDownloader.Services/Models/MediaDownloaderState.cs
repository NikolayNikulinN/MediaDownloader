using MediaDownloader.Services.Constants;
using System.Xml.Serialization;

namespace MediaDownloader.Services.Models;

[XmlRoot(ServiceConstants.Xml.RootName)]
public sealed class MediaDownloaderState
{
    public string SaveFolderPath { get; set; } = string.Empty;

    [XmlArray(ServiceConstants.Xml.UrlItemsArrayName)]
    [XmlArrayItem(ServiceConstants.Xml.UrlItemName)]
    public List<UrlMediaItem> UrlItems { get; set; } = [];
}
