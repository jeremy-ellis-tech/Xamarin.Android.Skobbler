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
    internal class SKToolsNavigationUIManager
    {

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static volatile SKToolsNavigationUIManager instance = null;

        /// <summary>
        /// OSM street types
        /// </summary>
        private const int OSM_STREET_TYPE_MOTORWAY = 9;

        private const int OSM_STREET_TYPE_MOTORWAY_LINK = 10;

        private const int OSM_STREET_TYPE_PRIMARY = 13;

        private const int OSM_STREET_TYPE_PRIMARY_LINK = 14;

        private const int OSM_STREET_TYPE_TRUNK = 24;

        private const int OSM_STREET_TYPE_TRUNK_LINK = 25;

        /// <summary>
        /// the list with the country codes for which the top panel has a different
        /// color
        /// </summary>
        private static readonly string[] signPostsCountryExceptions = new string[] { "DE", "AT", "GB", "IE", "CH", "US" };


        private enum NavigationMode
        {

            SETTINGS,

            ROUTE_INFO,

            ROUTE_OVERVIEW,

            PANNING,

            ROADBLOCK,

            FOLLOWER,

            PRE_NAVIGATION,

            POST_NAVIGATION

        }

        private NavigationMode currentNavigationMode;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity currentActivity;

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
        /// route overview panel
        /// </summary>
        private ViewGroup routeOverviewPanel;

        /// <summary>
        /// the view for re routing
        /// </summary>
        private ViewGroup reRoutingPanel;

        /// <summary>
        /// arriving navigation distance panel
        /// </summary>
        private ViewGroup routeDistancePanel;

        /// <summary>
        /// the  position me button
        /// </summary>
        private ViewGroup positionMeButtonPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup topCurrentNavigationPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup topNextNavigationPanel;

        /// <summary>
        /// the menu from tapping mode
        /// </summary>
        private ViewGroup menuOptions;

        /// <summary>
        /// root layout - to this will be added all views
        /// </summary>
        private ViewGroup rootLayout;

        /// <summary>
        /// road block screen
        /// </summary>
        private ViewGroup roadBlockPanel;

        /// <summary>
        /// current street name panel from free drive mode
        /// </summary>
        private ViewGroup freeDriveCurrentStreetPanel;

        /// <summary>
        /// via point panel
        /// </summary>
        private ViewGroup viaPointPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup speedPanel;

        /// <summary>
        /// the view flipper for estimated and arriving time panels
        /// </summary>
        private ViewGroup arrivingETATimeGroupPanels;

        /// <summary>
        /// bottom right panel with estimated time
        /// </summary>
        private ViewGroup estimatedTimePanel;

        /// <summary>
        /// bottom right panel with arriving time
        /// </summary>
        private ViewGroup arrivingTimePanel;

        /// <summary>
        /// navigation arriving time text
        /// </summary>
        private TextView arrivingTimeText;

        /// <summary>
        /// navigation estimated time text
        /// </summary>
        private TextView estimatedTimeText;

        /// <summary>
        /// true if the estimated time panel is visible, false if the arriving time
        /// panel is visible
        /// </summary>
        private bool estimatedTimePanelVisible;

        /// <summary>
        /// top current navigation distance & street panel
        /// </summary>
        private LinearLayout topCurrentNavigationDistanceStreetPanel;

        /// <summary>
        /// the image for the visual advice
        /// </summary>
        private ImageView nextAdviceImageView;

        /// <summary>
        /// next advice image distance panel
        /// </summary>
        private RelativeLayout nextAdviceImageDistancePanel;

        /// <summary>
        /// next advice street name panel
        /// </summary>
        private RelativeLayout nextAdviceStreetNamePanel;

        /// <summary>
        /// next advice street name text
        /// </summary>
        private TextView nextAdviceStreetNameTextView;

        /// <summary>
        /// arriving navigation distance text
        /// </summary>
        private TextView routeDistanceText;

        /// <summary>
        /// arriving navigation distance text value
        /// </summary>
        private TextView routeDistanceTextValue;

        /// <summary>
        /// the alternative routes buttons
        /// </summary>
        private TextView[] altRoutesButtons;

        /// <summary>
        /// true, if free drive selected
        /// </summary>
        private bool isFreeDrive;

        /// <summary>
        /// true if the country code for destination is US, false otherwise
        /// </summary>
        private bool isUS;

        /// <summary>
        /// true if the country code is different from US, false otherwise
        /// </summary>
        private bool isDefaultSpeedSign;

        /// <summary>
        /// flag that indicates if there is a speed limit on the current road
        /// </summary>
        private bool speedLimitAvailable;

        /// <summary>
        /// current street name from free drive mode
        /// </summary>
        private string currentStreetNameFreeDriveString;

        /// <summary>
        /// true if the background color for top panel is the default one, false
        /// otherwise
        /// </summary>
        private bool isDefaultTopPanelBackgroundColor = true;

        /// <summary>
        /// the drawable id for the current advice background
        /// </summary>
        private int currentAdviceBackgroundDrawableId;

        /// <summary>
        /// the drawable id for the next advice background
        /// </summary>
        private int nextAdviceBackgroundDrawableId;

        /// <summary>
        /// the thread for speed exceeded
        /// </summary>
        private SpeedExceededThread speedExceededThread;

        /// <summary>
        /// current street type
        /// </summary>
        protected internal int nextStreetType;

        /// <summary>
        /// next street type
        /// </summary>
        protected internal int secondNextStreetType;

        /// <summary>
        /// the current estimated total distance of the navigation trip
        /// </summary>
        private long navigationTotalDistance;

        /// <summary>
        /// current speed limit
        /// </summary>
        protected internal double currentSpeedLimit;

        /// <summary>
        /// true if the speed limit is exceeded, false otherwise
        /// </summary>
        protected internal bool speedLimitExceeded;

        /// <summary>
        /// false when we start a navigation, becomes true when first advice is
        /// received
        /// </summary>
        protected internal bool firstAdviceReceived;

        /// <summary>
        /// true if next advice is visible, false otherwise
        /// </summary>
        protected internal bool isNextAdviceVisible;

        /// <summary>
        /// true when the application starts
        /// </summary>
        private bool firstTimeNavigation = true;

        /// <summary>
        /// the currently estimated duration of the route to the navi destination
        /// </summary>
        protected internal int timeToDestination;

        protected internal int initialTimeToDestination;

        /// <summary>
        /// whether to display the navigation flag or not
        /// </summary>
        public bool showDestinationReachedFlag;

        /// <summary>
        /// the distance estimated between the user current position and destination
        /// </summary>
        public static int distanceEstimatedUntilDestination;

        /// <summary>
        /// current speed
        /// </summary>
        private double currentSpeed;

        /// <summary>
        /// route distance string
        /// </summary>
        protected internal string routeDistanceString;

        /// <summary>
        /// route distance value string
        /// </summary>
        protected internal string routeDistanceValueString;

        /// <summary>
        /// current street name
        /// </summary>
        protected internal TextView currentAdviceName;

        /// <summary>
        /// current advice distance
        /// </summary>
        protected internal TextView currentAdviceDistance;

        /// <summary>
        /// current advice street name string
        /// </summary>
        protected internal string currentVisualAdviceStreetName;

        /// <summary>
        /// current advice distance string
        /// </summary>
        protected internal int currentVisualAdviceDistance;

        /// <summary>
        /// next advice street name
        /// </summary>
        protected internal string nextVisualAdviceStreetName;

        /// <summary>
        /// current speed text from free drive mode
        /// </summary>
        protected internal TextView currentSpeedText;

        /// <summary>
        /// current speed text from free drive mode
        /// </summary>
        protected internal TextView currentSpeedTextValue;

        /// <summary>
        /// current speed string
        /// </summary>
        protected internal string currentSpeedString;

        /// <summary>
        /// next advice distance
        /// </summary>
        protected internal int nextVisualAdviceDistance;

        /// <summary>
        /// next advice distance text
        /// </summary>
        protected internal TextView nextAdviceDistanceTextView;

        /// <summary>
        /// current speed unit for navigation mode
        /// </summary>
        private SKMaps.SKDistanceUnitType distanceUnitType;

        /// <summary>
        /// the image for the visual advice
        /// </summary>
        protected internal ImageView currentAdviceImage;

        /// <summary>
        /// country code
        /// </summary>
        protected internal string currentCountryCode;

        /// <summary>
        /// Click listener for settings menu views
        /// </summary>
        private Android.Views.View.IOnClickListener settingsItemsClickListener = new OnClickListenerAnonymousInnerClassHelper();

        private class OnClickListenerAnonymousInnerClassHelper : View.IOnClickListener
        {
            public OnClickListenerAnonymousInnerClassHelper()
            {
            }


            public override void onClick(View v)
            {
                SKToolsLogicManager.Instance.handleSettingsItemsClick(v);
            }
        }

        /// <summary>
        /// Click listener for the rest of the views
        /// </summary>
        private Android.Views.View.IOnClickListener itemsClickListener = new OnClickListenerAnonymousInnerClassHelper2();

        private class OnClickListenerAnonymousInnerClassHelper2 : View.IOnClickListener
        {
            public OnClickListenerAnonymousInnerClassHelper2()
            {
            }


            public override void onClick(View view)
            {
                SKToolsLogicManager.Instance.handleItemsClick(view);
            }
        }

        /// <summary>
        /// Block roads list item click
        /// </summary>
        private AdapterView.OnItemClickListener blockRoadsListItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper();

        private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
        {
            public OnItemClickListenerAnonymousInnerClassHelper()
            {
            }

            public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
            {
                SKToolsLogicManager.Instance.handleBlockRoadsItemsClick(parent, position);
            }
        }

        /// <summary>
        /// Creates a single instance of <seealso cref="SKToolsNavigationUIManager"/>
        /// 
        /// @return
        /// </summary>
        public static SKToolsNavigationUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(SKToolsNavigationUIManager))
                    {
                        if (instance == null)
                        {
                            instance = new SKToolsNavigationUIManager();
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
            rootLayout = (ViewGroup)currentActivity.findViewById(rootId);
        }


        /// <summary>
        /// Inflates navigation relates views.
        /// </summary>
        /// <param name="activity"> </param>
        protected internal virtual void inflateNavigationViews(Activity activity)
        {

            activity.runOnUiThread(new RunnableAnonymousInnerClassHelper(this));
        }

        private class RunnableAnonymousInnerClassHelper : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public RunnableAnonymousInnerClassHelper(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override void run()
            {

                LayoutInflater inflater = outerInstance.currentActivity.LayoutInflater;

                outerInstance.inflateSettingsMenu();

                android.widget.RelativeLayout.LayoutParams relativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);

                outerInstance.backButtonPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_back_button, null, false);
                outerInstance.rootLayout.addView(outerInstance.backButtonPanel, relativeLayoutParams);
                outerInstance.backButtonPanel.Id = SKToolsUtils.generateViewId();
                outerInstance.backButtonPanel.Visibility = View.GONE;
                outerInstance.backButtonPanel.findViewById(R.id.navigation_top_back_button).OnClickListener = itemsClickListener;

                android.widget.RelativeLayout.LayoutParams routeOverviewRelativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                routeOverviewRelativeLayoutParams.addRule(RelativeLayout.BELOW, outerInstance.backButtonPanel.Id);

                outerInstance.routeOverviewPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_route_overview_panel, null, false);
                outerInstance.rootLayout.addView(outerInstance.routeOverviewPanel, routeOverviewRelativeLayoutParams);
                outerInstance.routeOverviewPanel.Visibility = View.GONE;

                outerInstance.roadBlockPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_roadblocks_list, null, false);
                outerInstance.rootLayout.addView(outerInstance.roadBlockPanel, routeOverviewRelativeLayoutParams);
                outerInstance.roadBlockPanel.Visibility = View.GONE;


                outerInstance.reRoutingPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_rerouting_panel, null, false);
                android.widget.RelativeLayout.LayoutParams reRoutingPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                reRoutingPanelParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                outerInstance.rootLayout.addView(outerInstance.reRoutingPanel, reRoutingPanelParams);
                outerInstance.reRoutingPanel.Visibility = View.GONE;

                outerInstance.menuOptions = (ViewGroup)inflater.inflate(R.layout.element_navigation_menu_options, null, false);
                outerInstance.rootLayout.addView(outerInstance.menuOptions, relativeLayoutParams);
                outerInstance.menuOptions.Visibility = View.GONE;

                outerInstance.topCurrentNavigationPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_current_advice_panel, null, false);
                android.widget.RelativeLayout.LayoutParams topCurrentAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                topCurrentAdviceParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                outerInstance.rootLayout.addView(outerInstance.topCurrentNavigationPanel, topCurrentAdviceParams);
                outerInstance.topCurrentNavigationPanel.Id = SKToolsUtils.generateViewId();
                outerInstance.topCurrentNavigationPanel.measure(View.MeasureSpec.UNSPECIFIED, View.MeasureSpec.UNSPECIFIED);
                outerInstance.topCurrentNavigationPanel.Visibility = View.GONE;

                outerInstance.topCurrentNavigationDistanceStreetPanel = (LinearLayout)outerInstance.topCurrentNavigationPanel.findViewById(R.id.current_advice_text_holder);
                outerInstance.topCurrentNavigationDistanceStreetPanel.OnClickListener = itemsClickListener;
                RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)outerInstance.topCurrentNavigationPanel.findViewById(R.id.current_advice_image_holder);
                topCurrentNavigationImagePanel.OnClickListener = itemsClickListener;
                outerInstance.currentAdviceImage = (ImageView)topCurrentNavigationImagePanel.findViewById(R.id.current_advice_image_turn);
                outerInstance.currentAdviceName = (TextView)outerInstance.topCurrentNavigationDistanceStreetPanel.findViewById(R.id.current_advice_street_text);
                outerInstance.currentAdviceName.Selected = true;
                outerInstance.currentAdviceDistance = (TextView)outerInstance.topCurrentNavigationDistanceStreetPanel.findViewById(R.id.current_advice_distance_text);


                // next advice panel
                outerInstance.topNextNavigationPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_next_advice_panel, null, false);
                android.widget.RelativeLayout.LayoutParams nextAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                nextAdviceParams.addRule(RelativeLayout.BELOW, outerInstance.topCurrentNavigationPanel.Id);
                outerInstance.rootLayout.addView(outerInstance.topNextNavigationPanel, nextAdviceParams);
                outerInstance.topNextNavigationPanel.measure(View.MeasureSpec.UNSPECIFIED, View.MeasureSpec.UNSPECIFIED);
                outerInstance.topNextNavigationPanel.Visibility = View.GONE;

                outerInstance.nextAdviceImageDistancePanel = (RelativeLayout)outerInstance.topNextNavigationPanel.findViewById(R.id.next_image_turn_advice_distance_layout);
                outerInstance.nextAdviceImageView = (ImageView)outerInstance.nextAdviceImageDistancePanel.findViewById(R.id.next_image_turn_advice);
                outerInstance.nextAdviceDistanceTextView = (TextView)outerInstance.nextAdviceImageDistancePanel.findViewById(R.id.next_advice_distance_text);
                outerInstance.nextAdviceStreetNamePanel = (RelativeLayout)outerInstance.topNextNavigationPanel.findViewById(R.id.next_advice_street_name_text_layout);
                outerInstance.nextAdviceStreetNameTextView = (TextView)outerInstance.nextAdviceStreetNamePanel.findViewById(R.id.next_advice_street_name_text);
                outerInstance.nextAdviceStreetNameTextView.Selected = true;

                outerInstance.freeDriveCurrentStreetPanel = (ViewGroup)inflater.inflate(R.layout.element_free_drive_current_street_panel, null, false);
                android.widget.RelativeLayout.LayoutParams freeDrivePanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                freeDrivePanelParams.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                outerInstance.rootLayout.addView(outerInstance.freeDriveCurrentStreetPanel, freeDrivePanelParams);
                outerInstance.freeDriveCurrentStreetPanel.Visibility = View.GONE;
                TextView freeDriveCurrentStreetText = (TextView)outerInstance.freeDriveCurrentStreetPanel.findViewById(R.id.free_drive_current_street_text);
                freeDriveCurrentStreetText.Text = "";

                outerInstance.viaPointPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_via_point_panel, null, false);
                outerInstance.rootLayout.addView(outerInstance.viaPointPanel, freeDrivePanelParams);
                outerInstance.viaPointPanel.Visibility = View.GONE;
                TextView viaPointText = (TextView)outerInstance.viaPointPanel.findViewById(R.id.via_point_text_view);
                viaPointText.Text = "";

                outerInstance.inflateBottomPanels();
            }
        }

        /// <summary>
        /// Inflates the bottom views (speed panels, eta, route distance).
        /// </summary>
        private void inflateBottomPanels()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            speedPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_speed_panel, null, false);
            android.widget.RelativeLayout.LayoutParams currentSpeedParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
            currentSpeedParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
            currentSpeedParams.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
            rootLayout.addView(speedPanel, currentSpeedParams);
            speedPanel.Id = SKToolsUtils.generateViewId();
            speedPanel.Visibility = View.GONE;
            currentSpeedText = (TextView)speedPanel.findViewById(R.id.free_drive_current_speed_text);
            currentSpeedTextValue = (TextView)speedPanel.findViewById(R.id.free_drive_current_speed_text_value);
            speedPanel.OnClickListener = null;

            arrivingETATimeGroupPanels = (ViewGroup)inflater.inflate(R.layout.element_navigation_eta_arriving_group_panels, null, false);
            android.widget.RelativeLayout.LayoutParams etaGroupPanelsParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
            etaGroupPanelsParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
            etaGroupPanelsParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
            rootLayout.addView(arrivingETATimeGroupPanels, etaGroupPanelsParams);
            arrivingETATimeGroupPanels.Id = SKToolsUtils.generateViewId();
            arrivingETATimeGroupPanels.Visibility = View.GONE;
            estimatedTimePanel = (ViewGroup)arrivingETATimeGroupPanels.findViewById(R.id.navigation_bottom_right_estimated_panel);
            estimatedTimePanel.OnClickListener = itemsClickListener;
            arrivingTimePanel = (ViewGroup)arrivingETATimeGroupPanels.findViewById(R.id.navigation_bottom_right_arriving_panel);
            arrivingTimePanel.OnClickListener = itemsClickListener;
            estimatedTimeText = (TextView)estimatedTimePanel.findViewById(R.id.estimated_navigation_time_text);
            arrivingTimeText = (TextView)arrivingTimePanel.findViewById(R.id.arriving_time_text);


            android.widget.RelativeLayout.LayoutParams routeDistanceParams;
            if (currentActivity.Resources.Configuration.orientation == Configuration.ORIENTATION_PORTRAIT)
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                routeDistanceParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
                routeDistanceParams.addRule(RelativeLayout.LEFT_OF, arrivingETATimeGroupPanels.Id);
                routeDistanceParams.addRule(RelativeLayout.RIGHT_OF, speedPanel.Id);
            }
            else
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
                routeDistanceParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
                routeDistanceParams.addRule(RelativeLayout.ABOVE, arrivingETATimeGroupPanels.Id);
            }

            routeDistancePanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_route_distance, null, false);
            rootLayout.addView(routeDistancePanel, routeDistanceParams);
            routeDistancePanel.Visibility = View.GONE;
            routeDistanceText = (TextView)routeDistancePanel.findViewById(R.id.arriving_distance_text);
            routeDistanceTextValue = (TextView)routeDistancePanel.findViewById(R.id.arriving_distance_text_value);
            android.widget.RelativeLayout.LayoutParams positionMeParams;
            positionMeButtonPanel = (ViewGroup)inflater.inflate(R.layout.element_position_me_button, null, false);
            positionMeParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
            rootLayout.addView(positionMeButtonPanel, positionMeParams);
            positionMeButtonPanel.Visibility = View.GONE;
            positionMeButtonPanel.findViewById(R.id.position_me_real_navigation_button).OnClickListener = itemsClickListener;

        }

        /// <summary>
        /// Inflates simulation navigation type buttons.
        /// </summary>
        public virtual void inflateSimulationViews()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            navigationSimulationPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_simulation_buttons, null, false);
            RelativeLayout.LayoutParams simulationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WRAP_CONTENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
            navigationSimulationPanel.LayoutParams = simulationPanelParams;
            rootLayout.addView(navigationSimulationPanel, simulationPanelParams);
            navigationSimulationPanel.findViewById(R.id.menu_back_follower_mode_button).OnClickListener = itemsClickListener;
            navigationSimulationPanel.findViewById(R.id.navigation_increase_speed).OnClickListener = itemsClickListener;
            navigationSimulationPanel.findViewById(R.id.navigation_decrease_speed).OnClickListener = itemsClickListener;
        }

        /// <summary>
        /// Inflates simulation menu.
        /// </summary>
        private void inflateSettingsMenu()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;

            settingsPanel = (ViewGroup)inflater.inflate(R.layout.element_navigation_settings, null, false);
            RelativeLayout.LayoutParams settingsPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
            rootLayout.addView(settingsPanel, settingsPanelParams);
            settingsPanel.setVisibility(View.GONE);

            settingsPanel.findViewById(R.id.navigation_settings_audio_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_day_night_mode_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_overview_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_route_info_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_roadblock_info_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_panning_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_view_mode_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_quit_button).setOnClickListener(settingsItemsClickListener);
            settingsPanel.findViewById(R.id.navigation_settings_back_button).setOnClickListener(settingsItemsClickListener);
        }

        /// <summary>
        /// Inflates the pre navigation views, that contain the panels with alternative routes.
        /// </summary>
        private void inflatePreNavigationViews()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            preNavigationPanel = (ViewGroup)inflater.inflate(R.layout.element_pre_navigation_buttons_panel, null, false);
            RelativeLayout.LayoutParams preNavigationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.WRAP_CONTENT);
            rootLayout.addView(preNavigationPanel, preNavigationPanelParams);
            preNavigationPanel.Visibility = View.GONE;

            preNavigationPanel.findViewById(R.id.first_route).OnClickListener = itemsClickListener;
            preNavigationPanel.findViewById(R.id.second_route).OnClickListener = itemsClickListener;
            preNavigationPanel.findViewById(R.id.third_route).OnClickListener = itemsClickListener;
            preNavigationPanel.findViewById(R.id.cancel_pre_navigation_button).OnClickListener = itemsClickListener;
            preNavigationPanel.findViewById(R.id.menu_back_prenavigation_button).OnClickListener = itemsClickListener;
            preNavigationPanel.findViewById(R.id.start_navigation_button).OnClickListener = itemsClickListener;
        }

        /// <summary>
        /// Shows the start navigation button from pre navigation panel.
        /// </summary>
        public virtual void showStartNavigationPanel()
        {
            preNavigationPanel.findViewById(R.id.start_navigation_button).Visibility = View.VISIBLE;
        }

        /// <summary>
        /// Sets the pre navigation buttons, related to alternative routes with corresponding time and distance.
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="time"> </param>
        /// <param name="distance"> </param>
        public virtual void sePreNavigationButtons(int id, string time, string distance)
        {
            if (preNavigationPanel != null)
            {
                TextView oneRoute = (TextView)preNavigationPanel.findViewById(R.id.first_route);
                TextView twoRoutes = (TextView)preNavigationPanel.findViewById(R.id.second_route);
                TextView threeRoutes = (TextView)preNavigationPanel.findViewById(R.id.third_route);

                altRoutesButtons = new TextView[] { (TextView)preNavigationPanel.findViewById(R.id.first_route), (TextView)preNavigationPanel.findViewById(R.id.second_route), (TextView)preNavigationPanel.findViewById(R.id.third_route) };
                currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper2(this, id, time, distance, oneRoute, twoRoutes, threeRoutes));
            }
        }

        private class RunnableAnonymousInnerClassHelper2 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private int id;
            private string time;
            private string distance;
            private TextView oneRoute;
            private TextView twoRoutes;
            private TextView threeRoutes;

            public RunnableAnonymousInnerClassHelper2(SKToolsNavigationUIManager outerInstance, int id, string time, string distance, TextView oneRoute, TextView twoRoutes, TextView threeRoutes)
            {
                this.outerInstance = outerInstance;
                this.id = id;
                this.time = time;
                this.distance = distance;
                this.oneRoute = oneRoute;
                this.twoRoutes = twoRoutes;
                this.threeRoutes = threeRoutes;
            }

            public override void run()
            {
                if (id == 0)
                {
                    oneRoute.Visibility = View.VISIBLE;
                    twoRoutes.Visibility = View.GONE;
                    threeRoutes.Visibility = View.GONE;
                    outerInstance.altRoutesButtons[0].Text = time + "\n" + distance;
                }
                else if (id == 1)
                {
                    twoRoutes.Visibility = View.VISIBLE;
                    threeRoutes.Visibility = View.GONE;
                    outerInstance.altRoutesButtons[1].Text = time + "\n" + distance;
                }
                else if (id == 2)
                {
                    threeRoutes.Visibility = View.VISIBLE;
                    outerInstance.altRoutesButtons[2].Text = time + "\n" + distance;
                }
            }
        }

        /// <summary>
        /// Shows the pre navigation screen.
        /// </summary>
        public virtual void showPreNavigationScreen()
        {
            inflatePreNavigationViews();
            showViewIfNotVisible(preNavigationPanel);
            currentNavigationMode = NavigationMode.PRE_NAVIGATION;
        }

        /// <summary>
        /// Selects an alternative route button depending of the index.
        /// </summary>
        /// <param name="routeIndex"> </param>
        public virtual void selectAlternativeRoute(int routeIndex)
        {
            if (altRoutesButtons == null)
            {
                return;
            }
            foreach (TextView b in altRoutesButtons)
            {
                b.Selected = false;
            }
            altRoutesButtons[routeIndex].Selected = true;
        }

        /// <summary>
        /// Handles back button in pre navigation and follower mode.
        /// </summary>
        public virtual void handleNavigationBackButton()
        {

            if (currentNavigationMode == NavigationMode.PRE_NAVIGATION)
            {
                Button backButton = (Button)currentActivity.findViewById(R.id.menu_back_prenavigation_button);
                Button cancelButton = (Button)currentActivity.findViewById(R.id.cancel_pre_navigation_button);
                if (backButton.Text.Equals(">"))
                {
                    backButton.Text = "<";
                    cancelButton.Visibility = View.VISIBLE;
                }
                else
                {
                    backButton.Text = ">";
                    cancelButton.Visibility = View.GONE;
                }
            }
            else if (currentNavigationMode == NavigationMode.FOLLOWER)
            {
                Button backFollowerModeButton = (Button)currentActivity.findViewById(R.id.menu_back_follower_mode_button);
                RelativeLayout increaseDecreaseLayout = (RelativeLayout)currentActivity.findViewById(R.id.increase_decrease_layout);
                if (backFollowerModeButton.Text.Equals(">"))
                {
                    backFollowerModeButton.Text = "<";
                    increaseDecreaseLayout.Visibility = View.VISIBLE;
                }
                else
                {
                    backFollowerModeButton.Text = ">";
                    increaseDecreaseLayout.Visibility = View.GONE;
                }
            }
        }

        /// <summary>
        /// Changes the menu settings for free drive.
        /// </summary>
        /// <param name="isLandscape"> </param>
        public virtual bool SettingsMenuForFreeDrive
        {
            set
            {
                if (!value)
                {
                    currentActivity.findViewById(R.id.nav_settings_second_row).setVisibility(View.GONE);

                    TextView routeInfo = (TextView)currentActivity.findViewById(R.id.navigation_settings_roadblock_info_text);
                    routeInfo.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_routeinfo, 0, 0);
                    routeInfo.Text = currentActivity.Resources.getString(R.@string.navigate_settings_routeinfo);
                }
                else
                {
                    currentActivity.findViewById(R.id.navigation_settings_overview_button).setVisibility(View.GONE);
                    currentActivity.findViewById(R.id.navigation_settings_roadblock_info_button).setVisibility(View.GONE);
                }

            }
        }

        /// <summary>
        /// hide view is visible
        /// </summary>
        /// <param name="target"> </param>
        private void hideViewIfVisible(ViewGroup target)
        {
            if (target != null && target.Visibility == View.VISIBLE)
            {
                target.Visibility = View.GONE;
            }
        }

        /// <summary>
        /// shows the view if not visible
        /// </summary>
        /// <param name="target"> </param>
        private void showViewIfNotVisible(ViewGroup target)
        {
            if (target != null && target.Visibility == View.GONE)
            {
                target.Visibility = View.VISIBLE;
            }
        }

        /// <summary>
        /// Shows the block road screen.
        /// </summary>
        /// <param name="distanceUnit"> </param>
        /// <param name="distanceToDestination"> </param>
        public virtual void showRoadBlockMode(SKMaps.SKDistanceUnitType distanceUnit, long distanceToDestination)
        {
            currentNavigationMode = NavigationMode.ROADBLOCK;

            IList<string> items = getRoadBlocksOptionsList(distanceUnit, distanceToDestination);
            ArrayAdapter<string> listAdapter = new ArrayAdapter<string>(currentActivity, android.R.layout.simple_list_item_1, items);
            currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper3(this, listAdapter));
        }

        private class RunnableAnonymousInnerClassHelper3 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private ArrayAdapter<string> listAdapter;

            public RunnableAnonymousInnerClassHelper3(SKToolsNavigationUIManager outerInstance, ArrayAdapter<string> listAdapter)
            {
                this.outerInstance = outerInstance;
                this.listAdapter = listAdapter;
            }


            public override void run()
            {
                ListView listView = (ListView)outerInstance.roadBlockPanel.findViewById(R.id.roadblock_list);
                listView.Adapter = listAdapter;
                listView.OnItemClickListener = blockRoadsListItemClickListener;
                outerInstance.roadBlockPanel.Visibility = View.VISIBLE;
                outerInstance.backButtonPanel.Visibility = View.VISIBLE;
            }
        }

        /// <summary>
        /// Gets the list with road block distance options.
        /// </summary>
        /// <param name="distanceUnit"> </param>
        /// <param name="distanceToDestination">
        /// @return </param>
        private IList<string> getRoadBlocksOptionsList(SKMaps.SKDistanceUnitType distanceUnit, long distanceToDestination)
        {
            IList<string> sourceList = new List<string>();
            IList<string> roadBlocksList = new LinkedList<string>();
            string[] list;
            switch (distanceUnit)
            {
                case DISTANCE_UNIT_KILOMETER_METERS:
                    list = currentActivity.Resources.getStringArray(R.array.road_blocks_in_meters);
                    break;
                case DISTANCE_UNIT_MILES_FEET:
                    list = currentActivity.Resources.getStringArray(R.array.road_blocks_in_feet);
                    break;
                case DISTANCE_UNIT_MILES_YARDS:
                    list = currentActivity.Resources.getStringArray(R.array.road_blocks_in_yards);
                    break;
                default:
                    list = currentActivity.Resources.getStringArray(R.array.road_blocks_in_meters);
                    break;
            }
            Collections.addAll(roadBlocksList, list);
            long distance = distanceToDestination;
            // we initialize the sourceList with the elements in the roadBlocksList
            // that are smaller than the distance to destination
            if (distance < 500)
            {
                sourceList.AddRange(roadBlocksList.subList(0, 2));
            }
            else if (distance < 2000)
            {
                sourceList.AddRange(roadBlocksList.subList(0, 3));
            }
            else if (distance < 5000)
            {
                sourceList.AddRange(roadBlocksList.subList(0, 4));
            }
            else if (distance < 10000)
            {
                sourceList.AddRange(roadBlocksList.subList(0, 5));
            }
            else if (distance < 150000)
            {
                sourceList.AddRange(roadBlocksList.subList(0, 6));
            }
            else
            {
                sourceList.AddRange(roadBlocksList);
            }

            // if the road has no blocks, we remove the "Unblock all" option
            if (!SKToolsLogicManager.Instance.RoadBlocked)
            {
                sourceList.RemoveAt(0);
            }

            return sourceList;
        }

        /// <summary>
        /// Checks if is in pre navigation mode.
        /// 
        /// @return
        /// </summary>
        public virtual bool PreNavigationMode
        {
            get
            {
                return currentNavigationMode == NavigationMode.PRE_NAVIGATION;
            }
        }

        /// <summary>
        /// Checks if the navigation is in follower mode.
        /// 
        /// @return
        /// </summary>
        public virtual bool FollowerMode
        {
            get
            {
                return currentNavigationMode == NavigationMode.FOLLOWER;
            }
        }

        /// <summary>
        /// Sets the navigation in follower mode.
        /// </summary>
        public virtual void setFollowerMode()
        {
            currentNavigationMode = NavigationMode.FOLLOWER;
        }

        /// <summary>
        /// Shows the panel from the main navigation screen.
        /// </summary>
        /// <param name="isSimulationMode"> </param>
        public virtual void showFollowerModePanels(bool isSimulationMode)
        {

            hideViewIfVisible(positionMeButtonPanel);
            hideViewIfVisible(settingsPanel);
            hideViewIfVisible(roadBlockPanel);
            hideViewIfVisible(backButtonPanel);
            hideViewIfVisible(routeOverviewPanel);

            showViewIfNotVisible(speedPanel);

            if (!isFreeDrive)
            {
                showViewIfNotVisible(routeDistancePanel);
                showViewIfNotVisible(topCurrentNavigationPanel);
                showViewIfNotVisible(arrivingETATimeGroupPanels);
            }
            else
            {
                showViewIfNotVisible(freeDriveCurrentStreetPanel);
            }

            if (isSimulationMode)
            {
                showViewIfNotVisible(navigationSimulationPanel);
            }
        }

        /// <summary>
        /// Shows the panning mode screen.
        /// </summary>
        /// <param name="isNavigationTypeReal"> </param>
        public virtual void showPanningMode(bool isNavigationTypeReal)
        {
            currentNavigationMode = NavigationMode.PANNING;
            if (isNavigationTypeReal)
            {
                showViewIfNotVisible(positionMeButtonPanel);
            }
            showViewIfNotVisible(backButtonPanel);
            cancelSpeedExceededThread();
        }

        /// <summary>
        /// Shows the setting menu screen.
        /// </summary>
        public virtual void showSettingsMode()
        {
            currentNavigationMode = NavigationMode.SETTINGS;
            hideViewIfVisible(navigationSimulationPanel);
            initialiseVolumeSeekBar();
            hideViewIfVisible(topNextNavigationPanel);
            hideViewIfVisible(topCurrentNavigationPanel);
            hideViewIfVisible(routeOverviewPanel);
            hideViewIfVisible(reRoutingPanel);
            hideViewIfVisible(freeDriveCurrentStreetPanel);
            hideBottomAndLeftPanels();
            showViewIfNotVisible(settingsPanel);
        }

        /// <summary>
        /// Shows the overview screen.
        /// </summary>
        /// <param name="address"> </param>
        public virtual void showOverviewMode(string address)
        {
            currentNavigationMode = NavigationMode.ROUTE_OVERVIEW;

            hideViewIfVisible(topNextNavigationPanel);
            hideViewIfVisible(topCurrentNavigationPanel);
            hideBottomAndLeftPanels();

            routeOverviewPanel.findViewById(R.id.navigation_route_overview_starting_position_layout).Visibility = View.GONE;

            ((TextView)routeOverviewPanel.findViewById(R.id.navigation_route_overview_destination_text)).Text = address;
            routeOverviewPanel.Visibility = View.VISIBLE;
            backButtonPanel.Visibility = View.VISIBLE;
        }

        /// <summary>
        /// Shows the route info screen in simple navigation mode.
        /// </summary>
        /// <param name="startAddress"> </param>
        /// <param name="destinationAddress"> </param>
        public virtual void showRouteInfoScreen(string startAddress, string destinationAddress)
        {
            currentNavigationMode = NavigationMode.ROUTE_INFO;

            hideViewIfVisible(topNextNavigationPanel);
            hideViewIfVisible(topCurrentNavigationPanel);
            hideBottomAndLeftPanels();

            ((TextView)routeOverviewPanel.findViewById(R.id.navigation_route_overview_current_position_text)).Text = startAddress;
            ((TextView)routeOverviewPanel.findViewById(R.id.navigation_route_overview_destination_text)).Text = destinationAddress;

            routeOverviewPanel.findViewById(R.id.navigation_route_overview_starting_position_layout).Visibility = View.VISIBLE;
            routeOverviewPanel.Visibility = View.VISIBLE;
            backButtonPanel.Visibility = View.VISIBLE;
        }

        /// <summary>
        /// Shows the route info screen from free drive mode.
        /// </summary>
        public virtual void showRouteInfoFreeDriveScreen()
        {
            currentNavigationMode = NavigationMode.ROUTE_INFO;

            hideViewIfVisible(freeDriveCurrentStreetPanel);
            hideBottomAndLeftPanels();
            routeOverviewPanel.findViewById(R.id.navigation_route_overview_starting_position_layout).Visibility = View.GONE;

            routeOverviewPanel.findViewById(R.id.navigation_route_overview_destination_layout).Visibility = View.VISIBLE;

            ((TextView)routeOverviewPanel.findViewById(R.id.navigation_route_overview_destination_label)).Text = R.@string.current_position;
            ((TextView)routeOverviewPanel.findViewById(R.id.navigation_route_overview_destination_text)).Text = currentStreetNameFreeDriveString;

            routeOverviewPanel.Visibility = View.VISIBLE;
            backButtonPanel.Visibility = View.VISIBLE;
        }

        /// <summary>
        /// removes the top panels
        /// </summary>
        public virtual void hideTopPanels()
        {
            hideViewIfVisible(reRoutingPanel);
            hideViewIfVisible(topNextNavigationPanel);
            hideViewIfVisible(topCurrentNavigationPanel);
            hideViewIfVisible(freeDriveCurrentStreetPanel);
            hideViewIfVisible(navigationSimulationPanel);
            hideViewIfVisible(viaPointPanel);
        }

        /// <summary>
        /// removes the bottom and bottom left panels
        /// </summary>
        public virtual void hideBottomAndLeftPanels()
        {
            hideViewIfVisible(routeDistancePanel);
            hideViewIfVisible(speedPanel);
            hideViewIfVisible(arrivingETATimeGroupPanels);
        }

        /// <summary>
        /// Hide the settings menu screen.
        /// </summary>
        public virtual void hideSettingsPanel()
        {
            hideViewIfVisible(settingsPanel);
        }

        /// <summary>
        /// Shows the rerouting panel.
        /// </summary>
        public virtual void showReroutingPanel()
        {
            showViewIfNotVisible(reRoutingPanel);
        }

        /// <summary>
        /// Shows the exit navigation dialog.
        /// </summary>
        public virtual void showExitNavigationDialog()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(currentActivity);

            alertDialog.Title = R.@string.exit_navigation_dialog_title;
            alertDialog.Message = currentActivity.Resources.getString(R.@string.exit_navigation_dialog_message);
            alertDialog.setPositiveButton(currentActivity.Resources.getString(R.@string.ok_label), new OnClickListenerAnonymousInnerClassHelper(this));
            alertDialog.setNegativeButton(currentActivity.Resources.getString(R.@string.cancel_label), new OnClickListenerAnonymousInnerClassHelper2(this));
            alertDialog.show();
        }

        private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public OnClickListenerAnonymousInnerClassHelper(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public virtual void onClick(DialogInterface dialog, int which)
            {
                dialog.cancel();
                isFreeDrive = false;
                currentNavigationMode = NavigationMode.POST_NAVIGATION;
                SKToolsLogicManager.Instance.stopNavigation();
            }
        }

        private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public OnClickListenerAnonymousInnerClassHelper2(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public virtual void onClick(DialogInterface dialog, int which)
            {
                dialog.cancel();
            }
        }

        /// <summary>
        /// Shows a dialog that notifies that the route calculation failed.
        /// </summary>
        /// <param name="statusCode"> </param>
        public virtual void showRouteCalculationFailedDialog(SKRouteListener.SKRoutingErrorCode statusCode)
        {
            currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper(this, statusCode));
        }

        private class RunnableAnonymousInnerClassHelper : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private SKRouteListener.SKRoutingErrorCode statusCode;

            public RunnableAnonymousInnerClassHelper(SKToolsNavigationUIManager outerInstance, SKRouteListener.SKRoutingErrorCode statusCode)
            {
                this.outerInstance = outerInstance;
                this.statusCode = statusCode;
            }


            public override void run()
            {
                string dialogMessage;
                Resources res = currentActivity.Resources;
                switch (statusCode)
                {
                    case SAME_START_AND_DESTINATION:
                        dialogMessage = res.getString(R.@string.route_same_start_and_destination);
                        break;
                    case INVALID_START:
                        dialogMessage = res.getString(R.@string.route_invalid_start);
                        break;
                    case INVALID_DESTINATION:
                        dialogMessage = res.getString(R.@string.route_invalid_destination);
                        break;
                    case INTERNAL_ERROR:
                        dialogMessage = res.getString(R.@string.route_unknown_server_error);
                        break;
                    case ROUTE_CANNOT_BE_CALCULATED:
                    default:
                        dialogMessage = res.getString(R.@string.route_cannot_be_calculated);
                        break;
                }

                AlertDialog.Builder alertDialog = new AlertDialog.Builder(currentActivity);
                alertDialog.Title = R.@string.routing_server_error;
                alertDialog.Message = dialogMessage;
                alertDialog.setNeutralButton(res.getString(R.@string.ok_label), new OnClickListenerAnonymousInnerClassHelper3(this));
                alertDialog.show();
            }

            private class OnClickListenerAnonymousInnerClassHelper3 : DialogInterface.OnClickListener
            {
                private readonly RunnableAnonymousInnerClassHelper outerInstance;

                public OnClickListenerAnonymousInnerClassHelper3(RunnableAnonymousInnerClassHelper outerInstance)
                {
                    this.outerInstance = outerInstance;
                }


                public override void onClick(DialogInterface dialog, int which)
                {
                    dialog.dismiss();
                }
            }
        }

        /// <summary>
        /// Resets the values to a default value.
        /// </summary>
        /// <param name="distanceUnit"> </param>
        public virtual void reset(SKMaps.SKDistanceUnitType distanceUnit)
        {
            distanceUnitType = distanceUnit;
            nextVisualAdviceDistance = 0;
            routeDistanceString = "";
            nextVisualAdviceStreetName = "";
            currentVisualAdviceDistance = 0;
            currentVisualAdviceStreetName = "";
            estimatedTimePanelVisible = true;
            isNextAdviceVisible = false;
            firstAdviceReceived = false;
            timeToDestination = 0;
            currentSpeedLimit = 0;
            currentSpeed = 0;
            initialTimeToDestination = 0;
        }

        /// <summary>
        /// Handles the navigation state update.
        /// </summary>
        /// <param name="skNavigationState"> </param>
        /// <param name="mapStyle"> </param>
        public virtual void handleNavigationState(SKNavigationState skNavigationState, int mapStyle)
        {

            if (currentNavigationMode == NavigationMode.FOLLOWER)
            {

                currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper2(this, skNavigationState, mapStyle));
            }

        }

        private class RunnableAnonymousInnerClassHelper2 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private SKNavigationState skNavigationState;
            private int mapStyle;

            public RunnableAnonymousInnerClassHelper2(SKToolsNavigationUIManager outerInstance, SKNavigationState skNavigationState, int mapStyle)
            {
                this.outerInstance = outerInstance;
                this.skNavigationState = skNavigationState;
                this.mapStyle = mapStyle;
            }


            public override void run()
            {

                hideViewIfVisible(reRoutingPanel);
                if (currentNavigationMode == NavigationMode.FOLLOWER)
                {
                    showViewIfNotVisible(topCurrentNavigationPanel);
                    showViewIfNotVisible(routeDistancePanel);
                    showViewIfNotVisible(speedPanel);
                }

                currentCountryCode = skNavigationState.CountryCode;
                distanceEstimatedUntilDestination = (int)Math.Round(skNavigationState.DistanceToDestination);

                string currentVisualAdviceImage = skNavigationState.CurrentAdviceVisualAdviceFile;

                Bitmap decodedAdvice = SKToolsUtils.decodeFileToBitmap(currentVisualAdviceImage);
                if (decodedAdvice != null)
                {
                    currentAdviceImage.ImageBitmap = decodedAdvice;
                    currentAdviceImage.Visibility = View.VISIBLE;
                }

                string nextStreetName = skNavigationState.CurrentAdviceNextStreetName;
                string nextAdviceNextStreetName = skNavigationState.NextAdviceNextStreetName;
                if (nextAdviceNextStreetName != null && nextAdviceNextStreetName.Equals(""))
                {
                    nextAdviceNextStreetName = null;
                    skNavigationState.NextAdviceNextStreetName = null;
                }
                string exitNumber = skNavigationState.CurrentAdviceExitNumber;
                if (nextStreetName != null)
                {
                    nextStreetName = nextStreetName.Replace("\u021B", "\u0163").Replace("\u021A", "\u0162").Replace("\u0218", "\u015E").Replace("\u0219", "\u015F");
                }
                string countryCode = skNavigationState.CountryCode;
                string nextVisualAdviceFile = skNavigationState.NextAdviceVisualAdviceFile;
                if (nextVisualAdviceFile != null && nextVisualAdviceFile.Equals(""))
                {
                    nextVisualAdviceFile = null;
                    skNavigationState.NextAdviceVisualAdviceFile = null;
                }
                showDestinationReachedFlag = skNavigationState.LastAdvice;

                if (initialTimeToDestination == 0)
                {
                    initialTimeToDestination = skNavigationState.CurrentAdviceTimeToDestination;
                }

                nextStreetType = skNavigationState.CurrentAdviceNextOsmStreetType.Value;
                secondNextStreetType = skNavigationState.NextAdviceNextOsmStreetType.Value;

                int currentDistanceToAdvice = skNavigationState.CurrentAdviceDistanceToAdvice;
                int nextDistanceToAdvice = skNavigationState.NextAdviceDistanceToAdvice;

                // speed values
                if (currentSpeed == 0 || currentSpeed != skNavigationState.CurrentSpeed)
                {
                    currentSpeed = skNavigationState.CurrentSpeed;
                    currentSpeedString = Convert.ToString(SKToolsUtils.getSpeedByUnit(currentSpeed, distanceUnitType));
                    currentSpeedText.Text = currentSpeedString;
                    currentSpeedTextValue.Text = SKToolsUtils.getSpeedTextByUnit(currentActivity, distanceUnitType);
                }

                if (currentSpeedLimit != skNavigationState.CurrentSpeedLimit)
                {
                    currentSpeedLimit = skNavigationState.CurrentSpeedLimit;
                    outerInstance.handleSpeedLimitAvailable(countryCode, distanceUnitType, mapStyle);
                }

                if (navigationTotalDistance == 0)
                {
                    navigationTotalDistance = distanceEstimatedUntilDestination;
                }

                // set next advice content & visibility
                if (nextVisualAdviceFile != null)
                {
                    if (nextVisualAdviceDistance != nextDistanceToAdvice)
                    {
                        nextVisualAdviceDistance = nextDistanceToAdvice;
                        nextAdviceDistanceTextView.Text = SKNavigationManager.Instance.formatDistance(nextDistanceToAdvice);
                    }
                    if (nextVisualAdviceStreetName != null && !nextVisualAdviceStreetName.Equals(nextAdviceNextStreetName))
                    {
                        nextVisualAdviceStreetName = nextAdviceNextStreetName;
                        nextAdviceStreetNameTextView.Text = nextAdviceNextStreetName;
                    }

                    Bitmap adviceFile = SKToolsUtils.decodeFileToBitmap(nextVisualAdviceFile);
                    if (adviceFile != null)
                    {
                        nextAdviceImageView.ImageBitmap = adviceFile;
                        nextAdviceImageView.Visibility = View.VISIBLE;
                    }

                    setNextAdviceStreetNameVisibility();
                }

                if (currentNavigationMode == NavigationMode.FOLLOWER && firstAdviceReceived)
                {

                    if (!isNextAdviceVisible)
                    {
                        if (nextVisualAdviceFile != null)
                        {
                            isNextAdviceVisible = true;
                            showNextAdvice();
                        }
                    }
                    else
                    {
                        if (nextVisualAdviceFile == null)
                        {
                            isNextAdviceVisible = false;
                            topNextNavigationPanel.Visibility = View.GONE;
                        }
                    }
                }

                // set current advice content
                if (currentAdviceDistance != null && currentVisualAdviceDistance != currentDistanceToAdvice)
                {
                    currentVisualAdviceDistance = currentDistanceToAdvice;
                    currentAdviceDistance.Text = SKNavigationManager.Instance.formatDistance(currentDistanceToAdvice);
                }
                if (currentAdviceName != null && !showDestinationReachedFlag)
                {
                    if (exitNumber != null && exitNumber.Length > 0)
                    {
                        string currentAdvice = currentActivity.Resources.getString(R.@string.exit_highway_advice_label) + " " + exitNumber;
                        if (nextStreetName != null && nextStreetName.Length > 0)
                        {
                            currentAdvice = currentAdvice + " " + nextStreetName;
                        }
                        currentAdviceName.Text = currentAdvice;
                        currentVisualAdviceStreetName = currentAdvice;
                    }
                    else
                    {
                        if (currentVisualAdviceStreetName != null && !currentVisualAdviceStreetName.Equals(nextStreetName))
                        {
                            currentVisualAdviceStreetName = nextStreetName;
                            currentAdviceName.Text = nextStreetName;
                        }
                    }
                }

                if (showDestinationReachedFlag)
                {
                    if (currentAdviceImage != null)
                    {
                        currentAdviceImage.ImageResource = R.drawable.ic_destination_advise_black;
                    }
                    if (currentAdviceName != null)
                    {
                        currentVisualAdviceStreetName = currentActivity.Resources.getString(R.@string.destination_reached_info_text);
                        currentAdviceName.Text = currentVisualAdviceStreetName;
                    }
                    if (currentAdviceDistance != null)
                    {
                        currentVisualAdviceDistance = 0;
                        currentAdviceDistance.Visibility = View.GONE;
                    }
                    disableNextAdvice();
                }


                // set estimated/arriving time
                if ((timeToDestination < 120) || (timeToDestination - 60 >= skNavigationState.CurrentAdviceTimeToDestination) || (timeToDestination + 60 < skNavigationState.CurrentAdviceTimeToDestination))
                {

                    timeToDestination = skNavigationState.CurrentAdviceTimeToDestination;
                    if (estimatedTimePanelVisible)
                    {
                        outerInstance.showEstimatedTime();
                    }
                    else
                    {
                        outerInstance.showArrivingTime();
                    }
                }

                string[] distanceToDestinationSplit = SKNavigationManager.Instance.formatDistance(distanceEstimatedUntilDestination).Split(" ");
                if (!routeDistanceString.Equals(distanceToDestinationSplit[0]))
                {
                    routeDistanceString = distanceToDestinationSplit[0];
                    if (distanceToDestinationSplit.Length > 1)
                    {
                        routeDistanceValueString = distanceToDestinationSplit[1];
                    }
                    setRouteDistanceFields();
                }

                // when we receive the first advice we show the panels that were set accordingly
                if (!firstAdviceReceived)
                {
                    firstAdviceReceived = true;

                    if (currentNavigationMode == NavigationMode.FOLLOWER)
                    {
                        showViewIfNotVisible(topCurrentNavigationPanel);

                        if (nextVisualAdviceFile != null)
                        {
                            isNextAdviceVisible = true;
                            showNextAdvice();
                        }
                        else
                        {
                            isNextAdviceVisible = false;
                        }
                        if (currentAdviceDistance != null && !showDestinationReachedFlag)
                        {
                            currentAdviceDistance.Visibility = View.VISIBLE;
                        }
                        if (!firstTimeNavigation)
                        {
                            topCurrentNavigationPanel.bringToFront();
                        }

                        showViewIfNotVisible(routeDistancePanel);
                        showViewIfNotVisible(arrivingETATimeGroupPanels);
                    }
                }
            }
        }

        /// <summary>
        /// sets top panels background colour
        /// </summary>
        public virtual void setTopPanelsBackgroundColour(int mapStyle, bool currentAdviceChanged, bool nextAdviceChanged)
        {
            if (signPostsCountryExceptions.contains(currentCountryCode))
            {
                isDefaultTopPanelBackgroundColor = false;
                if (currentAdviceChanged || !nextAdviceChanged)
                {
                    verifyStreetType(mapStyle, nextStreetType, true);
                }
                if (nextStreetType == secondNextStreetType)
                {
                    if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                    {
                        verifyStreetType(mapStyle, 0, false);
                    }
                }
                else
                {
                    if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                    {
                        verifyStreetType(mapStyle, secondNextStreetType, false);
                    }
                }
            }
            else
            {
                if (!isDefaultTopPanelBackgroundColor)
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        if (currentAdviceChanged || !nextAdviceChanged)
                        {
                            setTopPanelsStyle(R.color.white, R.color.black, true);
                        }
                        if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            setTopPanelsStyle(R.color.white, R.color.black, false);
                        }

                    }
                    else
                    {
                        if (currentAdviceChanged || !nextAdviceChanged)
                        {
                            setTopPanelsStyle(R.color.navigation_style_night, R.color.gray, true);
                        }
                        if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            setTopPanelsStyle(R.color.navigation_style_night, R.color.gray, false);
                        }
                    }
                }
                isDefaultTopPanelBackgroundColor = true;
            }
        }


        /// <summary>
        /// verifies the street type and sets the colors for top panels
        /// </summary>
        /// <param name="mapStyle"> </param>
        /// <param name="streetType"> </param>
        /// <param name="forCurrent"> </param>
        private void verifyStreetType(int mapStyle, int streetType, bool forCurrent)
        {
            if (streetType == OSM_STREET_TYPE_MOTORWAY || streetType == OSM_STREET_TYPE_MOTORWAY_LINK)
            {
                if ((currentCountryCode.Equals("CH")) || (currentCountryCode.Equals("US")))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.green_panel_day_background, R.color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.green_panel_night_background, R.color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.blue_panel_day_background, R.color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.blue_panel_night_background, R.color.blue_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OSM_STREET_TYPE_PRIMARY || streetType == OSM_STREET_TYPE_PRIMARY_LINK)
            {
                if (currentCountryCode.Equals("CH"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.blue_panel_day_background, R.color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.blue_panel_night_background, R.color.blue_panel_night_text, forCurrent);
                    }
                }
                else if (currentCountryCode.Equals("US"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.green_panel_day_background, R.color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.green_panel_night_background, R.color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.yellow_panel_day_background, R.color.black, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.yellow_panel_night_background, R.color.yellow_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OSM_STREET_TYPE_TRUNK || streetType == OSM_STREET_TYPE_TRUNK_LINK)
            {
                if ((currentCountryCode.Equals("GB")) || (currentCountryCode.Equals("US")))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.green_panel_day_background, R.color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.green_panel_night_background, R.color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(R.color.white, R.color.black, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(R.color.navigation_style_night, R.color.gray, forCurrent);
                    }
                }
            }
            else
            {
                if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                {
                    setTopPanelsStyle(R.color.white, R.color.black, forCurrent);
                }
                else
                {
                    setTopPanelsStyle(R.color.navigation_style_night, R.color.gray, forCurrent);
                }
            }
        }

        /// <summary>
        /// sets the background for top panel and the text color for the texts inside
        /// it
        /// </summary>
        /// <param name="drawableId"> </param>
        /// <param name="textColor"> </param>
        /// <param name="forCurrentAdvice"> </param>
        protected internal virtual void setTopPanelsStyle(int drawableId, int textColor, bool forCurrentAdvice)
        {
            if (!isFreeDrive)
            {
                if (forCurrentAdvice)
                {
                    if (topCurrentNavigationDistanceStreetPanel != null)
                    {
                        topCurrentNavigationDistanceStreetPanel.BackgroundColor = currentActivity.Resources.getColor(drawableId);
                    }
                    if (topCurrentNavigationPanel != null)
                    {
                        RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)topCurrentNavigationPanel.findViewById(R.id.current_advice_image_holder);
                        if (topCurrentNavigationImagePanel != null)
                        {
                            topCurrentNavigationImagePanel.BackgroundColor = currentActivity.Resources.getColor(drawableId);
                        }
                    }
                    if (currentAdviceDistance != null)
                    {
                        currentAdviceDistance.TextColor = currentActivity.Resources.getColor(textColor);
                    }
                    if (currentAdviceName != null)
                    {
                        currentAdviceName.TextColor = currentActivity.Resources.getColor(textColor);
                    }
                    currentAdviceBackgroundDrawableId = drawableId;
                }
                else
                {
                    if (nextAdviceImageDistancePanel != null)
                    {
                        nextAdviceImageDistancePanel.BackgroundColor = currentActivity.Resources.getColor(drawableId);
                    }
                    if (nextAdviceStreetNamePanel != null)
                    {
                        nextAdviceStreetNamePanel.BackgroundColor = currentActivity.Resources.getColor(drawableId);
                    }
                    if (nextAdviceDistanceTextView != null)
                    {
                        nextAdviceDistanceTextView.TextColor = currentActivity.Resources.getColor(textColor);
                    }
                    if (nextAdviceStreetNameTextView != null)
                    {
                        nextAdviceStreetNameTextView.TextColor = currentActivity.Resources.getColor(textColor);
                    }
                    nextAdviceBackgroundDrawableId = drawableId;
                    setNextAdviceOverlayVisibility();
                }
            }
            else
            {
                TextView freeDriveCurrentStreetText = (TextView)freeDriveCurrentStreetPanel.findViewById(R.id.free_drive_current_street_text);
                if (freeDriveCurrentStreetText != null)
                {
                    freeDriveCurrentStreetText.BackgroundColor = currentActivity.Resources.getColor(drawableId);
                    freeDriveCurrentStreetText.TextColor = currentActivity.Resources.getColor(textColor);
                }
                currentAdviceBackgroundDrawableId = drawableId;
            }
        }


        /// <summary>
        /// sets the advice overlay semi transparent visibility
        /// </summary>
        public virtual void setNextAdviceOverlayVisibility()
        {
            if (currentAdviceBackgroundDrawableId == nextAdviceBackgroundDrawableId)
            {
                topNextNavigationPanel.findViewById(R.id.navigation_next_advice_image_distance_overlay_background).Visibility = View.VISIBLE;
                if (nextVisualAdviceStreetName != null)
                {
                    topNextNavigationPanel.findViewById(R.id.navigation_next_advice_street_overlay_background).Visibility = View.VISIBLE;
                }
                else
                {
                    topNextNavigationPanel.findViewById(R.id.navigation_next_advice_street_overlay_background).Visibility = View.GONE;
                }
            }
            else
            {
                topNextNavigationPanel.findViewById(R.id.navigation_next_advice_image_distance_overlay_background).Visibility = View.GONE;
                topNextNavigationPanel.findViewById(R.id.navigation_next_advice_street_overlay_background).Visibility = View.GONE;
            }
        }

        /// <summary>
        /// switch from estimated time to the arrived time
        /// </summary>
        public virtual void switchEstimatedTime()
        {
            currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper3(this));
        }

        private class RunnableAnonymousInnerClassHelper3 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public RunnableAnonymousInnerClassHelper3(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override void run()
            {

                if (estimatedTimePanelVisible)
                {
                    outerInstance.showArrivingTime();
                }
                else
                {
                    outerInstance.showEstimatedTime();
                }
                estimatedTimePanelVisible = !estimatedTimePanelVisible;
            }

        }

        /// <summary>
        /// shows estimated time
        /// </summary>
        public virtual void showEstimatedTime()
        {
            hideViewIfVisible(arrivingTimePanel);
            if (estimatedTimeText != null)
            {
                estimatedTimeText.Text = SKToolsUtils.formatTime(timeToDestination);
            }
            showViewIfNotVisible(estimatedTimePanel);
        }

        /// <summary>
        /// calculates and shows the arriving time
        /// </summary>
        public virtual void showArrivingTime()
        {
            DateTime currentTime = new DateTime();
            DateTime arrivingTime = (DateTime)currentTime.clone();
            int hours = timeToDestination / 3600;
            int minutes = (timeToDestination % 3600) / 60;
            arrivingTime.AddMinutes(minutes);
            TextView arrivingTimeAMPM = (TextView)arrivingTimePanel.findViewById(R.id.arriving_time_text_ampm);
            if (arrivingTimeText != null)
            {
                SimpleDateFormat simpleDateFormat = new SimpleDateFormat("HH:mm");
                arrivingTime.add(DateTime.HOUR_OF_DAY, hours);
                arrivingTimeAMPM.Text = "";
                arrivingTimeText.Text = simpleDateFormat.format(arrivingTime);
            }
            hideViewIfVisible(estimatedTimePanel);
            showViewIfNotVisible(arrivingTimePanel);
        }

        /// <summary>
        /// Checks if the navigation is in free drive mode.
        /// 
        /// @return
        /// </summary>
        public virtual bool FreeDriveMode
        {
            get
            {
                return isFreeDrive;
            }
        }

        /// <summary>
        /// Sets the free drive mode.
        /// </summary>
        public virtual void setFreeDriveMode()
        {
            firstAdviceReceived = false;
            isFreeDrive = true;

            hideViewIfVisible(routeDistancePanel);
            hideViewIfVisible(arrivingETATimeGroupPanels);

            bool isLandscape = currentActivity.Resources.Configuration.orientation == Configuration.ORIENTATION_LANDSCAPE;
            SettingsMenuForFreeDrive = isLandscape;
            cancelSpeedExceededThread();
        }

        /// <summary>
        /// sets speed limit field and visibility
        /// </summary>
        /// <param name="countryCode"> </param>
        /// <param name="distanceUnitType"> </param>
        public virtual void handleSpeedLimitAvailable(string countryCode, SKMaps.SKDistanceUnitType distanceUnitType, int mapStyle)
        {

            if (speedPanel == null)
            {
                return;
            }

            TextView speedLimitText = (TextView)speedPanel.findViewById(R.id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)speedPanel.findViewById(R.id.navigation_speed_sign_image);

            if (countryCode != null)
            {
                isUS = countryCode.Equals("US");
                if (isUS)
                {
                    isDefaultSpeedSign = false;
                    if (speedExceededThread == null)
                    {
                        if (speedLimitImage != null)
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign_us;
                        }
                    }
                }
                else
                {
                    if (!isDefaultSpeedSign)
                    {
                        if (speedLimitImage != null)
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign;
                        }
                    }
                    isDefaultSpeedSign = true;
                }
            }

            // speed limit visibility
            if (currentNavigationMode == NavigationMode.FOLLOWER)
            {
                if (currentSpeedLimit != 0) //&& gpsIsWorking) {
                {
                    if (!speedLimitAvailable)
                    {
                        currentActivity.findViewById(R.id.navigation_free_drive_speed_limit_panel).Visibility = View.VISIBLE;
                    }
                }
            }

            // set speed limit
            if (currentSpeedLimit != 0)
            {
                if (speedLimitText != null)
                {
                    speedLimitText.Text = Convert.ToString(SKToolsUtils.getSpeedByUnit(currentSpeedLimit, distanceUnitType));
                    if (!speedLimitExceeded)
                    {
                        speedLimitText.Visibility = View.VISIBLE;
                        speedLimitImage.Visibility = View.VISIBLE;
                    }
                }
                if (!speedLimitAvailable)
                {
                    CurrentSpeedPanelBackgroundAndTextColour = mapStyle;
                }
                speedLimitAvailable = true;
            }
            else
            {
                if (speedLimitAvailable)
                {
                    currentActivity.findViewById(R.id.navigation_free_drive_speed_limit_panel).Visibility = View.GONE;

                    CurrentSpeedPanelBackgroundAndTextColour = mapStyle;

                }
                speedLimitAvailable = false;
            }

        }

        /// <summary>
        /// cancels the thread that deals with the speed exceeded flow
        /// </summary>
        private void cancelSpeedExceededThread()
        {
            if (speedExceededThread != null && speedExceededThread.Alive)
            {
                speedExceededThread.cancel();
            }
        }

        /// <summary>
        /// sets current speed panel background color when speed limit is available.
        /// </summary>
        /// <param name="currentMapStyle"> </param>
        public virtual int CurrentSpeedPanelBackgroundAndTextColour
        {
            set
            {
                RelativeLayout currentSpeedPanel = (RelativeLayout)currentActivity.findViewById(R.id.navigation_free_drive_speed_limit_panel);
                if (value == SKToolsMapOperationsManager.DAY_STYLE)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.BackgroundColor = currentActivity.Resources.getColor(R.color.gray);
                    }
                    currentActivity.findViewById(R.id.free_drive_current_speed_linear_layout).BackgroundColor = currentActivity.Resources.getColor(R.color.gray);
                    if (currentSpeedText != null)
                    {
                        currentSpeedText.TextColor = currentActivity.Resources.getColor(R.color.black);
                    }
                    if (currentSpeedTextValue != null)
                    {
                        currentSpeedTextValue.TextColor = currentActivity.Resources.getColor(R.color.black);
                    }
                }
                else if (value == SKToolsMapOperationsManager.NIGHT_STYLE)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.BackgroundColor = currentActivity.Resources.getColor(R.color.speed_panel_night_background);
                    }
                    currentActivity.findViewById(R.id.free_drive_current_speed_linear_layout).BackgroundColor = currentActivity.Resources.getColor(R.color.speed_panel_night_background);
                    if (currentSpeedText != null)
                    {
                        currentSpeedText.TextColor = currentActivity.Resources.getColor(R.color.gray);
                    }
                    if (currentSpeedTextValue != null)
                    {
                        currentSpeedTextValue.TextColor = currentActivity.Resources.getColor(R.color.gray);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the free drive updates.
        /// </summary>
        /// <param name="countryCode"> </param>
        /// <param name="streetName"> </param>
        /// <param name="currentFreeDriveSpeed"> </param>
        /// <param name="speedLimit"> </param>
        /// <param name="distanceUnitType"> </param>
        /// <param name="mapStyle"> </param>
        public virtual void handleFreeDriveUpdated(string countryCode, string streetName, double currentFreeDriveSpeed, double speedLimit, SKMaps.SKDistanceUnitType distanceUnitType, int mapStyle)
        {
            currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper4(this, countryCode, streetName, currentFreeDriveSpeed, speedLimit, distanceUnitType, mapStyle));
        }

        private class RunnableAnonymousInnerClassHelper4 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private string countryCode;
            private string streetName;
            private double currentFreeDriveSpeed;
            private double speedLimit;
            private SKMaps.SKDistanceUnitType distanceUnitType;
            private int mapStyle;

            public RunnableAnonymousInnerClassHelper4(SKToolsNavigationUIManager outerInstance, string countryCode, string streetName, double currentFreeDriveSpeed, double speedLimit, SKMaps.SKDistanceUnitType distanceUnitType, int mapStyle)
            {
                this.outerInstance = outerInstance;
                this.countryCode = countryCode;
                this.streetName = streetName;
                this.currentFreeDriveSpeed = currentFreeDriveSpeed;
                this.speedLimit = speedLimit;
                this.distanceUnitType = distanceUnitType;
                this.mapStyle = mapStyle;
            }

            public override void run()
            {

                if (isFreeDrive)
                {

                    if (currentSpeed == 0 || currentSpeed != currentFreeDriveSpeed)
                    {
                        currentSpeed = currentFreeDriveSpeed;
                        currentSpeedString = Convert.ToString(SKToolsUtils.getSpeedByUnit(currentSpeed, distanceUnitType));
                        currentSpeedText.Text = currentSpeedString;
                        currentSpeedTextValue.Text = SKToolsUtils.getSpeedTextByUnit(currentActivity, distanceUnitType);
                    }

                    if (currentSpeedLimit != speedLimit)
                    {
                        currentSpeedLimit = speedLimit;
                        outerInstance.handleSpeedLimitAvailable(countryCode, distanceUnitType, mapStyle);
                    }

                    outerInstance.setTopPanelsBackgroundColour(mapStyle, false, false);

                    if (streetName != null && !streetName.Equals(""))
                    {
                        currentStreetNameFreeDriveString = streetName;
                        ((TextView)freeDriveCurrentStreetPanel.findViewById(R.id.free_drive_current_street_text)).Text = streetName;
                    }
                    else
                    {
                        currentStreetNameFreeDriveString = null;
                    }
                    if (currentNavigationMode == NavigationMode.FOLLOWER && currentStreetNameFreeDriveString != null)
                    {
                        freeDriveCurrentStreetPanel.Visibility = View.VISIBLE;
                        showViewIfNotVisible(speedPanel);
                    }
                }

            }
        }

        /// <summary>
        /// Handles orientation change.
        /// </summary>
        /// <param name="mapStyle"> </param>
        /// <param name="displayMode"> </param>
        public virtual void handleOrientationChanged(int mapStyle, SKMapSettings.SKMapDisplayMode displayMode)
        {
            if (currentNavigationMode != NavigationMode.PRE_NAVIGATION || currentNavigationMode != NavigationMode.POST_NAVIGATION)
            {
                currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper(this, mapStyle, displayMode));
            }
        }

        private class RunnableAnonymousInnerClassHelper : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            private int mapStyle;
            private SKMapSettings.SKMapDisplayMode displayMode;

            public RunnableAnonymousInnerClassHelper(SKToolsNavigationUIManager outerInstance, int mapStyle, SKMapSettings.SKMapDisplayMode displayMode)
            {
                this.outerInstance = outerInstance;
                this.mapStyle = mapStyle;
                this.displayMode = displayMode;
            }


            public override void run()
            {

                if (settingsPanel != null)
                {
                    rootLayout.removeView(settingsPanel);
                }
                rootLayout.removeView(speedPanel);
                rootLayout.removeView(routeDistancePanel);
                rootLayout.removeView(arrivingETATimeGroupPanels);

                inflateSettingsMenu();
                outerInstance.initialiseVolumeSeekBar();
                inflateBottomPanels();

                outerInstance.setAudioViewsFromSettings();
                outerInstance.switchMapMode(displayMode);
                outerInstance.switchDayNightStyle(mapStyle);

                bool isLandscape = currentActivity.Resources.Configuration.orientation == Configuration.ORIENTATION_LANDSCAPE;
                if (isFreeDrive)
                {
                    SettingsMenuForFreeDrive = isLandscape;
                }

                if (currentNavigationMode == NavigationMode.SETTINGS)
                {
                    showViewIfNotVisible(settingsPanel);
                }
                else if (currentNavigationMode == NavigationMode.FOLLOWER)
                {
                    showViewIfNotVisible(speedPanel);
                    showViewIfNotVisible(routeDistancePanel);
                    showViewIfNotVisible(arrivingETATimeGroupPanels);
                }

                outerInstance.setAdvicesFields();
                outerInstance.changePanelsBackgroundAndTextViewsColour(mapStyle);

            }
        }

        /// <summary>
        /// Handles the speed exceeded.
        /// </summary>
        /// <param name="speedExceeded"> </param>
        public virtual void handleSpeedExceeded(bool speedExceeded)
        {
            speedLimitExceeded = speedExceeded;
            if (currentNavigationMode == NavigationMode.FOLLOWER)
            {
                changeSpeedSigns();
            }
        }


        /// <summary>
        /// sets the current and next advice panels dimensions
        /// </summary>
        private void setAdvicesFields()
        {
            if (!isFreeDrive)
            {
                setRouteDistanceFields();
                setETAFields();
            }
            else
            {
                TextView freeDriveCurrentStreetText = (TextView)freeDriveCurrentStreetPanel.findViewById(R.id.free_drive_current_street_text);
                if (freeDriveCurrentStreetText != null)
                {
                    freeDriveCurrentStreetText.Text = currentStreetNameFreeDriveString;
                }
            }
            setCurrentSpeedFields();
            setSpeedLimitFields();
        }

        /// <summary>
        /// set speed limit fields
        /// </summary>
        private void setSpeedLimitFields()
        {
            TextView speedLimitText = (TextView)speedPanel.findViewById(R.id.speed_limit_value);
            if (speedLimitAvailable && speedLimitText != null && currentSpeedLimit != 0)
            {
                speedLimitText.Text = Convert.ToString(Convert.ToString(SKToolsUtils.getSpeedByUnit(currentSpeedLimit, distanceUnitType)));
                changeSpeedSigns();

                currentActivity.findViewById(R.id.navigation_free_drive_speed_limit_panel).Visibility = View.VISIBLE;
            }
        }

        /// <summary>
        /// changes the speed exceeded sign
        /// </summary>
        private void changeSpeedSigns()
        {
            TextView speedLimitText = (TextView)speedPanel.findViewById(R.id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)speedPanel.findViewById(R.id.navigation_speed_sign_image);

            if (speedLimitExceeded)
            {
                if (speedExceededThread == null || !speedExceededThread.Alive)
                {
                    speedExceededThread = new SpeedExceededThread(this, true);
                    speedExceededThread.start();
                }
            }
            else
            {
                if (speedExceededThread != null)
                {
                    if (speedLimitImage != null)
                    {
                        if (isUS)
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign_us;
                        }
                        else
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign;
                        }
                    }
                    if (speedLimitText != null)
                    {
                        speedLimitText.Visibility = View.VISIBLE;
                    }
                    speedExceededThread.cancel();
                    speedExceededThread = null;
                }
            }
        }

        /// <summary>
        /// set current speed fields
        /// </summary>
        private void setCurrentSpeedFields()
        {
            if (currentSpeedText != null)
            {
                currentSpeedText.Text = currentSpeedString;
            }
            if (currentSpeedTextValue != null)
            {
                currentSpeedTextValue.Text = SKToolsUtils.getSpeedTextByUnit(currentActivity, distanceUnitType);
            }
        }

        /// <summary>
        /// set eta fields
        /// </summary>
        private void setETAFields()
        {
            if (estimatedTimePanelVisible)
            {
                showEstimatedTime();
            }
            else
            {
                showArrivingTime();
            }
        }


        /// <summary>
        /// sets route distance fields.
        /// </summary>
        protected internal virtual void setRouteDistanceFields()
        {
            if (routeDistanceText != null)
            {
                routeDistanceText.Text = routeDistanceString;
            }
            if (routeDistanceTextValue != null)
            {
                routeDistanceTextValue.Text = routeDistanceValueString;
            }
        }

        /// <summary>
        /// sets next advice street name visibility
        /// </summary>
        private void setNextAdviceStreetNameVisibility()
        {
            if (topNextNavigationPanel != null && nextAdviceStreetNameTextView != null && nextAdviceStreetNamePanel != null)
            {
                if (nextVisualAdviceStreetName != null)
                {
                    nextAdviceStreetNamePanel.Visibility = View.VISIBLE;
                }
                else
                {
                    nextAdviceStreetNamePanel.Visibility = View.GONE;
                }
            }
        }

        /// <summary>
        /// Shows the next advice.
        /// </summary>
        private void showNextAdvice()
        {
            topNextNavigationPanel.Visibility = View.VISIBLE;
            topNextNavigationPanel.bringToFront();
        }

        /// <summary>
        /// removes the next advice
        /// </summary>
        private void disableNextAdvice()
        {
            isNextAdviceVisible = false;
            if (topNextNavigationPanel != null)
            {
                topNextNavigationPanel.Visibility = View.GONE;
            }
        }

        /// <summary>
        /// Shows the via point panel for 2 seconds.
        /// </summary>
        public virtual void showViaPointPanel()
        {

            Runnable runnable;
            Handler handler = new Handler();

            runnable = new RunnableAnonymousInnerClassHelper2(this);

            hideViewIfVisible(topCurrentNavigationPanel);
            hideViewIfVisible(topNextNavigationPanel);
            showViewIfNotVisible(viaPointPanel);

            TextView viaPointText = (TextView)viaPointPanel.findViewById(R.id.via_point_text_view);
            viaPointText.Text = currentActivity.Resources.getString(R.@string.via_point_reached);
            handler.postDelayed(runnable, 2000);

        }

        private class RunnableAnonymousInnerClassHelper2 : Runnable
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public RunnableAnonymousInnerClassHelper2(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override void run()
            {
                hideViewIfVisible(viaPointPanel);
            }
        }

        /// <summary>
        /// Initialises the volume bar.
        /// </summary>
        public virtual void initialiseVolumeSeekBar()
        {
            int currentVolume = SKToolsAdvicePlayer.getCurrentDeviceVolume(currentActivity);
            int maxVolume = SKToolsAdvicePlayer.getMaximAudioLevel(currentActivity);
            SeekBar volumeBar = (SeekBar)settingsPanel.findViewById(R.id.navigation_settings_volume);
            volumeBar.Max = maxVolume;
            volumeBar.Progress = currentVolume;
            volumeBar.OnSeekBarChangeListener = new OnSeekBarChangeListenerAnonymousInnerClassHelper(this);

        }

        private class OnSeekBarChangeListenerAnonymousInnerClassHelper : SeekBar.OnSeekBarChangeListener
        {
            private readonly SKToolsNavigationUIManager outerInstance;

            public OnSeekBarChangeListenerAnonymousInnerClassHelper(SKToolsNavigationUIManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public override void onStopTrackingTouch(SeekBar seekBar)
            {
            }

            public override void onStartTrackingTouch(SeekBar seekBar)
            {
            }

            public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
            {
                AudioManager audioManager = (AudioManager)currentActivity.getSystemService(Context.AUDIO_SERVICE);
                audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, progress, AudioManager.FLAG_SHOW_UI);
            }
        }

        /// <summary>
        /// Changes audio settings menu item panels.
        /// </summary>
        public virtual void loadAudioSettings()
        {
            TextView audioText = ((TextView)settingsPanel.findViewById(R.id.navigation_settings_audio_text));
            int? audioImageTag = (int?)audioText.Tag;
            audioImageTag = audioImageTag == null ? 0 : audioImageTag;

            Resources res = currentActivity.Resources;
            if (audioImageTag == R.drawable.ic_audio_on)
            {
                SKToolsAdvicePlayer.Instance.disableMute();
                SKToolsLogicManager.Instance.playLastAdvice();
                audioText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_audio_off, 0, 0);
                audioText.Tag = R.drawable.ic_audio_off;
                audioText.Text = res.getString(R.@string.navigate_settings_audio_off);
            }
            else if (audioImageTag == 0 || audioImageTag == R.drawable.ic_audio_off)
            {
                SKToolsAdvicePlayer.Instance.stop();
                SKToolsAdvicePlayer.Instance.enableMute();
                audioText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_audio_on, 0, 0);
                audioText.Tag = R.drawable.ic_audio_on;
                audioText.Text = res.getString(R.@string.navigate_settings_audio_on);
            }
        }

        /// <summary>
        /// sets the image view and the text view for audio button in settings
        /// screen, depending on the progress value of the volume bar (set
        /// previously) and the current device volume.
        /// </summary>
        protected internal virtual void setAudioViewsFromSettings()
        {
            if (settingsPanel != null)
            {
                if (SKToolsAdvicePlayer.Instance.Muted)
                {
                    TextView audioText = ((TextView)settingsPanel.findViewById(R.id.navigation_settings_audio_text));
                    audioText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_audio_on, 0, 0);
                    audioText.Tag = R.drawable.ic_audio_on;
                    audioText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_audio_on);
                }
                else
                {
                    TextView audioText = ((TextView)settingsPanel.findViewById(R.id.navigation_settings_audio_text));
                    audioText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_audio_off, 0, 0);
                    audioText.Tag = R.drawable.ic_audio_off;
                    audioText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_audio_off);
                }
            }
        }

        /// <summary>
        /// Changes settings menu item, map style panels.
        /// </summary>
        /// <param name="mapStyle"> </param>
        public virtual void switchDayNightStyle(int mapStyle)
        {
            TextView dayNightText = ((TextView)settingsPanel.findViewById(R.id.navigation_settings_day_night_mode_text));
            if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
            {
                dayNightText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_nightmode, 0, 0);
                dayNightText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_nightmode);
            }
            else
            {
                dayNightText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_daymode, 0, 0);
                dayNightText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_daymode);
            }

            isDefaultTopPanelBackgroundColor = false;
            changePanelsBackgroundAndTextViewsColour(mapStyle);
            setTopPanelsBackgroundColour(mapStyle, false, false);
        }

        /// <summary>
        /// Changes settings menu item, map mode panels.
        /// </summary>
        /// <param name="displayMode"> </param>
        public virtual void switchMapMode(SKMapSettings.SKMapDisplayMode displayMode)
        {
            TextView dayNightText = ((TextView)settingsPanel.findViewById(R.id.navigation_settings_view_mode_text));
            if (displayMode == SKMapSettings.SKMapDisplayMode.MODE_3D)
            {
                dayNightText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_2d, 0, 0);
                dayNightText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_2d_view);
            }
            else
            {
                dayNightText.setCompoundDrawablesWithIntrinsicBounds(0, R.drawable.ic_3d, 0, 0);
                dayNightText.Text = currentActivity.Resources.getString(R.@string.navigate_settings_3d_view);
            }
        }

        /// <summary>
        /// sets the background drawable for all views
        /// </summary>
        /// <param name="currentMapStyle"> </param>
        public virtual void changePanelsBackgroundAndTextViewsColour(int currentMapStyle)
        {

            if (currentNavigationMode == NavigationMode.PRE_NAVIGATION)
            {
                setPanelBackgroundAndTextColour(preNavigationPanel.findViewById(R.id.alternative_routes_layout), null, currentMapStyle);
                setPanelBackgroundAndTextColour(preNavigationPanel.findViewById(R.id.start_navigation_button), null, currentMapStyle);
            }
            else
            {

                if (!isFreeDrive)
                {
                    EtaTimeGroupPanelsBackgroundAndTextViewColour = currentMapStyle;

                    setPanelBackgroundAndTextColour(routeDistancePanel.findViewById(R.id.route_distance_linear_layout), routeDistanceText, currentMapStyle);
                    setPanelBackgroundAndTextColour(null, routeDistanceTextValue, currentMapStyle);
                }
                else
                {
                    setPanelBackgroundAndTextColour(freeDriveCurrentStreetPanel, null, currentMapStyle);
                }

                setPanelBackgroundAndTextColour(speedPanel, null, currentMapStyle);
                setPanelBackgroundAndTextColour(topCurrentNavigationPanel, null, currentMapStyle);
                setPanelBackgroundAndTextColour(nextAdviceStreetNamePanel, null, currentMapStyle);
                setPanelBackgroundAndTextColour(nextAdviceImageDistancePanel, null, currentMapStyle);
                setPanelBackgroundAndTextColour(backButtonPanel.findViewById(R.id.navigation_top_back_button), null, currentMapStyle);
                setPanelBackgroundAndTextColour(roadBlockPanel.findViewById(R.id.road_block_relative_layout), null, currentMapStyle);

                ViewGroup routeOverviewPanel = (ViewGroup)currentActivity.findViewById(R.id.navigation_route_overview_linear_layout);
                setPanelBackgroundAndTextColour(routeOverviewPanel, null, currentMapStyle);
                setPanelBackgroundAndTextColour(viaPointPanel, null, currentMapStyle);

                CurrentSpeedPanelBackgroundAndTextColour = currentMapStyle;
                SettingsPanelBackground = currentMapStyle;
            }
        }

        /// <summary>
        /// Sets the settings panel background depending on a map style.
        /// </summary>
        /// <param name="currentMapStyle"> </param>
        private int SettingsPanelBackground
        {
            set
            {
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_audio_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_day_night_mode_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_overview_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_route_info_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_roadblock_info_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_panning_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_view_mode_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_quit_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_back_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.findViewById(R.id.navigation_settings_seek_bar_layout), null, value);
            }
        }


        /// <summary>
        /// sets the background and text view colours for eta group panels
        /// </summary>
        /// <param name="currentMapStyle"> </param>
        public virtual int EtaTimeGroupPanelsBackgroundAndTextViewColour
        {
            set
            {
                TextView arrivingTimeAmPm = ((TextView)arrivingTimePanel.findViewById(R.id.arriving_time_text_ampm));
                TextView estimatedTimeTextValue = ((TextView)estimatedTimePanel.findViewById(R.id.estimated_navigation_time_text_value));

                setPanelBackgroundAndTextColour(arrivingTimePanel, arrivingTimeText, value);
                setPanelBackgroundAndTextColour(null, arrivingTimeAmPm, value);
                arrivingTimeAmPm.setCompoundDrawablesWithIntrinsicBounds(R.drawable.bullet_gray, 0, R.drawable.bullet_green, 0);

                setPanelBackgroundAndTextColour(estimatedTimePanel, estimatedTimeText, value);
                setPanelBackgroundAndTextColour(null, estimatedTimeTextValue, value);
                estimatedTimeTextValue.setCompoundDrawablesWithIntrinsicBounds(R.drawable.bullet_green, 0, R.drawable.bullet_gray, 0);
            }
        }

        /// <summary>
        /// sets the background for top panel and the text color for the texts inside it
        /// </summary>
        private void setPanelBackgroundAndTextColour(View panel, TextView textView, int currentMapStyle)
        { // View view,

            if (currentMapStyle == SKToolsMapOperationsManager.DAY_STYLE)
            {
                if (panel != null)
                {
                    panel.BackgroundColor = currentActivity.Resources.getColor(R.color.navigation_style_day);
                }
                if (textView != null)
                {
                    textView.TextColor = currentActivity.Resources.getColor(R.color.white);
                }
            }
            else if (currentMapStyle == SKToolsMapOperationsManager.NIGHT_STYLE)
            {
                if (panel != null)
                {
                    panel.BackgroundColor = currentActivity.Resources.getColor(R.color.navigation_style_night);
                }
                if (textView != null)
                {
                    textView.TextColor = currentActivity.Resources.getColor(R.color.gray);
                }
            }

        }


        /// <summary>
        /// removes views for pre navigation
        /// </summary>
        protected internal virtual void removePreNavigationViews()
        {
            if (preNavigationPanel != null)
            {
                rootLayout.removeView(preNavigationPanel);
                preNavigationPanel = null;
            }
        }

        /// <summary>
        /// remove views with different UI for portrait and landscape
        /// </summary>
        protected internal virtual void removeNavigationViews()
        {
            cancelSpeedExceededThread();

            if (topCurrentNavigationPanel != null)
            {
                rootLayout.removeView(topCurrentNavigationPanel);
                topCurrentNavigationPanel = null;
            }
            if (topNextNavigationPanel != null)
            {
                rootLayout.removeView(topNextNavigationPanel);
                topNextNavigationPanel = null;
            }
            if (menuOptions != null)
            {
                rootLayout.removeView(menuOptions);
                menuOptions = null;
            }
            if (routeDistancePanel != null)
            {
                rootLayout.removeView(routeDistancePanel);
                routeDistancePanel = null;
            }
            if (speedPanel != null)
            {
                rootLayout.removeView(speedPanel);
                speedPanel = null;
            }
            if (arrivingETATimeGroupPanels != null)
            {
                rootLayout.removeView(arrivingETATimeGroupPanels);
                arrivingETATimeGroupPanels = null;
            }
            if (reRoutingPanel != null)
            {
                rootLayout.removeView(reRoutingPanel);
                reRoutingPanel = null;
            }
            if (freeDriveCurrentStreetPanel != null)
            {
                rootLayout.removeView(freeDriveCurrentStreetPanel);
                freeDriveCurrentStreetPanel = null;
            }
            if (settingsPanel != null)
            {
                rootLayout.removeView(settingsPanel);
                settingsPanel = null;
            }
            if (navigationSimulationPanel != null)
            {
                rootLayout.removeView(navigationSimulationPanel);
                navigationSimulationPanel = null;
            }
            if (viaPointPanel != null)
            {
                rootLayout.removeView(viaPointPanel);
                viaPointPanel = null;
            }
            if (routeOverviewPanel != null)
            {
                rootLayout.removeView(routeOverviewPanel);
                routeOverviewPanel = null;
            }
            if (roadBlockPanel != null)
            {
                rootLayout.removeView(roadBlockPanel);
                roadBlockPanel = null;
            }
            if (backButtonPanel != null)
            {
                rootLayout.removeView(backButtonPanel);
                backButtonPanel = null;
            }
        }


        /// <summary>
        /// speed exceeded thread
        /// </summary>
        private class SpeedExceededThread : System.Threading.Thread
        {
            private readonly SKToolsNavigationUIManager outerInstance;


            internal bool speedExceeded;

            public SpeedExceededThread(SKToolsNavigationUIManager outerInstance, bool speedExceeded)
            {
                this.outerInstance = outerInstance;
                this.speedExceeded = speedExceeded;
            }

            public virtual void run()
            {
                ImageView speedLimitImage = (ImageView)speedPanel.findViewById(R.id.navigation_speed_sign_image);
                ImageView speedAlertImage = (ImageView)speedPanel.findViewById(R.id.navigation_alert_sign_image);
                TextView speedLimitText = (TextView)speedPanel.findViewById(R.id.speed_limit_value);

                while (speedExceeded)
                {
                    currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper3(this, speedLimitImage, speedAlertImage, speedLimitText));
                    lock (this)
                    {
                        try
                        {
                            Monitor.Wait(this, TimeSpan.FromMilliseconds(1000));
                        }
                        catch (InterruptedException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                    }
                    currentActivity.runOnUiThread(new RunnableAnonymousInnerClassHelper4(this, speedLimitImage, speedAlertImage, speedLimitText));
                    lock (this)
                    {
                        try
                        {
                            Monitor.Wait(this, TimeSpan.FromMilliseconds(2000));
                        }
                        catch (InterruptedException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                    }
                }
            }

            private class RunnableAnonymousInnerClassHelper3 : Runnable
            {
                private readonly SpeedExceededThread outerInstance;

                private ImageView speedLimitImage;
                private ImageView speedAlertImage;
                private TextView speedLimitText;

                public RunnableAnonymousInnerClassHelper3(SpeedExceededThread outerInstance, ImageView speedLimitImage, ImageView speedAlertImage, TextView speedLimitText)
                {
                    this.outerInstance = outerInstance;
                    this.speedLimitImage = speedLimitImage;
                    this.speedAlertImage = speedAlertImage;
                    this.speedLimitText = speedLimitText;
                }


                public override void run()
                {
                    if (speedLimitText != null)
                    {
                        speedLimitText.Visibility = View.GONE;
                    }
                    if (speedAlertImage != null)
                    {
                        speedAlertImage.Visibility = View.VISIBLE;
                    }
                    if (speedLimitImage != null)
                    {
                        if (isUS)
                        {
                            speedLimitImage.ImageResource = R.drawable.background_alert_sign_us;
                        }
                        else
                        {
                            speedLimitImage.ImageResource = R.drawable.background_alert_sign;
                        }
                        speedLimitImage.BackgroundDrawable = null;

                        Animation fadeOut = new AlphaAnimation(1, 0);
                        fadeOut.Interpolator = new AccelerateInterpolator();
                        fadeOut.Duration = 800;
                        speedLimitImage.Animation = fadeOut;
                        speedLimitImage.clearAnimation();
                    }

                }
            }

            private class RunnableAnonymousInnerClassHelper4 : Runnable
            {
                private readonly SpeedExceededThread outerInstance;

                private ImageView speedLimitImage;
                private ImageView speedAlertImage;
                private TextView speedLimitText;

                public RunnableAnonymousInnerClassHelper4(SpeedExceededThread outerInstance, ImageView speedLimitImage, ImageView speedAlertImage, TextView speedLimitText)
                {
                    this.outerInstance = outerInstance;
                    this.speedLimitImage = speedLimitImage;
                    this.speedAlertImage = speedAlertImage;
                    this.speedLimitText = speedLimitText;
                }


                public override void run()
                {
                    if (speedLimitImage != null)
                    {
                        if (isUS)
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign_us;
                        }
                        else
                        {
                            speedLimitImage.ImageResource = R.drawable.background_speed_sign;
                        }
                        Animation fadeIn = new AlphaAnimation(0, 1);
                        fadeIn.Interpolator = new DecelerateInterpolator();
                        fadeIn.Duration = 800;
                        speedLimitImage.Animation = fadeIn;
                        speedLimitImage.clearAnimation();
                    }
                    if (speedAlertImage != null)
                    {
                        speedAlertImage.Visibility = View.GONE;
                    }
                    if (speedLimitText != null)
                    {
                        speedLimitText.Visibility = View.VISIBLE;
                    }

                }
            }

            /// <summary>
            /// cancels the thread
            /// </summary>
            public virtual void cancel()
            {
                this.speedExceeded = false;
            }
        }

    }
}