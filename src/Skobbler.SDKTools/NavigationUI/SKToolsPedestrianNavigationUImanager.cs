using Android.App;
using Android.Views;
using Android.Widget;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsPedestrianNavigationUiManager
    {
        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static SKToolsPedestrianNavigationUiManager _instance;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity _currentActivity;
        /// <summary>
        /// root layout - to this will be added all views
        /// </summary>
        private ViewGroup _rootLayout;
        /// <summary>
        /// the settings from navigations
        /// </summary>
        private ViewGroup _settingsPanel;

        /*
        the view for pre navigation
         */
        private ViewGroup _preNavigationPanel;

        /*
        the view for pre navigation increase decrease buttons
         */
        private ViewGroup _navigationSimulationPanel;

        /// <summary>
        /// the back button
        /// </summary>
        private ViewGroup _backButtonPanel;
        /// <summary>
        /// compass panel
        /// </summary>
        protected internal ViewGroup CompassPanel;
        /// <summary>
        /// pedestrian follower mode toast
        /// </summary>
        private static Toast _pedestrianFollowerModeToast;


        /// <summary>
        /// Creates a single instance
        /// </summary>
        public static SKToolsPedestrianNavigationUiManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(SKToolsNavigationUiManager))
                    {
                        if (_instance == null)
                        {
                            _instance = new SKToolsPedestrianNavigationUiManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sets the current activity.
        /// </summary>
        /// <param name="activity">The activity</param>
        /// <param name="rootId">Root layout Id</param>
        protected internal virtual void SetActivity(Activity activity, int rootId)
        {
            _currentActivity = activity;
            _rootLayout = _currentActivity.FindViewById<ViewGroup>(rootId);
        }


    }
}