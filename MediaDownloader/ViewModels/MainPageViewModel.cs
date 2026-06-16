using CommunityToolkit.Mvvm.ComponentModel;
using MediaDownloader.Presentation.Constants;
using MediaDownloader.Presentation.Resources;
using System.Collections.ObjectModel;

namespace MediaDownloader.ViewModels
{
    public sealed class MainPageViewModel : ObservableObject
    {
        private ObservableObject activePage;
        private string maximizeRestoreIconPath;
        private string maximizeRestoreToolTip;

        public MainPageViewModel(
            AppConstants appConstants,
            ResourceWrapper resourceWrapper,
            HomePageViewModel homePageViewModel)
        {
            AppConstants = appConstants;
            ResourceWrapper = resourceWrapper;
            maximizeRestoreIconPath = AppConstants.Images.MaximizePath;
            maximizeRestoreToolTip = ResourceWrapper.MaximizeToolTip;

            ListPages =
            [
                homePageViewModel
            ];

            activePage = ListPages[0];
        }

        public AppConstants AppConstants { get; }

        public ResourceWrapper ResourceWrapper { get; }

        public ObservableCollection<ObservableObject> ListPages { get; }

        public ObservableObject ActivePage
        {
            get => activePage;
            private set => SetProperty(ref activePage, value);
        }

        public string MaximizeRestoreIconPath
        {
            get => maximizeRestoreIconPath;
            private set => SetProperty(ref maximizeRestoreIconPath, value);
        }

        public string MaximizeRestoreToolTip
        {
            get => maximizeRestoreToolTip;
            private set => SetProperty(ref maximizeRestoreToolTip, value);
        }

        public void UpdateMaximizeRestoreState(bool isMaximized)
        {
            MaximizeRestoreIconPath = isMaximized
                ? AppConstants.Images.RestorePath
                : AppConstants.Images.MaximizePath;

            MaximizeRestoreToolTip = isMaximized
                ? ResourceWrapper.RestoreToolTip
                : ResourceWrapper.MaximizeToolTip;
        }
    }
}
