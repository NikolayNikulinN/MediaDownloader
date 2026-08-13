using MediaDownloader.Presentation.Models;
using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;
using MediaDownloader.Tests.Support;
using System.IO;
using Xunit;

namespace MediaDownloader.Tests.ViewModels;

public sealed class HomePageViewModelTests
{
    [Fact]
    public async Task AddUrlCommand_adds_analyzed_media_and_saves_state()
    {
        var context = new ViewModelTestContext();
        context.YtDlpService.Results.Enqueue(url => TestModels.CreateMediaItem(url));
        var viewModel = context.CreateHomePageViewModel();
        viewModel.Url = TestConstants.VideoUrl;

        await viewModel.AddUrlCommand.ExecuteAsync(null);

        var item = Assert.Single(viewModel.UrlItems);
        Assert.Equal(TestConstants.VideoUrl, item.Url);
        Assert.Equal(TestConstants.VideoTitle, item.Title);
        Assert.Equal(string.Empty, viewModel.Url);
        Assert.Equal(string.Empty, viewModel.AnalysisStatus);
        Assert.False(viewModel.IsAnalyzing);
        Assert.Single(context.XmlService.SavedStates);
    }

    [Fact]
    public async Task AddUrlCommand_ignores_duplicate_urls_case_insensitively()
    {
        var existingItem = TestModels.CreateMediaItem(TestConstants.VideoUrl);
        var context = new ViewModelTestContext(existingItem);
        context.YtDlpService.Results.Enqueue(_ => TestModels.CreateMediaItem(TestConstants.VideoUrl));
        var viewModel = context.CreateHomePageViewModel();
        viewModel.Url = TestConstants.DuplicateVideoUrlUpper;

        await viewModel.AddUrlCommand.ExecuteAsync(null);

        Assert.Single(viewModel.UrlItems);
        Assert.Equal(string.Empty, viewModel.Url);
        Assert.Empty(context.XmlService.SavedStates);
    }

    [Fact]
    public async Task AddUrlCommand_reports_analysis_failure()
    {
        var context = new ViewModelTestContext();
        context.YtDlpService.Exceptions.Enqueue(new InvalidOperationException(TestConstants.AnalyzeFailureMessage));
        var viewModel = context.CreateHomePageViewModel();
        viewModel.Url = TestConstants.FailedVideoUrl;

        await viewModel.AddUrlCommand.ExecuteAsync(null);

        Assert.Equal(TestConstants.AnalyzeFailureMessage, viewModel.AnalysisStatus);
        Assert.Contains(context.UiPopup.Errors, message => message.Contains(TestConstants.AnalyzeFailureMessage, StringComparison.Ordinal));
        Assert.Contains(context.Logger.Entries, entry => entry.Level == MediaDownloader.Logger.Enums.LogLevel.Error);
        Assert.Empty(viewModel.UrlItems);
    }

    [Fact]
    public void BrowseOutputFolder_saves_selected_folder()
    {
        var context = new ViewModelTestContext();
        context.FolderDialog.SelectedFolder = TestConstants.SelectedFolder;
        var viewModel = context.CreateHomePageViewModel();

        viewModel.BrowseOutputFolderCommand.Execute(null);

        Assert.Equal(TestConstants.SelectedFolder, viewModel.OutputFolder);
        var savedSettings = Assert.Single(context.XmlService.SavedSettings);
        Assert.Equal(TestConstants.SelectedFolder, savedSettings.SaveFolderPath);
    }

    [Fact]
    public void Constructor_populates_common_bulk_video_qualities()
    {
        var firstItem = TestModels.CreateMediaItem(
            TestConstants.VideoUrl,
            TestConstants.VideoTitle,
            TestConstants.Media.Quality1080p,
            TestConstants.Media.Quality720p);
        var secondItem = TestModels.CreateMediaItem(
            TestConstants.SecondVideoUrl,
            TestConstants.SecondVideoTitle,
            TestConstants.Media.Quality720p,
            TestConstants.Media.Quality480p);
        var context = new ViewModelTestContext(firstItem, secondItem);

        var viewModel = context.CreateHomePageViewModel();

        var quality = Assert.Single(viewModel.BulkAvailableVideoQualities);
        Assert.Equal(TestConstants.Media.Quality720p, quality.Name);
        Assert.True(quality.IsSelected);
    }

    [Fact]
    public void BulkAudioSelection_updates_non_downloading_items_and_saves_state()
    {
        var firstItem = TestModels.CreateMediaItem(TestConstants.VideoUrl);
        var secondItem = TestModels.CreateMediaItem(TestConstants.SecondVideoUrl, TestConstants.SecondVideoTitle);
        var context = new ViewModelTestContext(firstItem, secondItem);
        var viewModel = context.CreateHomePageViewModel();
        viewModel.UrlItems[1].IsDownloading = true;

        viewModel.IsBulkAudioSelected = true;

        Assert.True(viewModel.UrlItems[0].IsAudioSelected);
        Assert.False(viewModel.UrlItems[1].IsAudioSelected);
        Assert.Single(context.XmlService.SavedStates);
    }

    [Fact]
    public void DeleteUrlItemCommand_deletes_downloaded_file_when_confirmed()
    {
        var downloadedFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mp4");
        File.WriteAllText(downloadedFilePath, TestConstants.VideoTitle);
        var item = TestModels.CreateMediaItem(TestConstants.VideoUrl);
        item.DownloadFilePath = downloadedFilePath;
        var context = new ViewModelTestContext(item);
        context.UiPopup.ConfirmMediaItemDeleteResult = new DeleteMediaItemConfirmationResult
        {
            IsConfirmed = true,
            DeleteDownloadedFile = true
        };
        var viewModel = context.CreateHomePageViewModel();

        try
        {
            viewModel.DeleteUrlItemCommand.Execute(viewModel.UrlItems[0]);

            Assert.Empty(viewModel.UrlItems);
            Assert.False(File.Exists(downloadedFilePath));
            Assert.Single(context.XmlService.SavedStates);
        }
        finally
        {
            File.Delete(downloadedFilePath);
        }
    }

    [Fact]
    public async Task ToggleDownloadCommand_downloads_media_and_updates_item_state()
    {
        var context = new ViewModelTestContext(TestModels.CreateMediaItem(TestConstants.VideoUrl));
        context.MediaDownloadService.Results.Enqueue((_, _, _) =>
            Task.FromResult(new MediaDownloadResult { FilePath = TestConstants.DownloadPath }));
        var viewModel = context.CreateHomePageViewModel();
        var item = viewModel.UrlItems[0];

        viewModel.ToggleDownloadCommand.Execute(item);
        await AsyncAssert.WaitUntilAsync(() => !item.IsDownloading && context.MediaDownloadService.Requests.Count == 1);

        Assert.Equal(100, item.DownloadProgress);
        Assert.Equal(context.ResourceWrapper.DownloadCompletedStatus, item.DownloadStatus);
        Assert.Equal(TestConstants.DownloadPath, item.DownloadFilePath);
        Assert.Equal(TestConstants.VideoUrl, context.MediaDownloadService.Requests[0].Url);
        Assert.Equal(TestConstants.OutputFolder, context.MediaDownloadService.Requests[0].OutputFolder);
    }

    [Fact]
    public async Task ToggleDownloadCommand_cancels_active_download_when_user_confirms_stop()
    {
        var context = new ViewModelTestContext(TestModels.CreateMediaItem(TestConstants.VideoUrl));
        context.MediaDownloadService.Results.Enqueue(async (_, _, cancellationToken) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return new MediaDownloadResult { FilePath = TestConstants.DownloadPath };
        });
        var viewModel = context.CreateHomePageViewModel();
        var item = viewModel.UrlItems[0];

        viewModel.ToggleDownloadCommand.Execute(item);
        await AsyncAssert.WaitUntilAsync(() => item.IsDownloading);
        viewModel.ToggleDownloadCommand.Execute(item);
        await AsyncAssert.WaitUntilAsync(() => !item.IsDownloading);

        Assert.Equal(0, item.DownloadProgress);
        Assert.Equal(context.ResourceWrapper.DownloadCancelledStatus, item.DownloadStatus);
        Assert.Equal(string.Empty, item.DownloadFilePath);
    }
}
