using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Coordinates the main window state and active application page.
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    private readonly ILoggerWrapper loggerWrapper;
    private readonly HomePageViewModel homePageViewModel;
    private readonly IShellUtil shellUtil;
    private readonly IToolBootstrapService toolBootstrapService;
    private readonly IToolUpdateService toolUpdateService;
    private readonly IUiPopupUtil uiPopupUtil;

    [ObservableProperty]
    private ObservableObject activePage;

    [ObservableProperty]
    private bool isCheckingUpdates;

    [ObservableProperty]
    private string libraryUpdateStatus = string.Empty;

    [ObservableProperty]
    private string maximizeRestoreIconPath;

    [ObservableProperty]
    private string maximizeRestoreToolTip;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    public MainPageViewModel(
                             AppConstants appConstants,
                             ResourceWrapper resourceWrapper,
                             HomePageViewModel homePageViewModel,
                             IToolUpdateService toolUpdateService,
                             IToolBootstrapService toolBootstrapService,
                             IUiPopupUtil uiPopupUtil,
                             IShellUtil shellUtil,
                             ILoggerWrapper loggerWrapper)
    {
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(homePageViewModel);
        ArgumentNullException.ThrowIfNull(toolUpdateService);
        ArgumentNullException.ThrowIfNull(toolBootstrapService);
        ArgumentNullException.ThrowIfNull(uiPopupUtil);
        ArgumentNullException.ThrowIfNull(shellUtil);
        ArgumentNullException.ThrowIfNull(loggerWrapper);

        AppConstants = appConstants;
        ResourceWrapper = resourceWrapper;
        this.homePageViewModel = homePageViewModel;
        this.toolUpdateService = toolUpdateService;
        this.toolBootstrapService = toolBootstrapService;
        this.uiPopupUtil = uiPopupUtil;
        this.shellUtil = shellUtil;
        this.loggerWrapper = loggerWrapper;
        maximizeRestoreIconPath = AppConstants.Images.MaximizePath;
        maximizeRestoreToolTip = ResourceWrapper.MaximizeToolTip;
        homePageViewModel.PropertyChanged += HomePageViewModelPropertyChanged;

        ListPages =
        [
            homePageViewModel
        ];

        activePage = ListPages[0];
        CheckLibraryUpdatesCommand = new AsyncRelayCommand(
                                                            CheckLibraryUpdatesAsync,
                                                            CanCheckLibraryUpdates);
        OpenToolsFolderCommand = new RelayCommand(OpenToolsFolder);
    }

    public AppConstants AppConstants { get; }

    public ResourceWrapper ResourceWrapper { get; }

    public ObservableCollection<ObservableObject> ListPages { get; }

    public IAsyncRelayCommand CheckLibraryUpdatesCommand { get; }

    public IRelayCommand OpenToolsFolderCommand { get; }

    public bool CanCloseApplication => !IsCheckingUpdates && !homePageViewModel.HasActiveOperation;

    public string CloseButtonToolTip => CanCloseApplication
        ? ResourceWrapper.CloseToolTip
        : ResourceWrapper.CloseBlockedToolTip;

    partial void OnIsCheckingUpdatesChanged(bool value)
    {
        CheckLibraryUpdatesCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanCloseApplication));
        OnPropertyChanged(nameof(CloseButtonToolTip));
    }

    /// <summary>
    /// Updates the title-bar icon and tooltip for the current window state.
    /// </summary>
    public void UpdateMaximizeRestoreState(bool isMaximized)
    {
        MaximizeRestoreIconPath = isMaximized ? AppConstants.Images.RestorePath : AppConstants.Images.MaximizePath;
        MaximizeRestoreToolTip = isMaximized ? ResourceWrapper.RestoreToolTip : ResourceWrapper.MaximizeToolTip;
    }

    private bool CanCheckLibraryUpdates() => !IsCheckingUpdates;

    private void HomePageViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(HomePageViewModel.HasActiveOperation))
        {
            return;
        }

        OnPropertyChanged(nameof(CanCloseApplication));
        OnPropertyChanged(nameof(CloseButtonToolTip));
    }

    private async Task CheckLibraryUpdatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            IsCheckingUpdates = true;
            LibraryUpdateStatus = ResourceWrapper.CheckingUpdatesStatus;

            var updates = await toolUpdateService.CheckForUpdatesAsync(cancellationToken);
            if (updates.Count == 0)
            {
                LibraryUpdateStatus = ResourceWrapper.LibrariesUpToDateStatus;
                return;
            }

            LibraryUpdateStatus = ResourceWrapper.UpdatesAvailableStatus;
            var updateDescriptions = updates
                                            .Select(CreateUpdateDescription)
                                            .ToArray();

            if (!uiPopupUtil.ConfirmToolUpdate(updateDescriptions))
            {
                return;
            }

            await UpdateToolsAsync(updates);
            LibraryUpdateStatus = ResourceWrapper.LibrariesUpdatedStatus;
            uiPopupUtil.ShowMessage(ResourceWrapper.ToolUpdateSucceededMessage);
        }
        catch (OperationCanceledException)
        {
            LibraryUpdateStatus = string.Empty;
        }
        catch (Exception exception)
        {
            LibraryUpdateStatus = string.Empty;
            loggerWrapper.Write(string.Format(
                                               CultureInfo.InvariantCulture,
                                               LogConstants.ToolUpdateCheckFailed,
                                               exception.Message),
                                LogLevel.Error);

            uiPopupUtil.ShowError(string.Format(
                                                 CultureInfo.CurrentUICulture,
                                                 ResourceWrapper.ToolUpdateFailedMessage,
                                                 exception.Message));
        }
        finally
        {
            IsCheckingUpdates = false;
        }
    }

    private async Task UpdateToolsAsync(IReadOnlyCollection<ToolUpdateInfo> updates)
    {
        var fileNames = updates
                                .SelectMany(update => update.FileNames)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToArray();

        await uiPopupUtil.ShowToolUpdateProgressAsync(progress => toolBootstrapService.DownloadMissingToolsAsync(
                                                                                                                  fileNames,
                                                                                                                  userConfirmed: true,
                                                                                                                  progress: progress));
    }

    private string CreateUpdateDescription(ToolUpdateInfo update) =>
                                                                     string.Format(
                                                                                    CultureInfo.CurrentUICulture,
                                                                                    ResourceWrapper.ToolUpdateDescriptionFormat,
                                                                                    update.DisplayName,
                                                                                    update.CurrentVersion,
                                                                                    update.LatestVersion);

    private void OpenToolsFolder() => shellUtil.OpenFolder(toolBootstrapService.ToolsDirectoryPath);
}
