using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using System.Xml.Serialization;

namespace MediaDownloader.Services;

public sealed class XmlWrapper : IXmlWrapper
{
    private static readonly XmlSerializer Serializer = new(typeof(MediaDownloaderState));
    private readonly ILoggerWrapper loggerWrapper;

    public XmlWrapper(ILoggerWrapper loggerWrapper)
    {
        ArgumentNullException.ThrowIfNull(loggerWrapper);
        this.loggerWrapper = loggerWrapper;
    }

    public void Save(string filePath, MediaDownloaderState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(state);

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using var stream = File.Create(filePath);
            Serializer.Serialize(stream, state);
        }
        catch (Exception exception)
        {
            loggerWrapper.Write(string.Format(LogConstants.XmlSaveFailed, filePath, exception.Message), LogLevel.Error);
            throw;
        }
    }

    public MediaDownloaderState Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            return new MediaDownloaderState();
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            return Serializer.Deserialize(stream) as MediaDownloaderState
                   ?? new MediaDownloaderState();
        }
        catch (Exception exception)
        {
            loggerWrapper.Write(string.Format(LogConstants.XmlLoadFailed, filePath, exception.Message), LogLevel.Error);
            throw;
        }
    }
}
