using System.Collections.Generic;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Routing;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsNavigationConfiguration
    {


        /// <summary>
        /// The start coordinate of the route
        /// </summary>
        private SKCoordinate _startCoordinate;

        /// <summary>
        /// The destination coordinate of the route
        /// </summary>
        private SKCoordinate _destinationCoordinate;

        /// <summary>
        /// The via point coordinate list of the route
        /// </summary>
        private IList<SKViaPoint> _viaPointCoordinateList;

        /// <summary>
        /// Desired style to use during the day.
        /// </summary>
        private SKMapViewStyle _dayStyle;

        /// <summary>
        /// Desired style to use during the night.
        /// </summary>
        private SKMapViewStyle _nightStyle;

        /// <summary>
        /// The route mode. Default is Simulation.
        /// </summary>
        private SKRouteSettings.SKRouteMode _routeType;

        /// <summary>
        /// Desired distance format.
        /// </summary>
        private SKMaps.SKDistanceUnitType _distanceUnitType;

        /// <summary>
        /// speed warning in city in m/s
        /// </summary>
        private double _speedWarningThresholdInCity;

        /// <summary>
        /// speed warning outside city in m/s.
        /// </summary>
        private double _speedWarningThresholdOutsideCity;

        /// <summary>
        /// Enables automatic style switching according to time of day. Default is true.
        /// </summary>
        private bool _automaticDayNight;

        /// <summary>
        /// Indicates whether to avoid toll roads when calculating the route.
        /// </summary>
        private bool _tollRoadsAvoided;

        /// <summary>
        /// Indicates whether to avoid highways when calculating the route.
        /// </summary>
        private bool _highWaysAvoided;

        /// <summary>
        /// Indicates whether to avoid ferries when calculating the route
        /// </summary>
        private bool _ferriesAvoided;

        /// <summary>
        /// If true, free drive will be automatically started after reaching the destination.
        /// </summary>
        private bool _continueFreeDriveAfterNavigationEnd;

        /// <summary>
        /// Desired navigation type.
        /// </summary>
        private SKNavigationSettings.SKNavigationType _navigationType;

        /// <summary>
        /// The path from log file
        /// </summary>
        private string _freeDriveNavigationFilePath;

        public SKToolsNavigationConfiguration()
        {
            _viaPointCoordinateList = new List<SKViaPoint>();
            _routeType = SKRouteSettings.SKRouteMode.Efficient;
            _distanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
            _navigationType = SKNavigationSettings.SKNavigationType.Real;

            _automaticDayNight = true;
            _speedWarningThresholdInCity = 20.0;
            _speedWarningThresholdOutsideCity = 20.0;
            _tollRoadsAvoided = false;
            _highWaysAvoided = false;
            _ferriesAvoided = false;
            _continueFreeDriveAfterNavigationEnd = true;
            _freeDriveNavigationFilePath = "";
        }

        /// <summary>
        /// Sets the route mode used for route calculation.
        /// </summary>
        /// <param name="routeType"> </param>
        public virtual SKRouteSettings.SKRouteMode RouteType
        {
            set
            {
                _routeType = value;
            }
            get
            {
                return _routeType;
            }
        }


        /// <returns> the start coordinate of the route. </returns>
        public virtual SKCoordinate StartCoordinate
        {
            get
            {
                return _startCoordinate;
            }
            set
            {
                _startCoordinate = value;
            }
        }


        /// <returns> the destination coordinate of the route </returns>
        public virtual SKCoordinate DestinationCoordinate
        {
            get
            {
                return _destinationCoordinate;
            }
            set
            {
                _destinationCoordinate = value;
            }
        }


        /// <returns> the via point coordinate list of the route </returns>
        public virtual IList<SKViaPoint> ViaPointCoordinateList
        {
            get
            {
                return _viaPointCoordinateList;
            }
            set
            {
                _viaPointCoordinateList = value;
            }
        }


        /// <returns> the day style. </returns>
        public virtual SKMapViewStyle DayStyle
        {
            get
            {
                return _dayStyle;
            }
            set
            {
                _dayStyle = value;
            }
        }


        /// <returns> the night style. </returns>
        public virtual SKMapViewStyle NightStyle
        {
            get
            {
                return _nightStyle;
            }
            set
            {
                _nightStyle = value;
            }
        }


        /// <returns> the distance unit. </returns>
        public virtual SKMaps.SKDistanceUnitType DistanceUnitType
        {
            get
            {
                return _distanceUnitType;
            }
            set
            {
                _distanceUnitType = value;
            }
        }


        /// <returns> the threshold for speed warning callback inside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdInCity
        {
            get
            {
                return _speedWarningThresholdInCity;
            }
            set
            {
                _speedWarningThresholdInCity = value;
            }
        }


        /// <returns> the threshold for speed warning callback outside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdOutsideCity
        {
            get
            {
                return _speedWarningThresholdOutsideCity;
            }
            set
            {
                _speedWarningThresholdOutsideCity = value;
            }
        }


        /// <returns> boolean that indicates whether day night algorithm is taken into consideration. </returns>
        public virtual bool AutomaticDayNight
        {
            get
            {
                return _automaticDayNight;
            }
            set
            {
                _automaticDayNight = value;
            }
        }


        /// <returns> boolean that indicates whether to continue free drive after real navigation ends. </returns>
        public virtual bool ContinueFreeDriveAfterNavigationEnd
        {
            get
            {
                return _continueFreeDriveAfterNavigationEnd;
            }
            set
            {
                _continueFreeDriveAfterNavigationEnd = value;
            }
        }


        /// <returns> the navigation type </returns>
        public virtual SKNavigationSettings.SKNavigationType NavigationType
        {
            get
            {
                return _navigationType;
            }
            set
            {
                _navigationType = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool TollRoadsAvoided
        {
            get
            {
                return _tollRoadsAvoided;
            }
            set
            {
                _tollRoadsAvoided = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid highways roads
        /// when calculating the route. </returns>
        public virtual bool HighWaysAvoided
        {
            get
            {
                return _highWaysAvoided;
            }
            set
            {
                _highWaysAvoided = value;
            }
        }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool FerriesAvoided
        {
            get
            {
                return _ferriesAvoided;
            }
            set
            {
                _ferriesAvoided = value;
            }
        }


        /// <returns> the path to the file used to make free drive navigation. </returns>
        public virtual string FreeDriveNavigationFilePath
        {
            get
            {
                return _freeDriveNavigationFilePath;
            }
            set
            {
                _freeDriveNavigationFilePath = value;
            }
        }

    }
}