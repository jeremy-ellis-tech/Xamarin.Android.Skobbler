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
    public class SKToolsDownloadItem
    {
        public const sbyte NOT_QUEUED = 0;

        public const sbyte QUEUED = 1;

        public const sbyte DOWNLOADING = 2;

        public const sbyte PAUSED = 3;

        public const sbyte DOWNLOADED = 4;

        public const sbyte INSTALLING = 5;

        public const sbyte INSTALLED = 6;

        private string itemCode;


        private sbyte downloadState;

        private IList<SKToolsFileDownloadStep> downloadSteps;

        private sbyte currentStepIndex;

        private long noDownloadedBytes;

        private long noDownloadedBytesInThisConnection;

        private bool unzipIsNeeded_Renamed;

        private bool installOperationIsNeeded;

        public SKToolsDownloadItem(string itemCode, IList<SKToolsFileDownloadStep> downloadSteps, sbyte downloadState, bool unzipIsNeeded, bool installOperationIsNeeded)
        {
            this.itemCode = itemCode;
            this.downloadSteps = downloadSteps;
            this.downloadState = downloadState;
            this.unzipIsNeeded_Renamed = unzipIsNeeded;
            this.installOperationIsNeeded = installOperationIsNeeded;
        }

        public virtual bool unzipIsNeeded()
        {
            return unzipIsNeeded_Renamed;
        }

        /// <summary>
        /// sets current download step index </summary>
        /// <param name="currentStepIndex"> current step index </param>
        public virtual sbyte CurrentStepIndex
        {
            set
            {
                this.currentStepIndex = value;
            }
            get
            {
                return this.currentStepIndex;
            }
        }


        /// <summary>
        /// gets current download step object </summary>
        /// <returns> current download step </returns>
        public virtual SKToolsFileDownloadStep CurrentDownloadStep
        {
            get
            {
                if (downloadSteps != null)
                {
                    if (downloadSteps.Count > currentStepIndex)
                    {
                        return downloadSteps[currentStepIndex];
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// returns if current item is downloaded </summary>
        /// <returns> true if current item is downloaded, false otherwise </returns>
        public virtual bool DownloadFinished
        {
            get
            {
                if (downloadSteps != null)
                {
                    if (downloadSteps.Count <= currentStepIndex)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// go to next download step
        /// </summary>
        public virtual void goToNextDownloadStep()
        {
            currentStepIndex++;
        }

        /// <summary>
        /// sets download state for current item </summary>
        /// <param name="downloadState"> download state for current item </param>
        public virtual sbyte DownloadState
        {
            set
            {
                this.downloadState = value;
            }
            get
            {
                return this.downloadState;
            }
        }


        /// <summary>
        /// gets the number of downloaded bytes </summary>
        /// <returns> no downloaded bytes </returns>
        public virtual long NoDownloadedBytes
        {
            get
            {
                return noDownloadedBytes;
            }
            set
            {
                this.noDownloadedBytes = value;
                for (int i = 0; i < currentStepIndex; i++)
                {
                    if ((downloadSteps != null) && (i < downloadSteps.Count))
                    {
                        SKToolsFileDownloadStep currentStep = downloadSteps[i];
                        if (currentStep != null)
                        {
                            this.noDownloadedBytes += currentStep.DownloadItemSize;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// gets current download item code </summary>
        /// <returns> current download item code </returns>
        public virtual string ItemCode
        {
            get
            {
                return itemCode;
            }
        }

        /// <summary>
        /// marks current item as NOT-QUEUED (e.g. if its download is cancelled from some reason)
        /// </summary>
        public virtual void markAsNotQueued()
        {
            // removes already downloaded bytes from current item
            for (int i = 0; i <= currentStepIndex; i++)
            {
                if ((downloadSteps != null) && (i < downloadSteps.Count))
                {
                    SKToolsFileDownloadStep currentStep = downloadSteps[i];
                    if (currentStep != null)
                    {
                        SKToolsDownloadUtils.removeCurrentLocationFromDisk(currentStep.DestinationPath);
                    }
                }
            }
            // revert current item state
            noDownloadedBytes = 0;
            noDownloadedBytesInThisConnection = 0;
            downloadState = NOT_QUEUED;
            currentStepIndex = 0;
        }

        /// <summary>
        /// sets the number of downloaded bytes during current internet connection </summary>
        /// <param name="noDownloadedBytesInThisConnection"> no downloaded bytes that will be set </param>
        public virtual long NoDownloadedBytesInThisConnection
        {
            set
            {
                this.noDownloadedBytesInThisConnection = value;
            }
        }

        /// <summary>
        /// gets current download step destination path
        /// </summary>
        public virtual string CurrentStepDestinationPath
        {
            get
            {
                if ((downloadSteps != null) && (currentStepIndex < downloadSteps.Count))
                {
                    return downloadSteps[currentStepIndex].DestinationPath;
                }
                return null;
            }
        }

        /// <summary>
        /// returns if install operation is needed </summary>
        /// <returns> true if install operation is need, false otherwise </returns>
        public virtual bool InstallOperationIsNeeded
        {
            get
            {
                return installOperationIsNeeded;
            }
        }

        /// <returns> item size starting with current step (e.g if current step is 0, returns size for all sub-items, otherwise the size starting with current-sub-item) </returns>
        public virtual long RemainingSize
        {
            get
            {
                long remainingSize = 0;
                if (downloadSteps != null)
                {
                    for (int i = currentStepIndex; i < downloadSteps.Count; i++)
                    {
                        SKToolsFileDownloadStep currentStep = downloadSteps[i];
                        if (currentStep != null)
                        {
                            remainingSize += currentStep.DownloadItemSize;
                        }
                    }
                }
                return remainingSize;
            }
        }

        public override bool Equals(object another)
        {
            if (another == null)
            {
                return false;
            }
            else if (!(another is SKToolsDownloadItem))
            {
                return false;
            }
            else
            {
                SKToolsDownloadItem anotherItem = (SKToolsDownloadItem)another;
                if ((itemCode == null) || (anotherItem.ItemCode == null))
                {
                    return false;
                }
                return itemCode.Equals(anotherItem.ItemCode);
            }
        }
    }
}