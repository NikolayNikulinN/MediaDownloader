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
        string closeIconPath)
    {
        PopupTitle = title;
        Message = message;
        ButtonText = buttonText;
        IconPath = iconPath;
        CloseToolTip = closeToolTip;
        CloseIconPath = closeIconPath;

        InitializeComponent();
        DataContext = this;
    }

    public string PopupTitle { get; }

    public string Message { get; }

    public string ButtonText { get; }

    public string IconPath { get; }

    public string CloseToolTip { get; }

    public string CloseIconPath { get; }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void OkButtonClick(object sender, RoutedEventArgs e) =>
        ClosePopup();

    private void CloseButtonClick(object sender, RoutedEventArgs e) =>
        ClosePopup();

    private void WindowPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ClosePopup();
        }
    }

    private void ClosePopup() =>
        DialogResult = true;
}
