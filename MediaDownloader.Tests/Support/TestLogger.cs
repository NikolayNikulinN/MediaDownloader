using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Enums;

namespace MediaDownloader.Tests.Support;

internal sealed class TestLogger : ILoggerWrapper
{
    public List<(string Message, LogLevel Level)> Entries { get; } = [];

    public void Write(string message) => Entries.Add((message, LogLevel.Info));

    public void Write(string message, LogLevel level) => Entries.Add((message, level));

    public void CloseAndFlush()
    {
    }
}
