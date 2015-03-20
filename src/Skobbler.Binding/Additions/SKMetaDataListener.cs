using System.Threading.Tasks;
using JavaObject = Java.Lang.Object;

namespace Skobbler.Ngx.Versioning
{
    internal sealed class SKMetaDataListener : JavaObject, ISKMetaDataListener
    {
        private readonly TaskCompletionSource<int> _taskCompletionSource;

        public SKMetaDataListener(TaskCompletionSource<int> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public void OnMetaDataDownloadFinished(int versionNumber)
        {
            _taskCompletionSource.TrySetResult(versionNumber);
        }
    }
}