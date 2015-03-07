using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activity
{
	/// <summary>
	/// Activity in which a nearby search with some user provided parameters is
	/// performed
	/// </summary>
	public class NearbySearchResultsActivity : Activity, SKSearchListener
	{

		/// <summary>
		/// Search manager object
		/// </summary>
		private SKSearchManager searchManager;

		private ListView listView;

		private ResultsListAdapter adapter;

		/// <summary>
		/// List of pairs containing the search results names and categories
		/// </summary>
		private IList<Pair<string, string>> items = new List<Pair<string, string>>();

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_list;

			((TextView) findViewById(R.id.label_operation_in_progress)).Text = Resources.getString(R.@string.searching);
			listView = (ListView) findViewById(R.id.list_view);

			// get the search manager and set the search result listener
			searchManager = new SKSearchManager(this);
			// get a nearby search object
			SKNearbySearchSettings nearbySearchObject = new SKNearbySearchSettings();
			// set the position around which to do the search and the search radius
			nearbySearchObject.Location = new SKCoordinate(Intent.getDoubleExtra("longitude", 0), Intent.getDoubleExtra("latitude", 0));
			nearbySearchObject.Radius = Intent.getIntExtra("radius", 0);
			// set the search topic
			nearbySearchObject.SearchTerm = Intent.getStringExtra("searchTopic");
			// initiate the nearby search
			SKSearchStatus status = searchManager.nearbySearch(nearbySearchObject);
			if (status != SKSearchStatus.SK_SEARCH_NO_ERROR)
			{
				Toast.makeText(this, "An error occurred", Toast.LENGTH_SHORT).show();
			}
		}

		public override void onReceivedSearchResults(IList<SKSearchResult> results)
		{
			findViewById(R.id.label_operation_in_progress).Visibility = View.GONE;
			listView.Visibility = View.VISIBLE;
			// populate the pair list when receiving search results
			foreach (SKSearchResult result in results)
			{
				string firstLine;
				if (result.Name == null || result.Name.Equals(""))
				{
					firstLine = result.Category.name();
					firstLine = firstLine.Substring(firstLine.LastIndexOf("_", StringComparison.Ordinal) + 1);
				}
				else
				{
					firstLine = result.Name;
				}
				items.Add(new Pair<string, string>(firstLine, Convert.ToString(result.Category.Value)));
			}
			adapter = new ResultsListAdapter(this);
			listView.Adapter = adapter;
		}

		private class ResultsListAdapter : BaseAdapter
		{
			private readonly NearbySearchResultsActivity outerInstance;

			public ResultsListAdapter(NearbySearchResultsActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return outerInstance.items.Count;
				}
			}

			public override object getItem(int position)
			{
				return outerInstance.items[position];
			}

			public override long getItemId(int position)
			{
				return 0;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				View view = null;
				if (convertView == null)
				{
					LayoutInflater inflater = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
					view = inflater.inflate(R.layout.layout_search_list_item, null);
				}
				else
				{
					view = convertView;
				}
				((TextView) view.findViewById(R.id.title)).Text = !outerInstance.items[position].first.Equals("") ? outerInstance.items[position].first : " - ";
				((TextView) view.findViewById(R.id.subtitle)).Text = "type: " + outerInstance.items[position].second;
				return view;
			}

		}
	}

}