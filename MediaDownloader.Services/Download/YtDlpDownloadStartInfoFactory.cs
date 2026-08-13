using MediaDownloader.Services.Constants;
using MediaDownloader.Services.Models;
using MediaDownloader.Services.Utils;
using System.Diagnostics;
using System.Globalization;

namespace MediaDownloader.Services.Download;

internal static class YtDlpDownloadStartInfoFactory
{
    public static ProcessStartInfo Create(
                                          string ytDlpPath,
                                          MediaDownloadRequest request,
                                          string temporaryDirectoryPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        AddCommonArguments(
                            startInfo,
                            request,
                            temporaryDirectoryPath);

        if (request.IsAudioOnly)
        {
            AddAudioArguments(startInfo);
        }
        else
        {
            AddVideoArguments(startInfo, request.VideoQuality);
        }

        startInfo.ArgumentList.Add(request.Url);
        return startInfo;
    }

    private static void AddCommonArguments(
                                            ProcessStartInfo startInfo,
                                            MediaDownloadRequest request,
                                            string temporaryDirectoryPath)
    {
        startInfo.ArgumentList.Add(ServiceConstants.YtDlp.NoPlaylistArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.IgnoreConfigArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoSimulateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoUpdateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NoColorArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.NewLineArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.WindowsFileNamesArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressTemplateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ProgressTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PostProcessTemplateArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PostProcessTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PrintArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AfterMoveFilePathTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.FfmpegLocationArgument);
        startInfo.ArgumentList.Add(ToolPathResolver.ToolsDirectoryPath);
        startInfo.ArgumentList.Add(ServiceConstants.Download.PathsArgument);
        startInfo.ArgumentList.Add($"{ServiceConstants.Download.TempPathPrefix}{temporaryDirectoryPath}");
        startInfo.ArgumentList.Add(ServiceConstants.Download.PathsArgument);
        startInfo.ArgumentList.Add($"{ServiceConstants.Download.HomePathPrefix}{request.OutputFolder}");
        startInfo.ArgumentList.Add(ServiceConstants.Download.OutputArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.OutputTemplate);
        startInfo.ArgumentList.Add(ServiceConstants.Download.ForceOverwriteArgument);
    }

    private static void AddAudioArguments(ProcessStartInfo startInfo)
    {
        startInfo.ArgumentList.Add(ServiceConstants.Download.ExtractAudioArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AudioFormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp3Format);
        startInfo.ArgumentList.Add(ServiceConstants.Download.AudioQualityArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.BestAudioQuality);
        startInfo.ArgumentList.Add(ServiceConstants.Download.FormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.BestAudioFormatSelector);
    }

    private static void AddVideoArguments(ProcessStartInfo startInfo, string selectedQuality)
    {
        var height = ParseHeight(selectedQuality);
        var formatSelector = height > 0
                                        ? string.Format(
                                                        CultureInfo.InvariantCulture,
                                                        ServiceConstants.Download.VideoFormatSelector,
                                                        height)
                                        : ServiceConstants.Download.BestVideoFormatSelector;

        startInfo.ArgumentList.Add(ServiceConstants.Download.FormatArgument);
        startInfo.ArgumentList.Add(formatSelector);
        startInfo.ArgumentList.Add(ServiceConstants.Download.MergeOutputFormatArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp4Format);
        startInfo.ArgumentList.Add(ServiceConstants.Download.RemuxVideoArgument);
        startInfo.ArgumentList.Add(ServiceConstants.Download.Mp4Format);
    }

    private static int ParseHeight(string quality)
    {
        var numericValue = quality.TrimEnd('p', 'P');
        return int.TryParse(
                            numericValue,
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out var height)
                            ? height
                            : 0;
    }
}
