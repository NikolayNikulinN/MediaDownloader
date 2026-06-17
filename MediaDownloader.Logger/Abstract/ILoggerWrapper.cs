using MediaDownloader.Logger.Enums;

namespace MediaDownloader.Logger.Abstract;

public interface ILoggerWrapper
{
    void Write(string message);

    void Write(string message, LogLevel level);
}
