using MediaDownloader.Presentation.Abstract;
using System.Diagnostics;
using System.IO;

namespace MediaDownloader.Presentation.Services;

/// <summary>
/// Opens folders through the Windows shell.
/// </summary>
public sealed class ShellUtil : IShellUtil
{
    /// <inheritdoc />
    public void OpenFolder(string folderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

        Directory.CreateDirectory(folderPath);
        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }
}
