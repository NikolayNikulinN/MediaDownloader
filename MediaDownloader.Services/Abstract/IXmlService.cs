using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Loads and saves application settings in XML format.
/// </summary>
public interface IXmlService
{
    /// <summary>
    /// Loads settings from Configuration\settings.xml.
    /// Creates the file with default settings when it does not exist.
    /// </summary>
    /// <returns>Application settings.</returns>
    ApplicationSettings LoadSettings();

    /// <summary>
    /// Saves settings to Configuration\settings.xml.
    /// </summary>
    /// <param name="settings">Settings to save.</param>
    void SaveSettings(ApplicationSettings settings);

    /// <summary>
    /// Loads media data from Configuration\data.xml.
    /// Creates an empty data file when it does not exist.
    /// </summary>
    /// <returns>Persisted media data.</returns>
    MediaDownloaderState LoadData();

    /// <summary>
    /// Saves media data to Configuration\data.xml.
    /// </summary>
    /// <param name="state">Media data to save.</param>
    void SaveData(MediaDownloaderState state);
}
