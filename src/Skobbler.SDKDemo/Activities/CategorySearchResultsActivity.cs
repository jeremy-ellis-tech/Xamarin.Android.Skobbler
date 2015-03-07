using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity in which a nearby search for some main categories is performed
	/// </summary>
	public class CategorySearchResultsActivity : Activity, SKSearchListener
	{

		/// <summary>
		/// The main categories for which the nearby search will be executed
		/// </summary>
		private static readonly int[] mainCategories = new int[]{SKPOIMainCategory.SKPOI_MAIN_CATEGORY_ACCOMODATION.Value, SKPOIMainCategory.SKPOI_MAIN_CATEGORY_SERVICES.Value, SKPOIMainCategory.SKPOI_MAIN_CATEGORY_SHOPPING.Value, SKPOIMainCategory.SKPOI_MAIN_CATEGORY_LEISURE.Value};

		/// <summary>
		/// The main category selected
		/// </summary>
		private SKPOIMainCategory selectedMainCategory;

		private ListView listView;

		private TextView operationInProgressLabel;

		private ResultsListAdapter adapter;

		/// <summary>
		/// Search results grouped by their main category field
		/// </summary>
		private IDictionary<SKPOIMainCategory, IList<SKSearchResult>> results = new LinkedHashMap<SKPOIMainCategory, IList<SKSearchResult>>();

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_list;

			operationInProgressLabel = (TextView) findViewById(R.id.label_operation_in_progress);
			listView = (ListView) findViewById(R.id.list_view);
			operationInProgressLabel.Text = Resources.getString(R.@string.searching);

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
			searchObject.SearchCategories = mainCategories;
			// set the search term
			searchObject.SearchTerm = "";
			// launch nearby search
			SKSearchStatus status = searchManager.nearbySearch(searchObject);
			if (status != SKSearchStatus.SK_SEARCH_NO_ERROR)
			{
				Toast.makeText(this, "An error occurred", Toast.LENGTH_SHORT).show();
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
				results[SKPOIMainCategory.forInt(mainCategory)] = new List<SKSearchResult>();
			}
			foreach (SKSearchResult result in searchResults)
			{
				results[result.MainCategory].Add(result);
			}
		}

		public override void onReceivedSearchResults(IList<SKSearchResult> results)
		{
			buildResultsMap(results);
			operationInProgressLabel.Visibility = View.GONE;
			listView.Visibility = View.VISIBLE;
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
					outerInstance.adapter.notifyDataSetChanged();
				}
			}
		}

		public override void onBackPressed()
		{
			if (selectedMainCategory == null)
			{
				base.onBackPressed();
			}
			else
			{
				selectedMainCategory = null;
				adapter.notifyDataSetChanged();
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
					LayoutInflater inflater = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
					view = inflater.inflate(R.layout.layout_search_list_item, null);
				}
				else
				{
					view = convertView;
				}
				if (outerInstance.selectedMainCategory == null)
				{
					((TextView) view.findViewById(R.id.title)).Text = SKPOIMainCategory.forInt(mainCategories[position]).ToString().replaceFirst(".*_", "");
					((TextView) view.findViewById(R.id.subtitle)).Text = "number of POIs: " + outerInstance.results[SKPOIMainCategory.forInt(mainCategories[position])].Count;
				}
				else
				{
					SKSearchResult result = outerInstance.results[outerInstance.selectedMainCategory][position];
					((TextView) view.findViewById(R.id.title)).Text = !result.Name.Equals("") ? result.Name : result.MainCategory.ToString().replaceAll(".*_", "");
					((TextView) view.findViewById(R.id.subtitle)).Text = "type: " + result.Category.ToString().replaceAll(".*_", "");
				}
				return view;
			}
		}
	}

}