using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using System.Globalization;

namespace MediaDownloader.Services.Download;

internal static class MediaDownloadOutputParser
{
    public static int ParsePercentage(string line)
    {
        var value = line[ServiceConstants.Download.ProgressPrefix.Length..]
                                                                            .Replace(
                                                                                     ServiceConstants.Download.PercentSymbol,
                                                                                     string.Empty,
                                                                                     StringComparison.Ordinal)
                                                                            .Trim();

        return double.TryParse(
                                value,
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out var percentage)
                                ? Math.Clamp((int)Math.Round(percentage), 0, 100)
                                : 0;
    }

    public static string ResolveFinalFilePath(string filePath, string outputFolder)
    {
        var normalizedFilePath = filePath.Trim().Trim('"');
        return Path.IsPathRooted(normalizedFilePath)
                                            ? normalizedFilePath
                                            : Path.Combine(outputFolder, normalizedFilePath);
    }

    public static string FindCompletedFilePath(
                                                MediaDownloadRequest request,
                                                DateTime processStartUtc)
    {
        var extension = request.IsAudioOnly
                                  ? ServiceConstants.Download.Mp3FileExtension
                                  : ServiceConstants.Download.Mp4FileExtension;
        var earliestWriteTimeUtc = processStartUtc.AddSeconds(-ServiceConstants.Download.FileSearchClockSkewSeconds);
        var candidateFiles = Directory.EnumerateFiles(request.OutputFolder, $"*{extension}", SearchOption.TopDirectoryOnly)
                                      .Where(filePath => File.GetLastWriteTimeUtc(filePath) >= earliestWriteTimeUtc);

        var videoId = TryGetYouTubeVideoId(request.Url);
        if (!string.IsNullOrWhiteSpace(videoId))
        {
            var idMarker = string.Format(
                                         CultureInfo.InvariantCulture,
                                         ServiceConstants.Download.YouTubeVideoIdFileNameFormat,
                                         videoId);
            candidateFiles = candidateFiles.Where(filePath => Path.GetFileName(filePath).Contains(
                                                                                               idMarker,
                                                                                               StringComparison.OrdinalIgnoreCase));
        }

        return candidateFiles.OrderByDescending(File.GetLastWriteTimeUtc)
                             .FirstOrDefault() ?? string.Empty;
    }

    public static string TryGetYouTubeVideoId(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        var query = uri.Query.TrimStart('?');
        foreach (var parameter in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = parameter.Split('=', 2);
            if (parts.Length == 2
                && string.Equals(
                                 Uri.UnescapeDataString(parts[0]),
                                 ServiceConstants.Download.YouTubeVideoIdQueryParameter,
                                 StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return string.Empty;
    }
}
