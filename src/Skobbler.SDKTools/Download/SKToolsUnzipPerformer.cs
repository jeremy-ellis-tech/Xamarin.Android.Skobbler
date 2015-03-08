using System;
using System.Collections.Generic;
using Java.IO;
using Java.Lang;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.Util;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsUnzipPerformer : Thread
    {

        /// <summary>
        /// the tag associated with this class, used for debugging
        /// </summary>
        private const string Tag = "SKToolsUnzipPerformer";

        /// <summary>
        /// queued installing items
        /// </summary>
        private LinkedList<SKToolsDownloadItem> _queuedInstallingItems;

        /// <summary>
        /// current installing item
        /// </summary>
        private SKToolsDownloadItem _currentInstallingItem;

        /// <summary>
        /// download listener
        /// </summary>
        private ISKToolsDownloadListener _downloadListener;

        /// <summary>
        /// tells that current install process is paused
        /// </summary>
        private volatile bool _isInstallProcessPaused;

        /// <summary>
        /// creates an object of SKToolsUnzipPerformer type </summary>
        /// <param name="downloadListener"> download listener </param>
        public SKToolsUnzipPerformer(ISKToolsDownloadListener downloadListener)
        {
            lock (typeof(SKToolsUnzipPerformer))
            {
                this._queuedInstallingItems = new LinkedList<SKToolsDownloadItem>();
            }
            this._downloadListener = downloadListener;
        }

        public virtual void SetDownloadListener(ISKToolsDownloadListener value)
        {
            this._downloadListener = value;
        }

        /// <summary>
        /// installs a list of DOWNLOADED resources
        /// </summary>
        public override void Run()
        {
            while (ExistsAnyRemainingInstall())
            {
                if ((_currentInstallingItem == null) || (_queuedInstallingItems == null) || _isInstallProcessPaused)
                {
                    break;
                }
                string filePath = _currentInstallingItem.CurrentStepDestinationPath;
                SKLogging.WriteLog(Tag, "The path of the file that must be installed = " + filePath, SKLogging.LogDebug);
                bool zipFileExists = false;
                File zipFile = null;
                string rootFilePath = null;
                if (filePath != null)
                {
                    zipFile = new File(filePath);
                    zipFileExists = zipFile.Exists();
                    rootFilePath = filePath.Substring(0, filePath.IndexOf((new StringBuilder(_currentInstallingItem.ItemCode)).Append(SKToolsDownloadManager.PointExtension).ToString(), StringComparison.Ordinal));
                }
                if (zipFileExists)
                {
                    // change the state for current download item
                    _currentInstallingItem.DownloadState = SKToolsDownloadItem.Installing;

                    // database and UI update
                    if (_downloadListener != null)
                    {
                        _downloadListener.OnInstallStarted(_currentInstallingItem);
                    }

                    SKLogging.WriteLog(Tag, "Start unzipping file with path = " + filePath, SKLogging.LogDebug);
                    SKMaps.Instance.UnzipFile(zipFile.AbsolutePath, rootFilePath);
                    SKLogging.WriteLog(Tag, "Unzip finished. Start installing current resource (performed by NG library)", SKLogging.LogDebug);

                    if (_isInstallProcessPaused)
                    {
                        SKLogging.WriteLog(Tag, "Install was not finalized, because install process was stopped by client", SKLogging.LogDebug);
                        break;
                    }

                    if (_currentInstallingItem.InstallOperationIsNeeded)
                    {
                        int result = SKPackageManager.Instance.AddOfflinePackage(rootFilePath, _currentInstallingItem.ItemCode);
                        SKLogging.WriteLog(Tag, "Current resource installing result code = " + result, SKLogging.LogDebug);
                        if ((result & SKPackageManager.AddPackageMissingSkmResult & SKPackageManager.AddPackageMissingNgiResult & SKPackageManager.AddPackageMissingNgiDatResult) == 0)
                        {
                            // current install was performed with success set current resource as already download
                            _currentInstallingItem.DownloadState = SKToolsDownloadItem.Installed;
                            SKLogging.WriteLog(Tag, "The " + _currentInstallingItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                            // notify the UI that current resource was installed
                            if (_downloadListener != null)
                            {
                                _downloadListener.OnInstallFinished(_currentInstallingItem);
                            }
                        }
                        else
                        {
                            // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                            _currentInstallingItem.MarkAsNotQueued();
                            SKLogging.WriteLog(Tag, "The " + _currentInstallingItem.ItemCode + " resource couldn't be installed by our NG component,although it was downloaded.", SKLogging.LogDebug);
                            // notify the UI that current resource was not installed
                            if (_downloadListener != null)
                            {
                                _downloadListener.OnDownloadProgress(_currentInstallingItem);
                            }
                        }
                    }
                    else
                    {
                        // current install was performed with success set current resource as already download
                        _currentInstallingItem.DownloadState = SKToolsDownloadItem.Installed;
                        SKLogging.WriteLog(Tag, "The " + _currentInstallingItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                        // notify the UI that current resource was installed
                        if (_downloadListener != null)
                        {
                            _downloadListener.OnInstallFinished(_currentInstallingItem);
                        }
                    }
                    // remove current ZIP file from device
                    SKToolsDownloadUtils.RemoveCurrentLocationFromDisk(filePath);
                }
                else
                {
                    SKLogging.WriteLog(Tag, "The zip file doesn't exist => download again the resource !!! " + filePath, SKLogging.LogDebug);
                    // prepare again current resource for download queue(change its state, remove all related downloaded bytes)
                    _currentInstallingItem.MarkAsNotQueued();
                    _currentInstallingItem.DownloadState = SKToolsDownloadItem.Queued;

                    // notify the UI that current resource is again put in download queue
                    if (_downloadListener != null)
                    {
                        _downloadListener.OnDownloadProgress(_currentInstallingItem);
                    }

                    // add again the resource in download queue
                    IList<SKToolsDownloadItem> downloadItems = new List<SKToolsDownloadItem>();
                    downloadItems.Add(_currentInstallingItem);
                    SKToolsDownloadManager.GetInstance(_downloadListener).StartDownload(downloadItems);
                }
                // remove current download from download queue
                lock (typeof(SKToolsUnzipPerformer))
                {
                    if (_queuedInstallingItems != null)
                    {
                        _queuedInstallingItems.RemoveFirst();
                    }
                }
            }
            SKLogging.WriteLog(Tag, "The install thread has stopped", SKLogging.LogDebug);
        }

        /// <summary>
        /// adds downloaded item for install (in the install queue) </summary>
        /// <param name="currentItem"> current item </param>
        public virtual void AddItemForInstall(SKToolsDownloadItem currentItem)
        {
            this._queuedInstallingItems.AddLast(currentItem);
        }

        /// <summary>
        /// stops the install process
        /// </summary>
        public virtual void StopInstallProcess()
        {
            _isInstallProcessPaused = true;
        }

        /// <summary>
        /// checks if there is any remaining item to install </summary>
        /// <returns> true, if there is any remaining item to install, false otherwise </returns>
        private bool ExistsAnyRemainingInstall()
        {
            lock (typeof(SKToolsUnzipPerformer))
            {
                if ((_queuedInstallingItems != null) && _queuedInstallingItems.Count > 0)
                {
                    _currentInstallingItem = _queuedInstallingItems.First.Value;
                    if (_currentInstallingItem != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}