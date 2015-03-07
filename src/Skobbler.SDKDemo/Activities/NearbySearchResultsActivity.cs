using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;
using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity in which a nearby search with some user provided parameters is
	/// performed
	/// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
	public class NearbySearchResultsActivity : Activity, ISKSearchListener
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
        private IList<Tuple<string, string>> items = new List<Tuple<string, string>>();

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_list);

			((TextView) FindViewById(Resource.Id.label_operation_in_progress)).Text = Resources.GetString(Resource.String.searching);
			listView = (ListView) FindViewById(Resource.Id.list_view);

			// get the search manager and set the search result listener
			searchManager = new SKSearchManager(this);
			// get a nearby search object
			SKNearbySearchSettings nearbySearchObject = new SKNearbySearchSettings();
			// set the position around which to do the search and the search radius
			nearbySearchObject.Location = new SKCoordinate(Intent.GetDoubleExtra("longitude", 0), Intent.GetDoubleExtra("latitude", 0));
			nearbySearchObject.Radius = Intent.GetIntExtra("radius", 0);
			// set the search topic
			nearbySearchObject.SearchTerm = Intent.GetStringExtra("searchTopic");
			// initiate the nearby search
			SKSearchStatus status = searchManager.NearbySearch(nearbySearchObject);
			if (status != SKSearchStatus.SkSearchNoError)
			{
				Toast.MakeText(this, "An error occurred", ToastLength.Short).Show();
			}
		}

		public override void onReceivedSearchResults(IList<SKSearchResult> results)
		{
			FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
			listView.Visibility = ViewStates.Visible;
			// populate the pair list when receiving search results
			foreach (SKSearchResult result in results)
			{
				string firstLine;
				if (result.Name == null || result.Name.Equals(""))
				{
					firstLine = result.Category.Name();
					firstLine = firstLine.Substring(firstLine.LastIndexOf("_", StringComparison.Ordinal) + 1);
				}
				else
				{
					firstLine = result.Name;
				}
				items.Add(new Tuple<string, string>(firstLine, Convert.ToString(result.Category.Value)));
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
                    LayoutInflater inflater = (LayoutInflater)outerInstance.GetSystemService(Context.LayoutInflaterService);
					view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
				}
				else
				{
					view = convertView;
				}
				((TextView) view.FindViewById(Resource.Id.title)).Text = !outerInstance.items[position].Item1.Equals("") ? outerInstance.items[position].Item1 : " - ";
				((TextView) view.FindViewById(Resource.Id.subtitle)).Text = "type: " + outerInstance.items[position].Item2;
				return view;
			}

		}
	}

}