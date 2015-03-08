using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.SDKTools.NavigationUI.AutoNight;
using Skobbler.Ngx.Search;
using JavaObject = Java.Lang.Object;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// This class handles the logic related to the navigation and route calculation.
    /// </summary>
    public class SKToolsLogicManager : JavaObject, ISKMapSurfaceListener, ISKNavigationListener, ISKRouteListener, ISKCurrentPositionListener
    {

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static volatile SKToolsLogicManager _instance = null;

        /// <summary>
        /// the map view instance
        /// </summary>
        private SKMapSurfaceView _mapView;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity _currentActivity;

        /// <summary>
        /// Current position provider
        /// </summary>
        private SKCurrentPositionProvider _currentPositionProvider;

        /// <summary>
        /// Navigation manager
        /// </summary>
        private SKNavigationManager _naviManager;

        /// <summary>
        /// the initial configuration for calculating route and navigating
        /// </summary>
        private SKToolsNavigationConfiguration _configuration;

        /// <summary>
        /// number of options pressed (in navigation settings).
        /// </summary>
        public int NumberOfSettingsOptionsPressed;

        /// <summary>
        /// last audio advices that needs to be played when the visual advice is
        /// pressed
        /// </summary>
        private string[] _lastAudioAdvices;

        /// <summary>
        /// the distance left to the destination after every route update
        /// </summary>
        private long _navigationCurrentDistance;

        /// <summary>
        /// boolean value which shows if there are blocks on the route or not
        /// </summary>
        private bool _roadBlocked;

        /// <summary>
        /// flag for when a re-routing was done, which is set to true only until the
        /// next update of the navigation state
        /// </summary>
        private bool _reRoutingInProgress = false;

        /// <summary>
        /// the current location
        /// </summary>
        public static volatile SKPosition LastUserPosition;

        /// <summary>
        /// true, if the navigation was stopped
        /// </summary>
        private bool _navigationStopped;

        /// <summary>
        /// Map surface listener
        /// </summary>
        private ISKMapSurfaceListener _previousMapSurfaceListener;

        /// <summary>
        /// SKRouteInfo list
        /// </summary>
        private IList<SKRouteInfo> _skRouteInfoList = new List<SKRouteInfo>();

        /// <summary>
        /// Navigation listener
        /// </summary>
        private ISKToolsNavigationListener _navigationListener;
        /*
        Current map style
         */
        private SKMapViewStyle _currentMapStyle;

        /*
        Current display mode
         */
        private SKMapSettings.SKMapDisplayMode _currentUserDisplayMode;

        /// <summary>
        /// Creates a single instance of <seealso cref="SKToolsNavigationUiManager"/>
        /// 
        /// @return
        /// </summary>
        public static SKToolsLogicManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(SKToolsLogicManager))
                    {
                        if (_instance == null)
                        {
                            _instance = new SKToolsLogicManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private SKToolsLogicManager()
        {
            _naviManager = SKNavigationManager.Instance;
        }

        /// <summary>
        /// Sets the current activity.
        /// </summary>
        /// <param name="activity"> </param>
        /// <param name="rootId"> </param>
        protected internal virtual void SetActivity(Activity activity, int rootId)
        {
            this._currentActivity = activity;
            _currentPositionProvider = new SKCurrentPositionProvider(_currentActivity);
            if (SKToolsUtils.HasGpsModule(_currentActivity))
            {
                _currentPositionProvider.RequestLocationUpdates(true, false, true);
            }
            else if (SKToolsUtils.HasNetworkModule(_currentActivity))
            {
                _currentPositionProvider.RequestLocationUpdates(false, true, true);
            }
            _currentPositionProvider.SetCurrentPositionListener(this);
            SKToolsNavigationUiManager.Instance.SetActivity(_currentActivity, rootId);


        }

        /// <summary>
        /// Sets the listener.
        /// </summary>
        /// <param name="navigationListener"> </param>
        public virtual ISKToolsNavigationListener NavigationListener
        {
            set
            {
                this._navigationListener = value;
            }
        }

        /// <summary>
        /// Starts a route calculation.
        /// </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        protected internal virtual void CalculateRoute(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            this._mapView = mapView;
            this._configuration = configuration;
            SKToolsMapOperationsManager.Instance.MapView = mapView;
            _currentPositionProvider.RequestUpdateFromLastPosition();
            _currentMapStyle = mapView.MapSettings.MapStyle;
            SKRouteSettings route = new SKRouteSettings();
            route.StartCoordinate = configuration.StartCoordinate;
            route.DestinationCoordinate = configuration.DestinationCoordinate;
            SKToolsMapOperationsManager.Instance.DrawDestinationNavigationFlag(configuration.DestinationCoordinate.Longitude, configuration.DestinationCoordinate.Latitude);
            IList<SKViaPoint> viaPointList;
            viaPointList = configuration.ViaPointCoordinateList;
            if (viaPointList != null)
            {
                route.ViaPoints = viaPointList;
            }

            if (configuration.RouteType == SKRouteSettings.SKRouteMode.CarShortest)
            {
                route.NoOfRoutes = 1;
            }
            else
            {
                route.NoOfRoutes = 3;
            }

            route.RouteMode = configuration.RouteType;
            route.RouteExposed = true;
            route.TollRoadsAvoided = configuration.TollRoadsAvoided;
            route.AvoidFerries = configuration.FerriesAvoided;
            route.HighWaysAvoided = configuration.HighWaysAvoided;
            SKRouteManager.Instance.SetRouteListener(this);


            SKRouteManager.Instance.CalculateRoute(route);
            SKToolsNavigationUiManager.Instance.ShowPreNavigationScreen();


            if (configuration.AutomaticDayNight)
            {
                CheckCorrectMapStyle();

                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
                {
                    SKToolsAutoNightManager.Instance.SetAlarmForHourlyNotificationAfterKitKat(_currentActivity, true);
                }
                else
                {
                    SKToolsAutoNightManager.Instance.AlarmForHourlyNotification = _currentActivity;
                }
            }
            _navigationStopped = false;

            if (_navigationListener != null)
            {
                _navigationListener.OnRouteCalculationStarted();
            }
        }

        /// <summary>
        /// Starts a navigation with the specified configuration.
        /// </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapSurfaceView"> </param>
        /// <param name="isFreeDrive"> </param>
        protected internal virtual void StartNavigation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapSurfaceView, bool isFreeDrive)
        {

            _reRoutingInProgress = false;
            this._configuration = configuration;
            _mapView = mapSurfaceView;
            _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.Navigation;
            _currentUserDisplayMode = SKMapSettings.SKMapDisplayMode.Mode3d;
            _mapView.MapSettings.MapDisplayMode = _currentUserDisplayMode;
            _mapView.MapSettings.SetStreetNamePopupsShown(true);
            _mapView.MapSettings.MapZoomingEnabled = false;
            _previousMapSurfaceListener = _mapView.MapSurfaceListener;
            _mapView.MapSurfaceListener = this;
            SKToolsMapOperationsManager.Instance.MapView = _mapView;

            SKNavigationSettings navigationSettings = new SKNavigationSettings();
            navigationSettings.NavigationType = configuration.NavigationType;
            navigationSettings.PositionerVerticalAlignment = -0.25f;
            navigationSettings.ShowRealGPSPositions = false;
            navigationSettings.DistanceUnit = configuration.DistanceUnitType;
            navigationSettings.SpeedWarningThresholdInCity = configuration.SpeedWarningThresholdInCity;
            navigationSettings.SpeedWarningThresholdOutsideCity = configuration.SpeedWarningThresholdOutsideCity;
            if (configuration.NavigationType.Equals(SKNavigationSettings.SKNavigationType.File))
            {
                navigationSettings.FileNavigationPath = configuration.FreeDriveNavigationFilePath;
            }
            _naviManager.SetNavigationListener(this);
            _naviManager.SetMapView(_mapView);
            _naviManager.StartNavigation(navigationSettings);


            SKToolsNavigationUiManager.Instance.InflateNavigationViews(_currentActivity);
            SKToolsNavigationUiManager.Instance.Reset(configuration.DistanceUnitType);
            SKToolsNavigationUiManager.Instance.SetFollowerMode();
            if (configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation)
            {
                SKToolsNavigationUiManager.Instance.InflateSimulationViews();
            }
            if (isFreeDrive)
            {
                SKToolsNavigationUiManager.Instance.SetFreeDriveMode();
                _currentMapStyle = _mapView.MapSettings.MapStyle;
            }
            _currentActivity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            if (configuration.AutomaticDayNight && LastUserPosition != null)
            {
                if (isFreeDrive)
                {
                    CheckCorrectMapStyle();
                    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
                    {
                        SKToolsAutoNightManager.Instance.SetAlarmForHourlyNotificationAfterKitKat(_currentActivity, true);
                    }
                    else
                    {
                        SKToolsAutoNightManager.Instance.AlarmForHourlyNotification = _currentActivity;
                    }
                }
            }
            SKToolsNavigationUiManager.Instance.SwitchDayNightStyle(SKToolsMapOperationsManager.Instance.CurrentMapStyle);
            _navigationStopped = false;

            if (_navigationListener != null)
            {
                _navigationListener.OnNavigationStarted();
            }
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        protected internal virtual void StopNavigation()
        {
            SKToolsMapOperationsManager.Instance.StartPanningMode();
            _mapView.MapSettings.MapStyle = _currentMapStyle;
            _mapView.MapSettings.CompassShown = false;
            SKRouteManager.Instance.ClearCurrentRoute();
            _naviManager.StopNavigation();
            _currentPositionProvider.StopLocationUpdates();
            _mapView.MapSurfaceListener = _previousMapSurfaceListener;
            _mapView.RotateTheMapToNorth();
            _navigationStopped = true;

            _currentActivity.RunOnUiThread(() =>
            {
                SKToolsNavigationUiManager.Instance.RemoveNavigationViews();
                _currentActivity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            });

            if (_navigationListener != null)
            {
                _navigationListener.OnNavigationEnded();
            }

            SKToolsAdvicePlayer.Instance.Stop();
        }

        /// <summary>
        /// Checks the correct map style, taking into consideration auto night configuration settings.
        /// </summary>
        private void CheckCorrectMapStyle()
        {
            int currentMapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            int correctMapStyle = SKToolsMapOperationsManager.Instance.GetMapStyleBeforeStartDriveMode(_configuration.AutomaticDayNight);
            if (currentMapStyle != correctMapStyle)
            {
                SKToolsMapOperationsManager.Instance.SwitchDayNightStyle(_configuration, correctMapStyle);
                SKToolsNavigationUiManager.Instance.ChangePanelsBackgroundAndTextViewsColour(SKToolsMapOperationsManager.Instance.CurrentMapStyle);
            }
        }

        /// <summary>
        /// Checks if the navigation is stopped.
        /// 
        /// @return
        /// </summary>
        public virtual bool NavigationStopped
        {
            get
            {
                return _navigationStopped;
            }
        }

        /// <summary>
        /// Gets the current activity.
        /// 
        /// @return
        /// </summary>
        public virtual Activity CurrentActivity
        {
            get
            {
                return _currentActivity;
            }
        }

        /// <summary>
        /// Handles orientation changed.
        /// </summary>
        public virtual void NotifyOrientationChanged()
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            SKMapSettings.SKMapDisplayMode displayMode = _mapView.MapSettings.MapDisplayMode;
            SKToolsNavigationUiManager.Instance.HandleOrientationChanged(mapStyle, displayMode);
        }

        /// <summary>
        /// Handles the block roads list items click.
        /// </summary>
        /// <param name="parent"> </param>
        /// <param name="position"> </param>
        protected internal virtual void HandleBlockRoadsItemsClick(AdapterView parent, int position)
        {

            SKToolsNavigationUiManager.Instance.SetFollowerMode();
            SKToolsNavigationUiManager.Instance.ShowFollowerModePanels(_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);

            string item = (string)parent.GetItemAtPosition(position);
            if (item.Equals(_currentActivity.Resources.GetString(Resource.String.unblock_all)))
            {
                _naviManager.UnblockAllRoads();
                _roadBlocked = false;
            }
            else
            {
                // blockedDistance[0] - value, blockedDistance[1] - unit
                string[] blockedDistance = item.Split(' ');
                int distance;
                try
                {
                    distance = int.Parse(blockedDistance[0]);
                }
                catch (FormatException)
                {
                    distance = -1;
                }
                // set unit type based on blockDistance[1]
                int type = -1;
                if ("ft".Equals(blockedDistance[1]))
                {
                    type = 0;
                }
                else if ("yd".Equals(blockedDistance[1]))
                {
                    type = 1;
                }
                else if ("mi".Equals(blockedDistance[1]))
                {
                    type = 2;
                }
                else if ("km".Equals(blockedDistance[1]))
                {
                    type = 3;
                }

                _naviManager.BlockRoad(SKToolsUtils.DistanceInMeters(distance, type));
                _roadBlocked = true;
            }
        }

        /// <summary>
        /// Handles the items click.
        /// </summary>
        /// <param name="v"> </param>
        protected internal virtual void HandleItemsClick(View v)
        {
            int id = v.Id;

            if (id == Resource.Id.first_route || id == Resource.Id.second_route || id == Resource.Id.third_route)
            {

                int routeIndex = 0;
                if (id == Resource.Id.first_route)
                {
                    routeIndex = 0;
                }
                else if (id == Resource.Id.second_route)
                {
                    routeIndex = 1;
                }
                else if (id == Resource.Id.third_route)
                {
                    routeIndex = 2;
                }

                SKToolsMapOperationsManager.Instance.ZoomToRoute(_currentActivity);
                if (_skRouteInfoList.Count > routeIndex)
                {
                    int routeId = _skRouteInfoList[routeIndex].RouteID;
                    SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeId);
                    SKToolsNavigationUiManager.Instance.SelectAlternativeRoute(routeIndex);
                }
            }
            else if (id == Resource.Id.start_navigation_button)
            {
                SKToolsNavigationUiManager.Instance.RemovePreNavigationViews();
                SKRouteManager.Instance.ClearRouteAlternatives();
                _skRouteInfoList.Clear();
                StartNavigation(_configuration, _mapView, false);
            }
            else if (id == Resource.Id.navigation_top_back_button)
            {
                SKToolsMapOperationsManager.Instance.SetMapInNavigationMode();
                SKToolsNavigationUiManager.Instance.SetFollowerMode();
                SKToolsNavigationUiManager.Instance.ShowFollowerModePanels(_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
                _mapView.MapSettings.CompassShown = false;
                _mapView.MapSettings.MapZoomingEnabled = false;
                if (_currentUserDisplayMode != null)
                {
                    SKToolsMapOperationsManager.Instance.SwitchMapDisplayMode(_currentUserDisplayMode);
                }
            }
            else if (id == Resource.Id.cancel_pre_navigation_button)
            {
                RemoveRouteCalculationScreen();
            }
            else if (id == Resource.Id.menu_back_prenavigation_button)
            {
                SKToolsNavigationUiManager.Instance.HandleNavigationBackButton();
            }
            else if (id == Resource.Id.navigation_increase_speed)
            {
                SKNavigationManager.Instance.IncreaseSimulationSpeed(3);
            }
            else if (id == Resource.Id.navigation_decrease_speed)
            {
                SKNavigationManager.Instance.DecreaseSimulationSpeed(3);
            }
            else if (id == Resource.Id.menu_back_follower_mode_button)
            {
                SKToolsNavigationUiManager.Instance.HandleNavigationBackButton();
            }
            else if (id == Resource.Id.navigation_bottom_right_estimated_panel || id == Resource.Id.navigation_bottom_right_arriving_panel)
            {
                SKToolsNavigationUiManager.Instance.SwitchEstimatedTime();
            }
            else if (id == Resource.Id.position_me_real_navigation_button)
            {
                if (LastUserPosition != null)
                {
                    _mapView.CenterMapOnCurrentPositionSmooth(15, 1000);
                }
                else
                {
                    Toast.MakeText(_currentActivity, _currentActivity.Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                }
            }
            else if (id == Resource.Id.current_advice_image_holder || id == Resource.Id.current_advice_text_holder)
            {
                PlayLastAdvice();
            }

        }

        /// <summary>
        /// Removes the pre navigation screen.
        /// </summary>
        protected internal virtual void RemoveRouteCalculationScreen()
        {
            SKToolsNavigationUiManager.Instance.RemovePreNavigationViews();
            SKRouteManager.Instance.ClearCurrentRoute();
            SKRouteManager.Instance.ClearRouteAlternatives();
            _skRouteInfoList.Clear();
            Console.WriteLine("------ current map style remove" + _mapView.MapSettings.MapStyle);
            _mapView.MapSettings.MapStyle = _currentMapStyle;
            SKToolsAutoNightManager.Instance.CancelAlarmForForHourlyNotification();

            if (_navigationListener != null)
            {
                _navigationListener.OnRouteCalculationCanceled();
            }
        }

        /// <summary>
        /// handles the click on different views
        /// </summary>
        /// <param name="v"> the current view on which the click is detected </param>
        protected internal virtual void HandleSettingsItemsClick(View v)
        {
            bool naviScreenSet = false;

            int id = v.Id;
            if (id == Resource.Id.navigation_settings_audio_button)
            {
                NumberOfSettingsOptionsPressed++;
                if (NumberOfSettingsOptionsPressed == 1)
                {
                    SKToolsNavigationUiManager.Instance.LoadAudioSettings();
                }
            }
            else if (id == Resource.Id.navigation_settings_day_night_mode_button)
            {
                NumberOfSettingsOptionsPressed++;
                if (NumberOfSettingsOptionsPressed == 1)
                {
                    LoadDayNightSettings(_configuration);
                }
            }
            else if (id == Resource.Id.navigation_settings_overview_button)
            {
                SKSearchResult destination = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(_configuration.DestinationCoordinate);
                if (destination != null)
                {
                    SKToolsMapOperationsManager.Instance.SwitchToOverViewMode(_currentActivity, _configuration);
                    SKToolsNavigationUiManager.Instance.ShowOverviewMode(SKToolsUtils.GetFormattedAddress(destination.ParentsList));
                    naviScreenSet = true;
                }
            }
            else if (id == Resource.Id.navigation_settings_route_info_button)
            {
                NumberOfSettingsOptionsPressed++;
                if (NumberOfSettingsOptionsPressed == 1)
                {
                    SKSearchResult startCoord = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(_configuration.StartCoordinate);
                    SKSearchResult destCoord = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(_configuration.DestinationCoordinate);
                    string startAdd = SKToolsUtils.GetFormattedAddress(startCoord.ParentsList);
                    string destAdd = SKToolsUtils.GetFormattedAddress(destCoord.ParentsList);
                    SKToolsNavigationUiManager.Instance.ShowRouteInfoScreen(startAdd, destAdd);
                    naviScreenSet = true;
                }
            }
            else if (id == Resource.Id.navigation_settings_roadblock_info_button)
            {
                naviScreenSet = true;

                if (!SKToolsNavigationUiManager.Instance.FreeDriveMode)
                {
                    SKToolsNavigationUiManager.Instance.ShowRoadBlockMode(_configuration.DistanceUnitType, _navigationCurrentDistance);
                }
                else
                {
                    SKToolsNavigationUiManager.Instance.ShowRouteInfoFreeDriveScreen();
                }

            }
            else if (id == Resource.Id.navigation_settings_panning_button)
            {
                SKToolsMapOperationsManager.Instance.StartPanningMode();
                SKToolsNavigationUiManager.Instance.ShowPanningMode(_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real);
                naviScreenSet = true;
            }
            else if (id == Resource.Id.navigation_settings_view_mode_button)
            {
                LoadMapDisplayMode();
            }
            else if (id == Resource.Id.navigation_settings_quit_button)
            {
                SKToolsNavigationUiManager.Instance.ShowExitNavigationDialog();
            }
            else if (id == Resource.Id.navigation_settings_back_button)
            {
                if (_currentUserDisplayMode != null)
                {
                    SKToolsMapOperationsManager.Instance.SwitchMapDisplayMode(_currentUserDisplayMode);
                }
            }

            SKToolsNavigationUiManager.Instance.HideSettingsPanel();
            NumberOfSettingsOptionsPressed = 0;

            if (!naviScreenSet)
            {
                SKToolsNavigationUiManager.Instance.SetFollowerMode();
                SKToolsNavigationUiManager.Instance.ShowFollowerModePanels(_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
            }
        }

        /// <summary>
        /// play the last advice
        /// </summary>
        protected internal virtual void PlayLastAdvice()
        {
            SKToolsAdvicePlayer.Instance.PlayAdvice(_lastAudioAdvices, SKToolsAdvicePlayer.PriorityUser);
        }

        /// <summary>
        /// Checks if the roads are blocked.
        /// 
        /// @return
        /// </summary>
        protected internal virtual bool RoadBlocked
        {
            get
            {
                return _roadBlocked;
            }
        }

        /// <summary>
        /// Changes the map style from day -> night or night-> day
        /// </summary>
        private void LoadDayNightSettings(SKToolsNavigationConfiguration configuration)
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            int newStyle;
            if (mapStyle == SKToolsMapOperationsManager.DayStyle)
            {
                newStyle = SKToolsMapOperationsManager.NightStyle;
            }
            else
            {
                newStyle = SKToolsMapOperationsManager.DayStyle;
            }

            SKToolsNavigationUiManager.Instance.SwitchDayNightStyle(newStyle);
            SKToolsMapOperationsManager.Instance.SwitchDayNightStyle(configuration, newStyle);
        }


        /// <summary>
        /// Decides the style in which the map needs to be changed next.
        /// </summary>
        public virtual void ComputeMapStyle(bool isDaytime)
        {
            Log.Debug("", "Update the map style after receiving the broadcast");
            int mapStyle;
            if (isDaytime)
            {
                mapStyle = SKToolsMapOperationsManager.DayStyle;
            }
            else
            {
                mapStyle = SKToolsMapOperationsManager.NightStyle;
            }
            SKToolsNavigationUiManager.Instance.SwitchDayNightStyle(mapStyle);
            SKToolsMapOperationsManager.Instance.SwitchDayNightStyle(_configuration, mapStyle);
        }


        /// <summary>
        /// Changes the map display from 3d-> 2d and vice versa
        /// </summary>
        private void LoadMapDisplayMode()
        {
            SKMapSettings.SKMapDisplayMode displayMode = _mapView.MapSettings.MapDisplayMode;
            SKMapSettings.SKMapDisplayMode newDisplayMode;
            if (displayMode == SKMapSettings.SKMapDisplayMode.Mode3d)
            {
                newDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
            }
            else
            {
                newDisplayMode = SKMapSettings.SKMapDisplayMode.Mode3d;
            }

            _currentUserDisplayMode = newDisplayMode;
            SKToolsNavigationUiManager.Instance.SwitchMapMode(newDisplayMode);
            SKToolsMapOperationsManager.Instance.SwitchMapDisplayMode(newDisplayMode);
        }

        public void OnActionPan()
        {
        }

        public void OnActionZoom()
        {
            float currentZoom = _mapView.ZoomLevel;
            if (currentZoom < 5)
            {
                // do not show the blue dot
                _mapView.MapSettings.CurrentPositionShown = false;
            }
            else
            {
                _mapView.MapSettings.CurrentPositionShown = true;
            }
        }

        public void OnSurfaceCreated()
        {
        }

        public void OnScreenOrientationChanged()
        {
        }

        public void OnMapRegionChanged(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public void OnMapRegionChangeEnded(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public void OnDoubleTap(SKScreenPoint skScreenPoint)
        {
        }

        public void OnSingleTap(SKScreenPoint skScreenPoint)
        {
            if (SKToolsNavigationUiManager.Instance.FollowerMode)
            {
                SKToolsNavigationUiManager.Instance.ShowSettingsMode();
            }
        }

        public void OnRotateMap()
        {
        }

        public void OnLongPress(SKScreenPoint skScreenPoint)
        {
        }

        public void OnInternetConnectionNeeded()
        {
        }

        public void OnMapActionDown(SKScreenPoint skScreenPoint)
        {
        }

        public void OnMapActionUp(SKScreenPoint skScreenPoint)
        {
        }

        public void OnPOIClusterSelected(SKPOICluster skpoiCluster)
        {
        }

        public void OnMapPOISelected(SKMapPOI skMapPoi)
        {
        }

        public void OnAnnotationSelected(SKAnnotation skAnnotation)
        {
        }

        public void OnCustomPOISelected(SKMapCustomPOI skMapCustomPoi)
        {
        }

        public void OnCompassSelected()
        {
        }

        public void OnCurrentPositionSelected()
        {
        }

        public void OnObjectSelected(int i)
        {
        }


        public void OnInternationalisationCalled(int i)
        {
        }

        public void OnBoundingBoxImageRendered(int i)
        {

        }

        public void OnGLInitializationError(string messsage)
        {

        }

        public void OnDestinationReached()
        {

            if (_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real && _configuration.ContinueFreeDriveAfterNavigationEnd)
            {
                _currentActivity.RunOnUiThread(() =>
                {
                    SKRouteManager.Instance.ClearCurrentRoute();
                    SKToolsMapOperationsManager.Instance.DeleteDestinationPoint();
                    SKToolsNavigationUiManager.Instance.SetFreeDriveMode();
                });
            }
            else
            {
                StopNavigation();
            }
        }

        public void OnSignalNewAdviceWithInstruction(string instruction)
        {
        }

        public void OnSignalNewAdviceWithAudioFiles(string[] audioFiles, bool specialSoundFile)
        {
            SKToolsAdvicePlayer.Instance.PlayAdvice(audioFiles, SKToolsAdvicePlayer.PriorityNavigation);
        }

        public void OnSpeedExceededWithAudioFiles(string[] adviceList, bool speedExceeded)
        {
            PlaySoundWhenSpeedIsExceeded(adviceList, speedExceeded);
        }

        /// <summary>
        /// play sound when the speed is exceeded
        /// </summary>
        /// <param name="adviceList">    - the advices that needs to be played </param>
        /// <param name="speedExceeded"> - true if speed is exceeded, false otherwise </param>
        private void PlaySoundWhenSpeedIsExceeded(string[] adviceList, bool speedExceeded)
        {
            if (!_navigationStopped)
            {
                _currentActivity.RunOnUiThread(() =>
                {
                    if (speedExceeded)
                    {
                        SKToolsAdvicePlayer.Instance.PlayAdvice(adviceList, SKToolsAdvicePlayer.PrioritySpeedWarning);
                    }
                    SKToolsNavigationUiManager.Instance.HandleSpeedExceeded(speedExceeded);
                });
            }
        }

        public void OnSpeedExceededWithInstruction(string instruction, bool speedExceeded)
        {
        }

        public void OnUpdateNavigationState(SKNavigationState skNavigationState)
        {
            _lastAudioAdvices = skNavigationState.GetCurrentAdviceAudioAdvices();
            _navigationCurrentDistance = (int)Math.Round(skNavigationState.DistanceToDestination);

            if (_reRoutingInProgress)
            {
                _reRoutingInProgress = false;

                _currentActivity.RunOnUiThread(() =>
                {
                    bool followerMode = SKToolsNavigationUiManager.Instance.FollowerMode;
                    if (followerMode)
                    {
                        SKToolsNavigationUiManager.Instance.ShowFollowerModePanels(_configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
                    }
                });
            }

            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            SKToolsNavigationUiManager.Instance.HandleNavigationState(skNavigationState, mapStyle);
        }

        public void OnReRoutingStarted()
        {
            if (SKToolsNavigationUiManager.Instance.FollowerMode)
            {
                _currentActivity.RunOnUiThread(() =>
                {
                    SKToolsNavigationUiManager.Instance.HideTopPanels();
                    SKToolsNavigationUiManager.Instance.HideBottomAndLeftPanels();
                    SKToolsNavigationUiManager.Instance.ShowReroutingPanel();
                    _reRoutingInProgress = true;
                });
            }
        }

        public void OnFreeDriveUpdated(string countryCode, string streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
        {

            if (SKToolsNavigationUiManager.Instance.FollowerMode)
            {
                int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
                SKToolsNavigationUiManager.Instance.HandleFreeDriveUpdated(countryCode, streetName, currentSpeed, speedLimit, _configuration.DistanceUnitType, mapStyle);
            }
        }

        public void OnVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState skNavigationState)
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            _currentActivity.RunOnUiThread(() => { SKToolsNavigationUiManager.Instance.SetTopPanelsBackgroundColour(mapStyle, firstVisualAdviceChanged, secondVisualAdviceChanged); });

        }

        public void OnTunnelEvent(bool b)
        {
        }

        public void OnRouteCalculationCompleted(SKRouteInfo skRouteInfo)
        {
            if (!skRouteInfo.CorridorDownloaded)
            {
                return;
            }
            _skRouteInfoList.Add(skRouteInfo);

        }

        public void OnRouteCalculationFailed(SKRouteListenerSKRoutingErrorCode skRoutingErrorCode)
        {
            SKToolsNavigationUiManager.Instance.ShowRouteCalculationFailedDialog(skRoutingErrorCode);
            _currentActivity.RunOnUiThread(() => { SKToolsNavigationUiManager.Instance.RemovePreNavigationViews(); });
        }

        public void OnAllRoutesCompleted()
        {
            if (_skRouteInfoList.Count > 0)
            {
                _currentActivity.RunOnUiThread(() =>
                {
                    if (SKToolsNavigationUiManager.Instance.PreNavigationMode)
                    {
                        SKToolsNavigationUiManager.Instance.ShowStartNavigationPanel();
                    }
                    for (int i = 0; i < _skRouteInfoList.Count; i++)
                    {
                        string time = SKToolsUtils.FormatTime(_skRouteInfoList[i].EstimatedTime);
                        string distance = SKToolsUtils.ConvertAndFormatDistance(_skRouteInfoList[i].Distance, _configuration.DistanceUnitType, _currentActivity);

                        SKToolsNavigationUiManager.Instance.SePreNavigationButtons(i, time, distance);
                    }

                    int routeId = _skRouteInfoList[0].RouteID;
                    SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeId);
                    SKToolsNavigationUiManager.Instance.SelectAlternativeRoute(0);
                    if (SKToolsNavigationUiManager.Instance.PreNavigationMode)
                    {
                        SKToolsMapOperationsManager.Instance.ZoomToRoute(_currentActivity);
                    }
                });
            }

            if (_navigationListener != null)
            {
                _navigationListener.OnRouteCalculationCompleted();
            }
        }

        public void OnServerLikeRouteCalculationCompleted(SKRouteJsonAnswer skRouteJsonAnswer)
        {
        }

        public void OnOnlineRouteComputationHanging(int i)
        {
        }

        public void OnCurrentPositionUpdate(SKPosition skPosition)
        {
            LastUserPosition = skPosition;
            if (_mapView != null && _configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real)
            {
                _mapView.ReportNewGPSPosition(skPosition);
            }
        }


        public void OnViaPointReached(int index)
        {
            _currentActivity.RunOnUiThread(() => { SKToolsNavigationUiManager.Instance.ShowViaPointPanel(); });
        }
    }
}