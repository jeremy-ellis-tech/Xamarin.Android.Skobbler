using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Java.IO;
using Java.Lang;
using Java.Net;
using Org.Apache.Http.Client.Methods;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.SDKTools.Extensions;
using Skobbler.Ngx.Util;
using Exception = System.Exception;
using FileNotFoundException = System.IO.FileNotFoundException;
using IOException = System.IO.IOException;
using StringBuilder = System.Text.StringBuilder;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadPerformer : Thread
    {

        /// <summary>
        /// the tag associated with this class, used for debugging
        /// </summary>
        public const string Tag = "SKToolsDownloadPerformer";

        /// <summary>
        /// Key for HTTP header request property to send the DOWNLOADED bytes
        /// </summary>
        private const string HttpPropRange = "Range";

        /// <summary>
        /// represents the number of bytes from one megabyte
        /// </summary>
        private const int NoBytesIntoOneMb = 1048576;

        /// <summary>
        /// the timeout limit for the edge cases requests
        /// </summary>
        private const int TimeOutLimitForEdgeCases = 20000;

        /// <summary>
        /// represents the number of milliseconds from one second
        /// </summary>
        private const int NoMillisIntoOneSec = 1000;

        /// <summary>
        /// the timeout limit for the requests
        /// </summary>
        private const int TimeOutLimit = 15000;

        /// <summary>
        /// true, if current download is cancelled while download thread is running
        /// </summary>
        private volatile bool _isCurrentDownloadCancelled;

        /// <summary>
        /// true, if download process is cancelled
        /// </summary>
        private volatile bool _isDownloadProcessCancelled;

        /// <summary>
        /// true, if download request doesn't respond
        /// </summary>
        private volatile bool _isDownloadRequestUnresponsive;

        /// <summary>
        /// tells that current download process is paused
        /// </summary>
        private volatile bool _isDownloadProcessPaused;

        /// <summary>
        /// queued downloads
        /// </summary>
        private LinkedList<SKToolsDownloadItem> _queuedDownloads;

        /// <summary>
        /// download listener
        /// </summary>
        private ISKToolsDownloadListener _downloadListener;

        /// <summary>
        /// current download step
        /// </summary>
        private SKToolsFileDownloadStep _currentDownloadStep;

        /// <summary>
        /// current download item
        /// </summary>
        private SKToolsDownloadItem _currentDownloadItem;

        // WifiLock used for long running downloads
        private WifiManager.WifiLock _wifiLock;

        /// <summary>
        /// instance of HttpClient used for download
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// current HTTP request
        /// </summary>
        private HttpRequestBase _httpRequest;

        /// <summary>
        /// download timeout handler ; added for the edge cases (networks on which HttpClient blocks)
        /// </summary>
        private Handler _downloadTimeoutHandler;

        /// <summary>
        /// runs when download request cannot return a response, after a while
        /// </summary>
        private Action _downloadTimeoutAction;

        /// <summary>
        /// time at first retry
        /// </summary>
        private long _timeAtFirstRetry;

        /// <summary>
        /// true, if any retry was made (reset after an INTERNET connection is received)
        /// </summary>
        private volatile bool _anyRetryMade;

        /// <summary>
        /// last time when the INTERNET was working
        /// </summary>
        private long _lastTimeWhenInternetWorked;

        /// <summary>
        /// SK-TOOLS unzip performer
        /// </summary>
        private SKToolsUnzipPerformer _skToolsUnzipPerformer;

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

        public event EventHandler<SKDownloadProgressEventArgs> DownloadProgress
        {
            add { EventDispatcher.DownloadProgress += value; }
            remove { EventDispatcher.DownloadProgress -= value; }
        }

        public event EventHandler<SKDownloadCancelledEventArgs> DownloadCancelled
        {
            add { EventDispatcher.DownloadCancelled += value; }
            remove { EventDispatcher.DownloadCancelled -= value; }
        }

        public event EventHandler<SKDownloadPausedEventArgs> DownloadPaused
        {
            add { EventDispatcher.DownloadPaused += value; }
            remove { EventDispatcher.DownloadPaused -= value; }
        }

        public event EventHandler<SKInternetConnectionFailedEventArgs> InternetConnectionFailed
        {
            add { EventDispatcher.InternetConnectionFailed += value; }
            remove { EventDispatcher.InternetConnectionFailed -= value; }
        }

        public event EventHandler AllDownloadsCancelled
        {
            add { EventDispatcher.AllDownloadsCancelled += value; }
            remove { EventDispatcher.AllDownloadsCancelled -= value; }
        }

        public event EventHandler<SKNotEnoughMemoryOnCurrentStorageEventArgs> NotEnoughMemoryOnCurrentStorage
        {
            add { EventDispatcher.NotEnoughMemoryOnCurrentStorage += value; }
            remove { EventDispatcher.NotEnoughMemoryOnCurrentStorage -= value; }
        }

        public event EventHandler<SKInstallStartedEventArgs> InstallStarted
        {
            add { EventDispatcher.InstallStarted += value; }
            remove { EventDispatcher.InstallStarted -= value; }
        }

        public event EventHandler<SKInstallFinishedEventArgs> InstallFinished
        {
            add { EventDispatcher.InstallFinished += value; }
            remove { EventDispatcher.InstallFinished -= value; }
        } 

        /// <summary>
        /// creates an object of SKToolsDownloadPerformer type </summary>
        /// <param name="queuedDownloads"> queued downloads </param>
        /// <param name="downloadListener"> download listener </param>
        public SKToolsDownloadPerformer(LinkedList<SKToolsDownloadItem> queuedDownloads, ISKToolsDownloadListener downloadListener)
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                _queuedDownloads = queuedDownloads;
            }
            _downloadListener = downloadListener;
        }

        public virtual void SetDownloadListener(ISKToolsDownloadListener downloadListener)
        {
            _downloadListener = downloadListener;
            if (_skToolsUnzipPerformer != null)
            {
                _skToolsUnzipPerformer.SetDownloadListener(downloadListener);
            }
        }

        public override void Run()
        {
            // current input stream
            Stream responseStream = null;
            // bytes read during current INTERNET connection
            long bytesReadInThisConnection = 0;
            // time of the download during current INTERNET connection
            long lastDownloadProgressTime = 0;

            _anyRetryMade = false;
            _timeAtFirstRetry = 0;
            _lastTimeWhenInternetWorked = DateTimeOffset.Now.CurrentTimeMillis();

            InitializeResourcesWhenDownloadThreadStarts();

            while (ExistsAnyRemainingDownload())
            {
                if ((_currentDownloadItem == null) || (_queuedDownloads == null) || (_currentDownloadStep == null) || (_currentDownloadStep.DestinationPath == null) || (_currentDownloadStep.DownloadURL == null))
                {
                    break;
                }

                // change the state for current download item
                if (_currentDownloadItem.SKDownloadState != SKDownloadState.Downloading)
                {
                    _currentDownloadItem.SKDownloadState = SKDownloadState.Downloading;
                }

                // check if current item is already downloaded (could be the case when download is performed very slow and the user exists the download thread without finishing the
                // download, but with the file downloaded)
                if (CurrentItemFullyDownloaded)
                {
                    try
                    {
                        FinishCurrentDownload();
                    }
                    catch (SocketException)
                    {
                        // restart the download in this case
                        SKLogging.WriteLog(Tag, "Not possible, because in this case the download wouldn't appear to be finished => restart the download and remove the old data!!!", SKLogging.LogDebug);
                        // reset the number of downloaded bytes and remove them from storage
                        _currentDownloadItem.NoDownloadedBytes = 0;
                        string deleteCmd = "rm -r " + _currentDownloadItem.CurrentStepDestinationPath;
                        Runtime runtime = Runtime.GetRuntime();
                        try
                        {
                            runtime.Exec(deleteCmd);
                            SKLogging.WriteLog(Tag, "The file was deleted from its current installation folder", SKLogging.LogDebug);
                        }
                        catch (IOException)
                        {
                            SKLogging.WriteLog(Tag, "The file couldn't be deleted !!!", SKLogging.LogDebug);
                        }
                    }
                }
                else
                {
                    // create a new download request
                    _httpRequest = new HttpGet(_currentDownloadStep.DownloadURL);

                    SKLogging.WriteLog(Tag, "Current url = " + _currentDownloadStep.DownloadURL + " ; current step = " + _currentDownloadItem.CurrentStepIndex, SKLogging.LogDebug);

                    // resume operation => send already downloaded bytes
                    var bytesReadSoFar = SendAlreadyDownloadedBytes();

                    // check if exists any free memory and return if there is not enough memory for this download
                    var memoryNeeded = GetNeededMemoryForCurrentDownload(bytesReadSoFar);
                    if (memoryNeeded != 0)
                    {
                        SKLogging.WriteLog(Tag, "Not enough memory on current storage", SKLogging.LogDebug);
                        PauseDownloadProcess();
                        // notify the UI about memory issues
                        if (_downloadListener != null)
                        {
                            _downloadListener.OnNotEnoughMemoryOnCurrentStorage(_currentDownloadItem);
                        }
                        break;
                    }

                    // database and UI update
                    if (_downloadListener != null)
                    {
                        _downloadListener.OnDownloadProgress(_currentDownloadItem);
                    }

                    try
                    {
                        // starts the timeout handler
                        startsDownloadTimeoutHandler();
                        // executes the download request
                        HttpResponseMessage response = null;
                        if (response == null)
                        {
                            throw new SocketException();
                        }
                        _anyRetryMade = false;
                        HttpResponseMessage entity = null; //response.Entity;
                        int statusCode = 0; // response.StatusLine.StatusCode;
                        // if other status code than 200 or 206(partial download) throw exception
                        if (statusCode != (int)HttpURLConnection.HttpOk && statusCode != (int)HttpURLConnection.HttpPartial)
                        {
                            SKLogging.WriteLog(Tag, "Wrong status code returned !", SKLogging.LogDebug);
                            throw new IOException("HTTP response code: " + statusCode);
                        }
                        // stops the timeout handler
                        stopsDownloadTimeoutHandler();
                        SKLogging.WriteLog(Tag, "Correct response status code returned !", SKLogging.LogDebug);
                        try
                        {
                            if (entity != null)
                            {
                                responseStream = null; //entity.Content;
                            }
                        }
                        catch (IllegalStateException)
                        {
                            SKLogging.WriteLog(Tag, "The returned response content is not correct !", SKLogging.LogDebug);
                        }
                        if (responseStream == null)
                        {
                            SKLogging.WriteLog(Tag, "Response stream is null !!!", SKLogging.LogDebug);
                        }

                        // create a byte array buffer of 1Mb
                        var data = new byte[NoBytesIntoOneMb];
                        // creates the randomAccessFile - if exists it opens it
                        RandomAccessFile localFile = new RandomAccessFile(_currentDownloadStep.DestinationPath, "rw");
                        bytesReadSoFar = localFile.Length();
                        // position in the file
                        localFile.Seek(bytesReadSoFar);
                        while (true)
                        {
                            // starts the timeout handler
                            startsDownloadTimeoutHandler();
                            // reads 1 MB data
                            int bytesReadThisTime = (responseStream != null) ? responseStream.Read(data, 0, data.Length) : 0;
                            // stops the timeout handler
                            stopsDownloadTimeoutHandler();
                            // check number of read bytes
                            if (bytesReadThisTime > 0)
                            {
                                _lastTimeWhenInternetWorked = DateTimeOffset.Now.CurrentTimeMillis();
                                bytesReadSoFar += bytesReadThisTime;
                                _currentDownloadItem.NoDownloadedBytes = bytesReadSoFar;
                                bytesReadInThisConnection += bytesReadThisTime;
                                _currentDownloadItem.SetNoDownloadedBytesInThisConnection(bytesReadInThisConnection);
                                // write the chunk of data in the file
                                localFile.Write(data, 0, bytesReadThisTime);
                                long newTime = DateTimeOffset.Now.CurrentTimeMillis();
                                // notify the UI every second
                                if ((newTime - lastDownloadProgressTime) > NoMillisIntoOneSec)
                                {
                                    lastDownloadProgressTime = newTime;
                                    if (_downloadListener != null)
                                    {
                                        _downloadListener.OnDownloadProgress(_currentDownloadItem);
                                    }
                                }
                            }
                            else if (bytesReadThisTime == -1)
                            {
                                SKLogging.WriteLog(Tag, "No more data to read, so exit !", SKLogging.LogDebug);
                                // if no more data to read, exit
                                break;
                            }
                            if (_isCurrentDownloadCancelled || _isDownloadProcessCancelled || _isDownloadRequestUnresponsive || _isDownloadProcessPaused)
                            {
                                break;
                            }
                        }
                        localFile.Close();
                        responseStream = null;

                        if (!_isDownloadRequestUnresponsive)
                        {
                            if (_isDownloadProcessCancelled)
                            {
                                CancelDownloadProcess();
                            }
                            else if (_isCurrentDownloadCancelled)
                            {
                                CancelCurrentDownload();
                            }
                            else if (_isDownloadProcessPaused)
                            {
                                PauseDownloadProcess();
                            }
                            else
                            {
                                FinishCurrentDownload();
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        SKLogging.WriteLog(Tag, "Socket Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!_isDownloadRequestUnresponsive)
                        {
                            StopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (UnknownHostException e)
                    {
                        SKLogging.WriteLog(Tag, "Unknown Host Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!_isDownloadRequestUnresponsive)
                        {
                            StopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (IOException e)
                    {
                        SKLogging.WriteLog(Tag, "IO Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!_isDownloadRequestUnresponsive)
                        {
                            StopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        SKLogging.WriteLog(Tag, "Index Out Of Bounds Exception ; " + e.Message, SKLogging.LogDebug);
                        stopsDownloadTimeoutHandler();
                    }
                }
            }
            ReleaseResourcesWhenDownloadThreadFinishes();
            SKLogging.WriteLog(Tag, "The download thread has stopped", SKLogging.LogDebug);
        }

        /// <summary>
        /// set current download as cancelled
        /// </summary>
        public virtual void SetCurrentDownloadAsCancelled()
        {
            _isCurrentDownloadCancelled = true;
        }

        /// <summary>
        /// sets download process as cancelled
        /// </summary>
        public virtual void SetDownloadProcessAsCancelled()
        {
            _isDownloadProcessCancelled = true;
        }

        /// <summary>
        /// sets download process as paused
        /// </summary>
        public virtual void SetDownloadProcessAsPaused()
        {
            _isDownloadProcessPaused = true;
        }

        /// <summary>
        /// send the number of bytes already DOWNLOADED - for the resume operation
        /// </summary>
        private long SendAlreadyDownloadedBytes()
        {
            long bytesRead;
            try
            {
                RandomAccessFile destinationFile = new RandomAccessFile(_currentDownloadItem.CurrentStepDestinationPath, "r");
                bytesRead = destinationFile.Length();
                if (bytesRead > 0)
                {
                    SKLogging.WriteLog(Tag, "There are some bytes at this path ; number of downloaded bytes for current resource is " + _currentDownloadItem.NoDownloadedBytes + " ; download step = " + _currentDownloadItem.CurrentStepIndex + " ; current path = " + _currentDownloadItem.CurrentStepDestinationPath, SKLogging.LogDebug);
                    if (_currentDownloadItem.NoDownloadedBytes == 0)
                    {
                        SKLogging.WriteLog(Tag, "There remained some resources with the same name at the same path ! Try to delete the file " + _currentDownloadItem.CurrentStepDestinationPath, SKLogging.LogDebug);
                        string deleteCmd = "rm -r " + _currentDownloadItem.CurrentStepDestinationPath;
                        Runtime runtime = Runtime.GetRuntime();
                        try
                        {
                            runtime.Exec(deleteCmd);
                            SKLogging.WriteLog(Tag, "The file was deleted from its current installation folder", SKLogging.LogDebug);
                        }
                        catch (IOException)
                        {
                            SKLogging.WriteLog(Tag, "The file couldn't be deleted !!!", SKLogging.LogDebug);
                        }
                        bytesRead = 0;
                    }
                    else
                    {
                        SKLogging.WriteLog(Tag, "Current resource is only partially downloaded, so its download will continue = ", SKLogging.LogDebug);
                        _httpRequest.AddHeader(HttpPropRange, "bytes=" + bytesRead + "-");
                    }
                }
                destinationFile.Close();
            }
            catch (FileNotFoundException)
            {
                bytesRead = 0;
            }
            catch (IOException)
            {
                bytesRead = 0;
            }

            return bytesRead;
        }

        /// <summary>
        /// Gets needed memory for current download
        /// </summary>
        /// <param name="bytesRead"> bytes already read </param>
        private long GetNeededMemoryForCurrentDownload(long bytesRead)
        {
            long neededSize = _currentDownloadItem.RemainingSize - bytesRead;
            string filePath = _currentDownloadItem.CurrentStepDestinationPath;
            string basePath = filePath.Substring(0, filePath.IndexOf((new StringBuilder(_currentDownloadItem.ItemCode)).Append(SKToolsDownloadManager.PointExtension).ToString(), StringComparison.Ordinal));
            long memoryNeeded = SKToolsDownloadUtils.GetNeededBytesForADownload(neededSize, basePath);
            SKLogging.WriteLog(Tag, "Memory needed = " + memoryNeeded, SKLogging.LogDebug);
            return memoryNeeded;
        }

        /// <summary>
        /// Checks if there is any remaining item to download
        /// </summary>
        /// <returns> True, if there is any remaining item to download, false otherwise </returns>
        private bool ExistsAnyRemainingDownload()
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                if ((_queuedDownloads != null) && _queuedDownloads.Count > 0)
                {
                    _currentDownloadItem = _queuedDownloads.First.Value;
                    while (_currentDownloadItem != null)
                    {
                        if ((_currentDownloadItem.SKDownloadState == SKDownloadState.Installing) || (_currentDownloadItem.SKDownloadState == SKDownloadState.NotQueued) || (_currentDownloadItem.SKDownloadState == SKDownloadState.Installed))
                        {
                            if (_currentDownloadItem.SKDownloadState == SKDownloadState.Installing)
                            {
                                SKLogging.WriteLog(Tag, "Current download item = " + _currentDownloadItem.ItemCode + " is in INSTALLING state => add it to install queue", SKLogging.LogDebug);
                                // add current resource to install queue
                                lock (typeof(SKToolsUnzipPerformer))
                                {
                                    if ((_skToolsUnzipPerformer == null) || (!_skToolsUnzipPerformer.IsAlive))
                                    {
                                        _skToolsUnzipPerformer = new SKToolsUnzipPerformer(_downloadListener);
                                        _skToolsUnzipPerformer.AddItemForInstall(_currentDownloadItem);
                                        _skToolsUnzipPerformer.Start();
                                    }
                                    else
                                    {
                                        _skToolsUnzipPerformer.AddItemForInstall(_currentDownloadItem);
                                    }
                                }
                            }
                            else
                            {
                                SKLogging.WriteLog(Tag, "Current download item = " + _currentDownloadItem.ItemCode + " is in NOT_QUEUED / INSTALLED state => remove it from " + "download queue", SKLogging.LogDebug);
                            }
                            // remove this item from download queue and go to next item
                            _queuedDownloads.RemoveFirst();
                            _currentDownloadItem = _queuedDownloads.First.Value;
                        }
                        else
                        {
                            SKLogging.WriteLog(Tag, "Current download item = " + _currentDownloadItem.ItemCode + " is added to download queue", SKLogging.LogDebug);
                            _currentDownloadStep = _currentDownloadItem.CurrentDownloadStep;
                            if (_currentDownloadStep != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Initializes resources(http client, wifi lock) when download thread starts
        /// </summary>
        private void InitializeResourcesWhenDownloadThreadStarts()
        {
            // instance of HttpClient instance
            _httpClient = new HttpClient();
            if ((_downloadListener != null) && (_downloadListener is Activity))
            {
                WifiManager wifimanager = (WifiManager)((Activity)_downloadListener).GetSystemService(Context.WifiService);
                _wifiLock = wifimanager.CreateWifiLock("my_lock");
            }
        }

        /// <summary>
        /// Release resources(http client, wifi lock) when download thread finishes
        /// </summary>
        private void ReleaseResourcesWhenDownloadThreadFinishes()
        {
            // release the WI-FI lock
            if (_wifiLock != null)
            {
                _wifiLock.Release();
            }
            // release the HttpClient resource
            if (_httpClient != null)
            {
                if (_httpClient != null)
                {
                    try
                    {
                        _httpClient.Dispose();
                    }
                    catch (Exception ex)
                    {
                        SKLogging.WriteLog(Tag, "Thrown exception when release the HttpClient resource ; exception = " + ex.Message, SKLogging.LogDebug);
                    }
                }
            }
        }

        /// <summary>
        /// starts the download timeout handler
        /// </summary>
        private void startsDownloadTimeoutHandler()
        {
            var activity = _downloadListener as Activity;
            if (activity != null)
            {
                activity.RunOnUiThread(() =>
                {
                    if (_downloadTimeoutHandler == null)
                    {
                        _downloadTimeoutHandler = new Handler();
                        _downloadTimeoutAction = CancelAction;
                        _downloadTimeoutHandler.PostDelayed(_downloadTimeoutAction, TimeOutLimitForEdgeCases);
                    }
                });
            }
        }

        private void CancelAction()
        {
            SKLogging.WriteLog(Tag, "The blocked request is stopped now => the user is notified that connection was lost", SKLogging.LogDebug);
            _isDownloadRequestUnresponsive = true;

            // abort current request
            if (_httpRequest != null)
            {
                _httpRequest.Abort();
            }

            _downloadTimeoutHandler = null;
            if (!_isDownloadProcessCancelled && !_isCurrentDownloadCancelled || !_isDownloadProcessPaused)
            {
                StopDownloadProcessWhenInternetConnectionFails(false);
            }
            else if (_isDownloadProcessCancelled)
            {
                CancelDownloadProcess();
            }
            else if (_isCurrentDownloadCancelled)
            {
                CancelCurrentDownload();
            }
            else if (_isDownloadProcessPaused)
            {
                PauseDownloadProcess();
            }
        }

        /// <summary>
        /// Stops the download timeout handler
        /// </summary>
        private void stopsDownloadTimeoutHandler()
        {
            if ((_downloadListener != null) && _downloadListener is Activity)
            {
                ((Activity)_downloadListener).RunOnUiThread(() =>
                {
                    if (_downloadTimeoutHandler != null)
                    {
                        _downloadTimeoutHandler.RemoveCallbacks(_downloadTimeoutAction);
                        _downloadTimeoutAction = null;
                        _downloadTimeoutHandler = null;
                    }
                });
            }
        }

        /// <summary>
        /// if timeout limit ended, stops otherwise, performs the retry mechanism </summary>
        /// <param name="stopRequest"> true if the request must be stopped </param>
        private void StopIfTimeoutLimitEnded(bool stopRequest)
        {
            stopsDownloadTimeoutHandler();
            if (((DateTimeOffset.Now.CurrentTimeMillis() - _lastTimeWhenInternetWorked) > TimeOutLimit) || stopRequest)
            {
                SKLogging.WriteLog(Tag, "The request last more than 15 seconds, so no timeout is made", SKLogging.LogDebug);
                if (!_isDownloadProcessCancelled && !_isCurrentDownloadCancelled && !_isDownloadProcessPaused)
                {
                    // stop download process and notifies the UI
                    StopDownloadProcessWhenInternetConnectionFails(true);
                }
                else if (_isDownloadProcessCancelled)
                {
                    CancelDownloadProcess();
                }
                else if (_isCurrentDownloadCancelled)
                {
                    CancelCurrentDownload();
                }
                else if (_isDownloadProcessPaused)
                {
                    PauseDownloadProcess();
                }
            }
            else
            {
                RetryUntilTimeoutLimitReached();
            }
        }

        /// <summary>
        /// If download process is not paused/cancelled, or current download is not cancelled,
        /// retries until timeout limit is reached
        /// </summary>
        private void RetryUntilTimeoutLimitReached()
        {
            if (!_isDownloadProcessCancelled && !_isCurrentDownloadCancelled && !_isDownloadProcessPaused)
            {
                // if no retry was made, during current INTERNET connection, then retain the time at which the first one is made
                if (!_anyRetryMade)
                {
                    _timeAtFirstRetry = DateTimeOffset.Now.CurrentTimeMillis();
                    _anyRetryMade = true;
                }

                // if it didn't pass 15 seconds from the first retry, will sleep 0.5 seconds and then will make a new attempt to download the resource
                if ((DateTimeOffset.Now.CurrentTimeMillis() - _timeAtFirstRetry) < TimeOutLimit)
                {
                    SKLogging.WriteLog(Tag, "Sleep and then retry", SKLogging.LogDebug);
                    try
                    {
                        Sleep(NoMillisIntoOneSec / 2);
                    }
                    catch (InterruptedException e1)
                    {
                        SKLogging.WriteLog(Tag, "Retry ; interrupted exception = " + e1.Message, SKLogging.LogDebug);
                    }
                }
                else
                {
                    StopIfTimeoutLimitEnded(true);
                }
            }
            else if (_isDownloadProcessCancelled)
            {
                CancelDownloadProcess();
            }
            else if (_isCurrentDownloadCancelled)
            {
                CancelCurrentDownload();
            }
            else if (_isDownloadProcessPaused)
            {
                PauseDownloadProcess();
            }
        }

        /// <summary>
        /// Cancels current download while download process is running
        /// </summary>
        private void CancelCurrentDownload()
        {
            _isCurrentDownloadCancelled = false;
            if (_isDownloadProcessPaused)
            {
                _isDownloadProcessPaused = false;
            }
            // cancel download for current item
            if (_currentDownloadItem != null)
            {
                _currentDownloadItem.MarkAsNotQueued();
            }
            // remove current download from download queue (first element from queue)
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (_queuedDownloads != null)
                {
                    _queuedDownloads.RemoveFirst();
                }
            }
            // notify the UI that current download was cancelled
            if (_downloadListener != null)
            {
                _downloadListener.OnDownloadCancelled(_currentDownloadItem.ItemCode);
            }
        }

        /// <summary>
        /// Cancels download process while running
        /// </summary>
        private void CancelDownloadProcess()
        {
            _isDownloadProcessCancelled = false;
            if (_isCurrentDownloadCancelled)
            {
                _isCurrentDownloadCancelled = false;
            }
            if (_isDownloadProcessPaused)
            {
                _isDownloadProcessPaused = false;
            }
            // cancel download for current item
            if (_currentDownloadItem != null)
            {
                _currentDownloadItem.MarkAsNotQueued();
            }
            // remove all resources from download queue => stop download thread
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (_queuedDownloads != null)
                {
                    _queuedDownloads.Clear();
                }
            }
            if (_downloadListener != null)
            {
                _downloadListener.OnAllDownloadsCancelled();
            }
        }

        /// <summary>
        /// pause download process while running
        /// </summary>
        private void PauseDownloadProcess()
        {
            PauseDownloadThread();
            if (_downloadListener != null)
            {
                _downloadListener.OnDownloadPaused(_currentDownloadItem);
            }
        }

        /// <summary>
        /// finishes current download
        /// </summary>
        private void FinishCurrentDownload()
        {
            if (_currentDownloadItem != null)
            {
                // check the total read bytes for current download
                long totalBytesRead;
                try
                {
                    RandomAccessFile currentDestinationFile = new RandomAccessFile(_currentDownloadItem.CurrentStepDestinationPath, "r");
                    totalBytesRead = currentDestinationFile.Length();
                }
                catch (FileNotFoundException)
                {
                    totalBytesRead = 0;
                }
                catch (IOException)
                {
                    totalBytesRead = 0;
                }
                if (totalBytesRead < _currentDownloadItem.CurrentDownloadStep.DownloadItemSize)
                {
                    SKLogging.WriteLog(Tag, "The " + _currentDownloadItem.ItemCode + " current file was not fully downloaded ; total bytes read = " + totalBytesRead + " ; size = " + _currentDownloadItem.CurrentDownloadStep.DownloadItemSize + " ; current step index = " + _currentDownloadItem.CurrentStepIndex, SKLogging.LogDebug);
                    throw new SocketException();
                }
                _currentDownloadItem.GoToNextDownloadStep();
                if (_currentDownloadItem.DownloadFinished)
                {
                    _currentDownloadItem.SKDownloadState = SKDownloadState.Downloaded;
                    // remove current download from download queue
                    lock (typeof(SKToolsDownloadPerformer))
                    {
                        if (_queuedDownloads != null)
                        {
                            _queuedDownloads.RemoveFirst();
                        }
                    }
                    if (_currentDownloadItem.UnzipIsNeeded())
                    { // UNZIP is needed for current resource
                        SKLogging.WriteLog(Tag, "Current item = " + _currentDownloadItem.ItemCode + " is now downloaded => add it to install queue for unzip", SKLogging.LogDebug);
                        // we know that UNZIP operation corresponds to last download step
                        _currentDownloadItem.CurrentStepIndex = (sbyte)(_currentDownloadItem.CurrentStepIndex - 1);

                        // notify the UI that current resource was downloaded
                        if (_downloadListener != null)
                        {
                            _downloadListener.OnDownloadProgress(_currentDownloadItem);
                        }

                        // add current resource to install queue
                        lock (typeof(SKToolsUnzipPerformer))
                        {
                            if ((_skToolsUnzipPerformer == null) || (!_skToolsUnzipPerformer.IsAlive))
                            {
                                _skToolsUnzipPerformer = new SKToolsUnzipPerformer(_downloadListener);
                                _skToolsUnzipPerformer.AddItemForInstall(_currentDownloadItem);
                                _skToolsUnzipPerformer.Start();
                            }
                            else
                            {
                                _skToolsUnzipPerformer.AddItemForInstall(_currentDownloadItem);
                            }
                        }
                    }
                    else
                    { // UNZIP is not needed for current resource => INSTALL it now
                        // go back to previous step
                        _currentDownloadItem.CurrentStepIndex = (sbyte)(_currentDownloadItem.CurrentStepIndex - 1);
                        string rootFilePath = null;
                        string destinationPath = _currentDownloadItem.CurrentStepDestinationPath;
                        if (destinationPath != null)
                        {
                            rootFilePath = destinationPath.Substring(0, destinationPath.IndexOf((new StringBuilder(_currentDownloadItem.ItemCode)).Append(SKToolsDownloadManager.PointExtension).ToString(), StringComparison.Ordinal));
                        }
                        SKLogging.WriteLog(Tag, "Current item = " + _currentDownloadItem.ItemCode + " is now downloaded => unzip is not needed => install it now at base path" + " = " + rootFilePath, SKLogging.LogDebug);
                        if (rootFilePath != null)
                        {
                            int result = SKPackageManager.Instance.AddOfflinePackage(rootFilePath, _currentDownloadItem.ItemCode);
                            SKLogging.WriteLog(Tag, "Current resource installing result code = " + result, SKLogging.LogDebug);
                            if ((result & SKPackageManager.AddPackageMissingSkmResult & SKPackageManager.AddPackageMissingNgiResult & SKPackageManager.AddPackageMissingNgiDatResult) == 0)
                            {
                                // current install was performed with success set current resource as already download
                                _currentDownloadItem.SKDownloadState = SKDownloadState.Installed;
                                SKLogging.WriteLog(Tag, "The " + _currentDownloadItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                                // notify the UI that current resource was installed
                                if (_downloadListener != null)
                                {
                                    _downloadListener.OnInstallFinished(_currentDownloadItem);
                                }
                            }
                            else
                            {
                                // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                                _currentDownloadItem.MarkAsNotQueued();
                                SKLogging.WriteLog(Tag, "The " + _currentDownloadItem.ItemCode + " resource couldn't be installed by our NG component, " + "although it was downloaded.", SKLogging.LogDebug);
                                // notify the UI that current resource was not installed
                                if (_downloadListener != null)
                                {
                                    _downloadListener.OnDownloadProgress(_currentDownloadItem);
                                }
                            }
                        }
                        else
                        {
                            // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                            _currentDownloadItem.MarkAsNotQueued();
                            SKLogging.WriteLog(Tag, "The " + _currentDownloadItem.ItemCode + " resource couldn't be installed by our NG component, " + "although it was downloaded, because installing path is null", SKLogging.LogDebug);
                            // notify the UI that current resource was not installed
                            if (_downloadListener != null)
                            {
                                _downloadListener.OnDownloadProgress(_currentDownloadItem);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stops download process when internet connection fails
        /// </summary>
        /// <param name="failureResponseReceivedFromServer"> true, if a response was received from server (add to identify a blocking request) </param>
        private void StopDownloadProcessWhenInternetConnectionFails(bool failureResponseReceivedFromServer)
        {
            PauseDownloadProcess();
            // update the UI (set current resource as paused, shows a toast)
            if (_downloadListener != null)
            {
                _downloadListener.OnInternetConnectionFailed(_currentDownloadItem, failureResponseReceivedFromServer);
            }
        }

        /// <summary>
        /// pauses download thread (stops current download thread, pauses current downloading resource)
        /// </summary>
        private void PauseDownloadThread()
        {
            // pause current resource
            if (_currentDownloadItem != null)
            {
                _currentDownloadItem.SKDownloadState = SKDownloadState.Paused;
            }
            // automatically stop the download thread
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (_queuedDownloads != null)
                {
                    _queuedDownloads.Clear();
                }
            }
        }

        /// <summary>
        /// return true if current item is fully downloaded
        /// </summary>
        private bool CurrentItemFullyDownloaded
        {
            get
            {
                // check the total read bytes for current download
                long totalBytesRead;
                try
                {
                    RandomAccessFile currentDestinationFile = new RandomAccessFile(_currentDownloadItem.CurrentStepDestinationPath, "r");
                    totalBytesRead = currentDestinationFile.Length();
                }
                catch (FileNotFoundException)
                {
                    totalBytesRead = 0;
                }
                catch (IOException)
                {
                    totalBytesRead = 0;
                }
                if (totalBytesRead == _currentDownloadItem.CurrentDownloadStep.DownloadItemSize)
                {
                    return true;
                }
                return false;
            }
        }
    }
}