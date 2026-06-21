using MediaDownloader.Presentation.Abstract;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Controls;
using MediaDownloader.Presentation.Resources;
using System.Windows;
using System.Windows.Threading;

namespace MediaDownloader.Presentation.Services;

/// <summary>
/// Displays modal Fluent UI popup windows.
/// </summary>
public sealed class UiPopupUtil : IUiPopupUtil
{
    private readonly AppConstants appConstants;
    private readonly ResourceWrapper resourceWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UiPopupUtil"/> class.
    /// </summary>
    /// <param name="appConstants">Application constants.</param>
    /// <param name="resourceWrapper">Localized UI resources.</param>
    public UiPopupUtil(AppConstants appConstants, ResourceWrapper resourceWrapper)
    {
        ArgumentNullException.ThrowIfNull(appConstants);
        ArgumentNullException.ThrowIfNull(resourceWrapper);

        this.appConstants = appConstants;
        this.resourceWrapper = resourceWrapper;
    }

    /// <inheritdoc />
    public void ShowMessage(string message) =>
        Show(
            message,
            resourceWrapper.InformationPopupTitle,
            resourceWrapper.OkButtonText,
            appConstants.Images.InformationPath,
            resourceWrapper.CloseToolTip,
            appConstants.Images.ClosePath);

    /// <inheritdoc />
    public void ShowWarning(string message) =>
        Show(
            message,
            resourceWrapper.WarningPopupTitle,
            resourceWrapper.OkButtonText,
            appConstants.Images.WarningPath,
            resourceWrapper.CloseToolTip,
            appConstants.Images.ClosePath);

    /// <inheritdoc />
    public void ShowError(string message) =>
        Show(
            message,
            resourceWrapper.ErrorPopupTitle,
            resourceWrapper.OkButtonText,
            appConstants.Images.ErrorPath,
            resourceWrapper.CloseToolTip,
            appConstants.Images.ClosePath);

    /// <inheritdoc />
    public bool ConfirmToolDownload(IReadOnlyCollection<string> missingTools)
    {
        ArgumentNullException.ThrowIfNull(missingTools);

        var toolList = string.Join(
                                    Environment.NewLine,
                                    missingTools.Select(tool => $"{resourceWrapper.ListItemPrefix}{tool}"));

        var message = string.Format(
                                    System.Globalization.CultureInfo.CurrentUICulture,
                                    resourceWrapper.MissingToolsMessage,
                                    toolList);

        return Show(
                    message,
                    resourceWrapper.MissingToolsTitle,
                    resourceWrapper.DownloadButtonText,
                    appConstants.Images.WarningPath,
                    resourceWrapper.CloseToolTip,
                    appConstants.Images.ClosePath,
                    resourceWrapper.CancelButtonText);
    }

    /// <inheritdoc />
    public bool ConfirmToolUpdate(IReadOnlyCollection<string> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var updateList = string.Join(
                                     Environment.NewLine,
                                     updates.Select(update => $"{resourceWrapper.ListItemPrefix}{update}"));

        var message = string.Format(
                                     System.Globalization.CultureInfo.CurrentUICulture,
                                     resourceWrapper.ToolUpdatesAvailableMessage,
                                     updateList);

        return Show(
                    message,
                    resourceWrapper.ToolUpdatesAvailableTitle,
                    resourceWrapper.UpdateButtonText,
                    appConstants.Images.InformationPath,
                    resourceWrapper.CloseToolTip,
                    appConstants.Images.ClosePath,
                    resourceWrapper.CancelButtonText);
    }

    /// <inheritdoc />
    public bool ConfirmStopDownload() => Show(
                                               resourceWrapper.StopDownloadConfirmationMessage,
                                               resourceWrapper.StopDownloadConfirmationTitle,
                                               resourceWrapper.YesButtonText,
                                               appConstants.Images.WarningPath,
                                               resourceWrapper.CloseToolTip,
                                               appConstants.Images.ClosePath,
                                               resourceWrapper.NoButtonText);

    /// <inheritdoc />
    public async Task ShowToolDownloadProgressAsync(Func<IProgress<string>, Task> downloadOperation) => await ShowProgressAsync(
                                                                                                                                 downloadOperation,
                                                                                                                                 resourceWrapper.DownloadingToolsTitle,
                                                                                                                                 resourceWrapper.PreparingDownloadStatus,
                                                                                                                                 resourceWrapper.DownloadingToolStatus);

    /// <inheritdoc />
    public async Task ShowToolUpdateProgressAsync(Func<IProgress<string>, Task> updateOperation) => await ShowProgressAsync(
                                                                                                                             updateOperation,
                                                                                                                             resourceWrapper.UpdatingLibrariesTitle,
                                                                                                                             resourceWrapper.PreparingUpdateStatus,
                                                                                                                             resourceWrapper.UpdatingLibraryStatus);

    private async Task ShowProgressAsync(
                                          Func<IProgress<string>, Task> operation,
                                          string title,
                                          string initialStatus,
                                          string statusFormat)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var progressWindow = new ToolDownloadProgressWindow(
                                                            title,
                                                            initialStatus,
                                                            statusFormat,
                                                            appConstants.Images.InformationPath);
        var popupOwner = FindActiveWindow();
        var ownerWasEnabled = popupOwner?.IsEnabled ?? false;

        if (popupOwner is { IsVisible: true })
        {
            progressWindow.Owner = popupOwner;
            progressWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            popupOwner.IsEnabled = false;
        }

        progressWindow.Show();
        await progressWindow.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

        try
        {
            var progress = new Progress<string>(progressWindow.Report);
            await operation(progress);
        }
        finally
        {
            progressWindow.Complete();
            if (popupOwner is not null)
            {
                popupOwner.IsEnabled = ownerWasEnabled;
                popupOwner.Activate();
            }
        }
    }

    private static bool Show(
                              string message,
                              string title,
                              string buttonText,
                              string iconPath,
                              string closeToolTip,
                              string closeIconPath,
                              string? secondaryButtonText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        if (!dispatcher.CheckAccess())
        {
            return dispatcher.Invoke(() => ShowCore(
                                                     message,
                                                     title,
                                                     buttonText,
                                                     iconPath,
                                                     closeToolTip,
                                                     closeIconPath,
                                                     secondaryButtonText));
        }

        return ShowCore(
                        message,
                        title,
                        buttonText,
                        iconPath,
                        closeToolTip,
                        closeIconPath,
                        secondaryButtonText);
    }

    private static bool ShowCore(
                                 string message,
                                 string title,
                                 string buttonText,
                                 string iconPath,
                                 string closeToolTip,
                                 string closeIconPath,
                                 string? secondaryButtonText)
    {
        var popup = new UiPopupWindow(
                                      message,
                                      title,
                                      buttonText,
                                      iconPath,
                                      closeToolTip,
                                      closeIconPath,
                                      secondaryButtonText);

        var popupOwner = FindActiveWindow();
        if (popupOwner is { IsVisible: true } && popupOwner != popup)
        {
            popup.Owner = popupOwner;
        }
        else
        {
            popup.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return popup.ShowDialog() == true;
    }

    private static Window? FindActiveWindow()
    {
        if (Application.Current is null)
        {
            return null;
        }

        return Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive) ?? Application.Current.MainWindow;
    }
}
