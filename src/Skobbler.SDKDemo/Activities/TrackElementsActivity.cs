using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Tracks;
using Skobbler.SDKDemo.Application;
using JavaObject = Java.Lang.Object;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class TrackElementsActivity : Activity
    {
        private SKTracksFile loadedFile;

        public static SKTrackElement SelectedTrackElement;

        private ListView listView;

        private TrackElementsListAdapter adapter;

        private Dictionary<int, List<object>> elementsPerLevel = new Dictionary<int, List<object>>();

        private DemoApplication app;

        private int currentLevel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_list);
            FindViewById(Resource.Id.label_operation_in_progress).Visibility = ViewStates.Gone;
            listView = (ListView)FindViewById(Resource.Id.list_view);
            app = (DemoApplication)Application;
            string gpxName = Intent.Extras.GetString(Intent.ExtraText);
            loadedFile = SKTracksFile.LoadAtPath(app.MapResourcesDirPath + "GPXTracks/" + gpxName);
            Initialize();
        }

        private List<object> GetChildrenForCollectionElement(SKTrackElement parent)
        {
            List<object> children = new List<object>();

            foreach (SKTrackElement childElement in parent.ChildElements)
            {
                if (childElement.Type.Equals(SKTrackElementType.Collection))
                {
                    children.Add(childElement);
                }
            }

            children.AddRange(parent.PointsOnTrackElement);
            return children;
        }

        private void ChangeLevel(int newLevel, SKTrackElement parent)
        {
            if (newLevel > currentLevel)
            {
                elementsPerLevel.Add(newLevel, GetChildrenForCollectionElement(parent));
            }
            currentLevel = newLevel;
            adapter.NotifyDataSetChanged();
            listView.SetSelection(0);
        }

        private void Initialize()
        {
            elementsPerLevel.Add(currentLevel, GetChildrenForCollectionElement(loadedFile.RootTrackElement));
            adapter = new TrackElementsListAdapter(this);
            listView.Adapter = adapter;
            listView.ItemClick += (s, e) =>
            {
                if (elementsPerLevel[currentLevel][e.Position] is SKTrackElement)
                {
                    ChangeLevel(currentLevel + 1, (SKTrackElement)elementsPerLevel[currentLevel][e.Position]);
                }
            };
        }

        public override void OnBackPressed()
        {
            if (currentLevel == 0)
            {
                base.OnBackPressed();
            }
            else
            {
                ChangeLevel(currentLevel - 1, null);
            }
        }

        private class TrackElementsListAdapter : BaseAdapter<object>
        {
            private readonly TrackElementsActivity _context;

            public TrackElementsListAdapter(TrackElementsActivity context)
            {
                _context = context;
            }

            public override int Count
            {
                get { return _context.elementsPerLevel[_context.currentLevel].Count; }
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
                object item = _context.elementsPerLevel[_context.currentLevel][position];
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
                    if (string.IsNullOrEmpty(name))
                    {
                        text.Text = trackElement.GPXElementType.ToString();
                    }
                    else
                    {
                        text.Text = name;
                    }

                    drawButton.Click += (s, e) =>
                    {
                        TrackElementsActivity.SelectedTrackElement = trackElement;
                        _context.SetResult(Result.Ok);
                        _context.Finish();
                    };
                }
                return view;
            }

            public override object this[int position]
            {
                get { return _context.elementsPerLevel[_context.currentLevel][position]; }
            }
        }
    }
}