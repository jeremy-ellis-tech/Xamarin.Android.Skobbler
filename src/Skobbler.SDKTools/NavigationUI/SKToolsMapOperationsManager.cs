using Android.App;
using Android.Content.Res;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.SDKTools.NavigationUI.AutoNight;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// Singleton class that provides various methods for changing the state of the
    /// map.
    /// </summary>
    internal class SKToolsMapOperationsManager
    {

        /// <summary>
        /// default value to set full screen mode on different devices
        /// </summary>
        private const double FullScreenMinimalScreensize = 3.85;

        /// <summary>
        /// Day style
        /// </summary>
        public const sbyte DayStyle = 0;

        /// <summary>
        /// Night style
        /// </summary>
        public const sbyte NightStyle = 1;

        /// <summary>
        /// Other style
        /// </summary>
        public const sbyte OtherStyle = 2;

        /// <summary>
        /// Ids for annotations
        /// </summary>
        public const sbyte GreenPinIconId = 0;

        public const sbyte RedPinIconId = 1;

        public const sbyte GreyPinIconId = 3;

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        private static SKToolsMapOperationsManager _instance;

        /// <summary>
        /// the map surface view
        /// </summary>
        private SKMapSurfaceView _mapView;

        /// <summary>
        /// Last zoom before going in panning mode / overviewmode
        /// </summary>
        private float _zoomBeforeSwitch;

        /// <summary>
        /// Gets the <seealso cref="SKToolsMapOperationsManager"/> object
        /// @return
        /// </summary>
        public static SKToolsMapOperationsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SKToolsMapOperationsManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sets the map view, necessary for handling operations on it. </summary>
        /// <param name="mapView"> </param>
        public virtual void SetMapView(SKMapSurfaceView mapView)
        {
            _mapView = mapView;
        }

        /// <summary>
        /// draw the grey pin </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void DrawGreyPinOnMap(double longitude, double latitude)
        {
            CreateAnnotation(GreyPinIconId, SKAnnotation.SkAnnotationTypePurple, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Draws the starting point. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void DrawStartPoint(double longitude, double latitude)
        {
            CreateAnnotation(GreenPinIconId, SKAnnotation.SkAnnotationTypeGreen, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Draws the destination point. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void DrawDestinationPoint(double longitude, double latitude)
        {
            CreateAnnotation(RedPinIconId, SKAnnotation.SkAnnotationTypeRed, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Deletes the destination point.
        /// </summary>
        public virtual void DeleteDestinationPoint()
        {
            _mapView.DeleteAnnotation(RedPinIconId);
        }

        /// <summary>
        /// Draws the destiunation flag. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void DrawDestinationNavigationFlag(double longitude, double latitude)
        {
            CreateAnnotation(RedPinIconId, SKAnnotation.SkAnnotationTypeDestinationFlag, longitude, latitude, SKAnimationSettings.AnimationNone);
        }

        /// <summary>
        /// Creates an annotation with a specific id, location and type. </summary>
        /// <param name="id"> </param>
        /// <param name="type"> </param>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        /// <param name="annotationAnimationType"> </param>
        private void CreateAnnotation(int id, int type, double longitude, double latitude, SKAnimationSettings annotationAnimationType)
        {
            SKAnnotation annotation = new SKAnnotation(id);
            annotation.AnnotationType = type;
            annotation.Location = new SKCoordinate(longitude, latitude);
            _mapView.AddAnnotation(annotation, annotationAnimationType);
        }

        /// <summary>
        /// Sets map in overview mode.
        /// </summary>
        public virtual void SwitchToOverViewMode(Activity currentActivity, SKToolsNavigationConfiguration configuration)
        {
            _zoomBeforeSwitch = _mapView.ZoomLevel;
            ZoomToRoute(currentActivity);
            SKMapSettings mapSettings = _mapView.MapSettings;
            mapSettings.MapZoomingEnabled = true;
            mapSettings.MapRotationEnabled = false;
            mapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.NoneWithHeading;
            mapSettings.MapDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
            _mapView.RotateTheMapToNorth();
        }

        /// <summary>
        /// Sets map in panning mode.
        /// </summary>
        public virtual void StartPanningMode()
        {

            _zoomBeforeSwitch = _mapView.ZoomLevel;
            SKMapSettings mapSettings = _mapView.MapSettings;
            mapSettings.InertiaPanningEnabled = true;
            mapSettings.MapZoomingEnabled = true;
            mapSettings.MapRotationEnabled = true;
            _mapView.MapSettings.CompassPosition = new SKScreenPoint(5, 5);
            _mapView.MapSettings.CompassShown = true;
            _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.NoneWithHeading;
            _mapView.MapSettings.MapDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
        }


        /// <summary>
        /// Sets the map in navigation mode
        /// </summary>
        public virtual void SetMapInNavigationMode()
        {
            _mapView.SetZoom(_zoomBeforeSwitch);
            _mapView.MapSettings.MapZoomingEnabled = false;

            _mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.Navigation;
            SKPosition naviPosition = _mapView.GetCurrentGPSPosition(true);
            if (naviPosition != null)
            {
                _mapView.RotateMapWithAngle((float)naviPosition.Heading);
            }
        }

        /// <summary>
        /// Changes the map style from day -> night or night-> day
        /// </summary>
        public virtual void SwitchDayNightStyle(SKToolsNavigationConfiguration configuration, int mapStyle)
        {
            int fastSwitchStyleIndex;
            if (mapStyle == DayStyle)
            {
                fastSwitchStyleIndex = 0;
            }
            else
            {
                fastSwitchStyleIndex = 1;
            }
            _mapView.MapSettings.MapStyle = new SKMapViewStyle(SKToolsUtils.GetMapStyleFilesFolderPath(configuration, mapStyle), SKToolsUtils.GetStyleFileName(mapStyle));

            _mapView.SetFastSwitchStyle(fastSwitchStyleIndex);
        }


        /// <summary>
        /// Changes the map display from 3d-> 2d and vice versa
        /// </summary>
        public virtual void SwitchMapDisplayMode(SKMapSettings.SKMapDisplayMode displayMode)
        {
            _mapView.MapSettings.MapDisplayMode = displayMode;
        }

        /// <summary>
        /// Gets current map view styles: day/night/other.
        /// @return
        /// </summary>
        public virtual int CurrentMapStyle
        {
            get
            {
                int mapStyle;
                SKMapViewStyle currentMapStyle = _mapView.MapSettings.MapStyle;
                string dayStyleFileName = SKToolsUtils.GetStyleFileName(DayStyle);
                string nightStyleFileName = SKToolsUtils.GetStyleFileName(NightStyle);
                if (currentMapStyle.StyleFileName.Equals(dayStyleFileName))
                {
                    mapStyle = DayStyle;
                }
                else if (currentMapStyle.StyleFileName.Equals(nightStyleFileName))
                {
                    mapStyle = NightStyle;
                }
                else
                {
                    mapStyle = OtherStyle;
                }
                return mapStyle;
            }
        }

        /// <summary>
        /// Gets the map style before starting drive mode depending on autonight settings
        /// </summary>
        public virtual int GetMapStyleBeforeStartDriveMode(bool autoNightIsOn)
        {
            int currentMapStyle = CurrentMapStyle;

            if (autoNightIsOn)
            {
                int correctMapStyleWhenStartDriveMode = GetCorrectMapStyleForDriveModeWhenAutoNightIsOn(true);
                return correctMapStyleWhenStartDriveMode;
            }

            return currentMapStyle;
        }

        /// <summary>
        /// Gets the correct map style (day/night) when auto night is on. </summary>
        /// <param name="autoNightIsOn"></param>
        private int GetCorrectMapStyleForDriveModeWhenAutoNightIsOn(bool autoNightIsOn)
        {
            if (autoNightIsOn)
            {
                if (SKToolsLogicManager.LastUserPosition != null)
                {
                    SKToolsAutoNightManager.Instance.CalculateSunriseSunsetHours(SKToolsLogicManager.LastUserPosition.Latitude, SKToolsLogicManager.LastUserPosition.Longitude);

                    if (SKToolsAutoNightManager.Instance.CurrentTimeInSunriseSunsetLimit)
                    {
                        return DayStyle;
                    }
                    return NightStyle;
                }
            }
            return DayStyle;
        }

        /// <summary>
        /// Zooms to route.
        /// </summary>
        public virtual void ZoomToRoute(Activity currentActivity)
        {
            const int offsetPixelsTop = 100;
            if ((currentActivity.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeLarge || (currentActivity.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeXlarge)
            {
                // large and xlarge
                SKRouteManager.Instance.ZoomToRoute(1.3f, 1.5f, offsetPixelsTop, 10, 5, 5);
            }
            else if (SKToolsUtils.GetDisplaySizeInches(currentActivity) < FullScreenMinimalScreensize)
            {
                // small
                SKRouteManager.Instance.ZoomToRoute(1.3f, 2.5f, offsetPixelsTop, 10, 5, 5);
            }
            else
            {
                if (currentActivity.Resources.Configuration.Orientation == Orientation.Portrait)
                {
                    SKRouteManager.Instance.ZoomToRoute(1.3f, 2.2f, offsetPixelsTop, 10, 5, 5);
                }
                else
                {
                    SKRouteManager.Instance.ZoomToRoute(1.3f, 2.2f, 0, 10, 5, 5);
                }
            }
        }
    }
}