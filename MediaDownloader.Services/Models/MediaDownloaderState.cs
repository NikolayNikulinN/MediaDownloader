using System.Xml.Serialization;

namespace MediaDownloader.Services.Models;

[XmlRoot("MediaDownloaderState")]
public sealed class MediaDownloaderState
{
    public string SaveFolderPath { get; set; } = string.Empty;

    [XmlArray("UrlItems")]
    [XmlArrayItem("UrlItem")]
    public List<UrlMediaItem> UrlItems { get; set; } = [];
}
