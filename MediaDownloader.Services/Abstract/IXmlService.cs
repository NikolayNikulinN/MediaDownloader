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
}
