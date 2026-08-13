using MediaDownloader.Services.Abstract;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeToolBootstrapService : IToolBootstrapService
{
    public string ToolsDirectoryPath { get; set; } = string.Empty;

    public IReadOnlyList<string> MissingTools { get; set; } = [];

    public List<IReadOnlyCollection<string>> DownloadRequests { get; } = [];

    public IReadOnlyList<string> GetMissingTools() => MissingTools;

    public Task DownloadMissingToolsAsync(
        IReadOnlyCollection<string> missingTools,
        bool userConfirmed,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        DownloadRequests.Add(missingTools);
        return Task.CompletedTask;
    }
}
