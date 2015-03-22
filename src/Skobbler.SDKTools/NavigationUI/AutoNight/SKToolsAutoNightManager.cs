using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Skobbler.Ngx.SDKTools.Extensions;

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    public class SKToolsAutoNightManager
    {

        private const string Tag = "SKToolsAutoNightManager";

        /// <summary>
        /// Singleton instance for current class
        /// </summary>
        private static SKToolsAutoNightManager _instance;

        /// <summary>
        /// the alarm manager used to set the alarm manager for the hourly notification, when autonight is on
        /// </summary>
        private AlarmManager _hourlyAlarmManager;

        /// <summary>
        /// the pending alarm intent for the hourly alarm manager
        /// </summary>
        private PendingIntent _pendingHourlyAlarmIntent;

        /// <summary>
        /// true if the alarm for sunrise / sunset calculation was set, false otherwise
        /// </summary>
        public static bool WasSetAlarmForSunriseSunsetCalculation;

        /// <summary>
        /// the alarm manager used to set the alarm for the calculate sunrise / sunset hours
        /// </summary>
        private AlarmManager _alarmManagerForAutoNightForCalculatedSunriseSunsetHours;

        /// <summary>
        /// the pending alarm intent for the alarm manager used to recalculate the sunrise / sunset hours
        /// </summary>
        private PendingIntent _pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours;


        public static SKToolsAutoNightManager Instance
        {
            get { return _instance ?? (_instance = new SKToolsAutoNightManager()); }
        }

        /// <summary>
        /// Sets the alarm and starts to listen for the times when an hour has passed, in the case when autonight is on. </summary>
        public virtual void SetAlarmForHourlyNotification(Context value)
        {
            if (_hourlyAlarmManager == null)
            {
                //if it already an existing alarm for hourly notification, cancel it
                CancelAlarmForForHourlyNotification();
                _hourlyAlarmManager = (AlarmManager) value.GetSystemService(Context.AlarmService);
                Intent intent = new Intent(value, typeof (SKToolsCalculateSunriseSunsetTimeAutoReceiver));
                _pendingHourlyAlarmIntent = PendingIntent.GetBroadcast(value, 0, intent, 0);

                _hourlyAlarmManager.SetRepeating(AlarmType.Rtc, DateTimeOffset.Now.JavaTimeMillis(),
                    SKToolsSunriseSunsetCalculator.NrOfMillisecondsInAHour, _pendingHourlyAlarmIntent);
            }
        }

        /// <summary>
        /// Sets the alarm and starts to listen for the times when an hour has passed, in the case when autonight is on. </summary>
        /// <param name="context"> </param>
        /// <param name="startNow"> start now or after an hour </param>
        public virtual void SetAlarmForHourlyNotificationAfterKitKat(Context context, bool startNow)
        {
            if (_hourlyAlarmManager == null && Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            {
                //if it already an existing alarm for hourly notification, cancel it
                CancelAlarmForForHourlyNotification();
                _hourlyAlarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
                Intent intent = new Intent(context, typeof(SKToolsCalculateSunriseSunsetTimeAutoReceiver));
                _pendingHourlyAlarmIntent = PendingIntent.GetBroadcast(context, 0, intent, 0);

                long timeToStart = DateTimeOffset.Now.JavaTimeMillis();
                if (!startNow)
                {
                    timeToStart += SKToolsSunriseSunsetCalculator.NrOfMillisecondsInAHour;
                }
                _hourlyAlarmManager.SetExact(AlarmType.Rtc, timeToStart, _pendingHourlyAlarmIntent);
            }
        }

        /// <summary>
        /// Cancels the alarm for hourly notification.
        /// </summary>

        public virtual void CancelAlarmForForHourlyNotification()
        {
            if (_hourlyAlarmManager != null && _pendingHourlyAlarmIntent != null)
            {
                _hourlyAlarmManager.Cancel(_pendingHourlyAlarmIntent);
                _hourlyAlarmManager = null;
            }
        }

        /// <summary>
        /// Sets auto day / night alarm according to user position
        /// if the user position is null is set the alarm with fixed hours (8AM, 8PM),
        /// otherwise is set the alarm for calculation of sunrise / sunset hours. </summary>
        public virtual void SetAutoNightAlarmAccordingToUserPosition(double latitude, double longitude, Activity currentActivity)
        {
            SKToolsSunriseSunsetCalculator.CalculateSunriseSunsetHours(latitude, longitude, SKToolsSunriseSunsetCalculator.Official);
            SetAlarmForDayNightModeWithSunriseSunset(currentActivity);
        }

        /// <summary>
        /// Sets the alarm for sunrise / sunset and starts to listen for the times of changing the map
        /// style (day/night).
        /// </summary>
        public virtual void SetAlarmForDayNightModeWithSunriseSunset(Context value)
        {
            Log.Debug(Tag, "setAlarmForDayNightModeWithSunriseSunset");
            // if there is already an existing alarm then cancel it before starting
            // a new one
            CancelAlarmForDayNightModeWithSunriseSunset();

            _pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours = PendingIntent.GetBroadcast(value, 0,
                new Intent(value, typeof (SKToolsChangeMapStyleAutoReceiver)), 0);
            _alarmManagerForAutoNightForCalculatedSunriseSunsetHours =
                (AlarmManager) value.GetSystemService(Context.AlarmService);

            DateTime date = DateTime.Now; // initializes to now
            DateTime mapStyleChangeCalendar = new DateTime(date.Ticks);

            WasSetAlarmForSunriseSunsetCalculation = true;

            if (!SKToolsDateUtils.Daytime)
            {
                if (ShouldSetAlarmNextDay())
                {
                    mapStyleChangeCalendar = mapStyleChangeCalendar.AddDays(1);
                }
                // set the hour for starting the day style
                mapStyleChangeCalendar = new DateTime(mapStyleChangeCalendar.Year, mapStyleChangeCalendar.Month,
                    mapStyleChangeCalendar.Day, SKToolsDateUtils.AutoNightSunriseHour, SKToolsDateUtils.AutoNightSunriseMinute,
                    0, 0);
            }
            else
            {
                // set the hour for starting the night style
                mapStyleChangeCalendar = new DateTime(mapStyleChangeCalendar.Year, mapStyleChangeCalendar.Month,
                    mapStyleChangeCalendar.Day, SKToolsDateUtils.AutoNightSunsetHour, SKToolsDateUtils.AutoNightSunsetMinute, 0,
                    0);
            }

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            {
                _alarmManagerForAutoNightForCalculatedSunriseSunsetHours.Set(AlarmType.Rtc, mapStyleChangeCalendar.Ticks,
                    _pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
            }
            else
            {
                _alarmManagerForAutoNightForCalculatedSunriseSunsetHours.Set(AlarmType.Rtc, mapStyleChangeCalendar.Ticks,
                    _pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
            }
        }

        /// <returns> true if the alarm for sunrise should be set next day, false otherwise </returns>
        private bool ShouldSetAlarmNextDay()
        {
            int currentMinutes = SKToolsDateUtils.MinuteOfDay + SKToolsDateUtils.HourOfDay * 60;
            int upperMinutes = SKToolsDateUtils.AutoNightSunriseHour * 60 + SKToolsDateUtils.AutoNightSunriseMinute;
            return currentMinutes > upperMinutes;
        }

        /// <summary>
        /// Cancels the alarm for day / night mode with sunrise / sunset calculation.
        /// </summary>
        public virtual void CancelAlarmForDayNightModeWithSunriseSunset()
        {
            if (_alarmManagerForAutoNightForCalculatedSunriseSunsetHours != null && _pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours != null)
            {
                _alarmManagerForAutoNightForCalculatedSunriseSunsetHours.Cancel(_pendingAlarmIntentForAutoNightForCalculatedSunriseSunsetHours);
                WasSetAlarmForSunriseSunsetCalculation = false;
            }
        }

        /// <summary>
        /// Calculates sunrise and sunset hours </summary>
        /// <param name="latitude"> </param>
        /// <param name="longitude"> </param>
        public virtual void CalculateSunriseSunsetHours(double latitude, double longitude)
        {
            if (SKToolsDateUtils.AutoNightSunriseHour == 0 && SKToolsDateUtils.AutoNightSunsetHour == 0)
            {
                SKToolsSunriseSunsetCalculator.CalculateSunriseSunsetHours(latitude, longitude, SKToolsSunriseSunsetCalculator.Official);
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