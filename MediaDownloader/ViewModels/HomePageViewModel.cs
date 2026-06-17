using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace MediaDownloader.ViewModels
{
    public sealed class HomePageViewModel : ObservableObject
    {
        private string outputFolder;

        public HomePageViewModel()
        {
            outputFolder = GetDefaultOutputFolder();
            BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
            OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);
        }

        public string OutputFolder
        {
            get => outputFolder;
            set => SetProperty(ref outputFolder, value);
        }

        public IRelayCommand BrowseOutputFolderCommand { get; }

        public IRelayCommand OpenOutputFolderCommand { get; }

        private static string GetDefaultOutputFolder() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        private void BrowseOutputFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select download folder",
                InitialDirectory = Directory.Exists(OutputFolder)
                    ? OutputFolder
                    : GetDefaultOutputFolder()
            };

            if (dialog.ShowDialog() == true)
            {
                OutputFolder = dialog.FolderName;
            }
        }

        private void OpenOutputFolder()
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                return;
            }

            Directory.CreateDirectory(OutputFolder);

            Process.Start(new ProcessStartInfo
            {
                FileName = OutputFolder,
                UseShellExecute = true
            });
        }
    }
}
