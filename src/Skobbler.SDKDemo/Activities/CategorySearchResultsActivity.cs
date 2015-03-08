using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;

namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity in which a nearby search for some main categories is performed
	/// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class CategorySearchResultsActivity : Activity, ISKSearchListener
	{

		/// <summary>
		/// The main categories for which the nearby search will be executed
		/// </summary>
        private static readonly int[] MainCategories = { SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryServices.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryShopping.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryLeisure.Value };

		/// <summary>
		/// The main category selected
		/// </summary>
		private SKCategories.SKPOIMainCategory _selectedMainCategory;

		private ListView _listView;

		private TextView _operationInProgressLabel;

		private ResultsListAdapter _adapter;

		/// <summary>
		/// Search results grouped by their main category field
		/// </summary>
		private IDictionary<SKCategories.SKPOIMainCategory, IList<SKSearchResult>> _results = new Dictionary<SKCategories.SKPOIMainCategory, IList<SKSearchResult>>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = (TextView)FindViewById(Resource.Id.label_operation_in_progress);
            _listView = (ListView)FindViewById(Resource.Id.list_view);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            startSearch();
        }

		/// <summary>
		/// Initiates a nearby search with the specified categories
		/// </summary>
		private void startSearch()
		{
			// get a search manager object on which the search listener is specified
			SKSearchManager searchManager = new SKSearchManager(this);
			// get a search object
			SKNearbySearchSettings searchObject = new SKNearbySearchSettings();
			// set nearby search center and radius
			searchObject.Location = new SKCoordinate(13.387165, 52.516929);
			searchObject.Radius = 1500;
			// set the maximum number of search results to be returned
			searchObject.SearchResultsNumber = 300;
			// set the main categories for which to search
			searchObject.SetSearchCategories(MainCategories);
			// set the search term
			searchObject.SearchTerm = "";
			// launch nearby search
			SKSearchStatus status = searchManager.NearbySearch(searchObject);
			if (status != SKSearchStatus.SkSearchNoError)
			{
				Toast.MakeText(this, "An error occurred", ToastLength.Short).Show();
			}
		}

		/// <summary>
		/// Build the search results map from the results of the search
		/// </summary>
		/// <param name="searchResults"> </param>
		private void BuildResultsMap(IList<SKSearchResult> searchResults)
		{
			foreach (int mainCategory in MainCategories)
			{
				_results[SKCategories.SKPOIMainCategory.ForInt(mainCategory)] = new List<SKSearchResult>();
			}
			foreach (SKSearchResult result in searchResults)
			{
				_results[result.MainCategory].Add(result);
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
				    if (_outerInstance._selectedMainCategory == null)
					{
						return _outerInstance._results.Count;
					}
				    return _outerInstance._results[_outerInstance._selectedMainCategory].Count;
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
				if (_outerInstance._selectedMainCategory == null)
				{
					((TextView) view.FindViewById(Resource.Id.title)).Text = SKCategories.SKPOIMainCategory.ForInt(MainCategories[position]).ToString().Replace(".*_", "");
					((TextView) view.FindViewById(Resource.Id.subtitle)).Text = "number of POIs: " + _outerInstance._results[SKCategories.SKPOIMainCategory.ForInt(MainCategories[position])].Count;
				}
				else
				{
					SKSearchResult result = _outerInstance._results[_outerInstance._selectedMainCategory][position];
                    ((TextView)view.FindViewById(Resource.Id.title)).Text = !result.Name.Equals("") ? result.Name : result.MainCategory.ToString().Replace(".*_", "");
                    ((TextView)view.FindViewById(Resource.Id.subtitle)).Text = "type: " + result.Category.ToString().Replace(".*_", "");
				}
				return view;
			}

            public override SKSearchResult this[int position]
            {
                get
                {
                    if (_outerInstance._selectedMainCategory == null)
                    {
                        return null; // return outerInstance.results[mainCategories[0]][position];
                    }
                    return _outerInstance._results[_outerInstance._selectedMainCategory][position];
                }
            }
        }
	}

}