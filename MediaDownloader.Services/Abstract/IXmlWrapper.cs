using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

/// <summary>
/// Saves and loads application state in XML format.
/// </summary>
public interface IXmlWrapper
{
    /// <summary>
    /// Saves application state.
    /// </summary>
    /// <param name="filePath">Destination XML file path.</param>
    /// <param name="state">State to save.</param>
    void Save(string filePath, MediaDownloaderState state);

    /// <summary>
    /// Loads application state.
    /// </summary>
    /// <param name="filePath">Source XML file path.</param>
    /// <returns>Loaded state, or an empty state when the file does not exist.</returns>
    MediaDownloaderState Load(string filePath);
}
