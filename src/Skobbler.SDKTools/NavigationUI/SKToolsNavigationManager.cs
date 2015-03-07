using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Map;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsNavigationManager
    {

        public SKToolsNavigationManager(Activity activity, int rootId)
        {
            SKToolsLogicManager.Instance.setActivity(activity, rootId);
        }

        /// <summary>
        /// Starts a route calculation. </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void launchRouteCalculation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.calculateRoute(configuration, mapView);
        }

        /// <summary>
        /// Removes the screen with the route calculation.
        /// </summary>
        public virtual void removeRouteCalculationScreen()
        {
            SKToolsLogicManager.Instance.removeRouteCalculationScreen();
        }

        /// <summary>
        /// Starts the navigation </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void startNavigation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.startNavigation(configuration, mapView, false);
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        public virtual void stopNavigation()
        {
            SKToolsLogicManager.Instance.stopNavigation();
        }

        /// <summary>
        /// Starts free drive. </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        public virtual void startFreeDriveWithConfiguration(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            SKToolsLogicManager.Instance.startNavigation(configuration, mapView, true);
        }

        /// <summary>
        /// Method that should be called when the orientation of the activity has changed.
        /// </summary>
        public virtual void notifyOrientationChanged()
        {
            SKToolsLogicManager.Instance.notifyOrientationChanged();
        }

        /// <summary>
        /// Sets the listener </summary>
        /// <param name="navigationListener"> </param>
        public virtual ISKToolsNavigationListener NavigationListener
        {
            set
            {
                SKToolsLogicManager.Instance.NavigationListener = value;
            }
        }
    }
}