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
using Android.Net.Wifi;
using Skobbler.Ngx.Util;
using System.IO;
using Java.IO;
using Java.Net;
using Skobbler.Ngx.Packages;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadPerformer : Thread
    {

        /// <summary>
        /// the tag associated with this class, used for debugging
        /// </summary>
        public const string TAG = "SKToolsDownloadPerformer";

        /// <summary>
        /// Key for HTTP header request property to send the DOWNLOADED bytes
        /// </summary>
        private const string HTTP_PROP_RANGE = "Range";

        /// <summary>
        /// represents the number of bytes from one megabyte
        /// </summary>
        private const int NO_BYTES_INTO_ONE_MB = 1048576;

        /// <summary>
        /// the timeout limit for the edge cases requests
        /// </summary>
        private const int TIME_OUT_LIMIT_FOR_EDGE_CASES = 20000;

        /// <summary>
        /// represents the number of milliseconds from one second
        /// </summary>
        private const int NO_MILLIS_INTO_ONE_SEC = 1000;

        /// <summary>
        /// the timeout limit for the requests
        /// </summary>
        private const int TIME_OUT_LIMIT = 15000;

        /// <summary>
        /// true, if current download is cancelled while download thread is running
        /// </summary>
        private volatile bool isCurrentDownloadCancelled;

        /// <summary>
        /// true, if download process is cancelled
        /// </summary>
        private volatile bool isDownloadProcessCancelled;

        /// <summary>
        /// true, if download request doesn't respond
        /// </summary>
        private volatile bool isDownloadRequestUnresponsive;

        /// <summary>
        /// tells that current download process is paused
        /// </summary>
        private volatile bool isDownloadProcessPaused;

        /// <summary>
        /// queued downloads
        /// </summary>
        private LinkedList<SKToolsDownloadItem> queuedDownloads;

        /// <summary>
        /// download listener
        /// </summary>
        private ISKToolsDownloadListener downloadListener;

        /// <summary>
        /// current download step
        /// </summary>
        private SKToolsFileDownloadStep currentDownloadStep;

        /// <summary>
        /// current download item
        /// </summary>
        private SKToolsDownloadItem currentDownloadItem;

        // WifiLock used for long running downloads
        private WifiManager.WifiLock wifiLock;

        /// <summary>
        /// instance of HttpClient used for download
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// current HTTP request
        /// </summary>
        private HttpRequestBase httpRequest;

        /// <summary>
        /// download timeout handler ; added for the edge cases (networks on which HttpClient blocks)
        /// </summary>
        private Handler downloadTimeoutHandler;

        /// <summary>
        /// runs when download request cannot return a response, after a while
        /// </summary>
        private Runnable downloadTimeoutRunnable;

        /// <summary>
        /// time at first retry
        /// </summary>
        private long timeAtFirstRetry;

        /// <summary>
        /// true, if any retry was made (reset after an INTERNET connection is received)
        /// </summary>
        private volatile bool anyRetryMade;

        /// <summary>
        /// last time when the INTERNET was working
        /// </summary>
        private long lastTimeWhenInternetWorked;

        /// <summary>
        /// SK-TOOLS unzip performer
        /// </summary>
        private SKToolsUnzipPerformer skToolsUnzipPerformer;

        /// <summary>
        /// creates an object of SKToolsDownloadPerformer type </summary>
        /// <param name="queuedDownloads"> queued downloads </param>
        /// <param name="downloadListener"> download listener </param>
        public SKToolsDownloadPerformer(LinkedList<SKToolsDownloadItem> queuedDownloads, ISKToolsDownloadListener downloadListener)
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                this.queuedDownloads = queuedDownloads;
            }
            this.downloadListener = downloadListener;
        }

        public virtual ISKToolsDownloadListener DownloadListener
        {
            set
            {
                this.downloadListener = value;
                if (skToolsUnzipPerformer != null)
                {
                    skToolsUnzipPerformer.DownloadListener = value;
                }
            }
        }

        public override void run()
        {
            // current input stream
            System.IO.Stream responseStream = null;
            sbyte[] data;
            // total bytes read
            long bytesReadSoFar;
            // bytes read during current INTERNET connection
            long bytesReadInThisConnection = 0;
            // time of the download during current INTERNET connection
            long lastDownloadProgressTime = 0;
            // memory needed
            long memoryNeeded;

            anyRetryMade = false;
            timeAtFirstRetry = 0;
            lastTimeWhenInternetWorked = DateTimeHelperClass.CurrentUnixTimeMillis();

            initializeResourcesWhenDownloadThreadStarts();

            while (existsAnyRemainingDownload())
            {
                if ((currentDownloadItem == null) || (queuedDownloads == null) || (currentDownloadStep == null) || (currentDownloadStep.DestinationPath == null) || (currentDownloadStep.DownloadURL == null))
                {
                    break;
                }

                // change the state for current download item
                if (currentDownloadItem.DownloadState != SKToolsDownloadItem.DOWNLOADING)
                {
                    currentDownloadItem.DownloadState = SKToolsDownloadItem.DOWNLOADING;
                }

                // check if current item is already downloaded (could be the case when download is performed very slow and the user exists the download thread without finishing the
                // download, but with the file downloaded)
                if (CurrentItemFullyDownloaded)
                {
                    try
                    {
                        finishCurrentDownload();
                    }
                    catch (SocketException)
                    {
                        // restart the download in this case
                        SKLogging.WriteLog(TAG, "Not possible, because in this case the download wouldn't appear to be finished => restart the download and remove the old data!!!", SKLogging.LogDebug);
                        // reset the number of downloaded bytes and remove them from storage
                        currentDownloadItem.NoDownloadedBytes = 0;
                        string deleteCmd = "rm -r " + currentDownloadItem.CurrentStepDestinationPath;
                        Runtime runtime = Runtime.GetRuntime();
                        try
                        {
                            runtime.Exec(deleteCmd);
                            SKLogging.WriteLog(TAG, "The file was deleted from its current installation folder", SKLogging.LogDebug);
                        }
                        catch (IOException)
                        {
                            SKLogging.WriteLog(TAG, "The file couldn't be deleted !!!", SKLogging.LogDebug);
                        }
                    }
                }
                else
                {
                    // create a new download request
                    httpRequest = new HttpGet(currentDownloadStep.DownloadURL);

                    SKLogging.WriteLog(TAG, "Current url = " + currentDownloadStep.DownloadURL + " ; current step = " + currentDownloadItem.CurrentStepIndex, SKLogging.LogDebug);

                    // resume operation => send already downloaded bytes
                    bytesReadSoFar = sendAlreadyDownloadedBytes();

                    // check if exists any free memory and return if there is not enough memory for this download
                    memoryNeeded = getNeededMemoryForCurrentDownload(bytesReadSoFar);
                    if (memoryNeeded != 0)
                    {
                        SKLogging.WriteLog(TAG, "Not enough memory on current storage", SKLogging.LogDebug);
                        pauseDownloadProcess();
                        // notify the UI about memory issues
                        if (downloadListener != null)
                        {
                            downloadListener.OnNotEnoughMemoryOnCurrentStorage(currentDownloadItem);
                        }
                        break;
                    }

                    // database and UI update
                    if (downloadListener != null)
                    {
                        downloadListener.OnDownloadProgress(currentDownloadItem);
                    }

                    try
                    {
                        // starts the timeout handler
                        startsDownloadTimeoutHandler();
                        // executes the download request
                        HttpResponse response = httpClient.execute(httpRequest);
                        if (response == null)
                        {
                            throw new SocketException();
                        }
                        else
                        {
                            anyRetryMade = false;
                            HttpEntity entity = response.Entity;
                            int statusCode = response.StatusLine.StatusCode;
                            // if other status code than 200 or 206(partial download) throw exception
                            if (statusCode != HttpURLConnection.HTTP_OK && statusCode != HttpURLConnection.HTTP_PARTIAL)
                            {
                                SKLogging.WriteLog(TAG, "Wrong status code returned !", SKLogging.LogDebug);
                                throw new IOException("HTTP response code: " + statusCode);
                            }
                            // stops the timeout handler
                            stopsDownloadTimeoutHandler();
                            SKLogging.WriteLog(TAG, "Correct response status code returned !", SKLogging.LogDebug);
                            try
                            {
                                if (entity != null)
                                {
                                    responseStream = entity.Content;
                                }
                            }
                            catch (IllegalStateException)
                            {
                                SKLogging.WriteLog(TAG, "The returned response content is not correct !", SKLogging.LogDebug);
                            }
                            if (responseStream == null)
                            {
                                SKLogging.WriteLog(TAG, "Response stream is null !!!", SKLogging.LogDebug);
                            }

                            // create a byte array buffer of 1Mb
                            data = new sbyte[NO_BYTES_INTO_ONE_MB];
                            // creates the randomAccessFile - if exists it opens it
                            RandomAccessFile localFile = new RandomAccessFile(currentDownloadStep.DestinationPath, "rw");
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
                                    lastTimeWhenInternetWorked = DateTimeHelperClass.CurrentUnixTimeMillis();
                                    bytesReadSoFar += bytesReadThisTime;
                                    currentDownloadItem.NoDownloadedBytes = bytesReadSoFar;
                                    bytesReadInThisConnection += bytesReadThisTime;
                                    currentDownloadItem.NoDownloadedBytesInThisConnection = bytesReadInThisConnection;
                                    // write the chunk of data in the file
                                    localFile.Write(data, 0, bytesReadThisTime);
                                    long newTime = DateTimeHelperClass.CurrentUnixTimeMillis();
                                    // notify the UI every second
                                    if ((newTime - lastDownloadProgressTime) > NO_MILLIS_INTO_ONE_SEC)
                                    {
                                        lastDownloadProgressTime = newTime;
                                        if (downloadListener != null)
                                        {
                                            downloadListener.OnDownloadProgress(currentDownloadItem);
                                        }
                                    }
                                }
                                else if (bytesReadThisTime == -1)
                                {
                                    SKLogging.WriteLog(TAG, "No more data to read, so exit !", SKLogging.LogDebug);
                                    // if no more data to read, exit
                                    break;
                                }
                                if (isCurrentDownloadCancelled || isDownloadProcessCancelled || isDownloadRequestUnresponsive || isDownloadProcessPaused)
                                {
                                    break;
                                }
                            }
                            localFile.Close();
                            responseStream = null;

                            if (!isDownloadRequestUnresponsive)
                            {
                                if (isDownloadProcessCancelled)
                                {
                                    cancelDownloadProcess();
                                }
                                else if (isCurrentDownloadCancelled)
                                {
                                    cancelCurrentDownload();
                                }
                                else if (isDownloadProcessPaused)
                                {
                                    pauseDownloadProcess();
                                }
                                else
                                {
                                    finishCurrentDownload();
                                }
                            }
                        }
                    }
                    catch (SocketException e)
                    {
                        SKLogging.WriteLog(TAG, "Socket Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!isDownloadRequestUnresponsive)
                        {
                            stopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (UnknownHostException e)
                    {
                        SKLogging.WriteLog(TAG, "Unknown Host Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!isDownloadRequestUnresponsive)
                        {
                            stopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (IOException e)
                    {
                        SKLogging.WriteLog(TAG, "IO Exception ; " + e.Message, SKLogging.LogDebug);
                        if (!isDownloadRequestUnresponsive)
                        {
                            stopIfTimeoutLimitEnded(false);
                        }
                    }
                    catch (System.IndexOutOfRangeException e)
                    {
                        SKLogging.WriteLog(TAG, "Index Out Of Bounds Exception ; " + e.Message, SKLogging.LogDebug);
                        stopsDownloadTimeoutHandler();
                    }
                }
            }
            releaseResourcesWhenDownloadThreadFinishes();
            SKLogging.WriteLog(TAG, "The download thread has stopped", SKLogging.LogDebug);
        }

        /// <summary>
        /// set current download as cancelled
        /// </summary>
        public virtual void setCurrentDownloadAsCancelled()
        {
            isCurrentDownloadCancelled = true;
        }

        /// <summary>
        /// sets download process as cancelled
        /// </summary>
        public virtual void setDownloadProcessAsCancelled()
        {
            isDownloadProcessCancelled = true;
        }

        /// <summary>
        /// sets download process as paused
        /// </summary>
        public virtual void setDownloadProcessAsPaused()
        {
            isDownloadProcessPaused = true;
        }

        /// <summary>
        /// send the number of bytes already DOWNLOADED - for the resume operation
        /// </summary>
        private long sendAlreadyDownloadedBytes()
        {
            long bytesRead;
            try
            {
                RandomAccessFile destinationFile = new RandomAccessFile(currentDownloadItem.CurrentStepDestinationPath, "r");
                bytesRead = destinationFile.Length();
                if (bytesRead > 0)
                {
                    SKLogging.WriteLog(TAG, "There are some bytes at this path ; number of downloaded bytes for current resource is " + currentDownloadItem.NoDownloadedBytes + " ; download step = " + currentDownloadItem.CurrentStepIndex + " ; current path = " + currentDownloadItem.CurrentStepDestinationPath, SKLogging.LogDebug);
                    if (currentDownloadItem.NoDownloadedBytes == 0)
                    {
                        SKLogging.WriteLog(TAG, "There remained some resources with the same name at the same path ! Try to delete the file " + currentDownloadItem.CurrentStepDestinationPath, SKLogging.LogDebug);
                        string deleteCmd = "rm -r " + currentDownloadItem.CurrentStepDestinationPath;
                        Runtime runtime = Runtime.GetRuntime();
                        try
                        {
                            runtime.Exec(deleteCmd);
                            SKLogging.WriteLog(TAG, "The file was deleted from its current installation folder", SKLogging.LogDebug);
                        }
                        catch (IOException)
                        {
                            SKLogging.WriteLog(TAG, "The file couldn't be deleted !!!", SKLogging.LogDebug);
                        }
                        bytesRead = 0;
                    }
                    else
                    {
                        SKLogging.WriteLog(TAG, "Current resource is only partially downloaded, so its download will continue = ", SKLogging.LogDebug);
                        httpRequest.addHeader(HTTP_PROP_RANGE, "bytes=" + bytesRead + "-");
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
        /// gets needed memory for current download </summary>
        /// <param name="bytesRead"> bytes already read </param>
        private long getNeededMemoryForCurrentDownload(long bytesRead)
        {
            long neededSize = currentDownloadItem.RemainingSize - bytesRead;
            string filePath = currentDownloadItem.CurrentStepDestinationPath;
            string basePath = filePath.Substring(0, filePath.IndexOf((new StringBuilder(currentDownloadItem.ItemCode)).Append(SKToolsDownloadManager.POINT_EXTENSION).ToString(), StringComparison.Ordinal));
            long memoryNeeded = SKToolsDownloadUtils.getNeededBytesForADownload(neededSize, basePath);
            SKLogging.WriteLog(TAG, "Memory needed = " + memoryNeeded, SKLogging.LogDebug);
            return memoryNeeded;
        }

        /// <summary>
        /// checks if there is any remaining item to download </summary>
        /// <returns> true, if there is any remaining item to download, false otherwise </returns>
        private bool existsAnyRemainingDownload()
        {
            lock (typeof(SKToolsDownloadPerformer))
            {
                if ((queuedDownloads != null) && queuedDownloads.Count > 0)
                {
                    currentDownloadItem = queuedDownloads.First.Value;
                    while (currentDownloadItem != null)
                    {
                        if ((currentDownloadItem.DownloadState == SKToolsDownloadItem.INSTALLING) || (currentDownloadItem.DownloadState == SKToolsDownloadItem.NOT_QUEUED) || (currentDownloadItem.DownloadState == SKToolsDownloadItem.INSTALLED))
                        {
                            if (currentDownloadItem.DownloadState == SKToolsDownloadItem.INSTALLING)
                            {
                                SKLogging.WriteLog(TAG, "Current download item = " + currentDownloadItem.ItemCode + " is in INSTALLING state => add it to install queue", SKLogging.LogDebug);
                                // add current resource to install queue
                                lock (typeof(SKToolsUnzipPerformer))
                                {
                                    if ((skToolsUnzipPerformer == null) || (!skToolsUnzipPerformer.Alive))
                                    {
                                        skToolsUnzipPerformer = new SKToolsUnzipPerformer(downloadListener);
                                        skToolsUnzipPerformer.addItemForInstall(currentDownloadItem);
                                        skToolsUnzipPerformer.Start();
                                    }
                                    else
                                    {
                                        skToolsUnzipPerformer.addItemForInstall(currentDownloadItem);
                                    }
                                }
                            }
                            else
                            {
                                SKLogging.WriteLog(TAG, "Current download item = " + currentDownloadItem.ItemCode + " is in NOT_QUEUED / INSTALLED state => remove it from " + "download queue", SKLogging.LogDebug);
                            }
                            // remove this item from download queue and go to next item
                            queuedDownloads.RemoveFirst();
                            currentDownloadItem = queuedDownloads.First.Value;
                        }
                        else
                        {
                            SKLogging.WriteLog(TAG, "Current download item = " + currentDownloadItem.ItemCode + " is added to download queue", SKLogging.LogDebug);
                            currentDownloadStep = currentDownloadItem.CurrentDownloadStep;
                            if (currentDownloadStep != null)
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
        /// initializes resources(http client, wifi lock) when download thread starts
        /// </summary>
        private void initializeResourcesWhenDownloadThreadStarts()
        {
            // instance of HttpClient instance
            httpClient = new DefaultHttpClient();
            if ((downloadListener != null) && (downloadListener is Activity))
            {
                WifiManager wifimanager = (WifiManager)((Activity)downloadListener).GetSystemService(Context.WifiService);
                wifiLock = wifimanager.CreateWifiLock("my_lock");
            }
        }

        /// <summary>
        /// release resources(http client, wifi lock) when download thread finishes
        /// </summary>
        private void releaseResourcesWhenDownloadThreadFinishes()
        {
            // release the WI-FI lock
            if (wifiLock != null)
            {
                wifiLock.Release();
            }
            // release the HttpClient resource
            if (httpClient != null)
            {
                if (httpClient.ConnectionManager != null)
                {
                    try
                    {
                        httpClient.ConnectionManager.shutdown();
                    }
                    catch (Exception ex)
                    {
                        SKLogging.WriteLog(TAG, "Thrown exception when release the HttpClient resource ; exception = " + ex.Message, SKLogging.LogDebug);
                    }
                }
            }
        }

        /// <summary>
        /// starts the download timeout handler
        /// </summary>
        private void startsDownloadTimeoutHandler()
        {
            if ((downloadListener != null) && downloadListener is Activity)
            {
                ((Activity)downloadListener).RunOnUiThread(new RunnableAnonymousInnerClassHelper(this));
            }
        }

        private class RunnableAnonymousInnerClassHelper : IRunnable
        {
            private readonly SKToolsDownloadPerformer outerInstance;

            public RunnableAnonymousInnerClassHelper(SKToolsDownloadPerformer outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                if (outerInstance.downloadTimeoutHandler == null)
                {
                    outerInstance.downloadTimeoutHandler = new Handler();
                    outerInstance.downloadTimeoutRunnable = new RunnableAnonymousInnerClassHelper2(this);
                    outerInstance.downloadTimeoutHandler.postDelayed(outerInstance.downloadTimeoutRunnable, TIME_OUT_LIMIT_FOR_EDGE_CASES);
                }
            }

            private class RunnableAnonymousInnerClassHelper2 : IRunnable
            {
                private readonly RunnableAnonymousInnerClassHelper outerInstance;

                public RunnableAnonymousInnerClassHelper2(RunnableAnonymousInnerClassHelper outerInstance)
                {
                    this.outerInstance = outerInstance;
                }


                public override void run()
                {
                    SKLogging.WriteLog(TAG, "The blocked request is stopped now => the user is notified that connection was lost", SKLogging.LogDebug);
                    outerInstance.outerInstance.isDownloadRequestUnresponsive = true;
                    // abort current request
                    new AsyncTaskAnonymousInnerClassHelper(this)
                    .execute();
                    outerInstance.outerInstance.downloadTimeoutHandler = null;
                    if (!outerInstance.outerInstance.isDownloadProcessCancelled && !outerInstance.outerInstance.isCurrentDownloadCancelled || !outerInstance.outerInstance.isDownloadProcessPaused)
                    {
                        outerInstance.outerInstance.stopDownloadProcessWhenInternetConnectionFails(false);
                    }
                    else if (outerInstance.outerInstance.isDownloadProcessCancelled)
                    {
                        outerInstance.outerInstance.cancelDownloadProcess();
                    }
                    else if (outerInstance.outerInstance.isCurrentDownloadCancelled)
                    {
                        outerInstance.outerInstance.cancelCurrentDownload();
                    }
                    else if (outerInstance.outerInstance.isDownloadProcessPaused)
                    {
                        outerInstance.outerInstance.pauseDownloadProcess();
                    }
                }

                private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, Void>
                {
                    private readonly RunnableAnonymousInnerClassHelper2 outerInstance;

                    public AsyncTaskAnonymousInnerClassHelper(RunnableAnonymousInnerClassHelper2 outerInstance)
                    {
                        this.outerInstance = outerInstance;
                    }

                    protected internal override Void doInBackground(params Void[] @params)
                    {
                        if (outerInstance.outerInstance.outerInstance.httpRequest != null)
                        {
                            outerInstance.outerInstance.outerInstance.httpRequest.abort();
                        }
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// stops the download timeout handler
        /// </summary>
        private void stopsDownloadTimeoutHandler()
        {
            if ((downloadListener != null) && downloadListener is Activity)
            {
                ((Activity)downloadListener).RunOnUiThread(new RunnableAnonymousInnerClassHelper3(this));
            }
        }

        private class RunnableAnonymousInnerClassHelper3 : IRunnable
        {
            private readonly SKToolsDownloadPerformer outerInstance;

            public RunnableAnonymousInnerClassHelper3(SKToolsDownloadPerformer outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                if (outerInstance.downloadTimeoutHandler != null)
                {
                    outerInstance.downloadTimeoutHandler.RemoveCallbacks(outerInstance.downloadTimeoutRunnable);
                    outerInstance.downloadTimeoutRunnable = null;
                    outerInstance.downloadTimeoutHandler = null;
                }
            }
        }

        /// <summary>
        /// if timeout limit ended, stops otherwise, performs the retry mechanism </summary>
        /// <param name="stopRequest"> true if the request must be stopped </param>
        private void stopIfTimeoutLimitEnded(bool stopRequest)
        {
            stopsDownloadTimeoutHandler();
            if (((DateTimeHelperClass.CurrentUnixTimeMillis() - lastTimeWhenInternetWorked) > TIME_OUT_LIMIT) || stopRequest)
            {
                SKLogging.WriteLog(TAG, "The request last more than 15 seconds, so no timeout is made", SKLogging.LogDebug);
                if (!isDownloadProcessCancelled && !isCurrentDownloadCancelled && !isDownloadProcessPaused)
                {
                    // stop download process and notifies the UI
                    stopDownloadProcessWhenInternetConnectionFails(true);
                }
                else if (isDownloadProcessCancelled)
                {
                    cancelDownloadProcess();
                }
                else if (isCurrentDownloadCancelled)
                {
                    cancelCurrentDownload();
                }
                else if (isDownloadProcessPaused)
                {
                    pauseDownloadProcess();
                }
            }
            else
            {
                retryUntilTimeoutLimitReached();
            }
        }

        /// <summary>
        /// if download process is not paused/cancelled, or current download is not cancelled, retries until timeout limit is reached
        /// </summary>
        private void retryUntilTimeoutLimitReached()
        {
            if (!isDownloadProcessCancelled && !isCurrentDownloadCancelled && !isDownloadProcessPaused)
            {
                // if no retry was made, during current INTERNET connection, then retain the time at which the first one is made
                if (!anyRetryMade)
                {
                    timeAtFirstRetry = DateTimeHelperClass.CurrentUnixTimeMillis();
                    anyRetryMade = true;
                }

                // if it didn't pass 15 seconds from the first retry, will sleep 0.5 seconds and then will make a new attempt to download the resource
                if ((DateTimeHelperClass.CurrentUnixTimeMillis() - timeAtFirstRetry) < TIME_OUT_LIMIT)
                {
                    SKLogging.WriteLog(TAG, "Sleep and then retry", SKLogging.LogDebug);
                    try
                    {
                        Thread.Sleep(NO_MILLIS_INTO_ONE_SEC / 2);
                    }
                    catch (InterruptedException e1)
                    {
                        SKLogging.WriteLog(TAG, "Retry ; interrupted exception = " + e1.Message, SKLogging.LogDebug);
                    }
                }
                else
                {
                    stopIfTimeoutLimitEnded(true);
                }
            }
            else if (isDownloadProcessCancelled)
            {
                cancelDownloadProcess();
            }
            else if (isCurrentDownloadCancelled)
            {
                cancelCurrentDownload();
            }
            else if (isDownloadProcessPaused)
            {
                pauseDownloadProcess();
            }
        }

        /// <summary>
        /// cancels current download while download process is running
        /// </summary>
        private void cancelCurrentDownload()
        {
            isCurrentDownloadCancelled = false;
            if (isDownloadProcessPaused)
            {
                isDownloadProcessPaused = false;
            }
            // cancel download for current item
            if (currentDownloadItem != null)
            {
                currentDownloadItem.markAsNotQueued();
            }
            // remove current download from download queue (first element from queue)
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (queuedDownloads != null)
                {
                    queuedDownloads.RemoveFirst();
                }
            }
            // notify the UI that current download was cancelled
            if (downloadListener != null)
            {
                downloadListener.OnDownloadCancelled(currentDownloadItem.ItemCode);
            }
        }

        /// <summary>
        /// cancels download process while running
        /// </summary>
        private void cancelDownloadProcess()
        {
            isDownloadProcessCancelled = false;
            if (isCurrentDownloadCancelled)
            {
                isCurrentDownloadCancelled = false;
            }
            if (isDownloadProcessPaused)
            {
                isDownloadProcessPaused = false;
            }
            // cancel download for current item
            if (currentDownloadItem != null)
            {
                currentDownloadItem.markAsNotQueued();
            }
            // remove all resources from download queue => stop download thread
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (queuedDownloads != null)
                {
                    queuedDownloads.Clear();
                }
            }
            if (downloadListener != null)
            {
                downloadListener.OnAllDownloadsCancelled();
            }
        }

        /// <summary>
        /// pause download process while running
        /// </summary>
        private void pauseDownloadProcess()
        {
            pauseDownloadThread();
            if (downloadListener != null)
            {
                downloadListener.OnDownloadPaused(currentDownloadItem);
            }
        }

        /// <summary>
        /// finishes current download
        /// </summary>
        private void finishCurrentDownload()
        {
            if (currentDownloadItem != null)
            {
                // check the total read bytes for current download
                long totalBytesRead;
                try
                {
                    RandomAccessFile currentDestinationFile = new RandomAccessFile(currentDownloadItem.CurrentStepDestinationPath, "r");
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
                if (totalBytesRead < currentDownloadItem.CurrentDownloadStep.DownloadItemSize)
                {
                    SKLogging.WriteLog(TAG, "The " + currentDownloadItem.ItemCode + " current file was not fully downloaded ; total bytes read = " + totalBytesRead + " ; size = " + currentDownloadItem.CurrentDownloadStep.DownloadItemSize + " ; current step index = " + currentDownloadItem.CurrentStepIndex, SKLogging.LogDebug);
                    throw new SocketException();
                }
                else
                {
                    currentDownloadItem.goToNextDownloadStep();
                    if (currentDownloadItem.DownloadFinished)
                    {
                        currentDownloadItem.DownloadState = SKToolsDownloadItem.DOWNLOADED;
                        // remove current download from download queue
                        lock (typeof(SKToolsDownloadPerformer))
                        {
                            if (queuedDownloads != null)
                            {
                                queuedDownloads.RemoveFirst();
                            }
                        }
                        if (currentDownloadItem.unzipIsNeeded())
                        { // UNZIP is needed for current resource
                            SKLogging.WriteLog(TAG, "Current item = " + currentDownloadItem.ItemCode + " is now downloaded => add it to install queue for unzip", SKLogging.LogDebug);
                            // we know that UNZIP operation corresponds to last download step
                            currentDownloadItem.CurrentStepIndex = (sbyte)(currentDownloadItem.CurrentStepIndex - 1);

                            // notify the UI that current resource was downloaded
                            if (downloadListener != null)
                            {
                                downloadListener.OnDownloadProgress(currentDownloadItem);
                            }

                            // add current resource to install queue
                            lock (typeof(SKToolsUnzipPerformer))
                            {
                                if ((skToolsUnzipPerformer == null) || (!skToolsUnzipPerformer.Alive))
                                {
                                    skToolsUnzipPerformer = new SKToolsUnzipPerformer(downloadListener);
                                    skToolsUnzipPerformer.addItemForInstall(currentDownloadItem);
                                    skToolsUnzipPerformer.Start();
                                }
                                else
                                {
                                    skToolsUnzipPerformer.addItemForInstall(currentDownloadItem);
                                }
                            }
                        }
                        else
                        { // UNZIP is not needed for current resource => INSTALL it now
                            // go back to previous step
                            currentDownloadItem.CurrentStepIndex = (sbyte)(currentDownloadItem.CurrentStepIndex - 1);
                            string rootFilePath = null;
                            string destinationPath = currentDownloadItem.CurrentStepDestinationPath;
                            if (destinationPath != null)
                            {
                                rootFilePath = destinationPath.Substring(0, destinationPath.IndexOf((new StringBuilder(currentDownloadItem.ItemCode)).Append(SKToolsDownloadManager.POINT_EXTENSION).ToString(), StringComparison.Ordinal));
                            }
                            SKLogging.WriteLog(TAG, "Current item = " + currentDownloadItem.ItemCode + " is now downloaded => unzip is not needed => install it now at base path" + " = " + rootFilePath, SKLogging.LogDebug);
                            if (rootFilePath != null)
                            {
                                int result = SKPackageManager.Instance.AddOfflinePackage(rootFilePath, currentDownloadItem.ItemCode);
                                SKLogging.WriteLog(TAG, "Current resource installing result code = " + result, SKLogging.LogDebug);
                                if ((result & SKPackageManager.AddPackageMissingSkmResult & SKPackageManager.AddPackageMissingNgiResult & SKPackageManager.AddPackageMissingNgiDatResult) == 0)
                                {
                                    // current install was performed with success set current resource as already download
                                    currentDownloadItem.DownloadState = SKToolsDownloadItem.INSTALLED;
                                    SKLogging.WriteLog(TAG, "The " + currentDownloadItem.ItemCode + " resource was successfully downloaded and installed by our NG component.", SKLogging.LogDebug);
                                    // notify the UI that current resource was installed
                                    if (downloadListener != null)
                                    {
                                        downloadListener.OnInstallFinished(currentDownloadItem);
                                    }
                                }
                                else
                                {
                                    // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                                    currentDownloadItem.markAsNotQueued();
                                    SKLogging.WriteLog(TAG, "The " + currentDownloadItem.ItemCode + " resource couldn't be installed by our NG component, " + "although it was downloaded.", SKLogging.LogDebug);
                                    // notify the UI that current resource was not installed
                                    if (downloadListener != null)
                                    {
                                        downloadListener.OnDownloadProgress(currentDownloadItem);
                                    }
                                }
                            }
                            else
                            {
                                // current install was performed with error => set current resource as NOT_QUEUED, remove downloaded bytes etc
                                currentDownloadItem.markAsNotQueued();
                                SKLogging.WriteLog(TAG, "The " + currentDownloadItem.ItemCode + " resource couldn't be installed by our NG component, " + "although it was downloaded, because installing path is null", SKLogging.LogDebug);
                                // notify the UI that current resource was not installed
                                if (downloadListener != null)
                                {
                                    downloadListener.OnDownloadProgress(currentDownloadItem);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// stops download process when internet connection fails </summary>
        /// <param name="failureResponseReceivedFromServer"> true, if a response was received from server (add to identify a blocking request) </param>
        private void stopDownloadProcessWhenInternetConnectionFails(bool failureResponseReceivedFromServer)
        {
            pauseDownloadProcess();
            // update the UI (set current resource as paused, shows a toast)
            if (downloadListener != null)
            {
                downloadListener.OnInternetConnectionFailed(currentDownloadItem, failureResponseReceivedFromServer);
            }
        }

        /// <summary>
        /// pauses download thread (stops current download thread, pauses current downloading resource)
        /// </summary>
        private void pauseDownloadThread()
        {
            // pause current resource
            if (currentDownloadItem != null)
            {
                currentDownloadItem.DownloadState = SKToolsDownloadItem.PAUSED;
            }
            // automatically stop the download thread
            lock (typeof(SKToolsDownloadPerformer))
            {
                if (queuedDownloads != null)
                {
                    queuedDownloads.Clear();
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
                    RandomAccessFile currentDestinationFile = new RandomAccessFile(currentDownloadItem.CurrentStepDestinationPath, "r");
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
                if (totalBytesRead == currentDownloadItem.CurrentDownloadStep.DownloadItemSize)
                {
                    return true;
                }
                return false;
            }
        }
    }
}