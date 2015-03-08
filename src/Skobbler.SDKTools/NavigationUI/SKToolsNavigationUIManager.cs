using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;
using Java.Text;
using Java.Util;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Routing;
using Math = System.Math;
using Orientation = Android.Content.Res.Orientation;
using Thread = Java.Lang.Thread;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    internal class SKToolsNavigationUiManager
    {

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static volatile SKToolsNavigationUiManager _instance;

        /// <summary>
        /// OSM street types
        /// </summary>
        private const int OsmStreetTypeMotorway = 9;

        private const int OsmStreetTypeMotorwayLink = 10;

        private const int OsmStreetTypePrimary = 13;

        private const int OsmStreetTypePrimaryLink = 14;

        private const int OsmStreetTypeTrunk = 24;

        private const int OsmStreetTypeTrunkLink = 25;

        /// <summary>
        /// the list with the country codes for which the top panel has a different
        /// color
        /// </summary>
        private static readonly string[] SignPostsCountryExceptions = { "DE", "AT", "GB", "IE", "CH", "US" };


        private enum NavigationMode
        {

            Settings,

            RouteInfo,

            RouteOverview,

            Panning,

            Roadblock,

            Follower,

            PreNavigation,

            PostNavigation

        }

        private NavigationMode _currentNavigationMode;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity _currentActivity;

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
        /// route overview panel
        /// </summary>
        private ViewGroup _routeOverviewPanel;

        /// <summary>
        /// the view for re routing
        /// </summary>
        private ViewGroup _reRoutingPanel;

        /// <summary>
        /// arriving navigation distance panel
        /// </summary>
        private ViewGroup _routeDistancePanel;

        /// <summary>
        /// the  position me button
        /// </summary>
        private ViewGroup _positionMeButtonPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup _topCurrentNavigationPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup _topNextNavigationPanel;

        /// <summary>
        /// the menu from tapping mode
        /// </summary>
        private ViewGroup _menuOptions;

        /// <summary>
        /// root layout - to this will be added all views
        /// </summary>
        private ViewGroup _rootLayout;

        /// <summary>
        /// road block screen
        /// </summary>
        private ViewGroup _roadBlockPanel;

        /// <summary>
        /// current street name panel from free drive mode
        /// </summary>
        private ViewGroup _freeDriveCurrentStreetPanel;

        /// <summary>
        /// via point panel
        /// </summary>
        private ViewGroup _viaPointPanel;

        /// <summary>
        /// top navigation panel
        /// </summary>
        private ViewGroup _speedPanel;

        /// <summary>
        /// the view flipper for estimated and arriving time panels
        /// </summary>
        private ViewGroup _arrivingEtaTimeGroupPanels;

        /// <summary>
        /// bottom right panel with estimated time
        /// </summary>
        private ViewGroup _estimatedTimePanel;

        /// <summary>
        /// bottom right panel with arriving time
        /// </summary>
        private ViewGroup _arrivingTimePanel;

        /// <summary>
        /// navigation arriving time text
        /// </summary>
        private TextView _arrivingTimeText;

        /// <summary>
        /// navigation estimated time text
        /// </summary>
        private TextView _estimatedTimeText;

        /// <summary>
        /// true if the estimated time panel is visible, false if the arriving time
        /// panel is visible
        /// </summary>
        private bool _estimatedTimePanelVisible;

        /// <summary>
        /// top current navigation distance & street panel
        /// </summary>
        private LinearLayout _topCurrentNavigationDistanceStreetPanel;

        /// <summary>
        /// the image for the visual advice
        /// </summary>
        private ImageView _nextAdviceImageView;

        /// <summary>
        /// next advice image distance panel
        /// </summary>
        private RelativeLayout _nextAdviceImageDistancePanel;

        /// <summary>
        /// next advice street name panel
        /// </summary>
        private RelativeLayout _nextAdviceStreetNamePanel;

        /// <summary>
        /// next advice street name text
        /// </summary>
        private TextView _nextAdviceStreetNameTextView;

        /// <summary>
        /// arriving navigation distance text
        /// </summary>
        private TextView _routeDistanceText;

        /// <summary>
        /// arriving navigation distance text value
        /// </summary>
        private TextView _routeDistanceTextValue;

        /// <summary>
        /// the alternative routes buttons
        /// </summary>
        private TextView[] _altRoutesButtons;

        /// <summary>
        /// true, if free drive selected
        /// </summary>
        private bool _isFreeDrive;

        /// <summary>
        /// true if the country code for destination is US, false otherwise
        /// </summary>
        private bool _isUs;

        /// <summary>
        /// true if the country code is different from US, false otherwise
        /// </summary>
        private bool _isDefaultSpeedSign;

        /// <summary>
        /// flag that indicates if there is a speed limit on the current road
        /// </summary>
        private bool _speedLimitAvailable;

        /// <summary>
        /// current street name from free drive mode
        /// </summary>
        private string _currentStreetNameFreeDriveString;

        /// <summary>
        /// true if the background color for top panel is the default one, false
        /// otherwise
        /// </summary>
        private bool _isDefaultTopPanelBackgroundColor = true;

        /// <summary>
        /// the drawable id for the current advice background
        /// </summary>
        private int _currentAdviceBackgroundDrawableId;

        /// <summary>
        /// the drawable id for the next advice background
        /// </summary>
        private int _nextAdviceBackgroundDrawableId;

        /// <summary>
        /// the thread for speed exceeded
        /// </summary>
        private SpeedExceededThread _speedExceededThread;

        /// <summary>
        /// current street type
        /// </summary>
        protected internal int NextStreetType;

        /// <summary>
        /// next street type
        /// </summary>
        protected internal int SecondNextStreetType;

        /// <summary>
        /// the current estimated total distance of the navigation trip
        /// </summary>
        private long _navigationTotalDistance;

        /// <summary>
        /// current speed limit
        /// </summary>
        protected internal double CurrentSpeedLimit;

        /// <summary>
        /// true if the speed limit is exceeded, false otherwise
        /// </summary>
        protected internal bool SpeedLimitExceeded;

        /// <summary>
        /// false when we start a navigation, becomes true when first advice is
        /// received
        /// </summary>
        protected internal bool FirstAdviceReceived;

        /// <summary>
        /// true if next advice is visible, false otherwise
        /// </summary>
        protected internal bool IsNextAdviceVisible;

        /// <summary>
        /// true when the application starts
        /// </summary>
        private bool _firstTimeNavigation = true;

        /// <summary>
        /// the currently estimated duration of the route to the navi destination
        /// </summary>
        protected internal int TimeToDestination;

        protected internal int InitialTimeToDestination;

        /// <summary>
        /// whether to display the navigation flag or not
        /// </summary>
        public bool ShowDestinationReachedFlag;

        /// <summary>
        /// the distance estimated between the user current position and destination
        /// </summary>
        public static int DistanceEstimatedUntilDestination;

        /// <summary>
        /// current speed
        /// </summary>
        private double _currentSpeed;

        /// <summary>
        /// route distance string
        /// </summary>
        protected internal string RouteDistanceString;

        /// <summary>
        /// route distance value string
        /// </summary>
        protected internal string RouteDistanceValueString;

        /// <summary>
        /// current street name
        /// </summary>
        protected internal TextView CurrentAdviceName;

        /// <summary>
        /// current advice distance
        /// </summary>
        protected internal TextView CurrentAdviceDistance;

        /// <summary>
        /// current advice street name string
        /// </summary>
        protected internal string CurrentVisualAdviceStreetName;

        /// <summary>
        /// current advice distance string
        /// </summary>
        protected internal int CurrentVisualAdviceDistance;

        /// <summary>
        /// next advice street name
        /// </summary>
        protected internal string NextVisualAdviceStreetName;

        /// <summary>
        /// current speed text from free drive mode
        /// </summary>
        protected internal TextView CurrentSpeedText;

        /// <summary>
        /// current speed text from free drive mode
        /// </summary>
        protected internal TextView CurrentSpeedTextValue;

        /// <summary>
        /// current speed string
        /// </summary>
        protected internal string CurrentSpeedString;

        /// <summary>
        /// next advice distance
        /// </summary>
        protected internal int NextVisualAdviceDistance;

        /// <summary>
        /// next advice distance text
        /// </summary>
        protected internal TextView NextAdviceDistanceTextView;

        /// <summary>
        /// current speed unit for navigation mode
        /// </summary>
        private SKMaps.SKDistanceUnitType _distanceUnitType;

        /// <summary>
        /// the image for the visual advice
        /// </summary>
        protected internal ImageView CurrentAdviceImage;

        /// <summary>
        /// country code
        /// </summary>
        protected internal string CurrentCountryCode;

        /// <summary>
        /// Click listener for settings menu views
        /// </summary>
        public void OnSettingsItemClick(object sender, EventArgs e)
        {
            SKToolsLogicManager.Instance.HandleSettingsItemsClick((View)sender);
        }

        /// <summary>
        /// Click listener for the rest of the views
        /// </summary>
        public void OnItemClick(object sender, EventArgs e)
        {
            SKToolsLogicManager.Instance.HandleItemsClick((View)sender);
        }

        /// <summary>
        /// Block roads list item click
        /// </summary>
        public void OnBockedRoadsListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SKToolsLogicManager.Instance.HandleBlockRoadsItemsClick(e.Parent, e.Position);
        }

        /// <summary>
        /// Creates a single instance of <seealso cref="SKToolsNavigationUiManager"/>
        /// 
        /// @return
        /// </summary>
        public static SKToolsNavigationUiManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(SKToolsNavigationUiManager))
                    {
                        if (_instance == null)
                        {
                            _instance = new SKToolsNavigationUiManager();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sets the current activity.
        /// </summary>
        /// <param name="activity"> </param>
        /// <param name="rootId"> </param>
        protected internal virtual void SetActivity(Activity activity, int rootId)
        {
            _currentActivity = activity;
            _rootLayout = (ViewGroup)_currentActivity.FindViewById(rootId);
        }


        /// <summary>
        /// Inflates navigation relates views.
        /// </summary>
        /// <param name="activity"> </param>
        protected internal virtual void InflateNavigationViews(Activity activity)
        {
            activity.RunOnUiThread(() =>
            {
                LayoutInflater inflater = _currentActivity.LayoutInflater;

                InflateSettingsMenu();

                RelativeLayout.LayoutParams relativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);

                _backButtonPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_back_button, null, false);
                _rootLayout.AddView(_backButtonPanel, relativeLayoutParams);
                _backButtonPanel.Id = SKToolsUtils.GenerateViewId();
                _backButtonPanel.Visibility = ViewStates.Gone;
                _backButtonPanel.FindViewById(Resource.Id.navigation_top_back_button).Click += OnItemClick;

                RelativeLayout.LayoutParams routeOverviewRelativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                routeOverviewRelativeLayoutParams.AddRule(LayoutRules.Below, _backButtonPanel.Id);

                _routeOverviewPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_route_overview_panel, null, false);
                _rootLayout.AddView(_routeOverviewPanel, routeOverviewRelativeLayoutParams);
                _routeOverviewPanel.Visibility = ViewStates.Gone;

                _roadBlockPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_roadblocks_list, null, false);
                _rootLayout.AddView(_roadBlockPanel, routeOverviewRelativeLayoutParams);
                _roadBlockPanel.Visibility = ViewStates.Gone;


                _reRoutingPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_rerouting_panel, null, false);
                RelativeLayout.LayoutParams reRoutingPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                reRoutingPanelParams.AddRule(LayoutRules.AlignParentTop);
                _rootLayout.AddView(_reRoutingPanel, reRoutingPanelParams);
                _reRoutingPanel.Visibility = ViewStates.Gone;

                _menuOptions = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_menu_options, null, false);
                _rootLayout.AddView(_menuOptions, relativeLayoutParams);
                _menuOptions.Visibility = ViewStates.Gone;

                _topCurrentNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_current_advice_panel, null, false);
                RelativeLayout.LayoutParams topCurrentAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                topCurrentAdviceParams.AddRule(LayoutRules.AlignParentTop);
                _rootLayout.AddView(_topCurrentNavigationPanel, topCurrentAdviceParams);
                _topCurrentNavigationPanel.Id = SKToolsUtils.GenerateViewId();
                _topCurrentNavigationPanel.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                _topCurrentNavigationPanel.Visibility = ViewStates.Gone;

                _topCurrentNavigationDistanceStreetPanel = (LinearLayout)_topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_text_holder);
                _topCurrentNavigationDistanceStreetPanel.Click += OnItemClick;
                RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)_topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_image_holder);
                topCurrentNavigationImagePanel.Click += OnItemClick;
                CurrentAdviceImage = (ImageView)topCurrentNavigationImagePanel.FindViewById(Resource.Id.current_advice_image_turn);
                CurrentAdviceName = (TextView)_topCurrentNavigationDistanceStreetPanel.FindViewById(Resource.Id.current_advice_street_text);
                CurrentAdviceName.Selected = true;
                CurrentAdviceDistance = (TextView)_topCurrentNavigationDistanceStreetPanel.FindViewById(Resource.Id.current_advice_distance_text);


                // next advice panel
                _topNextNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_next_advice_panel, null, false);
                RelativeLayout.LayoutParams nextAdviceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                nextAdviceParams.AddRule(LayoutRules.Below, _topCurrentNavigationPanel.Id);
                _rootLayout.AddView(_topNextNavigationPanel, nextAdviceParams);
                _topNextNavigationPanel.Measure(View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified), View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                _topNextNavigationPanel.Visibility = ViewStates.Gone;

                _nextAdviceImageDistancePanel = (RelativeLayout)_topNextNavigationPanel.FindViewById(Resource.Id.next_image_turn_advice_distance_layout);
                _nextAdviceImageView = (ImageView)_nextAdviceImageDistancePanel.FindViewById(Resource.Id.next_image_turn_advice);
                NextAdviceDistanceTextView = (TextView)_nextAdviceImageDistancePanel.FindViewById(Resource.Id.next_advice_distance_text);
                _nextAdviceStreetNamePanel = (RelativeLayout)_topNextNavigationPanel.FindViewById(Resource.Id.next_advice_street_name_text_layout);
                _nextAdviceStreetNameTextView = (TextView)_nextAdviceStreetNamePanel.FindViewById(Resource.Id.next_advice_street_name_text);
                _nextAdviceStreetNameTextView.Selected = true;

                _freeDriveCurrentStreetPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_free_drive_current_street_panel, null, false);
                RelativeLayout.LayoutParams freeDrivePanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                freeDrivePanelParams.AddRule(LayoutRules.AlignParentTop);
                _rootLayout.AddView(_freeDriveCurrentStreetPanel, freeDrivePanelParams);
                _freeDriveCurrentStreetPanel.Visibility = ViewStates.Gone;
                TextView freeDriveCurrentStreetText = (TextView)_freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
                freeDriveCurrentStreetText.Text = "";

                _viaPointPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_via_point_panel, null, false);
                _rootLayout.AddView(_viaPointPanel, freeDrivePanelParams);
                _viaPointPanel.Visibility = ViewStates.Gone;
                TextView viaPointText = (TextView)_viaPointPanel.FindViewById(Resource.Id.via_point_text_view);
                viaPointText.Text = "";

                InflateBottomPanels();
            });
        }

        /// <summary>
        /// Inflates the bottom views (speed panels, eta, route distance).
        /// </summary>
        private void InflateBottomPanels()
        {
            LayoutInflater inflater = _currentActivity.LayoutInflater;
            _speedPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_speed_panel, null, false);
            RelativeLayout.LayoutParams currentSpeedParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            currentSpeedParams.AddRule(LayoutRules.AlignParentBottom);
            currentSpeedParams.AddRule(LayoutRules.AlignParentLeft);
            _rootLayout.AddView(_speedPanel, currentSpeedParams);
            _speedPanel.Id = SKToolsUtils.GenerateViewId();
            _speedPanel.Visibility = ViewStates.Gone;
            CurrentSpeedText = (TextView)_speedPanel.FindViewById(Resource.Id.free_drive_current_speed_text);
            CurrentSpeedTextValue = (TextView)_speedPanel.FindViewById(Resource.Id.free_drive_current_speed_text_value);
            _speedPanel.SetOnClickListener(null);

            _arrivingEtaTimeGroupPanels = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_eta_arriving_group_panels, null, false);
            RelativeLayout.LayoutParams etaGroupPanelsParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            etaGroupPanelsParams.AddRule(LayoutRules.AlignParentBottom);
            etaGroupPanelsParams.AddRule(LayoutRules.AlignParentRight);
            _rootLayout.AddView(_arrivingEtaTimeGroupPanels, etaGroupPanelsParams);
            _arrivingEtaTimeGroupPanels.Id = SKToolsUtils.GenerateViewId();
            _arrivingEtaTimeGroupPanels.Visibility = ViewStates.Gone;
            _estimatedTimePanel = (ViewGroup)_arrivingEtaTimeGroupPanels.FindViewById(Resource.Id.navigation_bottom_right_estimated_panel);
            _estimatedTimePanel.Click += OnItemClick;
            _arrivingTimePanel = (ViewGroup)_arrivingEtaTimeGroupPanels.FindViewById(Resource.Id.navigation_bottom_right_arriving_panel);
            _arrivingTimePanel.Click += OnItemClick;
            _estimatedTimeText = (TextView)_estimatedTimePanel.FindViewById(Resource.Id.estimated_navigation_time_text);
            _arrivingTimeText = (TextView)_arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text);


            RelativeLayout.LayoutParams routeDistanceParams;
            if (_currentActivity.Resources.Configuration.Orientation == Orientation.Portrait)
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
                routeDistanceParams.AddRule(LayoutRules.AlignParentBottom);
                routeDistanceParams.AddRule(LayoutRules.LeftOf, _arrivingEtaTimeGroupPanels.Id);
                routeDistanceParams.AddRule(LayoutRules.RightOf, _speedPanel.Id);
            }
            else
            {
                routeDistanceParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
                routeDistanceParams.AddRule(LayoutRules.AlignParentRight);
                routeDistanceParams.AddRule(LayoutRules.Above, _arrivingEtaTimeGroupPanels.Id);
            }

            _routeDistancePanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_route_distance, null, false);
            _rootLayout.AddView(_routeDistancePanel, routeDistanceParams);
            _routeDistancePanel.Visibility = ViewStates.Gone;
            _routeDistanceText = (TextView)_routeDistancePanel.FindViewById(Resource.Id.arriving_distance_text);
            _routeDistanceTextValue = (TextView)_routeDistancePanel.FindViewById(Resource.Id.arriving_distance_text_value);
            RelativeLayout.LayoutParams positionMeParams;
            _positionMeButtonPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_position_me_button, null, false);
            positionMeParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
            _rootLayout.AddView(_positionMeButtonPanel, positionMeParams);
            _positionMeButtonPanel.Visibility = ViewStates.Gone;
            _positionMeButtonPanel.FindViewById(Resource.Id.position_me_real_navigation_button).Click += OnItemClick;

        }

        /// <summary>
        /// Inflates simulation navigation type buttons.
        /// </summary>
        public virtual void InflateSimulationViews()
        {
            LayoutInflater inflater = _currentActivity.LayoutInflater;
            _navigationSimulationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_simulation_buttons, null, false);
            RelativeLayout.LayoutParams simulationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            _navigationSimulationPanel.LayoutParameters = simulationPanelParams;
            _rootLayout.AddView(_navigationSimulationPanel, simulationPanelParams);
            _navigationSimulationPanel.FindViewById(Resource.Id.menu_back_follower_mode_button).Click += OnItemClick;
            _navigationSimulationPanel.FindViewById(Resource.Id.navigation_increase_speed).Click += OnItemClick;
            _navigationSimulationPanel.FindViewById(Resource.Id.navigation_decrease_speed).Click += OnItemClick;
        }

        /// <summary>
        /// Inflates simulation menu.
        /// </summary>
        private void InflateSettingsMenu()
        {
            LayoutInflater inflater = _currentActivity.LayoutInflater;

            _settingsPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_navigation_settings, null, false);
            RelativeLayout.LayoutParams settingsPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.MatchParent);
            _rootLayout.AddView(_settingsPanel, settingsPanelParams);
            _settingsPanel.Visibility = ViewStates.Gone;

            _settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_overview_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_route_info_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_roadblock_info_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_panning_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_quit_button).Click += OnSettingsItemClick;
            _settingsPanel.FindViewById(Resource.Id.navigation_settings_back_button).Click += OnSettingsItemClick;
        }

        /// <summary>
        /// Inflates the pre navigation views, that contain the panels with alternative routes.
        /// </summary>
        private void InflatePreNavigationViews()
        {
            LayoutInflater inflater = _currentActivity.LayoutInflater;
            _preNavigationPanel = (ViewGroup)inflater.Inflate(Resource.Layout.element_pre_navigation_buttons_panel, null, false);
            RelativeLayout.LayoutParams preNavigationPanelParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MatchParent, RelativeLayout.LayoutParams.WrapContent);
            _rootLayout.AddView(_preNavigationPanel, preNavigationPanelParams);
            _preNavigationPanel.Visibility = ViewStates.Gone;

            _preNavigationPanel.FindViewById(Resource.Id.first_route).Click += OnItemClick;
            _preNavigationPanel.FindViewById(Resource.Id.second_route).Click += OnItemClick;
            _preNavigationPanel.FindViewById(Resource.Id.third_route).Click += OnItemClick;
            _preNavigationPanel.FindViewById(Resource.Id.cancel_pre_navigation_button).Click += OnItemClick;
            _preNavigationPanel.FindViewById(Resource.Id.menu_back_prenavigation_button).Click += OnItemClick;
            _preNavigationPanel.FindViewById(Resource.Id.start_navigation_button).Click += OnItemClick;
        }

        /// <summary>
        /// Shows the start navigation button from pre navigation panel.
        /// </summary>
        public virtual void ShowStartNavigationPanel()
        {
            _preNavigationPanel.FindViewById(Resource.Id.start_navigation_button).Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Sets the pre navigation buttons, related to alternative routes with corresponding time and distance.
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="time"> </param>
        /// <param name="distance"> </param>
        public virtual void SePreNavigationButtons(int id, string time, string distance)
        {
            if (_preNavigationPanel != null)
            {
                TextView oneRoute = (TextView)_preNavigationPanel.FindViewById(Resource.Id.first_route);
                TextView twoRoutes = (TextView)_preNavigationPanel.FindViewById(Resource.Id.second_route);
                TextView threeRoutes = (TextView)_preNavigationPanel.FindViewById(Resource.Id.third_route);

                _altRoutesButtons = new[] { (TextView)_preNavigationPanel.FindViewById(Resource.Id.first_route), (TextView)_preNavigationPanel.FindViewById(Resource.Id.second_route), (TextView)_preNavigationPanel.FindViewById(Resource.Id.third_route) };
                _currentActivity.RunOnUiThread(() =>
                {
                    if (id == 0)
                    {
                        oneRoute.Visibility = ViewStates.Visible;
                        twoRoutes.Visibility = ViewStates.Gone;
                        threeRoutes.Visibility = ViewStates.Gone;
                        _altRoutesButtons[0].Text = time + "\n" + distance;
                    }
                    else if (id == 1)
                    {
                        twoRoutes.Visibility = ViewStates.Visible;
                        threeRoutes.Visibility = ViewStates.Gone;
                        _altRoutesButtons[1].Text = time + "\n" + distance;
                    }
                    else if (id == 2)
                    {
                        threeRoutes.Visibility = ViewStates.Visible;
                        _altRoutesButtons[2].Text = time + "\n" + distance;
                    }
                });
            }
        }

        /// <summary>
        /// Shows the pre navigation screen.
        /// </summary>
        public virtual void ShowPreNavigationScreen()
        {
            InflatePreNavigationViews();
            ShowViewIfNotVisible(_preNavigationPanel);
            _currentNavigationMode = NavigationMode.PreNavigation;
        }

        /// <summary>
        /// Selects an alternative route button depending of the index.
        /// </summary>
        /// <param name="routeIndex"> </param>
        public virtual void SelectAlternativeRoute(int routeIndex)
        {
            if (_altRoutesButtons == null)
            {
                return;
            }
            foreach (TextView b in _altRoutesButtons)
            {
                b.Selected = false;
            }
            _altRoutesButtons[routeIndex].Selected = true;
        }

        /// <summary>
        /// Handles back button in pre navigation and follower mode.
        /// </summary>
        public virtual void HandleNavigationBackButton()
        {

            if (_currentNavigationMode == NavigationMode.PreNavigation)
            {
                Button backButton = (Button)_currentActivity.FindViewById(Resource.Id.menu_back_prenavigation_button);
                Button cancelButton = (Button)_currentActivity.FindViewById(Resource.Id.cancel_pre_navigation_button);
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
            else if (_currentNavigationMode == NavigationMode.Follower)
            {
                Button backFollowerModeButton = (Button)_currentActivity.FindViewById(Resource.Id.menu_back_follower_mode_button);
                RelativeLayout increaseDecreaseLayout = (RelativeLayout)_currentActivity.FindViewById(Resource.Id.increase_decrease_layout);
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
                    _currentActivity.FindViewById(Resource.Id.nav_settings_second_row).Visibility = ViewStates.Gone;

                    TextView routeInfo = (TextView)_currentActivity.FindViewById(Resource.Id.navigation_settings_roadblock_info_text);
                    routeInfo.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_routeinfo, 0, 0);
                    routeInfo.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_routeinfo);
                }
                else
                {
                    _currentActivity.FindViewById(Resource.Id.navigation_settings_overview_button).Visibility = ViewStates.Gone;
                    _currentActivity.FindViewById(Resource.Id.navigation_settings_roadblock_info_button).Visibility = ViewStates.Gone;
                }

            }
        }

        /// <summary>
        /// hide view is visible
        /// </summary>
        /// <param name="target"> </param>
        private void HideViewIfVisible(ViewGroup target)
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
        private void ShowViewIfNotVisible(ViewGroup target)
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
        public virtual void ShowRoadBlockMode(SKMaps.SKDistanceUnitType distanceUnit, long distanceToDestination)
        {
            _currentNavigationMode = NavigationMode.Roadblock;

            IList<string> items = GetRoadBlocksOptionsList(distanceUnit, distanceToDestination);

            ArrayAdapter<string> listAdapter = new ArrayAdapter<string>(_currentActivity, Android.Resource.Layout.SimpleListItem1, items);

            _currentActivity.RunOnUiThread(() =>
            {
                ListView listView = (ListView)_roadBlockPanel.FindViewById(Resource.Id.roadblock_list);
                listView.Adapter = listAdapter;
                listView.ItemClick += OnBockedRoadsListItemClick;
                _roadBlockPanel.Visibility = ViewStates.Visible;
                _backButtonPanel.Visibility = ViewStates.Visible;
            });
        }

        /// <summary>
        /// Gets the list with road block distance options.
        /// </summary>
        /// <param name="distanceUnit"> </param>
        /// <param name="distanceToDestination">
        /// @return </param>
        private IList<string> GetRoadBlocksOptionsList(SKMaps.SKDistanceUnitType distanceUnit, long distanceToDestination)
        {
            IList<string> sourceList = new List<string>();
            IList<string> roadBlocksList = new List<string>();
            string[] list;

            if(distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                list = _currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_meters);
            }
            else if(distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet)
            {
                list = _currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_feet);
            }
            else if (distanceUnit == SKMaps.SKDistanceUnitType.DistanceUnitMilesYards)
            {
                list = _currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_yards);
            }
            else
            {
                list = _currentActivity.Resources.GetStringArray(Resource.Array.road_blocks_in_meters);
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
                return _currentNavigationMode == NavigationMode.PreNavigation;
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
                return _currentNavigationMode == NavigationMode.Follower;
            }
        }

        /// <summary>
        /// Sets the navigation in follower mode.
        /// </summary>
        public virtual void SetFollowerMode()
        {
            _currentNavigationMode = NavigationMode.Follower;
        }

        /// <summary>
        /// Shows the panel from the main navigation screen.
        /// </summary>
        /// <param name="isSimulationMode"> </param>
        public virtual void ShowFollowerModePanels(bool isSimulationMode)
        {

            HideViewIfVisible(_positionMeButtonPanel);
            HideViewIfVisible(_settingsPanel);
            HideViewIfVisible(_roadBlockPanel);
            HideViewIfVisible(_backButtonPanel);
            HideViewIfVisible(_routeOverviewPanel);

            ShowViewIfNotVisible(_speedPanel);

            if (!_isFreeDrive)
            {
                ShowViewIfNotVisible(_routeDistancePanel);
                ShowViewIfNotVisible(_topCurrentNavigationPanel);
                ShowViewIfNotVisible(_arrivingEtaTimeGroupPanels);
            }
            else
            {
                ShowViewIfNotVisible(_freeDriveCurrentStreetPanel);
            }

            if (isSimulationMode)
            {
                ShowViewIfNotVisible(_navigationSimulationPanel);
            }
        }

        /// <summary>
        /// Shows the panning mode screen.
        /// </summary>
        /// <param name="isNavigationTypeReal"> </param>
        public virtual void ShowPanningMode(bool isNavigationTypeReal)
        {
            _currentNavigationMode = NavigationMode.Panning;
            if (isNavigationTypeReal)
            {
                ShowViewIfNotVisible(_positionMeButtonPanel);
            }
            ShowViewIfNotVisible(_backButtonPanel);
            CancelSpeedExceededThread();
        }

        /// <summary>
        /// Shows the setting menu screen.
        /// </summary>
        public virtual void ShowSettingsMode()
        {
            _currentNavigationMode = NavigationMode.Settings;
            HideViewIfVisible(_navigationSimulationPanel);
            InitialiseVolumeSeekBar();
            HideViewIfVisible(_topNextNavigationPanel);
            HideViewIfVisible(_topCurrentNavigationPanel);
            HideViewIfVisible(_routeOverviewPanel);
            HideViewIfVisible(_reRoutingPanel);
            HideViewIfVisible(_freeDriveCurrentStreetPanel);
            HideBottomAndLeftPanels();
            ShowViewIfNotVisible(_settingsPanel);
        }

        /// <summary>
        /// Shows the overview screen.
        /// </summary>
        /// <param name="address"> </param>
        public virtual void ShowOverviewMode(string address)
        {
            _currentNavigationMode = NavigationMode.RouteOverview;

            HideViewIfVisible(_topNextNavigationPanel);
            HideViewIfVisible(_topCurrentNavigationPanel);
            HideBottomAndLeftPanels();

            _routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Gone;

            ((TextView)_routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = address;
            _routeOverviewPanel.Visibility = ViewStates.Visible;
            _backButtonPanel.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Shows the route info screen in simple navigation mode.
        /// </summary>
        /// <param name="startAddress"> </param>
        /// <param name="destinationAddress"> </param>
        public virtual void ShowRouteInfoScreen(string startAddress, string destinationAddress)
        {
            _currentNavigationMode = NavigationMode.RouteInfo;

            HideViewIfVisible(_topNextNavigationPanel);
            HideViewIfVisible(_topCurrentNavigationPanel);
            HideBottomAndLeftPanels();

            ((TextView)_routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_current_position_text)).Text = startAddress;
            ((TextView)_routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = destinationAddress;

            _routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Visible;
            _routeOverviewPanel.Visibility = ViewStates.Visible;
            _backButtonPanel.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Shows the route info screen from free drive mode.
        /// </summary>
        public virtual void ShowRouteInfoFreeDriveScreen()
        {
            _currentNavigationMode = NavigationMode.RouteInfo;

            HideViewIfVisible(_freeDriveCurrentStreetPanel);
            HideBottomAndLeftPanels();
            _routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_starting_position_layout).Visibility = ViewStates.Gone;

            _routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_layout).Visibility = ViewStates.Visible;

            ((TextView)_routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_label)).Text = _currentActivity.GetString(Resource.String.current_position);
            ((TextView)_routeOverviewPanel.FindViewById(Resource.Id.navigation_route_overview_destination_text)).Text = _currentStreetNameFreeDriveString;

            _routeOverviewPanel.Visibility = ViewStates.Visible;
            _backButtonPanel.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// removes the top panels
        /// </summary>
        public virtual void HideTopPanels()
        {
            HideViewIfVisible(_reRoutingPanel);
            HideViewIfVisible(_topNextNavigationPanel);
            HideViewIfVisible(_topCurrentNavigationPanel);
            HideViewIfVisible(_freeDriveCurrentStreetPanel);
            HideViewIfVisible(_navigationSimulationPanel);
            HideViewIfVisible(_viaPointPanel);
        }

        /// <summary>
        /// removes the bottom and bottom left panels
        /// </summary>
        public virtual void HideBottomAndLeftPanels()
        {
            HideViewIfVisible(_routeDistancePanel);
            HideViewIfVisible(_speedPanel);
            HideViewIfVisible(_arrivingEtaTimeGroupPanels);
        }

        /// <summary>
        /// Hide the settings menu screen.
        /// </summary>
        public virtual void HideSettingsPanel()
        {
            HideViewIfVisible(_settingsPanel);
        }

        /// <summary>
        /// Shows the rerouting panel.
        /// </summary>
        public virtual void ShowReroutingPanel()
        {
            ShowViewIfNotVisible(_reRoutingPanel);
        }

        /// <summary>
        /// Shows the exit navigation dialog.
        /// </summary>
        public virtual void ShowExitNavigationDialog()
        {
            AlertDialog.Builder alertDialog = new AlertDialog.Builder(_currentActivity);

            alertDialog.SetTitle(Resource.String.exit_navigation_dialog_title);
            alertDialog.SetMessage(_currentActivity.Resources.GetString(Resource.String.exit_navigation_dialog_message));

            alertDialog.SetPositiveButton(_currentActivity.Resources.GetString(Resource.String.ok_label), (s,e) =>
                {
                    //dialog.Cancel();
                    _isFreeDrive = false;
                    _currentNavigationMode = NavigationMode.PostNavigation;
                    SKToolsLogicManager.Instance.StopNavigation();
                });

            alertDialog.SetNegativeButton(_currentActivity.Resources.GetString(Resource.String.cancel_label), (s, e) =>
            {
                //dialog.Cancel();
            });

            alertDialog.Show();
        }

        /// <summary>
        /// Shows a dialog that notifies that the route calculation failed.
        /// </summary>
        /// <param name="statusCode"> </param>
        public virtual void ShowRouteCalculationFailedDialog(SKRouteListenerSKRoutingErrorCode statusCode)
        {
            _currentActivity.RunOnUiThread(() =>
            {
                string dialogMessage;
                Resources res = _currentActivity.Resources;

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

                AlertDialog.Builder alertDialog = new AlertDialog.Builder(_currentActivity);
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
        public virtual void Reset(SKMaps.SKDistanceUnitType distanceUnit)
        {
            _distanceUnitType = distanceUnit;
            NextVisualAdviceDistance = 0;
            RouteDistanceString = "";
            NextVisualAdviceStreetName = "";
            CurrentVisualAdviceDistance = 0;
            CurrentVisualAdviceStreetName = "";
            _estimatedTimePanelVisible = true;
            IsNextAdviceVisible = false;
            FirstAdviceReceived = false;
            TimeToDestination = 0;
            CurrentSpeedLimit = 0;
            _currentSpeed = 0;
            InitialTimeToDestination = 0;
        }

        /// <summary>
        /// Handles the navigation state update.
        /// </summary>
        /// <param name="skNavigationState"> </param>
        /// <param name="mapStyle"> </param>
        public virtual void HandleNavigationState(SKNavigationState skNavigationState, int mapStyle)
        {

            if (_currentNavigationMode == NavigationMode.Follower)
            {

                _currentActivity.RunOnUiThread(() =>
                    {
                        HideViewIfVisible(_reRoutingPanel);
                        if (_currentNavigationMode == NavigationMode.Follower)
                        {
                            ShowViewIfNotVisible(_topCurrentNavigationPanel);
                            ShowViewIfNotVisible(_routeDistancePanel);
                            ShowViewIfNotVisible(_speedPanel);
                        }

                        CurrentCountryCode = skNavigationState.CountryCode;
                        DistanceEstimatedUntilDestination = (int)Math.Round(skNavigationState.DistanceToDestination);

                        string currentVisualAdviceImage = skNavigationState.CurrentAdviceVisualAdviceFile;

                        Bitmap decodedAdvice = SKToolsUtils.DecodeFileToBitmap(currentVisualAdviceImage);
                        if (decodedAdvice != null)
                        {
                            CurrentAdviceImage.SetImageBitmap(decodedAdvice);
                            CurrentAdviceImage.Visibility = ViewStates.Visible;
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
                        ShowDestinationReachedFlag = skNavigationState.LastAdvice;

                        if (InitialTimeToDestination == 0)
                        {
                            InitialTimeToDestination = skNavigationState.CurrentAdviceTimeToDestination;
                        }

                        NextStreetType = skNavigationState.CurrentAdviceNextOsmStreetType.Value;
                        SecondNextStreetType = skNavigationState.NextAdviceNextOsmStreetType.Value;

                        int currentDistanceToAdvice = skNavigationState.CurrentAdviceDistanceToAdvice;
                        int nextDistanceToAdvice = skNavigationState.NextAdviceDistanceToAdvice;

                        // speed values
                        if (_currentSpeed == 0 || _currentSpeed != skNavigationState.CurrentSpeed)
                        {
                            _currentSpeed = skNavigationState.CurrentSpeed;
                            CurrentSpeedString = Convert.ToString(SKToolsUtils.GetSpeedByUnit(_currentSpeed, _distanceUnitType));
                            CurrentSpeedText.Text = CurrentSpeedString;
                            CurrentSpeedTextValue.Text = SKToolsUtils.GetSpeedTextByUnit(_currentActivity, _distanceUnitType);
                        }

                        if (CurrentSpeedLimit != skNavigationState.CurrentSpeedLimit)
                        {
                            CurrentSpeedLimit = skNavigationState.CurrentSpeedLimit;
                            HandleSpeedLimitAvailable(countryCode, _distanceUnitType, mapStyle);
                        }

                        if (_navigationTotalDistance == 0)
                        {
                            _navigationTotalDistance = DistanceEstimatedUntilDestination;
                        }

                        // set next advice content & visibility
                        if (nextVisualAdviceFile != null)
                        {
                            if (NextVisualAdviceDistance != nextDistanceToAdvice)
                            {
                                NextVisualAdviceDistance = nextDistanceToAdvice;
                                NextAdviceDistanceTextView.Text = SKNavigationManager.Instance.FormatDistance(nextDistanceToAdvice);
                            }
                            if (NextVisualAdviceStreetName != null && !NextVisualAdviceStreetName.Equals(nextAdviceNextStreetName))
                            {
                                NextVisualAdviceStreetName = nextAdviceNextStreetName;
                                _nextAdviceStreetNameTextView.Text = nextAdviceNextStreetName;
                            }

                            Bitmap adviceFile = SKToolsUtils.DecodeFileToBitmap(nextVisualAdviceFile);
                            if (adviceFile != null)
                            {
                                _nextAdviceImageView.SetImageBitmap(adviceFile);
                                _nextAdviceImageView.Visibility = ViewStates.Visible;
                            }

                            SetNextAdviceStreetNameVisibility();
                        }

                        if (_currentNavigationMode == NavigationMode.Follower && FirstAdviceReceived)
                        {

                            if (!IsNextAdviceVisible)
                            {
                                if (nextVisualAdviceFile != null)
                                {
                                    IsNextAdviceVisible = true;
                                    ShowNextAdvice();
                                }
                            }
                            else
                            {
                                if (nextVisualAdviceFile == null)
                                {
                                    IsNextAdviceVisible = false;
                                    _topNextNavigationPanel.Visibility = ViewStates.Gone;
                                }
                            }
                        }

                        // set current advice content
                        if (CurrentAdviceDistance != null && CurrentVisualAdviceDistance != currentDistanceToAdvice)
                        {
                            CurrentVisualAdviceDistance = currentDistanceToAdvice;
                            CurrentAdviceDistance.Text = SKNavigationManager.Instance.FormatDistance(currentDistanceToAdvice);
                        }
                        if (CurrentAdviceName != null && !ShowDestinationReachedFlag)
                        {
                            if (exitNumber != null && exitNumber.Length > 0)
                            {
                                string currentAdvice = _currentActivity.Resources.GetString(Resource.String.exit_highway_advice_label) + " " + exitNumber;
                                if (nextStreetName != null && nextStreetName.Length > 0)
                                {
                                    currentAdvice = currentAdvice + " " + nextStreetName;
                                }
                                CurrentAdviceName.Text = currentAdvice;
                                CurrentVisualAdviceStreetName = currentAdvice;
                            }
                            else
                            {
                                if (CurrentVisualAdviceStreetName != null && !CurrentVisualAdviceStreetName.Equals(nextStreetName))
                                {
                                    CurrentVisualAdviceStreetName = nextStreetName;
                                    CurrentAdviceName.Text = nextStreetName;
                                }
                            }
                        }

                        if (ShowDestinationReachedFlag)
                        {
                            if (CurrentAdviceImage != null)
                            {
                                CurrentAdviceImage.SetImageResource(Resource.Drawable.ic_destination_advise_black);
                            }
                            if (CurrentAdviceName != null)
                            {
                                CurrentVisualAdviceStreetName = _currentActivity.Resources.GetString(Resource.String.destination_reached_info_text);
                                CurrentAdviceName.Text = CurrentVisualAdviceStreetName;
                            }
                            if (CurrentAdviceDistance != null)
                            {
                                CurrentVisualAdviceDistance = 0;
                                CurrentAdviceDistance.Visibility = ViewStates.Gone;
                            }
                            DisableNextAdvice();
                        }


                        // set estimated/arriving time
                        if ((TimeToDestination < 120) || (TimeToDestination - 60 >= skNavigationState.CurrentAdviceTimeToDestination) || (TimeToDestination + 60 < skNavigationState.CurrentAdviceTimeToDestination))
                        {

                            TimeToDestination = skNavigationState.CurrentAdviceTimeToDestination;
                            if (_estimatedTimePanelVisible)
                            {
                                ShowEstimatedTime();
                            }
                            else
                            {
                                ShowArrivingTime();
                            }
                        }

                        string[] distanceToDestinationSplit = SKNavigationManager.Instance.FormatDistance(DistanceEstimatedUntilDestination).Split(' ');
                        if (!RouteDistanceString.Equals(distanceToDestinationSplit[0]))
                        {
                            RouteDistanceString = distanceToDestinationSplit[0];
                            if (distanceToDestinationSplit.Length > 1)
                            {
                                RouteDistanceValueString = distanceToDestinationSplit[1];
                            }
                            SetRouteDistanceFields();
                        }

                        // when we receive the first advice we show the panels that were set accordingly
                        if (!FirstAdviceReceived)
                        {
                            FirstAdviceReceived = true;

                            if (_currentNavigationMode == NavigationMode.Follower)
                            {
                                ShowViewIfNotVisible(_topCurrentNavigationPanel);

                                if (nextVisualAdviceFile != null)
                                {
                                    IsNextAdviceVisible = true;
                                    ShowNextAdvice();
                                }
                                else
                                {
                                    IsNextAdviceVisible = false;
                                }
                                if (CurrentAdviceDistance != null && !ShowDestinationReachedFlag)
                                {
                                    CurrentAdviceDistance.Visibility = ViewStates.Visible;
                                }
                                if (!_firstTimeNavigation)
                                {
                                    _topCurrentNavigationPanel.BringToFront();
                                }

                                ShowViewIfNotVisible(_routeDistancePanel);
                                ShowViewIfNotVisible(_arrivingEtaTimeGroupPanels);
                            }
                        }
                    });
            }

        }

        /// <summary>
        /// sets top panels background colour
        /// </summary>
        public virtual void SetTopPanelsBackgroundColour(int mapStyle, bool currentAdviceChanged, bool nextAdviceChanged)
        {
            if (SignPostsCountryExceptions.Contains(CurrentCountryCode))
            {
                _isDefaultTopPanelBackgroundColor = false;
                if (currentAdviceChanged || !nextAdviceChanged)
                {
                    VerifyStreetType(mapStyle, NextStreetType, true);
                }
                if (NextStreetType == SecondNextStreetType)
                {
                    if (nextAdviceChanged && IsNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                    {
                        VerifyStreetType(mapStyle, 0, false);
                    }
                }
                else
                {
                    if (nextAdviceChanged && IsNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                    {
                        VerifyStreetType(mapStyle, SecondNextStreetType, false);
                    }
                }
            }
            else
            {
                if (!_isDefaultTopPanelBackgroundColor)
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        if (currentAdviceChanged || !nextAdviceChanged)
                        {
                            SetTopPanelsStyle(Resource.Color.white, Resource.Color.black, true);
                        }
                        if (nextAdviceChanged && IsNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            SetTopPanelsStyle(Resource.Color.white, Resource.Color.black, false);
                        }

                    }
                    else
                    {
                        if (currentAdviceChanged || !nextAdviceChanged)
                        {
                            SetTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, true);
                        }
                        if (nextAdviceChanged && IsNextAdviceVisible || !currentAdviceChanged && !nextAdviceChanged)
                        {
                            SetTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, false);
                        }
                    }
                }
                _isDefaultTopPanelBackgroundColor = true;
            }
        }


        /// <summary>
        /// verifies the street type and sets the colors for top panels
        /// </summary>
        /// <param name="mapStyle"> </param>
        /// <param name="streetType"> </param>
        /// <param name="forCurrent"> </param>
        private void VerifyStreetType(int mapStyle, int streetType, bool forCurrent)
        {
            if (streetType == OsmStreetTypeMotorway || streetType == OsmStreetTypeMotorwayLink)
            {
                if ((CurrentCountryCode.Equals("CH")) || (CurrentCountryCode.Equals("US")))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.blue_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.blue_panel_night_background, Resource.Color.blue_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OsmStreetTypePrimary || streetType == OsmStreetTypePrimaryLink)
            {
                if (CurrentCountryCode.Equals("CH"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.blue_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.blue_panel_night_background, Resource.Color.blue_panel_night_text, forCurrent);
                    }
                }
                else if (CurrentCountryCode.Equals("US"))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.yellow_panel_day_background, Resource.Color.black, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.yellow_panel_night_background, Resource.Color.yellow_panel_night_text, forCurrent);
                    }
                }
            }
            else if (streetType == OsmStreetTypeTrunk || streetType == OsmStreetTypeTrunkLink)
            {
                if ((CurrentCountryCode.Equals("GB")) || (CurrentCountryCode.Equals("US")))
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_day_background, Resource.Color.white, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.green_panel_night_background, Resource.Color.green_panel_night_text, forCurrent);
                    }
                }
                else
                {
                    if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                    {
                        SetTopPanelsStyle(Resource.Color.white, Resource.Color.black, forCurrent);
                    }
                    else
                    {
                        SetTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, forCurrent);
                    }
                }
            }
            else
            {
                if (mapStyle == SKToolsMapOperationsManager.DayStyle)
                {
                    SetTopPanelsStyle(Resource.Color.white, Resource.Color.black, forCurrent);
                }
                else
                {
                    SetTopPanelsStyle(Resource.Color.navigation_style_night, Resource.Color.gray, forCurrent);
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
        protected internal virtual void SetTopPanelsStyle(int drawableId, int textColor, bool forCurrentAdvice)
        {
            if (!_isFreeDrive)
            {
                if (forCurrentAdvice)
                {
                    if (_topCurrentNavigationDistanceStreetPanel != null)
                    {
                        _topCurrentNavigationDistanceStreetPanel.SetBackgroundColor(_currentActivity.Resources.GetColor(drawableId));
                    }
                    if (_topCurrentNavigationPanel != null)
                    {
                        RelativeLayout topCurrentNavigationImagePanel = (RelativeLayout)_topCurrentNavigationPanel.FindViewById(Resource.Id.current_advice_image_holder);
                        if (topCurrentNavigationImagePanel != null)
                        {
                            topCurrentNavigationImagePanel.SetBackgroundColor(_currentActivity.Resources.GetColor(drawableId));
                        }
                    }
                    if (CurrentAdviceDistance != null)
                    {
                        CurrentAdviceDistance.SetTextColor(_currentActivity.Resources.GetColor(textColor));
                    }
                    if (CurrentAdviceName != null)
                    {
                        CurrentAdviceName.SetTextColor(_currentActivity.Resources.GetColor(textColor));
                    }
                    _currentAdviceBackgroundDrawableId = drawableId;
                }
                else
                {
                    if (_nextAdviceImageDistancePanel != null)
                    {
                        _nextAdviceImageDistancePanel.SetBackgroundColor(_currentActivity.Resources.GetColor(drawableId));
                    }
                    if (_nextAdviceStreetNamePanel != null)
                    {
                        _nextAdviceStreetNamePanel.SetBackgroundColor(_currentActivity.Resources.GetColor(drawableId));
                    }
                    if (NextAdviceDistanceTextView != null)
                    {
                        NextAdviceDistanceTextView.SetTextColor(_currentActivity.Resources.GetColor(textColor));
                    }
                    if (_nextAdviceStreetNameTextView != null)
                    {
                        _nextAdviceStreetNameTextView.SetTextColor(_currentActivity.Resources.GetColor(textColor));
                    }
                    _nextAdviceBackgroundDrawableId = drawableId;
                    SetNextAdviceOverlayVisibility();
                }
            }
            else
            {
                TextView freeDriveCurrentStreetText = (TextView)_freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
                if (freeDriveCurrentStreetText != null)
                {
                    freeDriveCurrentStreetText.SetBackgroundColor(_currentActivity.Resources.GetColor(drawableId));
                    freeDriveCurrentStreetText.SetTextColor(_currentActivity.Resources.GetColor(textColor));
                }
                _currentAdviceBackgroundDrawableId = drawableId;
            }
        }


        /// <summary>
        /// sets the advice overlay semi transparent visibility
        /// </summary>
        public virtual void SetNextAdviceOverlayVisibility()
        {
            if (_currentAdviceBackgroundDrawableId == _nextAdviceBackgroundDrawableId)
            {
                _topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_image_distance_overlay_background).Visibility = ViewStates.Visible;
                if (NextVisualAdviceStreetName != null)
                {
                    _topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Visible;
                }
                else
                {
                    _topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Gone;
                }
            }
            else
            {
                _topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_image_distance_overlay_background).Visibility = ViewStates.Gone;
                _topNextNavigationPanel.FindViewById(Resource.Id.navigation_next_advice_street_overlay_background).Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// switch from estimated time to the arrived time
        /// </summary>
        public virtual void SwitchEstimatedTime()
        {
            _currentActivity.RunOnUiThread(() =>
            {
                if (_estimatedTimePanelVisible)
                {
                    ShowArrivingTime();
                }
                else
                {
                    ShowEstimatedTime();
                }
                _estimatedTimePanelVisible = !_estimatedTimePanelVisible;
            });
        }

        /// <summary>
        /// shows estimated time
        /// </summary>
        public virtual void ShowEstimatedTime()
        {
            HideViewIfVisible(_arrivingTimePanel);
            if (_estimatedTimeText != null)
            {
                _estimatedTimeText.Text = SKToolsUtils.FormatTime(TimeToDestination);
            }
            ShowViewIfNotVisible(_estimatedTimePanel);
        }

        /// <summary>
        /// calculates and shows the arriving time
        /// </summary>
        public virtual void ShowArrivingTime()
        {
            DateTime currentTime = new DateTime();
            DateTime arrivingTime = currentTime;
            int hours = TimeToDestination / 3600;
            int minutes = (TimeToDestination % 3600) / 60;
            arrivingTime.AddMinutes(minutes);
            TextView arrivingTimeAmpm = (TextView)_arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text_ampm);
            if (_arrivingTimeText != null)
            {
                SimpleDateFormat simpleDateFormat = new SimpleDateFormat("HH:mm");
                arrivingTime.Add(TimeSpan.FromHours(hours));
                arrivingTimeAmpm.Text = "";
                _arrivingTimeText.Text = simpleDateFormat.Format(new Date(arrivingTime.Year, arrivingTime.Month, arrivingTime.Day));
            }
            HideViewIfVisible(_estimatedTimePanel);
            ShowViewIfNotVisible(_arrivingTimePanel);
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
                return _isFreeDrive;
            }
        }

        /// <summary>
        /// Sets the free drive mode.
        /// </summary>
        public virtual void SetFreeDriveMode()
        {
            FirstAdviceReceived = false;
            _isFreeDrive = true;

            HideViewIfVisible(_routeDistancePanel);
            HideViewIfVisible(_arrivingEtaTimeGroupPanels);

            bool isLandscape = _currentActivity.Resources.Configuration.Orientation == Orientation.Landscape;
            SettingsMenuForFreeDrive = isLandscape;
            CancelSpeedExceededThread();
        }

        /// <summary>
        /// sets speed limit field and visibility
        /// </summary>
        /// <param name="countryCode"> </param>
        /// <param name="distanceUnitType"> </param>
        public virtual void HandleSpeedLimitAvailable(string countryCode, SKMaps.SKDistanceUnitType distanceUnitType, int mapStyle)
        {

            if (_speedPanel == null)
            {
                return;
            }

            TextView speedLimitText = (TextView)_speedPanel.FindViewById(Resource.Id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)_speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);

            if (countryCode != null)
            {
                _isUs = countryCode.Equals("US");
                if (_isUs)
                {
                    _isDefaultSpeedSign = false;
                    if (_speedExceededThread == null)
                    {
                        if (speedLimitImage != null)
                        {
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign_us);
                        }
                    }
                }
                else
                {
                    if (!_isDefaultSpeedSign)
                    {
                        if (speedLimitImage != null)
                        {
                            speedLimitImage.SetImageResource(Resource.Drawable.background_speed_sign);
                        }
                    }
                    _isDefaultSpeedSign = true;
                }
            }

            // speed limit visibility
            if (_currentNavigationMode == NavigationMode.Follower)
            {
                if (CurrentSpeedLimit != 0) //&& gpsIsWorking) {
                {
                    if (!_speedLimitAvailable)
                    {
                        _currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Visible;
                    }
                }
            }

            // set speed limit
            if (CurrentSpeedLimit != 0)
            {
                if (speedLimitText != null)
                {
                    speedLimitText.Text = Convert.ToString(SKToolsUtils.GetSpeedByUnit(CurrentSpeedLimit, distanceUnitType));
                    if (!SpeedLimitExceeded)
                    {
                        speedLimitText.Visibility = ViewStates.Visible;
                        speedLimitImage.Visibility = ViewStates.Visible;
                    }
                }
                if (!_speedLimitAvailable)
                {
                    CurrentSpeedPanelBackgroundAndTextColour = mapStyle;
                }
                _speedLimitAvailable = true;
            }
            else
            {
                if (_speedLimitAvailable)
                {
                    _currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Gone;

                    CurrentSpeedPanelBackgroundAndTextColour = mapStyle;

                }
                _speedLimitAvailable = false;
            }

        }

        /// <summary>
        /// cancels the thread that deals with the speed exceeded flow
        /// </summary>
        private void CancelSpeedExceededThread()
        {
            if (_speedExceededThread != null && _speedExceededThread.IsAlive)
            {
                _speedExceededThread.Cancel();
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
                RelativeLayout currentSpeedPanel = (RelativeLayout)_currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel);
                if (value == SKToolsMapOperationsManager.DayStyle)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.gray));
                    }
                    _currentActivity.FindViewById(Resource.Id.free_drive_current_speed_linear_layout).SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.gray));
                    if (CurrentSpeedText != null)
                    {
                        CurrentSpeedText.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.black));
                    }
                    if (CurrentSpeedTextValue != null)
                    {
                        CurrentSpeedTextValue.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.black));
                    }
                }
                else if (value == SKToolsMapOperationsManager.NightStyle)
                {
                    if (currentSpeedPanel != null)
                    {
                        currentSpeedPanel.SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.speed_panel_night_background));
                    }
                    _currentActivity.FindViewById(Resource.Id.free_drive_current_speed_linear_layout).SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.speed_panel_night_background));
                    if (CurrentSpeedText != null)
                    {
                        CurrentSpeedText.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.gray));
                    }
                    if (CurrentSpeedTextValue != null)
                    {
                        CurrentSpeedTextValue.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.gray));
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
        public virtual void HandleFreeDriveUpdated(string countryCode, string streetName, double currentFreeDriveSpeed, double speedLimit, SKMaps.SKDistanceUnitType distanceUnitType, int mapStyle)
        {
            _currentActivity.RunOnUiThread(() =>
            {
                if (_isFreeDrive)
                {

                    if (_currentSpeed == 0 || _currentSpeed != currentFreeDriveSpeed)
                    {
                        _currentSpeed = currentFreeDriveSpeed;
                        CurrentSpeedString = Convert.ToString(SKToolsUtils.GetSpeedByUnit(_currentSpeed, distanceUnitType));
                        CurrentSpeedText.Text = CurrentSpeedString;
                        CurrentSpeedTextValue.Text = SKToolsUtils.GetSpeedTextByUnit(_currentActivity, distanceUnitType);
                    }

                    if (CurrentSpeedLimit != speedLimit)
                    {
                        CurrentSpeedLimit = speedLimit;
                        HandleSpeedLimitAvailable(countryCode, distanceUnitType, mapStyle);
                    }

                    SetTopPanelsBackgroundColour(mapStyle, false, false);

                    if (streetName != null && !streetName.Equals(""))
                    {
                        _currentStreetNameFreeDriveString = streetName;
                        ((TextView)_freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text)).Text = streetName;
                    }
                    else
                    {
                        _currentStreetNameFreeDriveString = null;
                    }
                    if (_currentNavigationMode == NavigationMode.Follower && _currentStreetNameFreeDriveString != null)
                    {
                        _freeDriveCurrentStreetPanel.Visibility = ViewStates.Visible;
                        ShowViewIfNotVisible(_speedPanel);
                    }
                }
            });
        }

        /// <summary>
        /// Handles orientation change.
        /// </summary>
        /// <param name="mapStyle"> </param>
        /// <param name="displayMode"> </param>
        public virtual void HandleOrientationChanged(int mapStyle, SKMapSettings.SKMapDisplayMode displayMode)
        {
            if (_currentNavigationMode != NavigationMode.PreNavigation || _currentNavigationMode != NavigationMode.PostNavigation)
            {
                _currentActivity.RunOnUiThread(() =>
                {
                    if (_settingsPanel != null)
                    {
                        _rootLayout.RemoveView(_settingsPanel);
                    }
                    _rootLayout.RemoveView(_speedPanel);
                    _rootLayout.RemoveView(_routeDistancePanel);
                    _rootLayout.RemoveView(_arrivingEtaTimeGroupPanels);

                    InflateSettingsMenu();
                    InitialiseVolumeSeekBar();
                    InflateBottomPanels();

                    SetAudioViewsFromSettings();
                    SwitchMapMode(displayMode);
                    SwitchDayNightStyle(mapStyle);

                    bool isLandscape = _currentActivity.Resources.Configuration.Orientation == Orientation.Landscape;
                    if (_isFreeDrive)
                    {
                        SettingsMenuForFreeDrive = isLandscape;
                    }

                    if (_currentNavigationMode == NavigationMode.Settings)
                    {
                        ShowViewIfNotVisible(_settingsPanel);
                    }
                    else if (_currentNavigationMode == NavigationMode.Follower)
                    {
                        ShowViewIfNotVisible(_speedPanel);
                        ShowViewIfNotVisible(_routeDistancePanel);
                        ShowViewIfNotVisible(_arrivingEtaTimeGroupPanels);
                    }

                    SetAdvicesFields();
                    ChangePanelsBackgroundAndTextViewsColour(mapStyle);
                });
            }
        }

        /// <summary>
        /// Handles the speed exceeded.
        /// </summary>
        /// <param name="speedExceeded"> </param>
        public virtual void HandleSpeedExceeded(bool speedExceeded)
        {
            SpeedLimitExceeded = speedExceeded;
            if (_currentNavigationMode == NavigationMode.Follower)
            {
                ChangeSpeedSigns();
            }
        }


        /// <summary>
        /// sets the current and next advice panels dimensions
        /// </summary>
        private void SetAdvicesFields()
        {
            if (!_isFreeDrive)
            {
                SetRouteDistanceFields();
                SetEtaFields();
            }
            else
            {
                TextView freeDriveCurrentStreetText = (TextView)_freeDriveCurrentStreetPanel.FindViewById(Resource.Id.free_drive_current_street_text);
                if (freeDriveCurrentStreetText != null)
                {
                    freeDriveCurrentStreetText.Text = _currentStreetNameFreeDriveString;
                }
            }
            SetCurrentSpeedFields();
            SetSpeedLimitFields();
        }

        /// <summary>
        /// set speed limit fields
        /// </summary>
        private void SetSpeedLimitFields()
        {
            TextView speedLimitText = (TextView)_speedPanel.FindViewById(Resource.Id.speed_limit_value);
            if (_speedLimitAvailable && speedLimitText != null && CurrentSpeedLimit != 0)
            {
                speedLimitText.Text = Convert.ToString(Convert.ToString(SKToolsUtils.GetSpeedByUnit(CurrentSpeedLimit, _distanceUnitType)));
                ChangeSpeedSigns();

                _currentActivity.FindViewById(Resource.Id.navigation_free_drive_speed_limit_panel).Visibility = ViewStates.Visible;
            }
        }

        /// <summary>
        /// changes the speed exceeded sign
        /// </summary>
        private void ChangeSpeedSigns()
        {
            TextView speedLimitText = (TextView)_speedPanel.FindViewById(Resource.Id.speed_limit_value);
            ImageView speedLimitImage = (ImageView)_speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);

            if (SpeedLimitExceeded)
            {
                if (_speedExceededThread == null || !_speedExceededThread.IsAlive)
                {
                    _speedExceededThread = new SpeedExceededThread(this, true);
                    _speedExceededThread.Start();
                }
            }
            else
            {
                if (_speedExceededThread != null)
                {
                    if (speedLimitImage != null)
                    {
                        if (_isUs)
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
                    _speedExceededThread.Cancel();
                    _speedExceededThread = null;
                }
            }
        }

        /// <summary>
        /// set current speed fields
        /// </summary>
        private void SetCurrentSpeedFields()
        {
            if (CurrentSpeedText != null)
            {
                CurrentSpeedText.Text = CurrentSpeedString;
            }
            if (CurrentSpeedTextValue != null)
            {
                CurrentSpeedTextValue.Text = SKToolsUtils.GetSpeedTextByUnit(_currentActivity, _distanceUnitType);
            }
        }

        /// <summary>
        /// set eta fields
        /// </summary>
        private void SetEtaFields()
        {
            if (_estimatedTimePanelVisible)
            {
                ShowEstimatedTime();
            }
            else
            {
                ShowArrivingTime();
            }
        }


        /// <summary>
        /// sets route distance fields.
        /// </summary>
        protected internal virtual void SetRouteDistanceFields()
        {
            if (_routeDistanceText != null)
            {
                _routeDistanceText.Text = RouteDistanceString;
            }
            if (_routeDistanceTextValue != null)
            {
                _routeDistanceTextValue.Text = RouteDistanceValueString;
            }
        }

        /// <summary>
        /// sets next advice street name visibility
        /// </summary>
        private void SetNextAdviceStreetNameVisibility()
        {
            if (_topNextNavigationPanel != null && _nextAdviceStreetNameTextView != null && _nextAdviceStreetNamePanel != null)
            {
                if (NextVisualAdviceStreetName != null)
                {
                    _nextAdviceStreetNamePanel.Visibility = ViewStates.Visible;
                }
                else
                {
                    _nextAdviceStreetNamePanel.Visibility = ViewStates.Gone;
                }
            }
        }

        /// <summary>
        /// Shows the next advice.
        /// </summary>
        private void ShowNextAdvice()
        {
            _topNextNavigationPanel.Visibility = ViewStates.Visible;
            _topNextNavigationPanel.BringToFront();
        }

        /// <summary>
        /// removes the next advice
        /// </summary>
        private void DisableNextAdvice()
        {
            IsNextAdviceVisible = false;
            if (_topNextNavigationPanel != null)
            {
                _topNextNavigationPanel.Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// Shows the via point panel for 2 seconds.
        /// </summary>
        public virtual void ShowViaPointPanel()
        {
            HideViewIfVisible(_topCurrentNavigationPanel);
            HideViewIfVisible(_topNextNavigationPanel);
            ShowViewIfNotVisible(_viaPointPanel);

            TextView viaPointText = (TextView)_viaPointPanel.FindViewById(Resource.Id.via_point_text_view);
            viaPointText.Text = _currentActivity.Resources.GetString(Resource.String.via_point_reached);

            Handler handler = new Handler();
            handler.PostDelayed(() => { HideViewIfVisible(_viaPointPanel); }, 2000);

        }

        /// <summary>
        /// Initialises the volume bar.
        /// </summary>
        public virtual void InitialiseVolumeSeekBar()
        {
            int currentVolume = SKToolsAdvicePlayer.GetCurrentDeviceVolume(_currentActivity);
            int maxVolume = SKToolsAdvicePlayer.GetMaximAudioLevel(_currentActivity);
            SeekBar volumeBar = (SeekBar)_settingsPanel.FindViewById(Resource.Id.navigation_settings_volume);
            volumeBar.Max = maxVolume;
            volumeBar.Progress = currentVolume;

            volumeBar.ProgressChanged += (s, e) =>
            {
                AudioManager audioManager = (AudioManager)_currentActivity.GetSystemService(Context.AudioService);
                audioManager.SetStreamVolume(Stream.Music, e.Progress, VolumeNotificationFlags.ShowUi);
            };
        }

        /// <summary>
        /// Changes audio settings menu item panels.
        /// </summary>
        public virtual void LoadAudioSettings()
        {
            TextView audioText = ((TextView)_settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
            int? audioImageTag = (int?)audioText.Tag;
            audioImageTag = audioImageTag == null ? 0 : audioImageTag;

            Resources res = _currentActivity.Resources;
            if (audioImageTag == Resource.Drawable.ic_audio_on)
            {
                SKToolsAdvicePlayer.Instance.DisableMute();
                SKToolsLogicManager.Instance.PlayLastAdvice();
                audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_off, 0, 0);
                audioText.Tag = Resource.Drawable.ic_audio_off;
                audioText.Text = res.GetString(Resource.String.navigate_settings_audio_off);
            }
            else if (audioImageTag == 0 || audioImageTag == Resource.Drawable.ic_audio_off)
            {
                SKToolsAdvicePlayer.Instance.Stop();
                SKToolsAdvicePlayer.Instance.EnableMute();
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
        protected internal virtual void SetAudioViewsFromSettings()
        {
            if (_settingsPanel != null)
            {
                if (SKToolsAdvicePlayer.Instance.Muted)
                {
                    TextView audioText = ((TextView)_settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
                    audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_on, 0, 0);
                    audioText.Tag = Resource.Drawable.ic_audio_on;
                    audioText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_audio_on);
                }
                else
                {
                    TextView audioText = ((TextView)_settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_text));
                    audioText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_audio_off, 0, 0);
                    audioText.Tag = Resource.Drawable.ic_audio_off;
                    audioText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_audio_off);
                }
            }
        }

        /// <summary>
        /// Changes settings menu item, map style panels.
        /// </summary>
        /// <param name="mapStyle"> </param>
        public virtual void SwitchDayNightStyle(int mapStyle)
        {
            TextView dayNightText = ((TextView)_settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_text));
            if (mapStyle == SKToolsMapOperationsManager.DayStyle)
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_nightmode, 0, 0);
                dayNightText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_nightmode);
            }
            else
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_daymode, 0, 0);
                dayNightText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_daymode);
            }

            _isDefaultTopPanelBackgroundColor = false;
            ChangePanelsBackgroundAndTextViewsColour(mapStyle);
            SetTopPanelsBackgroundColour(mapStyle, false, false);
        }

        /// <summary>
        /// Changes settings menu item, map mode panels.
        /// </summary>
        /// <param name="displayMode"> </param>
        public virtual void SwitchMapMode(SKMapSettings.SKMapDisplayMode displayMode)
        {
            TextView dayNightText = ((TextView)_settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_text));
            if (displayMode == SKMapSettings.SKMapDisplayMode.Mode3d)
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_2d, 0, 0);
                dayNightText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_2d_view);
            }
            else
            {
                dayNightText.SetCompoundDrawablesWithIntrinsicBounds(0, Resource.Drawable.ic_3d, 0, 0);
                dayNightText.Text = _currentActivity.Resources.GetString(Resource.String.navigate_settings_3d_view);
            }
        }

        /// <summary>
        /// sets the background drawable for all views
        /// </summary>
        /// <param name="currentMapStyle"> </param>
        public virtual void ChangePanelsBackgroundAndTextViewsColour(int currentMapStyle)
        {

            if (_currentNavigationMode == NavigationMode.PreNavigation)
            {
                SetPanelBackgroundAndTextColour(_preNavigationPanel.FindViewById(Resource.Id.alternative_routes_layout), null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_preNavigationPanel.FindViewById(Resource.Id.start_navigation_button), null, currentMapStyle);
            }
            else
            {

                if (!_isFreeDrive)
                {
                    EtaTimeGroupPanelsBackgroundAndTextViewColour = currentMapStyle;

                    SetPanelBackgroundAndTextColour(_routeDistancePanel.FindViewById(Resource.Id.route_distance_linear_layout), _routeDistanceText, currentMapStyle);
                    SetPanelBackgroundAndTextColour(null, _routeDistanceTextValue, currentMapStyle);
                }
                else
                {
                    SetPanelBackgroundAndTextColour(_freeDriveCurrentStreetPanel, null, currentMapStyle);
                }

                SetPanelBackgroundAndTextColour(_speedPanel, null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_topCurrentNavigationPanel, null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_nextAdviceStreetNamePanel, null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_nextAdviceImageDistancePanel, null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_backButtonPanel.FindViewById(Resource.Id.navigation_top_back_button), null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_roadBlockPanel.FindViewById(Resource.Id.road_block_relative_layout), null, currentMapStyle);

                ViewGroup routeOverviewPanel = (ViewGroup)_currentActivity.FindViewById(Resource.Id.navigation_route_overview_linear_layout);
                SetPanelBackgroundAndTextColour(routeOverviewPanel, null, currentMapStyle);
                SetPanelBackgroundAndTextColour(_viaPointPanel, null, currentMapStyle);

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
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_audio_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_day_night_mode_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_overview_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_route_info_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_roadblock_info_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_panning_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_view_mode_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_quit_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_back_button), null, value);
                SetPanelBackgroundAndTextColour(_settingsPanel.FindViewById(Resource.Id.navigation_settings_seek_bar_layout), null, value);
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
                TextView arrivingTimeAmPm = ((TextView)_arrivingTimePanel.FindViewById(Resource.Id.arriving_time_text_ampm));
                TextView estimatedTimeTextValue = ((TextView)_estimatedTimePanel.FindViewById(Resource.Id.estimated_navigation_time_text_value));

                SetPanelBackgroundAndTextColour(_arrivingTimePanel, _arrivingTimeText, value);
                SetPanelBackgroundAndTextColour(null, arrivingTimeAmPm, value);
                arrivingTimeAmPm.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.bullet_gray, 0, Resource.Drawable.bullet_green, 0);

                SetPanelBackgroundAndTextColour(_estimatedTimePanel, _estimatedTimeText, value);
                SetPanelBackgroundAndTextColour(null, estimatedTimeTextValue, value);
                estimatedTimeTextValue.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.bullet_green, 0, Resource.Drawable.bullet_gray, 0);
            }
        }

        /// <summary>
        /// sets the background for top panel and the text color for the texts inside it
        /// </summary>
        private void SetPanelBackgroundAndTextColour(View panel, TextView textView, int currentMapStyle)
        { // View view,

            if (currentMapStyle == SKToolsMapOperationsManager.DayStyle)
            {
                if (panel != null)
                {
                    panel.SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.navigation_style_day));
                }
                if (textView != null)
                {
                    textView.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.white));
                }
            }
            else if (currentMapStyle == SKToolsMapOperationsManager.NightStyle)
            {
                if (panel != null)
                {
                    panel.SetBackgroundColor(_currentActivity.Resources.GetColor(Resource.Color.navigation_style_night));
                }
                if (textView != null)
                {
                    textView.SetTextColor(_currentActivity.Resources.GetColor(Resource.Color.gray));
                }
            }

        }


        /// <summary>
        /// removes views for pre navigation
        /// </summary>
        protected internal virtual void RemovePreNavigationViews()
        {
            if (_preNavigationPanel != null)
            {
                _rootLayout.RemoveView(_preNavigationPanel);
                _preNavigationPanel = null;
            }
        }

        /// <summary>
        /// remove views with different UI for portrait and landscape
        /// </summary>
        protected internal virtual void RemoveNavigationViews()
        {
            CancelSpeedExceededThread();

            if (_topCurrentNavigationPanel != null)
            {
                _rootLayout.RemoveView(_topCurrentNavigationPanel);
                _topCurrentNavigationPanel = null;
            }
            if (_topNextNavigationPanel != null)
            {
                _rootLayout.RemoveView(_topNextNavigationPanel);
                _topNextNavigationPanel = null;
            }
            if (_menuOptions != null)
            {
                _rootLayout.RemoveView(_menuOptions);
                _menuOptions = null;
            }
            if (_routeDistancePanel != null)
            {
                _rootLayout.RemoveView(_routeDistancePanel);
                _routeDistancePanel = null;
            }
            if (_speedPanel != null)
            {
                _rootLayout.RemoveView(_speedPanel);
                _speedPanel = null;
            }
            if (_arrivingEtaTimeGroupPanels != null)
            {
                _rootLayout.RemoveView(_arrivingEtaTimeGroupPanels);
                _arrivingEtaTimeGroupPanels = null;
            }
            if (_reRoutingPanel != null)
            {
                _rootLayout.RemoveView(_reRoutingPanel);
                _reRoutingPanel = null;
            }
            if (_freeDriveCurrentStreetPanel != null)
            {
                _rootLayout.RemoveView(_freeDriveCurrentStreetPanel);
                _freeDriveCurrentStreetPanel = null;
            }
            if (_settingsPanel != null)
            {
                _rootLayout.RemoveView(_settingsPanel);
                _settingsPanel = null;
            }
            if (_navigationSimulationPanel != null)
            {
                _rootLayout.RemoveView(_navigationSimulationPanel);
                _navigationSimulationPanel = null;
            }
            if (_viaPointPanel != null)
            {
                _rootLayout.RemoveView(_viaPointPanel);
                _viaPointPanel = null;
            }
            if (_routeOverviewPanel != null)
            {
                _rootLayout.RemoveView(_routeOverviewPanel);
                _routeOverviewPanel = null;
            }
            if (_roadBlockPanel != null)
            {
                _rootLayout.RemoveView(_roadBlockPanel);
                _roadBlockPanel = null;
            }
            if (_backButtonPanel != null)
            {
                _rootLayout.RemoveView(_backButtonPanel);
                _backButtonPanel = null;
            }
        }


        /// <summary>
        /// speed exceeded thread
        /// </summary>
        private class SpeedExceededThread : Thread
        {
            private readonly SKToolsNavigationUiManager _outerInstance;


            internal bool SpeedExceeded;

            public SpeedExceededThread(SKToolsNavigationUiManager outerInstance, bool speedExceeded)
            {
                _outerInstance = outerInstance;
                SpeedExceeded = speedExceeded;
            }

            public virtual void run()
            {
                ImageView speedLimitImage = (ImageView)_outerInstance._speedPanel.FindViewById(Resource.Id.navigation_speed_sign_image);
                ImageView speedAlertImage = (ImageView)_outerInstance._speedPanel.FindViewById(Resource.Id.navigation_alert_sign_image);
                TextView speedLimitText = (TextView)_outerInstance._speedPanel.FindViewById(Resource.Id.speed_limit_value);

                while (SpeedExceeded)
                {
                    _outerInstance._currentActivity.RunOnUiThread(() =>
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
                            if (_outerInstance._isUs)
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
                    _outerInstance._currentActivity.RunOnUiThread(() =>
                    {
                        if (speedLimitImage != null)
                        {
                            if (_outerInstance._isUs)
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
            public virtual void Cancel()
            {
                SpeedExceeded = false;
            }
        }

    }
}