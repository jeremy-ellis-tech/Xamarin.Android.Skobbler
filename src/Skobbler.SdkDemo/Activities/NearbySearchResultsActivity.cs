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
    [Activity(Label = "NearbySearchresultsActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class NearbySearchResultsActivity : Activity, ISKSearchListener
    {

        private SKSearchManager _searchManager;
        private ListView _listView;
        private ResultsListAdapter _adapter;

        private List<Tuple<string, string>> _items = new List<Tuple<string, string>>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);

            FindViewById<TextView>(Resource.Id.label_operation_in_progress).Text = Resources.GetString(Resource.String.searching);
            _listView = FindViewById<ListView>(Resource.Id.list_view);

            _searchManager = new SKSearchManager(this);

            var nearbySearchObject = new SKNearbySearchSettings();
            nearbySearchObject.Location = new SKCoordinate(Intent.GetDoubleExtra("longitude", 0.0), Intent.GetDoubleExtra("latitude", 0.0));
            nearbySearchObject.Radius = Intent.GetIntExtra("radius", 0);
            nearbySearchObject.SearchTerm = Intent.GetStringExtra("searchTopic");

            SKSearchStatus status = _searchManager.NearbySearch(nearbySearchObject);

            if(status != SKSearchStatus.SkSearchNoError)
            {
                Toast.MakeText(this, "An error occured", ToastLength.Short).Show();
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            RunOnUiThread(() =>
            {
                FindViewById<View>(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
                _listView.Visibility = ViewStates.Visible;

                foreach (SKSearchResult result in results)
                {
                    _items.Add(new Tuple<string, string>(result.Name, result.Category.Value.ToString()));
                }

                _adapter = new ResultsListAdapter(this, _items);
                _listView.Adapter = _adapter;
            });
        }

        private class ResultsListAdapter : BaseAdapter<Tuple<string,string>>
        {
            private Context _context;
            private List<Tuple<string, string>> _items;

            public ResultsListAdapter(Context context, List<Tuple<string, string>> items)
            {
                _context = context;
                _items = items;
            }

            public override Tuple<string, string> this[int position]
            {
                get { return _items[position]; }
            }

            public override int Count
            {
                get { return _items.Count; }
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                View view = null;

                if(convertView == null)
                {
                    LayoutInflater layoutInflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = layoutInflater.Inflate(Resource.Layout.layout_search_list_item, null);
                }
                else
                {
                    view = convertView;
                }

                view.FindViewById<TextView>(Resource.Id.title).Text = _items[position].Item1 != "" ? _items[position].Item2 : " - ";
                view.FindViewById<TextView>(Resource.Id.subtitle).Text = "type: " + _items[position].Item2;

                return view;
            }
        }
    }
}