using Android.App;
namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity in which nearby search parameters are introduced
	/// </summary>
    [Activity]
	public class NearbySearchActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_nearby_search;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
				case Resource.Id.search_button:
					if (validateCoordinates())
					{
						int radius = int.Parse(((TextView) FindViewById(Resource.Id.radius_field)).Text.ToString());
						Intent intent = new Intent(this, typeof(NearbySearchResultsActivity));
						intent.PutExtra("radius", radius);
						intent.PutExtra("latitude", double.Parse(((TextView) FindViewById(Resource.Id.latitude_field)).Text.ToString()));
						intent.PutExtra("longitude", double.Parse(((TextView) FindViewById(Resource.Id.longitude_field)).Text.ToString()));
						intent.PutExtra("searchTopic", ((TextView) FindViewById(Resource.Id.search_topic_field)).Text.ToString());
						StartActivity(intent);
					}
					else
					{
						Toast.MakeText(this, "Invalid latitude or longitude was provided", ToastLength.Short).Show();
					}
					break;
				default:
					break;
			}
		}

		private bool validateCoordinates()
		{
			try
			{
				string latString = ((TextView) FindViewById(Resource.Id.latitude_field)).Text.ToString();
				string longString = ((TextView) FindViewById(Resource.Id.longitude_field)).Text.ToString();
				double latitude = double.Parse(latString);
				double longitude = double.Parse(longString);
				if (latitude > 90 || latitude < -90)
				{
					return false;
				}
				if (longitude > 180 || longitude < -180)
				{
					return false;
				}
				return true;
			}
			catch (System.FormatException)
			{
				return false;
			}
		}
	}

}