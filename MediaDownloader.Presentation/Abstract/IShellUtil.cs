namespace MediaDownloader.Presentation.Abstract;

/// <summary>
/// Provides operating-system shell operations used by the user interface.
/// </summary>
public interface IShellUtil
{
    /// <summary>
    /// Creates the folder when needed and opens it in the system file manager.
    /// </summary>
    /// <param name="folderPath">Folder path to open.</param>
    void OpenFolder(string folderPath);
}
