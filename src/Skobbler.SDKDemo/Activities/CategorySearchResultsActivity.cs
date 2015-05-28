using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;
using Android.Content;
using System;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class CategorySearchResultsActivity : Activity, ISKSearchListener
    {
        private static readonly int[] MainCategories = new int[]
	    {
            SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value,
           SKCategories.SKPOIMainCategory.SkpoiMainCategoryServices.Value,
           SKCategories.SKPOIMainCategory.SkpoiMainCategoryShopping.Value,
           SKCategories.SKPOIMainCategory.SkpoiMainCategoryLeisure.Value
	    };

        private SKCategories.SKPOIMainCategory _selectedMainCategory;

        private ListView _listView;

        private TextView _operationInProgressLabel;

        private ResultsListAdapter _adapter;

        private Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>> results = new Dictionary<SKCategories.SKPOIMainCategory, List<SKSearchResult>>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = FindViewById<TextView>(Resource.Id.label_operation_in_progress);
            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            StartSearch();
        }

        private void StartSearch()
        {
            SKSearchManager searchManager = new SKSearchManager(this);

            SKNearbySearchSettings searchObject = new SKNearbySearchSettings
            {
                Location = new SKCoordinate(13.387165, 52.516929),
                Radius = 1500,
                SearchResultsNumber = 300,
                SearchTerm = String.Empty,
            };

            searchObject.SetSearchCategories(MainCategories);

            SKSearchStatus status = searchManager.NearbySearch(searchObject);

            if (status != SKSearchStatus.SkSearchNoError)
            {
                Toast.MakeText(this, "An error occurred", ToastLength.Short).Show();
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            BuildResultsMap(results);

            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView.Visibility = ViewStates.Visible;

            _adapter = new ResultsListAdapter(this);

            _listView.Adapter = _adapter;

            _listView.ItemClick += (s, e) =>
            {
                if (_selectedMainCategory == null)
                {
                    _selectedMainCategory = SKCategories.SKPOIMainCategory.ForInt(MainCategories[e.Position]);
                    _adapter.NotifyDataSetChanged();
                }
            };
        }

        public override void OnBackPressed()
        {
            if (_selectedMainCategory == null)
            {
                base.OnBackPressed();
            }
            else
            {
                _selectedMainCategory = null;
                _adapter.NotifyDataSetChanged();
            }
        }

        private void BuildResultsMap(IList<SKSearchResult> searchResults)
        {
            foreach (int mainCategory in MainCategories)
            {
                results.Add(SKCategories.SKPOIMainCategory.ForInt(mainCategory), new List<SKSearchResult>());
            }

            foreach (SKSearchResult result in searchResults)
            {
                results[result.MainCategory].Add(result);
            }
        }

        private class ResultsListAdapter : BaseAdapter<SKSearchResult>
        {
            private readonly CategorySearchResultsActivity _outerInstance;

            public ResultsListAdapter(CategorySearchResultsActivity outerInstance)
            {
                _outerInstance = outerInstance;
            }

            public override int Count
            {
                get
                {
                    return _outerInstance._selectedMainCategory == null ? _outerInstance.results.Count : _outerInstance.results[_outerInstance._selectedMainCategory].Count;
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
                    LayoutInflater inflater = _outerInstance.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
                }
                else
                {
                    view = convertView;
                }
                if (_outerInstance._selectedMainCategory == null)
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = SKCategories.SKPOIMainCategory.ForInt(MainCategories[position]).ToString().Replace(".*_", "");
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "number of POIs: " + _outerInstance.results[SKCategories.SKPOIMainCategory.ForInt(MainCategories[position])].Count;
                }
                else
                {
                    SKSearchResult result = _outerInstance.results[_outerInstance._selectedMainCategory][position];
                    view.FindViewById<TextView>(Resource.Id.title).Text = !result.Name.Equals("") ? result.Name : result.MainCategory.ToString().Replace(".*_", "");
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "type: " + result.Category.ToString().Replace(".*_", "");
                }

                return view;
            }

            public override SKSearchResult this[int position]
            {
                get
                {
                    if (_outerInstance._selectedMainCategory == null)
                    {
                        //return _outerInstance.results[MainCategories[position]];
                    }
                    else
                    {
                        //return _outerInstance.results[_outerInstance._selectedMainCategory][position];
                    }

                    return null;
                }
            }
        }
    }
}