using Skobbler.Additions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skobbler.Ngx.Search
{
    public partial class SKSearchManager
    {
        private readonly ISKSearchListener _searchListener;
        private readonly TaskCompletionSource<IList<SKSearchResult>> _taskCompletionSource;

        public SKSearchManager()
        {
            _taskCompletionSource = new TaskCompletionSource<IList<SKSearchResult>>();
            _searchListener = new SKSearchListener(_taskCompletionSource);

            SetSearchListener(_searchListener);
        }

        public async Task<IList<SKSearchResult>> NearbySearchAsync(SKNearbySearchSettings nearbySearchObj)
        {
            SKSearchStatus searchStatus = NearbySearch(nearbySearchObj);

            if(searchStatus != SKSearchStatus.SkSearchNoError)
            {
                _taskCompletionSource.SetException(new Exception(searchStatus.ToString()));
            }

            return await _taskCompletionSource.Task;
        }
    }
}