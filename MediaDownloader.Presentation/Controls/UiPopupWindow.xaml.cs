using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace MediaDownloader.Presentation.Controls;

internal partial class UiPopupWindow : FluentWindow
{
    public UiPopupWindow(
        string message,
        string title,
        string buttonText,
        string iconPath,
        string closeToolTip,
        string closeIconPath,
        string? secondaryButtonText = null)
    {
        PopupTitle = title;
        Message = message;
        ButtonText = buttonText;
        IconPath = iconPath;
        CloseToolTip = closeToolTip;
        CloseIconPath = closeIconPath;
        SecondaryButtonText = secondaryButtonText ?? string.Empty;
        SecondaryButtonVisibility = string.IsNullOrWhiteSpace(secondaryButtonText)
                                                                                    ? Visibility.Collapsed
                                                                                    : Visibility.Visible;

        InitializeComponent();
        DataContext = this;
    }

    public string PopupTitle { get; }

    public string Message { get; }

    public string ButtonText { get; }

    public string IconPath { get; }

    public string CloseToolTip { get; }

    public string CloseIconPath { get; }

    public string SecondaryButtonText { get; }

    public Visibility SecondaryButtonVisibility { get; }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OkButtonClick(object sender, RoutedEventArgs e) =>
        ClosePopup(true);

    private void CloseButtonClick(object sender, RoutedEventArgs e) =>
        ClosePopup(false);

    private void CancelButtonClick(object sender, RoutedEventArgs e) => ClosePopup(false);

    private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClosePopup(false);
        }
    }

    private void ClosePopup(bool result) => DialogResult = result;
}
