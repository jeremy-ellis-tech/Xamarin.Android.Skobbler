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

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    [BroadcastReceiver]
    public class SKToolsCalculateSunriseSunsetTimeAutoReceiver : BroadcastReceiver
    {

        private const string TAG = "CalculateSunriseSunsetTimeAutoReceiver";

        public override void onReceive(Context context, Intent intent)
        {
            Log.e(TAG, "Received Broadcast from alarm manager to recalculate the sunrise / sunset hours");
            if (SKToolsLogicManager.lastUserPosition != null && !SKToolsLogicManager.Instance.NavigationStopped)
            {
                SKToolsSunriseSunsetCalculator.calculateSunriseSunsetHours(SKToolsLogicManager.lastUserPosition.Latitude, SKToolsLogicManager.lastUserPosition.Longitude, SKToolsSunriseSunsetCalculator.OFFICIAL);
            }

            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT)
            {
                SKToolsAutoNightManager.Instance.setAlarmForHourlyNotificationAfterKitKat(context, false);
            }

        }
    }
}