using MediaDownloader.Services.Constants;

namespace MediaDownloader.Services.Utils;

internal static class ToolPathResolver
{
    public static string ToolsDirectoryPath { get; } = Path.Combine(AppContext.BaseDirectory, ServiceConstants.Tools.FolderName);

    public static string Resolve(string fileName) => Path.Combine(ToolsDirectoryPath, fileName);
}
