using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using System.Globalization;
using System.Text.Json;

namespace MediaDownloader.Services.Parsing;

internal static class YtDlpMediaInfoParser
{
    public static UrlMediaItem Parse(string url, string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var qualities = GetVideoQualities(root);

        return new UrlMediaItem
        {
            Url = url,
            Title = GetString(root, ServiceConstants.Json.Title),
            Author = GetFirstString(root, ServiceConstants.Json.Uploader, ServiceConstants.Json.Channel, ServiceConstants.Json.Creator),
            ThumbnailUrl = GetString(root, ServiceConstants.Json.Thumbnail),
            Duration = FormatDuration(GetDouble(root, ServiceConstants.Json.Duration)),
            UploadDate = FormatUploadDate(GetFirstString(root, ServiceConstants.Json.UploadDate, ServiceConstants.Json.ReleaseDate)),
            SelectedMediaType = ServiceConstants.Media.Mp4,
            SelectedVideoQuality = qualities.FirstOrDefault()?.Name ?? string.Empty,
            AvailableVideoQualities = qualities
        };
    }

    public static List<VideoQualityOption> GetVideoQualities(JsonElement root)
    {
        if (!root.TryGetProperty(ServiceConstants.Json.Formats, out var formats) || formats.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var heights = new SortedSet<int>(Comparer<int>.Create((left, right) => right.CompareTo(left)));

        foreach (var format in formats.EnumerateArray())
        {
            var videoCodec = GetString(format, ServiceConstants.Json.VideoCodec);

            if (string.Equals(videoCodec, ServiceConstants.Json.NoCodec, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var height = GetInt(format, ServiceConstants.Json.Height);

            if (height > 0)
            {
                heights.Add(height);
            }
        }

        return heights
            .Select((height, index) => new VideoQualityOption
            {
                Name = string.Format(CultureInfo.InvariantCulture, ServiceConstants.Media.VideoQualityNameFormat, height),
                IsSelected = index == 0
            })
            .ToList();
    }

    public static string FormatDuration(double totalSeconds)
    {
        if (totalSeconds <= 0)
        {
            return string.Empty;
        }

        var duration = TimeSpan.FromSeconds(totalSeconds);
        return duration.TotalHours >= 1
            ? duration.ToString(ServiceConstants.DateTimeFormats.DurationWithHours, CultureInfo.InvariantCulture)
            : duration.ToString(ServiceConstants.DateTimeFormats.DurationWithoutHours, CultureInfo.InvariantCulture);
    }

    public static string FormatUploadDate(string uploadDate)
    {
        return DateTime.TryParseExact(uploadDate, ServiceConstants.DateTimeFormats.YtDlpUploadDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString(ServiceConstants.DateTimeFormats.DisplayDate, CultureInfo.InvariantCulture)
            : uploadDate;
    }

    private static string GetFirstString(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetString(element, propertyName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var result)
            ? result
            : 0;
    }

    private static double GetDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.TryGetDouble(out var result)
            ? result
            : 0;
    }
}
