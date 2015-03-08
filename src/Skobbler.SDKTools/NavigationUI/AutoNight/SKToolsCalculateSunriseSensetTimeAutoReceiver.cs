using Android.Content;
using Android.OS;
using Android.Util;

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    [BroadcastReceiver]
    public class SKToolsCalculateSunriseSunsetTimeAutoReceiver : BroadcastReceiver
    {

        private const string Tag = "CalculateSunriseSunsetTimeAutoReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Error(Tag, "Received Broadcast from alarm manager to recalculate the sunrise / sunset hours");
            if (SKToolsLogicManager.LastUserPosition != null && !SKToolsLogicManager.Instance.NavigationStopped)
            {
                SKToolsSunriseSunsetCalculator.CalculateSunriseSunsetHours(SKToolsLogicManager.LastUserPosition.Latitude, SKToolsLogicManager.LastUserPosition.Longitude, SKToolsSunriseSunsetCalculator.Official);
            }

            if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            {
                SKToolsAutoNightManager.Instance.SetAlarmForHourlyNotificationAfterKitKat(context, false);
            }

        }
    }
}