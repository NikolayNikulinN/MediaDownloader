using MediaDownloader.Services.Models;
using MediaDownloader.Tests.Constants;
using MediaDownloader.Tests.Support;
using Xunit;

namespace MediaDownloader.Tests.ViewModels;

public sealed class MainPageViewModelTests
{
    [Fact]
    public void CanCloseApplication_tracks_home_page_active_operation()
    {
        var context = new ViewModelTestContext(TestModels.CreateMediaItem(TestConstants.VideoUrl));
        var homePageViewModel = context.CreateHomePageViewModel();
        var mainPageViewModel = context.CreateMainPageViewModel(homePageViewModel);

        homePageViewModel.UrlItems[0].IsDownloading = true;

        Assert.False(mainPageViewModel.CanCloseApplication);
        Assert.Equal(context.ResourceWrapper.CloseBlockedToolTip, mainPageViewModel.CloseButtonToolTip);

        homePageViewModel.UrlItems[0].IsDownloading = false;

        Assert.True(mainPageViewModel.CanCloseApplication);
        Assert.Equal(context.ResourceWrapper.CloseToolTip, mainPageViewModel.CloseButtonToolTip);
    }

    [Fact]
    public void UpdateMaximizeRestoreState_updates_icon_and_tooltip()
    {
        var context = new ViewModelTestContext();
        var mainPageViewModel = context.CreateMainPageViewModel(context.CreateHomePageViewModel());

        mainPageViewModel.UpdateMaximizeRestoreState(isMaximized: true);

        Assert.Equal(context.AppConstants.Images.RestorePath, mainPageViewModel.MaximizeRestoreIconPath);
        Assert.Equal(context.ResourceWrapper.RestoreToolTip, mainPageViewModel.MaximizeRestoreToolTip);

        mainPageViewModel.UpdateMaximizeRestoreState(isMaximized: false);

        Assert.Equal(context.AppConstants.Images.MaximizePath, mainPageViewModel.MaximizeRestoreIconPath);
        Assert.Equal(context.ResourceWrapper.MaximizeToolTip, mainPageViewModel.MaximizeRestoreToolTip);
    }

    [Fact]
    public async Task CheckLibraryUpdatesCommand_sets_up_to_date_status_when_no_updates_exist()
    {
        var context = new ViewModelTestContext();
        var mainPageViewModel = context.CreateMainPageViewModel(context.CreateHomePageViewModel());

        await mainPageViewModel.CheckLibraryUpdatesCommand.ExecuteAsync(null);

        Assert.Equal(context.ResourceWrapper.LibrariesUpToDateStatus, mainPageViewModel.LibraryUpdateStatus);
        Assert.Equal(1, context.ToolUpdateService.Calls);
        Assert.True(mainPageViewModel.CanCloseApplication);
    }

    [Fact]
    public async Task CheckLibraryUpdatesCommand_downloads_confirmed_updates()
    {
        var context = new ViewModelTestContext();
        context.ToolUpdateService.Updates =
        [
            new ToolUpdateInfo
            {
                DisplayName = TestConstants.ToolName,
                CurrentVersion = TestConstants.CurrentToolVersion,
                LatestVersion = TestConstants.LatestToolVersion,
                FileNames = [TestConstants.ToolFileName]
            }
        ];
        var mainPageViewModel = context.CreateMainPageViewModel(context.CreateHomePageViewModel());

        await mainPageViewModel.CheckLibraryUpdatesCommand.ExecuteAsync(null);

        Assert.Equal(context.ResourceWrapper.LibrariesUpdatedStatus, mainPageViewModel.LibraryUpdateStatus);
        Assert.Single(context.UiPopup.ToolUpdateConfirmations);
        Assert.Equal(1, context.UiPopup.ToolUpdateProgressCalls);
        var downloadRequest = Assert.Single(context.ToolBootstrapService.DownloadRequests);
        Assert.Equal([TestConstants.ToolFileName], downloadRequest);
        Assert.Single(context.UiPopup.Messages);
    }

    [Fact]
    public async Task CheckLibraryUpdatesCommand_logs_and_shows_error_when_check_fails()
    {
        var context = new ViewModelTestContext();
        context.ToolUpdateService.Exception = new InvalidOperationException(TestConstants.UpdateFailureMessage);
        var mainPageViewModel = context.CreateMainPageViewModel(context.CreateHomePageViewModel());

        await mainPageViewModel.CheckLibraryUpdatesCommand.ExecuteAsync(null);

        Assert.Equal(string.Empty, mainPageViewModel.LibraryUpdateStatus);
        Assert.Contains(context.UiPopup.Errors, message => message.Contains(TestConstants.UpdateFailureMessage, StringComparison.Ordinal));
        Assert.Contains(context.Logger.Entries, entry => entry.Level == MediaDownloader.Logger.Enums.LogLevel.Error);
        Assert.True(mainPageViewModel.CanCloseApplication);
    }

    [Fact]
    public void OpenToolsFolderCommand_opens_tools_directory()
    {
        var context = new ViewModelTestContext();
        var mainPageViewModel = context.CreateMainPageViewModel(context.CreateHomePageViewModel());

        mainPageViewModel.OpenToolsFolderCommand.Execute(null);

        Assert.Equal([TestConstants.ToolsFolder], context.Shell.OpenedFolders);
    }
}
