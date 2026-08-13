using MediaDownloader.Presentation.Abstract;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeFolderDialogUtil : IFolderDialogUtil
{
    public string? SelectedFolder { get; set; }

    public List<(string Title, string InitialDirectory)> Calls { get; } = [];

    public string? SelectFolder(string title, string initialDirectory)
    {
        Calls.Add((title, initialDirectory));
        return SelectedFolder;
    }
}
