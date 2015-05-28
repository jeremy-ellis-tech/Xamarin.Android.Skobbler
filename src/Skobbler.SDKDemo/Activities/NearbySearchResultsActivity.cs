using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;
using Android.Content;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
	public class NearbySearchResultsActivity : Activity , ISKSearchListener
{
    private SKSearchManager _searchManager;
    private ListView _listView;
    private ResultsListAdapter _adapter;
	
    private List<Tuple<string, string>> _items = new List<Tuple<string, string>>();
	
    protected override void OnCreate(Bundle savedInstanceState)
	{
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_list);

        FindViewById<TextView>(Resource.Id.label_operation_in_progress).Text = Resources.GetString(Resource.String.searching);
        _listView = FindViewById<ListView>(Resource.Id.list_view);
		
        _searchManager = new SKSearchManager(this);
		
        SKNearbySearchSettings nearbySearchObject = new SKNearbySearchSettings
		{
			Location = new SKCoordinate(Intent.GetDoubleExtra("longitude", 0), Intent.GetDoubleExtra("latitude", 0)),
			Radius = Intent.GetShortExtra("radius", (short)0),
			SearchTerm = Intent.GetStringExtra("searchTopic"),
		};
		
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
		
        foreach (SKSearchResult result in results)
		{
            string firstLine;
            if (String.IsNullOrEmpty(result.Name))
			{
                firstLine = result.Category.Name();
                firstLine = firstLine.Substring(firstLine.IndexOf("_") + 1);
            }
			else
			{
                firstLine = result.Name;
            }
			
            _items.Add(new Tuple<string, string>(firstLine, result.Category.Value.ToString()));
        }
		
        _adapter = new ResultsListAdapter(this);
        _listView.Adapter = _adapter;
    }

    private class ResultsListAdapter : BaseAdapter<Tuple<string,string>>
	{
		private readonly NearbySearchResultsActivity _activity;
		
		public ResultsListAdapter(NearbySearchResultsActivity activity)
		{
			_activity = activity;
		}

        public override Tuple<string,string> this[int position]
        {
            get { return _activity._items[position]; }
        }

        public override int Count
        {
            get { return _activity._items.Count; }
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
                LayoutInflater inflater = _activity.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
            }
            else
            {
                view = convertView;
            }

            view.FindViewById<TextView>(Resource.Id.title).Text = !_activity._items[position].Item1.Equals("") ? _activity._items[position].Item1 : " - ";
            view.FindViewById<TextView>(Resource.Id.subtitle).Text = "type: " + _activity._items[position].Item2;

            return view;
        }
    }
}

}