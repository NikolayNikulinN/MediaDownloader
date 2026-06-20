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
}
