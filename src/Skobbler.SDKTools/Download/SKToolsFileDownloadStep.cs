namespace Skobbler.SDKTools.Download
{
    public class SKToolsFileDownloadStep
    {
        public string DownloadURL { get; private set; }
        public string DestinationPath { get; private set; }
        public long DownloadItemSize { get; private set; }

        public SKToolsFileDownloadStep(string downloadURL, string destinationPath, long downloadItemSize)
        {
            DownloadURL = downloadURL;
            DestinationPath = destinationPath;
            DownloadItemSize = downloadItemSize;
        }
    }
}