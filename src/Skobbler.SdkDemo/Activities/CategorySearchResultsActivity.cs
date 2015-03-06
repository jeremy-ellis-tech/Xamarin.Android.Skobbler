using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;
using System.Collections.Generic;
using JavaObject = Java.Lang.Object;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "CategorySearchResultsActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class CategorySearchResultsActivity : Activity, ISKSearchListener
    {
        private static readonly int[] MainCategories = new[]
        {
            SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value,
            SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value,
            SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value,
            SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value
        };

        private SKCategories.SKPOIMainCategory _selectedMainCategory;
        private ListView _listView;
        private TextView _operationInProgressLabel;
        private ResultsListAdapter _adaper;

        private Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>> _results = new Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = FindViewById<TextView>(Resource.Id.label_operation_in_progress);
            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            StartSearch();
        }

        private void StartSearch()
        {
            var searchManager = new SKSearchManager(this);
            var searchObject = new SKNearbySearchSettings();

            searchObject.Location = new SKCoordinate(13.387165, 52.516929);
            searchObject.Radius = 1500;

            searchObject.SearchResultsNumber = 300;
            searchObject.SetSearchCategories(MainCategories);
            searchObject.SearchTerm = "";

            SKSearchStatus status = searchManager.NearbySearch(searchObject);

            if(status != SKSearchStatus.SkSearchNoError)
            {
                Toast.MakeText(this, "An error occurred", ToastLength.Short).Show();
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            BuildResultsMap(results);

            RunOnUiThread(() =>
            {
                _operationInProgressLabel.Visibility = ViewStates.Gone;
                _listView.Visibility = ViewStates.Visible;

                _adaper = new ResultsListAdapter(this, _selectedMainCategory, _results);
                _listView.Adapter = _adaper;

                _listView.ItemClick += OnItemClick;
            });
        }

        void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(_selectedMainCategory == null)
            {
                _selectedMainCategory = SKCategories.SKPOIMainCategory.ForInt(MainCategories[e.Position]);
                _adaper.NotifyDataSetChanged();
            }
        }

        private void BuildResultsMap(IList<SKSearchResult> searchResults)
        {
            foreach (var mainCategory in MainCategories)
            {
                _results.Add(SKCategories.SKPOIMainCategory.ForInt(mainCategory), new List<SKSearchResult>());
            }

            foreach (var result in searchResults)
            {
                _results[result.MainCategory].Add(result);
            }
        }

        public override void OnBackPressed()
        {
            if(_selectedMainCategory == null)
            {
                base.OnBackPressed();
            }
            else
            {
                _selectedMainCategory = null;
                _adaper.NotifyDataSetChanged();
            }
            
        }

        private class ResultsListAdapter : BaseAdapter
        {
            private Context _context;
            private SKCategories.SKPOIMainCategory _selectedMainCategory;
            private Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>> _results;

            public ResultsListAdapter(Context context, SKCategories.SKPOIMainCategory selectedMainCategory, Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>> results)
            {
                _context = context;
                _selectedMainCategory = selectedMainCategory;
                _results = results;
            }

            public override int Count
            {
                get
                {
                    if (_selectedMainCategory == null)
                    {
                        return _results.Count;
                    }
                    else
                    {
                        return _results[_selectedMainCategory].Count;
                    }
                }
            }

            public override JavaObject GetItem(int position)
            {
                if(_selectedMainCategory == null)
                {
                    return _results[_selectedMainCategory][position];
                }
                else
                {
                    return _results[_selectedMainCategory][position];
                }
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
                    LayoutInflater inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
                }
                else
                {
                    view = convertView;
                }

                if(_selectedMainCategory == null)
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = SKCategories.SKPOIMainCategory.ForInt(MainCategories[position]).ToString().Replace(".*_", "");
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "number of POIs: " + _results[SKCategories.SKPOIMainCategory.ForInt(MainCategories[position])].Count;
                }
                else
	            {
                    SKSearchResult result = _results[_selectedMainCategory][position];
                    view.FindViewById<TextView>(Resource.Id.title).Text = result.Name == "" ? result.Name : " - ";
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "type: " + result.Category.ToString().Replace(".*_", "");
	            }

                return view;
            }
        }
    }
}