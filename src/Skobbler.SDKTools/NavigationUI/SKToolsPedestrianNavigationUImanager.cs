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

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsPedestrianNavigationUIManager
    {


        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static SKToolsPedestrianNavigationUIManager instance = null;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity currentActivity;
        /// <summary>
        /// root layout - to this will be added all views
        /// </summary>
        private ViewGroup rootLayout;
        /// <summary>
        /// the settings from navigations
        /// </summary>
        private ViewGroup settingsPanel;

        /*
        the view for pre navigation
         */
        private ViewGroup preNavigationPanel;

        /*
        the view for pre navigation increase decrease buttons
         */
        private ViewGroup navigationSimulationPanel;

        /// <summary>
        /// the back button
        /// </summary>
        private ViewGroup backButtonPanel;
        /// <summary>
        /// compass panel
        /// </summary>
        protected internal ViewGroup compassPanel;
        /// <summary>
        /// pedestrian follower mode toast
        /// </summary>
        private static Toast pedestrianFollowerModeToast;


        /// <summary>
        /// Creates a single instance of <seealso cref="SKToolsPedestrianNavigationUIManager"/>
        /// 
        /// @return
        /// </summary>
        public static SKToolsPedestrianNavigationUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(SKToolsNavigationUIManager))
                    {
                        if (instance == null)
                        {
                            instance = new SKToolsPedestrianNavigationUIManager();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Sets the current activity.
        /// </summary>
        /// <param name="activity"> </param>
        /// <param name="rootId"> </param>
        protected internal virtual void setActivity(Activity activity, int rootId)
        {
            this.currentActivity = activity;
            rootLayout = currentActivity.FindViewById<ViewGroup>(rootId);
        }


    }
}