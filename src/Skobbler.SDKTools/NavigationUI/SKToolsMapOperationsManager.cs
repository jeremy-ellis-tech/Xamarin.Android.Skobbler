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
using Skobbler.Ngx.SDKTools.NavigationUI.AutoNight;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Positioner;

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
        private const double FULL_SCREEN_MINIMAL_SCREENSIZE = 3.85;

        /// <summary>
        /// Day style
        /// </summary>
        public const sbyte DAY_STYLE = 0;

        /// <summary>
        /// Night style
        /// </summary>
        public const sbyte NIGHT_STYLE = 1;

        /// <summary>
        /// Other style
        /// </summary>
        public const sbyte OTHER_STYLE = 2;

        /// <summary>
        /// Ids for annotations
        /// </summary>
        public const sbyte GREEN_PIN_ICON_ID = 0;

        public const sbyte RED_PIN_ICON_ID = 1;

        public const sbyte GREY_PIN_ICON_ID = 3;

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        private static SKToolsMapOperationsManager instance;

        /// <summary>
        /// the map surface view
        /// </summary>
        private SKMapSurfaceView mapView;

        /// <summary>
        /// Last zoom before going in panning mode / overviewmode
        /// </summary>
        private float zoomBeforeSwitch;

        /// <summary>
        /// Gets the <seealso cref="SKToolsMapOperationsManager"/> object
        /// @return
        /// </summary>
        public static SKToolsMapOperationsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SKToolsMapOperationsManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Sets the map view, necessary for handling operations on it. </summary>
        /// <param name="mapView"> </param>
        public virtual SKMapSurfaceView MapView
        {
            set
            {
                this.mapView = value;
            }
        }

        /// <summary>
        /// draw the grey pin </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void drawGreyPinOnMap(double longitude, double latitude)
        {
            createAnnotation(GREY_PIN_ICON_ID, SKAnnotation.SkAnnotationTypePurple, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Draws the starting point. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void drawStartPoint(double longitude, double latitude)
        {
            createAnnotation(GREEN_PIN_ICON_ID, SKAnnotation.SkAnnotationTypeGreen, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Draws the destination point. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void drawDestinationPoint(double longitude, double latitude)
        {
            createAnnotation(RED_PIN_ICON_ID, SKAnnotation.SkAnnotationTypeRed, longitude, latitude, SKAnimationSettings.AnimationPinDrop);
        }

        /// <summary>
        /// Deletes the destination point.
        /// </summary>
        public virtual void deleteDestinationPoint()
        {
            mapView.DeleteAnnotation(RED_PIN_ICON_ID);
        }

        /// <summary>
        /// Draws the destiunation flag. </summary>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        public virtual void drawDestinationNavigationFlag(double longitude, double latitude)
        {
            createAnnotation(RED_PIN_ICON_ID, SKAnnotation.SkAnnotationTypeDestinationFlag, longitude, latitude, SKAnimationSettings.AnimationNone);
        }

        /// <summary>
        /// Creates an annotation with a specific id, location and type. </summary>
        /// <param name="id"> </param>
        /// <param name="type"> </param>
        /// <param name="longitude"> </param>
        /// <param name="latitude"> </param>
        /// <param name="annotationAnimationType"> </param>
        private void createAnnotation(int id, int type, double longitude, double latitude, SKAnimationSettings annotationAnimationType)
        {
            SKAnnotation annotation = new SKAnnotation(id);
            annotation.AnnotationType = type;
            annotation.Location = new SKCoordinate(longitude, latitude);
            mapView.AddAnnotation(annotation, annotationAnimationType);
        }

        /// <summary>
        /// Sets map in overview mode.
        /// </summary>
        public virtual void switchToOverViewMode(Activity currentActivity, SKToolsNavigationConfiguration configuration)
        {
            zoomBeforeSwitch = mapView.ZoomLevel;
            zoomToRoute(currentActivity);
            SKMapSettings mapSettings = mapView.MapSettings;
            mapSettings.MapZoomingEnabled = true;
            mapSettings.MapRotationEnabled = false;
            mapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.NoneWithHeading;
            mapSettings.MapDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
            mapView.RotateTheMapToNorth();
        }

        /// <summary>
        /// Sets map in panning mode.
        /// </summary>
        public virtual void startPanningMode()
        {

            zoomBeforeSwitch = mapView.ZoomLevel;
            SKMapSettings mapSettings = mapView.MapSettings;
            mapSettings.InertiaPanningEnabled = true;
            mapSettings.MapZoomingEnabled = true;
            mapSettings.MapRotationEnabled = true;
            mapView.MapSettings.CompassPosition = new SKScreenPoint(5, 5);
            mapView.MapSettings.CompassShown = true;
            mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.NoneWithHeading;
            mapView.MapSettings.MapDisplayMode = SKMapSettings.SKMapDisplayMode.Mode2d;
        }


        /// <summary>
        /// Sets the map in navigation mode
        /// </summary>
        public virtual void setMapInNavigationMode()
        {
            mapView.SetZoom(zoomBeforeSwitch);
            mapView.MapSettings.MapZoomingEnabled = false;

            mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.Navigation;
            SKPosition naviPosition = mapView.GetCurrentGPSPosition(true);
            if (naviPosition != null)
            {
                mapView.RotateMapWithAngle((float)naviPosition.Heading);
            }
        }

        /// <summary>
        /// Changes the map style from day -> night or night-> day
        /// </summary>
        public virtual void switchDayNightStyle(SKToolsNavigationConfiguration configuration, int mapStyle)
        {
            int fastSwitchStyleIndex;
            if (mapStyle == DAY_STYLE)
            {
                fastSwitchStyleIndex = 0;
            }
            else
            {
                fastSwitchStyleIndex = 1;
            }
            mapView.MapSettings.MapStyle = new SKMapViewStyle(SKToolsUtils.getMapStyleFilesFolderPath(configuration, mapStyle), SKToolsUtils.getStyleFileName(mapStyle));

            mapView.SetFastSwitchStyle(fastSwitchStyleIndex);
        }


        /// <summary>
        /// Changes the map display from 3d-> 2d and vice versa
        /// </summary>
        public virtual void switchMapDisplayMode(SKMapSettings.SKMapDisplayMode displayMode)
        {
            mapView.MapSettings.MapDisplayMode = displayMode;
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
                SKMapViewStyle currentMapStyle = mapView.MapSettings.MapStyle;
                string dayStyleFileName = SKToolsUtils.getStyleFileName(SKToolsMapOperationsManager.DAY_STYLE);
                string nightStyleFileName = SKToolsUtils.getStyleFileName(SKToolsMapOperationsManager.NIGHT_STYLE);
                if (currentMapStyle.StyleFileName.Equals(dayStyleFileName))
                {
                    mapStyle = SKToolsMapOperationsManager.DAY_STYLE;
                }
                else if (currentMapStyle.StyleFileName.Equals(nightStyleFileName))
                {
                    mapStyle = SKToolsMapOperationsManager.NIGHT_STYLE;
                }
                else
                {
                    mapStyle = SKToolsMapOperationsManager.OTHER_STYLE;
                }
                return mapStyle;
            }
        }

        /// <summary>
        /// Gets the map style before starting drive mode depending on autonight settings
        /// </summary>
        public virtual int getMapStyleBeforeStartDriveMode(bool autoNightIsOn)
        {
            int currentMapStyle = CurrentMapStyle;

            if (autoNightIsOn)
            {
                int correctMapStyleWhenStartDriveMode = getCorrectMapStyleForDriveModeWhenAutoNightIsOn(autoNightIsOn);
                return correctMapStyleWhenStartDriveMode;
            }
            else
            {
                return currentMapStyle;
            }
        }

        /// <summary>
        /// Gets the correct map style (day/night) when auto night is on. </summary>
        /// <param name="autoNightIsOn">
        /// @return </param>
        private int getCorrectMapStyleForDriveModeWhenAutoNightIsOn(bool autoNightIsOn)
        {
            if (autoNightIsOn)
            {
                if (SKToolsLogicManager.lastUserPosition != null)
                {
                    SKToolsAutoNightManager.Instance.calculateSunriseSunsetHours(SKToolsLogicManager.lastUserPosition.Latitude, SKToolsLogicManager.lastUserPosition.Longitude);

                    if (SKToolsAutoNightManager.Instance.CurrentTimeInSunriseSunsetLimit)
                    {
                        return DAY_STYLE;
                    }
                    else
                    {
                        return NIGHT_STYLE;
                    }
                }
            }
            return DAY_STYLE;
        }

        /// <summary>
        /// Zooms to route.
        /// </summary>
        public virtual void zoomToRoute(Activity currentActivity)
        {
            int offsetPixelsTop = 100;
            if ((currentActivity.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeLarge || (currentActivity.Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeXlarge)
            {
                // large and xlarge
                SKRouteManager.Instance.ZoomToRoute(1.3f, 1.5f, offsetPixelsTop, 10, 5, 5);
            }
            else if (SKToolsUtils.getDisplaySizeInches(currentActivity) < FULL_SCREEN_MINIMAL_SCREENSIZE)
            {
                // small
                SKRouteManager.Instance.ZoomToRoute(1.3f, 2.5f, offsetPixelsTop, 10, 5, 5);
            }
            else
            {
                if (currentActivity.Resources.Configuration.ScreenLayout == Configuration.ORIENTATION_PORTRAIT)
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