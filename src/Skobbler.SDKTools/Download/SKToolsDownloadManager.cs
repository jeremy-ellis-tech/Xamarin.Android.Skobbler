using System;
using System.Collections.Generic;
using System.Linq;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadManager
    {

        /// <summary>
        /// Download files extensions
        /// </summary>
        public const string SkmFileExtension = ".skm";

        public const string ZipFileExtension = ".zip";

        public const string TxgFileExtension = ".txg";

        public const string PointExtension = ".";

        /// <summary>
        /// Contains all items that are in download queue
        /// </summary>
        private LinkedList<SKToolsDownloadItem> _queuedDownloads;

        /// <summary>
        /// Current download listener (used to notify the user interface)
        /// </summary>
        private ISKToolsDownloadListener _downloadListener;

        /// <summary>
        /// Current download thread
        /// </summary>
        private SKToolsDownloadPerformer _downloadThread;

        /// <summary>
        /// Single instance for SKToolsDownloadManager reference
        /// </summary>
        private static SKToolsDownloadManager _skToolsDownloadManagerInstance;

        private readonly object _downloadPerformerLock;

        private WeakReference _dispatcher;

        SKToolsDownloadListenerEventDispatcher EventDispatcher
        {
            get
            {
                if (_dispatcher == null || !_dispatcher.IsAlive)
                {
                    var dispatcher = new SKToolsDownloadListenerEventDispatcher(this);
                    SetDownloadListener(dispatcher);
                    _dispatcher = new WeakReference(dispatcher);
                }

                return _dispatcher.Target as SKToolsDownloadListenerEventDispatcher;
            }
        }

        public event EventHandler<SKDownloadCancelledEventArgs> DownloadCancelled
        {
            add { EventDispatcher.DownloadCancelled += value; }
            remove { EventDispatcher.DownloadCancelled -= value; }
        }

        /// <summary>
        /// Constructs an object of SKToolsDownloadManager type </summary>
        /// <param name="downloadListener"> Download listener </param>
        private SKToolsDownloadManager(ISKToolsDownloadListener downloadListener = null)
        {
            _downloadListener = downloadListener;
            _downloadPerformerLock = new object();
        }

        /// <summary>
        /// gets a single instance of SKToolsDownloadManager reference </summary>
        /// <param name="downloadListener"> download listener </param>
        /// <returns> a single instance of SKToolsDownloadManager reference </returns>
        public static SKToolsDownloadManager GetInstance(ISKToolsDownloadListener downloadListener)
        {
            if (_skToolsDownloadManagerInstance == null)
            {
                _skToolsDownloadManagerInstance = new SKToolsDownloadManager(downloadListener);
            }
            else
            {
                _skToolsDownloadManagerInstance.SetDownloadListener(downloadListener);
            }
            return _skToolsDownloadManagerInstance;
        }

        /// <summary>
        /// sets a download listener for download manager component </summary>
        /// <param name="downloadListener"> download listener that will be set </param>
        public virtual void SetDownloadListener(ISKToolsDownloadListener downloadListener)
        {
            _downloadListener = downloadListener;
            if (_downloadThread != null)
            {
                _downloadThread.SetDownloadListener(downloadListener);
            }
        }

        /// <summary>
        /// start download operation </summary>
        /// <param name="downloadItems"> download items that will be added to download queue </param>
        public virtual void StartDownload(IList<SKToolsDownloadItem> downloadItems)
        {
            lock (_downloadPerformerLock)
            {
                if ((_downloadThread == null) || (!_downloadThread.IsAlive))
                {
                    if (_queuedDownloads != null)
                    {
                        _queuedDownloads.Clear();
                    }
                    else
                    {
                        _queuedDownloads = new LinkedList<SKToolsDownloadItem>();
                    }
                    PutAnyPausedItemFirst(downloadItems);
                    foreach (var item in downloadItems)
                    {
                        _queuedDownloads.AddLast(item);
                    }
                    _downloadThread = new SKToolsDownloadPerformer(_queuedDownloads, _downloadListener);
                    _downloadThread.Start();
                }
                else
                {
                    if (_queuedDownloads != null)
                    {
                        downloadItems.Select(x => _queuedDownloads.AddLast(x));
                    }
                }
            }
        }

        /// <summary>
        /// cancels a download item (from download queue) that has a specific code </summary>
        /// <param name="downloadItemCode"> current download item code </param>
        /// <returns> true, if current download is cancelled, false otherwise (because download process is not running) </returns>
        public virtual bool CancelDownload(string downloadItemCode)
        {
            lock (_downloadPerformerLock)
            {
                if ((_downloadThread != null) && _downloadThread.IsAlive)
                {
                    if (_queuedDownloads != null)
                    {
                        SKToolsDownloadItem removedItem = null;
                        foreach (SKToolsDownloadItem currentItem in _queuedDownloads)
                        {
                            if ((currentItem != null) && (currentItem.ItemCode != null) && currentItem.ItemCode.Equals(downloadItemCode))
                            {
                                var currentItemState = currentItem.SKDownloadState;
                                // if the download is already running (cannot cancel an already downloaded, installing or installed map)
                                if ((currentItemState == SKDownloadState.Paused) || (currentItemState == SKDownloadState.Downloading))
                                {
                                    // mark that current download is cancelled, if download thread is running
                                    _downloadThread.SetCurrentDownloadAsCancelled();
                                    return true;
                                }
                                if (currentItemState == SKDownloadState.Queued)
                                {
                                    removedItem = currentItem;
                                    break;
                                }
                            }
                        }
                        if (removedItem != null)
                        {
                            // remove current item from download queue
                            _queuedDownloads.Remove(removedItem);
                            // notify the UI that current download was cancelled
                            if (_downloadListener != null)
                            {
                                _downloadListener.OnDownloadCancelled(removedItem.ItemCode);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// pause download thread </summary>
        /// <returns> true, if download thread is paused, false otherwise (because download process is not running) </returns>
        public virtual bool PauseDownloadThread()
        {
            lock (_downloadPerformerLock)
            {
                // if download thread is alive, stop it
                if ((_downloadThread != null) && _downloadThread.IsAlive)
                {
                    _downloadThread.SetDownloadProcessAsPaused();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// cancel all downloads from download queue </summary>
        /// <returns> true, if download thread is cancelled, false otherwise (because download process is not running) </returns>
        public virtual bool CancelAllDownloads()
        {
            lock (_downloadPerformerLock)
            {
                // if download thread is alive, stop it and return true
                if ((_downloadThread != null) && _downloadThread.IsAlive)
                {
                    _downloadThread.SetDownloadProcessAsCancelled();
                    return true;
                }
                return false;
            }
        }

        /// <returns> true if download process is running, false otherwise </returns>
        public virtual bool DownloadProcessRunning
        {
            get
            {
                lock (_downloadPerformerLock)
                {
                    // if download thread is alive, stop it and return true
                    if ((_downloadThread != null) && _downloadThread.IsAlive)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// put the paused/downloading item at the top of the list </summary>
        /// <param name="downloadItems"> download items that will be added to download queue </param>
        private void PutAnyPausedItemFirst(IList<SKToolsDownloadItem> downloadItems)
        {
            SKToolsDownloadItem downloadingItem = null;
            int downloadingItemIndex = 0;
            foreach (SKToolsDownloadItem currentItem in downloadItems)
            {
                if ((currentItem.SKDownloadState == SKDownloadState.Downloading) || (currentItem.SKDownloadState == SKDownloadState.Paused))
                {
                    downloadingItem = currentItem;
                    break;
                }
                downloadingItemIndex++;
            }
            if (downloadingItem != null)
            {
                downloadItems.RemoveAt(downloadingItemIndex);
                downloadItems.Insert(0, downloadingItem);
            }
        }
    }
}