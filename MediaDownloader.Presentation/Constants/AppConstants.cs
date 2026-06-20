namespace MediaDownloader.Presentation.Constants
{
    public sealed class AppConstants
    {
        public AppConstants()
        {
            Images = new();
            Common = new();
        }

        public ImagePaths Images { get; }

        public CommonConstants Common { get; }
    }
}
