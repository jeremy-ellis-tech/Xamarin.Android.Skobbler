using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Search;
using System.Collections.Generic;

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
        private static readonly int[] mainCategories = new [] { SKCategories.SKPOIMainCategory.SkpoiMainCategoryAccomodation.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryServices.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryShopping.Value, SKCategories.SKPOIMainCategory.SkpoiMainCategoryLeisure.Value };

		/// <summary>
		/// The main category selected
		/// </summary>
		private SKCategories.SKPOIMainCategory selectedMainCategory;

		private ListView listView;

		private TextView operationInProgressLabel;

		private ResultsListAdapter adapter;

		/// <summary>
		/// Search results grouped by their main category field
		/// </summary>
		private IDictionary<SKCategories.SKPOIMainCategory, IList<SKSearchResult>> results = new LinkedHashMap<Skobbler.Ngx.SKCategories.SKPOIMainCategory, IList<SKSearchResult>>();

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_list);

			operationInProgressLabel = (TextView) FindViewById(Resource.Id.label_operation_in_progress);
			listView = (ListView) FindViewById(Resource.Id.list_view);
			operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

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
			searchObject.SetSearchCategories(mainCategories);
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
		private void buildResultsMap(IList<SKSearchResult> searchResults)
		{
			foreach (int mainCategory in mainCategories)
			{
				results[Skobbler.Ngx.SKCategories.SKPOIMainCategory.forInt(mainCategory)] = new List<SKSearchResult>();
			}
			foreach (SKSearchResult result in searchResults)
			{
				results[result.MainCategory].Add(result);
			}
		}

		public override void onReceivedSearchResults(IList<SKSearchResult> results)
		{
			buildResultsMap(results);
			operationInProgressLabel.Visibility = ViewStates.Gone;
			listView.Visibility = ViewStates.Visible;
			adapter = new ResultsListAdapter(this);
			listView.Adapter = adapter;

			listView.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly CategorySearchResultsActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(CategorySearchResultsActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
			{
				if (outerInstance.selectedMainCategory == null)
				{
					outerInstance.selectedMainCategory = SKPOIMainCategory.forInt(mainCategories[position]);
					outerInstance.adapter.NotifyDataSetChanged();
				}
			}
		}

		public override void onBackPressed()
		{
			if (selectedMainCategory == null)
			{
				base.OnBackPressed();
			}
			else
			{
				selectedMainCategory = null;
				adapter.NotifyDataSetChanged();
			}
		}

		private class ResultsListAdapter : BaseAdapter
		{
			private readonly CategorySearchResultsActivity outerInstance;

			public ResultsListAdapter(CategorySearchResultsActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					if (outerInstance.selectedMainCategory == null)
					{
						return outerInstance.results.Count;
					}
					else
					{
						return outerInstance.results[outerInstance.selectedMainCategory].Count;
					}
				}
			}

			public override object getItem(int position)
			{
				if (outerInstance.selectedMainCategory == null)
				{
					return outerInstance.results[mainCategories[position]];
				}
				else
				{
					return outerInstance.results[outerInstance.selectedMainCategory][position];
				}
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
					LayoutInflater inflater = (LayoutInflater) GetSystemService(Context.LayoutInflaterService);
					view = inflater.Inflate(Resource.Layout.layout_search_list_item, null);
				}
				else
				{
					view = convertView;
				}
				if (outerInstance.selectedMainCategory == null)
				{
					((TextView) view.FindViewById(Resource.Id.title)).Text = SKCategories.SKPOIMainCategory.ForInt(mainCategories[position]).ToString().ReplaceFirst(".*_", "");
					((TextView) view.FindViewById(Resource.Id.subtitle)).Text = "number of POIs: " + outerInstance.results[SKCategories.SKPOIMainCategory.ForInt(mainCategories[position])].Count;
				}
				else
				{
					SKSearchResult result = outerInstance.results[outerInstance.selectedMainCategory][position];
					((TextView) view.FindViewById(Resource.Id.title)).Text = !result.Name.Equals("") ? result.Name : result.MainCategory.ToString().ReplaceAll(".*_", "");
					((TextView) view.FindViewById(Resource.Id.subtitle)).Text = "type: " + result.Category.ToString().ReplaceAll(".*_", "");
				}
				return view;
			}
		}
	}

}