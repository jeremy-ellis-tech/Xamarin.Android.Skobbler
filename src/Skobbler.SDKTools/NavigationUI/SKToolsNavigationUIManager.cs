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
using Skobbler.Ngx.Routing;
using Android.Content.Res;
using Skobbler.Ngx.Navigation;
using Java.Lang;
using Android.Graphics;
using Skobbler.Ngx.Map;
using Android.Media;
using Android.Views.Animations;
using System.Threading;
using Java.Util;
using Math = System.Math;
using Java.Text;

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
        public void OnSettingsItemClick(object sender, EventArgs e)
        {
            SKToolsLogicManager.Instance.handleSettingsItemsClick((View)sender);
        }

        /// <summary>
        /// Click listener for the rest of the views
        /// </summary>
        public void OnItemClick(object sender, EventArgs e)
        {
            SKToolsLogicManager.Instance.handleItemsClick((View)sender);
        }

        /// <summary>
        /// Block roads list item click
        /// </summary>
        public void OnBockedRoadsListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SKToolsLogicManager.Instance.handleBlockRoadsItemsClick(e.Parent, e.Position);
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
            rootLayout = (ViewGroup)currentActivity.FindViewById(rootId);
        }


        /// <summary>
        /// Inflates navigation relates views.
        /// </summary>
        /// <param name="activity"> </param>
        protected internal virtual void inflateNavigationViews(Activity activity)
        {
            activity.RunOnUiThread(() =>
            {
                LayoutInflater inflater = currentActivity.LayoutInflater;

                inflateSettingsMenu();

                RelativeLayout.LayoutParams relativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);

                backButtonPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_back_button, null, false);
                rootLayout.AddView(backButtonPanel, relativeLayoutParams);
                backButtonPanel.Id = SKToolsUtils.generateViewId();
                backButtonPanel.Visibility = ViewStates.Gone;
                backButtonPanel.FindViewById(Resource.Id.navigation_top_back_button).Click += OnItemClick;

                RelativeLayout.LayoutParams routeOverviewRelativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                routeOverviewRelativeLayoutParams.AddRule(LayoutRules.Below, backButtonPanel.Id);

                routeOverviewPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_route_overview_panel, null, false);
                rootLayout.AddView(routeOverviewPanel, routeOverviewRelativeLayoutParams);
                routeOverviewPanel.Visibility = ViewStates.Gone;

                roadBlockPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_roadblocks_list, null, false);
                rootLayout.AddView(roadBlockPanel, routeOverviewRelativeLayoutParams);
                roadBlockPanel.Visibility = ViewStates.Gone;


                reRoutingPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_rerouting_panel, null, false);
                RelativeLayout.LayoutParams reRoutingPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                reRoutingPanelParams.AddRule(LayoutRules.AlignParentTop);
                rootLayout.AddView(reRoutingPanel, reRoutingPanelParams);
                reRoutingPanel.Visibility = ViewStates.Gone;

                menuOptions = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_menu_options, null, false);
                rootLayout.AddView(menuOptions, relativeLayoutParams);
                menuOptions.Visibility = ViewStates.Gone;

                topCurrentNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_current_advice_panel, null, false);
                RelativeLayout.LayoutParams topCurrentAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                topCurrentAdviceParams.AddRule(LayoutRules.AlignParentTop);
                rootLayout.AddView(topCurrentNavigationPanel, topCurrentAdviceParams);
                topCurrentNavigationPanel.Id = SKToolsUtils.generateViewId();
                topCurrentNavigationPanel.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                topCurrentNavigationPanel.Visibility = ViewStates.Gone;

                topCurrentNavigationDistanceStreetPanel = (LinearLayout)topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_text_holder);
                topCurrentNavigationDistanceStreetPanel.Click += OnItemClick;
                RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_image_holder);
                topCurrentNavigationImagePanel.Click += OnItemClick;
                currentAdviceImage = (ImageView)topCurrentNavigationImagePanel.FindViewById(Resource.Id.current_advice_image_turn);
                currentAdviceName = (TextView)topCurrentNavigationDistanceStreetPanel.FindViewById(Resource.Id.current_advice_street_text);
                currentAdviceName.Selected = true;
                currentAdviceDistance = (TextView)topCurrentNavigationDistanceStreetPanel.FindViewById(Resource.Id.current_advice_distance_text);


                // next advice panel
                topNextNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_next_advice_panel, null, false);
                RelativeLayout.LayoutParams nextAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                nextAdviceParams.AddRule(LayoutRules.Below, topCurrentNavigationPanel.Id);
                rootLayout.AddView(topNextNavigationPanel, nextAdviceParams);
                topNextNavigationPanel.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                topNextNavigationPanel.Visibility = ViewStates.Gone;

                nextAdviceImageDistancePanel = (RelativeLayout)topNextNavigationPanel.FindViewById(Resource.Id.next_image_turn_advice_distance_layout);
                nextAdviceImageView = (ImageView)nextAdviceImageDistancePanel.FindViewById(Resource.Id.next_image_turn_advice);
                nextAdviceDistanceTextView = (TextView)nextAdviceImageDistancePanel.FindViewById(Resource.Id.next_advice_distance_text);
                nextAdviceStreetNamePanel = (RelativeLayout)topNextNavigationPanel.FindViewById(Resource.Id.next_advice_street_name_text_layout);
                nextAdviceStreetNameTextView = (TextView)nextAdviceStreetNamePanel.FindViewById(Resource.Id.next_advice_street_name_text);
                nextAdviceStreetNameTextView.Selected = true;

                freeDriveCurrentStreetPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_free_drive_current_street_panel, null, false);
                RelativeLayout.LayoutParams freeDrivePanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                freeDrivePanelParams.AddRule(LayoutRules.AlignParentTop);
                rootLayout.AddView(freeDriveCurrentStreetPanel, freeDrivePanelParams);
                freeDriveCurrentStreetPanel.Visibility = ViewStates.Gone;
                TextView freeDriveCurrentStreetText = (TextView)freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
                freeDriveCurrentStreetText.Text = "";

                viaPointPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_via_point_panel, null, false);
                rootLayout.AddView(viaPointPanel, freeDrivePanelParams);
                viaPointPanel.Visibility = ViewStates.Gone;
                TextView viaPointText = (TextView)viaPointPanel.FindViewById(Resource.Id.via_point_text_view);
                viaPointText.Text = "";

                inflateBottomPanels();
            });
        }

        /// <summary>
        /// Inflates the bottom views (speed panels, eta, route distance).
        /// </summary>
        private void inflateBottomPanels()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            speedPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_speed_panel, null, false);
            RelativeLayout.LayoutParams currentSpeedParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            currentSpeedParams.AddRule(LayoutRules.AlignParentBottom);
            currentSpeedParams.AddRule(LayoutRules.AlignParentLeft);
            rootLayout.AddView(speedPanel, currentSpeedParams);
            speedPanel.Id = SKToolsUtils.generateViewId();
            speedPanel.Visibility = ViewStates.Gone;
            currentSpeedText = (TextView)speedPanel.FindViewById(Resource.Id.free_drive_current_speed_text);
            currentSpeedTextValue = (TextView)speedPanel.FindViewById(Resource.Id.free_drive_current_speed_text_value);
            speedPanel.SetOnClickListener(null);

            arrivingETATimeGroupPanels = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_eta_arriving_group_panels, null, false);
            RelativeLayout.LayoutParams etaGroupPanelsParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            etaGroupPanelsParams.AddRule(LayoutRules.AlignParentBottom);
            etaGroupPanelsParams.AddRule(LayoutRules.AlignParentRight);
            rootLayout.AddView(arrivingETATimeGroupPanels, etaGroupPanelsParams);
            arrivingETATimeGroupPanels.Id = SKToolsUtils.generateViewId();
            arrivingETATimeGroupPanels.Visibility = ViewStates.Gone;
            estimatedTimePanel = (ViewGroup)arrivingETATimeGroupPanels.FindViewById(Resource.Id.navigation_bottom_right_estimated_panel);
            estimatedTimePanel.Click += OnItemClick;
            arrivingTimePanel = (ViewGroup)arrivingETATimeGroupPanels.FindViewById(Resource.Id.navigation_bottom_right_arriving_panel);
            arrivingTimePanel.Click += OnItemClick;
            estimatedTimeText = (TextView)estimatedTimePanel.FindViewById(Resource.Id.estimated_navigation_time_text);
            arrivingTimeText = (TextView)arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text);


            RelativeLayout.LayoutParams routeDistanceParams;
            if (currentActivity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Portrait)
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                routeDistanceParams.AddRule(LayoutRules.AlignParentBottom);
                routeDistanceParams.AddRule(LayoutRules.LeftOf, arrivingETATimeGroupPanels.Id);
                routeDistanceParams.AddRule(LayoutRules.RightOf, speedPanel.Id);
            }
            else
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                routeDistanceParams.AddRule(LayoutRules.AlignParentRight);
                routeDistanceParams.AddRule(LayoutRules.Above, arrivingETATimeGroupPanels.Id);
            }

            routeDistancePanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_route_distance, null, false);
            rootLayout.AddView(routeDistancePanel, routeDistanceParams);
            routeDistancePanel.Visibility = ViewStates.Gone;
            routeDistanceText = (TextView)routeDistancePanel.FindViewById(Resource.Id.arriving_distance_text);
            routeDistanceTextValue = (TextView)routeDistancePanel.FindViewById(Resource.Id.arriving_distance_text_value);
            RelativeLayout.LayoutParams positionMeParams;
            positionMeButtonPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_position_me_button, null, false);
            positionMeParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
            rootLayout.AddView(positionMeButtonPanel, positionMeParams);
            positionMeButtonPanel.Visibility = ViewStates.Gone;
            positionMeButtonPanel.FindViewById(Resource.Id.position_me_real_navigation_button).Click += OnItemClick;

        }

        /// <summary>
        /// Inflates simulation navigation type buttons.
        /// </summary>
        public virtual void inflateSimulationViews()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            navigationSimulationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_simulation_buttons, null, false);
            RelativeLayout.LayoutParams simulationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            navigationSimulationPanel.LayoutParameters = simulationPanelParams;
            rootLayout.AddView(navigationSimulationPanel, simulationPanelParams);
            navigationSimulationPanel.FindViewById(Resource.Id.menu_back_follower_mode_button).Click += OnItemClick;
            navigationSimulationPanel.FindViewById(Resource.Id.navigation_increase_speed).Click += OnItemClick;
            navigationSimulationPanel.FindViewById(Resource.Id.navigation_decrease_speed).Click += OnItemClick;
        }

        /// <summary>
        /// Inflates simulation menu.
        /// </summary>
        private void inflateSettingsMenu()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;

            settingsPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_settings, null, false);
            RelativeLayout.LayoutParams settingsPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
            rootLayout.AddView(settingsPanel, settingsPanelParams);
            settingsPanel.Visibility = ViewStates.Gone;

            settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_overview_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_route_info_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_roadblock_info_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_panning_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_quit_button).Click += OnSettingsItemClick;
            settingsPanel.FindViewById(Resource.Id.navigation_settings_back_button).Click += OnSettingsItemClick;
        }

        /// <summary>
        /// Inflates the pre navigation views, that contain the panels with alternative routes.
        /// </summary>
        private void inflatePreNavigationViews()
        {
            LayoutInflater inflater = currentActivity.LayoutInflater;
            preNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_pre_navigation_buttons_panel, null, false);
            RelativeLayout.LayoutParams preNavigationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
            rootLayout.AddView(preNavigationPanel, preNavigationPanelParams);
            preNavigationPanel.Visibility = ViewStates.Gone;

            preNavigationPanel.FindViewById(Resource.Id.first_route).Click += OnItemClick;
            preNavigationPanel.FindViewById(Resource.Id.second_route).Click += OnItemClick;
            preNavigationPanel.FindViewById(Resource.Id.third_route).Click += OnItemClick;
            preNavigationPanel.FindViewById(Resource.Id.cancel_pre_navigation_button).Click += OnItemClick;
            preNavigationPanel.FindViewById(Resource.Id.menu_back_prenavigation_button).Click += OnItemClick;
            preNavigationPanel.FindViewById(Resource.Id.start_navigation_button).Click += OnItemClick;
        }

        /// <summary>
        /// Shows the start navigation button from pre navigation panel.
        /// </summary>
        public virtual void showStartNavigationPanel()
        {
            preNavigationPanel.FindViewById(Resource.Id.start_navigation_button).Visibility = ViewStates.Visible;
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
                TextView oneRoute = (TextView)preNavigationPanel.FindViewById(Resource.Id.first_route);
                TextView twoRoutes = (TextView)preNavigationPanel.FindViewById(Resource.Id.second_route);
                TextView threeRoutes = (TextView)preNavigationPanel.FindViewById(Resource.Id.third_route);

                altRoutesButtons = new TextView[] { (TextView)preNavigationPanel.FindViewById(Resource.Id.first_route), (TextView)preNavigationPanel.FindViewById(Resource.Id.second_route), (TextView)preNavigationPanel.FindViewById(Resource.Id.third_route) };
                currentActivity.RunOnUiThread(() =>
                {
                    if (id == 0)
                    {
                        oneRoute.Visibility = ViewStates.Visible;
                        twoRoutes.Visibility = ViewStates.Gone;
                        threeRoutes.Visibility = ViewStates.Gone;
                        altRoutesButtons[0].Text = time + "\n" + distance;
                    }
                    else if (id == 1)
                    {
                        twoRoutes.Visibility = ViewStates.Visible;
                        threeRoutes.Visibility = ViewStates.Gone;
                        altRoutesButtons[1].Text = time + "\n" + distance;
                    }
                    else if (id == 2)
                    {
                        threeRoutes.Visibility = ViewStates.Visible;
                        altRoutesButtons[2].Text = time + "\n" + distance;
                    }
                });
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
                Button backButton = (Button)currentActivity.FindViewById(Resource.Id.menu_back_prenavigation_button);
                Button cancelButton = (Button)currentActivity.FindViewById(Resource.Id.cancel_pre_navigation_button);
                if (backButton.Text.Equals(">"))
                {
                    backButton.Text = "<";
                    cancelButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    backButton.Text = ">";
                    cancelButton.Visibility = ViewStates.Gone;
                }
            }
            else if (currentNavigationMode == NavigationMode.FOLLOWER)
            {
                Button backFollowerModeButton = (Button)currentActivity.FindViewById(Resource.Id.menu_back_follower_mode_button);
                RelativeLayout increaseDecreaseLayout = (RelativeLayout)currentActivity.FindViewById(Resource.Id.increase_decrease_layout);
                if (backFollowerModeButton.Text.Equals(">"))
                {
                    backFollowerModeButton.Text = "<";
                    increaseDecreaseLayout.Visibility = ViewStates.Visible;
                }
                else
                {
                    backFollowerModeButton.Text = ">";
                    increaseDecreaseLayout.Visibility = ViewStates.Gone;
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
                    currentActivity.FindViewById(Resource.Id.nav_settings_second_row).Visibility = ViewStates.Gone;

                    TextView routeInfo = (TextView)currentActivity.FindViewById(Resource.Id.navigation_settings_roadblock_info_text);
                    routeInfo.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_routeinfo, 0, 0);
                    routeInfo.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_routeinfo);
                }
                else
                {
                    currentActivity.FindViewById(Resource.Id.navigation_settings_overview_button).Visibility = ViewStates.Gone;
                    currentActivity.FindViewById(Resource.Id.navigation_settings_roadblock_info_button).Visibility = ViewStates.Gone;
                }

            }
        }

        /// <summary>
        /// hide view is visible
        /// </summary>
        /// <param name="target"> </param>
        private void hideViewIfVisible(ViewGroup target)
        {
            if (target != null && target.Visibility == ViewStates.Visible)
            {
                target.Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// shows the view if not visible
        /// </summary>
        /// <param name="target"> </param>
        private void showViewIfNotVisible(ViewGroup target)
        {
            if (target != null && target.Visibility == ViewStates.Gone)
            {
                target.Visibility = ViewStates.Visible;
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

            ArrayAdapter<string> listAdapter = new ArrayAdapter<string>(currentActivity, Android.Resource.Layout.SimpleListItem1, items);

            currentActivity.RunOnUiThread(() =>
            {
                ListView listView = (ListView)roadBlockPanel.FindViewById(Resource.Id.roadblock_list);
                listView.Adapter = listAdapter;
                listView.ItemClick += OnBockedRoadsListItemClick;
                roadBlockPanel.Visibility = ViewStates.Visible;
                backButtonPanel.Visibility = ViewStates.Visible;
            });
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
            IList<string> roadBlocksList = new List<string>();
            string[] list;

            if(distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                list = currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_meters);
            }
            else if(distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet)
            {
                list = currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_feet);
            }
            else if (distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitMilesYards)
            {
                list = currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_yards);
            }
            else
            {
                list = currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_meters);
            }

            foreach (var item in list)
            {
                roadBlocksList.Add(item);
            }

            long distance = distanceToDestination;
            // we initialize the sourceList with the elements in the roadBlocksList
            // that are smaller than the distance to destination
            if (distance < 500)
            {
                foreach (var item in roadBlocksList.Take(2))
                {
                    sourceList.Add(item);
                }
            }
            else if (distance < 2000)
            {
                foreach (var item in roadBlocksList.Take(3))
                {
                    sourceList.Add(item);
                }
            }
            else if (distance < 5000)
            {
                foreach (var item in roadBlocksList.Take(4))
                {
                    sourceList.Add(item);
                }
            }
            else if (distance < 10000)
            {
                foreach (var item in roadBlocksList.Take(5))
                {
                    sourceList.Add(item);
                }
            }
            else if (distance < 150000)
            {
                foreach (var item in roadBlocksList.Take(6))
                {
                    sourceList.Add(item);
                }
            }
            else
            {
                foreach (var item in roadBlocksList)
                {
                    sourceList.Add(item);
                }
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

            routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Gone;

            ((TextView)routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = address;
            routeOverviewPanel.Visibility = ViewStates.Visible;
            backButtonPanel.Visibility = ViewStates.Visible;
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

            ((TextView)routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_current_position_text)).Text = startAddress;
            ((TextView)routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = destinationAddress;

            routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Visible;
            routeOverviewPanel.Visibility = ViewStates.Visible;
            backButtonPanel.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Shows the route info screen from free drive mode.
        /// </summary>
        public virtual void showRouteInfoFreeDriveScreen()
        {
            currentNavigationMode = NavigationMode.ROUTE_INFO;

            hideViewIfVisible(freeDriveCurrentStreetPanel);
            hideBottomAndLeftPanels();
            routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Gone;

            routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_layout).Visibility = ViewStates.Visible;

            ((TextView)routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_label)).Text = currentActivity.GetString(Resource.String.current_position);
            ((TextView)routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = currentStreetNameFreeDriveString;

            routeOverviewPanel.Visibility = ViewStates.Visible;
            backButtonPanel.Visibility = ViewStates.Visible;
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

            alertDialog.SetTitle(Resource.String.exit_navigation_dialog_title);
            alertDialog.SetMessage(currentActivity.Resources.GetString(Resource.String.exit_navigation_dialog_message));

            alertDialog.SetPositiveButton(currentActivity.Resources.GetString(Resource.String.ok_label), (s,e) =>
                {
                    //dialog.Cancel();
                    isFreeDrive = false;
                    currentNavigationMode = NavigationMode.POST_NAVIGATION;
                    SKToolsLogicManager.Instance.StopNavigation();
                });

            alertDialog.SetNegativeButton(currentActivity.Resources.GetString(Resource.String.cancel_label), (s, e) =>
            {
                //dialog.Cancel();
            });

            alertDialog.Show();
        }

        /// <summary>
        /// Shows a dialog that notifies that the route calculation failed.
        /// </summary>
        /// <param name="statusCode"> </param>
        public virtual void showRouteCalculationFailedDialog(SKRouteListenerSKRoutingErrorCode statusCode)
        {
            currentActivity.RunOnUiThread(() =>
            {
                string dialogMessage;
                Resources res = currentActivity.Resources;

                if(statusCode == SKRouteListenerSKRoutingErrorCode.SameStartAndDestination)
                {
                    dialogMessage = res.GetString(Resource.String.route_same_start_and_destination);
                }
                else if(statusCode == SKRouteListenerSKRoutingErrorCode.InvalidStart)
                {
                    dialogMessage = res.GetString(Resource.String.route_invalid_start);
                }
                else if(statusCode == SKRouteListenerSKRoutingErrorCode.InvalidDestination)
                {
                    dialogMessage = res.GetString(Resource.String.route_invalid_destination);
                }
                else if(statusCode == SKRouteListenerSKRoutingErrorCode.InternalError)
                {
                    dialogMessage = res.GetString(Resource.String.route_unknown_server_error);
                }
                else
                {
                    dialogMessage = res.GetString(Resource.String.route_cannot_be_calculated);
                }

                AlertDialog.Builder alertDialog = new AlertDialog.Builder(currentActivity);
                alertDialog.SetTitle(Resource.String.routing_server_error);
                alertDialog.SetMessage(dialogMessage);

                alertDialog.SetNeutralButton(res.GetString(Resource.String.ok_label), (s,e) =>
                {
                    //alertDialog.Dismiss();
                });

                alertDialog.Show();
            });
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

                currentActivity.RunOnUiThread(() =>
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
                            currentAdviceImage.SetImageBitmap(decodedAdvice);
                            currentAdviceImage.Visibility = ViewStates.Visible;
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
                            handleSpeedLimitAvailable(countryCode, distanceUnitType, mapStyle);
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
                                nextAdviceDistanceTextView.Text = SKNavigationManager.Instance.FormatDistance(nextDistanceToAdvice);
                            }
                            if (nextVisualAdviceStreetName != null && !nextVisualAdviceStreetName.Equals(nextAdviceNextStreetName))
                            {
                                nextVisualAdviceStreetName = nextAdviceNextStreetName;
                                nextAdviceStreetNameTextView.Text = nextAdviceNextStreetName;
                            }

                            Bitmap adviceFile = SKToolsUtils.decodeFileToBitmap(nextVisualAdviceFile);
                            if (adviceFile != null)
                            {
                                nextAdviceImageView.SetImageBitmap(adviceFile);
                                nextAdviceImageView.Visibility = ViewStates.Visible;
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
                                    topNextNavigationPanel.Visibility = ViewStates.Gone;
                                }
                            }
                        }

                        // set current advice content
                        if (currentAdviceDistance != null && currentVisualAdviceDistance != currentDistanceToAdvice)
                        {
                            currentVisualAdviceDistance = currentDistanceToAdvice;
                            currentAdviceDistance.Text = SKNavigationManager.Instance.FormatDistance(currentDistanceToAdvice);
                        }
                        if (currentAdviceName != null && !showDestinationReachedFlag)
                        {
                            if (exitNumber != null && exitNumber.Length > 0)
                            {
                                string currentAdvice = currentActivity.Resources.GetString(Resource.String.exit_highway_advice_label) + " " + exitNumber;
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
                                currentAdviceImage.SetImageResource(Resource.Drawable.ic_destination_advise_black);
                            }
                            if (currentAdviceName != null)
                            {
                                currentVisualAdviceStreetName = currentActivity.Resources.GetString(Resource.String.destination_reached_info_text);
                                currentAdviceName.Text = currentVisualAdviceStreetName;
                            }
                            if (currentAdviceDistance != null)
                            {
                                currentVisualAdviceDistance = 0;
                                currentAdviceDistance.Visibility = ViewStates.Gone;
                            }
                            disableNextAdvice();
                        }


                        // set estimated/arriving time
                        if ((timeToDestination < 120) || (timeToDestination - 60 >= skNavigationState.CurrentAdviceTimeToDestination) || (timeToDestination + 60 < skNavigationState.CurrentAdviceTimeToDestination))
                        {

                            timeToDestination = skNavigationState.CurrentAdviceTimeToDestination;
                            if (estimatedTimePanelVisible)
                            {
                                showEstimatedTime();
                            }
                            else
                            {
                                showArrivingTime();
                            }
                        }

                        string[] distanceToDestinationSplit = SKNavigationManager.Instance.FormatDistance(distanceEstimatedUntilDestination).Split(' ');
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
                                    currentAdviceDistance.Visibility = ViewStates.Visible;
                                }
                                if (!firstTimeNavigation)
                                {
                                    topCurrentNavigationPanel.BringToFront();
                                }

                                showViewIfNotVisible(routeDistancePanel);
                                showViewIfNotVisible(arrivingETATimeGroupPanels);
                            }
                        }
                    });
            }

        }

        /// <summary>
        /// sets top panels background colour
        /// </summary>
        public virtual void setTopPanelsBackgroundColour(int mapStyle, bool currentAdviceChanged, bool nextAdviceChanged)
        {
            if (signPostsCountryExceptions.Contains(currentCountryCode))
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
                            setTopPanelsStyle(Resource.Color.white, Resource.Color.black, true);
                        }
                        if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            setTopPanelsStyle(Resource.Color.white, Resource.Color.black, false);
                        }

                    }
                    else
                    {
                        if (currentAdviceChanged || !nextAdviceChanged)
                        {
                            setTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, true);
                        }
                        if (nextAdviceChanged && isNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            setTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, false);
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
                        setTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.blue_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.blue_panel_night_background, Resource.Color.blue_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OSM_STREET_TYPE_PRIMARY || streetType == OSM_STREET_TYPE_PRIMARY_LINK)
            {
                if (currentCountryCode.Equals("CH"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.blue_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.blue_panel_night_background, Resource.Color.blue_panel_night_text, forCurrent);
                    }
                }
                else if (currentCountryCode.Equals("US"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.yellow_panel_day_background, Resource.Color.black, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.yellow_panel_night_background, Resource.Color.yellow_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OSM_STREET_TYPE_TRUNK || streetType == OSM_STREET_TYPE_TRUNK_LINK)
            {
                if ((currentCountryCode.Equals("GB")) || (currentCountryCode.Equals("US")))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                    {
                        setTopPanelsStyle(Resource.Color.white, Resource.Color.black, forCurrent);
                    }
                    else
                    {
                        setTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, forCurrent);
                    }
                }
            }
            else
            {
                if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
                {
                    setTopPanelsStyle(Resource.Color.white, Resource.Color.black, forCurrent);
                }
                else
                {
                    setTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, forCurrent);
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
                        topCurrentNavigationDistanceStreetPanel.SetBackgroundColor(currentActivity.Resources.GetColor(drawableId));
                    }
                    if (topCurrentNavigationPanel != null)
                    {
                        RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_image_holder);
                        if (topCurrentNavigationImagePanel != null)
                        {
                            topCurrentNavigationImagePanel.SetBackgroundColor(currentActivity.Resources.GetColor(drawableId));
                        }
                    }
                    if (currentAdviceDistance != null)
                    {
                        currentAdviceDistance.SetTextColor(currentActivity.Resources.GetColor(textColor));
                    }
                    if (currentAdviceName != null)
                    {
                        currentAdviceName.SetTextColor(currentActivity.Resources.GetColor(textColor));
                    }
                    currentAdviceBackgroundDrawableId = drawableId;
                }
                else
                {
                    if (nextAdviceImageDistancePanel != null)
                    {
                        nextAdviceImageDistancePanel.SetBackgroundColor(currentActivity.Resources.GetColor(drawableId));
                    }
                    if (nextAdviceStreetNamePanel != null)
                    {
                        nextAdviceStreetNamePanel.SetBackgroundColor(currentActivity.Resources.GetColor(drawableId));
                    }
                    if (nextAdviceDistanceTextView != null)
                    {
                        nextAdviceDistanceTextView.SetTextColor(currentActivity.Resources.GetColor(textColor));
                    }
                    if (nextAdviceStreetNameTextView != null)
                    {
                        nextAdviceStreetNameTextView.SetTextColor(currentActivity.Resources.GetColor(textColor));
                    }
                    nextAdviceBackgroundDrawableId = drawableId;
                    setNextAdviceOverlayVisibility();
                }
            }
            else
            {
                TextView freeDriveCurrentStreetText = (TextView)freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
                if (freeDriveCurrentStreetText != null)
                {
                    freeDriveCurrentStreetText.SetBackgroundColor(currentActivity.Resources.GetColor(drawableId));
                    freeDriveCurrentStreetText.SetTextColor(currentActivity.Resources.GetColor(textColor));
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
                topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_image_distance_overlay_background).Visibility = ViewStates.Visible;
                if (nextVisualAdviceStreetName != null)
                {
                    topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Visible;
                }
                else
                {
                    topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Gone;
                }
            }
            else
            {
                topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_image_distance_overlay_background).Visibility = ViewStates.Gone;
                topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// switch from estimated time to the arrived time
        /// </summary>
        public virtual void switchEstimatedTime()
        {
            currentActivity.RunOnUiThread(() =>
            {
                if (estimatedTimePanelVisible)
                {
                    showArrivingTime();
                }
                else
                {
                    showEstimatedTime();
                }
                estimatedTimePanelVisible = !estimatedTimePanelVisible;
            });
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
            DateTime arrivingTime = currentTime;
            int hours = timeToDestination / 3600;
            int minutes = (timeToDestination % 3600) / 60;
            arrivingTime.AddMinutes(minutes);
            TextView arrivingTimeAMPM = (TextView)arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text_ampm);
            if (arrivingTimeText != null)
            {
                SimpleDateFormat simpleDateFormat = new SimpleDateFormat("HH:mm");
                arrivingTime.Add(TimeSpan.FromHours(hours));
                arrivingTimeAMPM.Text = "";
                arrivingTimeText.Text = simpleDateFormat.Format(new Date(arrivingTime.Year, arrivingTime.Month, arrivingTime.Day));
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

            bool isLandscape = currentActivity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape;
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

            TextView speedLimitText = (TextView)speedPanel.FindViewById(Resource.Id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);

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
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign_us);
                        }
                    }
                }
                else
                {
                    if (!isDefaultSpeedSign)
                    {
                        if (speedLimitImage != null)
                        {
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign);
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
                        currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Visible;
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
                        speedLimitText.Visibility = ViewStates.Visible;
                        speedLimitImage.Visibility = ViewStates.Visible;
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
                    currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Gone;

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
            if (speedExceededThread != null && speedExceededThread.IsAlive)
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
                RelativeLayout currentSpeedPanel = (RelativeLayout)currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel);
                if (value == SKToolsMapOperationsManager.DAY_STYLE)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.gray));
                    }
                    currentActivity.FindViewById(Resource.Id.free_drive_current_speed_linear_layout).SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.gray));
                    if (currentSpeedText != null)
                    {
                        currentSpeedText.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.black));
                    }
                    if (currentSpeedTextValue != null)
                    {
                        currentSpeedTextValue.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.black));
                    }
                }
                else if (value == SKToolsMapOperationsManager.NIGHT_STYLE)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.speed_panel_night_background));
                    }
                    currentActivity.FindViewById(Resource.Id.free_drive_current_speed_linear_layout).SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.speed_panel_night_background));
                    if (currentSpeedText != null)
                    {
                        currentSpeedText.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.gray));
                    }
                    if (currentSpeedTextValue != null)
                    {
                        currentSpeedTextValue.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.gray));
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
            currentActivity.RunOnUiThread(() =>
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
                        handleSpeedLimitAvailable(countryCode, distanceUnitType, mapStyle);
                    }

                    setTopPanelsBackgroundColour(mapStyle, false, false);

                    if (streetName != null && !streetName.Equals(""))
                    {
                        currentStreetNameFreeDriveString = streetName;
                        ((TextView)freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text)).Text = streetName;
                    }
                    else
                    {
                        currentStreetNameFreeDriveString = null;
                    }
                    if (currentNavigationMode == NavigationMode.FOLLOWER && currentStreetNameFreeDriveString != null)
                    {
                        freeDriveCurrentStreetPanel.Visibility = ViewStates.Visible;
                        showViewIfNotVisible(speedPanel);
                    }
                }
            });
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
                currentActivity.RunOnUiThread(() =>
                {
                    if (settingsPanel != null)
                    {
                        rootLayout.RemoveView(settingsPanel);
                    }
                    rootLayout.RemoveView(speedPanel);
                    rootLayout.RemoveView(routeDistancePanel);
                    rootLayout.RemoveView(arrivingETATimeGroupPanels);

                    inflateSettingsMenu();
                    initialiseVolumeSeekBar();
                    inflateBottomPanels();

                    setAudioViewsFromSettings();
                    switchMapMode(displayMode);
                    switchDayNightStyle(mapStyle);

                    bool isLandscape = currentActivity.Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape;
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

                    setAdvicesFields();
                    changePanelsBackgroundAndTextViewsColour(mapStyle);
                });
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
                TextView freeDriveCurrentStreetText = (TextView)freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
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
            TextView speedLimitText = (TextView)speedPanel.FindViewById(Resource.Id.speed_limit_value);
            if (speedLimitAvailable && speedLimitText != null && currentSpeedLimit != 0)
            {
                speedLimitText.Text = Convert.ToString(Convert.ToString(SKToolsUtils.getSpeedByUnit(currentSpeedLimit, distanceUnitType)));
                changeSpeedSigns();

                currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Visible;
            }
        }

        /// <summary>
        /// changes the speed exceeded sign
        /// </summary>
        private void changeSpeedSigns()
        {
            TextView speedLimitText = (TextView)speedPanel.FindViewById(Resource.Id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);

            if (speedLimitExceeded)
            {
                if (speedExceededThread == null || !speedExceededThread.IsAlive)
                {
                    speedExceededThread = new SpeedExceededThread(this, true);
                    speedExceededThread.Start();
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
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign_us);
                        }
                        else
                        {
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign);
                        }
                    }
                    if (speedLimitText != null)
                    {
                        speedLimitText.Visibility = ViewStates.Visible;
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
                    nextAdviceStreetNamePanel.Visibility = ViewStates.Visible;
                }
                else
                {
                    nextAdviceStreetNamePanel.Visibility = ViewStates.Gone;
                }
            }
        }

        /// <summary>
        /// Shows the next advice.
        /// </summary>
        private void showNextAdvice()
        {
            topNextNavigationPanel.Visibility = ViewStates.Visible;
            topNextNavigationPanel.BringToFront();
        }

        /// <summary>
        /// removes the next advice
        /// </summary>
        private void disableNextAdvice()
        {
            isNextAdviceVisible = false;
            if (topNextNavigationPanel != null)
            {
                topNextNavigationPanel.Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// Shows the via point panel for 2 seconds.
        /// </summary>
        public virtual void showViaPointPanel()
        {
            hideViewIfVisible(topCurrentNavigationPanel);
            hideViewIfVisible(topNextNavigationPanel);
            showViewIfNotVisible(viaPointPanel);

            TextView viaPointText = (TextView)viaPointPanel.FindViewById(Resource.Id.via_point_text_view);
            viaPointText.Text = currentActivity.Resources.GetString(Resource.String.via_point_reached);

            Handler handler = new Handler();
            handler.PostDelayed(() => { hideViewIfVisible(viaPointPanel); }, 2000);

        }

        /// <summary>
        /// Initialises the volume bar.
        /// </summary>
        public virtual void initialiseVolumeSeekBar()
        {
            int currentVolume = SKToolsAdvicePlayer.getCurrentDeviceVolume(currentActivity);
            int maxVolume = SKToolsAdvicePlayer.getMaximAudioLevel(currentActivity);
            SeekBar volumeBar = (SeekBar)settingsPanel.FindViewById(Resource.Id.navigation_settings_volume);
            volumeBar.Max = maxVolume;
            volumeBar.Progress = currentVolume;

            volumeBar.ProgressChanged += (s, e) =>
            {
                AudioManager audioManager = (AudioManager)currentActivity.GetSystemService(Context.AudioService);
                audioManager.SetStreamVolume(Stream.Music, e.Progress, VolumeNotificationFlags.ShowUi);
            };
        }

        /// <summary>
        /// Changes audio settings menu item panels.
        /// </summary>
        public virtual void loadAudioSettings()
        {
            TextView audioText = ((TextView)settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
            int? audioImageTag = (int?)audioText.Tag;
            audioImageTag = audioImageTag == null ? 0 : audioImageTag;

            Resources res = currentActivity.Resources;
            if (audioImageTag == Resource.Drawable.ic_audio_on)
            {
                SKToolsAdvicePlayer.Instance.disableMute();
                SKToolsLogicManager.Instance.playLastAdvice();
                audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_off, 0, 0);
                audioText.Tag = Resource.Drawable.ic_audio_off;
                audioText.Text = res.GetString(Resource.String.navigate_settings_audio_off);
            }
            else if (audioImageTag == 0 || audioImageTag == Resource.Drawable.ic_audio_off)
            {
                SKToolsAdvicePlayer.Instance.stop();
                SKToolsAdvicePlayer.Instance.enableMute();
                audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_on, 0, 0);
                audioText.Tag = Resource.Drawable.ic_audio_on;
                audioText.Text = res.GetString(Resource.String.navigate_settings_audio_on);
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
                    TextView audioText = ((TextView)settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
                    audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_on, 0, 0);
                    audioText.Tag = Resource.Drawable.ic_audio_on;
                    audioText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_audio_on);
                }
                else
                {
                    TextView audioText = ((TextView)settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
                    audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_off, 0, 0);
                    audioText.Tag = Resource.Drawable.ic_audio_off;
                    audioText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_audio_off);
                }
            }
        }

        /// <summary>
        /// Changes settings menu item, map style panels.
        /// </summary>
        /// <param name="mapStyle"> </param>
        public virtual void switchDayNightStyle(int mapStyle)
        {
            TextView dayNightText = ((TextView)settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_text));
            if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_nightmode, 0, 0);
                dayNightText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_nightmode);
            }
            else
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_daymode, 0, 0);
                dayNightText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_daymode);
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
            TextView dayNightText = ((TextView)settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_text));
            if (displayMode == SKMapSettings.SKMapDisplayMode.Mode3d)
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_2d, 0, 0);
                dayNightText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_2d_view);
            }
            else
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_3d, 0, 0);
                dayNightText.Text = currentActivity.Resources.GetString(Resource.String.navigate_settings_3d_view);
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
                setPanelBackgroundAndTextColour(preNavigationPanel.FindViewById(Resource.Id.alternative_routes_layout), null, currentMapStyle);
                setPanelBackgroundAndTextColour(preNavigationPanel.FindViewById(Resource.Id.start_navigation_button), null, currentMapStyle);
            }
            else
            {

                if (!isFreeDrive)
                {
                    EtaTimeGroupPanelsBackgroundAndTextViewColour = currentMapStyle;

                    setPanelBackgroundAndTextColour(routeDistancePanel.FindViewById(Resource.Id.route_distance_linear_layout), routeDistanceText, currentMapStyle);
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
                setPanelBackgroundAndTextColour(backButtonPanel.FindViewById(Resource.Id.navigation_top_back_button), null, currentMapStyle);
                setPanelBackgroundAndTextColour(roadBlockPanel.FindViewById(Resource.Id.road_block_relative_layout), null, currentMapStyle);

                ViewGroup routeOverviewPanel = (ViewGroup)currentActivity.FindViewById(Resource.Id.navigation_route_overview_linear_layout);
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
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_overview_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_route_info_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_roadblock_info_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_panning_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_quit_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_back_button), null, value);
                setPanelBackgroundAndTextColour(settingsPanel.FindViewById(Resource.Id.navigation_settings_seek_bar_layout), null, value);
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
                TextView arrivingTimeAmPm = ((TextView)arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text_ampm));
                TextView estimatedTimeTextValue = ((TextView)estimatedTimePanel.FindViewById(Resource.Id.estimated_navigation_time_text_value));

                setPanelBackgroundAndTextColour(arrivingTimePanel, arrivingTimeText, value);
                setPanelBackgroundAndTextColour(null, arrivingTimeAmPm, value);
                arrivingTimeAmPm.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.bullet_gray, 0, Resource.Drawable.bullet_green, 0);

                setPanelBackgroundAndTextColour(estimatedTimePanel, estimatedTimeText, value);
                setPanelBackgroundAndTextColour(null, estimatedTimeTextValue, value);
                estimatedTimeTextValue.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.bullet_green, 0, Resource.Drawable.bullet_gray, 0);
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
                    panel.SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.navigation_style_day));
                }
                if (textView != null)
                {
                    textView.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.white));
                }
            }
            else if (currentMapStyle == SKToolsMapOperationsManager.NIGHT_STYLE)
            {
                if (panel != null)
                {
                    panel.SetBackgroundColor(currentActivity.Resources.GetColor(Resource.Color.navigation_style_night));
                }
                if (textView != null)
                {
                    textView.SetTextColor(currentActivity.Resources.GetColor(Resource.Color.gray));
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
                rootLayout.RemoveView(preNavigationPanel);
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
                rootLayout.RemoveView(topCurrentNavigationPanel);
                topCurrentNavigationPanel = null;
            }
            if (topNextNavigationPanel != null)
            {
                rootLayout.RemoveView(topNextNavigationPanel);
                topNextNavigationPanel = null;
            }
            if (menuOptions != null)
            {
                rootLayout.RemoveView(menuOptions);
                menuOptions = null;
            }
            if (routeDistancePanel != null)
            {
                rootLayout.RemoveView(routeDistancePanel);
                routeDistancePanel = null;
            }
            if (speedPanel != null)
            {
                rootLayout.RemoveView(speedPanel);
                speedPanel = null;
            }
            if (arrivingETATimeGroupPanels != null)
            {
                rootLayout.RemoveView(arrivingETATimeGroupPanels);
                arrivingETATimeGroupPanels = null;
            }
            if (reRoutingPanel != null)
            {
                rootLayout.RemoveView(reRoutingPanel);
                reRoutingPanel = null;
            }
            if (freeDriveCurrentStreetPanel != null)
            {
                rootLayout.RemoveView(freeDriveCurrentStreetPanel);
                freeDriveCurrentStreetPanel = null;
            }
            if (settingsPanel != null)
            {
                rootLayout.RemoveView(settingsPanel);
                settingsPanel = null;
            }
            if (navigationSimulationPanel != null)
            {
                rootLayout.RemoveView(navigationSimulationPanel);
                navigationSimulationPanel = null;
            }
            if (viaPointPanel != null)
            {
                rootLayout.RemoveView(viaPointPanel);
                viaPointPanel = null;
            }
            if (routeOverviewPanel != null)
            {
                rootLayout.RemoveView(routeOverviewPanel);
                routeOverviewPanel = null;
            }
            if (roadBlockPanel != null)
            {
                rootLayout.RemoveView(roadBlockPanel);
                roadBlockPanel = null;
            }
            if (backButtonPanel != null)
            {
                rootLayout.RemoveView(backButtonPanel);
                backButtonPanel = null;
            }
        }


        /// <summary>
        /// speed exceeded thread
        /// </summary>
        private class SpeedExceededThread : Java.Lang.Thread
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
                ImageView speedLimitImage = (ImageView)outerInstance.speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);
                ImageView speedAlertImage = (ImageView)outerInstance.speedPanel.FindViewById(Resource.Id.navigation_alert_sign_image);
                TextView speedLimitText = (TextView)outerInstance.speedPanel.FindViewById(Resource.Id.speed_limit_value);

                while (speedExceeded)
                {
                    outerInstance.currentActivity.RunOnUiThread(() =>
                    {
                        if (speedLimitText != null)
                        {
                            speedLimitText.Visibility = ViewStates.Gone;
                        }
                        if (speedAlertImage != null)
                        {
                            speedAlertImage.Visibility = ViewStates.Visible;
                        }
                        if (speedLimitImage != null)
                        {
                            if (outerInstance.isUS)
                            {
                                speedLimitImage.SetImageResource(Resource.Drawable.background_alert_sign_us);
                            }
                            else
                            {
                                speedLimitImage.SetImageResource(Resource.Drawable.background_alert_sign);
                            }

                            speedLimitImage.SetBackgroundDrawable(null);

                            Animation fadeOut = new AlphaAnimation(1, 0);
                            fadeOut.Interpolator = new AccelerateInterpolator();
                            fadeOut.Duration = 800;
                            speedLimitImage.Animation = fadeOut;
                            speedLimitImage.ClearAnimation();
                        }
                    });

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
                    outerInstance.currentActivity.RunOnUiThread(() =>
                    {
                        if (speedLimitImage != null)
                        {
                            if (outerInstance.isUS)
                            {
                                speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign_us);
                            }
                            else
                            {
                                speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign);
                            }
                            Animation fadeIn = new AlphaAnimation(0, 1);
                            fadeIn.Interpolator = new DecelerateInterpolator();
                            fadeIn.Duration = 800;
                            speedLimitImage.Animation = fadeIn;
                            speedLimitImage.ClearAnimation();
                        }
                        if (speedAlertImage != null)
                        {
                            speedAlertImage.Visibility = ViewStates.Gone;
                        }
                        if (speedLimitText != null)
                        {
                            speedLimitText.Visibility = ViewStates.Visible;
                        }
                    });
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