using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Java.Interop;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
	public class TracksActivity : Activity
	{
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_tracks);

            AlertDialog alertDialog = (new AlertDialog.Builder(this)).Create();
            alertDialog.SetMessage(GetString(Resource.String.gpx_license_notification_text));
            alertDialog.SetCancelable(true);
            alertDialog.SetButton(GetString(Resource.String.ok_text), (s, e) => { alertDialog.Cancel(); });
            alertDialog.Show();
        }

        [Export("OnMenuOptionClick")]
		public virtual void OnMenuOptionClick(View v)
		{
			Intent intent = new Intent(this, typeof(TrackElementsActivity));
			switch (v.Id)
			{
				case Resource.Id.gpx_chicago:
					intent.PutExtra(Intent.ExtraText, "Route_5_Chicago_city_track.gpx");
					break;
				case Resource.Id.gpx_route_1:
					intent.PutExtra(Intent.ExtraText, "Route_1_BerlinUnterDenLinden_BerlinHohenzollerndamm.gpx");
					break;
				case Resource.Id.gpx_route_2:
					intent.PutExtra(Intent.ExtraText, "Route_2_BerlinUnterDenLinden_BerlinGrunewaldstrasse.gpx");
					break;
				case Resource.Id.gpx_route_3:
					intent.PutExtra(Intent.ExtraText, "Route_3_MunchenOskarVonMillerRing_Herterichstrasse.gpx");
					break;
				case Resource.Id.gpx_route_4:
					intent.PutExtra(Intent.ExtraText, "Route_4_Berlin_Hamburg.gpx");
					break;
				default:
					break;
			}
			StartActivityForResult(intent, MapActivity.Tracks);
		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case MapActivity.Tracks:
                        SetResult(Result.Ok);
                        Finish();
                        break;
                    default:
                        break;
                }
            }
        }
	}

}