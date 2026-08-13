using MediaDownloader.Tests.Constants;
using MediaDownloader.Tests.Support;
using MediaDownloader.ViewModels;
using Xunit;

namespace MediaDownloader.Tests.ViewModels;

public sealed class UrlMediaItemViewModelTests
{
    [Fact]
    public void IsAudioSelected_updates_media_type_and_video_quality_visibility()
    {
        var context = new ViewModelTestContext();
        var viewModel = CreateViewModel(context);

        viewModel.IsAudioSelected = true;

        Assert.Equal(context.ResourceWrapper.Mp3Label, viewModel.SelectedMediaType);
        Assert.False(viewModel.AreVideoQualitiesVisible);

        viewModel.IsAudioSelected = false;

        Assert.Equal(context.ResourceWrapper.Mp4Label, viewModel.SelectedMediaType);
        Assert.True(viewModel.AreVideoQualitiesVisible);
    }

    [Fact]
    public void IsDownloading_updates_button_state_and_options_availability()
    {
        var context = new ViewModelTestContext();
        var viewModel = CreateViewModel(context);

        viewModel.IsDownloading = true;

        Assert.Equal(context.AppConstants.Images.StopDownloadPath, viewModel.DownloadButtonIconPath);
        Assert.Equal(context.ResourceWrapper.StopDownloadToolTip, viewModel.DownloadButtonToolTip);
        Assert.False(viewModel.CanChangeDownloadOptions);

        viewModel.IsDownloading = false;

        Assert.Equal(context.AppConstants.Images.DownloadPath, viewModel.DownloadButtonIconPath);
        Assert.Equal(context.ResourceWrapper.DownloadToolTip, viewModel.DownloadButtonToolTip);
        Assert.True(viewModel.CanChangeDownloadOptions);
    }

    [Fact]
    public void Selecting_quality_updates_selected_video_quality()
    {
        var context = new ViewModelTestContext();
        var viewModel = CreateViewModel(context);

        viewModel.AvailableVideoQualities.Single(option => option.Name == TestConstants.Media.Quality720p)
            .IsSelected = true;

        Assert.Equal(TestConstants.Media.Quality720p, viewModel.SelectedVideoQuality);
    }

    [Fact]
    public void ToModel_preserves_mutable_download_state()
    {
        var context = new ViewModelTestContext();
        var viewModel = CreateViewModel(context);
        viewModel.DownloadProgress = TestConstants.Progress50;
        viewModel.DownloadStatus = context.ResourceWrapper.ProcessingDownloadStatus;
        viewModel.DownloadDate = TestConstants.DownloadDate;
        viewModel.DownloadFilePath = TestConstants.DownloadPath;

        var model = viewModel.ToModel();

        Assert.Equal(TestConstants.Progress50, model.DownloadProgress);
        Assert.Equal(context.ResourceWrapper.ProcessingDownloadStatus, model.DownloadStatus);
        Assert.Equal(TestConstants.DownloadDate, model.DownloadDate);
        Assert.Equal(TestConstants.DownloadPath, model.DownloadFilePath);
        Assert.Equal(TestConstants.Media.Quality1080p, model.AvailableVideoQualities[0].Name);
    }

    private static UrlMediaItemViewModel CreateViewModel(ViewModelTestContext context) =>
        new(
            TestModels.CreateMediaItem(),
            context.ResourceWrapper.ReadyStatus,
            context.ResourceWrapper.Mp4Label,
            context.ResourceWrapper.Mp3Label,
            context.AppConstants.Images.DownloadPath,
            context.AppConstants.Images.StopDownloadPath,
            context.ResourceWrapper.DownloadToolTip,
            context.ResourceWrapper.StopDownloadToolTip);
}
