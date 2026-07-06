namespace MediaDownloader.Presentation.Models;

/// <summary>
/// Contains the user decision from the media item deletion confirmation dialog.
/// </summary>
public sealed class DeleteMediaItemConfirmationResult
{
    /// <summary>
    /// Gets a value indicating whether the user confirmed media item deletion.
    /// </summary>
    public bool IsConfirmed { get; init; }

    /// <summary>
    /// Gets a value indicating whether the downloaded file should be deleted from the file system.
    /// </summary>
    public bool DeleteDownloadedFile { get; init; }
}
