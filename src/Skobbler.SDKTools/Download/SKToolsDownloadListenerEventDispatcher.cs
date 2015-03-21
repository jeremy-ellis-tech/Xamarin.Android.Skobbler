using System;

namespace Skobbler.Ngx.SDKTools.Download
{
    /// <summary>
    /// Helper class to provide events that correspond to listener callbacks.
    /// Taken from how automatic event generation is done during jar binding.
    /// See: http://developer.xamarin.com/guides/android/advanced_topics/java_integration_overview/binding_a_java_library_(.jar)/
    /// </summary>
    internal class SKToolsDownloadListenerEventDispatcher : ISKToolsDownloadListener
    {
        private readonly object _sender;

        public SKToolsDownloadListenerEventDispatcher(object sender)
        {
            _sender = sender;
        }

        internal EventHandler<SKDownloadProgressEventArgs> DownloadProgress;
        internal EventHandler<SKDownloadCancelledEventArgs> DownloadCancelled;
        internal EventHandler<SKDownloadPausedEventArgs> DownloadPaused; 
        internal EventHandler<SKInternetConnectionFailedEventArgs> InternetConnectionFailed;
        internal EventHandler AllDownloadsCancelled;
        internal EventHandler<SKNotEnoughMemoryOnCurrentStorageEventArgs> NotEnoughMemoryOnCurrentStorage;
        internal EventHandler<SKInstallStartedEventArgs> InstallStarted;
        internal EventHandler<SKInstallFinishedEventArgs> InstallFinished; 

        public void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem)
        {
            var handler = DownloadProgress;

            if (handler != null)
            {
                handler(_sender, new SKDownloadProgressEventArgs(currentDownloadItem));
            }
        }

        public void OnDownloadCancelled(string currentDownloadItemCode)
        {
            var handler = DownloadCancelled;

            if (handler != null)
            {
                handler(_sender, new SKDownloadCancelledEventArgs(currentDownloadItemCode));
            }
        }

        public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
        {
            var handler = DownloadPaused;

            if (handler != null)
            {
                handler(_sender, new SKDownloadPausedEventArgs(currentDownloadItem));
            }
        }

        public void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
        {
            var handler = InternetConnectionFailed;

            if (handler != null)
            {
                handler(_sender, new SKInternetConnectionFailedEventArgs(currentDownloadItem, responseReceivedFromServer));
            }
        }

        public void OnAllDownloadsCancelled()
        {
            var handler = AllDownloadsCancelled;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }

        public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
        {
            var handler = NotEnoughMemoryOnCurrentStorage;

            if (handler != null)
            {
                handler(_sender, new SKNotEnoughMemoryOnCurrentStorageEventArgs(currentDownloadItem));
            }
        }

        public void OnInstallStarted(SKToolsDownloadItem currentInstallingItem)
        {
            var handler = InstallStarted;

            if (handler != null)
            {
                handler(_sender, new SKInstallStartedEventArgs(currentInstallingItem));
            }
        }

        public void OnInstallFinished(SKToolsDownloadItem currentInstallingItem)
        {
            var handler = InstallFinished;

            if (handler != null)
            {
                handler(_sender, new SKInstallFinishedEventArgs(currentInstallingItem));
            }
        }
    }

    public class SKDownloadCancelledEventArgs : EventArgs
    {
        public SKDownloadCancelledEventArgs(string currentDownloadItemCode)
        {
            CurrentDownloadItemCode = currentDownloadItemCode;
        }

        public string CurrentDownloadItemCode { get; private set; }
    }

    public class SKDownloadPausedEventArgs : EventArgs
    {
        public SKDownloadPausedEventArgs(SKToolsDownloadItem currentDownloadItem)
        {
            CurrentDownloadItem = currentDownloadItem;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
    }

    public class SKDownloadProgressEventArgs : EventArgs
    {
        public SKDownloadProgressEventArgs(SKToolsDownloadItem currentDownloadItem)
        {
            CurrentDownloadItem = currentDownloadItem;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
    }

    public class SKInstallFinishedEventArgs : EventArgs
    {
        public SKInstallFinishedEventArgs(SKToolsDownloadItem currentDownloadItem)
        {
            CurrentDownloadItem = currentDownloadItem;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
    }

    public class SKInstallStartedEventArgs : EventArgs
    {
        public SKInstallStartedEventArgs(SKToolsDownloadItem currentDownloadItem)
        {
            CurrentDownloadItem = currentDownloadItem;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
    }

    public class SKInternetConnectionFailedEventArgs : EventArgs
    {
        public SKInternetConnectionFailedEventArgs(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
        {
            CurrentDownloadItem = currentDownloadItem;
            ResponseReceivedFromServer = responseReceivedFromServer;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
        public bool ResponseReceivedFromServer { get; private set; }
    }

    public class SKNotEnoughMemoryOnCurrentStorageEventArgs : EventArgs
    {
        public SKNotEnoughMemoryOnCurrentStorageEventArgs(SKToolsDownloadItem currentDownloadItem)
        {
            CurrentDownloadItem = currentDownloadItem;
        }

        public SKToolsDownloadItem CurrentDownloadItem { get; private set; }
    }
}