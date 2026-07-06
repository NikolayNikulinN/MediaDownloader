using MediaDownloader.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace MediaDownloader.Views;

/// <summary>
/// Hosts the main application page and custom title bar.
/// </summary>
public partial class MainPage : FluentWindow
{
    private readonly MainPageViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPage"/> class.
    /// </summary>
    /// <param name="viewModel">Main window view model.</param>
    public MainPage(MainPageViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleWindowState();
            return;
        }

        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButtonClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void MaximizeRestoreButtonClick(object sender, RoutedEventArgs e) =>
        ToggleWindowState();

    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (viewModel.CanCloseApplication)
        {
            Close();
        }
    }

    private void WindowStateChanged(object sender, EventArgs e) =>
        viewModel.UpdateMaximizeRestoreState(WindowState == WindowState.Maximized);

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (!viewModel.CanCloseApplication)
        {
            e.Cancel = true;
        }
    }

    private void ToggleWindowState() =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
}
