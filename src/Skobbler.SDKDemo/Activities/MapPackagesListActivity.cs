using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "MapPackagesListActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class MapPackagesListActivity : Activity
    {
        private ListView _listView;
        private DemoApplication _application;

        private List<DownloadPackage> _currentPackages;
        private MapPackageListAdapter _adapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);
            _listView = FindViewById<ListView>(Resource.Id.list_view);
            _application = Application as DemoApplication;

            if(_application.PackageMap != null)
            {
                _currentPackages = SearchByParentCode(null);
                InitializeList();
            }
            else
            {
                Task.Run(()=>{});
            }
        }

        private void InitializeList()
        {
            FindViewById<View>(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
            _adapter = new MapPackageListAdapter(this, _currentPackages);
            _listView.Adapter = _adapter;
            _listView.Visibility = ViewStates.Visible;
            _listView.ItemClick += OnItemClick;
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            List<DownloadPackage> _childPackages = SearchByParentCode(_currentPackages[e.Position].Code);

            if(_childPackages.Count > 0)
            {
                _currentPackages = SearchByParentCode(_currentPackages[e.Position].Code);
                _adapter.NotifyDataSetChanged();
            }
        }

        private List<DownloadPackage> SearchByParentCode(string parentCode)
        {
            var packages = _application.PackageMap.Values;
            var results = new List<DownloadPackage>();

            foreach (var pack in packages)
            {
                if(parentCode == null)
                {
                    results.Add(pack);
                }
                else if(parentCode == pack.ParentCode)
                {
                    results.Add(pack);
                }
            }

            return results;
        }

        public override void OnBackPressed()
        {
            bool shouldClose = true;

            string grandParentCode = null;
            string parentCode = null;

            if(_currentPackages == null)
            {
                base.OnBackPressed();
            }

            else if(_currentPackages.Count != 0)
            {
                parentCode = _currentPackages[0].ParentCode;
            }

            if(parentCode != null)
            {
                shouldClose = false;
                grandParentCode = _application.PackageMap[parentCode].ParentCode;
            }

            if(shouldClose)
            {
                base.OnBackPressed();
            }
            else
            {
                _currentPackages = SearchByParentCode(grandParentCode);
                _adapter.NotifyDataSetChanged();
                DownloadPackage parentPackage = _application.PackageMap[parentCode];
                _listView.SetSelection(_currentPackages.IndexOf(parentPackage));
            }
        }

        private class MapPackageListAdapter : BaseAdapter<DownloadPackage>
        {
            Context _context;
            List<DownloadPackage> _currentPackages;

            public MapPackageListAdapter(Context context, List<DownloadPackage> currentPackages)
            {
                _context = context;
                _currentPackages = currentPackages;
            }

            public override int Count
            {
                get { return _currentPackages.Count; }
            }

            public override DownloadPackage this[int position]
            {
                get { return _currentPackages[position]; }
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
                    view = inflater.Inflate(Resource.Layout.layout_package_list_item, null);
                }
                else
                {
                    view = convertView;
                }

                DownloadPackage currentPackage = _currentPackages[position];
                Button downloadButton = view.FindViewById<Button>(Resource.Id.download_button);
                bool downloadalbe = (currentPackage.Type == "country" || currentPackage.Type == "state" && currentPackage.Code != ("US"));

                if(downloadalbe)
                {
                    downloadButton.Visibility = ViewStates.Visible;

                    view.FindViewById<View>(Resource.Id.download_button).Click += (s, e) =>
                    {
                        var intent = new Intent(_context, typeof(DownloadActivity));
                        intent.PutExtra("packageCode", currentPackage.Code);
                        _context.StartActivity(intent);
                    };
                }
                else
                {
                    downloadButton.Visibility = ViewStates.Gone;
                }

                TextView hasChildrenIndicator = view.FindViewById<TextView>(Resource.Id.indicator_children_available);
                if(currentPackage.ChildrenCodes.Count == 0)
                {
                    hasChildrenIndicator.Visibility = ViewStates.Invisible;
                }
                else
                {
                    hasChildrenIndicator.Visibility = ViewStates.Visible;
                }

                view.FindViewById<TextView>(Resource.Id.label_list_item).Text = currentPackage.Name;

                return view;
            }
        }
    }
}