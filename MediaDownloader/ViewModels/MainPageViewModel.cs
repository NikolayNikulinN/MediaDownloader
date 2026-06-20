using CommunityToolkit.Mvvm.ComponentModel;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using System.Collections.ObjectModel;

namespace MediaDownloader.ViewModels;

/// <summary>
/// Coordinates the main window state and active application page.
/// </summary>
public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject activePage;

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
        HomePageViewModel homePageViewModel)
    {
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(resourceWrapper);
        ArgumentNullException.ThrowIfNull(homePageViewModel);

        AppConstants = appConstants;
        ResourceWrapper = resourceWrapper;
        maximizeRestoreIconPath = AppConstants.Images.MaximizePath;
        maximizeRestoreToolTip = ResourceWrapper.MaximizeToolTip;

        ListPages =
        [
            homePageViewModel
        ];

        activePage = ListPages[0];
    }

    public AppConstants AppConstants { get; }

    public ResourceWrapper ResourceWrapper { get; }

    public ObservableCollection<ObservableObject> ListPages { get; }

    /// <summary>
    /// Updates the title-bar icon and tooltip for the current window state.
    /// </summary>
    /// <param name="isMaximized">Whether the main window is maximized.</param>
    public void UpdateMaximizeRestoreState(bool isMaximized)
    {
        MaximizeRestoreIconPath = isMaximized
            ? AppConstants.Images.RestorePath
            : AppConstants.Images.MaximizePath;

        MaximizeRestoreToolTip = isMaximized
            ? ResourceWrapper.RestoreToolTip
            : ResourceWrapper.MaximizeToolTip;
    }
}
