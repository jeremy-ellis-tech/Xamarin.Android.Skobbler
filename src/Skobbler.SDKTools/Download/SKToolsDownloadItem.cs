using System.Collections.Generic;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadItem
    {
        private readonly IList<SKToolsFileDownloadStep> _downloadSteps;
        private long _noDownloadedBytes;
        private long _noDownloadedBytesInThisConnection;
        private readonly bool _unzipIsNeeded;

        public SKToolsDownloadItem(string itemCode, IList<SKToolsFileDownloadStep> downloadSteps, SKDownloadState skDownloadState, bool unzipIsNeeded, bool installOperationIsNeeded)
        {
            ItemCode = itemCode;
            _downloadSteps = downloadSteps;
            SKDownloadState = skDownloadState;
            _unzipIsNeeded = unzipIsNeeded;
            InstallOperationIsNeeded = installOperationIsNeeded;
        }

        public virtual bool UnzipIsNeeded()
        {
            return _unzipIsNeeded;
        }

        /// <summary>
        /// Current download step index
        /// </summary>
        public virtual sbyte CurrentStepIndex { set; get; }


        /// <summary>
        /// gets current download step object </summary>
        /// <returns> current download step </returns>
        public virtual SKToolsFileDownloadStep CurrentDownloadStep
        {
            get
            {
                if (_downloadSteps != null && _downloadSteps.Count > CurrentStepIndex)
                {
                    return _downloadSteps[CurrentStepIndex];
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
                return _downloadSteps != null && _downloadSteps.Count <= CurrentStepIndex;
            }
        }

        /// <summary>
        /// go to next download step
        /// </summary>
        public virtual void GoToNextDownloadStep()
        {
            CurrentStepIndex++;
        }

        /// <summary>
        /// Download state for current item
        /// </summary>
        public virtual SKDownloadState SKDownloadState { get; set; }


        /// <summary>
        /// The number of downloaded bytes
        /// </summary>
        public virtual long NoDownloadedBytes
        {
            get
            {
                return _noDownloadedBytes;
            }
            set
            {
                _noDownloadedBytes = value;
                for (int i = 0; i < CurrentStepIndex; i++)
                {
                    if ((_downloadSteps != null) && (i < _downloadSteps.Count))
                    {
                        SKToolsFileDownloadStep currentStep = _downloadSteps[i];
                        if (currentStep != null)
                        {
                            _noDownloadedBytes += currentStep.DownloadItemSize;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Current download item code
        /// </summary>
        public virtual string ItemCode { get; private set; }

        /// <summary>
        /// Marks current item as NOT-QUEUED (e.g. if its download is cancelled from some reason)
        /// </summary>
        public virtual void MarkAsNotQueued()
        {
            // removes already downloaded bytes from current item
            for (int i = 0; i <= CurrentStepIndex; i++)
            {
                if ((_downloadSteps != null) && (i < _downloadSteps.Count))
                {
                    SKToolsFileDownloadStep currentStep = _downloadSteps[i];
                    if (currentStep != null)
                    {
                        SKToolsDownloadUtils.RemoveCurrentLocationFromDisk(currentStep.DestinationPath);
                    }
                }
            }
            // revert current item state
            _noDownloadedBytes = 0;
            _noDownloadedBytesInThisConnection = 0;
            SKDownloadState = SKDownloadState.NotQueued;
            CurrentStepIndex = 0;
        }

        /// <summary>
        /// Sets the number of downloaded bytes during current internet connection
        /// </summary>
        /// <param name="noDownloadedBytesInThisConnection"> no downloaded bytes that will be set </param>
        public virtual void SetNoDownloadedBytesInThisConnection(long noDownloadedBytesInThisConnection)
        {
            _noDownloadedBytesInThisConnection = noDownloadedBytesInThisConnection;
        }

        /// <summary>
        /// Gets current download step destination path
        /// </summary>
        public virtual string CurrentStepDestinationPath
        {
            get
            {
                if ((_downloadSteps != null) && (CurrentStepIndex < _downloadSteps.Count))
                {
                    return _downloadSteps[CurrentStepIndex].DestinationPath;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns if install operation is needed
        /// </summary>
        /// <returns> True if install operation is need, false otherwise </returns>
        public virtual bool InstallOperationIsNeeded { get; private set; }

        /// <summary>
        /// Item size starting with current step (e.g if current step is 0, returns size for all sub-items,
        /// otherwise the size starting with current-sub-item)
        /// </summary>
        public virtual long RemainingSize
        {
            get
            {
                long remainingSize = 0;
                if (_downloadSteps != null)
                {
                    for (int i = CurrentStepIndex; i < _downloadSteps.Count; i++)
                    {
                        SKToolsFileDownloadStep currentStep = _downloadSteps[i];
                        if (currentStep != null)
                        {
                            remainingSize += currentStep.DownloadItemSize;
                        }
                    }
                }
                return remainingSize;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this)) return true;

            var other = obj as SKToolsDownloadItem;

            if (other == null) return false;

            return other.ItemCode != null
                    && ItemCode != null
                    && Equals(ItemCode, other.ItemCode);
        }

        public override int GetHashCode()
        {
            return ItemCode.GetHashCode();
        }
    }
}