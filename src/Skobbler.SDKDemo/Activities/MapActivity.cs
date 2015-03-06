using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Map.RealReach;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.PoiTracker;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Util;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "MapActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    class MapActivity : Activity, ISKMapSurfaceListener, ISKCurrentPositionListener, ISKPOITrackerListener, ISKMapUpdateListener, ISKRealReachListener, ISensorEventListener, ISKNavigationListener, ISKRouteListener
    {
        public const int Tracks = 1;

        private const string Tag = "MapActivity";

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
            POITracking,
            HeatMap,
            MapInteraction
        }

        public static SKCategories.SKPOICategory[] HeatMapCategories;

        private MapOption _currentMapOption = MapOption.MapDisplay;
        private DemoApplication _app;
        private SKMapSurfaceView _mapView;
        private View _menu;
        private View _altRoutesView;
        private LinearLayout _mapStylesView;
        private RelativeLayout _realReachLayout;
        private Button[] _altRoutesButtons;
        private Button _bottomButton;
        private Button _positionMeButton;
        private RelativeLayout _customView;
        private Button _headingButton;
        private SKCalloutView _mapPopup;
        private TextView _popupTitleView;
        private TextView _popupDescriptionView;
        private List<int> _routeIds = new List<int>();
        private bool _navigationInProgress;
        private Dictionary<int, SKTrackablePOI> _trackablePOIs;
        private Dictionary<int, SKTrackablePOI> _drawnTrackablePOIs;
        private SKPOITrackerManager _poiTrackingManager;
        private SKCurrentPositionProvider _currentPositionProvider;
        private SKPosition _currentPosition;
        private bool _headingOn;
        private int _realReachRange;
        private sbyte _realReachVehicleType = SKRealReachSettings.VehicleTypePedestrian;
        private ImageButton _pedestrianButton;
        private ImageButton _bikeButton;
        private ImageButton _carButton;
        private TextView _realReachTime;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_map);

            _app = Application as DemoApplication;

            _currentPositionProvider = new SKCurrentPositionProvider(this);
            _currentPositionProvider.SetCurrentPositionListener(this);

            if (DemoUtils.HasGpsModule(this))
            {
                _currentPositionProvider.RequestLocationUpdates(true, true, true);
            }

            SKMapViewHolder mapViewGroup = FindViewById<SKMapViewHolder>(Resource.Id.view_group_map);
            _mapView = mapViewGroup.MapSurfaceView;
            _mapView.SetMapSurfaceListener(this);
            _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;

            LayoutInflater inflater = GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            _mapPopup = mapViewGroup.CalloutView;
            View view = inflater.Inflate(Resource.Layout.layout_popup, null);
            _popupTitleView = view.FindViewById<TextView>(Resource.Id.top_text);
            _popupDescriptionView = view.FindViewById<TextView>(Resource.Id.bottom_text);
            _mapPopup.SetCustomView(view);

            ApplySettingsOnMapView();
            _poiTrackingManager = new SKPOITrackerManager(this);

            _menu = FindViewById(Resource.Id.options_menu);
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

            _realReachTime = FindViewById<TextView>(Resource.Id.real_reach_time);
            SeekBar readReachSeekBar = FindViewById<SeekBar>(Resource.Id.real_reach_seekbar);
            readReachSeekBar.ProgressChanged += OnProgressChanged;
            _realReachLayout = FindViewById<RelativeLayout>(Resource.Id.real_reach_layout);

            InitializeTrackablePOIs();
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

        void OnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            _realReachRange = e.Progress;
            _realReachTime.Text = _realReachRange + " min";
            ShowRealReach(_realReachVehicleType, _realReachRange);
        }

        private void ShowRealReach(sbyte vehicleType, int range)
        {
            _mapView.SetRealReachListener(this);

            var realReachSettings = new SKRealReachSettings();
            var realReachCenter = new SKCoordinate(23.593957, 46.773361);
            realReachSettings.Latitude = realReachCenter.Latitude;
            realReachCenter.Longitude = realReachCenter.Longitude;

            realReachSettings.MeasurementUnit = SKRealReachSettings.UnitSecond;
            realReachSettings.Range = range * 60.0F;
            realReachSettings.TransportMode = vehicleType;

            _mapView.DisplayRealReachWithSettings(realReachSettings);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Menu)
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
            else
            {
                return base.OnKeyDown(keyCode, e);
            }
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

        protected override void OnResume()
        {
            base.OnResume();
            _mapView.OnResume();

            if (_headingOn)
            {
                StartOrientationSensor();
            }
        }

        private void StartOrientationSensor()
        {
            SensorManager sensorManager = GetSystemService(Context.SensorService) as SensorManager;
            Sensor orientationSensor = sensorManager.GetDefaultSensor(SensorType.Orientation);
            sensorManager.RegisterListener(this, orientationSensor, SensorDelay.Ui);
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

        private void StopOrientationSensor()
        {
            SensorManager sensorManager = GetSystemService(Context.SensorService) as SensorManager;
            sensorManager.UnregisterListener(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _currentPositionProvider.StopLocationUpdates();
            SKMaps.Instance.DestroySKMaps();
            Process.KillProcess(Process.MyPid());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case Tracks:
                        if (_currentMapOption == MapOption.Tracks && TrackElementsActivity.SelectedTrackElement != null)
                        {
                            _mapView.DrawTrackElement(TrackElementsActivity.SelectedTrackElement);
                            _mapView.FitTrackElementInView(TrackElementsActivity.SelectedTrackElement, false);

                            SKRouteManager.Instance.SetRouteListener(this);
                            SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.SelectedTrackElement, SKRouteSettings.SkrouteBicycleFastest, true, true, false);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void OnActionPan()
        {
            if (_headingOn)
            {
                SetHeading(false);
            }
        }

        private void SetHeading(bool enabled)
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

        public void OnActionZoom()
        {
        }

        public void OnAnnotationSelected(SKAnnotation annotation)
        {
            var metrics = new DisplayMetrics();
            float density = Resources.DisplayMetrics.Density;

            RunOnUiThread(() =>
            {
                switch (annotation.UniqueID)
                {
                    case 10:
                        _mapPopup.SetVerticalOffset(30.0F * density);
                        _popupTitleView.Text = "Annotation using texture ID";
                        _popupDescriptionView.Text = " Red pin ";
                        break;
                    case 11:
                        _mapPopup.SetVerticalOffset(30.0F * density);
                        _popupTitleView.Text = "Annotation using texture ID";
                        _popupDescriptionView.Text = " Green pin ";
                        break;
                    case 12:
                        _mapPopup.SetVerticalOffset(30.0F * density);
                        _popupTitleView.Text = "Annotation using texture ID";
                        _popupDescriptionView.Text = " Blue pin ";
                        break;
                    case 13:
                        if (metrics.DensityDpi < DisplayMetrics.DensityHigh)
                        {
                            _mapPopup.SetVerticalOffset(16.0F / density);
                        }
                        else
                        {
                            _mapPopup.SetVerticalOffset(32.0F / density);
                        }
                        _popupTitleView.Text = "Annotation using absolute \n image path";
                        _popupDescriptionView.Text = null;
                        break;
                    case 14:
                        _mapPopup.SetVerticalOffset(64.0F / density);
                        _popupTitleView.Text = "Annotation using  \n drawable resource ID ";
                        _popupDescriptionView.Text = null;
                        break;
                    case 15:
                        _mapPopup.SetVerticalOffset((float)_customView.Height / density);
                        _popupTitleView.Text = "Annotation using custom view";
                        _popupDescriptionView.Text = null;
                        break;
                }

                _mapPopup.ShowAtLocation(annotation.Location, true);

            });
        }

        public void OnCompassSelected()
        {

        }

        public void OnCurrentPositionSelected()
        {

        }

        public void OnCustomPOISelected(SKMapCustomPOI customPoi)
        {

        }

        public void OnDoubleTap(SKScreenPoint point)
        {
            _mapView.ZoomInAt(point);
        }

        public void OnInternationalisationCalled(int result)
        {

        }

        public void OnInternetConnectionNeeded()
        {

        }

        public void OnLongPress(SKScreenPoint point)
        {

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

        public void OnMapRegionChangeEnded(SKCoordinateRegion mapRegion)
        {

        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion mapRegion)
        {

        }

        public void OnMapRegionChanged(SKCoordinateRegion mapVisibleRegion)
        {

        }

        public void OnObjectSelected(int objectId)
        {

        }

        public void OnOffportRequestCompleted(int requestId)
        {

        }

        public void OnPOIClusterSelected(SKPOICluster poiCluster)
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

        public void OnSurfaceCreated()
        {
            RunOnUiThread(() =>
            {
                View chessBackground = FindViewById<View>(Resource.Id.chess_board_background);
                chessBackground.Visibility = ViewStates.Gone;
            });

            if (_currentMapOption == MapOption.HeatMap && HeatMapCategories != null)
            {
                _mapView.ShowHeatMapsWithPoiType(HeatMapCategories);
            }
        }

        public void OnCurrentPositionUpdate(SKPosition currentPosition)
        {
            _currentPosition = currentPosition;
            _mapView.ReportNewGPSPosition(_currentPosition);
        }

        public void OnReceivedPOIs(SKTrackablePOIType type, IList<SKDetectedPOI> detectedPOIs)
        {
            UpdateMapWithLatestDetectedPOIs(detectedPOIs);
        }

        private void UpdateMapWithLatestDetectedPOIs(IList<SKDetectedPOI> detectedPOIs)
        {
            var detectedIdsList = new List<int>();

            foreach (SKDetectedPOI detectedPoi in detectedPOIs)
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
            SKAnnotation annotation = new SKAnnotation();
            annotation.UniqueID = poiId;
            SKTrackablePOI poi = _trackablePOIs[poiId];
            annotation.Location = new SKCoordinate(poi.Longitude, poi.Latitude);
            annotation.MininumZoomLevel = 5;
            annotation.AnnotationType = SKAnnotation.SkAnnotationTypeMarker;
            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
        }

        public void OnUpdatePOIsInRadius(double latitude, double longitude, int radius)
        {
            Log.Error("", "trackablePOIs " + _trackablePOIs + " poiTrackingManager: " + _poiTrackingManager);
            _poiTrackingManager.SetTrackedPOIs(SKTrackablePOIType.Speedcam, new List<SKTrackablePOI>(_trackablePOIs.Values));
        }

        public void OnMapVersionSet(int newVersion)
        {

        }

        [Export("OnMenuOptionClick")]
        public void OnMenuOptionClick(View v)
        {
            ClearMap();
            switch (v.Id)
            {
                case Resource.Id.option_map_display:
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
                    var intent = new Intent(this, typeof(TracksActivity));
                    StartActivityForResult(intent, Tracks);
                    break;
                case Resource.Id.option_real_reach:
                    _currentMapOption = MapOption.RealReach;
                    _realReachLayout.Visibility = ViewStates.Visible;
                    ShowRealReach(_realReachVehicleType, _realReachRange);
                    break;
                case Resource.Id.option_map_xml_and_downloads:
                    if (DemoUtils.IsInternetAvailable(this))
                    {
                        StartActivity(new Intent(this, typeof(MapPackagesListActivity)));
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
                    _currentMapOption = MapOption.POITracking;
                    if (_trackablePOIs == null)
                    {
                        InitializeTrackablePOIs();
                    }
                    LaunchRouteCalculation();
                    break;
                case Resource.Id.option_heat_map:
                    _currentMapOption = MapOption.HeatMap;
                    StartActivity(new Intent(this, typeof(POICategoriesListActivity)));
                    break;
                case Resource.Id.option_map_updates:
                    SKVersioningManager.Instance.CheckNewVersion(3);
                    break;
                case Resource.Id.option_map_interaction:
                    _currentMapOption = MapOption.MapInteraction;
                    HandleMapInteractionOption();
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

        private void DrawShapes()
        {
            // get a polygon shape object
            SKPolygon polygon = new SKPolygon();
            // set the polygon's nodes
            List<SKCoordinate> nodes = new List<SKCoordinate>();
            nodes.Add(new SKCoordinate(-122.4342, 37.7765));
            nodes.Add(new SKCoordinate(-122.4141, 37.7765));
            nodes.Add(new SKCoordinate(-122.4342, 37.7620));
            polygon.Nodes = nodes;
            // set the outline size
            polygon.OutlineSize = 3;
            // set colors used to render the polygon
            polygon.SetOutlineColor(new float[] { 1f, 0f, 0f, 1f });
            polygon.SetColor(new float[] { 1f, 0f, 0f, 0.2f });
            // render the polygon on the map
            _mapView.AddPolygon(polygon);

            // get a circle mask shape object
            SKCircle circleMask = new SKCircle();
            // set the shape's mask scale
            circleMask.MaskedObjectScale = 1.3f;
            // set the colors
            circleMask.SetColor(new float[] { 1f, 1f, 0.5f, 0.67f });
            circleMask.SetOutlineColor(new float[] { 0f, 0f, 0f, 1f });
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
            // set the nodes on the polyline
            nodes = new List<SKCoordinate>();
            nodes.Add(new SKCoordinate(-122.4342, 37.7898));
            nodes.Add(new SKCoordinate(-122.4141, 37.7898));
            nodes.Add(new SKCoordinate(-122.4342, 37.7753));
            polyline.Nodes = nodes;
            // set polyline color
            polyline.SetColor(new float[] { 0f, 0f, 1f, 1f });
            // set properties for the outline
            polyline.SetOutlineColor(new float[] { 0f, 0f, 1f, 1f });
            polyline.OutlineSize = 4;
            polyline.OutlineDottedPixelsSolid = 3;
            polyline.OutlineDottedPixelsSkip = 3;
            _mapView.AddPolyline(polyline);
        }

        private void LaunchAlternativeRouteCalculation()
        {
            SKRouteSettings route = new SKRouteSettings();
            route.StartCoordinate = new SKCoordinate(-122.392284, 37.787189);
            route.DestinationCoordinate = new SKCoordinate(-122.484378, 37.856300);
            // number of alternative routes specified here
            route.NoOfRoutes = 3;
            route.RouteMode = SKRouteSettings.SkrouteCarFastest;
            route.RouteExposed = true;
            SKRouteManager.Instance.SetRouteListener(this);
            SKRouteManager.Instance.CalculateRoute(route);
        }

        private void SelectStyleButton()
        {
            for (int i = 0; i < _mapStylesView.ChildCount; i++)
            {
                _mapStylesView.GetChildAt(i).Selected = false;
            }

            SKMapViewStyle mapStyle = _mapView.MapSettings.MapStyle;

            if (mapStyle == null || mapStyle.StyleFileName == "daystyle.json")
            {
                FindViewById(Resource.Id.map_style_day).Selected = true;
            }
            else if (mapStyle.StyleFileName == "nightstyle.json")
            {
                FindViewById(Resource.Id.map_style_night).Selected = true;
            }
            else if (mapStyle.StyleFileName == "outdoorstyle.json")
            {
                FindViewById(Resource.Id.map_style_outdoor).Selected = true;
            }
            else if (mapStyle.StyleFileName == "grayscalestyle.json")
            {
                FindViewById(Resource.Id.map_style_grayscale).Selected = true;
            }
        }

        private void PrepareAnnotations()
        {
            // get the annotation object
            SKAnnotation annotation1 = new SKAnnotation();
            // set unique id used for rendering the annotation
            annotation1.UniqueID = 10;
            // set annotation location
            annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
            // set minimum zoom level at which the annotation should be visible
            annotation1.MininumZoomLevel = 5;
            // set the annotation's type
            annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
            // render annotation on map
            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation2 = new SKAnnotation();
            annotation2.UniqueID = 11;
            annotation2.Location = new SKCoordinate(-122.410338, 37.769193);
            annotation2.MininumZoomLevel = 5;
            annotation2.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
            _mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation3 = new SKAnnotation();
            annotation3.UniqueID = 12;
            annotation3.Location = new SKCoordinate(-122.430337, 37.779776);
            annotation3.MininumZoomLevel = 5;
            annotation3.AnnotationType = SKAnnotation.SkAnnotationTypeBlue;
            _mapView.AddAnnotation(annotation3, SKAnimationSettings.AnimationNone);

            // add an annotation with an image file
            SKAnnotation annotation = new SKAnnotation();
            annotation.UniqueID = 13;
            annotation.Location = new SKCoordinate(-122.434516, 37.770712);
            annotation.MininumZoomLevel = 5;


            DisplayMetrics metrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(metrics);
            if (metrics.DensityDpi < DisplayMetrics.DensityHigh)
            {
                // set the center point of the image - tapping on an annotation will
                // depend on
                // this value . Also the actual gps coordinates of the annotation
                // will
                // be in the center of the image.
                annotation.Offset.SetX(16);
                annotation.Offset.SetY(16);
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/poi_marker.png";
                // set the size of the image in pixels
                annotation.ImageSize = 32;
            }
            else
            {
                // set the center point of the image - tapping on an annotation will
                // depend on
                // this value . Also the actual gps coordinates of the annotation
                // will
                // be in the center of the image.
                annotation.Offset.SetX(32);
                annotation.Offset.SetY(32);
                annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/poi_marker_retina.png";
                // set the size of the image in pixels
                annotation.ImageSize = 64;

            }

            _mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);


            // add an annotation with a drawable resource
            SKAnnotation annotationDrawable = new SKAnnotation();
            annotationDrawable.UniqueID = 14;
            annotationDrawable.Location = new SKCoordinate(-122.437182, 37.777079);
            annotationDrawable.MininumZoomLevel = 5;
            // set the center point of the image - tapping on an annotation will
            // depend on
            // this value . Also the actual gps coordinates of the annotation will
            // be in the center of the image.
            annotationDrawable.Offset = new SKScreenPoint(64, 64);
            SKAnnotationView annotationView = new SKAnnotationView();
            annotationView.DrawableResourceId = (Resource.Drawable.icon_map_popup_navigate);
            // set the width and height of the image in pixels (they have to be
            // powers of 2)
            annotationView.Width = 128;
            annotationView.Height = 128;
            annotationDrawable.AnnotationView = annotationView;
            _mapView.AddAnnotation(annotationDrawable, SKAnimationSettings.AnimationNone);


            // // add an annotation with a view
            SKAnnotation annotationFromView = new SKAnnotation();
            annotationFromView.UniqueID = 15;
            annotationFromView.Location = new SKCoordinate(-122.423573, 37.761349);
            annotationFromView.MininumZoomLevel = 5;
            annotationView = new SKAnnotationView();
            _customView = (RelativeLayout)((LayoutInflater)GetSystemService(Context.LayoutInflaterService)).Inflate(Resource.Layout.layout_custom_view, null, false);
            // set view object to be displayed as annotation
            annotationView.View = _customView;
            annotationFromView.AnnotationView = annotationView;
            _mapView.AddAnnotation(annotationFromView, SKAnimationSettings.AnimationNone);

            // set map zoom level
            _mapView.SetZoom(13);
            // center map on a position
            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
        }

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
            route.RouteMode = SKRouteSettings.SkrouteCarFastest;
            // set whether the route should be shown on the map after it's computed
            route.RouteExposed = true;
            // set the route listener to be notified of route calculation
            // events
            SKRouteManager.Instance.SetRouteListener(this);
            // pass the route to the calculation routine
            SKRouteManager.Instance.CalculateRoute(route);
        }

        private void HandleMapInteractionOption()
        {
            _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));

            // get the annotation object
            SKAnnotation annotation1 = new SKAnnotation();
            // set unique id used for rendering the annotation
            annotation1.UniqueID = 10;
            // set annotation location
            annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
            // set minimum zoom level at which the annotation should be visible
            annotation1.MininumZoomLevel = 5;
            // set the annotation's type
            annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
            // render annotation on map
            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation2 = new SKAnnotation();
            annotation2.UniqueID = 11;
            annotation2.Location = new SKCoordinate(-122.419789, 37.775428);
            annotation2.MininumZoomLevel = 5;
            annotation2.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
            _mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);

            float density = Resources.DisplayMetrics.Density;

            TextView topText = _mapPopup.FindViewById<TextView>(Resource.Id.top_text);
            topText.Text = "Get details";
            topText.Click += (s, e) => StartActivity(new Intent(this, typeof(InteractionMapActivity)));
            _mapPopup.FindViewById<View>(Resource.Id.bottom_text).Visibility = ViewStates.Gone;

            _mapPopup.SetVerticalOffset(30 * density);
            _mapPopup.ShowAtLocation(annotation1.Location, true);
        }

        private void ClearMap()
        {
            SetHeading(false);
            switch (_currentMapOption)
            {
                case MapOption.MapDisplay:
                    break;
                case MapOption.MapOverlays:
                    _mapView.ClearAllOverlays();
                    break;
                case MapOption.AlternativeRoutes:
                    HideAlternativeRoutesButtons();
                    SKRouteManager.Instance.ClearRouteAlternatives();
                    SKRouteManager.Instance.ClearCurrentRoute();
                    _routeIds.Clear();
                    break;
                case MapOption.MapStyles:
                    _mapStylesView.Visibility = ViewStates.Gone;
                    break;
                case MapOption.Tracks:
                    if (_navigationInProgress)
                    {
                        StopNavigation();
                    }
                    _bottomButton.Visibility = ViewStates.Gone;
                    TrackElementsActivity.SelectedTrackElement = null;
                    break;
                case MapOption.RealReach:
                    _mapView.ClearRealReachDisplay();
                    _realReachLayout.Visibility = ViewStates.Gone;
                    break;
                case MapOption.Annotations:
                    _mapPopup.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    break;
                case MapOption.RoutingAndNavigation:
                    _bottomButton.Visibility = ViewStates.Gone;
                    if (_navigationInProgress)
                    {
                        StopNavigation();
                    }
                    break;
                case MapOption.POITracking:
                    if (_navigationInProgress)
                    {
                        StopNavigation();
                    }
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    _poiTrackingManager.StopPOITracker();
                    break;
                case MapOption.HeatMap:
                    HeatMapCategories = null;
                    _mapView.ClearHeatMapsDisplay();
                    break;
                case MapOption.MapInteraction:
                    _mapPopup.Visibility = ViewStates.Gone;
                    _mapView.DeleteAllAnnotationsAndCustomPOIs();
                    FindViewById<TextView>(Resource.Id.top_text).SetOnClickListener(null);
                    FindViewById<TextView>(Resource.Id.top_text).Text = "Title text";
                    FindViewById<TextView>(Resource.Id.top_text).Text = "Subtitle text";
                    break;
                default:
                    break;
            }

            _currentMapOption = MapOption.MapDisplay;
            _positionMeButton.Visibility = ViewStates.Visible;
            _headingButton.Visibility = ViewStates.Visible;
        }

        private void HideAlternativeRoutesButtons()
        {
            DeselectAlternativeRoutesButtons();
            _altRoutesView.Visibility = ViewStates.Gone;
            foreach (var button in _altRoutesButtons)
            {
                button.Text = "distance\ntime";
            }
        }

        private void StopNavigation()
        {
            _navigationInProgress = false;
            SKNavigationManager.Instance.StopNavigation();
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
                        if (_bottomButton.Text == Resources.GetString(Resource.String.calculate_route))
                        {
                            LaunchRouteCalculation();
                        }
                        else if (_bottomButton.Text == Resources.GetString(Resource.String.start_navigation))
                        {
                            _bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
                            LaunchNavigation();
                        }
                        else if (_bottomButton.Text == Resources.GetString(Resource.String.stop_navigation))
                        {
                            StopNavigation();
                            _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
                        }
                    }
                    break;
                case Resource.Id.position_me_button:
                    if (_headingOn)
                    {
                        SetHeading(false);
                    }
                    if (_currentPosition != null)
                    {
                        _mapView.CenterMapOnCurrentPositionSmooth(17.0F, 500);
                    }
                    else
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                    }
                    break;
                case Resource.Id.heading_button:
                    if (_currentPosition != null)
                    {
                        SetHeading(true);
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
                case Resource.Id.exit_real_reach:
                    _mapView.ClearRealReachDisplay();
                    _realReachLayout.Visibility = ViewStates.Gone;
                    break;

                default:
                    break;
            }

        }

        private void LaunchNavigation()
        {
            SKNavigationSettings navigationSettings = new SKNavigationSettings();
            // set the desired navigation settings
            navigationSettings.NavigationType = SKNavigationSettings.NavigationTypeSimulation;
            navigationSettings.PositionerVerticalAlignment = -0.25F;
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

        private void SelectMapStyle(SKMapViewStyle newStyle)
        {
            _mapView.MapSettings.MapStyle = newStyle;
            SelectStyleButton();
        }

        private void SelectAlternativeRoute(int routeIndex)
        {
            DeselectAlternativeRoutesButtons();
            _altRoutesButtons[routeIndex].Selected = true;
            SKRouteManager.Instance.ZoomToRoute(1.0F, 1.0F, 110, 8, 8, 8);
            SKRouteManager.Instance.SetCurrentRouteByUniqueId(_routeIds[routeIndex]);
        }

        private void DeselectAlternativeRoutesButtons()
        {
            foreach (var button in _altRoutesButtons)
            {
                button.Selected = false;
            }
        }

        public void OnNewVersionDetected(int newVersion)
        {
            RunOnUiThread(() =>
            {
                AlertDialog alertDialog = new AlertDialog.Builder(this).Create();
                alertDialog.SetMessage("New map version available");
                alertDialog.SetCancelable(true);
                alertDialog.SetButton(GetString(Resource.String.update_label),
                        (s, e) =>
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

                alertDialog.SetButton(GetString(Resource.String.cancel_label), (s, e) => alertDialog.Cancel());
                alertDialog.Show();
            });
        }

        public void OnNoNewVersionDetected()
        {
            RunOnUiThread(() => Toast.MakeText(this, "No new versions were detected", ToastLength.Short).Show());
        }

        public void OnVersionFileDownloadTimeout()
        {

        }

        public override void OnBackPressed()
        {
            if (_menu.Visibility == ViewStates.Visible)
            {
                _menu.Visibility = ViewStates.Gone;
            }
            else
            {
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("Really quit? ");
                alert.SetMessage("Do you really want to exit the app?");
                alert.SetPositiveButton("Yes", (s, e) => Finish());
                alert.SetNegativeButton("Cancel", (s, e) => { });
                alert.Show();
            }
        }

        public void OnRealReachCalculationCompleted(int xMin, int xMax, int yMin, int yMax)
        {
            _mapView.FitRealReachInView(xMin, xMax, yMin, yMax, false, 0);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent e)
        {
            _mapView.ReportNewHeading(e.Values[0]);
        }

        public void OnDestinationReached()
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(this, "Destination reached", ToastLength.Short).Show();
                ClearMap();
            });
        }

        public void OnFreeDriveUpdated(string countryCode, string streetName, int streetType, double currentSpeed, double speedLimit)
        {

        }

        public void OnReRoutingStarted()
        {

        }

        public void OnSignalNewAdvice(string[] audioFiles, bool specialSoundFile)
        {
            SKLogging.WriteLog(Tag, "navigation advice: ", SKLogging.LogDebug);
            AdvicePlayer.Instance.PlayAdvice(audioFiles);
        }

        public void OnSpeedExceeded(string[] adviceList, bool speedExceeded)
        {

        }

        public void OnTunnelEvent(bool tunnelEntered)
        {

        }

        public void OnUpdateNavigationState(SKNavigationState navigationState)
        {

        }

        public void OnVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState navigationState)
        {

        }

        public void OnAllRoutesCompleted()
        {

        }

        public void OnOnlineRouteComputationHanging(int timeSinceTheHangingBehaviorStarted)
        {

        }

        public void OnRouteCalculationCompleted(int statusMessage, int routeDistance, int routeEta, bool thisRouteIsComplete, int id)
        {
            if (statusMessage != SKRouteListener.RouteSuccess)
            {
                RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.route_calculation_failed), ToastLength.Short).Show());
                return;
            }

            if (_currentMapOption == MapOption.AlternativeRoutes)
            {
                RunOnUiThread(() =>
                {
                    int routeIndex = _routeIds.Count;
                    _routeIds.Add(id);

                    _altRoutesButtons[routeIndex].Text = DemoUtils.FormatDistance(routeDistance) + "\n" + DemoUtils.FormatTime(routeEta);

                    if (routeIndex == 0)
                    {
                        SelectAlternativeRoute(0);
                    }
                });
            }
            else if (_currentMapOption == MapOption.RoutingAndNavigation || _currentMapOption == MapOption.POITracking)
            {
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(id);
                // zoom to the current route
                SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);

                if (_currentMapOption == MapOption.RoutingAndNavigation)
                {
                    RunOnUiThread(() => _bottomButton.Text = Resources.GetString(Resource.String.start_navigation));
                }
                else if (_currentMapOption == MapOption.POITracking)
                {
                    _poiTrackingManager.StartPOITrackerWithRadius(10000, 0.5);
                    _poiTrackingManager.AddWarningRulesforPoiType(SKTrackablePOIType.Speedcam);
                    LaunchNavigation();
                }
            }
            else if (_currentMapOption == MapOption.Tracks)
            {

                RunOnUiThread(() =>
                {
                    SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);
                    _bottomButton.Visibility = ViewStates.Visible;
                    _bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
                });
            }
        }

        public void OnServerLikeRouteCalculationCompleted(int status)
        {

        }
    }
}