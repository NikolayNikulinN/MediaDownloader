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

    private static void Show(
        string message,
        string title,
        string buttonText,
        string iconPath,
        string closeToolTip,
        string closeIconPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        if (!dispatcher.CheckAccess())
        {
            dispatcher.Invoke(
                () => ShowCore(
                    message,
                    title,
                    buttonText,
                    iconPath,
                    closeToolTip,
                    closeIconPath));
            return;
        }

        ShowCore(message, title, buttonText, iconPath, closeToolTip, closeIconPath);
    }

    private static void ShowCore(
        string message,
        string title,
        string buttonText,
        string iconPath,
        string closeToolTip,
        string closeIconPath)
    {
        var popup = new UiPopupWindow(
            message,
            title,
            buttonText,
            iconPath,
            closeToolTip,
            closeIconPath);
        var popupOwner = FindActiveWindow();

        if (popupOwner is { IsVisible: true } && popupOwner != popup)
        {
            popup.Owner = popupOwner;
        }

        popup.ShowDialog();
    }

    private static Window? FindActiveWindow()
    {
        if (Application.Current is null)
        {
            return null;
        }

        return Application.Current.Windows
                   .OfType<Window>()
                   .FirstOrDefault(window => window.IsActive)
               ?? Application.Current.MainWindow;
    }
}
