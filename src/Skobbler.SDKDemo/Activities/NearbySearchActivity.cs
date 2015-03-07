namespace Skobbler.SDKDemo.Activity
{
	/// <summary>
	/// Activity in which nearby search parameters are introduced
	/// </summary>
	public class NearbySearchActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_nearby_search;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
				case R.id.search_button:
					if (validateCoordinates())
					{
						int radius = int.Parse(((TextView) findViewById(R.id.radius_field)).Text.ToString());
						Intent intent = new Intent(this, typeof(NearbySearchResultsActivity));
						intent.putExtra("radius", radius);
						intent.putExtra("latitude", double.Parse(((TextView) findViewById(R.id.latitude_field)).Text.ToString()));
						intent.putExtra("longitude", double.Parse(((TextView) findViewById(R.id.longitude_field)).Text.ToString()));
						intent.putExtra("searchTopic", ((TextView) findViewById(R.id.search_topic_field)).Text.ToString());
						startActivity(intent);
					}
					else
					{
						Toast.makeText(this, "Invalid latitude or longitude was provided", Toast.LENGTH_SHORT).show();
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
				string latString = ((TextView) findViewById(R.id.latitude_field)).Text.ToString();
				string longString = ((TextView) findViewById(R.id.longitude_field)).Text.ToString();
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