using Android.Content;
using Android.Util;

namespace Skobbler.Ngx.SDKTools.NavigationUI.AutoNight
{
    [BroadcastReceiver]
    public class SKToolsChangeMapStyleAutoReceiver : BroadcastReceiver
    {

        private const string Tag = "ChangeMapStyleAutoReceiver";

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Error(Tag, "Received Broadcast from alarm manager to change the map style");
            if (!SKToolsLogicManager.Instance.NavigationStopped)
            {
                if (SKToolsAutoNightManager.WasSetAlarmForSunriseSunsetCalculation)
                {
                    SKToolsAutoNightManager.Instance.AlarmForDayNightModeWithSunriseSunset = SKToolsLogicManager.Instance.CurrentActivity;
                }
                SKToolsLogicManager.Instance.ComputeMapStyle(SKToolsDateUtils.Daytime);
            }
        }
    }

}