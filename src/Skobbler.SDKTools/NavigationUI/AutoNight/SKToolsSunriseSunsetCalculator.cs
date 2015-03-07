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
using Android.Util;
using Java.Util;

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    internal class SKToolsSunriseSunsetCalculator
    {

        private const string TAG = "SunriseSunsetCalculator";

        /// <summary>
        /// constants for Sun's zenith values for sunrise/sunset
        /// </summary>
        public const double OFFICIAL = 90.5;

        public const double CIVIL = 96.0;

        public const double NAUTICAL = 102.0;

        public const double ASTRONOMICAL = 108.0;

        public const int NR_OF_MILLISECONDS_IN_A_HOUR = 3600000;

        public static void calculateSunriseSunsetHours(double latitude, double longitude, double zenith)
        {
            calculateTime(latitude, longitude, zenith, true);
            calculateTime(latitude, longitude, zenith, false);
        }

        private static void calculateTime(double latitude, double longitude, double zenith, bool calculateSunrise)
        {

            double approximateTime;
            double meanAnomaly;
            double localHour;
            double universalTime;

            DateTime calendar = new DateTime();
            int currentYear = calendar.Year;
            int currentMonth = calendar.Month + 1;
            int currentDay = calendar.Day;

            // first calculate the day of the year
            double N1 = Math.Floor(275.0 * currentMonth / 9.0);
            double N2 = Math.Floor((currentMonth + 9.0) / 12.0);
            double N3 = (1 + Math.Floor((currentYear - 4.0 * Math.Floor(currentYear / 4.0) + 2.0) / 3.0));
            double dayOfYear = N1 - (N2 * N3) + currentDay - 30.0;

            // convert the longitude to hour value and calculate an approximate time
            double longHour = longitude / 15.0;
            if (calculateSunrise)
            {
                approximateTime = dayOfYear + ((6.0 - longHour) / 24.0);
            }
            else
            {
                approximateTime = dayOfYear + ((18.0 - longHour) / 24.0);
            }

            // calculate the Sun's mean anomaly

            meanAnomaly = (0.9856 * approximateTime) - 3.289;

            // calculate the Sun's true longitude

            double sunLongitude = meanAnomaly + (1.916 * Math.Sin(Math.toRadians(meanAnomaly))) + (0.020 * Math.Sin(2 * Math.toRadians(meanAnomaly))) + 282.634;

            sunLongitude = getNormalizedValue(sunLongitude, 360);

            // calculate the Sun's right ascension

            double sunRightAscension = Math.toDegrees(Math.Atan(0.91764 * Math.Tan(Math.toRadians(sunLongitude))));
            sunRightAscension = getNormalizedValue(sunRightAscension, 360);
            // right ascension value needs to be in the same quadrant as L

            double longitudeQuadrant = (Math.Floor(sunLongitude / 90.0)) * 90.0;
            double rightAscensionQuadrant = (Math.Floor(sunRightAscension / 90.0)) * 90.0;
            sunRightAscension = sunRightAscension + (longitudeQuadrant - rightAscensionQuadrant);

            // right ascension value needs to be converted into hours

            sunRightAscension = sunRightAscension / 15.0;

            // calculate the Sun's declination

            double sunSinDeclination = 0.39782 * Math.Sin(Math.toRadians(sunLongitude));
            double sunCosDeclination = Math.Cos(Math.Asin(sunSinDeclination));

            // calculate the Sun's local hour angle

            double cosLocalHour = (Math.Cos(Math.toRadians(zenith)) - (sunSinDeclination * Math.Sin(Math.toRadians(latitude)))) / (sunCosDeclination * Math.Cos(Math.toRadians(latitude)));

            if (cosLocalHour > 1)
            {
                return;

            }
            if (cosLocalHour < -1)
            {
                return;
            }

            // finish calculating localHour and convert into hours

            if (calculateSunrise)
            {
                localHour = 360.0 - Math.toDegrees(Math.Acos(cosLocalHour));
            }
            else
            {
                localHour = Math.toDegrees(Math.Acos(cosLocalHour));
            }
            localHour = localHour / 15.0;

            // calculate local mean time of rising/setting

            double localMeanTime = localHour + sunRightAscension - (0.06571 * approximateTime) - 6.622;

            // adjust back to UTC

            universalTime = localMeanTime - longHour;
            universalTime = getNormalizedValue(universalTime, 24);

            // convert UT value to local time zone of latitude/longitude

            int localOffset = CurrentTimezoneOffset;

            double localTime = getNormalizedValue(universalTime + localOffset, 24);


            int hour = (int)Math.Floor(localTime);
            int hourSeconds = (int)(3600 * (localTime - hour));
            int minute = hourSeconds / 60;

            if (calculateSunrise)
            {
                SKToolsDateUtils.AUTO_NIGHT_SUNRISE_HOUR = hour;
                SKToolsDateUtils.AUTO_NIGHT_SUNRISE_MINUTE = minute;
                Log.Debug(TAG, "Sunrise : " + SKToolsDateUtils.AUTO_NIGHT_SUNRISE_HOUR + ":" + SKToolsDateUtils.AUTO_NIGHT_SUNRISE_MINUTE);
            }
            else
            {
                SKToolsDateUtils.AUTO_NIGHT_SUNSET_HOUR = hour;
                SKToolsDateUtils.AUTO_NIGHT_SUNSET_MINUTE = minute;
                Log.Debug(TAG, "Sunset : " + SKToolsDateUtils.AUTO_NIGHT_SUNSET_HOUR + ":" + SKToolsDateUtils.AUTO_NIGHT_SUNSET_MINUTE);
            }
        }

        /// <summary>
        /// normalizes a value within a given range </summary>
        /// <param name="value"> </param>
        /// <param name="maxRange"> </param>
        /// <returns> normalize value </returns>
        private static double getNormalizedValue(double value, double maxRange)
        {
            while (value > maxRange)
            {
                value -= maxRange;
            }
            while (value < 0)
            {
                value += maxRange;
            }
            return value;
        }

        /// <summary>
        /// calculates current timezone offset </summary>
        /// <returns> the current timezone offset </returns>
        private static int CurrentTimezoneOffset
        {
            get
            {
                TimeZone timezone = TimeZone.Default;
                DateTime calendar = GregorianCalendar.getInstance(timezone);
                int offsetInMillis = timezone.getOffset(calendar.Ticks);
                int offset = offsetInMillis / NR_OF_MILLISECONDS_IN_A_HOUR;
                return offset;
            }
        }
    }
}