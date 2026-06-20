namespace MediaDownloader.Presentation.Abstract;

/// <summary>
/// Provides folder selection dialogs for the user interface.
/// </summary>
public interface IFolderDialogUtil
{
    /// <summary>
    /// Shows a folder selection dialog.
    /// </summary>
    /// <param name="title">Dialog title.</param>
    /// <param name="initialDirectory">Directory selected when the dialog opens.</param>
    /// <returns>The selected folder path, or <see langword="null"/> when cancelled.</returns>
    string? SelectFolder(string title, string initialDirectory);
}
