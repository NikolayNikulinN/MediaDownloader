using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MediaDownloader.ViewModels;

namespace MediaDownloader.Views;

/// <summary>
/// Displays URL analysis and download controls.
/// </summary>
public partial class HomePage : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    public HomePage()
    {
        InitializeComponent();
    }

    private async void UrlTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        e.Handled = true;
        if (sender is not TextBox textBox
            || textBox.DataContext is not HomePageViewModel viewModel
            || !viewModel.AddUrlCommand.CanExecute(null))
        {
            return;
        }

        await viewModel.AddUrlCommand.ExecuteAsync(null);
        await textBox.Dispatcher.InvokeAsync(
                                             () =>
                                             {
                                                 textBox.Focus();
                                                 Keyboard.Focus(textBox);
                                             },
                                             DispatcherPriority.ContextIdle);
    }
}
