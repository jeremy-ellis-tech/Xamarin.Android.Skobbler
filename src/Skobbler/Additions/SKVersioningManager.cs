using System;
using System.Threading.Tasks;

namespace Skobbler.Ngx.Versioning
{
    public partial class SKVersioningManager
    {
        /// <summary>
        /// An asynchronous wrapper around Skobbler's DownloadMetaData()
        /// Generates request to download the metadata.
        /// Before calling this method, make sure that SKMaps.initializeSKMaps(android.content.Context, SKMapsInitSettings, String) is called.
        /// </summary>
        /// <param name="versionNumber">Version number that has to be downloaded. If it has value 0, will be used current version</param>
        /// <returns>Map version</returns>
        public async Task<int> DownloadMetaDataAsync(int versionNumber)
        {
            var taskCompletionSource = new TaskCompletionSource<int>();
            var metaDataListener = new SKMetaDataListener(taskCompletionSource);

            SetMetaDataListener(metaDataListener);

            bool downloadStarted = DownloadMetaData(versionNumber);

            if (!downloadStarted)
            {
                taskCompletionSource.TrySetException(new Exception("The download could not be started"));
            }

            return await taskCompletionSource.Task;
        }
    }
}