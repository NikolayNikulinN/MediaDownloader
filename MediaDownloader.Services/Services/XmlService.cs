using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using System.Xml.Serialization;

namespace MediaDownloader.Services.Services;

/// <summary>
/// Persists application settings in Configuration\settings.xml.
/// </summary>
public sealed class XmlService : IXmlService
{
    private static readonly XmlSerializer Serializer =
        new(typeof(ApplicationSettings));
    private readonly ILoggerWrapper loggerWrapper;
    private readonly string settingsFilePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlService"/> class.
    /// </summary>
    /// <param name="loggerWrapper">Application logger.</param>
    public XmlService(ILoggerWrapper loggerWrapper)
    {
        ArgumentNullException.ThrowIfNull(loggerWrapper);

        this.loggerWrapper = loggerWrapper;
        settingsFilePath = Path.Combine(
                                        AppContext.BaseDirectory,
                                        ServiceConstants.Xml.ConfigurationFolderName,
                                        ServiceConstants.Xml.SettingsFileName);
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
            using var stream = File.OpenRead(settingsFilePath);
            var settings = Serializer.Deserialize(stream) as ApplicationSettings ?? CreateDefaultSettings();
            if (string.IsNullOrWhiteSpace(settings.SaveFolderPath))
            {
                settings.SaveFolderPath = GetDefaultSaveFolderPath();
                SaveSettings(settings);
            }

            return settings;
        }
        catch (Exception exception)
        {
            loggerWrapper.Write(string.Format(
                                               LogConstants.XmlLoadFailed,
                                               settingsFilePath,
                                               exception.Message),
                                LogLevel.Error);

            var defaultSettings = CreateDefaultSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }
    }

    /// <inheritdoc />
    public void SaveSettings(ApplicationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var directoryPath = Path.GetDirectoryName(settingsFilePath)!;
            Directory.CreateDirectory(directoryPath);

            var temporaryFilePath = settingsFilePath + ServiceConstants.Xml.TemporaryFileExtension;

            try
            {
                using (var stream = File.Create(temporaryFilePath))
                {
                    Serializer.Serialize(stream, settings);
                }

                File.Move(temporaryFilePath, settingsFilePath, overwrite: true);
            }
            finally
            {
                File.Delete(temporaryFilePath);
            }
        }
        catch (Exception exception)
        {
            loggerWrapper.Write(string.Format(
                                               LogConstants.XmlSaveFailed,
                                               settingsFilePath,
                                               exception.Message),
                                LogLevel.Error);
            throw;
        }
    }

    private static ApplicationSettings CreateDefaultSettings() =>
                                                                   new()
                                                                   {
                                                                      SaveFolderPath = GetDefaultSaveFolderPath()
                                                                   };

    private static string GetDefaultSaveFolderPath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ServiceConstants.Xml.DefaultDownloadsFolderName);
}
