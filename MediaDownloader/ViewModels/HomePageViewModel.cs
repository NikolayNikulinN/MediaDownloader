using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediaDownloader.Logger.Abstract;
using MediaDownloader.Logger.Constants;
using MediaDownloader.Logger.Enums;
using MediaDownloader.Presentation.Resources;
using MediaDownloader.Presentation.Utils;
using MediaDownloader.Services.Abstract;
using MediaDownloader.Services.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace MediaDownloader.ViewModels
{
    public sealed class HomePageViewModel : ObservableObject
    {
        private readonly IYtDlpService ytDlpService;
        private readonly ILoggerWrapper loggerWrapper;
        private string outputFolder;
        private string url = string.Empty;
        private string analysisStatus = string.Empty;
        private bool isAnalyzing;

        public HomePageViewModel(
            IYtDlpService ytDlpService,
            ILoggerWrapper loggerWrapper,
            ResourceWrapper resourceWrapper)
        {
            ArgumentNullException.ThrowIfNull(ytDlpService);
            ArgumentNullException.ThrowIfNull(loggerWrapper);
            ArgumentNullException.ThrowIfNull(resourceWrapper);

            this.ytDlpService = ytDlpService;
            this.loggerWrapper = loggerWrapper;
            this.ResourceWrapper = resourceWrapper;

            outputFolder = GetDefaultOutputFolder();
            UrlItems = [];
            UrlItems.CollectionChanged += UrlItems_CollectionChanged;
            AddUrlCommand = new AsyncRelayCommand(AnalyzeUrlAsync, CanAnalyzeUrl);
            BrowseOutputFolderCommand = new RelayCommand(BrowseOutputFolder);
            OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);
        }

        public string Url
        {
            get => url;
            set
            {
                if (SetProperty(ref url, value))
                {
                    OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
                    AddUrlCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string OutputFolder
        {
            get => outputFolder;
            set => SetProperty(ref outputFolder, value);
        }

        public string AnalysisStatus
        {
            get => analysisStatus;
            private set => SetProperty(ref analysisStatus, value);
        }

        public bool IsAnalyzing
        {
            get => isAnalyzing;
            private set
            {
                if (SetProperty(ref isAnalyzing, value))
                {
                    OnPropertyChanged(nameof(IsUrlInputEnabled));
                    OnPropertyChanged(nameof(IsUrlPlaceholderVisible));
                    AddUrlCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsUrlInputEnabled => !IsAnalyzing;

        public bool IsUrlPlaceholderVisible =>
            IsUrlInputEnabled && string.IsNullOrWhiteSpace(Url);

        public bool IsNoDataVisible => UrlItems.Count == 0;

        public ObservableCollection<UrlMediaItem> UrlItems { get; }

        public ResourceWrapper ResourceWrapper { get; }

        public IAsyncRelayCommand AddUrlCommand { get; }

        public IRelayCommand BrowseOutputFolderCommand { get; }

        public IRelayCommand OpenOutputFolderCommand { get; }

        private static string GetDefaultOutputFolder() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        private void BrowseOutputFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = ResourceWrapper.SelectDownloadFolderTitle,
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

        private bool CanAnalyzeUrl() =>
            !IsAnalyzing && !string.IsNullOrWhiteSpace(Url);

        private void UrlItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsNoDataVisible));
        }

        private async Task AnalyzeUrlAsync()
        {
            UiPopupUtil.ShowWarning("test");
            loggerWrapper.Write("test", LogLevel.Error);

            //try
            //{
            //    IsAnalyzing = true;
            //    AnalysisStatus = ResourceWrapper.AnalyzingUrlStatus;

            //    var mediaItem = await ytDlpService.GetMediaInfoAsync(Url);
            //    mediaItem.DownloadStatus = ResourceWrapper.ReadyStatus;
            //    UrlItems.Add(mediaItem);

            //    Url = string.Empty;
            //    AnalysisStatus = string.Empty;
            //}
            //catch (Exception exception)
            //{
            //    AnalysisStatus = exception.Message;
            //    loggerWrapper.Write(string.Format(LogConstants.AnalyzeUrlFailed, exception.Message), LogLevel.Error);
            //    UiPopupUtil.ShowError(exception.Message);
            //}
            //finally
            //{
            //    IsAnalyzing = false;
            //}
        }
    }
}
