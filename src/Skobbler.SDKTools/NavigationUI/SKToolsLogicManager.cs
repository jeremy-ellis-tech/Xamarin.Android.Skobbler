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
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.SDKTools.NavigationUI.AutoNight;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Search;
using Java.Lang;
using Math = System.Math;
using Android.Util;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// This class handles the logic related to the navigation and route calculation.
    /// </summary>
    public class SKToolsLogicManager : ISKMapSurfaceListener, ISKNavigationListener, ISKRouteListener, ISKCurrentPositionListener
    {

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static volatile SKToolsLogicManager instance = null;

        /// <summary>
        /// the map view instance
        /// </summary>
        private SKMapSurfaceView mapView;

        /// <summary>
        /// the current activity
        /// </summary>
        private Activity currentActivity;

        /// <summary>
        /// Current position provider
        /// </summary>
        private SKCurrentPositionProvider currentPositionProvider;

        /// <summary>
        /// Navigation manager
        /// </summary>
        private SKNavigationManager naviManager;

        /// <summary>
        /// the initial configuration for calculating route and navigating
        /// </summary>
        private SKToolsNavigationConfiguration configuration;

        /// <summary>
        /// number of options pressed (in navigation settings).
        /// </summary>
        public int numberOfSettingsOptionsPressed;

        /// <summary>
        /// last audio advices that needs to be played when the visual advice is
        /// pressed
        /// </summary>
        private string[] lastAudioAdvices;

        /// <summary>
        /// the distance left to the destination after every route update
        /// </summary>
        private long navigationCurrentDistance;

        /// <summary>
        /// boolean value which shows if there are blocks on the route or not
        /// </summary>
        private bool roadBlocked;

        /// <summary>
        /// flag for when a re-routing was done, which is set to true only until the
        /// next update of the navigation state
        /// </summary>
        private bool reRoutingInProgress = false;

        /// <summary>
        /// the current location
        /// </summary>
        public static volatile SKPosition lastUserPosition;

        /// <summary>
        /// true, if the navigation was stopped
        /// </summary>
        private bool navigationStopped;

        /// <summary>
        /// Map surface listener
        /// </summary>
        private ISKMapSurfaceListener previousMapSurfaceListener;

        /// <summary>
        /// SKRouteInfo list
        /// </summary>
        private IList<SKRouteInfo> skRouteInfoList = new List<SKRouteInfo>();

        /// <summary>
        /// Navigation listener
        /// </summary>
        private ISKToolsNavigationListener navigationListener;
        /*
        Current map style
         */
        private SKMapViewStyle currentMapStyle;

        /*
        Current display mode
         */
        private SKMapSettings.SKMapDisplayMode currentUserDisplayMode;

        /// <summary>
        /// Creates a single instance of <seealso cref="SKToolsNavigationUIManager"/>
        /// 
        /// @return
        /// </summary>
        public static SKToolsLogicManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (typeof(SKToolsLogicManager))
                    {
                        if (instance == null)
                        {
                            instance = new SKToolsLogicManager();
                        }
                    }
                }
                return instance;
            }
        }

        private SKToolsLogicManager()
        {
            naviManager = SKNavigationManager.Instance;
        }

        /// <summary>
        /// Sets the current activity.
        /// </summary>
        /// <param name="activity"> </param>
        /// <param name="rootId"> </param>
        protected internal virtual void setActivity(Activity activity, int rootId)
        {
            this.currentActivity = activity;
            currentPositionProvider = new SKCurrentPositionProvider(currentActivity);
            if (SKToolsUtils.hasGpsModule(currentActivity))
            {
                currentPositionProvider.RequestLocationUpdates(true, false, true);
            }
            else if (SKToolsUtils.hasNetworkModule(currentActivity))
            {
                currentPositionProvider.RequestLocationUpdates(false, true, true);
            }
            currentPositionProvider.CurrentPositionListener = this;
            SKToolsNavigationUIManager.Instance.setActivity(currentActivity, rootId);


        }

        /// <summary>
        /// Sets the listener.
        /// </summary>
        /// <param name="navigationListener"> </param>
        public virtual ISKToolsNavigationListener NavigationListener
        {
            set
            {
                this.navigationListener = value;
            }
        }

        /// <summary>
        /// Starts a route calculation.
        /// </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapView"> </param>
        protected internal virtual void calculateRoute(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapView)
        {
            this.mapView = mapView;
            this.configuration = configuration;
            SKToolsMapOperationsManager.Instance.MapView = mapView;
            currentPositionProvider.RequestUpdateFromLastPosition();
            currentMapStyle = mapView.MapSettings.MapStyle;
            SKRouteSettings route = new SKRouteSettings();
            route.StartCoordinate = configuration.StartCoordinate;
            route.DestinationCoordinate = configuration.DestinationCoordinate;
            SKToolsMapOperationsManager.Instance.drawDestinationNavigationFlag(configuration.DestinationCoordinate.Longitude, configuration.DestinationCoordinate.Latitude);
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

            route.RouteMode = Configuration.RouteType;
            route.RouteExposed = true;
            route.TollRoadsAvoided = configuration.TollRoadsAvoided;
            route.AvoidFerries = configuration.FerriesAvoided;
            route.HighWaysAvoided = configuration.HighWaysAvoided;
            SKRouteManager.Instance.SetRouteListener(this);


            SKRouteManager.Instance.CalculateRoute(route);
            SKToolsNavigationUIManager.Instance.showPreNavigationScreen();


            if (configuration.AutomaticDayNight)
            {
                checkCorrectMapStyle();

                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.KITKAT)
                {
                    SKToolsAutoNightManager.Instance.setAlarmForHourlyNotificationAfterKitKat(currentActivity, true);
                }
                else
                {
                    SKToolsAutoNightManager.Instance.AlarmForHourlyNotification = currentActivity;
                }
            }
            navigationStopped = false;

            if (navigationListener != null)
            {
                navigationListener.OnRouteCalculationStarted();
            }
        }

        /// <summary>
        /// Starts a navigation with the specified configuration.
        /// </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapSurfaceView"> </param>
        /// <param name="isFreeDrive"> </param>
        protected internal virtual void startNavigation(SKToolsNavigationConfiguration configuration, SKMapSurfaceView mapSurfaceView, bool isFreeDrive)
        {

            reRoutingInProgress = false;
            this.configuration = configuration;
            mapView = mapSurfaceView;
            mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.Navigation;
            currentUserDisplayMode = SKMapSettings.SKMapDisplayMode.Mode3d;
            mapView.MapSettings.MapDisplayMode = currentUserDisplayMode;
            mapView.MapSettings.SetStreetNamePopupsShown(true);
            mapView.MapSettings.MapZoomingEnabled = false;
            previousMapSurfaceListener = mapView.MapSurfaceListener;
            mapView.MapSurfaceListener = this;
            SKToolsMapOperationsManager.Instance.MapView = mapView;

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
            naviManager.SetNavigationListener(this);
            naviManager.SetMapView(mapView);
            naviManager.StartNavigation(navigationSettings);


            SKToolsNavigationUIManager.Instance.inflateNavigationViews(currentActivity);
            SKToolsNavigationUIManager.Instance.reset(configuration.DistanceUnitType);
            SKToolsNavigationUIManager.Instance.setFollowerMode();
            if (configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation)
            {
                SKToolsNavigationUIManager.Instance.inflateSimulationViews();
            }
            if (isFreeDrive)
            {
                SKToolsNavigationUIManager.Instance.setFreeDriveMode();
                currentMapStyle = mapView.MapSettings.MapStyle;
            }
            currentActivity.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            if (configuration.AutomaticDayNight && lastUserPosition != null)
            {
                if (isFreeDrive)
                {
                    checkCorrectMapStyle();
                    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.KITKAT)
                    {
                        SKToolsAutoNightManager.Instance.setAlarmForHourlyNotificationAfterKitKat(currentActivity, true);
                    }
                    else
                    {
                        SKToolsAutoNightManager.Instance.AlarmForHourlyNotification = currentActivity;
                    }
                }
            }
            SKToolsNavigationUIManager.Instance.switchDayNightStyle(SKToolsMapOperationsManager.Instance.CurrentMapStyle);
            navigationStopped = false;

            if (navigationListener != null)
            {
                navigationListener.OnNavigationStarted();
            }
        }

        /// <summary>
        /// Stops the navigation.
        /// </summary>
        protected internal virtual void stopNavigation()
        {
            SKToolsMapOperationsManager.Instance.startPanningMode();
            mapView.MapSettings.MapStyle = currentMapStyle;
            mapView.MapSettings.CompassShown = false;
            SKRouteManager.Instance.ClearCurrentRoute();
            naviManager.StopNavigation();
            currentPositionProvider.StopLocationUpdates();
            mapView.MapSurfaceListener = previousMapSurfaceListener;
            mapView.RotateTheMapToNorth();
            navigationStopped = true;

            currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper(this));

            if (navigationListener != null)
            {
                navigationListener.OnNavigationEnded();
            }

            SKToolsAdvicePlayer.Instance.stop();
        }

        private class RunnableAnonymousInnerClassHelper : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                SKToolsNavigationUIManager.Instance.removeNavigationViews();
                outerInstance.currentActivity.Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        /// <summary>
        /// Checks the correct map style, taking into consideration auto night configuration settings.
        /// </summary>
        private void checkCorrectMapStyle()
        {
            int currentMapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            int correctMapStyle = SKToolsMapOperationsManager.Instance.getMapStyleBeforeStartDriveMode(configuration.AutomaticDayNight);
            if (currentMapStyle != correctMapStyle)
            {
                SKToolsMapOperationsManager.Instance.switchDayNightStyle(configuration, correctMapStyle);
                SKToolsNavigationUIManager.Instance.changePanelsBackgroundAndTextViewsColour(SKToolsMapOperationsManager.Instance.CurrentMapStyle);
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
                return navigationStopped;
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
                return currentActivity;
            }
        }

        /// <summary>
        /// Handles orientation changed.
        /// </summary>
        public virtual void notifyOrientationChanged()
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            SKMapSettings.SKMapDisplayMode displayMode = mapView.MapSettings.MapDisplayMode;
            SKToolsNavigationUIManager.Instance.handleOrientationChanged(mapStyle, displayMode);
        }

        /// <summary>
        /// Handles the block roads list items click.
        /// </summary>
        /// <param name="parent"> </param>
        /// <param name="position"> </param>
        protected internal virtual void handleBlockRoadsItemsClick<T1>(AdapterView<T1> parent, int position)
        {

            SKToolsNavigationUIManager.Instance.setFollowerMode();
            SKToolsNavigationUIManager.Instance.showFollowerModePanels(configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);

            string item = (string)parent.GetItemAtPosition(position);
            if (item.Equals(currentActivity.Resources.GetString(Resource.String.unblock_all)))
            {
                naviManager.UnblockAllRoads();
                roadBlocked = false;
            }
            else
            {
                // blockedDistance[0] - value, blockedDistance[1] - unit
                string[] blockedDistance = item.Split(" ", true);
                int distance;
                try
                {
                    distance = int.Parse(blockedDistance[0]);
                }
                catch (System.FormatException)
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

                naviManager.BlockRoad(SKToolsUtils.distanceInMeters(distance, type));
                roadBlocked = true;
            }
        }

        /// <summary>
        /// Handles the items click.
        /// </summary>
        /// <param name="v"> </param>
        protected internal virtual void handleItemsClick(View v)
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

                SKToolsMapOperationsManager.Instance.zoomToRoute(currentActivity);
                if (skRouteInfoList.Count > routeIndex)
                {
                    int routeId = skRouteInfoList[routeIndex].RouteID;
                    SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeId);
                    SKToolsNavigationUIManager.Instance.selectAlternativeRoute(routeIndex);
                }
            }
            else if (id == Resource.Id.start_navigation_button)
            {
                SKToolsNavigationUIManager.Instance.removePreNavigationViews();
                SKRouteManager.Instance.ClearRouteAlternatives();
                skRouteInfoList.Clear();
                startNavigation(configuration, mapView, false);
            }
            else if (id == Resource.Id.navigation_top_back_button)
            {
                SKToolsMapOperationsManager.Instance.setMapInNavigationMode();
                SKToolsNavigationUIManager.Instance.setFollowerMode();
                SKToolsNavigationUIManager.Instance.showFollowerModePanels(configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
                mapView.MapSettings.CompassShown = false;
                mapView.MapSettings.MapZoomingEnabled = false;
                if (currentUserDisplayMode != null)
                {
                    SKToolsMapOperationsManager.Instance.SwitchMapDisplayMode(currentUserDisplayMode);
                }
            }
            else if (id == Resource.Id.cancel_pre_navigation_button)
            {
                removeRouteCalculationScreen();
            }
            else if (id == Resource.Id.menu_back_prenavigation_button)
            {
                SKToolsNavigationUIManager.Instance.handleNavigationBackButton();
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
                SKToolsNavigationUIManager.Instance.handleNavigationBackButton();
            }
            else if (id == Resource.Id.navigation_bottom_right_estimated_panel || id == Resource.Id.navigation_bottom_right_arriving_panel)
            {
                SKToolsNavigationUIManager.Instance.switchEstimatedTime();
            }
            else if (id == Resource.Id.position_me_real_navigation_button)
            {
                if (lastUserPosition != null)
                {
                    mapView.CenterMapOnCurrentPositionSmooth(15, 1000);
                }
                else
                {
                    Toast.MakeText(currentActivity, currentActivity.Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
                }
            }
            else if (id == Resource.Id.current_advice_image_holder || id == Resource.Id.current_advice_text_holder)
            {
                playLastAdvice();
            }

        }

        /// <summary>
        /// Removes the pre navigation screen.
        /// </summary>
        protected internal virtual void removeRouteCalculationScreen()
        {
            SKToolsNavigationUIManager.Instance.removePreNavigationViews();
            SKRouteManager.Instance.ClearCurrentRoute();
            SKRouteManager.Instance.ClearRouteAlternatives();
            skRouteInfoList.Clear();
            Console.WriteLine("------ current map style remove" + mapView.MapSettings.MapStyle);
            mapView.MapSettings.MapStyle = currentMapStyle;
            SKToolsAutoNightManager.Instance.cancelAlarmForForHourlyNotification();

            if (navigationListener != null)
            {
                navigationListener.OnRouteCalculationCanceled();
            }
        }

        /// <summary>
        /// handles the click on different views
        /// </summary>
        /// <param name="v"> the current view on which the click is detected </param>
        protected internal virtual void handleSettingsItemsClick(View v)
        {
            bool naviScreenSet = false;

            int id = v.Id;
            if (id == Resource.Id.navigation_settings_audio_button)
            {
                numberOfSettingsOptionsPressed++;
                if (numberOfSettingsOptionsPressed == 1)
                {
                    SKToolsNavigationUIManager.Instance.loadAudioSettings();
                }
            }
            else if (id == Resource.Id.navigation_settings_day_night_mode_button)
            {
                numberOfSettingsOptionsPressed++;
                if (numberOfSettingsOptionsPressed == 1)
                {
                    loadDayNightSettings(configuration);
                }
            }
            else if (id == Resource.Id.navigation_settings_overview_button)
            {
                SKSearchResult destination = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(configuration.DestinationCoordinate);
                if (destination != null)
                {
                    SKToolsMapOperationsManager.Instance.switchToOverViewMode(currentActivity, configuration);
                    SKToolsNavigationUIManager.Instance.showOverviewMode(SKToolsUtils.getFormattedAddress(destination.ParentsList));
                    naviScreenSet = true;
                }
            }
            else if (id == Resource.Id.navigation_settings_route_info_button)
            {
                numberOfSettingsOptionsPressed++;
                if (numberOfSettingsOptionsPressed == 1)
                {
                    SKSearchResult startCoord = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(configuration.StartCoordinate);
                    SKSearchResult destCoord = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(configuration.DestinationCoordinate);
                    string startAdd = SKToolsUtils.getFormattedAddress(startCoord.ParentsList);
                    string destAdd = SKToolsUtils.getFormattedAddress(destCoord.ParentsList);
                    SKToolsNavigationUIManager.Instance.showRouteInfoScreen(startAdd, destAdd);
                    naviScreenSet = true;
                }
            }
            else if (id == Resource.Id.navigation_settings_roadblock_info_button)
            {
                naviScreenSet = true;

                if (!SKToolsNavigationUIManager.Instance.FreeDriveMode)
                {
                    SKToolsNavigationUIManager.Instance.showRoadBlockMode(configuration.DistanceUnitType, navigationCurrentDistance);
                }
                else
                {
                    SKToolsNavigationUIManager.Instance.showRouteInfoFreeDriveScreen();
                }

            }
            else if (id == Resource.Id.navigation_settings_panning_button)
            {
                SKToolsMapOperationsManager.Instance.startPanningMode();
                SKToolsNavigationUIManager.Instance.showPanningMode(configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real);
                naviScreenSet = true;
            }
            else if (id == Resource.Id.navigation_settings_view_mode_button)
            {
                loadMapDisplayMode();
            }
            else if (id == Resource.Id.navigation_settings_quit_button)
            {
                SKToolsNavigationUIManager.Instance.showExitNavigationDialog();
            }
            else if (id == Resource.Id.navigation_settings_back_button)
            {
                if (currentUserDisplayMode != null)
                {
                    SKToolsMapOperationsManager.Instance.switchMapDisplayMode(currentUserDisplayMode);
                }
            }

            SKToolsNavigationUIManager.Instance.hideSettingsPanel();
            numberOfSettingsOptionsPressed = 0;

            if (!naviScreenSet)
            {
                SKToolsNavigationUIManager.Instance.setFollowerMode();
                SKToolsNavigationUIManager.Instance.showFollowerModePanels(configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
            }
        }

        /// <summary>
        /// play the last advice
        /// </summary>
        protected internal virtual void playLastAdvice()
        {
            SKToolsAdvicePlayer.Instance.playAdvice(lastAudioAdvices, SKToolsAdvicePlayer.PRIORITY_USER);
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
                return roadBlocked;
            }
        }

        /// <summary>
        /// Changes the map style from day -> night or night-> day
        /// </summary>
        private void loadDayNightSettings(SKToolsNavigationConfiguration configuration)
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            int newStyle;
            if (mapStyle == SKToolsMapOperationsManager.DAY_STYLE)
            {
                newStyle = SKToolsMapOperationsManager.NIGHT_STYLE;
            }
            else
            {
                newStyle = SKToolsMapOperationsManager.DAY_STYLE;
            }

            SKToolsNavigationUIManager.Instance.switchDayNightStyle(newStyle);
            SKToolsMapOperationsManager.Instance.switchDayNightStyle(configuration, newStyle);
        }


        /// <summary>
        /// Decides the style in which the map needs to be changed next.
        /// </summary>
        public virtual void computeMapStyle(bool isDaytime)
        {
            Log.Debug("", "Update the map style after receiving the broadcast");
            int mapStyle;
            if (isDaytime)
            {
                mapStyle = SKToolsMapOperationsManager.DAY_STYLE;
            }
            else
            {
                mapStyle = SKToolsMapOperationsManager.NIGHT_STYLE;
            }
            SKToolsNavigationUIManager.Instance.switchDayNightStyle(mapStyle);
            SKToolsMapOperationsManager.Instance.switchDayNightStyle(configuration, mapStyle);
        }


        /// <summary>
        /// Changes the map display from 3d-> 2d and vice versa
        /// </summary>
        private void loadMapDisplayMode()
        {
            SKMapSettings.SKMapDisplayMode displayMode = mapView.MapSettings.MapDisplayMode;
            SKMapSettings.SKMapDisplayMode newDisplayMode;
            if (displayMode == SKMapSettings.SKMapDisplayMode.Mode3d)
            {
                newDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
            }
            else
            {
                newDisplayMode = SKMapSettings.SKMapDisplayMode.Mode3d;
            }

            currentUserDisplayMode = newDisplayMode;
            SKToolsNavigationUIManager.Instance.switchMapMode(newDisplayMode);
            SKToolsMapOperationsManager.Instance.switchMapDisplayMode(newDisplayMode);
        }

        public override void onActionPan()
        {
        }

        public override void onActionZoom()
        {
            float currentZoom = mapView.ZoomLevel;
            if (currentZoom < 5)
            {
                // do not show the blue dot
                mapView.MapSettings.CurrentPositionShown = false;
            }
            else
            {
                mapView.MapSettings.CurrentPositionShown = true;
            }
        }

        public override void onSurfaceCreated()
        {
        }

        public override void onScreenOrientationChanged()
        {
        }

        public override void onMapRegionChanged(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public override void onMapRegionChangeStarted(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public override void onMapRegionChangeEnded(SKCoordinateRegion skCoordinateRegion)
        {
        }

        public override void onDoubleTap(SKScreenPoint skScreenPoint)
        {
        }

        public override void onSingleTap(SKScreenPoint skScreenPoint)
        {
            if (SKToolsNavigationUIManager.Instance.FollowerMode)
            {
                SKToolsNavigationUIManager.Instance.showSettingsMode();
            }
        }

        public override void onRotateMap()
        {
        }

        public override void onLongPress(SKScreenPoint skScreenPoint)
        {
        }

        public override void onInternetConnectionNeeded()
        {
        }

        public override void onMapActionDown(SKScreenPoint skScreenPoint)
        {
        }

        public override void onMapActionUp(SKScreenPoint skScreenPoint)
        {
        }

        public override void onPOIClusterSelected(SKPOICluster skpoiCluster)
        {
        }

        public override void onMapPOISelected(SKMapPOI skMapPOI)
        {
        }

        public override void onAnnotationSelected(SKAnnotation skAnnotation)
        {
        }

        public override void onCustomPOISelected(SKMapCustomPOI skMapCustomPOI)
        {
        }

        public override void onCompassSelected()
        {
        }

        public override void onCurrentPositionSelected()
        {
        }

        public override void onObjectSelected(int i)
        {
        }


        public override void onInternationalisationCalled(int i)
        {
        }

        public override void onBoundingBoxImageRendered(int i)
        {

        }

        public override void onGLInitializationError(string messsage)
        {

        }

        public override void onDestinationReached()
        {

            if (configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real && configuration.ContinueFreeDriveAfterNavigationEnd)
            {
                currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper2(this));
            }
            else
            {
                stopNavigation();
            }
        }

        private class RunnableAnonymousInnerClassHelper2 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper2(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                SKRouteManager.Instance.ClearCurrentRoute();
                SKToolsMapOperationsManager.Instance.deleteDestinationPoint();
                SKToolsNavigationUIManager.Instance.setFreeDriveMode();
            }
        }

        public override void onSignalNewAdviceWithInstruction(string instruction)
        {
        }

        public override void onSignalNewAdviceWithAudioFiles(string[] audioFiles, bool specialSoundFile)
        {
            SKToolsAdvicePlayer.Instance.playAdvice(audioFiles, SKToolsAdvicePlayer.PRIORITY_NAVIGATION);
        }

        public override void onSpeedExceededWithAudioFiles(string[] adviceList, bool speedExceeded)
        {
            playSoundWhenSpeedIsExceeded(adviceList, speedExceeded);
        }

        /// <summary>
        /// play sound when the speed is exceeded
        /// </summary>
        /// <param name="adviceList">    - the advices that needs to be played </param>
        /// <param name="speedExceeded"> - true if speed is exceeded, false otherwise </param>
        private void playSoundWhenSpeedIsExceeded(string[] adviceList, bool speedExceeded)
        {
            if (!navigationStopped)
            {
                currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper3(this, adviceList, speedExceeded));
            }
        }

        private class RunnableAnonymousInnerClassHelper3 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            private string[] adviceList;
            private bool speedExceeded;

            public RunnableAnonymousInnerClassHelper3(SKToolsLogicManager outerInstance, string[] adviceList, bool speedExceeded)
            {
                this.outerInstance = outerInstance;
                this.adviceList = adviceList;
                this.speedExceeded = speedExceeded;
            }


            public override void run()
            {

                if (speedExceeded)
                {
                    SKToolsAdvicePlayer.Instance.playAdvice(adviceList, SKToolsAdvicePlayer.PRIORITY_SPEED_WARNING);
                }
                SKToolsNavigationUIManager.Instance.handleSpeedExceeded(speedExceeded);
            }
        }

        public override void onSpeedExceededWithInstruction(string instruction, bool speedExceeded)
        {
        }

        public override void onUpdateNavigationState(SKNavigationState skNavigationState)
        {
            lastAudioAdvices = skNavigationState.GetCurrentAdviceAudioAdvices();
            navigationCurrentDistance = (int)Math.Round(skNavigationState.DistanceToDestination);

            if (reRoutingInProgress)
            {
                reRoutingInProgress = false;

                currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper4(this));
            }
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            SKToolsNavigationUIManager.Instance.handleNavigationState(skNavigationState, mapStyle);
        }

        private class RunnableAnonymousInnerClassHelper4 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper4(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {

                bool followerMode = SKToolsNavigationUIManager.Instance.FollowerMode;
                if (followerMode)
                {
                    SKToolsNavigationUIManager.Instance.showFollowerModePanels(outerInstance.configuration.NavigationType == SKNavigationSettings.SKNavigationType.Simulation);
                }
            }
        }

        public override void onReRoutingStarted()
        {
            if (SKToolsNavigationUIManager.Instance.FollowerMode)
            {
                currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper5(this));
            }
        }

        private class RunnableAnonymousInnerClassHelper5 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper5(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                SKToolsNavigationUIManager.Instance.hideTopPanels();
                SKToolsNavigationUIManager.Instance.hideBottomAndLeftPanels();
                SKToolsNavigationUIManager.Instance.showReroutingPanel();
                outerInstance.reRoutingInProgress = true;
            }
        }

        public override void onFreeDriveUpdated(string countryCode, string streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
        {

            if (SKToolsNavigationUIManager.Instance.FollowerMode)
            {
                int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
                SKToolsNavigationUIManager.Instance.handleFreeDriveUpdated(countryCode, streetName, currentSpeed, speedLimit, configuration.DistanceUnitType, mapStyle);
            }
        }

        public override void onVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState skNavigationState)
        {
            int mapStyle = SKToolsMapOperationsManager.Instance.CurrentMapStyle;
            currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper6(this, firstVisualAdviceChanged, secondVisualAdviceChanged, mapStyle));

        }

        private class RunnableAnonymousInnerClassHelper6 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            private bool firstVisualAdviceChanged;
            private bool secondVisualAdviceChanged;
            private int mapStyle;

            public RunnableAnonymousInnerClassHelper6(SKToolsLogicManager outerInstance, bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, int mapStyle)
            {
                this.outerInstance = outerInstance;
                this.firstVisualAdviceChanged = firstVisualAdviceChanged;
                this.secondVisualAdviceChanged = secondVisualAdviceChanged;
                this.mapStyle = mapStyle;
            }

            public override void run()
            {
                SKToolsNavigationUIManager.Instance.setTopPanelsBackgroundColour(mapStyle, firstVisualAdviceChanged, secondVisualAdviceChanged);
            }
        }

        public override void onTunnelEvent(bool b)
        {
        }

        public override void onRouteCalculationCompleted(SKRouteInfo skRouteInfo)
        {
            if (!skRouteInfo.CorridorDownloaded)
            {
                return;
            }
            skRouteInfoList.Add(skRouteInfo);

        }

        public override void onRouteCalculationFailed(SKRoutingErrorCode skRoutingErrorCode)
        {
            SKToolsNavigationUIManager.Instance.showRouteCalculationFailedDialog(skRoutingErrorCode);
            currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper7(this));
        }

        private class RunnableAnonymousInnerClassHelper7 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper7(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                SKToolsNavigationUIManager.Instance.removePreNavigationViews();
            }
        }

        public override void onAllRoutesCompleted()
        {
            if (skRouteInfoList.Count > 0)
            {
                currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper8(this));
            }

            if (navigationListener != null)
            {
                navigationListener.OnRouteCalculationCompleted();
            }
        }

        private class RunnableAnonymousInnerClassHelper8 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper8(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                if (SKToolsNavigationUIManager.Instance.PreNavigationMode)
                {
                    SKToolsNavigationUIManager.Instance.showStartNavigationPanel();
                }
                for (int i = 0; i < outerInstance.skRouteInfoList.Count; i++)
                {
                    string time = SKToolsUtils.formatTime(outerInstance.skRouteInfoList[i].EstimatedTime);
                    string distance = SKToolsUtils.convertAndFormatDistance(outerInstance.skRouteInfoList[i].Distance, outerInstance.configuration.DistanceUnitType, outerInstance.currentActivity);

                    SKToolsNavigationUIManager.Instance.sePreNavigationButtons(i, time, distance);
                }

                int routeId = outerInstance.skRouteInfoList[0].RouteID;
                SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeId);
                SKToolsNavigationUIManager.Instance.selectAlternativeRoute(0);
                if (SKToolsNavigationUIManager.Instance.PreNavigationMode)
                {
                    SKToolsMapOperationsManager.Instance.zoomToRoute(outerInstance.currentActivity);
                }

            }
        }

        public override void onServerLikeRouteCalculationCompleted(SKRouteJsonAnswer skRouteJsonAnswer)
        {
        }

        public override void onOnlineRouteComputationHanging(int i)
        {
        }

        public override void onCurrentPositionUpdate(SKPosition skPosition)
        {
            lastUserPosition = skPosition;
            if (mapView != null && configuration.NavigationType == SKNavigationSettings.SKNavigationType.Real)
            {
                mapView.ReportNewGPSPosition(skPosition);
            }
        }


        public override void onViaPointReached(int index)
        {
            currentActivity.RunOnUiThread(new RunnableAnonymousInnerClassHelper9(this));
        }

        private class RunnableAnonymousInnerClassHelper9 : IRunnable
        {
            private readonly SKToolsLogicManager outerInstance;

            public RunnableAnonymousInnerClassHelper9(SKToolsLogicManager outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void run()
            {
                SKToolsNavigationUIManager.Instance.showViaPointPanel();
            }
        }

    }
}