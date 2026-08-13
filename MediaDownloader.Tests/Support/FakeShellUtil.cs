using MediaDownloader.Presentation.Abstract;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeShellUtil : IShellUtil
{
    public List<string> OpenedFolders { get; } = [];

    public void OpenFolder(string folderPath) => OpenedFolders.Add(folderPath);
}
