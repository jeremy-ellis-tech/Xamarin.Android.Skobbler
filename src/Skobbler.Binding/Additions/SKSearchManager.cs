using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skobbler.Ngx.Search
{
    public partial class SKSearchManager
    {
        public SKSearchManager() : this(null) { }

        /// <summary>
        /// An asynchronous wrapper around Skobbler's NearbySearch()
        /// </summary>
        /// <param name="nearbySearchObj">The nearby search object</param>
        /// <exception cref="SKSearchStatusException">Thrown if the searchStatus returned by NearbySearch() is not equal to SkSearchNoError</exception>
        /// <returns>An IList of SKSearchResult</returns>
        public async Task<IList<SKSearchResult>> NearbySearchAsync(SKNearbySearchSettings nearbySearchObj)
        {
            var taskCompletionSource = new TaskCompletionSource<IList<SKSearchResult>>();
            var searchListener = new SKSearchListener(taskCompletionSource);

            SetSearchListener(searchListener);

            SKSearchStatus searchStatus = NearbySearch(nearbySearchObj);

            if (searchStatus != SKSearchStatus.SkSearchNoError)
            {
                taskCompletionSource.TrySetException(new SKSearchStatusException(searchStatus, "The search status returned by NearbySearch is not SKSearchNoError"));
            }

            return await taskCompletionSource.Task;
        }

        /// <summary>
        /// An asynchoronous wrapper around Skobbler's MultistepSearch()
        /// </summary>
        /// <param name="stepObject">The multistep search object</param>
        /// <returns>An IList of SKSearchResult</returns>
        public async Task<IList<SKSearchResult>> MultistepSearchAsync(SKMultiStepSearchSettings stepObject)
        {
            var taskCompletionSource = new TaskCompletionSource<IList<SKSearchResult>>();
            var searchListener = new SKSearchListener(taskCompletionSource);

            SetSearchListener(searchListener);

            MultistepSearch(stepObject);

            return await taskCompletionSource.Task;
        }
    }
}