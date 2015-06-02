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
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Skobbler.SDKDemo.Model;
using Skobbler.SDKDemo.Adapter;
using JavaObject = Java.Lang.Object;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges=(ConfigChanges.Orientation|ConfigChanges.ScreenSize))]
    public class MapActivity : Activity, ISKMapSurfaceListener, ISKRouteListener, ISKNavigationListener, ISKRealReachListener, 
        ISKPOITrackerListener, ISKCurrentPositionListener, ISensorEventListener, ISKMapUpdateListener, ISKToolsNavigationListener, ISKToolsDownloadListener
    {
        private static readonly byte GreenPinIconId = 0;

        private static readonly byte RedPinIconId = 1;

        public static readonly byte ViaPointIconId = 4;

        private static readonly string Tag = "MapActivity";

        public const int Tracks = 1;

        public ToggleButton _toggleButton;

        public static bool RoundTrip;

        public static bool CompassAvailable;

        private static readonly int MinimumTimeUntiLMapCanBeUpdated = 30;

        private static readonly float SmoothFactorCompass = 0.1f;

        public static SKCategories.SKPOICategory[] HeatMapCategories;

        public enum MapOption
        {
            MAP_DISPLAY,
            MAP_STYLES,
            HEAT_MAP,
            MAP_CREATOR,
            MAP_OVERLAYS,
            ANNOTATIONS,
            MAP_DOWNLOADS,
            MAP_UPDATES,
            MAP_INTERACTION,
            ALTERNATIVE_ROUTES,
            REAL_REACH,
            TRACKS,
            ROUTING_AND_NAVIGATION,
            POI_TRACKING,
            NAVI_UI,
            ADDRESS_SEARCH,
            NEARBY_SEARCH,
            CATEGORY_SEARCH,
            REVERSE_GEOCODING,
            MAP_SECTION,
            NAVIGATION_SECTION,
            SEARCHES_SECTION,
            PEDESTRIAN_NAVI,
            TEST_SECTION,
            TEST
        }

        public enum MapAdvices
        {
            TEXT_TO_SPEECH, AUDIO_FILES
        }

        private float[] _energyConsumption = new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (float) 3.7395504, (float) 4.4476889, (float) 5.4306439, (float) 6.722719,
            (float) 8.2830299, (float) 10.0275093, (float) 11.8820908, (float) 13.799201, (float) 15.751434, (float) 17.7231534, (float) 19.7051378, (float) 21.6916725,
            (float) 23.679014, (float) 25.6645696, (float) 27.6464437, (float) 29.6231796, (float) 31.5936073};


        private float[] _orientationValues;
        private long _lastTimeWhenReceivedGpsSignal;
        private float _currentCompassValue;
        private ScreenOrientation _lastExactScreenOrientation = ScreenOrientation.Unspecified;
        private MapOption _currentMapOption = MapOption.MAP_DISPLAY;
        private DemoApplication _app;
        private SKMapSurfaceView _mapView;
        private View _altRoutesView;
        private LinearLayout _mapStylesView;
        private LinearLayout _realReachLayout;
        private Button[] _altRoutesButtons;
        public Button _bottomButton;
        private Button _positionMeButton;
        private RelativeLayout _customView;
        private Button _headingButton;
        private SKCalloutView _mapPopup;
        private TextView _popupTitleView;
        private TextView _popupDescriptionView;
        private List<int> _routeIds = new List<int>();
        private bool _navigationInProgress;
        private bool _skToolsNavigationInProgress;
        private bool _skToolsRouteCalculated;
        private Dictionary<int, SKTrackablePOI> _trackablePOIs;
        private Dictionary<int, SKTrackablePOI> _drawnTrackablePOIs;
        private SKPOITrackerManager _poiTrackingManager;
        private SKCurrentPositionProvider _currentPositionProvider;
        private SKPosition _currentPosition;
        private bool _headingOn;
        private int _realReachRange = 10;
        private SKRealReachSettings.SKRealReachVehicleType _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Car;
        private SKRealReachSettings.SKRealReachMeasurementUnit _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.Second;
        private SKRouteSettings.SKRouteConnectionMode _skRouteConnectionMode = SKRouteSettings.SKRouteConnectionMode.Offline;
        private ImageButton _pedestrianButton;
        private ImageButton _bikeButton;
        private ImageButton _carButton;
        private RelativeLayout _navigationUI;
        private bool _isStartPointBtnPressed = false;
        private bool _isEndPointBtnPressed = false;
        private bool _isViaPointSelected = false;
        private SKCoordinate _startPoint;
        private SKCoordinate _destinationPoint;
        private SKViaPoint _viaPoint;
        public TextToSpeech _textToSpeechEngine;
        private SKToolsNavigationManager _navigationManager;
        private DrawerLayout _drawerLayout;
        private ListView _drawerList;
        private ActionBarDrawerToggle _actionBarDrawerToggle;
        private Dictionary<MapOption, MenuDrawerItem> _menuItems;
        private List<MenuDrawerItem> _list;
        private SKMapViewHolder _mapViewGroup;
        private bool _shouldCacheTheNextRoute;
        private int? _cachedRouteId;
        private ListView _listView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            DemoUtils.InitializeLibrary(this);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_map);
            _app = Application as DemoApplication;

            _currentPositionProvider = new SKCurrentPositionProvider(this);
            _currentPositionProvider.SetCurrentPositionListener(this);
            _currentPositionProvider.RequestLocationUpdates(DemoUtils.HasGpsModule(this), DemoUtils.HasNetworkModule(this), false);

            _mapViewGroup = FindViewById<SKMapViewHolder>(Resource.Id.view_group_map);
            _mapViewGroup.SetMapSurfaceListener(this);
            LayoutInflater inflater = GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            _mapPopup = _mapViewGroup.CalloutView;

            View view = inflater.Inflate(Resource.Layout.layout_popup, null);
            _popupTitleView = view.FindViewById<TextView>(Resource.Id.top_text);
            _popupDescriptionView = view.FindViewById<TextView>(Resource.Id.bottom_text);
            _mapPopup.SetCustomView(view);

            _poiTrackingManager = new SKPOITrackerManager(this);

            _altRoutesView = FindViewById(Resource.Id.alt_routes);

            _altRoutesButtons = new Button[]
        {
            FindViewById<Button>(Resource.Id.alt_route_1),
			FindViewById<Button>(Resource.Id.alt_route_2),
			FindViewById<Button>(Resource.Id.alt_route_3)
        };

            _mapStylesView = FindViewById<LinearLayout>(Resource.Id.map_styles);
            _bottomButton = FindViewById<Button>(Resource.Id.bottom_button);
            _positionMeButton = FindViewById<Button>(Resource.Id.position_me_button);
            _headingButton = FindViewById<Button>(Resource.Id.heading_button);

            _pedestrianButton = FindViewById<ImageButton>(Resource.Id.real_reach_pedestrian_button);
            _bikeButton = FindViewById<ImageButton>(Resource.Id.real_reach_bike_button);
            _carButton = FindViewById<ImageButton>(Resource.Id.real_reach_car_button);

            SKVersioningManager.Instance.SetMapUpdateListener(this);

            _toggleButton = FindViewById<ToggleButton>(Resource.Id.real_reach_round_trip);
            _toggleButton.CheckedChange += (s, e) => RoundTrip = e.IsChecked;

            _realReachLayout = FindViewById<LinearLayout>(Resource.Id.real_reach_time_layout);
            TextView realReachTimeText = FindViewById<TextView>(Resource.Id.real_reach_time);
            SeekBar realReachSeekBar = FindViewById<SeekBar>(Resource.Id.real_reach_seekbar);

            realReachSeekBar.ProgressChanged += (s, e) =>
            {
                _realReachRange = e.Progress;

                string unit;
                if (_realReachUnitType == SKRealReachSettings.SKRealReachMeasurementUnit.Second)
                {
                    unit = "min";
                }
                else if (_realReachUnitType == SKRealReachSettings.SKRealReachMeasurementUnit.Meter)
                {
                    unit = "km";
                }
                else
                {
                    unit = "%";
                }

                realReachTimeText.Text = _realReachRange + " " + unit;
                ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
            };


            Spinner spinner = FindViewById<Spinner>(Resource.Id.real_reach_spinner);

            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.real_reach_measurement_unit, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinner.Adapter = adapter;

            spinner.ItemSelected += (s, e) =>
            {
                String unit = (String)e.Parent.GetItemAtPosition(e.Position);

                realReachSeekBar.Progress = 10;

                if (unit.Equals(GetString(Resource.String.real_reach_profile_distance)) || unit.Equals(GetString(Resource.String.real_reach_profile_time)))
                {

                    _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Car;
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    FindViewById(Resource.Id.real_reach_vehicle_layout).Visibility = ViewStates.Visible;

                    if (unit.Equals(GetString(Resource.String.real_reach_profile_distance)))
                    {
                        _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.Meter;
                        realReachSeekBar.Max = 30;
                        ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    }
                    else if (unit.Equals(GetString(Resource.String.real_reach_profile_time)))
                    {
                        _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.Second;
                        realReachSeekBar.Max = 60;
                        ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    }
                }
                else
                {
                    _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.MiliwattHours;
                    _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Bicycle;
                    realReachSeekBar.Max = 100;
                    FindViewById(Resource.Id.real_reach_vehicle_layout).Visibility = ViewStates.Gone;
                    ShowRealReach(_realReachUnitType, SKRealReachSettings.SKRealReachVehicleType.Bicycle, _realReachRange, _skRouteConnectionMode);
                }
            };

            spinner.NothingSelected += (s, e) =>
            {
                _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.Second;
                _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Car;
                realReachSeekBar.Max = 60;
                realReachSeekBar.Progress = 10;
                FindViewById(Resource.Id.real_reach_vehicle_layout).Visibility = ViewStates.Visible;
                ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
            };

            Spinner spinnerOnOfHy = FindViewById<Spinner>(Resource.Id.real_reach_online_offline_hybrid);
            ArrayAdapter adapterOnOfHy = ArrayAdapter.CreateFromResource(this, Resource.Array.real_reach_online_offline_hybrid, Android.Resource.Layout.SimpleSpinnerItem);
            adapterOnOfHy.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinnerOnOfHy.Adapter = adapterOnOfHy;

            spinnerOnOfHy.ItemSelected += (s, e) =>
            {
                string unit = (string)e.Parent.GetItemAtPosition(e.Position);
                realReachSeekBar.Progress = 10;

                if (unit.Equals(GetString(Resource.String.real_reach_online)) || unit.Equals(GetString(Resource.String.real_reach_offline)))
                {
                    if (unit.Equals(GetString(Resource.String.real_reach_online)))
                    {
                        _skRouteConnectionMode = SKRouteSettings.SKRouteConnectionMode.Online;
                        ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    }
                    else if (unit.Equals(GetString(Resource.String.real_reach_offline)))
                    {
                        _skRouteConnectionMode = SKRouteSettings.SKRouteConnectionMode.Offline;
                        ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    }

                }
                else
                {
                    _skRouteConnectionMode = SKRouteSettings.SKRouteConnectionMode.Hybrid;
                    ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                }
            };

            spinnerOnOfHy.NothingSelected += (s, e) =>
            {
                _realReachUnitType = SKRealReachSettings.SKRealReachMeasurementUnit.Second;
                _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Car;
                realReachSeekBar.Max = 60;
                realReachSeekBar.Progress = 10;
                FindViewById(Resource.Id.real_reach_vehicle_layout).Visibility = ViewStates.Visible;
                ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
            };

            _navigationUI = FindViewById<RelativeLayout>(Resource.Id.navigation_ui_layout);
            InitializeTrackablePOIs();

            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            _drawerList = FindViewById<ListView>(Resource.Id.left_drawer);
            _drawerLayout.SetDrawerShadow(Resource.Drawable.drawer_shadow, GravityCompat.Start);

            _actionBarDrawerToggle = new ActionBarDrawerToggle(this, _drawerLayout, Resource.Drawable.ic_launcher, Resource.String.open_drawer, Resource.String.close_drawer);

            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.SetHomeButtonEnabled(true);

            _drawerLayout.SetDrawerListener(_actionBarDrawerToggle);

            InitializeMenuItems();
        }

        public void InitializeMenuItems()
        {
            _menuItems = new Dictionary<MapOption, MenuDrawerItem>();
            _menuItems.Add(MapOption.MAP_SECTION, Create(MapOption.MAP_SECTION, Resources.GetString(Resource.String.options_group_map).ToUpper(), MenuDrawerItem.SectionType));
            _menuItems.Add(MapOption.MAP_DISPLAY, Create(MapOption.MAP_DISPLAY, Resources.GetString(Resource.String.option_map_display), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.MAP_STYLES, Create(MapOption.MAP_STYLES, Resources.GetString(Resource.String.option_map_styles), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.HEAT_MAP, Create(MapOption.HEAT_MAP, Resources.GetString(Resource.String.option_heat_map), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.MAP_CREATOR, Create(MapOption.MAP_CREATOR, Resources.GetString(Resource.String.option_map_creator), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.MAP_OVERLAYS, Create(MapOption.MAP_OVERLAYS, Resources.GetString(Resource.String.option_overlays), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.ANNOTATIONS, Create(MapOption.ANNOTATIONS, Resources.GetString(Resource.String.option_annotations), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.MAP_DOWNLOADS, Create(MapOption.MAP_DOWNLOADS, Resources.GetString(Resource.String.option_map_xml_and_downloads), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.MAP_UPDATES, Create(MapOption.MAP_UPDATES, Resources.GetString(Resource.String.option_map_updates), MenuDrawerItem.ItemTypeType));

            if (DemoUtils.IsMultipleMapSupportEnabled)
            {
                _menuItems.Add(MapOption.MAP_INTERACTION, Create(MapOption.MAP_INTERACTION, Resources.GetString(Resource.String.option_other_map), MenuDrawerItem.ItemTypeType));
            }

            _menuItems.Add(MapOption.NAVIGATION_SECTION, Create(MapOption.NAVIGATION_SECTION, Resources.GetString(Resource.String.options_group_navigation).ToUpper(), MenuDrawerItem.SectionType));
            _menuItems.Add(MapOption.ROUTING_AND_NAVIGATION, Create(MapOption.ROUTING_AND_NAVIGATION, Resources.GetString(Resource.String.option_routing_and_navigation), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.ALTERNATIVE_ROUTES, Create(MapOption.ALTERNATIVE_ROUTES, Resources.GetString(Resource.String.option_alternative_routes), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.REAL_REACH, Create(MapOption.REAL_REACH, Resources.GetString(Resource.String.option_real_reach), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.TRACKS, Create(MapOption.TRACKS, Resources.GetString(Resource.String.option_tracks), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.POI_TRACKING, Create(MapOption.POI_TRACKING, Resources.GetString(Resource.String.option_poi_tracking), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.NAVI_UI, Create(MapOption.NAVI_UI, Resources.GetString(Resource.String.option_car_navigation_ui), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.PEDESTRIAN_NAVI, Create(MapOption.PEDESTRIAN_NAVI, Resources.GetString(Resource.String.option_pedestrian_navigation_ui), MenuDrawerItem.ItemTypeType));

            _menuItems.Add(MapOption.SEARCHES_SECTION, Create(MapOption.SEARCHES_SECTION, Resources.GetString(Resource.String.search).ToUpper(), MenuDrawerItem.SectionType));
            _menuItems.Add(MapOption.ADDRESS_SEARCH, Create(MapOption.ADDRESS_SEARCH, Resources.GetString(Resource.String.option_address_search), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.NEARBY_SEARCH, Create(MapOption.NEARBY_SEARCH, Resources.GetString(Resource.String.option_nearby_search), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.CATEGORY_SEARCH, Create(MapOption.CATEGORY_SEARCH, Resources.GetString(Resource.String.option_category_search), MenuDrawerItem.ItemTypeType));
            _menuItems.Add(MapOption.REVERSE_GEOCODING, Create(MapOption.REVERSE_GEOCODING, Resources.GetString(Resource.String.option_reverse_geocoding), MenuDrawerItem.ItemTypeType));

            //menuItems.put(MapOption.TEST_SECTION, Create(MapOption.TEST_SECTION, Resources.GetString(Resource.String.test).toUpperCase(), MenuDrawerItem.SECTION_TYPE));
            //menuItems.put(MapOption.TEST, Create(MapOption.TEST, Resources.GetString(Resource.String.testing), MenuDrawerItem.ITEM_TYPE));

            _list = new List<MenuDrawerItem>(_menuItems.Values);

            _drawerList.Adapter = new MenuDrawerAdapter(this, Resource.Layout.element_menu_drawer_item, _list);
            _drawerList.ItemClick += (s, e) => SelectItem(e.Position);
        }

        public void SelectItem(int position)
        {
            _drawerList.SetItemChecked(position, true);
            if (_drawerLayout.IsDrawerOpen(_drawerList))
            {
                _drawerLayout.CloseDrawer(_drawerList);
            }
            HandleMenuItemClick(_list[position].MapOption);
        }

        public static MenuDrawerItem Create(MapOption mapOption, string label, int itemType)
        {
            MenuDrawerItem menuDrawerItem = new MenuDrawerItem(mapOption);
            menuDrawerItem.Label = label;
            menuDrawerItem.ItemType = itemType;
            return menuDrawerItem;
        }

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

        private void InitializeTrackablePOIs()
        {
            _trackablePOIs = new Dictionary<int, SKTrackablePOI>();

            _trackablePOIs.Add(64142, new SKTrackablePOI(64142, 0, 37.735610, -122.446434, -1, "Teresita Boulevard"));
            _trackablePOIs.Add(64143, new SKTrackablePOI(64143, 0, 37.732367, -122.442033, -1, "Congo Street"));
            _trackablePOIs.Add(64144, new SKTrackablePOI(64144, 0, 37.732237, -122.429190, -1, "John F Foran Freeway"));
            _trackablePOIs.Add(64145, new SKTrackablePOI(64145, 1, 37.738090, -122.401470, -1, "Revere Avenue"));
            _trackablePOIs.Add(64146, new SKTrackablePOI(64146, 0, 37.741128, -122.398562, -1, "McKinnon Ave"));
            _trackablePOIs.Add(64147, new SKTrackablePOI(64147, 1, 37.746154, -122.394077, -1, "Evans Ave"));
            _trackablePOIs.Add(64148, new SKTrackablePOI(64148, 0, 37.750057, -122.392287, -1, "Cesar Chavez Street"));
            _trackablePOIs.Add(64149, new SKTrackablePOI(64149, 1, 37.762823, -122.392957, -1, "18th Street"));
            _trackablePOIs.Add(64150, new SKTrackablePOI(64150, 0, 37.760242, -122.392495, 180, "20th Street"));
            _trackablePOIs.Add(64151, new SKTrackablePOI(64151, 0, 37.755157, -122.392196, 180, "23rd Street"));

            _trackablePOIs.Add(64152, new SKTrackablePOI(64152, 0, 37.773526, -122.452706, -1, "Shrader Street"));
            _trackablePOIs.Add(64153, new SKTrackablePOI(64153, 0, 37.786535, -122.444528, -1, "Pine Street"));
            _trackablePOIs.Add(64154, new SKTrackablePOI(64154, 1, 37.792242, -122.424426, -1, "Franklin Street"));
            _trackablePOIs.Add(64155, new SKTrackablePOI(64155, 0, 37.716146, -122.409480, -1, "Campbell Ave"));
            _trackablePOIs.Add(64156, new SKTrackablePOI(64156, 0, 37.719133, -122.388280, -1, "Fitzgerald Ave"));

            _drawnTrackablePOIs = new Dictionary<int, SKTrackablePOI>();
        }

        protected override void OnResume()
        {
            base.OnResume();

            _mapViewGroup.OnResume();

            if (_headingOn)
            {
                StartOrientationSensor();
            }

            if (_currentMapOption == MapOption.NAVI_UI)
            {
                ToggleButton selectStartPointBtn = FindViewById<ToggleButton>(Resource.Id.select_start_point_button);
                ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
                string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");

                if (prefNavigationType.Equals("0"))
                {
                    selectStartPointBtn.Visibility = ViewStates.Gone;
                }
                else if (prefNavigationType.Equals("1"))
                {
                    selectStartPointBtn.Visibility = ViewStates.Visible;
                }
            }

            if (!DemoUtils.IsMultipleMapSupportEnabled && _currentMapOption == MapOption.HEAT_MAP && HeatMapCategories != null)
            {
                _mapView.ShowHeatMapsWithPoiType(HeatMapCategories);
            }

            if (_currentMapOption == MapOption.MAP_INTERACTION && IsRouteCached)
            {
                LoadRouteFromCache();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            _mapViewGroup.OnPause();

            if (_headingOn || CompassAvailable)
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
        }

        public void OnSurfaceCreated(SKMapViewHolder mapHolder)
        {
            FindViewById(Resource.Id.chess_board_background).Visibility = ViewStates.Gone;

            _mapView = mapHolder.MapSurfaceView;

            ApplySettingsOnMapView();

            if (SplashActivity.NewMapVersionDetected != 0)
            {
                ShowUpdateDialog(SplashActivity.NewMapVersionDetected);
            }

            if (!_navigationInProgress)
            {
                _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
            }

            if (DemoUtils.IsMultipleMapSupportEnabled && _currentMapOption == MapOption.HEAT_MAP && HeatMapCategories != null)
            {
                _mapView.ShowHeatMapsWithPoiType(HeatMapCategories);
            }

            if (_currentPosition != null)
            {
                _mapView.ReportNewGPSPosition(_currentPosition);
            }
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
                        if (_currentMapOption.Equals(MapOption.TRACKS) && TrackElementsActivity.SelectedTrackElement != null)
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
                    _drawerLayout.OpenDrawer((int)GravityFlags.Left);
                }
                return true;
            }
            else
            {
                return base.OnKeyDown(keyCode, e);
            }
        }

        [Export("onClick")]
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
                    if (_currentMapOption == MapOption.ROUTING_AND_NAVIGATION || _currentMapOption == MapOption.TRACKS)
                    {
                        if (_bottomButton.Text.Equals(Resources.GetString(Resource.String.calculate_route)))
                        {
                            LaunchRouteCalculation(new SKCoordinate(-122.397674, 37.761278), new SKCoordinate(-122.448270, 37.738761));
                        }
                        else if (_bottomButton.Text.Equals(Resources.GetString(Resource.String.start_navigation)))
                        {
                            new AlertDialog.Builder(this)
                                    .SetMessage("Choose the advice type")
                                    .SetCancelable(false)
                                    .SetPositiveButton("Scout audio", (s, e) =>
                                    {
                                        _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                                        SetAdvicesAndStartNavigation(MapAdvices.AUDIO_FILES);
                                    })
                                    .SetNegativeButton("Text to speech", (s, e) =>
                                    {
                                        if (_textToSpeechEngine == null)
                                        {
                                            Toast.MakeText(this, "Initializing TTS engine", ToastLength.Long).Show();
                                            _textToSpeechEngine = new TextToSpeech(this, new TextToSpeechOnInitListener(this));
                                        }
                                        else
                                        {
                                            _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                                            SetAdvicesAndStartNavigation(MapAdvices.TEXT_TO_SPEECH);
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
                    else if (_currentMapOption == MapOption.MAP_INTERACTION)
                    {
                        Toast.MakeText(this, "New map instance created", ToastLength.Long).Show();
                        SKRouteManager.Instance.ClearCurrentRoute();

                        Intent intent = new Intent(this, typeof(MapCacheActivity));
                        StartActivity(intent);
                    }
                    break;
                case Resource.Id.position_me_button:
                    if (_headingOn)
                    {
                        setHeading(false);
                    }
                    if (_mapView != null && _currentPosition != null)
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
                        setHeading(true);
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                    }
                    break;
                case Resource.Id.real_reach_pedestrian_button:
                    _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Pedestrian;
                    ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.real_reach_bike_button:
                    _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Bicycle;
                    ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.real_reach_car_button:
                    _realReachVehicleType = SKRealReachSettings.SKRealReachVehicleType.Car;
                    ShowRealReach(_realReachUnitType, _realReachVehicleType, _realReachRange, _skRouteConnectionMode);
                    _carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
                    _pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    _bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
                    break;
                case Resource.Id.navigation_ui_back_button:
                    Button backButton = FindViewById<Button>(Resource.Id.navigation_ui_back_button);
                    LinearLayout naviButtons = FindViewById<LinearLayout>(Resource.Id.navigation_ui_buttons);

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
                    _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
                    ActionBar.SetDisplayHomeAsUpEnabled(false);
                    ActionBar.SetHomeButtonEnabled(false);
                    CalculateRouteFromSKTools();
                    break;

                case Resource.Id.settings_button:
                    StartActivity(new Intent(this, typeof(SettingsActivity)));
                    break;
                case Resource.Id.start_free_drive_button:
                    StartFreeDriveFromSKTools();
                    ActionBar.SetDisplayHomeAsUpEnabled(false);
                    ActionBar.SetHomeButtonEnabled(false);
                    break;
                case Resource.Id.clear_via_point_button:
                    _viaPoint = null;
                    _mapView.DeleteAnnotation(MapActivity.ViaPointIconId);
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

        private class TextToSpeechOnInitListener : JavaObject, TextToSpeech.IOnInitListener
        {
            private readonly MapActivity _context;
            public TextToSpeechOnInitListener(MapActivity context)
            {
                _context = context;
            }

            public void OnInit(OperationResult status)
            {

                if (status == OperationResult.Success)
                {
                    LanguageAvailableResult result = _context._textToSpeechEngine.SetLanguage(Locale.English);
                    if (result == LanguageAvailableResult.MissingData || result == LanguageAvailableResult.NotSupported)
                    {
                        Toast.MakeText(_context, "This Language is not supported", ToastLength.Long).Show();
                    }
                }
                else
                {
                    Toast.MakeText(_context, _context.GetString(Resource.String.text_to_speech_engine_not_initialized), ToastLength.Short).Show();
                }
                _context._bottomButton.Text = _context.Resources.GetString(Resource.String.stop_navigation);
                _context.SetAdvicesAndStartNavigation(MapAdvices.TEXT_TO_SPEECH);
            }
        }

        private void StartFreeDriveFromSKTools()
        {
            SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();

            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            String prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.K_DISTANCE_UNIT, "0");
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

            string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.K_IN_TOWN_SPEED_WARNING, "0");
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

            string prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.K_OUT_TOWN_SPEED_WARNING, "0");
            if (prefSpeedOutTown.Equals("0"))
            {
                configuration.SpeedWarningThresholdOutsideCity = (5.0);
            }
            else if (prefSpeedOutTown.Equals("1"))
            {
                configuration.SpeedWarningThresholdOutsideCity = (10.0);
            }
            else if (prefSpeedOutTown.Equals("2"))
            {
                configuration.SpeedWarningThresholdOutsideCity = (15.0);
            }
            else if (prefSpeedOutTown.Equals("3"))
            {
                configuration.SpeedWarningThresholdOutsideCity = (20.0);
            }

            bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.K_AUTO_DAY_NIGHT, true);

            if (!dayNight)
            {
                configuration.AutomaticDayNight = false;
            }

            configuration.NavigationType = SKNavigationSettings.SKNavigationType.File;
            configuration.FreeDriveNavigationFilePath = _app.MapResourcesDirPath + "logFile/Seattle.log";
            configuration.DayStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "daystyle/", "daystyle.json");
            configuration.NightStyle = new SKMapViewStyle(_app.MapResourcesDirPath + "nightstyle/", "nightstyle.json");

            _navigationUI.Visibility = ViewStates.Gone;
            _navigationManager = new SKToolsNavigationManager(this, Resource.Id.map_layout_root);
            _navigationManager.SetNavigationListener(this);

            if (_currentMapOption == MapOption.PEDESTRIAN_NAVI)
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.Pedestrian;
            }

            _navigationManager.StartFreeDriveWithConfiguration(configuration, _mapViewGroup);
        }

        private void CalculateRouteFromSKTools()
        {

            SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);

            string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");

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

            string prefRouteType = "0";
            if (_currentMapOption == MapOption.PEDESTRIAN_NAVI)
            {
                configuration.RouteType = SKRouteSettings.SKRouteMode.Pedestrian;
            }
            else
            {
                prefRouteType = sharedPreferences.GetString(PreferenceTypes.K_ROUTE_TYPE,
                        "2");
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
            }

            string prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.K_DISTANCE_UNIT, "0");
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

            string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.K_IN_TOWN_SPEED_WARNING, "0");
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

            String prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.K_OUT_TOWN_SPEED_WARNING, "0");
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

            bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.K_AUTO_DAY_NIGHT, true);
            if (!dayNight)
            {
                configuration.AutomaticDayNight = false;
            }
            bool tollRoads = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_TOLL_ROADS, false);
            if (tollRoads)
            {
                configuration.TollRoadsAvoided = true;
            }
            bool avoidFerries = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_FERRIES, false);
            if (avoidFerries)
            {
                configuration.FerriesAvoided = true;
            }
            bool highWays = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_HIGHWAYS, false);
            if (highWays)
            {
                configuration.HighWaysAvoided = true;
            }
            bool freeDrive = sharedPreferences.GetBoolean(PreferenceTypes.K_FREE_DRIVE, true);
            if (!freeDrive)
            {
                configuration.ContinueFreeDriveAfterNavigationEnd = false;
            }

            _navigationUI.Visibility = ViewStates.Gone;
            configuration.StartCoordinate = _startPoint;
            configuration.DestinationCoordinate = _destinationPoint;
            List<SKViaPoint> viaPointList = new List<SKViaPoint>();
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
                _navigationManager.LaunchRouteCalculation(configuration, _mapViewGroup);
            }
        }

        private void StartOrientationSensorInPedestrian()
        {
            CompassAvailable = PackageManager.HasSystemFeature(PackageManager.FeatureSensorCompass);

            if (CompassAvailable)
            {
                StartOrientationSensor();
            }
            else
            {
                StopOrientationSensor();
            }
        }

        private void InitializeNavigationUI(bool showStartingAndDestinationAnnotations)
        {
            ToggleButton selectViaPointBtn = FindViewById<ToggleButton>(Resource.Id.select_via_point_button);
            ToggleButton selectStartPointBtn = FindViewById<ToggleButton>(Resource.Id.select_start_point_button);
            ToggleButton selectEndPointBtn = FindViewById<ToggleButton>(Resource.Id.select_end_point_button);

            StartOrientationSensorInPedestrian();

            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            String prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");
            if (prefNavigationType.Equals("0"))
            {
                selectStartPointBtn.Visibility = ViewStates.Gone;
            }
            else if (prefNavigationType.Equals("1"))
            {

                selectStartPointBtn.Visibility = ViewStates.Visible;
            }

            if (showStartingAndDestinationAnnotations)
            {
                _startPoint = new SKCoordinate(13.34615707397461, 52.513086884218325);
                SKAnnotation annotation = new SKAnnotation(SKAnnotation.SkAnnotationTypeGreen);
                annotation.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
                annotation.Location = _startPoint;
                _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

                _destinationPoint = new SKCoordinate(13.398685455322266, 52.50995268098114);
                annotation = new SKAnnotation(SKAnnotation.SkAnnotationTypeRed);
                annotation.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
                annotation.Location = _destinationPoint;
                _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

            }

            _mapView.SetZoom(11.0F);
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

            _navigationUI.Visibility = ViewStates.Visible;
        }

        private void ShowNoCurrentPosDialog()
        {
            new AlertDialog.Builder(this)
            .SetMessage("There is no current position available")
            .SetNegativeButton("Ok", (s,e) => { })
            .Show();
        }

        private void LaunchRouteCalculation(SKCoordinate startPoint, SKCoordinate destinationPoint)
        {
            ClearRouteFromCache();

            SKRouteSettings route = new SKRouteSettings
            {
                StartCoordinate = startPoint,
                DestinationCoordinate = destinationPoint,
                NoOfRoutes = 1,
                RouteMode = SKRouteSettings.SKRouteMode.CarFastest,
                RouteExposed = true
            };

            SKRouteManager.Instance.SetRouteListener(this);

            SKRouteManager.Instance.CalculateRoute(route);
        }

        private void LaunchAlternativeRouteCalculation()
        {
            SKRouteSettings route = new SKRouteSettings()
            {
                StartCoordinate = new SKCoordinate(-122.392284, 37.787189),
                DestinationCoordinate = new SKCoordinate(-122.484378, 37.856300),
                NoOfRoutes = 3,
                RouteMode = SKRouteSettings.SKRouteMode.CarFastest,
                RouteExposed = true
            };

            SKRouteManager.Instance.SetRouteListener(this);
            SKRouteManager.Instance.CalculateRoute(route);
        }

        private void PrepareAnnotations()
        {
            SKAnnotation annotation1 = new SKAnnotation(10)
            {
                Location = new SKCoordinate(-122.4200, 37.7765),
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeRed
            };

            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            // Add an annotation using the absolute path to the image.
            SKAnnotation annotation = new SKAnnotation(13)
            {
                Location = new SKCoordinate(-122.434516, 37.770712),
                MininumZoomLevel = 5
            };

            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            if (metrics.DensityDpi < DisplayMetrics.DensityHigh)
            {
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@2x.png";
                annotation.ImageSize = 128;
            }
            else
            {
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@3x.png";
                annotation.ImageSize = 256;

            }

            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

            SKAnnotationView annotationView = new SKAnnotationView
            {
                DrawableResourceId = Resource.Drawable.icon_map_popup_navigate,
                Width = 128,
                Height = 128
            };

            SKAnnotation annotationDrawable = new SKAnnotation(14)
            {
                Location = new SKCoordinate(-122.437182, 37.777079),
                MininumZoomLevel = 5,
                AnnotationView = annotationView,
            };

            _mapView.AddAnnotation(annotationDrawable, SKAnimationSettings.AnimationNone);

            _customView = (GetSystemService(Context.LayoutInflaterService) as LayoutInflater).Inflate(Resource.Layout.layout_custom_view, null, false) as RelativeLayout;

            annotationView = new SKAnnotationView
            {
                View = _customView
            };

            SKAnnotation annotationFromView = new SKAnnotation(15)
            {
                Location = new SKCoordinate(-122.423573, 37.761349),
                MininumZoomLevel = 5,
                AnnotationView = annotationView
            };

            _mapView.AddAnnotation(annotationFromView, SKAnimationSettings.AnimationNone);

            _mapView.SetZoom(13);

            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
        }

        private void DrawShapes()
        {
            SKPolygon polygon = new SKPolygon
            {
                Nodes = new List<SKCoordinate>
					{
						new SKCoordinate(-122.4342, 37.7765),
						new SKCoordinate(-122.4141, 37.7765),
						new SKCoordinate(-122.4342, 37.7620)
					},
                OutlineSize = 3,
                Identifier = 10,
            };

            polygon.SetOutlineColor(new float[] { 1f, 0f, 0f, 1f });
            polygon.SetColor(new float[] { 1f, 0f, 0f, 0.2f });

            _mapView.AddPolygon(polygon);

            SKCircle circleMask = new SKCircle
            {
                MaskedObjectScale = 1.3f,
                OutlineSize = 3,
                CircleCenter = new SKCoordinate(-122.4200, 37.7665),
                Radius = 300,
                OutlineDottedPixelsSkip = 6,
                OutlineDottedPixelsSolid = 10,
                NumberOfPoints = 150,
                Identifier = 11,
            };

            circleMask.SetColor(new float[] { 1f, 1f, 0.5f, 0.67f });
            circleMask.SetOutlineColor(new float[] { 0f, 0f, 0f, 1f });

            _mapView.AddCircle(circleMask);

            SKPolyline polyline = new SKPolyline
            {
                Nodes = new List<SKCoordinate>
					{
						new SKCoordinate(-122.4342, 37.7898),
						new SKCoordinate(-122.4141, 37.7898),
						new SKCoordinate(-122.4342, 37.7753)
					},
                OutlineSize = 4,
                OutlineDottedPixelsSolid = 3,
                OutlineDottedPixelsSkip = 3,
                Identifier = 12
            };

            polyline.SetColor(new float[] { 0f, 0f, 1f, 1f });
            polyline.SetOutlineColor(new float[] { 0f, 0f, 1f, 1f });

            _mapView.AddPolyline(polyline);
        }

        private void SelectMapStyle(SKMapViewStyle newStyle)
        {
            _mapView.MapSettings.MapStyle = newStyle;
            SelectStyleButton();
        }

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

        private void ClearMap()
        {
            setHeading(false);

            switch (_currentMapOption)
            {
                case MapOption.MAP_DISPLAY:
                    break;
                case MapOption.MAP_OVERLAYS:
                    _mapView.ClearAllOverlays();
                    break;
                case MapOption.ALTERNATIVE_ROUTES:
                    HideAlternativeRoutesButtons();
                    SKRouteManager.Instance.ClearRouteAlternatives();
                    SKRouteManager.Instance.ClearCurrentRoute();
                    _routeIds.Clear();
                    break;
                case MapOption.MAP_STYLES:
                    _mapStylesView.Visibility = ViewStates.Gone;
                    break;
                case MapOption.TRACKS:

                    if (_navigationInProgress)
                    {
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
                case MapOption.REAL_REACH:
                    _mapView.ClearRealReachDisplay();
                    _realReachLayout.Visibility = ViewStates.Gone;
                    Spinner spinner = FindViewById<Spinner>(Resource.Id.real_reach_spinner);
                    spinner.SetSelection(0);
                    break;
                case MapOption.ANNOTATIONS:
                    _mapPopup.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    break;
                case MapOption.ROUTING_AND_NAVIGATION:
                    _bottomButton.Visibility = ViewStates.Gone;
                    SKRouteManager.Instance.ClearCurrentRoute();
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    if (_navigationInProgress)
                    {
                        StopNavigation();
                    }
                    break;
                case MapOption.POI_TRACKING:
                    if (_navigationInProgress)
                    {
                        StopNavigation();
                    }
                    SKRouteManager.Instance.ClearCurrentRoute();
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    _poiTrackingManager.StopPOITracker();
                    break;
                case MapOption.HEAT_MAP:
                    HeatMapCategories = null;
                    _mapView.ClearHeatMapsDisplay();
                    break;
                case MapOption.NAVI_UI:
                case MapOption.PEDESTRIAN_NAVI:
                    _navigationUI.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    break;
                case MapOption.MAP_INTERACTION:
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    _bottomButton.Visibility = ViewStates.Gone;
                    SKRouteManager.Instance.ClearCurrentRoute();
                    ClearRouteFromCache();
                    _shouldCacheTheNextRoute = false;
                    break;
                default:
                    break;
            }
            _currentMapOption = MapOption.MAP_DISPLAY;
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
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(_routeIds[routeIndex]);
            }

        }

        private void LaunchNavigation()
        {
            if (TrackElementsActivity.SelectedTrackElement != null)
            {
                _mapView.ClearTrackElement(TrackElementsActivity.SelectedTrackElement);
            }

            SKNavigationSettings navigationSettings = new SKNavigationSettings
            {
                NavigationType = SKNavigationSettings.SKNavigationType.Simulation,
                PositionerVerticalAlignment = -0.25f,
                ShowRealGPSPositions = false
            };

            SKNavigationManager navigationManager = SKNavigationManager.Instance;
            navigationManager.SetMapView(_mapView);
            navigationManager.SetNavigationListener(this);
            navigationManager.StartNavigation(navigationSettings);

            _navigationInProgress = true;
        }

        public void SetAdvicesAndStartNavigation(MapAdvices currentMapAdvices)
        {
            SKAdvisorSettings advisorSettings = new SKAdvisorSettings
            {
                Language = SKAdvisorSettings.SKAdvisorLanguage.LanguageEn,
                AdvisorConfigPath = _app.MapResourcesDirPath + "/Advisor",
                ResourcePath = _app.MapResourcesDirPath + "/Advisor/Languages",
                AdvisorVoice = "en"
            };

            switch (currentMapAdvices)
            {
                case MapAdvices.AUDIO_FILES:
                    advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.AudioFiles;
                    break;
                case MapAdvices.TEXT_TO_SPEECH:
                    advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.TextToSpeech;
                    break;
            }

            SKRouteManager.Instance.SetAudioAdvisorSettings(advisorSettings);
            LaunchNavigation();
        }

        private void StopNavigation()
        {
            _navigationInProgress = false;
            _routeIds.Clear();

            if (_textToSpeechEngine != null && !_textToSpeechEngine.IsSpeaking)
            {
                _textToSpeechEngine.Stop();
            }
            if (_currentMapOption.Equals(MapOption.TRACKS) && TrackElementsActivity.SelectedTrackElement != null)
            {
                SKRouteManager.Instance.ClearCurrentRoute();

                _mapView.DrawTrackElement(TrackElementsActivity.SelectedTrackElement);
                _mapView.FitTrackElementInView(TrackElementsActivity.SelectedTrackElement, false);

                SKRouteManager.Instance.SetRouteListener(this);
                SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.SelectedTrackElement, SKRouteSettings.SKRouteMode.BicycleFastest, true, true, false);
            }

            SKNavigationManager.Instance.StopNavigation();

        }

        public void OnAllRoutesCompleted()
        {
            if (_shouldCacheTheNextRoute)
            {
                _shouldCacheTheNextRoute = false;
                SKRouteManager.Instance.SaveRouteToCache(_cachedRouteId.Value);
            }

            SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);

            if (_currentMapOption == MapOption.POI_TRACKING)
            {
                _poiTrackingManager.StartPOITrackerWithRadius(10000, 0.5);
                _poiTrackingManager.AddWarningRulesforPoiType(SKTrackablePOIType.Speedcam);
                LaunchNavigation();
            }
        }

        public void OnReceivedPOIs(SKTrackablePOIType type, IList<SKDetectedPOI> detectedPois)
        {
            UpdateMapWithLatestDetectedPOIs(detectedPois);
        }

        private void UpdateMapWithLatestDetectedPOIs(IList<SKDetectedPOI> detectedPois)
        {

            List<int> detectedIdsList = new List<int>();

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
                if (!_drawnTrackablePOIs.ContainsKey(detectedPoiId))
                {
                    _drawnTrackablePOIs.Add(detectedPoiId, _trackablePOIs[detectedPoiId]);
                    DrawDetectedPOI(detectedPoiId);
                }
            }
            foreach (int drawnPoiId in new List<int>(_drawnTrackablePOIs.Keys))
            {
                if (!detectedIdsList.Contains(drawnPoiId))
                {
                    _drawnTrackablePOIs.Remove(drawnPoiId);
                    _mapView.DeleteAnnotation(drawnPoiId);
                }
            }
        }

        private void DrawDetectedPOI(int poiId)
        {
            SKTrackablePOI poi = _trackablePOIs[poiId];

            SKAnnotation annotation = new SKAnnotation(poiId)
            {
                Location = poi.Coordinate,
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeMarker
            };

            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
        }

        public void OnUpdatePOIsInRadius(double latitude, double longitude, int radius)
        {
            _poiTrackingManager.SetTrackedPOIs(SKTrackablePOIType.Speedcam, new List<SKTrackablePOI>(_trackablePOIs.Values));
        }

        private void ApplySmoothAlgorithm(float newCompassValue)
        {
            if (Math.Abs(newCompassValue - _currentCompassValue) < 180)
            {
                _currentCompassValue = _currentCompassValue + SmoothFactorCompass * (newCompassValue - _currentCompassValue);
            }
            else
            {
                if (_currentCompassValue > newCompassValue)
                {
                    _currentCompassValue = (_currentCompassValue + SmoothFactorCompass * ((360 + newCompassValue - _currentCompassValue) % 360) + 360) % 360;
                }
                else
                {
                    _currentCompassValue = (_currentCompassValue - SmoothFactorCompass * ((360 - newCompassValue + _currentCompassValue) % 360) + 360) % 360;
                }
            }
        }

        public void OnSensorChanged(SensorEvent sensorEvent)
        {
            //_mapView.reportNewHeading(t.values[0]);
            switch (sensorEvent.Sensor.Type)
            {
                case SensorType.Orientation:
                    if (_orientationValues != null)
                    {
                        for (int i = 0; i < _orientationValues.Length; i++)
                        {
                            _orientationValues[i] = sensorEvent.Values[i];

                        }
                        if (_orientationValues[0] != 0)
                        {
                            if ((DemoUtils.CurrentTimeMillis() - _lastTimeWhenReceivedGpsSignal) > MinimumTimeUntiLMapCanBeUpdated)
                            {
                                ApplySmoothAlgorithm(_orientationValues[0]);
                                ScreenOrientation currentExactScreenOrientation = DemoUtils.GetExactScreenOrientation(this);
                                if (_lastExactScreenOrientation != currentExactScreenOrientation)
                                {
                                    _lastExactScreenOrientation = currentExactScreenOrientation;
                                    switch (_lastExactScreenOrientation)
                                    {
                                        case ScreenOrientation.Portrait:
                                            _mapView.ReportNewDeviceOrientation(SKMapSurfaceView.SKOrientationType.Portrait);
                                            break;
                                        case ScreenOrientation.ReversePortrait:
                                            _mapView.ReportNewDeviceOrientation(SKMapSurfaceView.SKOrientationType.PortraitUpsidedown);
                                            break;
                                        case ScreenOrientation.Landscape:
                                            _mapView.ReportNewDeviceOrientation(SKMapSurfaceView.SKOrientationType.LandscapeRight);
                                            break;
                                        case ScreenOrientation.ReverseLandscape:
                                            _mapView.ReportNewDeviceOrientation(SKMapSurfaceView.SKOrientationType.LandscapeLeft);
                                            break;
                                    }
                                }

                                // report to NG the new value
                                if (_orientationValues[0] < 0)
                                {
                                    _mapView.ReportNewHeading(-_orientationValues[0]);
                                }
                                else
                                {
                                    _mapView.ReportNewHeading(_orientationValues[0]);
                                }

                                _lastTimeWhenReceivedGpsSignal = DemoUtils.CurrentTimeMillis();
                            }
                        }
                    }
                    break;
            }
        }

        private void setHeading(bool enabled)
        {
            if (enabled)
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

        private void StartOrientationSensor()
        {
            _orientationValues = new float[3];
            SensorManager sensorManager = GetSystemService(SensorService) as SensorManager;
            Sensor orientationSensor = sensorManager.GetDefaultSensor(SensorType.Orientation);
            sensorManager.RegisterListener(this, orientationSensor, SensorDelay.Ui);
        }

        private void StopOrientationSensor()
        {
            _orientationValues = null;
            SensorManager sensorManager = GetSystemService(SensorService) as SensorManager;
            sensorManager.UnregisterListener(this);
        }

        public void OnCurrentPositionUpdate(SKPosition currentPosition)
        {
            _currentPosition = currentPosition;

            if (_mapView != null)
            {
                _mapView.ReportNewGPSPosition(_currentPosition);
            }
        }

        public void OnOnlineRouteComputationHanging(int status)
        {

        }

        public void OnActionPan()
        {
            if (_headingOn)
            {
                setHeading(false);
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
            if (_navigationUI.Visibility == ViewStates.Visible)
            {
                return;
            }

            int annotationHeight = 0;
            float annotationOffset = annotation.Offset.GetY();

            switch (annotation.UniqueID)
            {
                case 10:
                    annotationHeight = annotation.ImageSize;
                    _popupTitleView.Text = "Annotation using texture ID";
                    _popupDescriptionView.Text = " Red pin";
                    break;
                case 13:
                    annotationHeight = annotation.ImageSize;
                    _popupTitleView.Text = "Annotation using absolute \n image path";
                    _popupDescriptionView.Text = null;
                    break;
                case 14:
                    annotationHeight = annotation.AnnotationView.Height;
                    _popupTitleView.Text = "Annotation using  \n drawable resource ID ";
                    _popupDescriptionView.Text = null;
                    break;
                case 15:
                    annotationHeight = _customView.Height;
                    _popupTitleView.Text = "Annotation using custom view";
                    _popupDescriptionView.Text = null;
                    break;
            }

            _mapPopup.SetVerticalOffset(-annotationOffset + annotationHeight / 2);
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

        public void OnCustomPOISelected(SKMapCustomPOI customPoi)
        {
        }

        public void OnDoubleTap(SKScreenPoint point)
        {
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

        public void OnMapPOISelected(SKMapPOI mapPOI)
        {
        }

        public void OnMapRegionChanged(SKCoordinateRegion mapRegion)
        {
        }

        public void OnRotateMap()
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
            ClearMap();
        }

        public void OnFreeDriveUpdated(String countryCode, String streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
        {
        }

        public void OnReRoutingStarted()
        {
        }

        public void OnSpeedExceededWithAudioFiles(String[] adviceList, bool speedExceeded)
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
            _mapView.FitRealReachInView(bbox, false, 0);
        }

        public void OnPOIClusterSelected(SKPOICluster poiCluster)
        {
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnTunnelEvent(bool tunnelEntered)
        {
        }

        public void OnMapRegionChangeEnded(SKCoordinateRegion mapRegion)
        {
        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion mapRegion)
        {
        }

        public void OnMapVersionSet(int newVersion)
        {
        }

        private void ShowUpdateDialog(int newVersion)
        {
            new AlertDialog.Builder(this)
            .SetMessage("New map version available")
            .SetCancelable(true)
            .SetPositiveButton(GetString(Resource.String.update_label), (s, e) =>
            {
                SKVersioningManager manager = SKVersioningManager.Instance;
                bool updated = manager.UpdateMapsVersion(newVersion);
                if (updated)
                {
                    _app.AppPrefs.SaveBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED, true);
                    SplashActivity.NewMapVersionDetected = 0;
                    Toast.MakeText(this, "The map has been updated to version " + newVersion, ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, "An error occurred in updating the map ", ToastLength.Short).Show();
                }
            })
            .SetNegativeButton(GetString(Resource.String.cancel_label), (s, e) => {/*AlertDialog.Cancel()*/}).Create().Show();
        }

        public void OnNewVersionDetected(int newVersion)
        {
            ShowUpdateDialog(newVersion);
        }

        public void OnNoNewVersionDetected()
        {
            Toast.MakeText(this, "No new versions were detected", ToastLength.Short).Show();
        }

        public void OnVersionFileDownloadTimeout()
        {
        }

        public void OnCurrentPositionSelected()
        {
        }

        public void OnObjectSelected(int id)
        {
        }

        public override void OnBackPressed()
        {
            if (_skToolsNavigationInProgress || _skToolsRouteCalculated)
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Really quit?");
                alert.SetMessage("Do you want to exit navigation?");
                alert.SetPositiveButton("Yes", (s, e) =>
                {
                    _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
                    ActionBar.SetDisplayHomeAsUpEnabled(true);
                    ActionBar.SetHomeButtonEnabled(true);
                    if (_skToolsNavigationInProgress)
                    {
                        _navigationManager.StopNavigation();
                    }
                    else
                    {
                        _navigationManager.RemoveRouteCalculationScreen();
                    }
                    InitializeNavigationUI(false);
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
                    if (ResourceDownloadsListActivity.mapsDAO != null)
                    {
                        //todo
                        SKToolsDownloadManager downloadManager = SKToolsDownloadManager.GetInstance(this);
                        if (downloadManager.IsDownloadProcessRunning)
                        {
                            downloadManager.PauseDownloadThread();
                            return;
                        }
                    }
                    Finish();
                    Process.KillProcess(Process.MyPid());
                });
                alert.SetNegativeButton("Cancel", (s,e) => { });
                alert.Show();

            }

        }

        public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
        {
            MapDownloadResource mapResource = (MapDownloadResource)ResourceDownloadsListActivity.allMapResources[currentDownloadItem.ItemCode];
            mapResource.DownloadState = (currentDownloadItem.DownloadState);
            mapResource.NoDownloadedBytes = (currentDownloadItem.NoDownloadedBytes);
            ResourceDownloadsListActivity.mapsDAO.updateMapResource(mapResource);
            _app.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
            Finish();
        }


        public void OnRouteCalculationCompleted(SKRouteInfo routeInfo)
        {
            if (_currentMapOption == MapOption.ALTERNATIVE_ROUTES)
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
            else if (_currentMapOption == MapOption.ROUTING_AND_NAVIGATION || _currentMapOption == MapOption.POI_TRACKING
                  || _currentMapOption == MapOption.NAVI_UI)
            {
                // select the current route (on which navigation will run)
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeInfo.RouteID);
                // zoom to the current route
                SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);

                if (_currentMapOption == MapOption.ROUTING_AND_NAVIGATION)
                {
                    _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
                }
            }
            else if (_currentMapOption == MapOption.TRACKS)
            {
                SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);
                _bottomButton.Visibility = ViewStates.Visible;
                _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
            }
            else if (_currentMapOption == MapOption.MAP_INTERACTION)
            {
                if (_shouldCacheTheNextRoute)
                {
                    _cachedRouteId = routeInfo.RouteID;
                }
            }
        }

        public void OnRouteCalculationFailed(SKRouteListenerSKRoutingErrorCode arg0)
        {
            _shouldCacheTheNextRoute = false;
            Toast.MakeText(this, Resources.GetString(Resource.String.route_calculation_failed), ToastLength.Short).Show();
        }

        public void OnSignalNewAdviceWithAudioFiles(string[] audioFiles, bool specialSoundFile)
        {
            SKLogging.WriteLog(Tag, " onSignalNewAdviceWithAudioFiles " + Arrays.AsList(audioFiles), SKLogging.LogDebug);
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
        }

        public void OnViaPointReached(int index)
        {
        }

        public void OnNavigationStarted()
        {
            _skToolsNavigationInProgress = true;
            if (_navigationUI.Visibility == ViewStates.Visible)
            {
                _navigationUI.Visibility = ViewStates.Gone;
            }
        }

        public void OnNavigationEnded()
        {
            _skToolsRouteCalculated = false;
            _skToolsNavigationInProgress = false;
            _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.SetHomeButtonEnabled(true);
            InitializeNavigationUI(false);
        }

        public void OnRouteCalculationStarted()
        {
            _skToolsRouteCalculated = true;
        }

        public void OnRouteCalculationCompleted()
        {
        }

        public void OnRouteCalculationCanceled()
        {
            _skToolsRouteCalculated = false;
            _skToolsNavigationInProgress = false;
            _drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            InitializeNavigationUI(false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (_actionBarDrawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected void HandleMenuItemClick(MapOption mapOption)
        {
            ClearMap();
            switch (mapOption)
            {
                case MapOption.MAP_DISPLAY:
                    _mapView.ClearHeatMapsDisplay();
                    _currentMapOption = MapOption.MAP_DISPLAY;
                    _bottomButton.Visibility = ViewStates.Gone;
                    SKRouteManager.Instance.ClearCurrentRoute();
                    break;
                case MapOption.MAP_INTERACTION:
                    _currentMapOption = MapOption.MAP_INTERACTION;
                    HandleMapInteractionOption();
                    break;
                case MapOption.MAP_OVERLAYS:
                    _currentMapOption = MapOption.MAP_OVERLAYS;
                    DrawShapes();
                    _mapView.SetZoom(14);
                    _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
                    break;
                case MapOption.ALTERNATIVE_ROUTES:
                    _currentMapOption = MapOption.ALTERNATIVE_ROUTES;
                    _altRoutesView.Visibility = ViewStates.Visible;
                    LaunchAlternativeRouteCalculation();
                    break;
                case MapOption.MAP_STYLES:
                    _currentMapOption = MapOption.MAP_STYLES;
                    _mapStylesView.Visibility = ViewStates.Visible;
                    SelectStyleButton();
                    break;
                case MapOption.MAP_CREATOR:
                    _currentMapOption = MapOption.MAP_DISPLAY;
                    _mapView.ApplySettingsFromFile(_app.MapCreatorFilePath);
                    break;
                case MapOption.TRACKS:
                    _currentMapOption = MapOption.TRACKS;
                    Intent trackIntent = new Intent(this, typeof(TracksActivity));
                    StartActivityForResult(trackIntent, Tracks);
                    break;
                case MapOption.REAL_REACH:
                    _currentMapOption = MapOption.REAL_REACH;
                    _mapView.CenterMapOnPosition(new SKCoordinate(13.4127, 52.5233));
                    _realReachLayout.Visibility = ViewStates.Visible;
                    break;
                case MapOption.MAP_DOWNLOADS:
                    if (DemoUtils.IsInternetAvailable(this))
                    {
                        StartActivity(new Intent(this, typeof(ResourceDownloadsListActivity)));
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_internet_connection), ToastLength.Short).Show();
                    }
                    break;
                case MapOption.REVERSE_GEOCODING:
                    StartActivity(new Intent(this, typeof(ReverseGeocodingActivity)));
                    break;
                case MapOption.ADDRESS_SEARCH:
                    StartActivity(new Intent(this, typeof(OfflineAddressSearchActivity)));
                    break;
                case MapOption.NEARBY_SEARCH:
                    StartActivity(new Intent(this, typeof(NearbySearchActivity)));
                    break;
                case MapOption.ANNOTATIONS:
                    _currentMapOption = MapOption.ANNOTATIONS;
                    PrepareAnnotations();
                    break;
                case MapOption.CATEGORY_SEARCH:
                    StartActivity(new Intent(this, typeof(CategorySearchResultsActivity)));
                    break;
                case MapOption.ROUTING_AND_NAVIGATION:
                    _currentMapOption = MapOption.ROUTING_AND_NAVIGATION;
                    _bottomButton.Visibility = ViewStates.Visible;
                    _bottomButton.Text = Resources.GetString(Resource.String.calculate_route);
                    break;
                case MapOption.POI_TRACKING:
                    _currentMapOption = MapOption.POI_TRACKING;
                    if (_trackablePOIs == null)
                    {
                        InitializeTrackablePOIs();
                    }
                    LaunchRouteCalculation(new SKCoordinate(-122.397674, 37.761278), new SKCoordinate(-122.448270, 37.738761));
                    break;
                case MapOption.HEAT_MAP:
                    _currentMapOption = MapOption.HEAT_MAP;
                    StartActivity(new Intent(this, typeof(POICategoriesListActivity)));
                    break;
                case MapOption.MAP_UPDATES:
                    SKVersioningManager.Instance.CheckNewVersion(3);
                    break;
                case MapOption.NAVI_UI:
                    _currentMapOption = MapOption.NAVI_UI;
                    InitializeNavigationUI(true);
                    FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Gone;
                    FindViewById(Resource.Id.settings_button).Visibility = ViewStates.Visible;
                    ((Button)FindViewById(Resource.Id.start_free_drive_button)).Text = "Start free drive";
                    break;
                case MapOption.PEDESTRIAN_NAVI:
                    _currentMapOption = MapOption.PEDESTRIAN_NAVI;
                    InitializeNavigationUI(true);
                    FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Gone;
                    FindViewById(Resource.Id.settings_button).Visibility = ViewStates.Gone;
                    ((Button)FindViewById(Resource.Id.start_free_drive_button)).Text = "Start free walk";
                    Toast.MakeText(this, "Pedestrian navigation: illustrating optimized 2D view with previous positions trail and pedestrian specific follow-modes: historic, compass & north bound", ToastLength.Long).Show();
                    break;
                default:
                    break;
            }
            if (_currentMapOption != MapOption.MAP_DISPLAY)
            {
                _positionMeButton.Visibility = ViewStates.Gone;
                _headingButton.Visibility = ViewStates.Gone;
            }
        }

        private void ShowRealReach(SKRealReachSettings.SKRealReachMeasurementUnit unitType, SKRealReachSettings.SKRealReachVehicleType vehicleType, int range, SKRouteSettings.SKRouteConnectionMode skRouteConnectionMode)
        {
            if (_mapView == null)
            {
                return;
            }

            _mapView.ClearRealReachDisplay();

            _mapView.SetRealReachListener(this);

            SKRealReachSettings realReachSettings = new SKRealReachSettings
            {
                Location = new SKCoordinate(13.4127, 52.5233),
                MeasurementUnit = unitType,
                RoundTrip = MapActivity.RoundTrip,
                ConnectionMode = skRouteConnectionMode,
                TransportMode = vehicleType
            };

            if (unitType == SKRealReachSettings.SKRealReachMeasurementUnit.MiliwattHours)
            {
                realReachSettings.SetConsumption(_energyConsumption);
                realReachSettings.Range = range * 100;
            }
            else if (unitType == SKRealReachSettings.SKRealReachMeasurementUnit.Second)
            {
                realReachSettings.Range = range * 60;
            }
            else
            {
                realReachSettings.Range = range * 1000;
            }

            _mapView.DisplayRealReachWithSettings(realReachSettings);
        }

        private void HandleMapInteractionOption()
        {
            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));

            SKAnnotation annotation1 = new SKAnnotation(10)
            {
                Location = new SKCoordinate(-122.4200, 37.7765),
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeRed
            };

            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation2 = new SKAnnotation(11)
            {
                Location = new SKCoordinate(-122.412753, 37.777142),
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeGreen
            };

            _mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);

            _bottomButton.Visibility = ViewStates.Visible;
            _bottomButton.Text = "Open new map instance";

            _shouldCacheTheNextRoute = true;

            LaunchRouteCalculation(new SKCoordinate(-122.4200, 37.7765), new SKCoordinate(-122.412753, 37.777142));
        }

        private bool IsRouteCached
        {
            get { return _cachedRouteId != null; }
        }

        public void LoadRouteFromCache()
        {
            SKRouteManager.Instance.LoadRouteFromCache(_cachedRouteId.Value);
        }

        public void ClearRouteFromCache()
        {
            SKRouteManager.Instance.ClearAllRoutesFromCache();
            _cachedRouteId = null;
        }

        public void OnAllDownloadsCancelled()
        {
        }

        public void OnDownloadCancelled(string p0)
        {
        }

        public void OnDownloadProgress(SKToolsDownloadItem p0)
        {
        }

        public void OnInstallFinished(SKToolsDownloadItem p0)
        {
        }

        public void OnInstallStarted(SKToolsDownloadItem p0)
        {
        }

        public void OnInternetConnectionFailed(SKToolsDownloadItem p0, bool p1)
        {
        }

        public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem p0)
        {
        }
    }
}