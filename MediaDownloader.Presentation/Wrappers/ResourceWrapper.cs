using System.Globalization;
using System.Resources;

namespace MediaDownloader.Presentation.Resources;

/// <summary>
/// Provides localized application resources to data bindings and services.
/// </summary>
public sealed class ResourceWrapper
{
    private readonly ResourceManager resourceManager = AppResource.ResourceManager;

    public string AppTitle => GetString(nameof(AppTitle));

    public string MinimizeToolTip => GetString(nameof(MinimizeToolTip));

    public string MaximizeToolTip => GetString(nameof(MaximizeToolTip));

    public string RestoreToolTip => GetString(nameof(RestoreToolTip));

    public string CloseToolTip => GetString(nameof(CloseToolTip));

    public string AnalyzeUrlButtonText => GetString(nameof(AnalyzeUrlButtonText));

    public string YouTubeUrlToolTip => GetString(nameof(YouTubeUrlToolTip));

    public string UrlPlaceholder => GetString(nameof(UrlPlaceholder));

    public string SaveToLabel => GetString(nameof(SaveToLabel));

    public string DownloadFolderToolTip => GetString(nameof(DownloadFolderToolTip));

    public string BrowseButtonText => GetString(nameof(BrowseButtonText));

    public string OpenFolderToolTip => GetString(nameof(OpenFolderToolTip));

    public string DownloadToolTip => GetString(nameof(DownloadToolTip));

    public string DeleteToolTip => GetString(nameof(DeleteToolTip));

    public string DeleteConfirmationTitle => GetString(nameof(DeleteConfirmationTitle));

    public string DeleteConfirmationMessage => GetString(nameof(DeleteConfirmationMessage));

    public string DeleteDownloadedFileCheckBoxText => GetString(nameof(DeleteDownloadedFileCheckBoxText));

    public string DeleteDownloadedFileFailedMessage => GetString(nameof(DeleteDownloadedFileFailedMessage));

    public string MediaModeToolTip => GetString(nameof(MediaModeToolTip));

    public string Mp4Label => GetString(nameof(Mp4Label));

    public string Mp3Label => GetString(nameof(Mp3Label));

    public string SelectDownloadFolderTitle => GetString(nameof(SelectDownloadFolderTitle));

    public string AnalyzingUrlStatus => GetString(nameof(AnalyzingUrlStatus));

    public string ReadyStatus => GetString(nameof(ReadyStatus));

    public string NoDataText => GetString(nameof(NoDataText));

    public string InformationPopupTitle => GetString(nameof(InformationPopupTitle));

    public string WarningPopupTitle => GetString(nameof(WarningPopupTitle));

    public string ErrorPopupTitle => GetString(nameof(ErrorPopupTitle));

    public string OkButtonText => GetString(nameof(OkButtonText));

    public string MissingToolsTitle => GetString(nameof(MissingToolsTitle));

    public string MissingToolsMessage => GetString(nameof(MissingToolsMessage));

    public string DownloadButtonText => GetString(nameof(DownloadButtonText));

    public string CancelButtonText => GetString(nameof(CancelButtonText));

    public string ListItemPrefix => GetString(nameof(ListItemPrefix));

    public string ToolDownloadFailedMessage => GetString(nameof(ToolDownloadFailedMessage));

    public string ToolDownloadSucceededMessage => GetString(nameof(ToolDownloadSucceededMessage));

    public string DownloadingToolsTitle => GetString(nameof(DownloadingToolsTitle));

    public string PreparingDownloadStatus => GetString(nameof(PreparingDownloadStatus));

    public string DownloadingToolStatus => GetString(nameof(DownloadingToolStatus));

    public string CheckLibrariesToolTip => GetString(nameof(CheckLibrariesToolTip));

    public string OpenToolsFolderToolTip => GetString(nameof(OpenToolsFolderToolTip));

    public string CheckingUpdatesStatus => GetString(nameof(CheckingUpdatesStatus));

    public string LibrariesUpToDateStatus => GetString(nameof(LibrariesUpToDateStatus));

    public string UpdatesAvailableStatus => GetString(nameof(UpdatesAvailableStatus));

    public string LibrariesUpdatedStatus => GetString(nameof(LibrariesUpdatedStatus));

    public string ToolUpdatesAvailableTitle => GetString(nameof(ToolUpdatesAvailableTitle));

    public string ToolUpdatesAvailableMessage => GetString(nameof(ToolUpdatesAvailableMessage));

    public string ToolUpdateDescriptionFormat => GetString(nameof(ToolUpdateDescriptionFormat));

    public string UpdateButtonText => GetString(nameof(UpdateButtonText));

    public string ToolUpdateSucceededMessage => GetString(nameof(ToolUpdateSucceededMessage));

    public string ToolUpdateFailedMessage => GetString(nameof(ToolUpdateFailedMessage));

    public string UpdatingLibrariesTitle => GetString(nameof(UpdatingLibrariesTitle));

    public string PreparingUpdateStatus => GetString(nameof(PreparingUpdateStatus));

    public string UpdatingLibraryStatus => GetString(nameof(UpdatingLibraryStatus));

    public string DownloadBestQualityText => GetString(nameof(DownloadBestQualityText));

    public string StopDownloadToolTip => GetString(nameof(StopDownloadToolTip));

    public string DownloadingStatusFormat => GetString(nameof(DownloadingStatusFormat));

    public string ProcessingDownloadStatus => GetString(nameof(ProcessingDownloadStatus));

    public string StoppingDownloadStatus => GetString(nameof(StoppingDownloadStatus));

    public string DownloadCompletedStatus => GetString(nameof(DownloadCompletedStatus));

    public string DownloadCancelledStatus => GetString(nameof(DownloadCancelledStatus));

    public string DownloadFailedStatus => GetString(nameof(DownloadFailedStatus));

    public string StopDownloadConfirmationTitle => GetString(nameof(StopDownloadConfirmationTitle));

    public string StopDownloadConfirmationMessage => GetString(nameof(StopDownloadConfirmationMessage));

    public string YesButtonText => GetString(nameof(YesButtonText));

    public string NoButtonText => GetString(nameof(NoButtonText));

    public string this[string key] => GetString(key);

    private string GetString(string key) => resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? string.Empty;
}
