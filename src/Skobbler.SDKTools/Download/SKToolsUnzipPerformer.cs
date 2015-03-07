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
using Java.Lang;
using Skobbler.Ngx.Util;
using Java.IO;
using Skobbler.Ngx.Packages;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsUnzipPerformer : Thread
    {
        private static readonly string Tag = "SKToolsUnzipPerformer";

        private Queue<SKToolsDownloadItem> _queuedInstallingItems;
        private SKToolsDownloadItem _currentInstallingItem;
        private ISKToolsDownloadListener _downloadListener;
        private volatile bool _isInstallProcessPaused;

        private readonly object _lock = new object();

        public SKToolsUnzipPerformer(ISKToolsDownloadListener downloadListener)
        {
            lock (_lock)
            {
                _queuedInstallingItems = new Queue<SKToolsDownloadItem>();
            }

            _downloadListener = downloadListener;
        }

        public void SetDownloadListener(ISKToolsDownloadListener downloadListener)
        {
            _downloadListener = downloadListener;
        }

        public override void Run()
        {
            while (ExistsAnyRemainingInstall())
            {
                if(_currentInstallingItem == null || _queuedInstallingItems == null || _isInstallProcessPaused)
                {
                    break;
                }

                string filePath = _currentInstallingItem.CurrentStepDestinationPath();

                SKLogging.WriteLog(Tag, "The path of the file that must be installed = " + filePath, SKLogging.LogDebug);

                bool zipfileExists = false;

                File zipFile = null;
                string rootFilePath = null;

                if(filePath != null)
                {
                    zipFile = new File(filePath);
                    zipfileExists = zipFile.Exists();
                    rootFilePath = filePath.Substring(0, filePath.IndexOf(new StringBuilder(_currentInstallingItem.ItemCode).append(SKToolsDownloadManager.POINT_EXTENSION).ToString()));
                }

                if(zipfileExists)
                {
                    _currentInstallingItem.DownloadState = SKToolsDownloadItem.Installing;

                    if(_downloadListener != null)
                    {
                        _downloadListener.OnInstallStarted(_currentInstallingItem);
                    }

                    SKLogging.WriteLog(Tag, "Start unzipping file with path = " + filePath, SKLogging.LogDebug);
                    SKMaps.Instance.UnzipFile(zipFile.AbsolutePath, rootFilePath);
                    SKLogging.WriteLog(Tag, "Unzip finished. Start installing current resource (performed by NG library)", SKLogging.LogDebug);

                    if(_isInstallProcessPaused)
                    {
                        SKLogging.WriteLog(Tag, "Install was not finalized, because install process was stopped by client", SKLogging.LogDebug);
                        break;
                    }

                    if(_currentInstallingItem.IsInstallOperationNeeded)
                    {
                        int result = SKPackageManager.Instance.AddOfflinePackage(rootFilePath, _currentInstallingItem.ItemCode);
                        SKLogging.WriteLog(Tag, "Current resource installing result code = " + result, SKLogging.LogDebug);

                        if((result & SKPackageManager.AddPackageMissingSkmResult & SKPackageManager.AddPackageMissingNgiResult & SKPackageManager.AddPackageMissingNgiDatResult) == 0)
                        {
                            _currentInstallingItem.DownloadState = SKToolsDownloadItem.Installed;
                            SKLogging.WriteLog(Tag, "The " + _currentInstallingItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);

                            if(_downloadListener != null)
                            {
                                _downloadListener.OnInstallFinished(_currentInstallingItem);
                            }
                        }
                        else
	{
                            _currentInstallingItem.MarkAsNotQueued;
                            SKLogging.writeLog(TAG, "The " + currentInstallingItem.getItemCode() + " resource couldn't be installed by our NG component,although it was downloaded.",
                                SKLogging.LOG_DEBUG);
                        // notify the UI that current resource was not installed
                        if (downloadListener != null) {
                            downloadListener.onDownloadProgress(currentInstallingItem);
                        }
	}
                        }
                    else
	else {
                    // current install was performed with success set current resource as already download
                    currentInstallingItem.setDownloadState(SKToolsDownloadItem.INSTALLED);
                    SKLogging.writeLog(TAG, "The " + currentInstallingItem.getItemCode() + " resource was successfully downloaded and installed by our NG component.",
                            SKLogging.LOG_DEBUG);
                    // notify the UI that current resource was installed
                    if (downloadListener != null) {
                        downloadListener.onInstallFinished(currentInstallingItem);
                    }
                }
                // remove current ZIP file from device
                SKToolsDownloadUtils.removeCurrentLocationFromDisk(filePath);
            } else {
                SKLogging.writeLog(TAG, "The zip file doesn't exist => download again the resource !!! " + filePath, Log.DEBUG);
                // prepare again current resource for download queue(change its state, remove all related downloaded bytes)
                currentInstallingItem.markAsNotQueued();
                currentInstallingItem.setDownloadState(SKToolsDownloadItem.QUEUED);

                // notify the UI that current resource is again put in download queue
                if (downloadListener != null) {
                    downloadListener.onDownloadProgress(currentInstallingItem);
                }

                // add again the resource in download queue
                List<SKToolsDownloadItem> downloadItems = new ArrayList<SKToolsDownloadItem>();
                downloadItems.add(currentInstallingItem);
                SKToolsDownloadManager.getInstance(downloadListener).startDownload(downloadItems);
            }
            // remove current download from download queue
            synchronized (SKToolsUnzipPerformer.class) {
                if (queuedInstallingItems != null) {
                    queuedInstallingItems.poll();
                }
            }
        }
        SKLogging.writeLog(TAG, "The install thread has stopped", SKLogging.LOG_DEBUG);
        }

        private bool ExistsAnyRemainingInstall()
        {
            lock (_lock)
            {
                if(_queuedInstallingItems != null && !(_queuedInstallingItems.Count == 0))
                {
                    _currentInstallingItem = _queuedInstallingItems.Peek();

                    if(_currentInstallingItem != null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void AddItemForInstall(SKToolsDownloadItem currentItem)
        {
            _queuedInstallingItems.Enqueue(currentItem);
        }

        public void StopInstallProcess()
        {
            _isInstallProcessPaused = true;
        }
    }
}