using MediaDownloader.Logger.Enums;

namespace MediaDownloader.Logger.Abstract;

/// <summary>
/// Writes application log events.
/// </summary>
public interface ILoggerWrapper
{
    /// <summary>
    /// Writes an informational message.
    /// </summary>
    /// <param name="message">Message to write.</param>
    void Write(string message);

    /// <summary>
    /// Writes a message at the specified log level.
    /// </summary>
    /// <param name="message">Message to write.</param>
    /// <param name="level">Log event level.</param>
    void Write(string message, LogLevel level);

    /// <summary>
    /// Flushes pending log events and closes the logger.
    /// </summary>
    void CloseAndFlush();
}
