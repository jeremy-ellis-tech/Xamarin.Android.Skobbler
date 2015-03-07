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
    [BroadcastReceiver]
    public class SKToolsCalculateSunriseSunsetTimeAutoReceiver : BroadcastReceiver
    {

        private const string TAG = "CalculateSunriseSunsetTimeAutoReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Error(TAG, "Received Broadcast from alarm manager to recalculate the sunrise / sunset hours");
            if (SKToolsLogicManager.lastUserPosition != null && !SKToolsLogicManager.Instance.NavigationStopped)
            {
                SKToolsSunriseSunsetCalculator.calculateSunriseSunsetHours(SKToolsLogicManager.lastUserPosition.Latitude, SKToolsLogicManager.lastUserPosition.Longitude, SKToolsSunriseSunsetCalculator.OFFICIAL);
            }

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            {
                SKToolsAutoNightManager.Instance.setAlarmForHourlyNotificationAfterKitKat(context, false);
            }

        }
    }
}