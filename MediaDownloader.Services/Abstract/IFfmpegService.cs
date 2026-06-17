namespace MediaDownloader.Services.Abstract;

public interface IFfmpegService
{
    string FfmpegPath { get; }

    string FfprobePath { get; }

    bool AreToolsAvailable();

    Task<string> GetFfmpegVersionAsync(CancellationToken cancellationToken = default);

    Task<string> GetFfprobeVersionAsync(CancellationToken cancellationToken = default);
}
