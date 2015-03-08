using System;
using System.Collections.Generic;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadItem
    {
        public const sbyte NotQueued = 0;
        public const sbyte Queued = 1;
        public const sbyte Downloading = 2;
        public const sbyte Paused = 3;
        public const sbyte Downloaded = 4;
        public const sbyte Installing = 5;
        public const sbyte Installed = 6;

        private string _itemCode;
        private sbyte _downloadState;
        private IList<SKToolsFileDownloadStep> _downloadSteps;
        private sbyte _currentStepIndex;
        private long _noDownloadedBytes;
        private long _noDownloadedBytesInThisConnection;
        private bool _unzipIsNeeded;
        private bool _installOperationIsNeeded;

        public SKToolsDownloadItem(string itemCode, IList<SKToolsFileDownloadStep> downloadSteps, sbyte downloadState, bool unzipIsNeeded, bool installOperationIsNeeded)
        {
            _itemCode = itemCode;
            _downloadSteps = downloadSteps;
            _downloadState = downloadState;
            _unzipIsNeeded = unzipIsNeeded;
            _installOperationIsNeeded = installOperationIsNeeded;
        }

        public virtual bool UnzipIsNeeded()
        {
            return _unzipIsNeeded;
        }

        /// <summary>
        /// sets current download step index </summary>
        /// <param name="currentStepIndex"> current step index </param>
        public virtual sbyte CurrentStepIndex
        {
            set
            {
                this._currentStepIndex = value;
            }
            get
            {
                return this._currentStepIndex;
            }
        }


        /// <summary>
        /// gets current download step object </summary>
        /// <returns> current download step </returns>
        public virtual SKToolsFileDownloadStep CurrentDownloadStep
        {
            get
            {
                if (_downloadSteps != null && _downloadSteps.Count > _currentStepIndex)
                {
                    return _downloadSteps[_currentStepIndex];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// returns if current item is downloaded </summary>
        /// <returns> true if current item is downloaded, false otherwise </returns>
        public virtual bool DownloadFinished
        {
            get
            {
                return _downloadSteps != null && _downloadSteps.Count <= _currentStepIndex;
            }
        }

        /// <summary>
        /// go to next download step
        /// </summary>
        public virtual void GoToNextDownloadStep()
        {
            _currentStepIndex++;
        }

        /// <summary>
        /// sets download state for current item </summary>
        /// <param name="downloadState"> download state for current item </param>
        public virtual sbyte DownloadState
        {
            set
            {
                this._downloadState = value;
            }
            get
            {
                return this._downloadState;
            }
        }


        /// <summary>
        /// gets the number of downloaded bytes </summary>
        /// <returns> no downloaded bytes </returns>
        public virtual long NoDownloadedBytes
        {
            get
            {
                return _noDownloadedBytes;
            }
            set
            {
                this._noDownloadedBytes = value;
                for (int i = 0; i < _currentStepIndex; i++)
                {
                    if ((_downloadSteps != null) && (i < _downloadSteps.Count))
                    {
                        SKToolsFileDownloadStep currentStep = _downloadSteps[i];
                        if (currentStep != null)
                        {
                            this._noDownloadedBytes += currentStep.DownloadItemSize;
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
                return _itemCode;
            }
        }

        /// <summary>
        /// marks current item as NOT-QUEUED (e.g. if its download is cancelled from some reason)
        /// </summary>
        public virtual void MarkAsNotQueued()
        {
            // removes already downloaded bytes from current item
            for (int i = 0; i <= _currentStepIndex; i++)
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
            _downloadState = NotQueued;
            _currentStepIndex = 0;
        }

        /// <summary>
        /// sets the number of downloaded bytes during current internet connection </summary>
        /// <param name="noDownloadedBytesInThisConnection"> no downloaded bytes that will be set </param>
        public virtual long NoDownloadedBytesInThisConnection
        {
            set
            {
                this._noDownloadedBytesInThisConnection = value;
            }
        }

        /// <summary>
        /// gets current download step destination path
        /// </summary>
        public virtual string CurrentStepDestinationPath
        {
            get
            {
                if ((_downloadSteps != null) && (_currentStepIndex < _downloadSteps.Count))
                {
                    return _downloadSteps[_currentStepIndex].DestinationPath;
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
                return _installOperationIsNeeded;
            }
        }

        /// <returns> item size starting with current step (e.g if current step is 0, returns size for all sub-items, otherwise the size starting with current-sub-item) </returns>
        public virtual long RemainingSize
        {
            get
            {
                long remainingSize = 0;
                if (_downloadSteps != null)
                {
                    for (int i = _currentStepIndex; i < _downloadSteps.Count; i++)
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