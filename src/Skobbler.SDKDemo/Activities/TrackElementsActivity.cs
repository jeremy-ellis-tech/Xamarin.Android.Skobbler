using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activity
{
	public class TrackElementsActivity : Activity
	{

		private SKTracksFile loadedFile;

		public static SKTrackElement selectedTrackElement;

		private ListView listView;

		private TrackElementsListAdapter adapter;

		private IDictionary<int?, IList<object>> elementsPerLevel = new Dictionary<int?, IList<object>>();

		private DemoApplication app;

		private int currentLevel;

		protected internal virtual void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_list;
			findViewById(R.id.label_operation_in_progress).Visibility = View.GONE;
			listView = (ListView) findViewById(R.id.list_view);
			app = (DemoApplication) Application;

			string gpxName = Intent.Extras.getString(Intent.EXTRA_TEXT);
			loadedFile = SKTracksFile.loadAtPath(app.MapResourcesDirPath + "GPXTracks/" + gpxName);
			initialize();
		}

		private IList<object> getChildrenForCollectionElement(SKTrackElement parent)
		{
			IList<object> children = new List<object>();
			foreach (SKTrackElement childElement in parent.ChildElements)
			{
				if (childElement.Type.Equals(SKTrackElementType.COLLECTION))
				{
					children.Add(childElement);
				}
			}
			children.AddRange(parent.PointsOnTrackElement);
			return children;
		}

		private void changeLevel(int newLevel, SKTrackElement parent)
		{
			if (newLevel > currentLevel)
			{
				elementsPerLevel[newLevel] = getChildrenForCollectionElement(parent);
			}
			currentLevel = newLevel;
			adapter.notifyDataSetChanged();
			listView.Selection = 0;
		}

		private void initialize()
		{
			elementsPerLevel[currentLevel] = getChildrenForCollectionElement(loadedFile.RootTrackElement);
			adapter = new TrackElementsListAdapter(this);
			listView.Adapter = adapter;
			listView.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly TrackElementsActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(TrackElementsActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onItemClick<T1>(AdapterView<T1> parent, View view, int pos, long id)
			{
				if (outerInstance.elementsPerLevel[outerInstance.currentLevel][pos] is SKTrackElement)
				{
					outerInstance.changeLevel(outerInstance.currentLevel + 1, (SKTrackElement) outerInstance.elementsPerLevel[outerInstance.currentLevel][pos]);
				}
			}
		}

		public override void onBackPressed()
		{
			if (currentLevel == 0)
			{
				base.onBackPressed();
			}
			else
			{
				changeLevel(currentLevel - 1, null);
			}
		}

		private class TrackElementsListAdapter : BaseAdapter
		{
			private readonly TrackElementsActivity outerInstance;

			public TrackElementsListAdapter(TrackElementsActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return outerInstance.elementsPerLevel[outerInstance.currentLevel].Count;
				}
			}

			public override object getItem(int position)
			{
				return outerInstance.elementsPerLevel[outerInstance.currentLevel][position];
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
					view = inflater.inflate(R.layout.layout_track_element_list_item, null);
				}
				else
				{
					view = convertView;
				}
				Button drawButton = (Button) view.findViewById(R.id.draw_button);
				TextView text = (TextView) view.findViewById(R.id.label_list_item);
				object item = outerInstance.elementsPerLevel[outerInstance.currentLevel][position];
				if (item is SKTracksPoint)
				{
					drawButton.Visibility = View.GONE;
					view.findViewById(R.id.indicator_children_available).Visibility = View.GONE;
					SKTracksPoint point = (SKTracksPoint) item;
					text.Text = "POINT\n(" + point.Latitude + ", " + point.Longitude + ")";
				}
				else if (item is SKTrackElement)
				{
					drawButton.Visibility = View.VISIBLE;
					view.findViewById(R.id.indicator_children_available).Visibility = View.VISIBLE;
					SKTrackElement trackElement = (SKTrackElement) item;
					string name = trackElement.Name;
					if (name == null || name.Equals(""))
					{
						text.Text = trackElement.GPXElementType.ToString();
					}
					else
					{
						text.Text = name;
					}
					drawButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, trackElement);
				}
				return view;
			}

			private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
			{
				private readonly TrackElementsListAdapter outerInstance;

				private SKTrackElement trackElement;

				public OnClickListenerAnonymousInnerClassHelper(TrackElementsListAdapter outerInstance, SKTrackElement trackElement)
				{
					this.outerInstance = outerInstance;
					this.trackElement = trackElement;
				}


				public override void onClick(View v)
				{
					selectedTrackElement = trackElement;
					Result = RESULT_OK;
					outerInstance.outerInstance.finish();
				}
			}
		}

	}
}