using Skobbler.Ngx.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using JavaObject = Java.Lang.Object;

namespace Skobbler.Additions
{
    internal sealed class SKSearchListener : JavaObject, ISKSearchListener
    {
        private readonly TaskCompletionSource<IList<SKSearchResult>> _taskCompletionSource;

        public SKSearchListener(TaskCompletionSource<IList<SKSearchResult>> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            _taskCompletionSource.TrySetResult(results);
        }
    }
}