using MediaDownloader.Presentation.Abstract;
using Microsoft.Win32;

namespace MediaDownloader.Presentation.Services;

/// <summary>
/// Shows the Windows folder selection dialog.
/// </summary>
public sealed class FolderDialogUtil : IFolderDialogUtil
{
    /// <inheritdoc />
    public string? SelectFolder(string title, string initialDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(initialDirectory);

        var dialog = new OpenFolderDialog
        {
            Title = title,
            InitialDirectory = initialDirectory
        };

        return dialog.ShowDialog() == true
            ? dialog.FolderName
            : null;
    }
}
