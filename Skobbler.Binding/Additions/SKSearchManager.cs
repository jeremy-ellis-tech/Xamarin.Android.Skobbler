using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skobbler.Ngx.Search
{
    public partial class SKSearchManager
    {
        private readonly ISKSearchListener _searchListener;
        private readonly TaskCompletionSource<IList<SKSearchResult>> _taskCompletionSource;

        public SKSearchManager() : this(null)
        {
            _taskCompletionSource = new TaskCompletionSource<IList<SKSearchResult>>();
            _searchListener = new SKSearchListener(_taskCompletionSource);

            SetSearchListener(_searchListener);
        }

        /// <summary>
        /// An asynchronous wrapper around Skobbler's NearbySearch()
        /// </summary>
        /// <param name="nearbySearchObj">The nearby search object</param>
        /// <exception cref="SKSearchStatusException">Thrown if the searchStatus returned by NearbySearch() is not equal to SkSearchNoError</exception>
        /// <returns>An IList of SKSearchResult</returns>
        public async Task<IList<SKSearchResult>> NearbySearchAsync(SKNearbySearchSettings nearbySearchObj)
        {
            SKSearchStatus searchStatus = NearbySearch(nearbySearchObj);

            if(searchStatus != SKSearchStatus.SkSearchNoError)
            {
                _taskCompletionSource.TrySetException(new SKSearchStatusException(searchStatus));
            }

            return await _taskCompletionSource.Task;
        }

        /// <summary>
        /// An asynchoronous wrapper around Skobbler's MultistepSearch()
        /// </summary>
        /// <param name="stepObject">The multistep search object</param>
        /// <returns>An IList of SKSearchResult</returns>
        public async Task<IList<SKSearchResult>> MultistepSearchAsync(SKMultiStepSearchSettings stepObject)
        {
            MultistepSearch(stepObject);

            return await _taskCompletionSource.Task;
        }
    }
}