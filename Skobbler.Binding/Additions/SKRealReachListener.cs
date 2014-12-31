using Skobbler.Ngx.Map.RealReach;
using System.Threading.Tasks;
using JavaObject = Java.Lang.Object;

namespace Skobbler.Additions
{
    internal sealed class SKRealReachListener : JavaObject, ISKRealReachListener
    {
        private readonly TaskCompletionSource<SKRealReachCalculationResult> _taskCompletionSource;

        public SKRealReachListener(TaskCompletionSource<SKRealReachCalculationResult> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public void OnRealReachCalculationCompleted(int xMin, int xMax, int yMin, int yMax)
        {
            var result = new SKRealReachCalculationResult(xMin, xMax, yMin, yMax);
            _taskCompletionSource.TrySetResult(result);
        }
    }
}