using MediaDownloader.Services.Constants;

namespace MediaDownloader.Services.Utils;

internal static class ToolPathResolver
{
    public static string ToolsDirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ServiceConstants.Tools.HttpUserAgentProductName, ServiceConstants.Tools.FolderName);

    public static string Resolve(string fileName)
    {
        var updatedPath = Path.Combine(ToolsDirectoryPath, fileName);
        var bundledPath = Path.Combine(AppContext.BaseDirectory, ServiceConstants.Tools.FolderName, fileName);
        return File.Exists(updatedPath) || !File.Exists(bundledPath) ? updatedPath : bundledPath;
    }
}
