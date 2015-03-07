using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activity
{
	/// <summary>
	/// Activity that displays a list of downloadable resources and provides the ability to download them
	/// </summary>
	public class ResourceDownloadsListActivity : Activity
	{

		/// <summary>
		/// Constants
		/// </summary>
		public const long KILO = 1024;

		public static readonly long MEGA = KILO * KILO;

		public static readonly long GIGA = MEGA * KILO;

		public static readonly long TERRA = GIGA * KILO;

		/// <summary>
		/// Download manager used for controlling the download process
		/// </summary>
		private SKToolsDownloadManager downloadManager;

		/// <summary>
		/// Adapter for download items
		/// </summary>
		private DownloadsAdapter adapter;

		/// <summary>
		/// List element displaying download items
		/// </summary>
		private ListView listView;

		/// <summary>
		/// List of items in the current screen
		/// </summary>
		private IList<ListItem> currentListItems;

		/// <summary>
		/// Map from resource codes to items
		/// </summary>
		private IDictionary<string, ListItem> codesMap = new Dictionary<string, ListItem>();

		/// <summary>
		/// List of all map resources
		/// </summary>
		public static IDictionary<string, MapDownloadResource> allMapResources;

		/// <summary>
		/// List of downloads which are currently in progress
		/// </summary>
		public static IList<DownloadResource> activeDownloads = new List<DownloadResource>();

		/// <summary>
		/// DAO object for accessing the maps database
		/// </summary>
		public static MapsDAO mapsDAO;

		/// <summary>
		/// Stack containing list indexes for opened screens
		/// </summary>
		private Stack<int?> previousListIndexes = new Stack<int?>();

		/// <summary>
		/// Context object
		/// </summary>
		private DemoApplication appContext;

		private IDictionary<long?, long?> downloadChunksMap = new SortedDictionary<long?, long?>();

		/// <summary>
		/// Handler object used for scheduling periodic UI updates while downloading is in progress
		/// </summary>
		private Handler handler;

		/// <summary>
		/// True if download estimates should be refreshed at next UI update
		/// </summary>
		private bool refreshDownloadEstimates;

		/// <summary>
		/// Timestamp at which last download started
		/// </summary>
		private long downloadStartTime;

		/// <summary>
		/// Item in the download list
		/// </summary>
		private class ListItem : IComparable<ListItem>
		{
			private readonly ResourceDownloadsListActivity outerInstance;

			public ListItem(ResourceDownloadsListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			internal string name;

			internal DownloadResource downloadResource;

			internal IList<ListItem> children;

			internal ListItem parent;

			public virtual int CompareTo(ListItem listItem)
			{
				if (listItem != null && listItem.name != null && name != null)
				{
					return name.CompareTo(listItem.name);
				}
				return 0;
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{

			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_downloads_list;
			appContext = (DemoApplication) Application;
			handler = new Handler();

			ListItem mapResourcesItem = new ListItem(this);
			new AsyncTaskAnonymousInnerClassHelper(this, mapResourcesItem)
			.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, bool?>
		{
			private readonly ResourceDownloadsListActivity outerInstance;

			private com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem mapResourcesItem;

			public AsyncTaskAnonymousInnerClassHelper(ResourceDownloadsListActivity outerInstance, com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem mapResourcesItem)
			{
				this.outerInstance = outerInstance;
				this.mapResourcesItem = mapResourcesItem;
			}

			protected internal override bool? doInBackground(params Void[] voids)
			{
				return outerInstance.initializeMapResources();
			}

			protected internal override void onPostExecute(bool? success)
			{
				if (success.Value)
				{
					outerInstance.populateWithChildMaps(mapResourcesItem);
					outerInstance.currentListItems = mapResourcesItem.children;

					outerInstance.listView = (ListView) findViewById(R.id.list_view);
					outerInstance.adapter = new DownloadsAdapter(outerInstance);
					outerInstance.listView.Adapter = outerInstance.adapter;
					outerInstance.findViewById(R.id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? View.GONE : View.VISIBLE;
					outerInstance.downloadManager = SKToolsDownloadManager.getInstance(outerInstance.adapter);
					if (activeDownloads.Count > 0 && activeDownloads[0].DownloadState == SKToolsDownloadItem.DOWNLOADING)
					{
						outerInstance.startPeriodicUpdates();
					}
				}
				else
				{
					Toast.makeText(outerInstance, "Could not retrieve map data from the server", Toast.LENGTH_SHORT).show();
					outerInstance.finish();
				}
			}
		}

		/// <summary>
		/// Runnable used to trigger UI updates that refresh the download estimates (for current speed and remaining time)
		/// </summary>
		private Runnable updater = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public override void run()
			{
				outerInstance.refreshDownloadEstimates = true;
				runOnUiThread(new RunnableAnonymousInnerClassHelper2(this));
				outerInstance.handler.postDelayed(this, 1000);
			}

			private class RunnableAnonymousInnerClassHelper2 : Runnable
			{
				private readonly RunnableAnonymousInnerClassHelper outerInstance;

				public RunnableAnonymousInnerClassHelper2(RunnableAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.outerInstance.adapter.notifyDataSetChanged();
				}
			}
		}

		/// <summary>
		/// Starte periodic UI updates
		/// </summary>
		private void startPeriodicUpdates()
		{
			downloadStartTime = DateTimeHelperClass.CurrentUnixTimeMillis();
			handler.postDelayed(updater, 3000);
		}

		/// <summary>
		/// Stops the periodic UI updates
		/// </summary>
		private void stopPeriodicUpdates()
		{
			downloadChunksMap.Clear();
			handler.removeCallbacks(updater);
		}

		/// <summary>
		/// Initializes the map resources (reads them from the database if they are available there or parses them otherwise and stores them in the database)
		/// </summary>
		private bool initializeMapResources()
		{
			mapsDAO = ResourcesDAOHandler.getInstance(this).MapsDAO;
			if (allMapResources == null)
			{
				allMapResources = mapsDAO.getAvailableMapsForACertainType(null);
				if (allMapResources == null)
				{
					// maps table in DB not populated yet
					IList<MapDownloadResource> parsedMapResources = new List<MapDownloadResource>();
					IDictionary<string, string> parsedMapItemsCodes = new Dictionary<string, string>();
					IDictionary<string, string> regionItemsCodes = new Dictionary<string, string>();
					try
					{
						// parse Maps.json
						string jsonUrl = SKPackageManager.Instance.MapsJSONPathForCurrentVersion;
						HttpURLConnection connection = (HttpURLConnection) (new URL(jsonUrl)).openConnection();
						(new MapDataParser()).parseMapJsonData(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, connection.InputStream);
						// populate DB maps table with parsing results
						mapsDAO.insertMaps(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, this);
						// get all map resources
						allMapResources = mapsDAO.getAvailableMapsForACertainType(null);
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
				activeDownloads = ActiveMapDownloads;
				if (activeDownloads.Count > 0 && activeDownloads[0].DownloadState == SKToolsDownloadItem.DOWNLOADING)
				{
					// pausing first download in queue, if it's in downloading state
					activeDownloads[0].DownloadState = SKToolsDownloadItem.PAUSED;
					mapsDAO.updateMapResource((MapDownloadResource) activeDownloads[0]);
				}
			}

			return allMapResources != null && allMapResources.Count > 0;
		}

		/// <summary>
		/// Filters the active downloads (having their state QUEUED, DOWNLOADING or PAUSED) from all the available downloads
		/// 
		/// @return
		/// </summary>
		private IList<DownloadResource> ActiveMapDownloads
		{
			get
			{
				IList<DownloadResource> activeMapDownloads = new List<DownloadResource>();
				string[] mapCodesArray = (new Gson()).fromJson(appContext.AppPrefs.getStringPreference(ApplicationPreferences.DOWNLOAD_QUEUE_PREF_KEY), typeof(string[]));
				if (mapCodesArray == null)
				{
					return activeMapDownloads;
				}
				foreach (string mapCode in mapCodesArray)
				{
					activeMapDownloads.Add(allMapResources[mapCode]);
				}
				return activeMapDownloads;
			}
		}

		/// <summary>
		/// Recursively populates list items with the corresponding parent & child data
		/// </summary>
		/// <param name="mapItem"> </param>
		private void populateWithChildMaps(ListItem mapItem)
		{
			string code;
			if (mapItem.downloadResource == null)
			{
				code = "";
			}
			else
			{
				code = mapItem.downloadResource.Code;
			}

			IList<ListItem> childrenItems = getChildrenOf(code);
			childrenItems.Sort();
			foreach (ListItem childItem in childrenItems)
			{
				childItem.parent = mapItem;
				populateWithChildMaps(childItem);
			}
			mapItem.children = childrenItems;
		}

		/// <summary>
		/// Gets the list of child items for a given code
		/// </summary>
		/// <param name="parentCode">
		/// @return </param>
		private IList<ListItem> getChildrenOf(string parentCode)
		{
			IList<ListItem> children = new List<ListItem>();
			foreach (MapDownloadResource mapResource in allMapResources.Values)
			{
				if (mapResource.ParentCode.Equals(parentCode))
				{
					ListItem listItem = new ListItem(this);
					listItem.name = mapResource.Name;
					listItem.downloadResource = mapResource;
					children.Add(listItem);
				}
			}
			return children;
		}

		/// <summary>
		/// Constructs a map from resource codes (keys )to list items (values) for the items currently displayed
		/// </summary>
		private void buildCodesMap()
		{
			foreach (ListItem item in currentListItems)
			{
				codesMap[item.downloadResource.Code] = item;
			}
		}

		/// <summary>
		/// Represents the adapter associated with maps list
		/// </summary>
		private class DownloadsAdapter : BaseAdapter, SKToolsDownloadListener
		{
			private readonly ResourceDownloadsListActivity outerInstance;

			public DownloadsAdapter(ResourceDownloadsListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return outerInstance.currentListItems.Count;
				}
			}

			public override ListItem getItem(int i)
			{
				return outerInstance.currentListItems[i];
			}

			public override long getItemId(int i)
			{
				return 0;
			}

			public override View getView(int position, View convertView, ViewGroup viewGroup)
			{
				ListItem currentItem = getItem(position);
				View view = null;
				if (convertView == null)
				{
					LayoutInflater inflater = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
					view = inflater.inflate(R.layout.element_download_list_item, null);
				}
				else
				{
					view = convertView;
				}

				ImageView arrowImage = (ImageView) view.findViewById(R.id.arrow);
				TextView downloadSizeText = (TextView) view.findViewById(R.id.package_size);
				TextView downloadNameText = (TextView) view.findViewById(R.id.package_name);
				RelativeLayout middleLayout = (RelativeLayout) view.findViewById(R.id.middle_layout);
				ImageView startPauseImage = (ImageView) view.findViewById(R.id.start_pause);
				ImageView cancelImage = (ImageView) view.findViewById(R.id.cancel);
				TextView stateText = (TextView) view.findViewById(R.id.current_state);
				ProgressBar progressBar = (ProgressBar) view.findViewById(R.id.download_progress);
				RelativeLayout progressDetailsLayout = (RelativeLayout) view.findViewById(R.id.progress_details);
				TextView percentageText = (TextView) view.findViewById(R.id.percentage);
				TextView timeLeftText = (TextView) view.findViewById(R.id.time_left);
				TextView speedText = (TextView) view.findViewById(R.id.speed);

				downloadNameText.Text = currentItem.name;

				if (currentItem.children == null || currentItem.children.Count == 0)
				{
					arrowImage.Visibility = View.GONE;
				}
				else
				{
					arrowImage.Visibility = View.VISIBLE;
				}

				if (currentItem.downloadResource != null)
				{

					DownloadResource downloadResource = currentItem.downloadResource;

					bool progressShown = downloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING || downloadResource.DownloadState == SKToolsDownloadItem.PAUSED;
					if (progressShown)
					{
						progressBar.Visibility = View.VISIBLE;
						progressDetailsLayout.Visibility = View.VISIBLE;
						progressBar.Progress = getPercentage(downloadResource);
						percentageText.Text = getPercentage(downloadResource) + "%";
						if (downloadResource.DownloadState == SKToolsDownloadItem.PAUSED)
						{
							timeLeftText.Text = "-";
							speedText.Text = "-";
						}
						else if (outerInstance.refreshDownloadEstimates)
						{
							Pair<string, string> pair = calculateDownloadEstimates(downloadResource, 20);
							speedText.Text = pair.first;
							timeLeftText.Text = pair.second;
							outerInstance.refreshDownloadEstimates = false;
						}
					}
					else
					{
						progressBar.Visibility = View.GONE;
						progressDetailsLayout.Visibility = View.GONE;
					}

					long bytesToDownload = 0;
					if (downloadResource is MapDownloadResource)
					{
						MapDownloadResource mapResource = (MapDownloadResource) downloadResource;
						bytesToDownload = mapResource.SkmAndZipFilesSize + mapResource.TXGFileSize;
					}

					if (bytesToDownload != 0)
					{
						middleLayout.Visibility = View.VISIBLE;
						downloadSizeText.Visibility = View.VISIBLE;
						downloadSizeText.Text = convertBytesToStringRepresentation(bytesToDownload);
					}
					else
					{
						middleLayout.Visibility = View.GONE;
						downloadSizeText.Visibility = View.GONE;
					}

					switch (downloadResource.DownloadState)
					{
						case SKToolsDownloadItem.NOT_QUEUED:
							stateText.Text = "NOT QUEUED";
							break;
						case SKToolsDownloadItem.QUEUED:
							stateText.Text = "QUEUED";
							break;
						case SKToolsDownloadItem.DOWNLOADING:
							stateText.Text = "DOWNLOADING";
							break;
						case SKToolsDownloadItem.DOWNLOADED:
							stateText.Text = "DOWNLOADED";
							break;
						case SKToolsDownloadItem.PAUSED:
							stateText.Text = "PAUSED";
							break;
						case SKToolsDownloadItem.INSTALLING:
							stateText.Text = "INSTALLING";
							break;
						case SKToolsDownloadItem.INSTALLED:
							stateText.Text = "INSTALLED";
							break;
						default:
					break;
					}

					if (downloadResource.DownloadState == SKToolsDownloadItem.NOT_QUEUED || downloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING || downloadResource.DownloadState == SKToolsDownloadItem.PAUSED)
					{
						startPauseImage.Visibility = View.VISIBLE;
						if (downloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING)
						{
							startPauseImage.ImageResource = R.drawable.pause;
						}
						else
						{
							startPauseImage.ImageResource = R.drawable.download;
						}
					}
					else
					{
						startPauseImage.Visibility = View.GONE;
					}

					if (downloadResource.DownloadState == SKToolsDownloadItem.NOT_QUEUED || downloadResource.DownloadState == SKToolsDownloadItem.INSTALLING)
					{
						cancelImage.Visibility = View.GONE;
					}
					else
					{
						cancelImage.Visibility = View.VISIBLE;
					}

					if (downloadResource is MapDownloadResource)
					{
						MapDownloadResource mapResource = (MapDownloadResource) downloadResource;
					}

				}
				else
				{
					// no download resource
					downloadSizeText.Visibility = View.GONE;
					middleLayout.Visibility = View.GONE;
					progressBar.Visibility = View.GONE;
					progressDetailsLayout.Visibility = View.GONE;
					downloadSizeText.Visibility = View.GONE;
				}

				view.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, currentItem, view);

				startPauseImage.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this, currentItem, view);

				cancelImage.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this, currentItem, view);

				return view;
			}

			private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
			{
				private readonly DownloadsAdapter outerInstance;

				private com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem;
				private View view;

				public OnClickListenerAnonymousInnerClassHelper(DownloadsAdapter outerInstance, com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem, View view)
				{
					this.outerInstance = outerInstance;
					this.currentItem = currentItem;
					this.view = view;
				}

				public override void onClick(View view)
				{
					if (currentItem.children == null || currentItem.children.Count == 0)
					{
						return;
					}
					outerInstance.outerInstance.currentListItems = currentItem.children;
					outerInstance.outerInstance.buildCodesMap();
					outerInstance.outerInstance.previousListIndexes.Push(outerInstance.outerInstance.listView.FirstVisiblePosition);
					outerInstance.outerInstance.updateListAndScrollToPosition(0);
				}
			}

			private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
			{
				private readonly DownloadsAdapter outerInstance;

				private com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem;
				private View view;

				public OnClickListenerAnonymousInnerClassHelper2(DownloadsAdapter outerInstance, com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem, View view)
				{
					this.outerInstance = outerInstance;
					this.currentItem = currentItem;
					this.view = view;
				}

				public override void onClick(View view)
				{
					if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.DOWNLOADING)
					{
						if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.PAUSED)
						{
							activeDownloads.Add(currentItem.downloadResource);
							currentItem.downloadResource.DownloadState = SKToolsDownloadItem.QUEUED;
							outerInstance.outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
							string destinationPath = outerInstance.outerInstance.appContext.MapResourcesDirPath + "downloads/";
							File destinationFile = new File(destinationPath);
							if (!destinationFile.exists())
							{
								destinationFile.mkdirs();
							}
							currentItem.downloadResource.DownloadPath = destinationPath;
							mapsDAO.updateMapResource((MapDownloadResource) currentItem.downloadResource);
						}

						outerInstance.notifyDataSetChanged();

						IList<SKToolsDownloadItem> downloadItems;
						if (!outerInstance.outerInstance.downloadManager.DownloadProcessRunning)
						{
							downloadItems = outerInstance.outerInstance.createDownloadItemsFromDownloadResources(activeDownloads);
						}
						else
						{
							IList<DownloadResource> mapDownloadResources = new List<DownloadResource>();
							mapDownloadResources.Add(currentItem.downloadResource);
							downloadItems = outerInstance.outerInstance.createDownloadItemsFromDownloadResources(mapDownloadResources);
						}

						foreach (SKToolsDownloadItem item in downloadItems)
						{
						}
						outerInstance.outerInstance.downloadManager.startDownload(downloadItems);
					}
					else
					{
						outerInstance.outerInstance.downloadManager.pauseDownloadThread();
					}
				}
			}

			private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
			{
				private readonly DownloadsAdapter outerInstance;

				private com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem;
				private View view;

				public OnClickListenerAnonymousInnerClassHelper3(DownloadsAdapter outerInstance, com.skobbler.sdkdemo.activity.ResourceDownloadsListActivity.ListItem currentItem, View view)
				{
					this.outerInstance = outerInstance;
					this.currentItem = currentItem;
					this.view = view;
				}

				public override void onClick(View view)
				{
					if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.INSTALLED)
					{
						bool downloadCancelled = outerInstance.outerInstance.downloadManager.cancelDownload(currentItem.downloadResource.Code);
						if (!downloadCancelled)
						{
							currentItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
							currentItem.downloadResource.NoDownloadedBytes = 0;
							mapsDAO.updateMapResource((MapDownloadResource) currentItem.downloadResource);
							activeDownloads.Remove(currentItem.downloadResource);
							outerInstance.outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
							outerInstance.notifyDataSetChanged();
						}
					}
					else
					{
						bool packageDeleted = SKPackageManager.Instance.deleteOfflinePackage(currentItem.downloadResource.Code);
						if (packageDeleted)
						{
							Toast.makeText(outerInstance.outerInstance.appContext, ((MapDownloadResource) currentItem.downloadResource).Name + " was uninstalled", Toast.LENGTH_SHORT).show();
						}
						currentItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
						currentItem.downloadResource.NoDownloadedBytes = 0;
						mapsDAO.updateMapResource((MapDownloadResource) currentItem.downloadResource);
						outerInstance.notifyDataSetChanged();
					}
				}
			}

			/// <summary>
			/// Calculates download estimates (for current speed and remaining time) for the currently downloading resource.
			/// This estimate is based on how much was downloaded during the reference period.
			/// </summary>
			/// <param name="resource"> currently downloading resource </param>
			/// <param name="referencePeriodInSeconds"> the reference period (in seconds) </param>
			/// <returns> formatted string representations of the current download speed and remaining time </returns>
			internal virtual Pair<string, string> calculateDownloadEstimates(DownloadResource resource, int referencePeriodInSeconds)
			{
				long referencePeriod = 1000 * referencePeriodInSeconds;
				long currentTimestamp = DateTimeHelperClass.CurrentUnixTimeMillis();
				long downloadPeriod = currentTimestamp - referencePeriod < outerInstance.downloadStartTime ? currentTimestamp - outerInstance.downloadStartTime : referencePeriod;
				long totalBytesDownloaded = 0;
				IEnumerator<KeyValuePair<long?, long?>> iterator = outerInstance.downloadChunksMap.SetOfKeyValuePairs().GetEnumerator();
				while (iterator.MoveNext())
				{
					KeyValuePair<long?, long?> entry = iterator.Current;
					long timestamp = entry.Key;
					long bytesDownloaded = entry.Value;
					if (currentTimestamp - timestamp > referencePeriod)
					{
						iterator.remove();
					}
					else
					{
						totalBytesDownloaded += bytesDownloaded;
					}
				}
				float downloadPeriodSec = downloadPeriod / 1000f;
				long bytesPerSecond = Math.Round(totalBytesDownloaded / downloadPeriodSec);
				string formattedTimeLeft = "";
				if (totalBytesDownloaded == 0)
				{
					formattedTimeLeft = "-";
				}
				else if (resource is MapDownloadResource)
				{
					MapDownloadResource mapResource = (MapDownloadResource) resource;
					long remainingBytes = (mapResource.SkmAndZipFilesSize + mapResource.TXGFileSize) - mapResource.NoDownloadedBytes;
					long timeLeft = (downloadPeriod * remainingBytes) / totalBytesDownloaded;
					formattedTimeLeft = getFormattedTime(timeLeft);
				}

				return new Pair<string, string>(convertBytesToStringRepresentation(bytesPerSecond) + "/s", formattedTimeLeft);
			}

			public override void notifyDataSetChanged()
			{
				outerInstance.findViewById(R.id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? View.GONE : View.VISIBLE;
				base.notifyDataSetChanged();
				outerInstance.listView.postInvalidate();
			}

			/// <summary>
			/// Gets a percentage of how much was downloaded from the given resource
			/// </summary>
			/// <param name="downloadResource"> download resource </param>
			/// <returns> perecntage value </returns>
			internal virtual int getPercentage(DownloadResource downloadResource)
			{
				int percentage = 0;
				if (downloadResource is MapDownloadResource)
				{
					MapDownloadResource mapDownloadResource = (MapDownloadResource) downloadResource;
					percentage = (int)(((float) mapDownloadResource.NoDownloadedBytes / (mapDownloadResource.SkmAndZipFilesSize + mapDownloadResource.TXGFileSize)) * 100);
				}
				return percentage;
			}

			public override void onDownloadProgress(SKToolsDownloadItem currentDownloadItem)
			{
				ListItem affectedListItem = outerInstance.codesMap[currentDownloadItem.ItemCode];
				DownloadResource resource;
				bool stateChanged = false;
				long bytesDownloadedSinceLastUpdate = 0;
				if (affectedListItem != null)
				{
					stateChanged = currentDownloadItem.DownloadState != affectedListItem.downloadResource.DownloadState;
					bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - affectedListItem.downloadResource.NoDownloadedBytes;
					affectedListItem.downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					affectedListItem.downloadResource.DownloadState = currentDownloadItem.DownloadState;
					resource = affectedListItem.downloadResource;
					runOnUiThread(new RunnableAnonymousInnerClassHelper3(this));
				}
				else
				{
					resource = allMapResources[currentDownloadItem.ItemCode];
					bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - resource.NoDownloadedBytes;
					stateChanged = currentDownloadItem.DownloadState != resource.DownloadState;
					resource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					resource.DownloadState = currentDownloadItem.DownloadState;
				}
				if (resource.DownloadState == SKToolsDownloadItem.DOWNLOADED)
				{
					activeDownloads.Remove(resource);
					outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
				}
				else if (resource.DownloadState == SKToolsDownloadItem.DOWNLOADING)
				{
					outerInstance.downloadChunksMap[DateTimeHelperClass.CurrentUnixTimeMillis()] = bytesDownloadedSinceLastUpdate;
					if (stateChanged)
					{
						outerInstance.startPeriodicUpdates();
					}
				}
				if (resource.DownloadState != SKToolsDownloadItem.DOWNLOADING)
				{
					outerInstance.stopPeriodicUpdates();
				}
				if (stateChanged)
				{
					mapsDAO.updateMapResource((MapDownloadResource) resource);
				}

				outerInstance.appContext.AppPrefs.saveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			private class RunnableAnonymousInnerClassHelper3 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper3(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			public override void onDownloadCancelled(string currentDownloadItemCode)
			{
				outerInstance.stopPeriodicUpdates();
				ListItem affectedListItem = outerInstance.codesMap[currentDownloadItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.NoDownloadedBytes = 0;
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
					activeDownloads.Remove(affectedListItem.downloadResource);
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
					runOnUiThread(new RunnableAnonymousInnerClassHelper4(this));
				}
				else
				{
					DownloadResource downloadResource = allMapResources[currentDownloadItemCode];
					downloadResource.NoDownloadedBytes = 0;
					downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
					activeDownloads.Remove(downloadResource);
					mapsDAO.updateMapResource((MapDownloadResource) downloadResource);
				}
				outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
			}

			private class RunnableAnonymousInnerClassHelper4 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper4(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			public override void onAllDownloadsCancelled()
			{
				outerInstance.stopPeriodicUpdates();
				outerInstance.appContext.AppPrefs.saveDownloadStepPreference(0);
				foreach (DownloadResource downloadResource in activeDownloads)
				{
					downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
					downloadResource.NoDownloadedBytes = 0;
				}
				mapsDAO.clearResourcesInDownloadQueue();
				activeDownloads.Clear();
				outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
				runOnUiThread(new RunnableAnonymousInnerClassHelper5(this));
			}

			private class RunnableAnonymousInnerClassHelper5 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper5(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			public override void onDownloadPaused(SKToolsDownloadItem currentDownloadItem)
			{
				outerInstance.stopPeriodicUpdates();
				ListItem affectedListItem = outerInstance.codesMap[currentDownloadItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = currentDownloadItem.DownloadState;
					affectedListItem.downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
					runOnUiThread(new RunnableAnonymousInnerClassHelper6(this));
				}
				else
				{
					DownloadResource downloadResource = allMapResources[currentDownloadItem.ItemCode];
					downloadResource.DownloadState = currentDownloadItem.DownloadState;
					downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					mapsDAO.updateMapResource((MapDownloadResource) downloadResource);
				}

				outerInstance.appContext.AppPrefs.saveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			private class RunnableAnonymousInnerClassHelper6 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper6(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			public override void onInstallFinished(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = outerInstance.codesMap[currentInstallingItem.ItemCode];
				DownloadResource resource;
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.INSTALLED;
					resource = affectedListItem.downloadResource;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
					runOnUiThread(new RunnableAnonymousInnerClassHelper7(this));
				}
				else
				{
					resource = allMapResources[currentInstallingItem.ItemCode];
					resource.DownloadState = SKToolsDownloadItem.INSTALLED;
					mapsDAO.updateMapResource((MapDownloadResource) resource);
				}
				runOnUiThread(new RunnableAnonymousInnerClassHelper8(this, resource));
			}

			private class RunnableAnonymousInnerClassHelper7 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper7(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			private class RunnableAnonymousInnerClassHelper8 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				private DownloadResource resource;

				public RunnableAnonymousInnerClassHelper8(DownloadsAdapter outerInstance, DownloadResource resource)
				{
					this.outerInstance = outerInstance;
					this.resource = resource;
				}

				public override void run()
				{
					Toast.makeText(outerInstance.outerInstance.appContext, ((MapDownloadResource) resource).Name + " was installed", Toast.LENGTH_SHORT).show();
				}
			}

			public override void onInstallStarted(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = outerInstance.codesMap[currentInstallingItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.INSTALLING;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
					runOnUiThread(new RunnableAnonymousInnerClassHelper9(this));
				}
				else
				{
					DownloadResource downloadResource = allMapResources[currentInstallingItem.ItemCode];
					downloadResource.DownloadState = SKToolsDownloadItem.INSTALLING;
					mapsDAO.updateMapResource((MapDownloadResource) downloadResource);
				}
			}

			private class RunnableAnonymousInnerClassHelper9 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper9(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					outerInstance.notifyDataSetChanged();
				}
			}

			public override void onInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
			{
				outerInstance.stopPeriodicUpdates();
				outerInstance.appContext.AppPrefs.saveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			public override void onNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
			{
				runOnUiThread(new RunnableAnonymousInnerClassHelper10(this));
			}

			private class RunnableAnonymousInnerClassHelper10 : Runnable
			{
				private readonly DownloadsAdapter outerInstance;

				public RunnableAnonymousInnerClassHelper10(DownloadsAdapter outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void run()
				{
					Toast.makeText(outerInstance.outerInstance.ApplicationContext, "Not enough memory on the storage", Toast.LENGTH_SHORT).show();
				}
			}
		}

		public override void onBackPressed()
		{

			ListItem firstItem = currentListItems[0];
			if (firstItem.parent.parent == null)
			{
				base.onBackPressed();
			}
			else
			{
				currentListItems = currentListItems[0].parent.parent.children;
				buildCodesMap();
				updateListAndScrollToPosition(previousListIndexes.Pop());
			}
		}

		/// <summary>
		/// Triggers an update on the list and sets its position to the given value </summary>
		/// <param name="position"> </param>
		private void updateListAndScrollToPosition(int position)
		{
			listView.Visibility = View.INVISIBLE;
			adapter.notifyDataSetChanged();
			listView.post(new RunnableAnonymousInnerClassHelper(this, position));
		}

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			private readonly ResourceDownloadsListActivity outerInstance;

			private int position;

			public RunnableAnonymousInnerClassHelper(ResourceDownloadsListActivity outerInstance, int position)
			{
				this.outerInstance = outerInstance;
				this.position = position;
			}

			public override void run()
			{
				outerInstance.listView.Selection = position;
				outerInstance.listView.Visibility = View.VISIBLE;
			}
		}

		/// <summary>
		/// Formats a given value (provided in bytes)
		/// </summary>
		/// <param name="value"> value (in bytes) </param>
		/// <returns> formatted string (value and unit) </returns>
		public static string convertBytesToStringRepresentation(long value)
		{
			long[] dividers = new long[]{TERRA, GIGA, MEGA, KILO, 1};
			string[] units = new string[]{"TB", "GB", "MB", "KB", "B"};

			string result = null;
			for (int i = 0; i < dividers.Length; i++)
			{
				long divider = dividers[i];
				if (value >= divider)
				{
					result = formatDecimals(value, divider, units[i]);
					break;
				}
			}
			if (result != null)
			{
				return result;
			}
			else
			{
				return "0 B";
			}
		}

		/// <summary>
		/// Format the time value given as parameter (in milliseconds)
		/// </summary>
		/// <param name="time"> time value (provided in milliseconds) </param>
		/// <returns> formatted time </returns>
		public static string getFormattedTime(long time)
		{
			string format = string.Format("%0{0:D}d", 2);
			time = time / 1000;
			string seconds = string.format(format, time % 60);
			string minutes = string.format(format, (time % 3600) / 60);
			string hours = string.format(format, time / 3600);
			string formattedTime = hours + ":" + minutes + ":" + seconds;
			return formattedTime;
		}

		/// <summary>
		/// Formats decimal numbers
		/// </summary>
		/// <param name="value"> the value that needs to be formatted </param>
		/// <param name="divider"> the amount to divide the value to obtain the proper unit </param>
		/// <param name="unit"> unit of the result </param>
		/// <returns> formatted value </returns>
		private static string formatDecimals(long value, long divider, string unit)
		{
			double result = divider > 1 ? (double) value / (double) divider : (double) value;
			return (new DecimalFormat("#,##0.#")).format(result) + " " + unit;
		}

		/// <summary>
		/// Generates a list of download items based on the list of resources given as input
		/// </summary>
		/// <param name="downloadResources"> list of resources </param>
		/// <returns> a list of SKToolsDownloadItem objects </returns>
		private IList<SKToolsDownloadItem> createDownloadItemsFromDownloadResources(IList<DownloadResource> downloadResources)
		{
			IList<SKToolsDownloadItem> downloadItems = new List<SKToolsDownloadItem>();
			foreach (DownloadResource currentDownloadResource in downloadResources)
			{
				SKToolsDownloadItem currentItem = currentDownloadResource.toDownloadItem();
				if (currentDownloadResource.DownloadState == SKToolsDownloadItem.QUEUED)
				{
					currentItem.CurrentStepIndex = (sbyte) 0;
				}
				else if ((currentDownloadResource.DownloadState == SKToolsDownloadItem.PAUSED) || (currentDownloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING))
				{
					int downloadStepIndex = appContext.AppPrefs.getIntPreference(ApplicationPreferences.DOWNLOAD_STEP_INDEX_PREF_KEY);
					currentItem.CurrentStepIndex = (sbyte) downloadStepIndex;
				}
				downloadItems.Add(currentItem);
			}
			return downloadItems;
		}

		/// <summary>
		/// Click handler </summary>
		/// <param name="view"> </param>
		public virtual void onClick(View view)
		{
			if (view.Id == R.id.cancel_all_button)
			{
				bool cancelled = downloadManager.cancelAllDownloads();
				if (!cancelled)
				{
					foreach (DownloadResource resource in activeDownloads)
					{
						resource.NoDownloadedBytes = 0;
						resource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
					}
					activeDownloads.Clear();
					appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
					mapsDAO.clearResourcesInDownloadQueue();
					adapter.notifyDataSetChanged();
				}
			}
		}
	}
}