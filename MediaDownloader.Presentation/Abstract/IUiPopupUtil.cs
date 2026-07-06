using MediaDownloader.Presentation.Models;

namespace MediaDownloader.Presentation.Abstract;

/// <summary>
/// Displays application popup messages.
/// </summary>
public interface IUiPopupUtil
{
    /// <summary>
    /// Displays an informational message.
    /// </summary>
    /// <param name="message">Message text.</param>
    void ShowMessage(string message);

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    /// <param name="message">Message text.</param>
    void ShowWarning(string message);

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// <param name="message">Message text.</param>
    void ShowError(string message);

    /// <summary>
    /// Asks the user to confirm downloading the specified missing tools.
    /// </summary>
    /// <param name="missingTools">Missing tool file names.</param>
    /// <returns><see langword="true"/> only when the user explicitly confirms.</returns>
    bool ConfirmToolDownload(IReadOnlyCollection<string> missingTools);

    /// <summary>
    /// Asks the user to confirm updating the specified tools.
    /// </summary>
    /// <param name="updates">Formatted update descriptions.</param>
    /// <returns><see langword="true"/> only when the user explicitly confirms.</returns>
    bool ConfirmToolUpdate(IReadOnlyCollection<string> updates);

    /// <summary>
    /// Asks the user to confirm stopping an active media download.
    /// </summary>
    /// <returns><see langword="true"/> only when the user explicitly confirms.</returns>
    bool ConfirmStopDownload();

    /// <summary>
    /// Asks the user to confirm deleting a media item.
    /// </summary>
    /// <param name="canDeleteDownloadedFile">Value indicating whether the downloaded file deletion option should be shown.</param>
    /// <returns>User confirmation details.</returns>
    DeleteMediaItemConfirmationResult ConfirmMediaItemDelete(bool canDeleteDownloadedFile);

    /// <summary>
    /// Asks the user to confirm deleting all media items.
    /// </summary>
    /// <param name="canDeleteDownloadedFile">Value indicating whether the downloaded file deletion option should be shown.</param>
    /// <returns>User confirmation details.</returns>
    DeleteMediaItemConfirmationResult ConfirmAllMediaItemsDelete(bool canDeleteDownloadedFile);

    /// <summary>
    /// Displays a progress dialog while required tools are downloaded.
    /// </summary>
    /// <param name="downloadOperation">Download operation that reports the current file name.</param>
    Task ShowToolDownloadProgressAsync(Func<IProgress<string>, Task> downloadOperation);

    /// <summary>
    /// Displays a progress dialog while installed tools are updated.
    /// </summary>
    /// <param name="updateOperation">Update operation that reports the current file name.</param>
    Task ShowToolUpdateProgressAsync(Func<IProgress<string>, Task> updateOperation);
}
