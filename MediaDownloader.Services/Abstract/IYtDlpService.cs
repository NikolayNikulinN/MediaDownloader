using MediaDownloader.Services.Models;

namespace MediaDownloader.Services.Abstract;

public interface IYtDlpService
{
    Task<UrlMediaItem> GetMediaInfoAsync(string url, CancellationToken cancellationToken = default);
}
