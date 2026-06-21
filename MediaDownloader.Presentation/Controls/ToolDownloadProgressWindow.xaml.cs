using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Wpf.Ui.Controls;

namespace MediaDownloader.Presentation.Controls;

internal partial class ToolDownloadProgressWindow : FluentWindow, INotifyPropertyChanged
{
    private readonly string statusFormat;
    private bool canClose;
    private string statusText;

    public ToolDownloadProgressWindow(
                                       string windowTitle,
                                       string initialStatus,
                                       string statusFormat,
                                       string iconPath)
    {
        WindowTitle = windowTitle;
        statusText = initialStatus;
        this.statusFormat = statusFormat;
        IconPath = iconPath;

        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string WindowTitle { get; }

    public string IconPath { get; }

    public string StatusText
    {
        get => statusText;
        private set
        {
            if (statusText == value)
            {
                return;
            }

            statusText = value;
            OnPropertyChanged();
        }
    }

    public void Report(string fileName) => StatusText = string.Format(
                                                                       CultureInfo.CurrentUICulture,
                                                                       statusFormat,
                                                                       fileName);

    public void Complete()
    {
        canClose = true;
        Close();
    }

    private void WindowClosing(object? sender, CancelEventArgs e) => e.Cancel = !canClose;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
