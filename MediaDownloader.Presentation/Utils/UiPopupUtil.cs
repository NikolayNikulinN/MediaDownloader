using MediaDownloader.Presentation.Enums;
using MediaDownloader.Presentation.Resources;
using System.Windows;
using System.Windows.Threading;

namespace MediaDownloader.Presentation.Utils
{
    public static class UiPopupUtil
    {
        private static readonly ResourceWrapper Resources = new();

        public static void ShowMessage(
                                        string message,
                                        Window? owner = null) =>
                                        Show(message, Resources.InformationPopupTitle, UiPopupType.Information, owner);

        public static void ShowWarning(
                                        string message,
                                        Window? owner = null) =>
                                        Show(message, Resources.WarningPopupTitle, UiPopupType.Warning, owner);

        public static void ShowError(
                                     string message,
                                     Window? owner = null) =>
                                     Show(message, Resources.ErrorPopupTitle, UiPopupType.Error, owner);

        private static void Show(
                                  string message,
                                  string title,
                                  UiPopupType type,
                                  Window? owner = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(message);
            ArgumentException.ThrowIfNullOrWhiteSpace(title);

            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() => ShowCore(message, title, type, owner));
                return;
            }

            ShowCore(message, title, type, owner);
        }

        private static void ShowCore(
                                     string message,
                                     string title,
                                     UiPopupType type,
                                     Window? owner)
        {
            var popup = new UiPopupWindow(message, title, type);
            var popupOwner = owner ?? FindActiveWindow();

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

            return Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive) ?? Application.Current.MainWindow;
        }
    }
}
