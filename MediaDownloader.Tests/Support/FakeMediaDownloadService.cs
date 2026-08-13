using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;

namespace MediaDownloader.Tests.Support;

internal sealed class FakeMediaDownloadService : IMediaDownloadService
{
    public Queue<Func<MediaDownloadRequest, IProgress<MediaDownloadProgress>?, CancellationToken, Task<MediaDownloadResult>>> Results { get; } = [];

    public List<MediaDownloadRequest> Requests { get; } = [];

    public Task<MediaDownloadResult> DownloadAsync(
        MediaDownloadRequest request,
        IProgress<MediaDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Requests.Add(request);

        if (Results.Count == 0)
        {
            throw new InvalidOperationException(TestConstants.Support.NoFakeDownloadResultConfigured);
        }

        return Results.Dequeue()(request, progress, cancellationToken);
    }
}
