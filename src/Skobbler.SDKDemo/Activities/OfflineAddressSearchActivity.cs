using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.Search;
using System.Collections.Generic;
using System.Linq;
using JavaObject = Java.Lang.Object;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "OfflineAddressSearchActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class OfflineAddressSearchActivity : Activity, ISKSearchListener
    {
        private short _currentListLevel;
        private List<SKPackage> _packages;
        private ListView _listView;
        private TextView _operationInProgressLabel;
        private ResultsListAdapter _adapter;

        private Dictionary<short, List<SKSearchResult>> _resultsPerLevel = new Dictionary<short, List<SKSearchResult>>();
        private string _currentCountryCode;
        private SKSearchManager _searchManager;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);

            _operationInProgressLabel = FindViewById<TextView>(Resource.Id.label_operation_in_progress);
            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _operationInProgressLabel.Text = Resources.GetString(Resource.String.searching);

            _packages = SKPackageManager.Instance.GetInstalledPackages().ToList();

            _searchManager = new SKSearchManager(this);

            if (_packages.Count == 0)
            {
                Toast.MakeText(this, "No offline map packages are available", ToastLength.Short).Show();
            }

            InitializeList();
        }

        private void InitializeList()
        {
            _adapter = new ResultsListAdapter(this, _currentListLevel, _resultsPerLevel, _packages);
            _listView.Adapter = _adapter;
            _operationInProgressLabel.Visibility = ViewStates.Gone;
            _listView.Visibility = ViewStates.Visible;
            _listView.ItemClick += OnItemClick;
        }

        void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (_currentListLevel == 0)
            {
                _currentCountryCode = _packages[e.Position].Name;
                ChangeLevel((short)(_currentListLevel + 1), -1, _currentCountryCode);
            }
            else if (_currentListLevel < 3)
            {
                ChangeLevel((short)(_currentListLevel + 1), (int)(_resultsPerLevel[_currentListLevel][e.Position].Id), _currentCountryCode);
            }
        }

        private void ChangeLevel(short newLevel, int parentId, string countryCode)
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

                var searchObject = new SKMultiStepSearchSettings();
                searchObject.MaxSearchResultsNumber = 25;
                searchObject.OfflinePackageCode = _currentCountryCode;
                searchObject.SearchTerm = "";
                searchObject.ParentIndex = parentId;
                searchObject.ListLevel = SKSearchManager.SKListLevel.ForInt(newLevel + 1);

                _searchManager.MultistepSearch(searchObject);
            }
        }

        public void OnReceivedSearchResults(IList<SKSearchResult> results)
        {
            _resultsPerLevel.Add(_currentListLevel, results.ToList());

            RunOnUiThread(() =>
            {
                _operationInProgressLabel.Visibility = ViewStates.Gone;
                _listView.Visibility = ViewStates.Visible;
                if(results.Count > 0)
                {
                    _adapter.NotifyDataSetChanged();
                }
                else
                {
                    _currentListLevel--;
                    _adapter.NotifyDataSetChanged();
                }
            });
        }

        public override void OnBackPressed()
        {
            if(_currentListLevel == 0)
            {
                base.OnBackPressed();
            }
            else
            {
                ChangeLevel((short)(_currentListLevel - 1), -1, _currentCountryCode);
            }
            
        }

        private class ResultsListAdapter : BaseAdapter
        {
            private Context _context;
            private short _currentListLevel;
            private Dictionary<short, List<SKSearchResult>> _resultsPerLevel;
            private List<SKPackage> _packages;

            public ResultsListAdapter(Context context, short currentListLevel, Dictionary<short, List<SKSearchResult>> resultsPerLevel, List<SKPackage> packages)
            {
                _context = context;
                _currentListLevel = currentListLevel;
                _resultsPerLevel = resultsPerLevel;
                _packages = packages;
            }

            public override int Count
            {
                get
                {
                    if (_currentListLevel > 0)
                    {
                        return _resultsPerLevel[_currentListLevel].Count;
                    }
                    else
                    {
                        return _packages.Count;
                    }
                }
            }

            public override JavaObject GetItem(int position)
            {
                if(_currentListLevel > 0)
                {
                    return _resultsPerLevel[_currentListLevel][position];
                }
                else
                {
                    return _packages[position];
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

                if(_currentListLevel > 0)
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = _resultsPerLevel[_currentListLevel][position].Name;

                    SKCoordinate location = _resultsPerLevel[_currentListLevel][position].Location;
                    view.FindViewById<TextView>(Resource.Id.subtitle).Visibility = ViewStates.Visible;
                    view.FindViewById<TextView>(Resource.Id.subtitle).Text = "location: (" + location.Latitude + ", " + location.Longitude + ")";
                }
                else
                {
                    view.FindViewById<TextView>(Resource.Id.title).Text = _packages[position].Name;
                    view.FindViewById<TextView>(Resource.Id.subtitle).Visibility = ViewStates.Gone;
                }

                return view;
            }
        }
    }
}