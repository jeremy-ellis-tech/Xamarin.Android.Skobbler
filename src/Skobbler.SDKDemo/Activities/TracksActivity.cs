namespace Skobbler.SDKDemo.Activity
{
	public class TracksActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_tracks;

			AlertDialog alertDialog = (new AlertDialog.Builder(this)).create();
			alertDialog.Message = getString(R.@string.gpx_license_notification_text);
			alertDialog.Cancelable = true;
			alertDialog.setButton(AlertDialog.BUTTON_POSITIVE, getString(R.@string.ok_text), new OnClickListenerAnonymousInnerClassHelper(this, alertDialog));
			alertDialog.show();


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
				alertDialog.cancel();
			}
		}


		public virtual void onMenuOptionClick(View v)
		{
			Intent intent = new Intent(TracksActivity.this, typeof(TrackElementsActivity));
			switch (v.Id)
			{
				case R.id.gpx_chicago:
					intent.putExtra(Intent.EXTRA_TEXT, "Route_5_Chicago_city_track.gpx");
					break;
				case R.id.gpx_route_1:
					intent.putExtra(Intent.EXTRA_TEXT, "Route_1_BerlinUnterDenLinden_BerlinHohenzollerndamm.gpx");
					break;
				case R.id.gpx_route_2:
					intent.putExtra(Intent.EXTRA_TEXT, "Route_2_BerlinUnterDenLinden_BerlinGrunewaldstrasse.gpx");
					break;
				case R.id.gpx_route_3:
					intent.putExtra(Intent.EXTRA_TEXT, "Route_3_MunchenOskarVonMillerRing_Herterichstrasse.gpx");
					break;
				case R.id.gpx_route_4:
					intent.putExtra(Intent.EXTRA_TEXT, "Route_4_Berlin_Hamburg.gpx");
					break;
				default:
					break;
			}
			startActivityForResult(intent, MapActivity.TRACKS);
		}


		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);

			if (resultCode == RESULT_OK)
			{
				switch (requestCode)
				{
					case MapActivity.TRACKS:
						Result = RESULT_OK;
						this.finish();
						break;
					default:
						break;
				}
			}
		}


	}

}