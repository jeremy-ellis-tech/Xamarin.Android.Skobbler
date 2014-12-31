using Skobbler.Additions;
using Skobbler.Ngx.Map.RealReach;
using System.Threading.Tasks;

namespace Skobbler.Ngx.Map
{
    public partial class SKMapSurfaceView
    {
        /// <summary>
        /// An asynchronous wrapper around Skobbler's DisplayRealReachWithSettings().
        /// Adds a RealReachLayer on the map.
        /// </summary>
        /// <param name="realReachSettings">Contains information about the RealReach layer</param>
        /// <returns>The RealReach xMin, xMax, yMin, yMax positions</returns>
        public async Task<SKRealReachCalculationResult> DisplayRealReachWithSettingsAsync(SKRealReachSettings realReachSettings)
        {
            var taskCompletionSource = new TaskCompletionSource<SKRealReachCalculationResult>();
            var realReachListener = new SKRealReachListener(taskCompletionSource);

            SetRealReachListener(realReachListener);

            DisplayRealReachWithSettings(realReachSettings);

            return await taskCompletionSource.Task;
        }
    }
}