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
    public class SKToolsChangeMapStyleAutoReceiver : BroadcastReceiver
    {

        private const string TAG = "ChangeMapStyleAutoReceiver";

        public override void onReceive(Context context, Intent intent)
        {
            Log.e(TAG, "Received Broadcast from alarm manager to change the map style");
            if (!SKToolsLogicManager.Instance.NavigationStopped)
            {
                if (SKToolsAutoNightManager.wasSetAlarmForSunriseSunsetCalculation)
                {
                    SKToolsAutoNightManager.Instance.AlarmForDayNightModeWithSunriseSunset = SKToolsLogicManager.Instance.CurrentActivity;
                }
                SKToolsLogicManager.Instance.computeMapStyle(SKToolsDateUtils.Daytime);
            }
        }
    }

}