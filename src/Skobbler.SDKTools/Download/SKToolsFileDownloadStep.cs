namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsFileDownloadStep
    {
        public SKToolsFileDownloadStep(string downloadURL, string destinationPath, long downloadItemSize)
        {
            DownloadURL = downloadURL;
            DestinationPath = destinationPath;
            DownloadItemSize = downloadItemSize;
        }

        public string DownloadURL { get; private set; }
        public string DestinationPath { get; private set; }
        public long DownloadItemSize { get; private set; }
    }
}