using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeToolUpdateService : IToolUpdateService
{
    public IReadOnlyList<ToolUpdateInfo> Updates { get; set; } = [];

    public Exception? Exception { get; set; }

    public int Calls { get; private set; }

    public Task<IReadOnlyList<ToolUpdateInfo>> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        Calls++;

        return Exception is null
            ? Task.FromResult(Updates)
            : Task.FromException<IReadOnlyList<ToolUpdateInfo>>(Exception);
    }
}
