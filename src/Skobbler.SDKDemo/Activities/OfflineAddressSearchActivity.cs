using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.Search;

namespace Skobbler.SDKDemo.Activities
{
    /// <summary>
    /// Activity where offline address searches are performed and results are listed
    /// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class OfflineAddressSearchActivity : Activity, ISKSearchListener
    {

        /// <summary>
        /// The current list level (see)
        /// </summary>
        private short _currentListLevel;

        /// <summary>
        /// Top level packages available offline (countries and US states)
        /// </summary>
        private IList<SKPackage> _packages;

        private ListView _listView;

        private TextView _operationInProgressLabel;

        private ResultsListAdapter _adapter;

        /// <summary>
        /// Offline address search results grouped by level
        /// </summary>
        private IDictionary<short?, IList<SKSearchResult>> _resultsPerLevel = new Dictionary<short?, IList<SKSearchResult>>();

        /// <summary>
        /// Current top level package code
        /// </summary>
        private string _currentCountryCode;

        /// <summary>
        /// Search manager object
        /// </summary>
        private SKSearchManager _searchManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = (TextView)FindViewById(Resource.Id.label_operation_in_progress);
            _listView = (ListView)FindViewById(Resource.Id.list_view);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            _packages = SKPackageManager.Instance.GetInstalledPackages().ToList();
            _searchManager = new SKSearchManager(this);

            if (_packages.Count == 0)
            {
                Toast.MakeText(this, "No offline map packages are available", ToastLength.Short).Show();
            }

            InitializeList();
        }

        /// <summary>
        /// Initializes list with top level packages
        /// </summary>
        private void InitializeList()
        {
            _adapter = new ResultsListAdapter(this);
            _listView.Adapter = _adapter;
            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView.Visibility = ViewStates.Visible;

            _listView.ItemClick += (s, e) =>
            {
                if (_currentListLevel == 0)
                {
                    _currentCountryCode = _packages[e.Position].Name;
                    ChangeLevel((short)(_currentListLevel + 1), -1, _currentCountryCode);
                }
                else if (_currentListLevel < 3)
                {
                    ChangeLevel((short)(_currentListLevel + 1), _resultsPerLevel[_currentListLevel][e.Position].Id, _currentCountryCode);
                }
            };
        }

        /// <summary>
        /// Changes the list level and executes the corresponding action for the list
        /// level change
        /// </summary>
        /// <param name="newLevel">    the new level </param>
        /// <param name="parentId">    the parent id for which to execute offline address search </param>
        /// <param name="countryCode"> the current code to use in offline address search </param>
        private void ChangeLevel(short newLevel, long parentId, string countryCode)
        {
            if (newLevel == 0 || newLevel < _currentListLevel)
            {
                // for new list level 0 or smaller than previous one just change the
                // level and update the adapter
                _operationInProgressLabel.Visibility = ViewStates.Gone;
                _listView.Visibility = ViewStates.Visible;
                _currentListLevel = newLevel;
                _adapter.NotifyDataSetChanged();
            }
            else if (newLevel > _currentListLevel && newLevel > 0)
            {
                // for new list level greater than previous one execute an offline
                // address search
                _operationInProgressLabel.Visibility = ViewStates.Visible;
                _listView.Visibility = ViewStates.Gone;
                // get a search object
                SKMultiStepSearchSettings searchObject = new SKMultiStepSearchSettings();
                // set the maximum number of results to be returned
                searchObject.MaxSearchResultsNumber = 25;
                // set the country code
                searchObject.OfflinePackageCode = _currentCountryCode;
                // set the search term
                searchObject.SearchTerm = "";
                // set the id of the parent node in which to search
                searchObject.ParentIndex = parentId;
                // set the list level
                searchObject.ListLevel = SKSearchManager.SKListLevel.ForInt(newLevel + 1);
                // change the list level to the new one
                _currentListLevel = newLevel;
                // initiate the search
                _searchManager.MultistepSearch(searchObject);
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            // put in the map at the corresponding level the received results
            _resultsPerLevel[_currentListLevel] = results;
            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView.Visibility = ViewStates.Visible;
            if (results.Count > 0)
            {
                // received results - update adapter to show the results
                _adapter.NotifyDataSetChanged();
            }
            else
            {
                // zero results - no change
                _currentListLevel--;
                _adapter.NotifyDataSetChanged();
            }
        }

        public override void OnBackPressed()
        {
            if (_currentListLevel == 0)
            {
                base.OnBackPressed();
            }
            else
            {
                // if not top level - decrement the current list level and show
                // results for the new level
                ChangeLevel((short)(_currentListLevel - 1), -1, _currentCountryCode);
            }
        }

        private class ResultsListAdapter : BaseAdapter<SKPackage>
        {
            private readonly OfflineAddressSearchActivity _outerInstance;

            public ResultsListAdapter(OfflineAddressSearchActivity outerInstance)
            {
                this._outerInstance = outerInstance;
            }


            public override int Count
            {
                get
                {
                    if (_outerInstance._currentListLevel > 0)
                    {
                        return _outerInstance._resultsPerLevel[_outerInstance._currentListLevel].Count;
                    }
                    else
                    {
                        return _outerInstance._packages.Count;
                    }
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
                if (_outerInstance._currentListLevel > 0)
                {
                    // for offline address search results show the result name and
                    // position
                    ((TextView)view.FindViewById(Resource.Id.title)).Text = _outerInstance._resultsPerLevel[_outerInstance._currentListLevel][position].Name;
                    SKCoordinate location = _outerInstance._resultsPerLevel[_outerInstance._currentListLevel][position].Location;
                    ((TextView)view.FindViewById(Resource.Id.subtitle)).Visibility = ViewStates.Visible;
                    ((TextView)view.FindViewById(Resource.Id.subtitle)).Text = "location: (" + location.Latitude + ", " + location.Longitude + ")";
                }
                else
                {
                    ((TextView)view.FindViewById(Resource.Id.title)).Text = _outerInstance._packages[position].Name;
                    ((TextView)view.FindViewById(Resource.Id.subtitle)).Visibility = ViewStates.Gone;
                }
                return view;
            }

            public override SKPackage this[int position]
            {
                get
                {
                    if (_outerInstance._currentListLevel > 0)
                    {
                        //TODO;
                        return _outerInstance._packages[position];
                    }
                    else
                    {
                        return _outerInstance._packages[position];
                    }
                }
            }
        }
    }

}