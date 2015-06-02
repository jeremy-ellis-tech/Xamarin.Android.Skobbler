using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.Search;
using Android.Content;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class OfflineAddressSearchActivity : Activity, ISKSearchListener
    {
        private short _currentListLevel;
        private List<SKPackage> _packages;

        private ListView _listView;

        private TextView _operationInProgressLabel;

        private ResultsListAdapter _adapter;
        private Dictionary<short, IList<SKSearchResult>> resultsPerLevel = new Dictionary<short, IList<SKSearchResult>>();
        private string _currentCountryCode;
        private SKSearchManager _searchManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = FindViewById<TextView>(Resource.Id.label_operation_in_progress);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            _packages = SKPackageManager.Instance.GetInstalledPackages().ToList();
            _searchManager = new SKSearchManager(this);

            if (_packages.Count == 0)
            {
                _operationInProgressLabel.Text = Resources.GetString(Resource.String.no_offline_packages);
            }
            else
            {
                InitializeList();
            }
        }

        private void InitializeList()
        {
            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _adapter = new ResultsListAdapter(this);

            _listView.Adapter = _adapter;
            _listView.Visibility = ViewStates.Visible;
            _listView.ItemClick += OnItemClick;
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (_currentListLevel == 0)
            {
                _currentCountryCode = _packages[e.Position].Name;
                ChangeLevel((short)(_currentListLevel + 1), -1);
            }
            else
            {
                if (_currentListLevel < 3)
                {
                    ChangeLevel((short)(_currentListLevel + 1), resultsPerLevel[_currentListLevel][e.Position].Id);
                }

            }
        }

        private void ChangeLevel(short newLevel, long parentId)
        {
            if (newLevel == 0 || newLevel < _currentListLevel)
            {
                _operationInProgressLabel.Visibility = ViewStates.Gone;
                _listView.Visibility = ViewStates.Visible;
                _currentListLevel = newLevel;
                _adapter.NotifyDataSetChanged();
            }
            else if (newLevel > _currentListLevel && newLevel > 0)
            {
                _operationInProgressLabel.Visibility = ViewStates.Visible;
                _listView.Visibility = ViewStates.Gone;

                SKMultiStepSearchSettings searchObject = new SKMultiStepSearchSettings
                {
                    MaxSearchResultsNumber = 25,
                    OfflinePackageCode = _currentCountryCode,
                    SearchTerm = "",
                    ParentIndex = parentId,
                    ListLevel = SKSearchManager.SKListLevel.ForInt(newLevel + 1)
                };

                _currentListLevel = newLevel;

                _searchManager.MultistepSearch(searchObject);
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            resultsPerLevel[_currentListLevel] = results;
            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView.Visibility = ViewStates.Visible;

            if (results.Count > 0)
            {
                _adapter.NotifyDataSetChanged();
            }
            else
            {
                _currentListLevel--;
            }
        }

        public override void OnBackPressed()
        {
            if(_currentListLevel == 0) base.OnBackPressed();
            else ChangeLevel((short)(_currentListLevel - 1), -1);
        }

        private class ResultsListAdapter : BaseAdapter<SKPackage>
        {
            private readonly OfflineAddressSearchActivity _activity;
            public ResultsListAdapter(OfflineAddressSearchActivity activity)
            {
                _activity = activity;
            }

            public override SKPackage this[int position]
            {
                get
                {
                    //if (_activity._currentListLevel > 0)
                    //{
                    //    //return _activity.resultsPerLevel[_activity._currentListLevel][position];
                    //}
                    //else
                    //{
                        return _activity._packages[position];
                    //}
                }
            }

            public override int Count
            {
                get
                {
                    if (_activity._currentListLevel > 0) return _activity.resultsPerLevel[_activity._currentListLevel].Count;
                    else return _activity._packages.Count;
                }
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                LayoutInflater inflater = _activity.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                View view = convertView ?? inflater.Inflate(Resource.Layout.layout_search_list_item, null);

                if (_activity._currentListLevel > 0)
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = _activity.resultsPerLevel[_activity._currentListLevel][position].Name;
                    SKCoordinate location = _activity.resultsPerLevel[_activity._currentListLevel][position].Location;
                    view.FindViewById(Resource.Id.subtitle).Visibility = ViewStates.Visible;
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "location: (" + location.Latitude + ", " + location.Longitude + ")";
                }
                else
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = _activity._packages[position].Name;
                    view.FindViewById(Resource.Id.subtitle).Visibility = ViewStates.Gone;
                }

                return view;
            }
        }
    }

}