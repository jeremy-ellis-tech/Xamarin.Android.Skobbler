
using Android.App;
using Skobbler.Ngx.Map;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsNavigationManager
    {

        public SKToolsNavigationManager(Activity activity, int rootId)
        {
            SKToolsLogicManager.Instance.SetActivity(activity, rootId);
        }

        /// <summary>
        /// Starts a route calculation. </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void LaunchRouteCalculation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.CalculateRoute(configuration, mapView);
        }

        /// <summary>
        /// Removes the screen with the route calculation.
        /// </summary>
        public virtual void RemoveRouteCalculationScreen()
        {
            SKToolsLogicManager.Instance.RemoveRouteCalculationScreen();
        }

        /// <summary>
        /// Starts the navigation </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void StartNavigation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.StartNavigation(configuration, mapView, false);
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        public virtual void StopNavigation()
        {
            SKToolsLogicManager.Instance.StopNavigation();
        }

        /// <summary>
        /// Starts free drive. </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void StartFreeDriveWithConfiguration(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.StartNavigation(configuration, mapView, true);
        }

        /// <summary>
        /// Method that should be called when the orientation of the activity has changed.
        /// </summary>
        public virtual void NotifyOrientationChanged()
        {
            SKToolsLogicManager.Instance.NotifyOrientationChanged();
        }

        /// <summary>
        /// Sets the listener </summary>
        /// <param name="navigationListener"> </param>
        public virtual void SetNavigationListener(ISKToolsNavigationListener value)
        {
            SKToolsLogicManager.Instance.SetNavigationListener(value);
        }
    }
}