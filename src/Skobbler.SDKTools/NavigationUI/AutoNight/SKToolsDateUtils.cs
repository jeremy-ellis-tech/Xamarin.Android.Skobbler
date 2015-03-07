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
using Java.Text;

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    internal sealed class SKToolsDateUtils
    {

        /// <summary>
        /// the sunset hour (in 24h format) used for auto day / night mode option
        /// </summary>
        protected internal static int AUTO_NIGHT_SUNSET_HOUR;

        /// <summary>
        /// the sunrise minute (in 24h format) used for auto day / night mode option
        /// </summary>
        protected internal static int AUTO_NIGHT_SUNRISE_MINUTE;

        /// <summary>
        /// the sunset minute (in 24h format) used for auto day / night mode option
        /// </summary>
        protected internal static int AUTO_NIGHT_SUNSET_MINUTE;

        /// <summary>
        /// the sunrise hour (in 24h format) used for auto day / night mode option
        /// </summary>
        protected internal static int AUTO_NIGHT_SUNRISE_HOUR;

        /// <summary>
        /// Returns true if it is day time, false otherwise
        /// @return
        /// </summary>
        public static bool Daytime
        {
            get
            {
                return CurrentTimeInSunriseSunsetLimit;
            }
        }

        private SKToolsDateUtils()
        {
        }

        /// <summary>
        /// Checks if the current time of user's device is in sunrise sunset limit.
        /// @return
        /// </summary>
        public static bool CurrentTimeInSunriseSunsetLimit
        {
            get
            {
                int currentMinutes = MinuteOfDay + HourOfDay * 60;
                int lowerMinutes = AUTO_NIGHT_SUNRISE_HOUR * 60 + AUTO_NIGHT_SUNRISE_MINUTE;
                int upperMinutes = AUTO_NIGHT_SUNSET_HOUR * 60 + AUTO_NIGHT_SUNSET_MINUTE;
                return currentMinutes >= lowerMinutes && currentMinutes < upperMinutes;
            }
        }

        /// <summary>
        /// Returns the current hour of the day as set on the device.
        /// @return
        /// </summary>
        public static int HourOfDay
        {
            get
            {
                return DateTime.Now.Hour;
            }
        }

        /// <summary>
        /// Returns the current minute of the day as set on the device.
        /// @return
        /// </summary>
        public static int MinuteOfDay
        {
            get
            {
                return DateTime.Now.Minute;
            }
        }

    }
}