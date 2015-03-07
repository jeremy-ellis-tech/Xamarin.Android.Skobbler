namespace Skobbler.Ngx.SDKTools.Download
{
    public interface ISKToolsDownloadListener
    {
        void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem);
        void OnDownloadCancelled(string currentDownloadItemCode);
        void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem);
        void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer);
        void OnAllDownloadsCancelled();
        void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem);
        void OnInstallStarted(SKToolsDownloadItem currentInstallingItem);
        void OnInstallFinished(SKToolsDownloadItem currentInstallingItem);
    }
}