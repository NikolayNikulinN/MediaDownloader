using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

public interface IXmlWrapper
{
    void Save(string filePath, MediaDownloaderState state);

    MediaDownloaderState Load(string filePath);
}
