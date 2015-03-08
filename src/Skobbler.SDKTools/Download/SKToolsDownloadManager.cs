using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadManager
    {

        /// <summary>
        /// download files extensions
        /// </summary>
        public const string SKM_FILE_EXTENSION = ".skm";

        public const string ZIP_FILE_EXTENSION = ".zip";

        public const string TXG_FILE_EXTENSION = ".txg";

        public const string POINT_EXTENSION = ".";

        /// <summary>
        /// contains all items that are in download queue
        /// </summary>
        private LinkedList<SKToolsDownloadItem> queuedDownloads;

        /// <summary>
        /// current download listener (used to notify the user interface)
        /// </summary>
        private ISKToolsDownloadListener downloadListener;

        /// <summary>
        /// current download thread
        /// </summary>
        private SKToolsDownloadPerformer downloadThread;

        /// <summary>
        /// single instance for SKToolsDownloadManager reference
        /// </summary>
        private static SKToolsDownloadManager skToolsDownloadManagerInstance;

        /// <summary>
        /// constructs an object of SKToolsDownloadManager type </summary>
        /// <param name="downloadListener"> download listener </param>
        private SKToolsDownloadManager(ISKToolsDownloadListener downloadListener)
        {
            this.downloadListener = downloadListener;
        }

        /// <summary>
        /// gets a single instance of SKToolsDownloadManager reference </summary>
        /// <param name="downloadListener"> download listener </param>
        /// <returns> a single instance of SKToolsDownloadManager reference </returns>
        public static SKToolsDownloadManager getInstance(ISKToolsDownloadListener downloadListener)
        {
            if (skToolsDownloadManagerInstance == null)
            {
                skToolsDownloadManagerInstance = new SKToolsDownloadManager(downloadListener);
            }
            else
            {
                skToolsDownloadManagerInstance.DownloadListener = downloadListener;
            }
            return skToolsDownloadManagerInstance;
        }

        /// <summary>
        /// sets a download listener for download manager component </summary>
        /// <param name="downloadListener"> download listener that will be set </param>
        public virtual ISKToolsDownloadListener DownloadListener
        {
            set
            {
                this.downloadListener = value;
                if (downloadThread != null)
                {
                    downloadThread.DownloadListener = value;
                }
            }
        }

        /// <summary>
        /// start download operation </summary>
        /// <param name="downloadItems"> download items that will be added to download queue </param>
        public virtual void startDownload(IList<SKToolsDownloadItem> downloadItems)
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                if ((downloadThread == null) || (!downloadThread.IsAlive))
                {
                    if (queuedDownloads != null)
                    {
                        queuedDownloads.Clear();
                    }
                    else
                    {
                        queuedDownloads = new LinkedList<SKToolsDownloadItem>();
                    }
                    putAnyPausedItemFirst(downloadItems);
                    downloadItems.Select(x => queuedDownloads.AddLast(x));
                    downloadThread = new SKToolsDownloadPerformer(queuedDownloads, downloadListener);
                    downloadThread.Start();
                }
                else
                {
                    if (queuedDownloads != null)
                    {
                        downloadItems.Select(x => queuedDownloads.AddLast(x));
                    }
                }
            }
        }

        /// <summary>
        /// cancels a download item (from download queue) that has a specific code </summary>
        /// <param name="downloadItemCode"> current download item code </param>
        /// <returns> true, if current download is cancelled, false otherwise (because download process is not running) </returns>
        public virtual bool cancelDownload(string downloadItemCode)
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                if ((downloadThread != null) && downloadThread.IsAlive)
                {
                    if (queuedDownloads != null)
                    {
                        SKToolsDownloadItem removedItem = null;
                        foreach (SKToolsDownloadItem currentItem in queuedDownloads)
                        {
                            if ((currentItem != null) && (currentItem.ItemCode != null) && currentItem.ItemCode.Equals(downloadItemCode))
                            {
                                sbyte currentItemState = currentItem.DownloadState;
                                // if the download is already running (cannot cancel an already downloaded, installing or installed map)
                                if ((currentItemState == SKToolsDownloadItem.Paused) || (currentItemState == SKToolsDownloadItem.Downloading))
                                {
                                    // mark that current download is cancelled, if download thread is running
                                    downloadThread.setCurrentDownloadAsCancelled();
                                    return true;
                                }
                                else if (currentItemState == SKToolsDownloadItem.Queued)
                                {
                                    removedItem = currentItem;
                                    break;
                                }
                            }
                        }
                        if (removedItem != null)
                        {
                            // remove current item from download queue
                            queuedDownloads.Remove(removedItem);
                            // notify the UI that current download was cancelled
                            if (downloadListener != null)
                            {
                                downloadListener.OnDownloadCancelled(removedItem.ItemCode);
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
        public virtual bool pauseDownloadThread()
        {
            lock ((typeof(SKToolsDownloadPerformer)))
            {
                // if download thread is alive, stop it
                if ((downloadThread != null) && downloadThread.IsAlive)
                {
                    downloadThread.setDownloadProcessAsPaused();
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// cancel all downloads from download queue </summary>
        /// <returns> true, if download thread is cancelled, false otherwise (because download process is not running) </returns>
        public virtual bool cancelAllDownloads()
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                // if download thread is alive, stop it and return true
                if ((downloadThread != null) && downloadThread.IsAlive)
                {
                    downloadThread.setDownloadProcessAsCancelled();
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
                lock (typeof(SKToolsDownloadPerformer))
                {
                    // if download thread is alive, stop it and return true
                    if ((downloadThread != null) && downloadThread.IsAlive)
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
        private void putAnyPausedItemFirst(IList<SKToolsDownloadItem> downloadItems)
        {
            SKToolsDownloadItem downloadingItem = null;
            int downloadingItemIndex = 0;
            foreach (SKToolsDownloadItem currentItem in downloadItems)
            {
                if ((currentItem.DownloadState == SKToolsDownloadItem.Downloading) || (currentItem.DownloadState == SKToolsDownloadItem.Paused))
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