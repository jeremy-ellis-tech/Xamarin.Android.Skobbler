using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Tracks;
using Skobbler.SdkDemo.Application;
using System;
using System.Collections.Generic;

namespace Skobbler.SdkDemo.Activities
{
    [Activity(Label = "TrackElementsActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class TrackElementsActivity : Activity
    {
        private SKTracksFile _loadedFile;
        public static SKTrackElement SelectedTrackElement;
        private ListView _listView;
        private TrackElementsListAdapter _adapter;

        private Dictionary<int, List<Object>> _elementsPerLevel = new Dictionary<int, List<object>>();

        private DemoApplication _app;
        private int _currentLevel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_list);

            FindViewById<View>(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;

            _listView = FindViewById<ListView>(Resource.Id.list_view);

            _app = Application as DemoApplication;

            string gpxName = Intent.Extras.GetString(Intent.ExtraIntent);
            _loadedFile = SKTracksFile.LoadAtPath(_app.MapResourcesDirPath + "GPXTracks/" + gpxName);
            Initialize();
        }

        private void Initialize()
        {
            _elementsPerLevel.Add(_currentLevel, GetChildrenForCollectionElement(_loadedFile.RootTrackElement));
            _adapter = new TrackElementsListAdapter(this, _elementsPerLevel, _currentLevel);
            _listView.Adapter = _adapter;
            _listView.ItemClick += OnItemClick;

        }

        private List<Object> GetChildrenForCollectionElement(SKTrackElement parent)
        {
            var children = new List<Object>();
            foreach (var childElement in parent.ChildElements)
            {
                if (childElement.Type == SKTrackElementType.Collection)
                {
                    children.Add(childElement);
                }
            }
            children.Add(parent.PointsOnTrackElement);
            return children;
        }

        public override void OnBackPressed()
        {
            if (_currentLevel == 0)
            {
                base.OnBackPressed();
            }
            else
            {
                ChangeLevel(_currentLevel - 1, null);
            }
        }

        private void ChangeLevel(int newLevel, SKTrackElement parent)
        {
            if (newLevel > _currentLevel)
            {
                _elementsPerLevel.Add(newLevel, GetChildrenForCollectionElement(parent));
            }
            _currentLevel = newLevel;
            _adapter.NotifyDataSetChanged();
            _listView.SetSelection(0);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (_elementsPerLevel[_currentLevel][e.Position] is SKTrackElement)
            {
                ChangeLevel(_currentLevel + 1, (SKTrackElement)_elementsPerLevel[_currentLevel][e.Position]);
            }
        }

        private class TrackElementsListAdapter : BaseAdapter<object>
        {
            Activity _context;
            Dictionary<int, List<object>> _elementsPerLevel;
            int _currentLevel;

            public TrackElementsListAdapter(Activity context, Dictionary<int, List<object>> elementsPerLevel, int currentLevel)
            {
                _context = context;
                _elementsPerLevel = elementsPerLevel;
                _currentLevel = currentLevel;
            }

            public override int Count
            {
                get { return _elementsPerLevel[_currentLevel].Count; }
            }

            public override object this[int position]
            {
                get { return _elementsPerLevel[_currentLevel][position]; }
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
                    LayoutInflater inflater = _context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = inflater.Inflate(Resource.Layout.layout_track_element_list_item, null);
                }
                else
                {
                    view = convertView;
                }

                Button drawButton = view.FindViewById<Button>(Resource.Id.draw_button);
                TextView text = view.FindViewById<TextView>(Resource.Id.label_list_item);
                object item = _elementsPerLevel[_currentLevel][position];

                if (item is SKTracksPoint)
                {
                    drawButton.Visibility = ViewStates.Gone;
                    view.FindViewById<View>(Resource.Id.indicator_children_available).Visibility = ViewStates.Gone;
                    SKTracksPoint point = item as SKTracksPoint;

                    text.Text = "POINT\n(" + point.Latitude + ", " + point.Longitude + ")";
                }

                else if (item is SKTrackElement)
                {
                    drawButton.Visibility = ViewStates.Visible;

                    view.FindViewById<View>(Resource.Id.indicator_children_available).Visibility = ViewStates.Visible;

                    SKTrackElement trackElement = item as SKTrackElement;

                    string name = trackElement.Name;

                    if (String.IsNullOrEmpty(name))
                    {
                        text.Text = trackElement.GPXElementType.ToString();
                    }
                    else
                    {
                        text.Text = name;
                    }

                    drawButton.Click += (s, e) =>
                    {
                        SelectedTrackElement = trackElement;
                        _context.SetResult(Result.Ok);
                        _context.Finish();
                    };
                }

                return view;
            }
        }
    }
}