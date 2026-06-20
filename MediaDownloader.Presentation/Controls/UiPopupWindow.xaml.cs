using MediaDownloader.Presentation.Enums;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace MediaDownloader.Presentation.Utils
{
    internal partial class UiPopupWindow : FluentWindow
    {
        public UiPopupWindow(string message, string title, UiPopupType type)
        {
            PopupTitle = title;
            Message = message;
            (IconGlyph, IconBackground) = GetAppearance(type);

            InitializeComponent();
            DataContext = this;
        }

        public string PopupTitle { get; }

        public string Message { get; }

        public string IconGlyph { get; }

        public Brush IconBackground { get; }

        private static (string Glyph, Brush Background) GetAppearance(UiPopupType type) =>
            type switch
            {
                UiPopupType.Warning => ("\uE7BA", CreateBrush("#F59E0B")),
                UiPopupType.Error => ("\uEA39", CreateBrush("#DC2626")),
                _ => ("\uE946", CreateBrush("#2563EB"))
            };

        private static Brush CreateBrush(string color)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            brush.Freeze();
            return brush;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) =>
            ClosePopup();

        private void CloseButton_Click(object sender, RoutedEventArgs e) =>
            ClosePopup();

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClosePopup();
            }
        }

        private void ClosePopup()
        {
            DialogResult = true;
        }
    }
}
