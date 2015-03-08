using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Tracks;
using Skobbler.SDKDemo.Application;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class TrackElementsActivity : Activity
    {

        private SKTracksFile _loadedFile;

        public static SKTrackElement SelectedTrackElement;

        private ListView _listView;

        private TrackElementsListAdapter _adapter;

        private IDictionary<int?, IList<object>> _elementsPerLevel = new Dictionary<int?, IList<object>>();

        private DemoApplication _app;

        private int _currentLevel;

        protected internal virtual void onCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);
            FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
            _listView = (ListView)FindViewById(Resource.Id.list_view);
            _app = (DemoApplication)Application;

            string gpxName = Intent.Extras.GetString(Intent.ExtraText);
            _loadedFile = SKTracksFile.LoadAtPath(_app.MapResourcesDirPath + "GPXTracks/" + gpxName);
            Initialize();
        }

        private IList<object> GetChildrenForCollectionElement(SKTrackElement parent)
        {
            IList<object> children = new List<object>();
            foreach (SKTrackElement childElement in parent.ChildElements)
            {
                if (childElement.Type.Equals(SKTrackElementType.Collection))
                {
                    children.Add(childElement);
                }
            }

            foreach (var item in parent.PointsOnTrackElement)
            {
                children.Add(item);
            }

            return children;
        }

        private void ChangeLevel(int newLevel, SKTrackElement parent)
        {
            if (newLevel > _currentLevel)
            {
                _elementsPerLevel[newLevel] = GetChildrenForCollectionElement(parent);
            }
            _currentLevel = newLevel;
            _adapter.NotifyDataSetChanged();
            _listView.SetSelection(0);
        }

        private void Initialize()
        {
            _elementsPerLevel[_currentLevel] = GetChildrenForCollectionElement(_loadedFile.RootTrackElement);
            _adapter = new TrackElementsListAdapter(this);
            _listView.Adapter = _adapter;
            _listView.ItemClick += (s, e) =>
            {
                if (_elementsPerLevel[_currentLevel][e.Position] is SKTrackElement)
                {
                    ChangeLevel(_currentLevel + 1, (SKTrackElement)_elementsPerLevel[_currentLevel][e.Position]);
                }
            };
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

        private class TrackElementsListAdapter : BaseAdapter<object>
        {
            private readonly TrackElementsActivity _outerInstance;

            public TrackElementsListAdapter(TrackElementsActivity outerInstance)
            {
                this._outerInstance = outerInstance;
            }

            public override int Count
            {
                get
                {
                    return _outerInstance._elementsPerLevel[_outerInstance._currentLevel].Count;
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
                    view = inflater.Inflate(Resource.Layout.layout_track_element_list_item, null);
                }
                else
                {
                    view = convertView;
                }
                Button drawButton = (Button)view.FindViewById(Resource.Id.draw_button);
                TextView text = (TextView)view.FindViewById(Resource.Id.label_list_item);
                object item = _outerInstance._elementsPerLevel[_outerInstance._currentLevel][position];
                if (item is SKTracksPoint)
                {
                    drawButton.Visibility = ViewStates.Gone;
                    view.FindViewById(Resource.Id.indicator_children_available).Visibility = ViewStates.Gone;
                    SKTracksPoint point = (SKTracksPoint)item;
                    text.Text = "POINT\n(" + point.Latitude + ", " + point.Longitude + ")";
                }
                else if (item is SKTrackElement)
                {
                    drawButton.Visibility = ViewStates.Visible;
                    view.FindViewById(Resource.Id.indicator_children_available).Visibility = ViewStates.Visible;
                    SKTrackElement trackElement = (SKTrackElement)item;
                    string name = trackElement.Name;
                    if (name == null || name.Equals(""))
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
                        _outerInstance.SetResult(Result.Ok);
                        _outerInstance.Finish();
                    };
                }
                return view;
            }

            public override object this[int position]
            {
                get { return _outerInstance._elementsPerLevel[_outerInstance._currentLevel][position]; }
            }
        }

    }
}