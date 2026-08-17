using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Persists application settings and media data in the Configuration folder.
/// </summary>
public sealed class XmlService : IXmlService
{
    private static readonly XmlSerializer SettingsSerializer = new(typeof(ApplicationSettings));
    private static readonly XmlSerializer DataSerializer = new(typeof(MediaDownloaderState));
    private readonly string dataFilePath;
    private readonly ILoggerWrapper loggerWrapper;
    private readonly string settingsFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlService"/> class.
    /// </summary>
    /// <param name="loggerWrapper">Application logger.</param>
    public XmlService(ILoggerWrapper loggerWrapper)
        : this(
            loggerWrapper,
            Path.Combine(AppContext.BaseDirectory, ServiceConstants.Xml.ConfigurationFolderName))
    {
    }

    internal XmlService(ILoggerWrapper loggerWrapper, string configurationDirectoryPath)
    {
        ArgumentNullException.ThrowIfNull(loggerWrapper);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationDirectoryPath);

        this.loggerWrapper = loggerWrapper;
        settingsFilePath = Path.Combine(configurationDirectoryPath,
        ServiceConstants.Xml.SettingsFileName);
        dataFilePath = Path.Combine(configurationDirectoryPath, ServiceConstants.Xml.DataFileName);
    }

    /// <inheritdoc />
    public ApplicationSettings LoadSettings()
    {
        if (!File.Exists(settingsFilePath))
        {
            var defaultSettings = CreateDefaultSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var settings = DeserializeXml<ApplicationSettings>(settingsFilePath, SettingsSerializer) ?? CreateDefaultSettings();

            if (string.IsNullOrWhiteSpace(settings.SaveFolderPath))
            {
                settings.SaveFolderPath = GetDefaultSaveFolderPath();
                SaveSettings(settings);
            }

            return settings;
        }
        catch (Exception exception)
        {
            LogLoadFailure(settingsFilePath, exception);
            var defaultSettings = CreateDefaultSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }
    }

    /// <inheritdoc />
    public void SaveSettings(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        SaveXml(settingsFilePath, settings, SettingsSerializer);
    }

    /// <inheritdoc />
    public MediaDownloaderState LoadData()
    {
        if (!File.Exists(dataFilePath))
        {
            var emptyState = new MediaDownloaderState();
            SaveData(emptyState);
            return emptyState;
        }

        try
        {
            return DeserializeXml<MediaDownloaderState>(dataFilePath, DataSerializer) ?? new MediaDownloaderState();
        }
        catch (Exception exception)
        {
            LogLoadFailure(dataFilePath, exception);
            var emptyState = new MediaDownloaderState();
            SaveData(emptyState);
            return emptyState;
        }
    }

    /// <inheritdoc />
    public void SaveData(MediaDownloaderState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        SaveXml(dataFilePath, state, DataSerializer);
    }

    private static ApplicationSettings CreateDefaultSettings() =>
                                                                    new()
                                                                    {
                                                                        SaveFolderPath = GetDefaultSaveFolderPath()
                                                                    };

    private static string GetDefaultSaveFolderPath() => Path.Combine(
                                                                     Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                                                     ServiceConstants.Xml.DefaultDownloadsFolderName);

    private static T? DeserializeXml<T>(string filePath, XmlSerializer serializer)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var stream = File.OpenRead(filePath);
        using var reader = XmlReader.Create(stream, settings);
        return serializer.Deserialize(reader) is T value ? value : default;
    }

    private void SaveXml(
                          string filePath,
                          object value,
                          XmlSerializer serializer)
    {
        try
        {
            var directoryPath = Path.GetDirectoryName(filePath)!;
            Directory.CreateDirectory(directoryPath);
            var temporaryFilePath = filePath + ServiceConstants.Xml.TemporaryFileExtension;

            try
            {
                using (var stream = File.Create(temporaryFilePath))
                {
                    serializer.Serialize(stream, value);
                }

                File.Move(temporaryFilePath, filePath, overwrite: true);
            }
            finally
            {
                File.Delete(temporaryFilePath);
            }
        }
        catch (Exception exception)
        {
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.XmlSaveFailed,
                                               filePath,
                                               exception.Message),
                LogLevel.Error);
            throw;
        }
    }

    private void LogLoadFailure(string filePath, Exception exception) => loggerWrapper.Write(string.Format(
                                                                                                            CultureInfo.InvariantCulture,
                                                                                                            LogConstants.XmlLoadFailed,
                                                                                                            filePath,
                                                                                                            exception.Message),
                                                                                              LogLevel.Error);
}
