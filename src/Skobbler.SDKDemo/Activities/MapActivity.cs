using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Speech.Tts;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Util;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Map.RealReach;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.PoiTracker;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.SDKTools.Download;
using Skobbler.Ngx.SDKTools.NavigationUI;
using Skobbler.Ngx.Search;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Database;
using Skobbler.SDKDemo.Util;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    class MapActivity : Activity, ISKMapSurfaceListener, ISKRouteListener, ISKNavigationListener, ISKRealReachListener, ISKPOITrackerListener, ISKCurrentPositionListener, ISensorEventListener, ISKMapUpdateListener, ISKToolsNavigationListener, TextToSpeech.IOnInitListener, ISKToolsDownloadListener
    {
        private const sbyte GreenPinIconId = 0;

        private const sbyte RedPinIconId = 1;

        public const sbyte ViaPointIconId = 4;

        private const string Tag = "MapActivity";

        public const int Tracks = 1;

        private enum MapOption
        {
            MapDisplay,
            MapOverlays,
            AlternativeRoutes,
            MapStyles,
            RealReach,
            Tracks,
            Annotations,
            RoutingAndNavigation,
            PoiTracking,
            HeatMap,
            MapInteraction,
            NaviUi
        }

        private enum MapAdvices
        {
            TextToSpeech,
            AudioFiles
        }


        public static SKCategories.SKPOICategory[] HeatMapCategories;

        /// <summary>
        /// Current option selected
        /// </summary>
        private MapOption _currentMapOption = MapOption.MapDisplay;

        /// <summary>
        /// Application context object
        /// </summary>
        private DemoApplication _app;

        /// <summary>
        /// Surface view for displaying the map
        /// </summary>
        private SKMapSurfaceView _mapView;

        /// <summary>
        /// Options menu
        /// </summary>
        private View _menu;

        /// <summary>
        /// View for selecting alternative routes
        /// </summary>
        private View _altRoutesView;

        /// <summary>
        /// View for selecting the map style
        /// </summary>
        private LinearLayout _mapStylesView;

        /// <summary>
        /// View for real reach time profile
        /// </summary>
        private RelativeLayout _realReachTimeLayout;

        /// <summary>
        /// View for real reach energy profile
        /// </summary>
        private RelativeLayout _realReachEnergyLayout;

        /// <summary>
        /// Buttons for selecting alternative routes
        /// </summary>
        private Button[] _altRoutesButtons;

        /// <summary>
        /// Bottom button
        /// </summary>
        private Button _bottomButton;

        /// <summary>
        /// The current position button
        /// </summary>
        private Button _positionMeButton;

        /// <summary>
        /// Custom view for adding an annotation
        /// </summary>
        private RelativeLayout _customView;

        /// <summary>
        /// The heading button
        /// </summary>
        private Button _headingButton;

        /// <summary>
        /// The map popup view
        /// </summary>
        private SKCalloutView _mapPopup;

        /// <summary>
        /// Custom callout view title
        /// </summary>
        private TextView _popupTitleView;

        /// <summary>
        /// Custom callout view description
        /// </summary>
        private TextView _popupDescriptionView;

        /// <summary>
        /// Ids for alternative routes
        /// </summary>
        private IList<int?> _routeIds = new List<int?>();

        /// <summary>
        /// Tells if a navigation is ongoing
        /// </summary>
        private bool _navigationInProgress;

        /// <summary>
        /// Tells if a navigation is ongoing
        /// </summary>
        private bool _skToolsNavigationInProgress;

        /// <summary>
        /// Tells if a route calculation is ongoing
        /// </summary>
        private bool _skToolsRouteCalculated;

        /// <summary>
        /// POIs to be detected on route
        /// </summary>
        private IDictionary<int?, SKTrackablePOI> _trackablePoIs;

        /// <summary>
        /// Trackable POIs that are currently rendered on the map
        /// </summary>
        private IDictionary<int?, SKTrackablePOI> _drawnTrackablePoIs;

        /// <summary>
        /// Tracker manager object
        /// </summary>
        private SKPOITrackerManager _poiTrackingManager;

        /// <summary>
        /// Current position provider
        /// </summary>
        private SKCurrentPositionProvider _currentPositionProvider;

        /// <summary>
        /// Current position
        /// </summary>
        private SKPosition _currentPosition;

        /// <summary>
        /// Tells if heading is currently active
        /// </summary>
        private bool _headingOn;


        /// <summary>
        /// Real reach range
        /// </summary>
        private int _realReachRange;

        /// <summary>
        /// Real reach default vehicle type
        /// </summary>
        private sbyte _realReachVehicleType = SKRealReachSettings.VehicleTypePedestrian;

        /// <summary>
        /// Pedestrian button
        /// </summary>
        private ImageButton _pedestrianButton;

        /// <summary>
        /// Bike button
        /// </summary>
        private ImageButton _bikeButton;

        /// <summary>
        /// Car button
        /// </summary>
        private ImageButton _carButton;

        /// <summary>
        /// Navigation UI layout
        /// </summary>
        private RelativeLayout _navigationUi;



        private bool _isStartPointBtnPressed, _isEndPointBtnPressed, _isViaPointSelected;

        /// <summary>
        /// The start point(long/lat) for the route.
        /// </summary>
        private SKCoordinate _startPoint;

        /// <summary>
        /// The destination(long/lat) point for the route
        /// </summary>
        private SKCoordinate _destinationPoint;

        /// <summary>
        /// The via point(long/lat) for the route
        /// </summary>
        private SKViaPoint _viaPoint;

        /// <summary>
        /// Text to speech engine
        /// </summary>
        private TextToSpeech _textToSpeechEngine;

        private SKToolsNavigationManager _navigationManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            DemoUtils.InitializeLibrary(this);
            SetContentView(Resource.Layout.activity_map);

            _app = (DemoApplication)Application;

            _currentPositionProvider = new SKCurrentPositionProvider(this);
            _currentPositionProvider.SetCurrentPositionListener(this);

            if (DemoUtils.HasGpsModule(this))
            {
                _currentPositionProvider.RequestLocationUpdates(true, false, true);
            }
            else if (DemoUtils.HasNetworkModule(this))
            {
                _currentPositionProvider.RequestLocationUpdates(false, true, true);
            }

            SKMapViewHolder mapViewGroup = (SKMapViewHolder)FindViewById(Resource.Id.view_group_map);
            _mapView = mapViewGroup.MapSurfaceView;
            _mapView.SetMapSurfaceListener(this);
            LayoutInflater inflater = (LayoutInflater)GetSystemService(LayoutInflaterService);
            _mapPopup = mapViewGroup.CalloutView;
            View view = inflater.Inflate(Resource.Layout.layout_popup, null);
            _popupTitleView = (TextView)view.FindViewById(Resource.Id.top_text);
            _popupDescriptionView = (TextView)view.FindViewById(Resource.Id.bottom_text);
            _mapPopup.SetCustomView(view);


            ApplySettingsOnMapView();
            _poiTrackingManager = new SKPOITrackerManager(this);

            _menu = FindViewById(Resource.Id.options_menu);
            _altRoutesView = FindViewById(Resource.Id.alt_routes);
            _altRoutesButtons = new[] { (Button)FindViewById(Resource.Id.alt_route_1), (Button)FindViewById(Resource.Id.alt_route_2), (Button)FindViewById(Resource.Id.alt_route_3) };

            _mapStylesView = (LinearLayout)FindViewById(Resource.Id.map_styles);
            _bottomButton = (Button)FindViewById(Resource.Id.bottom_button);
            _positionMeButton = (Button)FindViewById(Resource.Id.position_me_button);
            _headingButton = (Button)FindViewById(Resource.Id.heading_button);

            _pedestrianButton = (ImageButton)FindViewById(Resource.Id.real_reach_pedestrian_button);
            _bikeButton = (ImageButton)FindViewById(Resource.Id.real_reach_bike_button);
            _carButton = (ImageButton)FindViewById(Resource.Id.real_reach_car_button);

            SKVersioningManager.Instance.SetMapUpdateListener(this);

            TextView realReachTimeText = (TextView)FindViewById(Resource.Id.real_reach_time);
            TextView realReachEnergyText = (TextView)FindViewById(Resource.Id.real_reach_energy);

            SeekBar realReachSeekBar = (SeekBar)FindViewById(Resource.Id.real_reach_seekbar);

            realReachSeekBar.ProgressChanged += (s, e) =>
            {
                realReachTimeText.Text = _realReachRange + " min";
                ShowRealReach(_realReachVehicleType, _realReachRange);
            };

            SeekBar realReachEnergySeekBar = (SeekBar)FindViewById(Resource.Id.real_reach_energy_seekbar);
            realReachEnergySeekBar.ProgressChanged += (s, e) =>
            {
                _realReachRange = e.Progress;
                realReachEnergyText.Text = _realReachRange + "%";
                ShowRealReachEnergy(_realReachRange);
            };

            _realReachTimeLayout = (RelativeLayout)FindViewById(Resource.Id.real_reach_time_layout);
            _realReachEnergyLayout = (RelativeLayout)FindViewById(Resource.Id.real_reach_energy_layout);
            _navigationUi = (RelativeLayout)FindViewById(Resource.Id.navigation_ui_layout);

            InitializeTrackablePoIs();
        }

        /// <summary>
        /// Customize the map view
        /// </summary>
        private void ApplySettingsOnMapView()
        {
            _mapView.MapSettings.MapRotationEnabled = true;
            _mapView.MapSettings.MapZoomingEnabled = true;
            _mapView.MapSettings.MapPanningEnabled = true;
            _mapView.MapSettings.ZoomWithAnchorEnabled = true;
            _mapView.MapSettings.InertiaRotatingEnabled = true;
            _mapView.MapSettings.InertiaZoomingEnabled = true;
            _mapView.MapSettings.InertiaPanningEnabled = true;
        }

        /// <summary>
        /// Populate the collection of trackable POIs
        /// </summary>
        private void InitializeTrackablePoIs()
        {

            _trackablePoIs = new Dictionary<int?, SKTrackablePOI>();

            _trackablePoIs[64142] = new SKTrackablePOI(64142, 0, 37.735610, -122.446434, -1, "Teresita Boulevard");
            _trackablePoIs[64143] = new SKTrackablePOI(64143, 0, 37.732367, -122.442033, -1, "Congo Street");
            _trackablePoIs[64144] = new SKTrackablePOI(64144, 0, 37.732237, -122.429190, -1, "John F Foran Freeway");
            _trackablePoIs[64145] = new SKTrackablePOI(64145, 1, 37.738090, -122.401470, -1, "Revere Avenue");
            _trackablePoIs[64146] = new SKTrackablePOI(64146, 0, 37.741128, -122.398562, -1, "McKinnon Ave");
            _trackablePoIs[64147] = new SKTrackablePOI(64147, 1, 37.746154, -122.394077, -1, "Evans Ave");
            _trackablePoIs[64148] = new SKTrackablePOI(64148, 0, 37.750057, -122.392287, -1, "Cesar Chavez Street");
            _trackablePoIs[64149] = new SKTrackablePOI(64149, 1, 37.762823, -122.392957, -1, "18th Street");
            _trackablePoIs[64150] = new SKTrackablePOI(64150, 0, 37.760242, -122.392495, 180, "20th Street");
            _trackablePoIs[64151] = new SKTrackablePOI(64151, 0, 37.755157, -122.392196, 180, "23rd Street");

            _trackablePoIs[64152] = new SKTrackablePOI(64152, 0, 37.773526, -122.452706, -1, "Shrader Street");
            _trackablePoIs[64153] = new SKTrackablePOI(64153, 0, 37.786535, -122.444528, -1, "Pine Street");
            _trackablePoIs[64154] = new SKTrackablePOI(64154, 1, 37.792242, -122.424426, -1, "Franklin Street");
            _trackablePoIs[64155] = new SKTrackablePOI(64155, 0, 37.716146, -122.409480, -1, "Campbell Ave");
            _trackablePoIs[64156] = new SKTrackablePOI(64156, 0, 37.719133, -122.388280, -1, "Fitzgerald Ave");

            _drawnTrackablePoIs = new Dictionary<int?, SKTrackablePOI>();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _mapView.OnResume();

            if (_headingOn)
            {
                StartOrientationSensor();
            }

            if (_currentMapOption == MapOption.NaviUi)
            {
                ToggleButton selectStartPointBtn = (ToggleButton)FindViewById(Resource.Id.select_start_point_button);
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
                string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.KNavigationType, "1");
                if (prefNavigationType.Equals("0"))
                { // real navi
                    selectStartPointBtn.Visibility = ViewStates.Gone;
                }
                else if (prefNavigationType.Equals("1"))
                {
                    selectStartPointBtn.Visibility = ViewStates.Visible;
                }
            }

            if (_currentMapOption == MapOption.HeatMap && HeatMapCategories != null)
            {
                _mapView.ShowHeatMapsWithPoiType(HeatMapCategories);
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            _mapView.OnPause();
            if (_headingOn)
            {
                StopOrientationSensor();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _currentPositionProvider.StopLocationUpdates();
            SKMaps.Instance.DestroySKMaps();
            if (_textToSpeechEngine != null)
            {
                _textToSpeechEngine.Stop();
                _textToSpeechEngine.Shutdown();
            }
            Process.KillProcess(Process.MyPid());
        }

        public void OnSurfaceCreated()
        {
            View chessBackground = FindViewById(Resource.Id.chess_board_background);
            chessBackground.Visibility = ViewStates.Gone;
            _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
        }

        public void OnBoundingBoxImageRendered(int i)
        {

        }


        public void OnGLInitializationError(string messsage)
        {

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case Tracks:
                        if (_currentMapOption.Equals(MapOption.Tracks) && TrackElementsActivity.SelectedTrackElement != null)
                        {
                            _mapView.DrawTrackElement(TrackElementsActivity.SelectedTrackElement);
                            _mapView.FitTrackElementInView(TrackElementsActivity.SelectedTrackElement, false);

                            SKRouteManager.Instance.SetRouteListener(this);
                            SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.SelectedTrackElement, SKRouteSettings.SKRouteMode.BicycleFastest, true, true, false);
                        }
                        break;

                    default:
                        break;
                }
            }

        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Menu && !_skToolsNavigationInProgress && !_skToolsRouteCalculated)
            {
                if (keyCode == Keycode.Menu)
                {
                    if (_menu.Visibility == ViewStates.Visible)
                    {
                        _menu.Visibility = ViewStates.Gone;
                    }
                    else if (_menu.Visibility == ViewStates.Gone)
                    {
                        _menu.Visibility = ViewStates.Visible;
                        _menu.BringToFront();
                    }
                }
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        [Export("OnClick")]
        public void OnClick(View v)
        {

            switch (v.Id)
            {

                case Resource.Id.alt_route_1:
                    SelectAlternativeRoute(0);
                    break;
                case Resource.Id.alt_route_2:
                    SelectAlternativeRoute(1);
                    break;
                case Resource.Id.alt_route_3:
                    SelectAlternativeRoute(2);
                    break;
                case Resource.Id.map_style_day:
                    SelectMapStyle(new SKMapViewStyle(_app.MapResourcesDirPath + "daystyle/", "daystyle.json"));
                    break;
                case Resource.Id.map_style_night:
                    SelectMapStyle(new SKMapViewStyle(_app.MapResourcesDirPath + "nightstyle/", "nightstyle.json"));
                    break;
                case Resource.Id.map_style_outdoor:
                    SelectMapStyle(new SKMapViewStyle(_app.MapResourcesDirPath + "outdoorstyle/", "outdoorstyle.json"));
                    break;
                case Resource.Id.map_style_grayscale:
                    SelectMapStyle(new SKMapViewStyle(_app.MapResourcesDirPath + "grayscalestyle/", "grayscalestyle.json"));
                    break;
                case Resource.Id.bottom_button:
                    if (_currentMapOption == MapOption.RoutingAndNavigation || _currentMapOption == MapOption.Tracks)
                    {
                        if (_bottomButton.Text.Equals(Resources.GetString(Resource.String.calculate_route)))
                        {
                            LaunchRouteCalculation();
                        }
                        else if (_bottomButton.Text.Equals(Resources.GetString(Resource.String.start_navigation)))
                        {
                            (new AlertDialog.Builder(this))
                                .SetMessage("Choose the advice type")
                                .SetCancelable(false)
                                .SetPositiveButton("Scout audio", (s, e) =>
                                {
                                    _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                                    AdvicesAndStartNavigation = MapAdvices.AudioFiles;
                                })
                                   .SetNegativeButton("Text to speech", (s, e) =>
                                   {
                                       if (_textToSpeechEngine == null)
                                       {
                                           Toast.MakeText(this, "Initializing TTS engine", ToastLength.Long).Show();
                                           _textToSpeechEngine = new TextToSpeech(this, this);
                                       }
                                       else
                                       {
                                           _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                                           AdvicesAndStartNavigation = MapAdvices.TextToSpeech;
                                       }
                                   })
                                   .Show();
                            _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                        }
                        else if (_bottomButton.Text.Equals(Resources.GetString(Resource.String.stop_navigation)))
                        {
                            StopNavigation();
                            _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
                        }
                    }
                    break;
                case Resource.Id.position_me_button:
                    if (_headingOn)
                    {
                        Heading = false;
                    }
                    if (_currentPosition != null)
                    {
                        _mapView.CenterMapOnCurrentPositionSmooth(17, 500);
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                    }
                    break;
                case Resource.Id.heading_button:
                    if (_currentPosition != null)
                    {
                        Heading = true;
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                    }
                    break;
                case Resource.Id.real_reach_pedestrian_button:
                    _realReachVehicleType = SKRealReachSettings.VehicleTypePedestrian;
                    ShowRealReach(_realReachVehicleType, _realReachRange);
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.real_reach_bike_button:
                    _realReachVehicleType = SKRealReachSettings.VehicleTypeBicycle;
                    ShowRealReach(_realReachVehicleType, _realReachRange);
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.real_reach_car_button:
                    _realReachVehicleType = SKRealReachSettings.VehicleTypeCar;
                    ShowRealReach(_realReachVehicleType, _realReachRange);
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.exit_real_reach_time:
                    _realReachTimeLayout.Visibility = ViewStates.Gone;
                    ClearMap();
                    break;
                case Resource.Id.exit_real_reach_energy:
                    _realReachEnergyLayout.Visibility = ViewStates.Gone;
                    ClearMap();
                    break;
                case Resource.Id.navigation_ui_back_button:
                    Button backButton = (Button)FindViewById(Resource.Id.navigation_ui_back_button);
                    LinearLayout naviButtons = (LinearLayout)FindViewById(Resource.Id.navigation_ui_buttons);
                    if (backButton.Text.Equals(">"))
                    {
                        naviButtons.Visibility = ViewStates.Visible;
                        backButton.Text = "<";
                    }
                    else
                    {
                        naviButtons.Visibility = ViewStates.Gone;
                        backButton.Text = ">";
                    }
                    break;
                case Resource.Id.calculate_routes_button:
                    CalculateRouteFromSKTools();
                    break;

                case Resource.Id.settings_button:
                    Intent intent = new Intent(this, typeof(SettingsActivity));
                    StartActivity(intent);
                    break;
                case Resource.Id.start_free_drive_button:
                    StartFreeDriveFromSKTools();
                    break;
                case Resource.Id.clear_via_point_button:
                    _viaPoint = null;
                    _mapView.DeleteAnnotation(ViaPointIconId);
                    FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Gone;
                    break;
                case Resource.Id.position_me_navigation_ui_button:
                    if (_currentPosition != null)
                    {
                        _mapView.CenterMapOnCurrentPositionSmooth(15, 1000);
                        _mapView.MapSettings.OrientationIndicatorType = SKMapSurfaceView.SKOrientationIndicatorType.Default;
                        _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.no_position_available), ToastLength.Long).Show();
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnInit(OperationResult status)
        {
            if (status == OperationResult.Success)
            {
                LanguageAvailableResult result = _textToSpeechEngine.SetLanguage(Locale.English);

                if (result == LanguageAvailableResult.MissingData || result == LanguageAvailableResult.NotSupported)
                {
                    Toast.MakeText(this, "This Language is not supported", ToastLength.Long).Show();
                }
            }
            _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
            AdvicesAndStartNavigation = MapAdvices.TextToSpeech;
        }


        private void StartFreeDriveFromSKTools()
        {
            SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();

            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            string prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.KDistanceUnit, "0");
            if (prefDistanceFormat.Equals("0"))
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
            }
            else if (prefDistanceFormat.Equals("1"))
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet;
            }
            else
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesYards;
            }

            //set speed in town
            string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.KInTownSpeedWarning, "0");
            if (prefSpeedInTown.Equals("0"))
            {
                configuration.SpeedWarningThresholdInCity = 5.0;
            }
            else if (prefSpeedInTown.Equals("1"))
            {
                configuration.SpeedWarningThresholdInCity = 10.0;
            }
            else if (prefSpeedInTown.Equals("2"))
            {
                configuration.SpeedWarningThresholdInCity = 15.0;
            }
            else if (prefSpeedInTown.Equals("3"))
            {
                configuration.SpeedWarningThresholdInCity = 20.0;
            }
            //set speed out
            string prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.KOutTownSpeedWarning, "0");
            if (prefSpeedOutTown.Equals("0"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 5.0;
            }
            else if (prefSpeedOutTown.Equals("1"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 10.0;
            }
            else if (prefSpeedOutTown.Equals("2"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 15.0;
            }
            else if (prefSpeedOutTown.Equals("3"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 20.0;
            }
            bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.KAutoDayNight, true);
            if (!dayNight)
            {
                configuration.AutomaticDayNight = false;
            }
            configuration.NavigationType = SKNavigationSettings.SKNavigationType.File;
            configuration.FreeDriveNavigationFilePath = _app.MapResourcesDirPath + "logFile/Seattle.log";
            configuration.DayStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "daystyle/", "daystyle.json");
            configuration.NightStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "nightstyle/", "nightstyle.json");

            _navigationUi.Visibility = ViewStates.Gone;
            _navigationManager = new SKToolsNavigationManager(this, Resource.Id.map_layout_root);
            _navigationManager.SetNavigationListener(this);
            _navigationManager.StartFreeDriveWithConfiguration(configuration, _mapView);

        }

        private void CalculateRouteFromSKTools()
        {

            SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);

            //set navigation type
            string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.KNavigationType, "1");
            if (prefNavigationType.Equals("0"))
            {
                configuration.NavigationType = SKNavigationSettings.SKNavigationType.Real;
                if (_currentPosition == null)
                {
                    ShowNoCurrentPosDialog();
                    return;
                }
                _startPoint = new SKCoordinate(_currentPosition.Longitude, _currentPosition.Latitude);
            }
            else if (prefNavigationType.Equals("1"))
            {
                configuration.NavigationType = SKNavigationSettings.SKNavigationType.Simulation;

            }

            //set route type
            string prefRouteType = sharedPreferences.GetString(PreferenceTypes.KRouteType, "2");
            if (prefRouteType.Equals("0"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.CarShortest;
            }
            else if (prefRouteType.Equals("1"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.CarFastest;
            }
            else if (prefRouteType.Equals("2"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.Efficient;
            }
            else if (prefRouteType.Equals("3"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.Pedestrian;
            }
            else if (prefRouteType.Equals("4"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleFastest;
            }
            else if (prefRouteType.Equals("5"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleShortest;
            }
            else if (prefRouteType.Equals("6"))
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleQuietest;
            }

            //set distance format
            string prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.KDistanceUnit, "0");
            if (prefDistanceFormat.Equals("0"))
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
            }
            else if (prefDistanceFormat.Equals("1"))
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet;
            }
            else
            {
                configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesYards;
            }

            //set speed in town
            string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.KInTownSpeedWarning, "0");
            if (prefSpeedInTown.Equals("0"))
            {
                configuration.SpeedWarningThresholdInCity = 5.0;
            }
            else if (prefSpeedInTown.Equals("1"))
            {
                configuration.SpeedWarningThresholdInCity = 10.0;
            }
            else if (prefSpeedInTown.Equals("2"))
            {
                configuration.SpeedWarningThresholdInCity = 15.0;
            }
            else if (prefSpeedInTown.Equals("3"))
            {
                configuration.SpeedWarningThresholdInCity = 20.0;
            }

            //set speed out
            string prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.KOutTownSpeedWarning, "0");
            if (prefSpeedOutTown.Equals("0"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 5.0;
            }
            else if (prefSpeedOutTown.Equals("1"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 10.0;
            }
            else if (prefSpeedOutTown.Equals("2"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 15.0;
            }
            else if (prefSpeedOutTown.Equals("3"))
            {
                configuration.SpeedWarningThresholdOutsideCity = 20.0;
            }
            bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.KAutoDayNight, true);
            if (!dayNight)
            {
                configuration.AutomaticDayNight = false;
            }
            bool tollRoads = sharedPreferences.GetBoolean(PreferenceTypes.KAvoidTollRoads, false);
            if (tollRoads)
            {
                configuration.TollRoadsAvoided = true;
            }
            bool avoidFerries = sharedPreferences.GetBoolean(PreferenceTypes.KAvoidFerries, false);
            if (avoidFerries)
            {
                configuration.FerriesAvoided = true;
            }
            bool highWays = sharedPreferences.GetBoolean(PreferenceTypes.KAvoidHighways, false);
            if (highWays)
            {
                configuration.HighWaysAvoided = true;
            }
            bool freeDrive = sharedPreferences.GetBoolean(PreferenceTypes.KFreeDrive, true);
            if (!freeDrive)
            {
                configuration.ContinueFreeDriveAfterNavigationEnd = false;
            }

            _navigationUi.Visibility = ViewStates.Gone;
            configuration.StartCoordinate = _startPoint;
            configuration.DestinationCoordinate = _destinationPoint;
            IList<SKViaPoint> viaPointList = new List<SKViaPoint>();
            if (_viaPoint != null)
            {
                viaPointList.Add(_viaPoint);
                configuration.ViaPointCoordinateList = viaPointList;
            }
            configuration.DayStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "daystyle/", "daystyle.json");
            configuration.NightStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "nightstyle/", "nightstyle.json");
            _navigationManager = new SKToolsNavigationManager(this, Resource.Id.map_layout_root);
            _navigationManager.SetNavigationListener(this);

            if (configuration.StartCoordinate != null && configuration.DestinationCoordinate != null)
            {
                _navigationManager.LaunchRouteCalculation(configuration, _mapView);
            }


        }

        [Export("OnMenuOptionClick")]
        public void OnMenuOptionClick(View v)
        {
            ClearMap();
            switch (v.Id)
            {
                case Resource.Id.option_map_display:
                    _mapView.ClearHeatMapsDisplay();
                    _currentMapOption = MapOption.MapDisplay;
                    _bottomButton.Visibility = ViewStates.Gone;
                    SKRouteManager.Instance.ClearCurrentRoute();
                    break;
                case Resource.Id.option_overlays:
                    _currentMapOption = MapOption.MapOverlays;
                    DrawShapes();
                    _mapView.SetZoom(14);
                    _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
                    break;
                case Resource.Id.option_alt_routes:
                    _currentMapOption = MapOption.AlternativeRoutes;
                    _altRoutesView.Visibility = ViewStates.Visible;
                    LaunchAlternativeRouteCalculation();
                    break;
                case Resource.Id.option_map_styles:
                    _currentMapOption = MapOption.MapStyles;
                    _mapStylesView.Visibility = ViewStates.Visible;
                    SelectStyleButton();
                    break;
                case Resource.Id.option_map_creator:
                    _currentMapOption = MapOption.MapDisplay;
                    _mapView.ApplySettingsFromFile(_app.MapCreatorFilePath);
                    break;
                case Resource.Id.option_tracks:
                    _currentMapOption = MapOption.Tracks;
                    Intent intent = new Intent(this, typeof(TracksActivity));
                    StartActivityForResult(intent, Tracks);
                    break;
                case Resource.Id.option_real_reach:
                    (new AlertDialog.Builder(this))
                        .SetMessage("Choose the real reach type")
                        .SetCancelable(false)
                        .SetPositiveButton("Time profile", (s, e) =>
                        {
                            _currentMapOption = MapOption.RealReach;
                            _realReachTimeLayout.Visibility = ViewStates.Visible;
                            ShowRealReach(_realReachVehicleType, _realReachRange);
                        })
                           .SetNegativeButton("Energy profile", (s, e) =>
                           {
                               _currentMapOption = MapOption.RealReach;
                               _realReachEnergyLayout.Visibility = ViewStates.Visible;
                               ShowRealReachEnergy(_realReachRange);
                           })
                           .Show();
                    break;
                case Resource.Id.option_map_xml_and_downloads:
                    if (DemoUtils.IsInternetAvailable(this))
                    {
                        StartActivity(new Intent(this, typeof(ResourceDownloadsListActivity)));
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_internet_connection), ToastLength.Short).Show();
                    }
                    break;
                case Resource.Id.option_reverse_geocoding:
                    StartActivity(new Intent(this, typeof(ReverseGeocodingActivity)));
                    break;
                case Resource.Id.option_address_search:
                    StartActivity(new Intent(this, typeof(OfflineAddressSearchActivity)));
                    break;
                case Resource.Id.option_nearby_search:
                    StartActivity(new Intent(this, typeof(NearbySearchActivity)));
                    break;
                case Resource.Id.option_annotations:
                    _currentMapOption = MapOption.Annotations;
                    PrepareAnnotations();
                    break;
                case Resource.Id.option_category_search:
                    StartActivity(new Intent(this, typeof(CategorySearchResultsActivity)));
                    break;
                case Resource.Id.option_routing_and_navigation:
                    _currentMapOption = MapOption.RoutingAndNavigation;
                    _bottomButton.Visibility = ViewStates.Visible;
                    _bottomButton.Text = Resources.GetString(Resource.String.calculate_route);
                    break;
                case Resource.Id.option_poi_tracking:
                    _currentMapOption = MapOption.PoiTracking;
                    if (_trackablePoIs == null)
                    {
                        InitializeTrackablePoIs();
                    }
                    LaunchRouteCalculation();
                    break;
                case Resource.Id.option_heat_map:
                    _currentMapOption = MapOption.HeatMap;
                    StartActivity(new Intent(this, typeof(PoiCategoriesListActivity)));
                    break;
                case Resource.Id.option_map_updates:
                    SKVersioningManager.Instance.CheckNewVersion(3);
                    break;
                case Resource.Id.option_map_interaction:
                    _currentMapOption = MapOption.MapInteraction;
                    HandleMapInteractionOption();
                    break;
                case Resource.Id.option_navigation_ui:
                    _currentMapOption = MapOption.NaviUi;
                    InitializeNavigationUi(true);
                    break;
                default:
                    break;
            }
            if (_currentMapOption != MapOption.MapDisplay)
            {
                _positionMeButton.Visibility = ViewStates.Gone;
                _headingButton.Visibility = ViewStates.Gone;
            }
            _menu.Visibility = ViewStates.Gone;
        }

        private void InitializeNavigationUi(bool showStartingAndDestinationAnnotations)
        {
            ToggleButton selectViaPointBtn = (ToggleButton)FindViewById(Resource.Id.select_via_point_button);
            ToggleButton selectStartPointBtn = (ToggleButton)FindViewById(Resource.Id.select_start_point_button);
            ToggleButton selectEndPointBtn = (ToggleButton)FindViewById(Resource.Id.select_end_point_button);

            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.KNavigationType, "1");
            if (prefNavigationType.Equals("0"))
            { // real navi
                selectStartPointBtn.Visibility = ViewStates.Gone;
            }
            else if (prefNavigationType.Equals("1"))
            {
                selectStartPointBtn.Visibility = ViewStates.Visible;
            }

            if (showStartingAndDestinationAnnotations)
            {
                _startPoint = new SKCoordinate(13.34615707397461, 52.513086884218325);
                SKAnnotation annotation = new SKAnnotation(GreenPinIconId);
                annotation.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
                annotation.Location = _startPoint;
                _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

                _destinationPoint = new SKCoordinate(13.398685455322266, 52.50995268098114);
                annotation = new SKAnnotation(RedPinIconId);
                annotation.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
                annotation.Location = _destinationPoint;
                _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

            }
            _mapView.SetZoom(11);
            _mapView.CenterMapOnPosition(_startPoint);


            selectStartPointBtn.CheckedChange += (s, e) =>
            {
                if (e.IsChecked)
                {
                    _isStartPointBtnPressed = true;
                    _isEndPointBtnPressed = false;
                    _isViaPointSelected = false;
                    selectEndPointBtn.Checked = false;
                    selectViaPointBtn.Checked = false;
                    Toast.MakeText(this, GetString(Resource.String.long_tap_for_position), ToastLength.Long).Show();
                }
                else
                {
                    _isStartPointBtnPressed = false;
                }
            };

            selectEndPointBtn.CheckedChange += (s, e) =>
            {
                if (e.IsChecked)
                {
                    _isEndPointBtnPressed = true;
                    _isStartPointBtnPressed = false;
                    _isViaPointSelected = false;
                    selectStartPointBtn.Checked = false;
                    selectViaPointBtn.Checked = false;
                    Toast.MakeText(this, GetString(Resource.String.long_tap_for_position), ToastLength.Long).Show();
                }
                else
                {
                    _isEndPointBtnPressed = false;
                }
            };

            selectViaPointBtn.CheckedChange += (s, e) =>
            {
                if (e.IsChecked)
                {
                    _isViaPointSelected = true;
                    _isStartPointBtnPressed = false;
                    _isEndPointBtnPressed = false;
                    selectStartPointBtn.Checked = false;
                    selectEndPointBtn.Checked = false;
                    Toast.MakeText(this, GetString(Resource.String.long_tap_for_position), ToastLength.Long).Show();
                }
                else
                {
                    _isViaPointSelected = false;
                }
            };

            _navigationUi.Visibility = ViewStates.Visible;
        }

        private void ShowNoCurrentPosDialog()
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            //alert.setTitle("Really quit?");
            alert.SetMessage("There is no current position available");
            alert.SetNegativeButton("Ok", (s, e) => { });
            alert.Show();
        }

        private void HandleMapInteractionOption()
        {

            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));

            // get the annotation object
            SKAnnotation annotation1 = new SKAnnotation(10);
            // set annotation location
            annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
            // set minimum zoom level at which the annotation should be visible
            annotation1.MininumZoomLevel = 5;
            // set the annotation's type
            annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
            // render annotation on map
            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation2 = new SKAnnotation(11);
            annotation2.Location = new SKCoordinate(-122.419789, 37.775428);
            annotation2.MininumZoomLevel = 5;
            annotation2.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
            _mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);

            float density = Resources.DisplayMetrics.Density;

            TextView topText = (TextView)_mapPopup.FindViewById(Resource.Id.top_text);
            topText.Text = "Get details";

            topText.Click += (s, e) =>
            {
                StartActivity(new Intent(this, typeof(InteractionMapActivity)));
            };

            _mapPopup.FindViewById(Resource.Id.bottom_text).Visibility = ViewStates.Gone;

            _mapPopup.SetVerticalOffset(30 * density);
            _mapPopup.ShowAtLocation(annotation1.Location, true);

        }

        /// <summary>
        /// Launches a single route calculation
        /// </summary>
        private void LaunchRouteCalculation()
        {
            // get a route object and populate it with the desired properties
            SKRouteSettings route = new SKRouteSettings();
            // set start and destination points
            route.StartCoordinate = new SKCoordinate(-122.397674, 37.761278);
            route.DestinationCoordinate = new SKCoordinate(-122.448270, 37.738761);
            // set the number of routes to be calculated
            route.NoOfRoutes = 1;
            // set the route mode
            route.RouteMode = SKRouteSettings.SKRouteMode.CarFastest;
            // set whether the route should be shown on the map after it's computed
            route.RouteExposed = true;
            // set the route listener to be notified of route calculation
            // events
            SKRouteManager.Instance.SetRouteListener(this);
            // pass the route to the calculation routine
            SKRouteManager.Instance.CalculateRoute(route);
        }

        /// <summary>
        /// Launches the calculation of three alternative routes
        /// </summary>
        private void LaunchAlternativeRouteCalculation()
        {
            SKRouteSettings route = new SKRouteSettings();
            route.StartCoordinate = new SKCoordinate(-122.392284, 37.787189);
            route.DestinationCoordinate = new SKCoordinate(-122.484378, 37.856300);
            // number of alternative routes specified here
            route.NoOfRoutes = 3;
            route.RouteMode = SKRouteSettings.SKRouteMode.CarFastest;
            route.RouteExposed = true;
            SKRouteManager.Instance.SetRouteListener(this);
            SKRouteManager.Instance.CalculateRoute(route);
        }

        /// <summary>
        /// Initiate real reach time profile
        /// </summary>
        private void ShowRealReach(sbyte vehicleType, int range)
        {

            // set listener for real reach calculation events
            _mapView.SetRealReachListener(this);
            // get object that can be used to specify real reach calculation
            // properties
            SKRealReachSettings realReachSettings = new SKRealReachSettings();
            // set center position for real reach
            SKCoordinate realReachCenter = new SKCoordinate(23.593957, 46.773361);
            realReachSettings.Location = realReachCenter;
            // set measurement unit for real reach
            realReachSettings.MeasurementUnit = SKRealReachSettings.UnitSecond;
            // set the range value (in the unit previously specified)
            realReachSettings.Range = range * 60;
            // set the transport mode
            realReachSettings.TransportMode = vehicleType;
            // initiate real reach
            _mapView.DisplayRealReachWithSettings(realReachSettings);
        }

        /// <summary>
        /// The cunsumption values
        /// </summary>
        private float[] _energyConsumption = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (float)3.7395504, (float)4.4476889, (float)5.4306439, (float)6.722719, (float)8.2830299, (float)10.0275093, (float)11.8820908, (float)13.799201, (float)15.751434, (float)17.7231534, (float)19.7051378, (float)21.6916725, (float)23.679014, (float)25.6645696, (float)27.6464437, (float)29.6231796, (float)31.5936073 };

        /// <summary>
        /// Initiate real reach energy profile
        /// </summary>

        private void ShowRealReachEnergy(int range)
        {

            //set listener for real reach calculation events
            _mapView.SetRealReachListener(this);
            // get object that can be used to specify real reach calculation
            // properties
            SKRealReachSettings realReachSettings = new SKRealReachSettings();
            SKCoordinate realReachCenter = new SKCoordinate(23.593957, 46.773361);
            realReachSettings.Location = realReachCenter;
            // set measurement unit for real reach
            realReachSettings.MeasurementUnit = SKRealReachSettings.UnitMiliwattHours;
            // set consumption values
            realReachSettings.SetConsumption(_energyConsumption);
            // set the range value (in the unit previously specified)
            realReachSettings.Range = range * 100;
            // set the transport mode
            realReachSettings.TransportMode = SKRealReachSettings.VehicleTypeBicycle;
            // initiate real reach
            _mapView.DisplayRealReachWithSettings(realReachSettings);

        }

        /// <summary>
        /// Draws annotations on map
        /// </summary>
        private void PrepareAnnotations()
        {

            // Add annotation using texture ID - from the json files.
            // get the annotation object
            SKAnnotation annotation1 = new SKAnnotation(10);
            // set annotation location
            annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
            // set minimum zoom level at which the annotation should be visible
            annotation1.MininumZoomLevel = 5;
            // set the annotation's type
            annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
            // render annotation on map
            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);


            // Add an annotation using the absolute path to the image.
            SKAnnotation annotation = new SKAnnotation(13);
            annotation.Location = new SKCoordinate(-122.434516, 37.770712);
            annotation.MininumZoomLevel = 5;


            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            if (metrics.DensityDpi < DisplayMetrics.DensityHigh)
            {
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@2x.png";
                // set the size of the image in pixels
                annotation.ImageSize = 128;
            }
            else
            {
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@3x.png";
                // set the size of the image in pixels
                annotation.ImageSize = 256;

            }
            // by default the center of the image corresponds with the location .annotation.setOffset can be use to position the image around the location.
            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);


            // add an annotation with a drawable resource
            SKAnnotation annotationDrawable = new SKAnnotation(14);
            annotationDrawable.Location = new SKCoordinate(-122.437182, 37.777079);
            annotationDrawable.MininumZoomLevel = 5;


            SKAnnotationView annotationView = new SKAnnotationView();
            annotationView.DrawableResourceId = Resource.Drawable.icon_map_popup_navigate;
            // set the width and height of the image in pixels . If they are not power of 2 the actual size of the image will be the next power of 2 of max(width,height)
            annotationView.Width = 128;
            annotationView.Height = 128;
            annotationDrawable.AnnotationView = annotationView;
            _mapView.AddAnnotation(annotationDrawable, SKAnimationSettings.AnimationNone);


            // // add an annotation with a view
            SKAnnotation annotationFromView = new SKAnnotation(15);
            annotationFromView.Location = new SKCoordinate(-122.423573, 37.761349);
            annotationFromView.MininumZoomLevel = 5;
            annotationView = new SKAnnotationView();
            _customView = (RelativeLayout)((LayoutInflater)GetSystemService(LayoutInflaterService)).Inflate(Resource.Layout.layout_custom_view, null, false);
            //  If width and height of the view  are not power of 2 the actual size of the image will be the next power of 2 of max(width,height).
            annotationView.View = _customView;
            annotationFromView.AnnotationView = annotationView;
            _mapView.AddAnnotation(annotationFromView, SKAnimationSettings.AnimationNone);

            // set map zoom level
            _mapView.SetZoom(13);
            // center map on a position
            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
        }

        /// <summary>
        /// Draws shapes on map
        /// </summary>
        private void DrawShapes()
        {

            // get a polygon shape object
            SKPolygon polygon = new SKPolygon();
            polygon.Identifier = 1;
            // set the polygon's nodes
            IList<SKCoordinate> nodes = new List<SKCoordinate>();
            nodes.Add(new SKCoordinate(-122.4342, 37.7765));
            nodes.Add(new SKCoordinate(-122.4141, 37.7765));
            nodes.Add(new SKCoordinate(-122.4342, 37.7620));
            polygon.Nodes = nodes;
            // set the outline size
            polygon.OutlineSize = 3;
            // set colors used to render the polygon
            polygon.SetOutlineColor(new[] { 1f, 0f, 0f, 1f });
            polygon.SetColor(new[] { 1f, 0f, 0f, 0.2f });
            // render the polygon on the map
            _mapView.AddPolygon(polygon);

            // get a circle mask shape object
            SKCircle circleMask = new SKCircle();
            circleMask.Identifier = 2;
            // set the shape's mask scale
            circleMask.MaskedObjectScale = 1.3f;
            // set the colors
            circleMask.SetColor(new[] { 1f, 1f, 0.5f, 0.67f });
            circleMask.SetOutlineColor(new[] { 0f, 0f, 0f, 1f });
            circleMask.OutlineSize = 3;
            // set circle center and radius
            circleMask.CircleCenter = new SKCoordinate(-122.4200, 37.7665);
            circleMask.Radius = 300;
            // set outline properties
            circleMask.OutlineDottedPixelsSkip = 6;
            circleMask.OutlineDottedPixelsSolid = 10;
            // set the number of points for rendering the circle
            circleMask.NumberOfPoints = 150;
            // render the circle mask
            _mapView.AddCircle(circleMask);


            // get a polyline object
            SKPolyline polyline = new SKPolyline();
            polyline.Identifier = 3;
            // set the nodes on the polyline
            nodes = new List<SKCoordinate>();
            nodes.Add(new SKCoordinate(-122.4342, 37.7898));
            nodes.Add(new SKCoordinate(-122.4141, 37.7898));
            nodes.Add(new SKCoordinate(-122.4342, 37.7753));
            polyline.Nodes = nodes;
            // set polyline color
            polyline.SetColor(new[] { 0f, 0f, 1f, 1f });
            // set properties for the outline
            polyline.SetOutlineColor(new[] { 0f, 0f, 1f, 1f });
            polyline.OutlineSize = 4;
            polyline.OutlineDottedPixelsSolid = 3;
            polyline.OutlineDottedPixelsSkip = 3;
            _mapView.AddPolyline(polyline);
        }

        private void SelectMapStyle(SKMapViewStyle newStyle)
        {
            _mapView.MapSettings.MapStyle = newStyle;
            SelectStyleButton();
        }

        /// <summary>
        /// Selects the style button for the current map style
        /// </summary>
        private void SelectStyleButton()
        {
            for (int i = 0; i < _mapStylesView.ChildCount; i++)
            {
                _mapStylesView.GetChildAt(i).Selected = false;
            }
            SKMapViewStyle mapStyle = _mapView.MapSettings.MapStyle;
            if (mapStyle == null || mapStyle.StyleFileName.Equals("daystyle.json"))
            {
                FindViewById(Resource.Id.map_style_day).Selected = true;
            }
            else if (mapStyle.StyleFileName.Equals("nightstyle.json"))
            {
                FindViewById(Resource.Id.map_style_night).Selected = true;
            }
            else if (mapStyle.StyleFileName.Equals("outdoorstyle.json"))
            {
                FindViewById(Resource.Id.map_style_outdoor).Selected = true;
            }
            else if (mapStyle.StyleFileName.Equals("grayscalestyle.json"))
            {
                FindViewById(Resource.Id.map_style_grayscale).Selected = true;
            }
        }

        /// <summary>
        /// Clears the map
        /// </summary>
        private void ClearMap()
        {
            Heading = false;
            switch (_currentMapOption)
            {
                case MapOption.MapDisplay:
                    break;
                case MapOption.MapOverlays:
                    // clear all map overlays (shapes)
                    _mapView.ClearAllOverlays();
                    break;
                case MapOption.AlternativeRoutes:
                    HideAlternativeRoutesButtons();
                    // clear the alternative routes
                    SKRouteManager.Instance.ClearRouteAlternatives();
                    // clear the selected route
                    SKRouteManager.Instance.ClearCurrentRoute();
                    _routeIds.Clear();
                    break;
                case MapOption.MapStyles:
                    _mapStylesView.Visibility = ViewStates.Gone;
                    break;
                case MapOption.Tracks:
                    if (_navigationInProgress)
                    {
                        // stop the navigation
                        StopNavigation();
                    }
                    _bottomButton.Visibility = ViewStates.Gone;
                    if (TrackElementsActivity.SelectedTrackElement != null)
                    {
                        _mapView.ClearTrackElement(TrackElementsActivity.SelectedTrackElement);
                        SKRouteManager.Instance.ClearCurrentRoute();
                    }
                    TrackElementsActivity.SelectedTrackElement = null;
                    break;
                case MapOption.RealReach:
                    // removes real reach from the map
                    _mapView.ClearRealReachDisplay();
                    _realReachTimeLayout.Visibility = ViewStates.Gone;
                    _realReachEnergyLayout.Visibility = ViewStates.Gone;
                    break;
                case MapOption.Annotations:
                    _mapPopup.Visibility = ViewStates.Gone;
                    // removes the annotations and custom POIs currently rendered
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    goto case MapOption.RoutingAndNavigation;
                case MapOption.RoutingAndNavigation:
                    _bottomButton.Visibility = ViewStates.Gone;
                    SKRouteManager.Instance.ClearCurrentRoute();
                    if (_navigationInProgress)
                    {
                        // stop navigation if ongoing
                        StopNavigation();
                    }
                    break;
                case MapOption.PoiTracking:
                    if (_navigationInProgress)
                    {
                        // stop the navigation
                        StopNavigation();
                    }
                    SKRouteManager.Instance.ClearCurrentRoute();
                    // remove the detected POIs from the map
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    // stop the POI tracker
                    _poiTrackingManager.StopPOITracker();
                    break;
                case MapOption.HeatMap:
                    HeatMapCategories = null;
                    _mapView.ClearHeatMapsDisplay();
                    break;
                case MapOption.MapInteraction:
                    _mapPopup.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    ((TextView)FindViewById(Resource.Id.top_text)).SetOnClickListener(null);
                    ((TextView)FindViewById(Resource.Id.top_text)).Text = "Title text";
                    ((TextView)FindViewById(Resource.Id.bottom_text)).Text = "Subtitle text";
                    break;
                case MapOption.NaviUi:
                    _navigationUi.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    break;
                default:
                    break;
            }
            _currentMapOption = MapOption.MapDisplay;
            _positionMeButton.Visibility = ViewStates.Visible;
            _headingButton.Visibility = ViewStates.Visible;
        }

        private void DeselectAlternativeRoutesButtons()
        {
            foreach (Button b in _altRoutesButtons)
            {
                b.Selected = false;
            }
        }

        private void HideAlternativeRoutesButtons()
        {
            DeselectAlternativeRoutesButtons();
            _altRoutesView.Visibility = ViewStates.Gone;
            foreach (Button b in _altRoutesButtons)
            {
                b.Text = "distance\ntime";
            }
        }

        private void SelectAlternativeRoute(int routeIndex)
        {
            if (_routeIds.Count > routeIndex)
            {
                DeselectAlternativeRoutesButtons();
                _altRoutesButtons[routeIndex].Selected = true;
                SKRouteManager.Instance.ZoomToRoute(1, 1, 110, 8, 8, 8);
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(_routeIds[routeIndex].Value);
            }

        }

        /// <summary>
        /// Launches a navigation on the current route
        /// </summary>
        private void LaunchNavigation()
        {
            if (TrackElementsActivity.SelectedTrackElement != null)
            {
                _mapView.ClearTrackElement(TrackElementsActivity.SelectedTrackElement);

            }
            // get navigation settings object
            SKNavigationSettings navigationSettings = new SKNavigationSettings();
            // set the desired navigation settings
            navigationSettings.NavigationType = SKNavigationSettings.SKNavigationType.Simulation;
            navigationSettings.PositionerVerticalAlignment = -0.25f;
            navigationSettings.ShowRealGPSPositions = false;
            // get the navigation manager object
            SKNavigationManager navigationManager = SKNavigationManager.Instance;
            navigationManager.SetMapView(_mapView);
            // set listener for navigation events
            navigationManager.SetNavigationListener(this);

            // start navigating using the settings
            navigationManager.StartNavigation(navigationSettings);
            _navigationInProgress = true;
        }

        /// <summary>
        /// Setting the audio advices
        /// </summary>
        private MapAdvices AdvicesAndStartNavigation
        {
            set
            {
                SKAdvisorSettings advisorSettings = new SKAdvisorSettings();
                advisorSettings.Language = SKAdvisorSettings.SKAdvisorLanguage.LanguageEn;
                advisorSettings.AdvisorConfigPath = _app.MapResourcesDirPath + "/Advisor";
                advisorSettings.ResourcePath = _app.MapResourcesDirPath + "/Advisor/Languages";
                advisorSettings.AdvisorVoice = "en";
                switch (value)
                {
                    case MapAdvices.AudioFiles:
                        advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.AudioFiles;
                        break;
                    case MapAdvices.TextToSpeech:
                        advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.TextToSpeech;
                        break;
                }
                SKRouteManager.Instance.SetAudioAdvisorSettings(advisorSettings);
                LaunchNavigation();

            }
        }


        /// <summary>
        /// Stops the navigation
        /// </summary>
        private void StopNavigation()
        {
            _navigationInProgress = false;
            _routeIds.Clear();
            if (_textToSpeechEngine != null && !_textToSpeechEngine.IsSpeaking)
            {
                _textToSpeechEngine.Stop();
            }
            if (_currentMapOption.Equals(MapOption.Tracks) && TrackElementsActivity.SelectedTrackElement != null)
            {
                SKRouteManager.Instance.ClearCurrentRoute();
                _mapView.DrawTrackElement(TrackElementsActivity.SelectedTrackElement);
                _mapView.FitTrackElementInView(TrackElementsActivity.SelectedTrackElement, false);

                SKRouteManager.Instance.SetRouteListener(this);
                SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.SelectedTrackElement, SKRouteSettings.SKRouteMode.BicycleFastest, true, true, false);
            }
            SKNavigationManager.Instance.StopNavigation();

        }

        // route computation callbacks ...
        public void OnAllRoutesCompleted()
        {

            SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);
            if (_currentMapOption == MapOption.PoiTracking)
            {
                // start the POI tracker
                _poiTrackingManager.StartPOITrackerWithRadius(10000, 0.5);
                // set warning rules for trackable POIs
                _poiTrackingManager.AddWarningRulesforPoiType(SKTrackablePOIType.Speedcam);
                // launch navigation
                LaunchNavigation();
            }
        }


        public void OnReceivedPOIs(SKTrackablePOIType type, IList<SKDetectedPOI> detectedPois)
        {
            UpdateMapWithLatestDetectedPoIs(detectedPois);
        }

        /// <summary>
        /// Updates the map when trackable POIs are detected such that only the
        /// currently detected POIs are rendered on the map
        /// </summary>
        /// <param name="detectedPois"> </param>
        private void UpdateMapWithLatestDetectedPoIs(IList<SKDetectedPOI> detectedPois)
        {

            IList<int?> detectedIdsList = new List<int?>();
            foreach (SKDetectedPOI detectedPoi in detectedPois)
            {
                detectedIdsList.Add(detectedPoi.PoiID);
            }
            foreach (int detectedPoiId in detectedIdsList)
            {
                if (detectedPoiId == -1)
                {
                    continue;
                }
                if (!_drawnTrackablePoIs.ContainsKey(detectedPoiId))
                {
                    _drawnTrackablePoIs.Add(detectedPoiId, _trackablePoIs[detectedPoiId]);
                    DrawDetectedPoi(detectedPoiId);
                }
            }
            foreach (int drawnPoiId in new List<int?>(_drawnTrackablePoIs.Keys))
            {
                if (!detectedIdsList.Contains(drawnPoiId))
                {
                    _drawnTrackablePoIs.Remove(drawnPoiId);
                    _mapView.DeleteAnnotation(drawnPoiId);
                }
            }
        }

        /// <summary>
        /// Draws a detected trackable POI as an annotation on the map
        /// </summary>
        /// <param name="poiId"> </param>
        private void DrawDetectedPoi(int poiId)
        {
            SKAnnotation annotation = new SKAnnotation(poiId);
            SKTrackablePOI poi = _trackablePoIs[poiId];
            annotation.Location = poi.Coordinate;
            annotation.MininumZoomLevel = 5;
            annotation.AnnotationType = SKAnnotation.SkAnnotationTypeMarker;
            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
        }

        public void OnUpdatePOIsInRadius(double latitude, double longitude, int radius)
        {

            // set the POIs to be tracked by the POI tracker
            _poiTrackingManager.SetTrackedPOIs(SKTrackablePOIType.Speedcam, new List<SKTrackablePOI>(_trackablePoIs.Values));
        }

        public void OnSensorChanged(SensorEvent e)
        {
            _mapView.ReportNewHeading(e.Values[0]);
        }

        /// <summary>
        /// Enables/disables heading mode
        /// </summary>
        /// <param name="enabled"> </param>
        private bool Heading
        {
            set
            {
                if (value)
                {
                    _headingOn = true;
                    _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.PositionPlusHeading;
                    StartOrientationSensor();
                }
                else
                {
                    _headingOn = false;
                    _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
                    StopOrientationSensor();
                }
            }
        }

        /// <summary>
        /// Activates the orientation sensor
        /// </summary>
        private void StartOrientationSensor()
        {
            SensorManager sensorManager = (SensorManager)GetSystemService(SensorService);
            Sensor orientationSensor = sensorManager.GetDefaultSensor(SensorType.Orientation);
            sensorManager.RegisterListener(this, orientationSensor, SensorDelay.Ui);
        }

        /// <summary>
        /// Deactivates the orientation sensor
        /// </summary>
        private void StopOrientationSensor()
        {
            SensorManager sensorManager = (SensorManager)GetSystemService(SensorService);
            sensorManager.UnregisterListener(this);
        }

        public void OnCurrentPositionUpdate(SKPosition currentPosition)
        {
            _currentPosition = currentPosition;
            _mapView.ReportNewGPSPosition(_currentPosition);
        }

        public void OnOnlineRouteComputationHanging(int status)
        {

        }


        // map interaction callbacks ...
        public void OnActionPan()
        {
            if (_headingOn)
            {
                Heading = false;
            }
        }

        public void OnActionZoom()
        {

        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            if (_navigationManager != null && _skToolsNavigationInProgress)
            {
                _navigationManager.NotifyOrientationChanged();
            }
        }

        public void OnAnnotationSelected(SKAnnotation annotation)
        {
            DisplayMetrics metrics = new DisplayMetrics();
            float density = Resources.DisplayMetrics.Density;
            if (_navigationUi.Visibility == ViewStates.Visible)
            {
                return;
            }
            // show the popup at the proper position when selecting an
            // annotation
            switch (annotation.UniqueID)
            {
                case 10:
                    if (density <= 1)
                    {
                        SKLogging.WriteLog(Tag, "Density 1 ", SKLogging.LogError);
                        _mapPopup.SetVerticalOffset(48 / density);
                    }
                    else if (density <= 2)
                    {
                        SKLogging.WriteLog(Tag, "Density 2 ", SKLogging.LogError);
                        _mapPopup.SetVerticalOffset(96 / density);

                    }
                    else
                    {
                        SKLogging.WriteLog(Tag, "Density 3 ", SKLogging.LogError);
                        _mapPopup.SetVerticalOffset(192 / density);
                    }
                    _popupTitleView.Text = "Annotation using texture ID";
                    _popupDescriptionView.Text = " Red pin ";
                    break;
                case 13:
                    // because the location of the annotation is the center of the image the vertical offset has to be imageSize/2
                    _mapPopup.SetVerticalOffset(annotation.ImageSize / 2 / density);
                    _popupTitleView.Text = "Annotation using absolute \n image path";
                    _popupDescriptionView.Text = null;
                    break;
                case 14:
                    int properSize = CalculateProperSizeForView(annotation.AnnotationView.Width, annotation.AnnotationView.Height);
                    // If  imageWidth and imageHeight for the annotationView  are not power of 2 the actual size of the image will be the next power of 2 of max(width,
                    // height) so the vertical offset
                    // for the callout has to be half of the annotation's size
                    _mapPopup.SetVerticalOffset(properSize / 2 / density);
                    _popupTitleView.Text = "Annotation using  \n drawable resource ID ";
                    _popupDescriptionView.Text = null;
                    break;
                case 15:
                    properSize = CalculateProperSizeForView(_customView.Width, _customView.Height);
                    // If  width and height of the view  are not power of 2 the actual size of the image will be the next power of 2 of max(width,height) so the vertical offset
                    // for the callout has to be half of the annotation's size
                    _mapPopup.SetVerticalOffset(properSize / 2 / density);
                    _popupTitleView.Text = "Annotation using custom view";
                    _popupDescriptionView.Text = null;
                    break;

            }
            _mapPopup.ShowAtLocation(annotation.Location, true);
        }


        private int CalculateProperSizeForView(int width, int height)
        {
            int maxDimension = Math.Max(width, height);
            int power = 2;

            while (maxDimension > power)
            {
                power *= 2;
            }

            return power;

        }

        public void OnCustomPOISelected(SKMapCustomPOI skMapCustomPoi)
        {

        }


        public void OnDoubleTap(SKScreenPoint point)
        {
            // zoom in on a position when double tapping
            _mapView.ZoomInAt(point);
        }

        public void OnInternetConnectionNeeded()
        {

        }

        public void OnLongPress(SKScreenPoint point)
        {
            SKCoordinate poiCoordinates = _mapView.PointToCoordinate(point);
            SKSearchResult place = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(poiCoordinates);

            bool selectPoint = _isStartPointBtnPressed || _isEndPointBtnPressed || _isViaPointSelected;
            if (poiCoordinates != null && place != null && selectPoint)
            {
                SKAnnotation annotation = new SKAnnotation(GreenPinIconId);
                if (_isStartPointBtnPressed)
                {
                    annotation.UniqueID = GreenPinIconId;
                    annotation.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
                    _startPoint = place.Location;
                }
                else if (_isEndPointBtnPressed)
                {
                    annotation.UniqueID = RedPinIconId;
                    annotation.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
                    _destinationPoint = place.Location;
                }
                else if (_isViaPointSelected)
                {
                    annotation.UniqueID = ViaPointIconId;
                    annotation.AnnotationType = SKAnnotation.SkAnnotationTypeMarker;
                    _viaPoint = new SKViaPoint(ViaPointIconId, place.Location);
                    FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Visible;
                }

                annotation.Location = place.Location;
                annotation.MininumZoomLevel = 5;
                _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
            }

        }

        public void OnMapActionDown(SKScreenPoint point)
        {

        }

        public void OnMapActionUp(SKScreenPoint point)
        {

        }

        public void OnMapPOISelected(SKMapPOI skMapPoi)
        {

        }

        public void OnMapRegionChanged(SKCoordinateRegion mapRegion)
        {
        }

        public void OnRotateMap()
        {

        }

        public void OnScreenOrientationChanged()
        {

        }

        public void OnSingleTap(SKScreenPoint point)
        {
            _mapPopup.Visibility = ViewStates.Gone;
        }


        public void OnCompassSelected()
        {

        }

        public void OnInternationalisationCalled(int result)
        {

        }

        public void OnDestinationReached()
        {
            Toast.MakeText(this, "Destination reached", ToastLength.Short).Show();
            // clear the map when reaching destination
            ClearMap();
        }


        public void OnFreeDriveUpdated(string countryCode, string streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
        {

        }

        public void OnReRoutingStarted()
        {

        }

        public void OnSpeedExceededWithAudioFiles(string[] adviceList, bool speedExceeded)
        {

        }

        public void OnUpdateNavigationState(SKNavigationState navigationState)
        {
        }


        public void OnVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState navigationState)
        {
        }

        public void OnRealReachCalculationCompleted(SKBoundingBox bbox)
        {
            // fit the reachable area on the screen when real reach calculataion
            // ends
            _mapView.FitRealReachInView(bbox, false, 0);
        }


        public void OnPOIClusterSelected(SKPOICluster poiCluster)
        {
            // TODO Auto-generated method stub

        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // TODO Auto-generated method stub

        }

        public void OnTunnelEvent(bool tunnelEntered)
        {
            // TODO Auto-generated method stub

        }

        public void OnMapRegionChangeEnded(SKCoordinateRegion mapRegion)
        {
            // TODO Auto-generated method stub

        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion mapRegion)
        {
            // TODO Auto-generated method stub

        }

        public void OnMapVersionSet(int newVersion)
        {
            // TODO Auto-generated method stub

        }

        public void OnNewVersionDetected(int newVersion)
        {
            AlertDialog alertDialog = new AlertDialog.Builder(this).Create();
            alertDialog.SetMessage("New map version available");
            alertDialog.SetCancelable(true);

            alertDialog.SetButton(GetString(Resource.String.update_label), (s, e) =>
            {
                SKVersioningManager manager = SKVersioningManager.Instance;
                bool updated = manager.UpdateMapsVersion(newVersion);
                if (updated)
                {
                    Toast.MakeText(this, "The map has been updated to version " + newVersion, ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "An error occurred in updating the map ", ToastLength.Short).Show();
                }
            });

            alertDialog.SetButton(GetString(Resource.String.cancel_label), (s, e) =>
            {
                alertDialog.Cancel();
            });

            alertDialog.Show();
        }

        public void OnNoNewVersionDetected()
        {
            Toast.MakeText(this, "No new versions were detected", ToastLength.Short).Show();
        }

        public void OnVersionFileDownloadTimeout()
        {
            // TODO Auto-generated method stub

        }

        public void OnCurrentPositionSelected()
        {
            // TODO Auto-generated method stub

        }

        public void OnObjectSelected(int id)
        {
        }

        public override void OnBackPressed()
        {
            // TODO Auto-generated method stub
            if (_menu.Visibility == ViewStates.Visible)
            {
                _menu.Visibility = ViewStates.Gone;
            }
            else if (_skToolsNavigationInProgress || _skToolsRouteCalculated)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Really quit?");
                alert.SetMessage("Do you want to exit navigation?");

                alert.SetPositiveButton("Yes", (s, e) =>
                {
                    if (_skToolsNavigationInProgress)
                    {
                        _navigationManager.StopNavigation();
                    }
                    else
                    {
                        _navigationManager.RemoveRouteCalculationScreen();
                    }
                    InitializeNavigationUi(false);
                    _skToolsRouteCalculated = false;
                    _skToolsNavigationInProgress = false;
                });

                alert.SetNegativeButton("Cancel", (s, e) => { });
                alert.Show();
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Really quit? ");
                alert.SetMessage("Do you really want to exit the app?");

                alert.SetPositiveButton("Yes", (s, e) =>
                {
                    if (ResourceDownloadsListActivity.MapsDao != null)
                    {
                        SKToolsDownloadManager downloadManager = SKToolsDownloadManager.GetInstance(this);
                        if (downloadManager.DownloadProcessRunning)
                        {
                            // pause downloads when exiting app if one is currently in progress
                            downloadManager.PauseDownloadThread();
                            return;
                        }
                    }
                    Finish();
                });

                alert.SetNegativeButton("Cancel", (s, e) => { });
                alert.Show();
            }
        }

        public void OnRouteCalculationCompleted()
        {
            
        }

        public void OnRouteCalculationFailed(SKRouteListenerSKRoutingErrorCode arg0)
        {
            Toast.MakeText(this, Resources.GetString(Resource.String.route_calculation_failed), ToastLength.Short).Show();
        }

        public void OnSignalNewAdviceWithAudioFiles(string[] audioFiles, bool specialSoundFile)
        {
            // a new navigation advice was received
            SKLogging.WriteLog(Tag, " onSignalNewAdviceWithAudioFiles " + audioFiles, SKLogging.LogDebug);
            SKToolsAdvicePlayer.Instance.PlayAdvice(audioFiles, SKToolsAdvicePlayer.PriorityNavigation);
        }

        public void OnSignalNewAdviceWithInstruction(string instruction)
        {
            SKLogging.WriteLog(Tag, " onSignalNewAdviceWithInstruction " + instruction, SKLogging.LogDebug);
            _textToSpeechEngine.Speak(instruction, QueueMode.Add, null);
        }

        public void OnSpeedExceededWithInstruction(string instruction, bool speedExceeded)
        {
        }

        public void OnServerLikeRouteCalculationCompleted(SKRouteJsonAnswer arg0)
        {
            // TODO Auto-generated method stub

        }

        public void OnViaPointReached(int index)
        {
        }

        public void OnNavigationStarted()
        {
            _skToolsNavigationInProgress = true;
            if (_navigationUi.Visibility == ViewStates.Visible)
            {
                _navigationUi.Visibility = ViewStates.Gone;
            }
        }

        public void OnNavigationEnded()
        {
            _skToolsRouteCalculated = false;
            _skToolsNavigationInProgress = false;
            InitializeNavigationUi(false);
        }

        public void OnRouteCalculationStarted()
        {
            _skToolsRouteCalculated = true;
        }

        public void onRouteCalculationCompleted()
        {

        }


        public void OnRouteCalculationCanceled()
        {
            _skToolsRouteCalculated = false;
            _skToolsNavigationInProgress = false;
            InitializeNavigationUi(false);
        }

        public void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem)
        {
            throw new NotImplementedException();
        }

        public void OnDownloadCancelled(string currentDownloadItemCode)
        {
            throw new NotImplementedException();
        }

        public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
        {
            MapDownloadResource mapResource = ResourceDownloadsListActivity.AllMapResources[currentDownloadItem.ItemCode];
            mapResource.DownloadState = currentDownloadItem.DownloadState;
            mapResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
            ResourceDownloadsListActivity.MapsDao.UpdateMapResource(mapResource);
            _app.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
            Finish();
        }

        public void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
        {
            throw new NotImplementedException();
        }

        public void OnAllDownloadsCancelled()
        {
            throw new NotImplementedException();
        }

        public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
        {
            throw new NotImplementedException();
        }

        public void OnInstallStarted(SKToolsDownloadItem currentInstallingItem)
        {
            throw new NotImplementedException();
        }

        public void OnInstallFinished(SKToolsDownloadItem currentInstallingItem)
        {
            throw new NotImplementedException();
        }


        public void OnRouteCalculationCompleted(SKRouteInfo routeInfo)
        {
            if (_currentMapOption == MapOption.AlternativeRoutes)
            {
                int routeIndex = _routeIds.Count;
                _routeIds.Add(routeInfo.RouteID);
                _altRoutesButtons[routeIndex].Text = DemoUtils.FormatDistance(routeInfo.Distance) + "\n" + DemoUtils.FormatTime(routeInfo.EstimatedTime);
                if (routeIndex == 0)
                {
                    // select 1st alternative by default
                    SelectAlternativeRoute(0);
                }
            }
            else if (_currentMapOption == MapOption.RoutingAndNavigation || _currentMapOption == MapOption.PoiTracking || _currentMapOption == MapOption.NaviUi)
            {
                // select the current route (on which navigation will run)
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeInfo.RouteID);
                // zoom to the current route
                SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);

                if (_currentMapOption == MapOption.RoutingAndNavigation)
                {
                    _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
                }
            }
            else if (_currentMapOption == MapOption.Tracks)
            {
                SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);
                _bottomButton.Visibility = ViewStates.Visible;
                _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
            }
        }
    }
}