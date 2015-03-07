using Java.IO;
using Java.Lang;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.Util;
using System;
using System.Collections.Generic;
using StringBuilder = Java.Lang.StringBuilder;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsUnzipPerformer : Thread
    {

        /// <summary>
        /// the tag associated with this class, used for debugging
        /// </summary>
        private const string TAG = "SKToolsUnzipPerformer";

        /// <summary>
        /// queued installing items
        /// </summary>
        private LinkedList<SKToolsDownloadItem> queuedInstallingItems;

        /// <summary>
        /// current installing item
        /// </summary>
        private SKToolsDownloadItem currentInstallingItem;

        /// <summary>
        /// download listener
        /// </summary>
        private ISKToolsDownloadListener downloadListener;

        /// <summary>
        /// tells that current install process is paused
        /// </summary>
        private volatile bool isInstallProcessPaused;

        /// <summary>
        /// creates an object of SKToolsUnzipPerformer type </summary>
        /// <param name="downloadListener"> download listener </param>
        public SKToolsUnzipPerformer(ISKToolsDownloadListener downloadListener)
        {
            lock (typeof(SKToolsUnzipPerformer))
            {
                this.queuedInstallingItems = new LinkedList<SKToolsDownloadItem>();
            }
            this.downloadListener = downloadListener;
        }

        public virtual ISKToolsDownloadListener DownloadListener
        {
            set
            {
                this.downloadListener = value;
            }
        }

        /// <summary>
        /// installs a list of DOWNLOADED resources
        /// </summary>
        public override void Run()
        {
            while (existsAnyRemainingInstall())
            {
                if ((currentInstallingItem == null) || (queuedInstallingItems == null) || isInstallProcessPaused)
                {
                    break;
                }
                string filePath = currentInstallingItem.CurrentStepDestinationPath;
                SKLogging.WriteLog(TAG, "The path of the file that must be installed = " + filePath, SKLogging.LogDebug);
                bool zipFileExists = false;
                File zipFile = null;
                string rootFilePath = null;
                if (filePath != null)
                {
                    zipFile = new File(filePath);
                    zipFileExists = zipFile.Exists();
                    rootFilePath = filePath.Substring(0, filePath.IndexOf((new StringBuilder(currentInstallingItem.ItemCode)).Append(SKToolsDownloadManager.POINT_EXTENSION).ToString(), StringComparison.Ordinal));
                }
                if (zipFileExists)
                {
                    // change the state for current download item
                    currentInstallingItem.DownloadState = SKToolsDownloadItem.INSTALLING;

                    // database and UI update
                    if (downloadListener != null)
                    {
                        downloadListener.OnInstallStarted(currentInstallingItem);
                    }

                    SKLogging.WriteLog(TAG, "Start unzipping file with path = " + filePath, SKLogging.LogDebug);
                    SKMaps.Instance.UnzipFile(zipFile.AbsolutePath, rootFilePath);
                    SKLogging.WriteLog(TAG, "Unzip finished. Start installing current resource (performed by NG library)", SKLogging.LogDebug);

                    if (isInstallProcessPaused)
                    {
                        SKLogging.WriteLog(TAG, "Install was not finalized, because install process was stopped by client", SKLogging.LogDebug);
                        break;
                    }

                    if (currentInstallingItem.InstallOperationIsNeeded)
                    {
                        int result = SKPackageManager.Instance.AddOfflinePackage(rootFilePath, currentInstallingItem.ItemCode);
                        SKLogging.WriteLog(TAG, "Current resource installing result code = " + result, SKLogging.LogDebug);
                        if ((result & SKPackageManager.AddPackageMissingSkmResult & SKPackageManager.AddPackageMissingNgiResult & SKPackageManager.AddPackageMissingNgiDatResult) == 0)
                        {
                            // current install was performed with success set current resource as already download
                            currentInstallingItem.DownloadState = SKToolsDownloadItem.INSTALLED;
                            SKLogging.WriteLog(TAG, "The " + currentInstallingItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                            // notify the UI that current resource was installed
                            if (downloadListener != null)
                            {
                                downloadListener.OnInstallFinished(currentInstallingItem);
                            }
                        }
                        else
                        {
                            // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                            currentInstallingItem.markAsNotQueued();
                            SKLogging.WriteLog(TAG, "The " + currentInstallingItem.ItemCode + " resource couldn't be installed by our NG component,although it was downloaded.", SKLogging.LogDebug);
                            // notify the UI that current resource was not installed
                            if (downloadListener != null)
                            {
                                downloadListener.OnDownloadProgress(currentInstallingItem);
                            }
                        }
                    }
                    else
                    {
                        // current install was performed with success set current resource as already download
                        currentInstallingItem.DownloadState = SKToolsDownloadItem.INSTALLED;
                        SKLogging.WriteLog(TAG, "The " + currentInstallingItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                        // notify the UI that current resource was installed
                        if (downloadListener != null)
                        {
                            downloadListener.OnInstallFinished(currentInstallingItem);
                        }
                    }
                    // remove current ZIP file from device
                    SKToolsDownloadUtils.removeCurrentLocationFromDisk(filePath);
                }
                else
                {
                    SKLogging.WriteLog(TAG, "The zip file doesn't exist => download again the resource !!! " + filePath, SKLogging.LogDebug);
                    // prepare again current resource for download queue(change its state, remove all related downloaded bytes)
                    currentInstallingItem.markAsNotQueued();
                    currentInstallingItem.DownloadState = SKToolsDownloadItem.QUEUED;

                    // notify the UI that current resource is again put in download queue
                    if (downloadListener != null)
                    {
                        downloadListener.OnDownloadProgress(currentInstallingItem);
                    }

                    // add again the resource in download queue
                    IList<SKToolsDownloadItem> downloadItems = new List<SKToolsDownloadItem>();
                    downloadItems.Add(currentInstallingItem);
                    SKToolsDownloadManager.getInstance(downloadListener).startDownload(downloadItems);
                }
                // remove current download from download queue
                lock (typeof(SKToolsUnzipPerformer))
                {
                    if (queuedInstallingItems != null)
                    {
                        queuedInstallingItems.RemoveFirst();
                    }
                }
            }
            SKLogging.WriteLog(TAG, "The install thread has stopped", SKLogging.LogDebug);
        }

        /// <summary>
        /// adds downloaded item for install (in the install queue) </summary>
        /// <param name="currentItem"> current item </param>
        public virtual void addItemForInstall(SKToolsDownloadItem currentItem)
        {
            this.queuedInstallingItems.AddLast(currentItem);
        }

        /// <summary>
        /// stops the install process
        /// </summary>
        public virtual void stopInstallProcess()
        {
            isInstallProcessPaused = true;
        }

        /// <summary>
        /// checks if there is any remaining item to install </summary>
        /// <returns> true, if there is any remaining item to install, false otherwise </returns>
        private bool existsAnyRemainingInstall()
        {
            lock (typeof(SKToolsUnzipPerformer))
            {
                if ((queuedInstallingItems != null) && queuedInstallingItems.Count > 0)
                {
                    currentInstallingItem = queuedInstallingItems.First.Value;
                    if (currentInstallingItem != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}