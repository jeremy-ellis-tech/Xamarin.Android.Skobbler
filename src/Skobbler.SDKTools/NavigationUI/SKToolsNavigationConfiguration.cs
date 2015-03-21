using System.Collections.Generic;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Routing;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsNavigationConfiguration
    {
        public SKToolsNavigationConfiguration()
        {
            ViaPointCoordinateList = new List<SKViaPoint>();
            RouteType = SKRouteSettings.SKRouteMode.Efficient;
            DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
            NavigationType = SKNavigationSettings.SKNavigationType.Real;

            AutomaticDayNight = true;
            SpeedWarningThresholdInCity = 20.0;
            SpeedWarningThresholdOutsideCity = 20.0;
            TollRoadsAvoided = false;
            HighWaysAvoided = false;
            FerriesAvoided = false;
            ContinueFreeDriveAfterNavigationEnd = true;
            FreeDriveNavigationFilePath = "";
        }

        /// <summary>
        /// Sets the route mode used for route calculation.
        /// </summary>
        public virtual SKRouteSettings.SKRouteMode RouteType { set; get; }


        /// <returns> the start coordinate of the route. </returns>
        public virtual SKCoordinate StartCoordinate { get; set; }


        /// <returns> the destination coordinate of the route </returns>
        public virtual SKCoordinate DestinationCoordinate { get; set; }


        /// <returns> the via point coordinate list of the route </returns>
        public virtual IList<SKViaPoint> ViaPointCoordinateList { get; set; }


        /// <returns> the day style. </returns>
        public virtual SKMapViewStyle DayStyle { get; set; }


        /// <returns> the night style. </returns>
        public virtual SKMapViewStyle NightStyle { get; set; }


        /// <returns> the distance unit. </returns>
        public virtual SKMaps.SKDistanceUnitType DistanceUnitType { get; set; }


        /// <returns> the threshold for speed warning callback inside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdInCity { get; set; }


        /// <returns> the threshold for speed warning callback outside cities in m/s. </returns>
        public virtual double SpeedWarningThresholdOutsideCity { get; set; }


        /// <returns> boolean that indicates whether day night algorithm is taken into consideration. </returns>
        public virtual bool AutomaticDayNight { get; set; }


        /// <returns> boolean that indicates whether to continue free drive after real navigation ends. </returns>
        public virtual bool ContinueFreeDriveAfterNavigationEnd { get; set; }


        /// <returns> the navigation type </returns>
        public virtual SKNavigationSettings.SKNavigationType NavigationType { get; set; }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool TollRoadsAvoided { get; set; }


        /// <returns> Returns a boolean that indicates whether to avoid highways roads
        /// when calculating the route. </returns>
        public virtual bool HighWaysAvoided { get; set; }


        /// <returns> Returns a boolean that indicates whether to avoid ferries when
        /// calculating the route. </returns>
        public virtual bool FerriesAvoided { get; set; }


        /// <returns> the path to the file used to make free drive navigation. </returns>
        public virtual string FreeDriveNavigationFilePath { get; set; }
    }
}