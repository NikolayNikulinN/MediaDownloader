using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Enums;
using Serilog;

namespace MediaDownloader.Logger;

public sealed class LoggerWrapper : ILoggerWrapper
{
    private readonly ILogger logger;

    public LoggerWrapper()
        : this(FileLoggerConfigurator.GetLogger())
    {
    }

    public LoggerWrapper(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger.ForContext<LoggerWrapper>();
    }

    public void Write(string message)
    {
        logger.Information(message);
    }

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
}
