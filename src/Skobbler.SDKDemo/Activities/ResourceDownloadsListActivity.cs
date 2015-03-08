using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.IO;
using Java.Net;
using Java.Text;
using Newtonsoft.Json;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.SDKTools.Download;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Database;
using Skobbler.SDKDemo.Util;
using Console = System.Console;

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
		public const long Kilo = 1024;

		public static readonly long Mega = Kilo * Kilo;

		public static readonly long Giga = Mega * Kilo;

		public static readonly long Terra = Giga * Kilo;

		/// <summary>
		/// Download manager used for controlling the download process
		/// </summary>
		private SKToolsDownloadManager _downloadManager;

		/// <summary>
		/// Adapter for download items
		/// </summary>
		private DownloadsAdapter _adapter;

		/// <summary>
		/// List element displaying download items
		/// </summary>
		private ListView _listView;

		/// <summary>
		/// List of items in the current screen
		/// </summary>
		private IList<ListItem> _currentListItems;

		/// <summary>
		/// Map from resource codes to items
		/// </summary>
		private IDictionary<string, ListItem> _codesMap = new Dictionary<string, ListItem>();

		/// <summary>
		/// List of all map resources
		/// </summary>
		public static IDictionary<string, MapDownloadResource> AllMapResources;

		/// <summary>
		/// List of downloads which are currently in progress
		/// </summary>
		public static IList<DownloadResource> ActiveDownloads = new List<DownloadResource>();

		/// <summary>
		/// DAO object for accessing the maps database
		/// </summary>
		public static MapsDao MapsDao;

		/// <summary>
		/// Stack containing list indexes for opened screens
		/// </summary>
		private Stack<int?> _previousListIndexes = new Stack<int?>();

		/// <summary>
		/// Context object
		/// </summary>
		private DemoApplication _appContext;

		private IDictionary<long?, long?> _downloadChunksMap = new SortedDictionary<long?, long?>();

		/// <summary>
		/// Handler object used for scheduling periodic UI updates while downloading is in progress
		/// </summary>
		private Handler _handler;

		/// <summary>
		/// True if download estimates should be refreshed at next UI update
		/// </summary>
		private bool _refreshDownloadEstimates;

		/// <summary>
		/// Timestamp at which last download started
		/// </summary>
		private long _downloadStartTime;

		/// <summary>
		/// Item in the download list
		/// </summary>
		private class ListItem : IComparable<ListItem>
		{
			private readonly ResourceDownloadsListActivity _outerInstance;

			public ListItem(ResourceDownloadsListActivity outerInstance)
			{
				_outerInstance = outerInstance;
			}


			internal string Name;

			internal DownloadResource DownloadResource;

			internal IList<ListItem> Children;

			internal ListItem parent;

			public virtual int CompareTo(ListItem listItem)
			{
				if (listItem != null && listItem.Name != null && Name != null)
				{
					return Name.CompareTo(listItem.Name);
				}
				return 0;
			}
		}

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_downloads_list);
            _appContext = (DemoApplication)Application;
            _handler = new Handler();

            ListItem mapResourcesItem = new ListItem(this);

            bool success = await Task<bool>.Run(() => InitializeMapResources());

            if (success)
            {
                PopulateWithChildMaps(mapResourcesItem);
                _currentListItems = mapResourcesItem.Children;

                _listView = (ListView)FindViewById(Resource.Id.list_view);
                _adapter = new DownloadsAdapter(this);
                _listView.Adapter = _adapter;
                FindViewById(Resource.Id.cancel_all_button).Visibility = ActiveDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
                _downloadManager = SKToolsDownloadManager.GetInstance(_adapter);

                if (ActiveDownloads.Count > 0 && ActiveDownloads[0].DownloadState == SKToolsDownloadItem.Downloading)
                {
                    StartPeriodicUpdates();
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
        private void Updater()
        {
            _refreshDownloadEstimates = true;
            RunOnUiThread(() => { _adapter.NotifyDataSetChanged(); });
            _handler.PostDelayed(Updater, 1000);
        }

		/// <summary>
		/// Starte periodic UI updates
		/// </summary>
		private void StartPeriodicUpdates()
		{
			_downloadStartTime = DateTimeUtil.JavaTime();
			_handler.PostDelayed(Updater, 3000);
		}

		/// <summary>
		/// Stops the periodic UI updates
		/// </summary>
		private void StopPeriodicUpdates()
		{
			_downloadChunksMap.Clear();
			_handler.RemoveCallbacks(Updater);
		}

		/// <summary>
		/// Initializes the map resources (reads them from the database if they are available there or parses them otherwise and stores them in the database)
		/// </summary>
		private bool InitializeMapResources()
		{
			MapsDao = ResourcesDaoHandler.GetInstance(this).MapsDao;
			if (AllMapResources == null)
			{
				AllMapResources = MapsDao.GetAvailableMapsForACertainType(null);
				if (AllMapResources == null)
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
						(new MapDataParser()).ParseMapJsonData(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, connection.InputStream);
						// populate DB maps table with parsing results
						MapsDao.InsertMaps(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, this);
						// get all map resources
						AllMapResources = MapsDao.GetAvailableMapsForACertainType(null);
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
				ActiveDownloads = ActiveMapDownloads;
				if (ActiveDownloads.Count > 0 && ActiveDownloads[0].DownloadState == SKToolsDownloadItem.Downloading)
				{
					// pausing first download in queue, if it's in downloading state
					ActiveDownloads[0].DownloadState = SKToolsDownloadItem.Paused;
					MapsDao.UpdateMapResource((MapDownloadResource) ActiveDownloads[0]);
				}
			}

			return AllMapResources != null && AllMapResources.Count > 0;
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
                string[] mapCodesArray = JsonConvert.DeserializeObject<string[]>(_appContext.AppPrefs.GetStringPreference(ApplicationPreferences.DownloadQueuePrefKey));
				if (mapCodesArray == null)
				{
					return activeMapDownloads;
				}
				foreach (string mapCode in mapCodesArray)
				{
					activeMapDownloads.Add(AllMapResources[mapCode]);
				}
				return activeMapDownloads;
			}
		}

		/// <summary>
		/// Recursively populates list items with the corresponding parent & child data
		/// </summary>
		/// <param name="mapItem"> </param>
		private void PopulateWithChildMaps(ListItem mapItem)
		{
			string code;
			if (mapItem.DownloadResource == null)
			{
				code = "";
			}
			else
			{
				code = mapItem.DownloadResource.Code;
			}

			IList<ListItem> childrenItems = GetChildrenOf(code);
            childrenItems = childrenItems.OrderBy(x => x.Name).ToList();
			foreach (ListItem childItem in childrenItems)
			{
				childItem.parent = mapItem;
				PopulateWithChildMaps(childItem);
			}
			mapItem.Children = childrenItems;
		}

		/// <summary>
		/// Gets the list of child items for a given code
		/// </summary>
		/// <param name="parentCode">
		/// @return </param>
		private IList<ListItem> GetChildrenOf(string parentCode)
		{
			IList<ListItem> children = new List<ListItem>();
			foreach (MapDownloadResource mapResource in AllMapResources.Values)
			{
				if (mapResource.ParentCode.Equals(parentCode))
				{
					ListItem listItem = new ListItem(this);
					listItem.Name = mapResource.Name;
					listItem.DownloadResource = mapResource;
					children.Add(listItem);
				}
			}
			return children;
		}

		/// <summary>
		/// Constructs a map from resource codes (keys )to list items (values) for the items currently displayed
		/// </summary>
		private void BuildCodesMap()
		{
			foreach (ListItem item in _currentListItems)
			{
				_codesMap[item.DownloadResource.Code] = item;
			}
		}

		/// <summary>
		/// Represents the adapter associated with maps list
		/// </summary>
		private class DownloadsAdapter : BaseAdapter<ListItem>, ISKToolsDownloadListener
		{
			private readonly ResourceDownloadsListActivity _outerInstance;

			public DownloadsAdapter(ResourceDownloadsListActivity outerInstance)
			{
				_outerInstance = outerInstance;
			}


			public override int Count
			{
				get
				{
					return _outerInstance._currentListItems.Count;
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
                    LayoutInflater inflater = (LayoutInflater)_outerInstance.GetSystemService(LayoutInflaterService);
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

				downloadNameText.Text = currentItem.Name;

				if (currentItem.Children == null || currentItem.Children.Count == 0)
				{
					arrowImage.Visibility = ViewStates.Gone;
				}
				else
				{
					arrowImage.Visibility = ViewStates.Visible;
				}

				if (currentItem.DownloadResource != null)
				{

					DownloadResource downloadResource = currentItem.DownloadResource;

					bool progressShown = downloadResource.DownloadState == SKToolsDownloadItem.Downloading || downloadResource.DownloadState == SKToolsDownloadItem.Paused;
					if (progressShown)
					{
						progressBar.Visibility = ViewStates.Visible;
						progressDetailsLayout.Visibility = ViewStates.Visible;
						progressBar.Progress = GetPercentage(downloadResource);
						percentageText.Text = GetPercentage(downloadResource) + "%";
						if (downloadResource.DownloadState == SKToolsDownloadItem.Paused)
						{
							timeLeftText.Text = "-";
							speedText.Text = "-";
						}
						else if (_outerInstance._refreshDownloadEstimates)
						{
							Tuple<string, string> pair = CalculateDownloadEstimates(downloadResource, 20);
							speedText.Text = pair.Item1;
                            timeLeftText.Text = pair.Item2;
							_outerInstance._refreshDownloadEstimates = false;
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
						bytesToDownload = mapResource.SkmAndZipFilesSize + mapResource.TxgFileSize;
					}

					if (bytesToDownload != 0)
					{
						middleLayout.Visibility = ViewStates.Visible;
						downloadSizeText.Visibility = ViewStates.Visible;
						downloadSizeText.Text = ConvertBytesToStringRepresentation(bytesToDownload);
					}
					else
					{
						middleLayout.Visibility = ViewStates.Gone;
						downloadSizeText.Visibility = ViewStates.Gone;
					}

					switch (downloadResource.DownloadState)
					{
						case SKToolsDownloadItem.NotQueued:
							stateText.Text = "NOT QUEUED";
							break;
						case SKToolsDownloadItem.Queued:
							stateText.Text = "QUEUED";
							break;
						case SKToolsDownloadItem.Downloading:
							stateText.Text = "DOWNLOADING";
							break;
						case SKToolsDownloadItem.Downloaded:
							stateText.Text = "DOWNLOADED";
							break;
						case SKToolsDownloadItem.Paused:
							stateText.Text = "PAUSED";
							break;
						case SKToolsDownloadItem.Installing:
							stateText.Text = "INSTALLING";
							break;
						case SKToolsDownloadItem.Installed:
							stateText.Text = "INSTALLED";
							break;
						default:
					break;
					}

					if (downloadResource.DownloadState == SKToolsDownloadItem.NotQueued || downloadResource.DownloadState == SKToolsDownloadItem.Downloading || downloadResource.DownloadState == SKToolsDownloadItem.Paused)
					{
						startPauseImage.Visibility = ViewStates.Visible;
						if (downloadResource.DownloadState == SKToolsDownloadItem.Downloading)
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

					if (downloadResource.DownloadState == SKToolsDownloadItem.NotQueued || downloadResource.DownloadState == SKToolsDownloadItem.Installing)
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
                    if (currentItem.Children == null || currentItem.Children.Count == 0)
                    {
                        return;
                    }

                    _outerInstance._currentListItems = currentItem.Children;
                    _outerInstance.BuildCodesMap();
                    _outerInstance._previousListIndexes.Push(_outerInstance._listView.FirstVisiblePosition);
                    _outerInstance.UpdateListAndScrollToPosition(0);
                };

                startPauseImage.Click += (s, e) =>
                {
                    if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Downloading)
                    {
                        if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Paused)
                        {
                            ActiveDownloads.Add(currentItem.DownloadResource);
                            currentItem.DownloadResource.DownloadState = SKToolsDownloadItem.Queued;
                            _outerInstance._appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
                            string destinationPath = _outerInstance._appContext.MapResourcesDirPath + "downloads/";
                            File destinationFile = new File(destinationPath);
                            if (!destinationFile.Exists())
                            {
                                destinationFile.Mkdirs();
                            }
                            currentItem.DownloadResource.DownloadPath = destinationPath;
                            MapsDao.UpdateMapResource((MapDownloadResource)currentItem.DownloadResource);
                        }

                        NotifyDataSetChanged();

                        IList<SKToolsDownloadItem> downloadItems;
                        if (!_outerInstance._downloadManager.DownloadProcessRunning)
                        {
                            downloadItems = _outerInstance.CreateDownloadItemsFromDownloadResources(ActiveDownloads);
                        }
                        else
                        {
                            IList<DownloadResource> mapDownloadResources = new List<DownloadResource>();
                            mapDownloadResources.Add(currentItem.DownloadResource);
                            downloadItems = _outerInstance.CreateDownloadItemsFromDownloadResources(mapDownloadResources);
                        }

                        foreach (SKToolsDownloadItem item in downloadItems)
                        {
                        }
                        _outerInstance._downloadManager.StartDownload(downloadItems);
                    }
                    else
                    {
                        _outerInstance._downloadManager.PauseDownloadThread();
                    }
                };

                cancelImage.Click += (s,e) =>
                {
                    if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Installed)
                    {
                        bool downloadCancelled = _outerInstance._downloadManager.CancelDownload(currentItem.DownloadResource.Code);
                        if (!downloadCancelled)
                        {
                            currentItem.DownloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
                            currentItem.DownloadResource.NoDownloadedBytes = 0;
                            MapsDao.UpdateMapResource((MapDownloadResource)currentItem.DownloadResource);
                            ActiveDownloads.Remove(currentItem.DownloadResource);
                            _outerInstance._appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
                            NotifyDataSetChanged();
                        }
                    }
                    else
                    {
                        bool packageDeleted = SKPackageManager.Instance.DeleteOfflinePackage(currentItem.DownloadResource.Code);
                        if (packageDeleted)
                        {
                            Toast.MakeText(_outerInstance._appContext, ((MapDownloadResource)currentItem.DownloadResource).Name + " was uninstalled", ToastLength.Short).Show();
                        }
                        currentItem.DownloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
                        currentItem.DownloadResource.NoDownloadedBytes = 0;
                        MapsDao.UpdateMapResource((MapDownloadResource)currentItem.DownloadResource);
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
			internal virtual Tuple<string, string> CalculateDownloadEstimates(DownloadResource resource, int referencePeriodInSeconds)
			{
				long referencePeriod = 1000 * referencePeriodInSeconds;
				long currentTimestamp = DateTimeUtil.JavaTime();
				long downloadPeriod = currentTimestamp - referencePeriod < _outerInstance._downloadStartTime ? currentTimestamp - _outerInstance._downloadStartTime : referencePeriod;
				long totalBytesDownloaded = 0;
				IEnumerator<KeyValuePair<long?, long?>> iterator = _outerInstance._downloadChunksMap.GetEnumerator();
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
					long remainingBytes = (mapResource.SkmAndZipFilesSize + mapResource.TxgFileSize) - mapResource.NoDownloadedBytes;
					long timeLeft = (downloadPeriod * remainingBytes) / totalBytesDownloaded;
					formattedTimeLeft = GetFormattedTime(timeLeft);
				}

				return new Tuple<string, string>(ConvertBytesToStringRepresentation(bytesPerSecond) + "/s", formattedTimeLeft);
			}

			public override void NotifyDataSetChanged()
			{
				_outerInstance.FindViewById(Resource.Id.cancel_all_button).Visibility = ActiveDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
				base.NotifyDataSetChanged();
				_outerInstance._listView.PostInvalidate();
			}

			/// <summary>
			/// Gets a percentage of how much was downloaded from the given resource
			/// </summary>
			/// <param name="downloadResource"> download resource </param>
			/// <returns> perecntage value </returns>
			internal virtual int GetPercentage(DownloadResource downloadResource)
			{
				int percentage = 0;
				if (downloadResource is MapDownloadResource)
				{
					MapDownloadResource mapDownloadResource = (MapDownloadResource) downloadResource;
					percentage = (int)(((float) mapDownloadResource.NoDownloadedBytes / (mapDownloadResource.SkmAndZipFilesSize + mapDownloadResource.TxgFileSize)) * 100);
				}
				return percentage;
			}

			public void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem)
			{
				ListItem affectedListItem = _outerInstance._codesMap[currentDownloadItem.ItemCode];
				DownloadResource resource;
				bool stateChanged = false;
				long bytesDownloadedSinceLastUpdate = 0;
				if (affectedListItem != null)
				{
					stateChanged = currentDownloadItem.DownloadState != affectedListItem.DownloadResource.DownloadState;
					bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - affectedListItem.DownloadResource.NoDownloadedBytes;
					affectedListItem.DownloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					affectedListItem.DownloadResource.DownloadState = currentDownloadItem.DownloadState;
					resource = affectedListItem.DownloadResource;
                    _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					resource = AllMapResources[currentDownloadItem.ItemCode];
					bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - resource.NoDownloadedBytes;
					stateChanged = currentDownloadItem.DownloadState != resource.DownloadState;
					resource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					resource.DownloadState = currentDownloadItem.DownloadState;
				}
				if (resource.DownloadState == SKToolsDownloadItem.Downloaded)
				{
					ActiveDownloads.Remove(resource);
					_outerInstance._appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
				}
				else if (resource.DownloadState == SKToolsDownloadItem.Downloading)
				{
					_outerInstance._downloadChunksMap[DateTimeUtil.JavaTime()] = bytesDownloadedSinceLastUpdate;
					if (stateChanged)
					{
						_outerInstance.StartPeriodicUpdates();
					}
				}
				if (resource.DownloadState != SKToolsDownloadItem.Downloading)
				{
					_outerInstance.StopPeriodicUpdates();
				}
				if (stateChanged)
				{
					MapsDao.UpdateMapResource((MapDownloadResource) resource);
				}

				_outerInstance._appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			public void OnDownloadCancelled(string currentDownloadItemCode)
			{
				_outerInstance.StopPeriodicUpdates();
				ListItem affectedListItem = _outerInstance._codesMap[currentDownloadItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.DownloadResource.NoDownloadedBytes = 0;
					affectedListItem.DownloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
					ActiveDownloads.Remove(affectedListItem.DownloadResource);
					MapsDao.UpdateMapResource((MapDownloadResource) affectedListItem.DownloadResource);
                    _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					DownloadResource downloadResource = AllMapResources[currentDownloadItemCode];
					downloadResource.NoDownloadedBytes = 0;
					downloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
					ActiveDownloads.Remove(downloadResource);
					MapsDao.UpdateMapResource((MapDownloadResource) downloadResource);
				}
				_outerInstance._appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
			}

			public void OnAllDownloadsCancelled()
			{
				_outerInstance.StopPeriodicUpdates();
				_outerInstance._appContext.AppPrefs.SaveDownloadStepPreference(0);
				foreach (DownloadResource downloadResource in ActiveDownloads)
				{
					downloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
					downloadResource.NoDownloadedBytes = 0;
				}
				MapsDao.ClearResourcesInDownloadQueue();
				ActiveDownloads.Clear();
				_outerInstance._appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
                _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
			}

			public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
			{
				_outerInstance.StopPeriodicUpdates();
				ListItem affectedListItem = _outerInstance._codesMap[currentDownloadItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.DownloadResource.DownloadState = currentDownloadItem.DownloadState;
					affectedListItem.DownloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					MapsDao.UpdateMapResource((MapDownloadResource) affectedListItem.DownloadResource);
                    _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					DownloadResource downloadResource = AllMapResources[currentDownloadItem.ItemCode];
					downloadResource.DownloadState = currentDownloadItem.DownloadState;
					downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
					MapsDao.UpdateMapResource((MapDownloadResource) downloadResource);
				}

				_outerInstance._appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			public void OnInstallFinished(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = _outerInstance._codesMap[currentInstallingItem.ItemCode];
				DownloadResource resource;
				if (affectedListItem != null)
				{
					affectedListItem.DownloadResource.DownloadState = SKToolsDownloadItem.Installed;
					resource = affectedListItem.DownloadResource;
					MapsDao.UpdateMapResource((MapDownloadResource) affectedListItem.DownloadResource);
                    _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					resource = AllMapResources[currentInstallingItem.ItemCode];
					resource.DownloadState = SKToolsDownloadItem.Installed;
					MapsDao.UpdateMapResource((MapDownloadResource) resource);
				}
                _outerInstance.RunOnUiThread(() => { Toast.MakeText(_outerInstance._appContext, ((MapDownloadResource)resource).Name + " was installed", ToastLength.Short).Show(); });
			}

			public void OnInstallStarted(SKToolsDownloadItem currentInstallingItem)
			{
				ListItem affectedListItem = _outerInstance._codesMap[currentInstallingItem.ItemCode];
				if (affectedListItem != null)
				{
					affectedListItem.DownloadResource.DownloadState = SKToolsDownloadItem.Installing;
					MapsDao.UpdateMapResource((MapDownloadResource) affectedListItem.DownloadResource);
                    _outerInstance.RunOnUiThread(() => { NotifyDataSetChanged(); });
				}
				else
				{
					DownloadResource downloadResource = AllMapResources[currentInstallingItem.ItemCode];
					downloadResource.DownloadState = SKToolsDownloadItem.Installing;
					MapsDao.UpdateMapResource((MapDownloadResource) downloadResource);
				}
			}

			public void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
			{
				_outerInstance.StopPeriodicUpdates();
				_outerInstance._appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
			}

			public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
			{
                _outerInstance.RunOnUiThread(() => { Toast.MakeText(_outerInstance.ApplicationContext, "Not enough memory on the storage", ToastLength.Short).Show(); });
			}

            public override ListItem this[int position]
            {
                get { return _outerInstance._currentListItems[position]; }
            }
        }

		public override void OnBackPressed()
		{

			ListItem firstItem = _currentListItems[0];
			if (firstItem.parent.parent == null)
			{
				base.OnBackPressed();
			}
			else
			{
				_currentListItems = _currentListItems[0].parent.parent.Children;
				BuildCodesMap();
				UpdateListAndScrollToPosition(_previousListIndexes.Pop().Value);
			}
		}

		/// <summary>
		/// Triggers an update on the list and sets its position to the given value </summary>
		/// <param name="position"> </param>
		private void UpdateListAndScrollToPosition(int position)
		{
			_listView.Visibility = ViewStates.Invisible;
			_adapter.NotifyDataSetChanged();
            _listView.Post(() =>
            {
                _listView.SetSelection(position);
                _listView.Visibility = ViewStates.Visible;
            });
		}

		/// <summary>
		/// Formats a given value (provided in bytes)
		/// </summary>
		/// <param name="value"> value (in bytes) </param>
		/// <returns> formatted string (value and unit) </returns>
		public static string ConvertBytesToStringRepresentation(long value)
		{
			long[] dividers = {Terra, Giga, Mega, Kilo, 1};
			string[] units = {"TB", "GB", "MB", "KB", "B"};

			string result = null;
			for (int i = 0; i < dividers.Length; i++)
			{
				long divider = dividers[i];
				if (value >= divider)
				{
					result = FormatDecimals(value, divider, units[i]);
					break;
				}
			}
			if (result != null)
			{
				return result;
			}
		    return "0 B";
		}

		/// <summary>
		/// Format the time value given as parameter (in milliseconds)
		/// </summary>
		/// <param name="time"> time value (provided in milliseconds) </param>
		/// <returns> formatted time </returns>
		public static string GetFormattedTime(long time)
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
		private static string FormatDecimals(long value, long divider, string unit)
		{
			double result = divider > 1 ? value / (double) divider : value;
			return (new DecimalFormat("#,##0.#")).Format(result) + " " + unit;
		}

		/// <summary>
		/// Generates a list of download items based on the list of resources given as input
		/// </summary>
		/// <param name="downloadResources"> list of resources </param>
		/// <returns> a list of SKToolsDownloadItem objects </returns>
		private IList<SKToolsDownloadItem> CreateDownloadItemsFromDownloadResources(IList<DownloadResource> downloadResources)
		{
			IList<SKToolsDownloadItem> downloadItems = new List<SKToolsDownloadItem>();
			foreach (DownloadResource currentDownloadResource in downloadResources)
			{
				SKToolsDownloadItem currentItem = currentDownloadResource.ToDownloadItem();
				if (currentDownloadResource.DownloadState == SKToolsDownloadItem.Queued)
				{
					currentItem.CurrentStepIndex = 0;
				}
				else if ((currentDownloadResource.DownloadState == SKToolsDownloadItem.Paused) || (currentDownloadResource.DownloadState == SKToolsDownloadItem.Downloading))
				{
					int downloadStepIndex = _appContext.AppPrefs.GetIntPreference(ApplicationPreferences.DownloadStepIndexPrefKey);
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
		public virtual void OnClick(View view)
		{
			if (view.Id == Resource.Id.cancel_all_button)
			{
				bool cancelled = _downloadManager.CancelAllDownloads();
				if (!cancelled)
				{
					foreach (DownloadResource resource in ActiveDownloads)
					{
						resource.NoDownloadedBytes = 0;
						resource.DownloadState = SKToolsDownloadItem.NotQueued;
					}
					ActiveDownloads.Clear();
					_appContext.AppPrefs.SaveDownloadQueuePreference(ActiveDownloads);
					MapsDao.ClearResourcesInDownloadQueue();
					_adapter.NotifyDataSetChanged();
				}
			}
		}
	}
}