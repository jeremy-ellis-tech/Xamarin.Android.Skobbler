using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Locations;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.Util.Concurrent.Atomic;
using Skobbler.Ngx.Search;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    internal static class SKToolsUtils
    {

        /// <summary>
        /// the number of km/h in 1 m/s
        /// </summary>
        private const double SpeedInKilometres = 3.6;

        /// <summary>
        /// number of mi/h in 1 m/s
        /// </summary>
        private const double SpeedInMiles = 2.2369;

        /// <summary>
        /// the number of meters in a km
        /// </summary>
        private const int MetersInKm = 1000;

        /// <summary>
        /// the number of meters in a mile
        /// </summary>
        private const double MetersInMile = 1609.34;

        /// <summary>
        /// converter from meters to feet
        /// </summary>
        private const double MetersToFeet = 3.2808399;

        /// <summary>
        /// converter from meters to yards
        /// </summary>
        private const double MetersToYards = 1.0936133;

        /// <summary>
        /// the number of yards in a mile
        /// </summary>
        private const int YardsInMile = 1760;

        /// <summary>
        /// the number of feet in a yard
        /// </summary>
        private const int FeetInYard = 3;

        /// <summary>
        /// the number of feet in a mile
        /// </summary>
        private const int FeetInMile = 5280;

        /// <summary>
        /// the limit of feet where the distance should be converted into miles
        /// </summary>
        private const int LimitToMiles = 1500;

        private static readonly AtomicInteger SNextGeneratedId = new AtomicInteger(1);

        /// <summary>
        /// Get the configuration json file name according to the mapStyle parameter. </summary>
        /// <param name="mapStyle"> . Possible values are:
        /// <p/>
        /// <seealso cref="SKToolsMapOperationsManager#DAY_STYLE"/>
        /// <p/>
        /// <seealso cref="SKToolsMapOperationsManager#NIGHT_STYLE"/> </param>
        /// <returns> the name of the style file. ex: "daystyle.json" </returns>
        public static string GetStyleFileName(int mapStyle)
        {
            switch (mapStyle)
            {
                case SKToolsMapOperationsManager.DayStyle:
                    return "daystyle.json";
                case SKToolsMapOperationsManager.NightStyle:
                    return "nightstyle.json";
            }
            return null;
        }

        /// <summary>
        /// Gets the path for the style folder according to the mapStyle parameter. </summary>
        /// <param name="configuration"> </param>
        /// <param name="mapStyle"> Possible values are:
        /// <p/>
        /// <seealso cref="SKToolsMapOperationsManager#DAY_STYLE"/>
        /// <p/>
        /// <seealso cref="SKToolsMapOperationsManager#NIGHT_STYLE"/> </param>
        /// <returns> the full path to the style files folder. </returns>
        public static string GetMapStyleFilesFolderPath(SKToolsNavigationConfiguration configuration, int mapStyle)
        {
            switch (mapStyle)
            {
                case SKToolsMapOperationsManager.DayStyle:
                    return configuration.DayStyle.ResourceFolderPath;
                case SKToolsMapOperationsManager.NightStyle:
                    return configuration.NightStyle.ResourceFolderPath;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// Method used to convert speed from m/s into km/h or mi/h (according to the
        /// distance unit set from Setting option) </summary>
        /// <param name="initialSpeed"> - the speed in m/s </param>
        /// <param name="distanceUnitType"> </param>
        /// <returns> an int value for speed in km/h or mi/h </returns>
        public static int GetSpeedByUnit(double initialSpeed, SKMaps.SKDistanceUnitType distanceUnitType)
        {
            double tempSpeed = initialSpeed;
            if (distanceUnitType == SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                tempSpeed *= SpeedInKilometres;
            }
            else
            {
                tempSpeed *= SpeedInMiles;
            }
            return (int)Math.Round(tempSpeed);
        }

        /// <returns> speed text (km/h or mph) by distance unit </returns>
        public static string GetSpeedTextByUnit(Activity activity, SKMaps.SKDistanceUnitType distanceUnitType)
        {
            string currentSpeedUnit;
            if (distanceUnitType == SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                currentSpeedUnit = activity.Resources.GetString(Resource.String.kmh_label);
            }
            else
            {
                currentSpeedUnit = activity.Resources.GetString(Resource.String.mph_label);
            }
            return currentSpeedUnit;
        }


        /// <summary>
        /// Generate a value suitable for use in setId
        /// This value will not collide with ID values generated at build time by aapt for Resource.Id. </summary>
        /// <returns> a generated ID value </returns>
        public static int GenerateViewId()
        {
            if (Build.VERSION.SdkInt < Build.VERSION_CODES.JellyBeanMr1)
            {
                for (; ; )
                {
                    int result = SNextGeneratedId.Get();
                    // aapt-generated IDs have the high byte nonzero; clamp to the range under that.
                    int newValue = result + 1;
                    if (newValue > 0x00FFFFFF)
                    {
                        newValue = 1; // Roll over to 1, not 0.
                    }
                    if (SNextGeneratedId.CompareAndSet(result, newValue))
                    {
                        return result;
                    }
                }
            }
            return View.GenerateViewId();
        }

        /// <summary>
        /// Decodes a file given by its path to a Bitmap object </summary>
        /// <param name="pathToFile"> </param>
        /// <returns> the Bitmap object if the decoding was made succesfully and null if any errors appeared during the process </returns>
        public static Bitmap DecodeFileToBitmap(string pathToFile)
        {
            Bitmap decodedFile = null;
            try
            {
                decodedFile = BitmapFactory.DecodeFile(pathToFile);
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
            return decodedFile;
        }

        /// <summary>
        /// elapsed time in hours/minutes </summary>
        /// <returns> elapsed time as string </returns>
        public static string FormatTime(long elapsedTimeInSeconds)
        {
            string format = string.Format("%0{0:D}d", 2);
            StringBuilder time = new StringBuilder();
            time.Append(string.Format(format, elapsedTimeInSeconds / 3600)).Append(":").Append(string.Format(format, (elapsedTimeInSeconds % 3600) / 60));
            return time.ToString();
        }

        /// <summary>
        /// converts a distance given in meters to the according distance in yards </summary>
        /// <param name="distanceInMeters">
        /// @return </param>
        private static double DistanceInYards(double distanceInMeters)
        {
            if (distanceInMeters != -1)
            {
                return distanceInMeters *= MetersToYards;
            }
            return distanceInMeters;
        }

        /// <summary>
        /// converts a distance given in meters to the according distance in feet </summary>
        /// <param name="distanceInMeters">
        /// @return </param>
        private static double DistanceInFeet(double distanceInMeters)
        {
            if (distanceInMeters != -1)
            {
                return distanceInMeters * MetersToYards * FeetInYard;
            }
            return distanceInMeters;
        }

        /// <summary>
        /// Converts (to imperial units if necessary) and formats as string a
        /// distance value given in meters. </summary>
        /// <param name="distanceValue"> distance value in meters </param>
        /// <param name="activity"> activity object used to get the app preferences and the
        /// distance unit labels
        /// @return </param>
        public static string ConvertAndFormatDistance(double distanceValue, SKMaps.SKDistanceUnitType distanceUnitType, Activity activity)
        {

            if (distanceUnitType != SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                // convert meters to feet or yards if needed
                if (distanceUnitType == SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet)
                {
                    distanceValue = (float)DistanceInFeet(distanceValue);
                }
                else
                {
                    distanceValue = (float)DistanceInYards(distanceValue);
                }
            }

            string distanceValueText;

            if (distanceUnitType == SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters)
            {
                if (distanceValue >= MetersInKm)
                {
                    distanceValue /= MetersInKm;
                    if (distanceValue >= 10)
                    {
                        // if distance is >= 10 km => display distance without any
                        // decimals
                        distanceValueText = (Math.Round(distanceValue) + " " + activity.Resources.GetString(Resource.String.km_label));
                    }
                    else
                    {
                        // distance displayed in kilometers
                        distanceValueText = (((float)Math.Round(distanceValue * 10) / 10)) + " " + activity.Resources.GetString(Resource.String.km_label);
                    }
                }
                else
                {
                    // distance displayed in meters
                    distanceValueText = ((int)distanceValue) + " " + activity.Resources.GetString(Resource.String.meters_label);
                }
            }
            else if (distanceUnitType == SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet)
            {
                // if the distance in feet > 1500 => convert it in miles (FMA-2577)
                if (distanceValue >= LimitToMiles)
                {
                    distanceValue /= FeetInMile;
                    if (distanceValue >= 10)
                    {
                        // for routing if the distance is > 10 should be rounded to
                        // be an int; rounded distance displayed in miles
                        distanceValueText = (Math.Round(distanceValue) + " " + activity.Resources.GetString(Resource.String.mi_label));

                    }
                    else
                    {
                        // distance displayed in miles
                        distanceValueText = (((float)Math.Round(distanceValue * 10) / 10)) + " " + activity.Resources.GetString(Resource.String.mi_label);
                    }
                }
                else
                {
                    // distance displayed in feet
                    distanceValueText = ((int)distanceValue) + " " + activity.Resources.GetString(Resource.String.feet_label);
                }
            }
            else
            {
                if (distanceValue >= MetersInKm)
                {
                    distanceValue /= YardsInMile;
                    if (distanceValue >= 10)
                    {
                        distanceValueText = (Math.Round(distanceValue) + " " + activity.Resources.GetString(Resource.String.mi_label));
                    }
                    else
                    {
                        // distance displayed in miles
                        distanceValueText = (((float)Math.Round(distanceValue * 10) / 10)) + " " + activity.Resources.GetString(Resource.String.mi_label);
                    }
                }
                else
                {
                    // distance displayed in yards
                    distanceValueText = ((int)distanceValue) + " " + activity.Resources.GetString(Resource.String.yards_label);
                }
            }

            if ((distanceValueText != null) && distanceValueText.StartsWith("0 ", StringComparison.Ordinal))
            {
                return "";
            }
            return distanceValueText;

        }

        /// <summary>
        /// Returns the formatted address/vicinity of this place object (to be
        /// displayed in list items).
        /// @return
        /// </summary>
        public static string GetFormattedAddress(IList<SKSearchResultParent> parents)
        {
            if (parents != null)
            {
                StringBuilder builder = new StringBuilder();
                foreach (SKSearchResultParent skSearchResultParent in parents)
                {
                    builder.Append(skSearchResultParent.ParentName + " ");
                }

                string formattedAddress = builder.ToString().Replace("\n(\\s)*$", "");
                return formattedAddress.Trim().Equals("") ? "" : formattedAddress;
            }
            return "";
        }


        /// <summary>
        /// converts the distance given in feet/yards/miles/km to the according distance in meters </summary>
        /// <param name="distance"> </param>
        /// <param name="initialUnit">: 0 - feet
        /// 1 - yards
        /// 2 - mile
        /// 3 - km </param>
        /// <returns> distance in meters </returns>
        public static double DistanceInMeters(double distance, int initialUnit)
        {
            if (distance != -1)
            {
                switch (initialUnit)
                {
                    case 0:
                        return distance /= MetersToFeet;
                    case 1:
                        return distance /= MetersToYards;
                    case 2:
                        return distance *= MetersInMile;
                    case 3:
                        return distance *= MetersInKm;
                }
            }
            return distance;
        }

        /// <summary>
        /// Checks if the current device has a GPS module (hardware) </summary>
        /// <returns> true if the current device has GPS </returns>
        public static bool HasGpsModule(Context context)
        {
            LocationManager locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
            foreach (String provider in locationManager.AllProviders)
            {
                if (provider.Equals(LocationManager.GpsProvider))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if the current device has a  NETWORK module (hardware) </summary>
        /// <returns> true if the current device has NETWORK </returns>
        public static bool HasNetworkModule(Context context)
        {
            LocationManager locationManager = (LocationManager)context.GetSystemService(Context.LocationService);
            foreach (String provider in locationManager.AllProviders)
            {
                if (provider.Equals(LocationManager.NetworkProvider))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the display size in inches. </summary>
        /// <returns> the value in inches </returns>
        public static double GetDisplaySizeInches(Context context)
        {
            DisplayMetrics dm = context.Resources.DisplayMetrics;

            double x = Math.Pow(dm.WidthPixels / (double)dm.DensityDpi, 2);
            double y = Math.Pow(dm.HeightPixels / (double)dm.DensityDpi, 2);

            return Math.Sqrt(x + y);
        }
    }
}