using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;

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
		private SKSearchManager _searchManager;

		private ListView _listView;

		private ResultsListAdapter _adapter;

		/// <summary>
		/// List of pairs containing the search results names and categories
		/// </summary>
        private IList<Tuple<string, string>> _items = new List<Tuple<string, string>>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            ((TextView)FindViewById(Resource.Id.label_operation_in_progress)).Text = Resources.GetString(Resource.String.searching);
            _listView = (ListView)FindViewById(Resource.Id.list_view);

            // get the search manager and set the search result listener
            _searchManager = new SKSearchManager(this);
            // get a nearby search object
            SKNearbySearchSettings nearbySearchObject = new SKNearbySearchSettings();
            // set the position around which to do the search and the search radius
            nearbySearchObject.Location = new SKCoordinate(Intent.GetDoubleExtra("longitude", 0), Intent.GetDoubleExtra("latitude", 0));
            nearbySearchObject.Radius = Intent.GetIntExtra("radius", 0);
            // set the search topic
            nearbySearchObject.SearchTerm = Intent.GetStringExtra("searchTopic");
            // initiate the nearby search
            SKSearchStatus status = _searchManager.NearbySearch(nearbySearchObject);
            if (status != SKSearchStatus.SkSearchNoError)
            {
                Toast.MakeText(this, "An error occurred", ToastLength.Short).Show();
            }
        }

		public void OnReceivedSearchResults(IList<SKSearchResult> results)
		{
			FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
			_listView.Visibility = ViewStates.Visible;
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
				_items.Add(new Tuple<string, string>(firstLine, Convert.ToString(result.Category.Value)));
			}
			_adapter = new ResultsListAdapter(this);
			_listView.Adapter = _adapter;
		}

		private class ResultsListAdapter : BaseAdapter<Tuple<string,string>>
		{
			private readonly NearbySearchResultsActivity _outerInstance;

			public ResultsListAdapter(NearbySearchResultsActivity outerInstance)
			{
				this._outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return _outerInstance._items.Count;
				}
			}

			public override long GetItemId(int position)
			{
				return 0;
			}

			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				View view = null;
				if (convertView == null)
				{
                    LayoutInflater inflater = (LayoutInflater)_outerInstance.GetSystemService(LayoutInflaterService);
					view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
				}
				else
				{
					view = convertView;
				}
				((TextView) view.FindViewById(Resource.Id.title)).Text = !_outerInstance._items[position].Item1.Equals("") ? _outerInstance._items[position].Item1 : " - ";
				((TextView) view.FindViewById(Resource.Id.subtitle)).Text = "type: " + _outerInstance._items[position].Item2;
				return view;
			}


            public override Tuple<string, string> this[int position]
            {
                get { return _outerInstance._items[position]; }
            }
        }
	}

}