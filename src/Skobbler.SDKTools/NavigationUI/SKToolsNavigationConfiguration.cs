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
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsNavigationConfiguration
    {


        /// <summary>
        /// The start coordinate of the route
        /// </summary>
        private SKCoordinate startCoordinate;

        /// <summary>
        /// The destination coordinate of the route
        /// </summary>
        private SKCoordinate destinationCoordinate;

        /// <summary>
        /// The via point coordinate list of the route
        /// </summary>
        private IList<SKViaPoint> viaPointCoordinateList;

        /// <summary>
        /// Desired style to use during the day.
        /// </summary>
        private SKMapViewStyle dayStyle;

        /// <summary>
        /// Desired style to use during the night.
        /// </summary>
        private SKMapViewStyle nightStyle;

        /// <summary>
        /// The route mode. Default is Simulation.
        /// </summary>
        private SKRouteSettings.SKRouteMode routeType;

        /// <summary>
        /// Desired distance format.
        /// </summary>
        private SKMaps.SKDistanceUnitType distanceUnitType;

        /// <summary>
        /// speed warning in city in m/s
        /// </summary>
        private double speedWarningThresholdInCity;

        /// <summary>
        /// speed warning outside city in m/s.
        /// </summary>
        private double speedWarningThresholdOutsideCity;

        /// <summary>
        /// Enables automatic style switching according to time of day. Default is true.
        /// </summary>
        private bool automaticDayNight;

        /// <summary>
        /// Indicates whether to avoid toll roads when calculating the route.
        /// </summary>
        private bool tollRoadsAvoided;

        /// <summary>
        /// Indicates whether to avoid highways when calculating the route.
        /// </summary>
        private bool highWaysAvoided;

        /// <summary>
        /// Indicates whether to avoid ferries when calculating the route
        /// </summary>
        private bool ferriesAvoided;

        /// <summary>
        /// If true, free drive will be automatically started after reaching the destination.
        /// </summary>
        private bool continueFreeDriveAfterNavigationEnd;

        /// <summary>
        /// Desired navigation type.
        /// </summary>
        private SKNavigationSettings.SKNavigationType navigationType;

        /// <summary>
        /// The path from log file
        /// </summary>
        private string freeDriveNavigationFilePath;

        public SKToolsNavigationConfiguration()
        {
            viaPointCoordinateList = new List<SKViaPoint>();
            routeType = SKRouteSettings.SKRouteMode.Efficient;
            distanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
            navigationType = SKNavigationSettings.SKNavigationType.Real;

            automaticDayNight = true;
            speedWarningThresholdInCity = 20.0;
            speedWarningThresholdOutsideCity = 20.0;
            tollRoadsAvoided = false;
            highWaysAvoided = false;
            ferriesAvoided = false;
            continueFreeDriveAfterNavigationEnd = true;
            freeDriveNavigationFilePath = "";
        }

        /// <summary>
        /// Sets the route mode used for route calculation.
        /// </summary>
        /// <param name="routeType"> </param>
        public virtual SKRouteSettings.SKRouteMode RouteType
        {
            set
            {
                this.routeType = value;
            }
            get
            {
                return routeType;
            }
        }


        /// <returns> the start coordinate of the route. </returns>
        public virtual SKCoordinate StartCoordinate
        {
            get
            {
                return startCoordinate;
            }
            set
            {
                this.startCoordinate = value;
            }
        }


        /// <returns> the destination coordinate of the route </returns>
        public virtual SKCoordinate DestinationCoordinate
        {
            get
            {
                return destinationCoordinate;
            }
            set
            {
                this.destinationCoordinate = value;
            }
        }


        /// <returns> the via point coordinate list of the route </returns>
        public virtual IList<SKViaPoint> ViaPointCoordinateList
        {
            get
            {
                return viaPointCoordinateList;
            }
            set
            {
                this.viaPointCoordinateList = value;
            }
        }


        /// <returns> the day style. </returns>
        public virtual SKMapViewStyle DayStyle
        {
            get
            {
                return dayStyle;
            }
            set
            {
                this.dayStyle = value;
            }
        }


        /// <returns> the night style. </returns>
        public virtual SKMapViewStyle NightStyle
        {
            get
            {
                return nightStyle;
            }
            set
            {
                this.nightStyle = value;
            }
        }


        /// <returns> the distance unit. </returns>
        public virtual SKMaps.SKDistanceUnitType DistanceUnitType
        {
            get
            {
                return distanceUnitType;
            }
            set
            {
                this.distanceUnitType = value;
            }
        }


        /// <returns> the threshold for speed warning callback inside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdInCity
        {
            get
            {
                return speedWarningThresholdInCity;
            }
            set
            {
                this.speedWarningThresholdInCity = value;
            }
        }


        /// <returns> the threshold for speed warning callback outside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdOutsideCity
        {
            get
            {
                return speedWarningThresholdOutsideCity;
            }
            set
            {
                this.speedWarningThresholdOutsideCity = value;
            }
        }


        /// <returns> boolean that indicates whether day night algorithm is taken into consideration. </returns>
        public virtual bool AutomaticDayNight
        {
            get
            {
                return automaticDayNight;
            }
            set
            {
                this.automaticDayNight = value;
            }
        }


        /// <returns> boolean that indicates whether to continue free drive after real navigation ends. </returns>
        public virtual bool ContinueFreeDriveAfterNavigationEnd
        {
            get
            {
                return continueFreeDriveAfterNavigationEnd;
            }
            set
            {
                this.continueFreeDriveAfterNavigationEnd = value;
            }
        }


        /// <returns> the navigation type </returns>
        public virtual SKNavigationSettings.SKNavigationType NavigationType
        {
            get
            {
                return navigationType;
            }
            set
            {
                this.navigationType = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool TollRoadsAvoided
        {
            get
            {
                return tollRoadsAvoided;
            }
            set
            {
                this.tollRoadsAvoided = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid highways roads
        /// when calculating the route. </returns>
        public virtual bool HighWaysAvoided
        {
            get
            {
                return highWaysAvoided;
            }
            set
            {
                this.highWaysAvoided = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool FerriesAvoided
        {
            get
            {
                return ferriesAvoided;
            }
            set
            {
                this.ferriesAvoided = value;
            }
        }


        /// <returns> the path to the file used to make free drive navigation. </returns>
        public virtual string FreeDriveNavigationFilePath
        {
            get
            {
                return freeDriveNavigationFilePath;
            }
            set
            {
                this.freeDriveNavigationFilePath = value;
            }
        }

    }
}