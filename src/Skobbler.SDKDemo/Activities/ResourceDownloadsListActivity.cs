using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.IO;
using Java.Lang;
using Java.Net;
using Java.Text;
using Newtonsoft.Json;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.SDKTools.Download;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Database;
using Skobbler.SDKDemo.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Console = System.Console;
using Math = System.Math;

namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity that displays a list of downloadable resources and provides the ability to download them
	/// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
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

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_downloads_list);
            appContext = (DemoApplication)Application;
            handler = new Handler();

            ListItem mapResourcesItem = new ListItem(this);

            bool success = await Task<bool>.Run(() => initializeMapResources());

            if (success)
            {
                populateWithChildMaps(mapResourcesItem);
                currentListItems = mapResourcesItem.children;

                listView = (ListView)FindViewById(Resource.Id.list_view);
                adapter = new DownloadsAdapter(this);
                listView.Adapter = adapter;
                FindViewById(Resource.Id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
                downloadManager = SKToolsDownloadManager.getInstance(adapter);

                if (activeDownloads.Count > 0 && activeDownloads[0].DownloadState == SKToolsDownloadItem.DOWNLOADING)
                {
                    startPeriodicUpdates();
                }
            }
            else
            {
                Toast.MakeText(this, "Could not retrieve map data from the server", ToastLength.Short).Show();
                Finish();
            }
        }

		/// <summary>
		/// Runnable used to trigger UI updates that refresh the download estimates (for current speed and remaining time)
		/// </summary>
        private void updater()
        {
            refreshDownloadEstimates = true;
            RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
            handler.PostDelayed(new Action(updater), 1000);
        }

		/// <summary>
		/// Starte periodic UI updates
		/// </summary>
		private void startPeriodicUpdates()
		{
			downloadStartTime = DateTimeUtil.JavaTime();
			handler.PostDelayed(updater, 3000);
		}

		/// <summary>
		/// Stops the periodic UI updates
		/// </summary>
		private void stopPeriodicUpdates()
		{
			downloadChunksMap.Clear();
			handler.RemoveCallbacks(updater);
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
						HttpURLConnection connection = (HttpURLConnection) (new URL(jsonUrl)).OpenConnection();
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
                string[] mapCodesArray = JsonConvert.DeserializeObject<string[]>(appContext.AppPrefs.getStringPreference(ApplicationPreferences.DOWNLOAD_QUEUE_PREF_KEY));
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
            childrenItems = childrenItems.OrderBy(x => x.name).ToList();
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
		private class DownloadsAdapter : BaseAdapter<ListItem>, ISKToolsDownloadListener
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

			public override long GetItemId(int i)
			{
				return 0;
			}

			public override View GetView(int position, View convertView, ViewGroup viewGroup)
			{
                ListItem currentItem = null;// GetItem(position);
				View view = null;
				if (convertView == null)
				{
                    LayoutInflater inflater = (LayoutInflater)outerInstance.GetSystemService(Context.LayoutInflaterService);
					view = inflater.Inflate(Resource.Layout.element_download_list_item, null);
				}
				else
				{
					view = convertView;
				}

				ImageView arrowImage = (ImageView) view.FindViewById(Resource.Id.arrow);
				TextView downloadSizeText = (TextView) view.FindViewById(Resource.Id.package_size);
				TextView downloadNameText = (TextView) view.FindViewById(Resource.Id.package_name);
				RelativeLayout middleLayout = (RelativeLayout) view.FindViewById(Resource.Id.middle_layout);
				ImageView startPauseImage = (ImageView) view.FindViewById(Resource.Id.start_pause);
				ImageView cancelImage = (ImageView) view.FindViewById(Resource.Id.cancel);
				TextView stateText = (TextView) view.FindViewById(Resource.Id.current_state);
				ProgressBar progressBar = (ProgressBar) view.FindViewById(Resource.Id.download_progress);
				RelativeLayout progressDetailsLayout = (RelativeLayout) view.FindViewById(Resource.Id.progress_details);
				TextView percentageText = (TextView) view.FindViewById(Resource.Id.percentage);
				TextView timeLeftText = (TextView) view.FindViewById(Resource.Id.time_left);
				TextView speedText = (TextView) view.FindViewById(Resource.Id.speed);

				downloadNameText.Text = currentItem.name;

				if (currentItem.children == null || currentItem.children.Count == 0)
				{
					arrowImage.Visibility = ViewStates.Gone;
				}
				else
				{
					arrowImage.Visibility = ViewStates.Visible;
				}

				if (currentItem.downloadResource != null)
				{

					DownloadResource downloadResource = currentItem.downloadResource;

					bool progressShown = downloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING || downloadResource.DownloadState == SKToolsDownloadItem.PAUSED;
					if (progressShown)
					{
						progressBar.Visibility = ViewStates.Visible;
						progressDetailsLayout.Visibility = ViewStates.Visible;
						progressBar.Progress = getPercentage(downloadResource);
						percentageText.Text = getPercentage(downloadResource) + "%";
						if (downloadResource.DownloadState == SKToolsDownloadItem.PAUSED)
						{
							timeLeftText.Text = "-";
							speedText.Text = "-";
						}
						else if (outerInstance.refreshDownloadEstimates)
						{
							Tuple<string, string> pair = calculateDownloadEstimates(downloadResource, 20);
							speedText.Text = pair.Item1;
                            timeLeftText.Text = pair.Item2;
							outerInstance.refreshDownloadEstimates = false;
						}
					}
					else
					{
						progressBar.Visibility = ViewStates.Gone;
						progressDetailsLayout.Visibility = ViewStates.Gone;
					}

					long bytesToDownload = 0;
					if (downloadResource is MapDownloadResource)
					{
						MapDownloadResource mapResource = (MapDownloadResource) downloadResource;
						bytesToDownload = mapResource.SkmAndZipFilesSize + mapResource.TXGFileSize;
					}

					if (bytesToDownload != 0)
					{
						middleLayout.Visibility = ViewStates.Visible;
						downloadSizeText.Visibility = ViewStates.Visible;
						downloadSizeText.Text = convertBytesToStringRepresentation(bytesToDownload);
					}
					else
					{
						middleLayout.Visibility = ViewStates.Gone;
						downloadSizeText.Visibility = ViewStates.Gone;
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
						startPauseImage.Visibility = ViewStates.Visible;
						if (downloadResource.DownloadState == SKToolsDownloadItem.DOWNLOADING)
						{
							startPauseImage.SetImageResource(Resource.Drawable.pause);
						}
						else
						{
                            startPauseImage.SetImageResource(Resource.Drawable.download);
						}
					}
					else
					{
						startPauseImage.Visibility = ViewStates.Gone;
					}

					if (downloadResource.DownloadState == SKToolsDownloadItem.NOT_QUEUED || downloadResource.DownloadState == SKToolsDownloadItem.INSTALLING)
					{
						cancelImage.Visibility = ViewStates.Gone;
					}
					else
					{
						cancelImage.Visibility = ViewStates.Visible;
					}

					if (downloadResource is MapDownloadResource)
					{
						MapDownloadResource mapResource = (MapDownloadResource) downloadResource;
					}

				}
				else
				{
					// no download resource
					downloadSizeText.Visibility = ViewStates.Gone;
					middleLayout.Visibility = ViewStates.Gone;
					progressBar.Visibility = ViewStates.Gone;
					progressDetailsLayout.Visibility = ViewStates.Gone;
					downloadSizeText.Visibility = ViewStates.Gone;
				}

                view.Click += (s, e) =>
                {
                    if (currentItem.children == null || currentItem.children.Count == 0)
                    {
                        return;
                    }

                    outerInstance.currentListItems = currentItem.children;
                    outerInstance.buildCodesMap();
                    outerInstance.previousListIndexes.Push(outerInstance.listView.FirstVisiblePosition);
                    outerInstance.updateListAndScrollToPosition(0);
                };

                startPauseImage.Click += (s, e) =>
                {
                    if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.DOWNLOADING)
                    {
                        if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.PAUSED)
                        {
                            activeDownloads.Add(currentItem.downloadResource);
                            currentItem.downloadResource.DownloadState = SKToolsDownloadItem.QUEUED;
                            outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
                            string destinationPath = outerInstance.appContext.MapResourcesDirPath + "downloads/";
                            File destinationFile = new File(destinationPath);
                            if (!destinationFile.Exists())
                            {
                                destinationFile.Mkdirs();
                            }
                            currentItem.downloadResource.DownloadPath = destinationPath;
                            mapsDAO.updateMapResource((MapDownloadResource)currentItem.downloadResource);
                        }

                        NotifyDataSetChanged();

                        IList<SKToolsDownloadItem> downloadItems;
                        if (!outerInstance.downloadManager.DownloadProcessRunning)
                        {
                            downloadItems = outerInstance.createDownloadItemsFromDownloadResources(activeDownloads);
                        }
                        else
                        {
                            IList<DownloadResource> mapDownloadResources = new List<DownloadResource>();
                            mapDownloadResources.Add(currentItem.downloadResource);
                            downloadItems = outerInstance.createDownloadItemsFromDownloadResources(mapDownloadResources);
                        }

                        foreach (SKToolsDownloadItem item in downloadItems)
                        {
                        }
                        outerInstance.downloadManager.startDownload(downloadItems);
                    }
                    else
                    {
                        outerInstance.downloadManager.pauseDownloadThread();
                    }
                };

                cancelImage.Click += (s,e) =>
                {
                    if (currentItem.downloadResource.DownloadState != SKToolsDownloadItem.INSTALLED)
                    {
                        bool downloadCancelled = outerInstance.downloadManager.cancelDownload(currentItem.downloadResource.Code);
                        if (!downloadCancelled)
                        {
                            currentItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
                            currentItem.downloadResource.NoDownloadedBytes = 0;
                            mapsDAO.updateMapResource((MapDownloadResource)currentItem.downloadResource);
                            activeDownloads.Remove(currentItem.downloadResource);
                            outerInstance.appContext.AppPrefs.saveDownloadQueuePreference(activeDownloads);
                            NotifyDataSetChanged();
                        }
                    }
                    else
                    {
                        bool packageDeleted = SKPackageManager.Instance.DeleteOfflinePackage(currentItem.downloadResource.Code);
                        if (packageDeleted)
                        {
                            Toast.MakeText(outerInstance.appContext, ((MapDownloadResource)currentItem.downloadResource).Name + " was uninstalled", ToastLength.Short).Show();
                        }
                        currentItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
                        currentItem.downloadResource.NoDownloadedBytes = 0;
                        mapsDAO.updateMapResource((MapDownloadResource)currentItem.downloadResource);
                        NotifyDataSetChanged();
                    }
                };

				return view;
			}

			/// <summary>
			/// Calculates download estimates (for current speed and remaining time) for the currently downloading resource.
			/// This estimate is based on how much was downloaded during the reference period.
			/// </summary>
			/// <param name="resource"> currently downloading resource </param>
			/// <param name="referencePeriodInSeconds"> the reference period (in seconds) </param>
			/// <returns> formatted string representations of the current download speed and remaining time </returns>
			internal virtual Tuple<string, string> calculateDownloadEstimates(DownloadResource resource, int referencePeriodInSeconds)
			{
				long referencePeriod = 1000 * referencePeriodInSeconds;
				long currentTimestamp = DateTimeUtil.JavaTime();
				long downloadPeriod = currentTimestamp - referencePeriod < outerInstance.downloadStartTime ? currentTimestamp - outerInstance.downloadStartTime : referencePeriod;
				long totalBytesDownloaded = 0;
				IEnumerator<KeyValuePair<long?, long?>> iterator = outerInstance.downloadChunksMap.GetEnumerator();
				while (iterator.MoveNext())
				{
					KeyValuePair<long?, long?> entry = iterator.Current;
					long timestamp = entry.Key.Value;
					long bytesDownloaded = entry.Value.Value;
					if (currentTimestamp - timestamp > referencePeriod)
					{
						//iterator.Remove();
					}
					else
					{
						totalBytesDownloaded += bytesDownloaded;
					}
				}
				float downloadPeriodSec = downloadPeriod / 1000f;
				long bytesPerSecond = (long)Math.Round(totalBytesDownloaded / downloadPeriodSec);
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

				return new Tuple<string, string>(convertBytesToStringRepresentation(bytesPerSecond) + "/s", formattedTimeLeft);
			}

			public override void NotifyDataSetChanged()
			{
				outerInstance.FindViewById(Resource.Id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
				base.NotifyDataSetChanged();
				outerInstance.listView.PostInvalidate();
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

			public void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem)
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
                    outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
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
					outerInstance.downloadChunksMap[DateTimeUtil.JavaTime()] = bytesDownloadedSinceLastUpdate;
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

			public void OnDownloadCancelled(string currentDownloadItemCode)
			{
				outerInstance.stopPeriodicUpdates();
				ListItem affectedListItem = outerInstance.codesMap[currentDownloadItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.NoDownloadedBytes = 0;
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.NOT_QUEUED;
					activeDownloads.Remove(affectedListItem.downloadResource);
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
                    outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
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

			public void OnAllDownloadsCancelled()
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
                outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
			}

			public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
			{
				outerInstance.stopPeriodicUpdates();
				ListItem affectedListItem = outerInstance.codesMap[currentDownloadItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = currentDownloadItem.DownloadState;
					affectedListItem.downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
                    outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
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

			public void OnInstallFinished(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = outerInstance.codesMap[currentInstallingItem.ItemCode];
				DownloadResource resource;
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.INSTALLED;
					resource = affectedListItem.downloadResource;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
                    outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					resource = allMapResources[currentInstallingItem.ItemCode];
					resource.DownloadState = SKToolsDownloadItem.INSTALLED;
					mapsDAO.updateMapResource((MapDownloadResource) resource);
				}
                outerInstance.RunOnUiThread(() => { Toast.MakeText(outerInstance.appContext, ((MapDownloadResource)resource).Name + " was installed", ToastLength.Short).Show(); });
			}

			public void OnInstallStarted(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = outerInstance.codesMap[currentInstallingItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.downloadResource.DownloadState = SKToolsDownloadItem.INSTALLING;
					mapsDAO.updateMapResource((MapDownloadResource) affectedListItem.downloadResource);
                    outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					DownloadResource downloadResource = allMapResources[currentInstallingItem.ItemCode];
					downloadResource.DownloadState = SKToolsDownloadItem.INSTALLING;
					mapsDAO.updateMapResource((MapDownloadResource) downloadResource);
				}
			}

			public void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
			{
				outerInstance.stopPeriodicUpdates();
				outerInstance.appContext.AppPrefs.saveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
			{
                outerInstance.RunOnUiThread(() => { Toast.MakeText(outerInstance.ApplicationContext, "Not enough memory on the storage", ToastLength.Short).Show(); });
			}

            public override ListItem this[int position]
            {
                get { return outerInstance.currentListItems[position]; }
            }
        }

		public override void OnBackPressed()
		{

			ListItem firstItem = currentListItems[0];
			if (firstItem.parent.parent == null)
			{
				base.OnBackPressed();
			}
			else
			{
				currentListItems = currentListItems[0].parent.parent.children;
				buildCodesMap();
				updateListAndScrollToPosition(previousListIndexes.Pop().Value);
			}
		}

		/// <summary>
		/// Triggers an update on the list and sets its position to the given value </summary>
		/// <param name="position"> </param>
		private void updateListAndScrollToPosition(int position)
		{
			listView.Visibility = ViewStates.Invisible;
			adapter.NotifyDataSetChanged();
            listView.Post(() =>
            {
                listView.SetSelection(position);
                listView.Visibility = ViewStates.Visible;
            });
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
			string seconds = string.Format(format, time % 60);
			string minutes = string.Format(format, (time % 3600) / 60);
			string hours = string.Format(format, time / 3600);
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
			return (new DecimalFormat("#,##0.#")).Format(result) + " " + unit;
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
        [Export("OnClick")]
		public virtual void onClick(View view)
		{
			if (view.Id == Resource.Id.cancel_all_button)
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
					adapter.NotifyDataSetChanged();
				}
			}
		}
	}
}