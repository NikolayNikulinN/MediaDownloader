using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeYtDlpService : IYtDlpService
{
    public Queue<Func<string, UrlMediaItem>> Results { get; } = [];

    public Queue<Exception> Exceptions { get; } = [];

    public List<string> RequestedUrls { get; } = [];

    public Task<UrlMediaItem> GetMediaInfoAsync(string url, CancellationToken cancellationToken = default)
    {
        RequestedUrls.Add(url);

        if (Exceptions.Count > 0)
        {
            return Task.FromException<UrlMediaItem>(Exceptions.Dequeue());
        }

        if (Results.Count > 0)
        {
            return Task.FromResult(Results.Dequeue()(url));
        }

        throw new InvalidOperationException(TestConstants.Support.NoFakeYtDlpResultConfigured);
    }
}
