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

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    public class SKToolsAutoNightManager
    {

        private const string TAG = "SKToolsAutoNightManager";

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static SKToolsAutoNightManager instance;

        /// <summary>
        /// the alarm manager used to set the alarm manager for the hourly notification, when autonight is on
        /// </summary>
        private AlarmManager hourlyAlarmManager;

        /// <summary>
        /// the pending alarm intent for the hourly alarm manager
        /// </summary>
        private PendingIntent pendingHourlyAlarmIntent;

        /// <summary>
        /// true if the alarm for sunrise / sunset calculation was set, false otherwise
        /// </summary>
        public static bool wasSetAlarmForSunriseSunsetCalculation;

        /// <summary>
        /// the alarm manager used to set the alarm for the calculate sunrise / sunset hours
        /// </summary>
        private AlarmManager alarmManagerForAutoNightForCalculatedSunriseSunsetHours;

        /// <summary>
        /// the pending alarm intent for the alarm manager used to recalculate the sunrise / sunset hours
        /// </summary>
        private PendingIntent pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours;


        public static SKToolsAutoNightManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SKToolsAutoNightManager();
                }
                return instance;
            }
        }

        /// <summary>
        /// Sets the alarm and starts to listen for the times when an hour has passed, in the case when autonight is on. </summary>
        /// <param name="context"> </param>
        public virtual Context AlarmForHourlyNotification
        {
            set
            {
                if (hourlyAlarmManager == null)
                {
                    //if it already an existing alarm for hourly notification, cancel it
                    cancelAlarmForForHourlyNotification();
                    hourlyAlarmManager = (AlarmManager)value.GetSystemService(Context.AlarmService);
                    Intent intent = new Intent(value, typeof(SKToolsCalculateSunriseSunsetTimeAutoReceiver));
                    pendingHourlyAlarmIntent = PendingIntent.GetBroadcast(value, 0, intent, 0);

                    hourlyAlarmManager.SetRepeating(AlarmType.Rtc, DateTimeHelperClass.CurrentUnixTimeMillis(), SKToolsSunriseSunsetCalculator.NR_OF_MILLISECONDS_IN_A_HOUR, pendingHourlyAlarmIntent);

                }
            }
        }

        /// <summary>
        /// Sets the alarm and starts to listen for the times when an hour has passed, in the case when autonight is on. </summary>
        /// <param name="context"> </param>
        /// <param name="startNow"> start now or after an hour </param>
        public virtual void setAlarmForHourlyNotificationAfterKitKat(Context context, bool startNow)
        {
            if (hourlyAlarmManager == null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT)
            {
                //if it already an existing alarm for hourly notification, cancel it
                cancelAlarmForForHourlyNotification();
                hourlyAlarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
                Intent intent = new Intent(context, typeof(SKToolsCalculateSunriseSunsetTimeAutoReceiver));
                pendingHourlyAlarmIntent = PendingIntent.GetBroadcast(context, 0, intent, 0);

                long timeToStart = DateTimeHelperClass.CurrentUnixTimeMillis();
                if (!startNow)
                {
                    timeToStart += SKToolsSunriseSunsetCalculator.NR_OF_MILLISECONDS_IN_A_HOUR;
                }
                hourlyAlarmManager.SetExact(AlarmType.Rtc, timeToStart, pendingHourlyAlarmIntent);
            }
        }

        /// <summary>
        /// Cancels the alarm for hourly notification.
        /// </summary>

        public virtual void cancelAlarmForForHourlyNotification()
        {
            if (hourlyAlarmManager != null && pendingHourlyAlarmIntent != null)
            {
                hourlyAlarmManager.Cancel(pendingHourlyAlarmIntent);
                hourlyAlarmManager = null;
            }
        }

        /// <summary>
        /// Sets auto day / night alarm according to user position
        /// if the user position is null is set the alarm with fixed hours (8AM, 8PM),
        /// otherwise is set the alarm for calculation of sunrise / sunset hours. </summary>
        /// <param name="currentActivity"> </param>
        public virtual void setAutoNightAlarmAccordingToUserPosition(double latitude, double longitude, Activity currentActivity)
        {
            SKToolsSunriseSunsetCalculator.calculateSunriseSunsetHours(latitude, longitude, SKToolsSunriseSunsetCalculator.OFFICIAL);
            AlarmForDayNightModeWithSunriseSunset = currentActivity;
        }

        /// <summary>
        /// Sets the alarm for sunrise / sunset and starts to listen for the times of changing the map
        /// style (day/night).
        /// </summary>
        public virtual Context AlarmForDayNightModeWithSunriseSunset
        {
            set
            {
                Log.Debug(TAG, "setAlarmForDayNightModeWithSunriseSunset");
                // if there is already an existing alarm then cancel it before starting
                // a new one
                cancelAlarmForDayNightModeWithSunriseSunset();

                pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours = PendingIntent.GetBroadcast(value, 0, new Intent(value, typeof(SKToolsChangeMapStyleAutoReceiver)), 0);
                alarmManagerForAutoNightForCalculatedSunriseSunsetHours = (AlarmManager)value.GetSystemService(Context.AlarmService);

                DateTime date = DateTime.Now; // initializes to now
                DateTime mapStyleChangeCalendar = new DateTime();
                mapStyleChangeCalendar = new DateTime(date);
                wasSetAlarmForSunriseSunsetCalculation = true;
                if (!SKToolsDateUtils.Daytime)
                {
                    if (shouldSetAlarmNextDay())
                    {
                        mapStyleChangeCalendar.AddDays(1);
                    }
                    // set the hour for starting the day style
                    mapStyleChangeCalendar.Set(DateTime.HOUR_OF_DAY, SKToolsDateUtils.AUTO_NIGHT_SUNRISE_HOUR);
                    mapStyleChangeCalendar.Set(DateTime.MINUTE, SKToolsDateUtils.AUTO_NIGHT_SUNRISE_MINUTE);
                }
                else
                {
                    // set the hour for starting the night style
                    mapStyleChangeCalendar.set(DateTime.HOUR_OF_DAY, SKToolsDateUtils.AUTO_NIGHT_SUNSET_HOUR);
                    mapStyleChangeCalendar.set(DateTime.MINUTE, SKToolsDateUtils.AUTO_NIGHT_SUNSET_MINUTE);
                }
                mapStyleChangeCalendar.set(DateTime.SECOND, 0);

                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT)
                {
                    alarmManagerForAutoNightForCalculatedSunriseSunsetHours.setExact(AlarmManager.RTC, mapStyleChangeCalendar.Ticks, pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
                }
                else
                {
                    alarmManagerForAutoNightForCalculatedSunriseSunsetHours.set(AlarmManager.RTC, mapStyleChangeCalendar.Ticks, pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
                }

            }
        }

        /// <returns> true if the alarm for sunrise should be set next day, false otherwise </returns>
        private bool shouldSetAlarmNextDay()
        {
            int currentMinutes = SKToolsDateUtils.MinuteOfDay + SKToolsDateUtils.HourOfDay * 60;
            int upperMinutes = SKToolsDateUtils.AUTO_NIGHT_SUNRISE_HOUR * 60 + SKToolsDateUtils.AUTO_NIGHT_SUNRISE_MINUTE;
            return currentMinutes > upperMinutes;
        }

        /// <summary>
        /// Cancels the alarm for day / night mode with sunrise / sunset calculation.
        /// </summary>
        public virtual void cancelAlarmForDayNightModeWithSunriseSunset()
        {
            if (alarmManagerForAutoNightForCalculatedSunriseSunsetHours != null && pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours != null)
            {
                alarmManagerForAutoNightForCalculatedSunriseSunsetHours.Cancel(pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
                wasSetAlarmForSunriseSunsetCalculation = false;
            }
        }

        /// <summary>
        /// Calculates sunrise and sunset hours </summary>
        /// <param name="latitude"> </param>
        /// <param name="longitude"> </param>
        public virtual void calculateSunriseSunsetHours(double latitude, double longitude)
        {
            if (SKToolsDateUtils.AUTO_NIGHT_SUNRISE_HOUR == 0 && SKToolsDateUtils.AUTO_NIGHT_SUNSET_HOUR == 0)
            {
                SKToolsSunriseSunsetCalculator.calculateSunriseSunsetHours(latitude, longitude, SKToolsSunriseSunsetCalculator.OFFICIAL);
            }
        }

        /// <summary>
        /// Checks if the current time of user's device is in sunrise sunset limit.
        /// @return
        /// </summary>
        public virtual bool CurrentTimeInSunriseSunsetLimit
        {
            get
            {
                return SKToolsDateUtils.CurrentTimeInSunriseSunsetLimit;
            }
        }
    }
}