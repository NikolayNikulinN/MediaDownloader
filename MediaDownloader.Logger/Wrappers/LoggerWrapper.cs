using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Configuration;
using MediaDownloader.Logger.Enums;
using Serilog;

namespace MediaDownloader.Logger.Wrappers;

/// <summary>
/// Adapts Serilog to the application logging abstraction.
/// </summary>
public sealed class LoggerWrapper : ILoggerWrapper
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
    /// </summary>
    public LoggerWrapper()
        : this(FileLoggerConfigurator.GetLogger())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerWrapper"/> class.
    /// </summary>
    /// <param name="logger">Serilog logger.</param>
    public LoggerWrapper(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger.ForContext<LoggerWrapper>();
    }

    /// <inheritdoc />
    public void Write(string message) =>
        logger.Information(message);

    /// <inheritdoc />
    public void Write(string message, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Info:
                logger.Information(message);
                break;

            case LogLevel.Warning:
                logger.Warning(message);
                break;

            case LogLevel.Error:
                logger.Error(message);
                break;
        }
    }

    /// <inheritdoc />
    public void CloseAndFlush() =>
        FileLoggerConfigurator.CloseAndFlush();
}
