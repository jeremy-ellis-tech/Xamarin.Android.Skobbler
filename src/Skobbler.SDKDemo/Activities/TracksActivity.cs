using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
namespace Skobbler.SDKDemo.Activities
{
    [Activity]
	public class TracksActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_tracks);

			AlertDialog alertDialog = (new AlertDialog.Builder(this)).Create();
			alertDialog.SetMessage(GetString(Resource.String.gpx_license_notification_text));
			alertDialog.SetCancelable(true);
			alertDialog.SetButton(AlertDialog.BUTTON_POSITIVE, GetString(Resource.String.ok_text), new OnClickListenerAnonymousInnerClassHelper(this, alertDialog));
			alertDialog.Show();


		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly TracksActivity outerInstance;

			private AlertDialog alertDialog;

			public OnClickListenerAnonymousInnerClassHelper(TracksActivity outerInstance, AlertDialog alertDialog)
			{
				this.outerInstance = outerInstance;
				this.alertDialog = alertDialog;
			}


			public virtual void onClick(DialogInterface dialog, int id)
			{
				alertDialog.Cancel();
			}
		}


		public virtual void onMenuOptionClick(View v)
		{
			Intent intent = new Intent(TracksActivity.this, typeof(TrackElementsActivity));
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
			StartActivityForResult(intent, MapActivity.TRACKS);
		}


		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == RESULT_OK)
			{
				switch (requestCode)
				{
					case MapActivity.TRACKS:
						Result = RESULT_OK;
						this.Finish();
						break;
					default:
						break;
				}
			}
		}


	}

}