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
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Database;
using Console = System.Console;
using Skobbler.Ngx.SDKTools.Download;
using System.Timers;
using Android.Content;
using Skobbler.SDKDemo.Util;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class ResourceDownloadsListActivity : Activity
    {
        public static long KILO = 1024;
        public static long MEGA = KILO * KILO;
        public static long GIGA = MEGA * KILO;
        public static long TERRA = GIGA * KILO;

        private SKToolsDownloadManager downloadManager;
        private DownloadsAdapter adapter;
        private ListView listView;
        private List<ListItem> currentListItems;
        private Dictionary<string, ListItem> codesMap = new Dictionary<String, ListItem>();
        public static Dictionary<String, MapDownloadResource> allMapResources;
        public static List<DownloadResource> activeDownloads = new List<DownloadResource>();
        public static MapsDAO mapsDAO;
        private Stack<int> previousListIndexes = new Stack<int>();
        private DemoApplication appContext;
        private Dictionary<long, long> downloadChunksMap = new Dictionary<long, long>();
        private Timer handler;
        private bool refreshDownloadEstimates;
        private long downloadStartTime;

        private class ListItem : IComparable<ListItem>
        {

            public string Name { get; set; }

            public DownloadResource DownloadResource { get; set; }

            public List<ListItem> Children { get; set; }

            public ListItem Parent { get; set; }

            public int CompareTo(ListItem listItem)
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
            appContext = (DemoApplication)Application;
            handler = new Timer();

            ListItem mapResourcesItem = new ListItem();

            await Task.Run(() => initializeMapResources())
                .ContinueWith((t) =>
                {
                    if (t.Result)
                    {
                        appContext.AppPrefs.SaveBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED, false);
                        populateWithChildMaps(mapResourcesItem);
                        currentListItems = mapResourcesItem.Children;

                        listView = (ListView)FindViewById(Resource.Id.list_view);
                        adapter = new DownloadsAdapter(this);
                        listView.Adapter = adapter;
                        FindViewById(Resource.Id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
                        downloadManager = SKToolsDownloadManager.GetInstance(adapter);
                        if (activeDownloads.Count != 0 && activeDownloads[0].DownloadState == SKToolsDownloadItem.Downloading)
                        {
                            StartPeriodicUpdates();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "Could not retrieve map data from the server", ToastLength.Short).Show();
                        Finish();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        ///**
        // * Runnable used to trigger UI updates that refresh the download estimates (for current speed and remaining time)
        // */
        //private Action updater = () =>
        //{
        //    refreshDownloadEstimates = true;
        //    RunOnUiThread(() => adapter.NotifyDataSetChanged());
        //    handler.Interval = 1000;
        //    handler.Elapsed += () => { };
        //    handler.Start();
        //};

        private void StartPeriodicUpdates()
        {
            downloadStartTime = DemoUtils.CurrentTimeMillis();
            //handler.PostDelayed(updater, 3000);
        }

        private void StopPeriodicUpdates()
        {
            downloadChunksMap.Clear();
            handler.Stop();
        }

        private bool initializeMapResources()
        {
            mapsDAO = ResourcesDAOHandler.GetInstance(this).getMapsDAO();

            if (allMapResources == null || appContext.AppPrefs.GetBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED))
            {

                if (appContext.AppPrefs.GetBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED))
                {
                    mapsDAO.deleteMaps();
                }

                allMapResources = mapsDAO.getAvailableMapsForACertainType(null);
                if (allMapResources == null || appContext.AppPrefs.GetBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED))
                {
                    // maps table in DB not populated yet or needs to be updated
                    List<MapDownloadResource> parsedMapResources = new List<MapDownloadResource>();
                    Dictionary<string, string> parsedMapItemsCodes = new Dictionary<string, string>();
                    Dictionary<string, string> regionItemsCodes = new Dictionary<string, string>();
                    try
                    {
                        // parse Maps.json
                        string jsonUrl = SKPackageManager.Instance.MapsJSONPathForCurrentVersion;
                        HttpURLConnection connection = (HttpURLConnection)new URL(jsonUrl).OpenConnection();
                        new MapDataParser().parseMapJsonData(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, connection.InputStream);
                        // populate DB maps table with parsing results
                        mapsDAO.insertMaps(parsedMapResources, parsedMapItemsCodes, regionItemsCodes, this);
                        // get all map resources
                        allMapResources = mapsDAO.getAvailableMapsForACertainType(null);
                    }
                    catch (IOException e)
                    {
                        e.PrintStackTrace();
                    }
                }
                if (appContext.AppPrefs.GetBooleanPreference(ApplicationPreferences.MAP_RESOURCES_UPDATE_NEEDED))
                {
                    activeDownloads = new List<DownloadResource>();
                }
                else
                {
                    activeDownloads = getActiveMapDownloads();
                    if (activeDownloads.Count != 0 && activeDownloads[0].DownloadState == SKToolsDownloadItem.Downloading)
                    {
                        activeDownloads[0].DownloadState = SKToolsDownloadItem.Paused;
                        mapsDAO.updateMapResource((MapDownloadResource)activeDownloads[0]);
                    }
                }
            }

            return allMapResources != null && allMapResources.Count > 0;
        }

        private List<DownloadResource> getActiveMapDownloads()
        {
            List<DownloadResource> activeMapDownloads = new List<DownloadResource>();
            //String[] mapCodesArray = new Gson().fromJson(appContext.AppPrefs.GetStringPreference(ApplicationPreferences.DOWNLOAD_QUEUE_PREF_KEY), typeof(string[]));
            string[] mapCodesArray = JsonConvert.DeserializeObject<string[]>(appContext.AppPrefs.GetStringPreference(ApplicationPreferences.DOWNLOAD_QUEUE_PREF_KEY));
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

        private void populateWithChildMaps(ListItem mapItem)
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

            List<ListItem> childrenItems = getChildrenOf(code);
            childrenItems.Sort();
            foreach (ListItem childItem in childrenItems)
            {
                childItem.Parent = mapItem;
                populateWithChildMaps(childItem);
            }
            mapItem.Children = childrenItems;
        }

        private List<ListItem> getChildrenOf(String parentCode)
        {
            List<ListItem> children = new List<ListItem>();
            foreach (MapDownloadResource mapResource in allMapResources.Values)
            {
                if (mapResource.ParentCode.Equals(parentCode))
                {
                    ListItem listItem = new ListItem();
                    listItem.Name = mapResource.getName();
                    listItem.DownloadResource = mapResource;
                    children.Add(listItem);
                }
            }
            return children;
        }

        private void buildCodesMap()
        {
            foreach (ListItem item in currentListItems)
            {
                codesMap.Add(item.DownloadResource.Code, item);
            }
        }

        private class DownloadsAdapter : BaseAdapter<ListItem>, ISKToolsDownloadListener
        {
            private readonly ResourceDownloadsListActivity _activity;
            public DownloadsAdapter(ResourceDownloadsListActivity activity)
            {
                _activity = activity;
            }

            public override int Count
            {
                get { return _activity.currentListItems.Count; }
            }

            public override ListItem this[int position]
            {
                get { return _activity.currentListItems[position]; }
            }

            public override long GetItemId(int position)
            {
                return 0;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                ListItem currentItem = this[position];
                View view = null;
                if (convertView == null)
                {
                    LayoutInflater inflater = _activity.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
                    view = inflater.Inflate(Resource.Layout.element_download_list_item, null);
                }
                else
                {
                    view = convertView;
                }

                ImageView arrowImage = view.FindViewById<ImageView>(Resource.Id.arrow);
                TextView downloadSizeText = view.FindViewById<TextView>(Resource.Id.package_size);
                TextView downloadNameText = view.FindViewById<TextView>(Resource.Id.package_name);
                RelativeLayout middleLayout = view.FindViewById<RelativeLayout>(Resource.Id.middle_layout);
                ImageView startPauseImage = view.FindViewById<ImageView>(Resource.Id.start_pause);
                ImageView cancelImage = view.FindViewById<ImageView>(Resource.Id.cancel);
                TextView stateText = view.FindViewById<TextView>(Resource.Id.current_state);
                ProgressBar progressBar = view.FindViewById<ProgressBar>(Resource.Id.download_progress);
                RelativeLayout progressDetailsLayout = view.FindViewById<RelativeLayout>(Resource.Id.progress_details);
                TextView percentageText = view.FindViewById<TextView>(Resource.Id.percentage);
                TextView timeLeftText = view.FindViewById<TextView>(Resource.Id.time_left);
                TextView speedText = view.FindViewById<TextView>(Resource.Id.speed);

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
                        progressBar.Progress = getPercentage(downloadResource);
                        percentageText.Text = getPercentage(downloadResource) + "%";
                        if (downloadResource.DownloadState == SKToolsDownloadItem.Paused)
                        {
                            timeLeftText.Text = "-";
                            speedText.Text = "-";
                        }
                        else if (_activity.refreshDownloadEstimates)
                        {
                            Tuple<string, string> pair = calculateDownloadEstimates(downloadResource, 20);
                            speedText.Text = pair.Item1;
                            timeLeftText.Text = pair.Item2;
                            _activity.refreshDownloadEstimates = false;
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
                        MapDownloadResource mapResource = (MapDownloadResource)downloadResource;
                        bytesToDownload = mapResource.getSkmAndZipFilesSize() + mapResource.getTXGFileSize();
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
                        case SKToolsDownloadItem.NotQueued:
                            stateText.Text = ("NOT QUEUED");
                            break;
                        case SKToolsDownloadItem.Queued:
                            stateText.Text = ("QUEUED");
                            break;
                        case SKToolsDownloadItem.Downloading:
                            stateText.Text = ("DOWNLOADING");
                            break;
                        case SKToolsDownloadItem.Downloaded:
                            stateText.Text = ("DOWNLOADED");
                            break;
                        case SKToolsDownloadItem.Paused:
                            stateText.Text = ("PAUSED");
                            break;
                        case SKToolsDownloadItem.Installing:
                            stateText.Text = ("INSTALLING");
                            break;
                        case SKToolsDownloadItem.Installed:
                            stateText.Text = ("INSTALLED");
                            break;
                        default:
                            break;
                    }

                    if (downloadResource.DownloadState == SKToolsDownloadItem.NotQueued || downloadResource.DownloadState == SKToolsDownloadItem.Downloading ||
                            downloadResource.DownloadState == SKToolsDownloadItem.Paused)
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
                        MapDownloadResource mapResource = (MapDownloadResource)downloadResource;
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

                    _activity.currentListItems = currentItem.Children;
                    _activity.buildCodesMap();
                    _activity.previousListIndexes.Push(_activity.listView.FirstVisiblePosition);
                    _activity.updateListAndScrollToPosition(0);
                };

                startPauseImage.Click += (s, e) =>
                {
                    if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Downloading)
                    {
                        if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Paused)
                        {
                            activeDownloads.Add(currentItem.DownloadResource);
                            currentItem.DownloadResource.DownloadState = SKToolsDownloadItem.Queued;
                            _activity.appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
                            String destinationPath = _activity.appContext.MapResourcesDirPath + "downloads/";
                            File destinationFile = new File(destinationPath);
                            if (!destinationFile.Exists())
                            {
                                destinationFile.Mkdirs();
                            }
                            currentItem.DownloadResource.DownloadPath = destinationPath;
                            mapsDAO.updateMapResource((MapDownloadResource)currentItem.DownloadResource);
                        }

                        NotifyDataSetChanged();

                        List<SKToolsDownloadItem> downloadItems;
                        if (!_activity.downloadManager.IsDownloadProcessRunning)
                        {
                            downloadItems = _activity.createDownloadItemsFromDownloadResources(activeDownloads);
                        }
                        else
                        {
                            List<DownloadResource> mapDownloadResources = new List<DownloadResource>();
                            mapDownloadResources.Add(currentItem.DownloadResource);
                            downloadItems = _activity.createDownloadItemsFromDownloadResources(mapDownloadResources);
                        }
                        _activity.downloadManager.StartDownload(downloadItems);
                    }
                    else
                    {
                        _activity.downloadManager.PauseDownloadThread();
                    }
                };

                cancelImage.Click += (s, e) =>
                {
                    if (currentItem.DownloadResource.DownloadState != SKToolsDownloadItem.Installed)
                    {
                        bool downloadCancelled = _activity.downloadManager.CancelDownload(currentItem.DownloadResource.Code);
                        if (!downloadCancelled)
                        {
                            currentItem.DownloadResource.DownloadState = (SKToolsDownloadItem.NotQueued);
                            currentItem.DownloadResource.NoDownloadedBytes = (0);
                            mapsDAO.updateMapResource((MapDownloadResource)currentItem.DownloadResource);
                            activeDownloads.Remove(currentItem.DownloadResource);
                            _activity.appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
                            NotifyDataSetChanged();
                        }
                    }
                    else
                    {
                        bool packageDeleted = SKPackageManager.Instance.DeleteOfflinePackage(currentItem.DownloadResource.Code);
                        if (packageDeleted)
                        {
                            Toast.MakeText(_activity.appContext, ((MapDownloadResource)currentItem.DownloadResource).getName() + " was uninstalled", ToastLength.Short).Show();
                        }
                        currentItem.DownloadResource.DownloadState = (SKToolsDownloadItem.NotQueued);
                        currentItem.DownloadResource.NoDownloadedBytes = (0);
                        mapsDAO.updateMapResource((MapDownloadResource)currentItem.DownloadResource);
                        NotifyDataSetChanged();
                    }
                };

                return view;
            }

            private Tuple<String, String> calculateDownloadEstimates(DownloadResource resource, int referencePeriodInSeconds)
            {
                long referencePeriod = 1000 * referencePeriodInSeconds;
                long currentTimestamp = DemoUtils.CurrentTimeMillis();
                long downloadPeriod = currentTimestamp - referencePeriod < _activity.downloadStartTime ? currentTimestamp - _activity.downloadStartTime : referencePeriod;
                long totalBytesDownloaded = 0;
                var iterator = _activity.downloadChunksMap.GetEnumerator();
                do
                {
                    var entry = iterator.Current;
                    long timestamp = entry.Key;
                    long bytesDownloaded = entry.Value;
                    if (currentTimestamp - timestamp > referencePeriod)
                    {
                        //iterator.remove(); remove current item
                    }
                    else
                    {
                        totalBytesDownloaded += bytesDownloaded;
                    }
                } while (iterator.MoveNext());
                float downloadPeriodSec = downloadPeriod / 1000f;
                long bytesPerSecond = (long)Math.Round(totalBytesDownloaded / downloadPeriodSec);
                String formattedTimeLeft = "";
                if (totalBytesDownloaded == 0)
                {
                    formattedTimeLeft = "-";
                }
                else if (resource is MapDownloadResource)
                {
                    MapDownloadResource mapResource = (MapDownloadResource)resource;
                    long remainingBytes = (mapResource.getSkmAndZipFilesSize() + mapResource.getTXGFileSize()) - mapResource.NoDownloadedBytes;
                    long timeLeft = (downloadPeriod * remainingBytes) / totalBytesDownloaded;
                    formattedTimeLeft = getFormattedTime(timeLeft);
                }

                return new Tuple<string, string>(convertBytesToStringRepresentation(bytesPerSecond) + "/s", formattedTimeLeft);
            }

            public override void NotifyDataSetChanged()
            {
                _activity.FindViewById(Resource.Id.cancel_all_button).Visibility = activeDownloads.Count == 0 ? ViewStates.Gone : ViewStates.Visible;
                base.NotifyDataSetChanged();
                _activity.listView.PostInvalidate();
            }

            private int getPercentage(DownloadResource downloadResource)
            {
                int percentage = 0;
                if (downloadResource is MapDownloadResource)
                {
                    MapDownloadResource mapDownloadResource = (MapDownloadResource)downloadResource;
                    percentage = (int)(((float)mapDownloadResource.NoDownloadedBytes / (mapDownloadResource.getSkmAndZipFilesSize() + mapDownloadResource.getTXGFileSize())) * 100);
                }
                return percentage;
            }

            public void OnDownloadProgress(SKToolsDownloadItem currentDownloadItem)
            {
                ListItem affectedListItem = _activity.codesMap[currentDownloadItem.ItemCode];
                DownloadResource resource;
                bool stateChanged = false;
                long bytesDownloadedSinceLastUpdate = 0;
                if (affectedListItem != null)
                {
                    stateChanged = currentDownloadItem.DownloadState != affectedListItem.DownloadResource.DownloadState;
                    bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - affectedListItem.DownloadResource.NoDownloadedBytes;
                    affectedListItem.DownloadResource.NoDownloadedBytes = (currentDownloadItem.NoDownloadedBytes);
                    affectedListItem.DownloadResource.DownloadState = (currentDownloadItem.DownloadState);
                    resource = affectedListItem.DownloadResource;
                    _activity.RunOnUiThread((() => NotifyDataSetChanged()));
                }
                else
                {
                    resource = allMapResources[currentDownloadItem.ItemCode];
                    bytesDownloadedSinceLastUpdate = currentDownloadItem.NoDownloadedBytes - resource.NoDownloadedBytes;
                    stateChanged = currentDownloadItem.DownloadState != resource.DownloadState;
                    resource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
                    resource.DownloadState = currentDownloadItem.DownloadState;
                }
                if (resource.DownloadState == SKToolsDownloadItem.Downloaded)
                {
                    activeDownloads.Remove(resource);
                    _activity.appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
                }
                else if (resource.DownloadState == SKToolsDownloadItem.Downloading)
                {
                    _activity.downloadChunksMap.Add(DemoUtils.CurrentTimeMillis(), bytesDownloadedSinceLastUpdate);
                    if (stateChanged)
                    {
                        _activity.StartPeriodicUpdates();
                    }
                }
                if (resource.DownloadState != SKToolsDownloadItem.Downloading)
                {
                    _activity.StopPeriodicUpdates();
                }
                if (stateChanged)
                {
                    mapsDAO.updateMapResource((MapDownloadResource)resource);
                }

                _activity.appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
            }

            public void OnDownloadCancelled(string currentDownloadItemCode)
            {
                _activity.StopPeriodicUpdates();
                ListItem affectedListItem = _activity.codesMap[currentDownloadItemCode];
                if (affectedListItem != null)
                {
                    affectedListItem.DownloadResource.NoDownloadedBytes = (0);
                    affectedListItem.DownloadResource.DownloadState = (SKToolsDownloadItem.NotQueued);
                    activeDownloads.Remove(affectedListItem.DownloadResource);
                    mapsDAO.updateMapResource((MapDownloadResource)affectedListItem.DownloadResource);
                    _activity.RunOnUiThread(()=>_activity.adapter.NotifyDataSetChanged());
                }
                else
                {
                    DownloadResource downloadResource = allMapResources[currentDownloadItemCode];
                    downloadResource.NoDownloadedBytes = 0;
                    downloadResource.DownloadState = (SKToolsDownloadItem.NotQueued);
                    activeDownloads.Remove(downloadResource);
                    mapsDAO.updateMapResource((MapDownloadResource)downloadResource);
                }
                _activity.appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
            }

            public void OnAllDownloadsCancelled()
            {
                _activity.StopPeriodicUpdates();
                _activity.appContext.AppPrefs.SaveDownloadStepPreference(0);
                foreach (DownloadResource downloadResource in activeDownloads)
                {
                    downloadResource.DownloadState = SKToolsDownloadItem.NotQueued;
                    downloadResource.NoDownloadedBytes = 0;
                }

                mapsDAO.clearResourcesInDownloadQueue();
                activeDownloads.Clear();
                _activity.appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
                _activity.RunOnUiThread(() => _activity.adapter.NotifyDataSetChanged());
            }

            public void OnDownloadPaused(SKToolsDownloadItem currentDownloadItem)
            {
                _activity.StopPeriodicUpdates();
                ListItem affectedListItem = _activity.codesMap[currentDownloadItem.ItemCode];
                if (affectedListItem != null)
                {
                    affectedListItem.DownloadResource.DownloadState = (currentDownloadItem.DownloadState);
                    affectedListItem.DownloadResource.NoDownloadedBytes = (currentDownloadItem.NoDownloadedBytes);
                    mapsDAO.updateMapResource((MapDownloadResource)affectedListItem.DownloadResource);
                    _activity.RunOnUiThread(() => _activity.adapter.NotifyDataSetChanged());
                }
                else
                {
                    DownloadResource downloadResource = allMapResources[currentDownloadItem.ItemCode];
                    downloadResource.DownloadState = currentDownloadItem.DownloadState;
                    downloadResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
                    mapsDAO.updateMapResource((MapDownloadResource)downloadResource);
                }

                _activity.appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
            }

            public void OnInstallFinished(SKToolsDownloadItem currentInstallingItem)
            {
                ListItem affectedListItem = _activity.codesMap[currentInstallingItem.ItemCode];
                DownloadResource resource;
                if (affectedListItem != null)
                {
                    affectedListItem.DownloadResource.DownloadState = SKToolsDownloadItem.Installed;
                    resource = affectedListItem.DownloadResource;
                    mapsDAO.updateMapResource((MapDownloadResource)affectedListItem.DownloadResource);
                    _activity.RunOnUiThread(() => _activity.adapter.NotifyDataSetChanged());
                }
                else
                {
                    resource = allMapResources[currentInstallingItem.ItemCode];
                    resource.DownloadState = SKToolsDownloadItem.Installed;
                    mapsDAO.updateMapResource((MapDownloadResource)resource);
                }

                _activity.RunOnUiThread(() => Toast.MakeText(_activity.appContext, ((MapDownloadResource)resource).getName() + " was installed", ToastLength.Short).Show());
            }

            public void OnInstallStarted(SKToolsDownloadItem currentInstallingItem)
            {
                ListItem affectedListItem = _activity.codesMap[currentInstallingItem.ItemCode];
                if (affectedListItem != null)
                {
                    affectedListItem.DownloadResource.DownloadState = SKToolsDownloadItem.Installing;
                    mapsDAO.updateMapResource((MapDownloadResource)affectedListItem.DownloadResource);
                    _activity.RunOnUiThread(() => _activity.adapter.NotifyDataSetChanged());
                }
                else
                {
                    DownloadResource downloadResource = allMapResources[currentInstallingItem.ItemCode];
                    downloadResource.DownloadState = (SKToolsDownloadItem.Installing);
                    mapsDAO.updateMapResource((MapDownloadResource)downloadResource);
                }
            }

            public void OnInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
            {
                _activity.StopPeriodicUpdates();
                _activity.appContext.AppPrefs.SaveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
            }

            public void OnNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
            {
                _activity.RunOnUiThread(() => Toast.MakeText(_activity.ApplicationContext, "Not enough memory on the storage", ToastLength.Short).Show());
            }
        }

        public override void OnBackPressed()
        {
            if (currentListItems == null || currentListItems.Count == 0)
            {
                base.OnBackPressed();
                return;
            }

            ListItem firstItem = currentListItems[0];

            if (firstItem.Parent.Parent == null)
            {
                base.OnBackPressed();
            }
            else
            {
                currentListItems = currentListItems[0].Parent.Parent.Children;
                buildCodesMap();
                updateListAndScrollToPosition(previousListIndexes.Pop());
            }
        }

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

        public static String convertBytesToStringRepresentation(long value)
        {
            long[] dividers = new long[] { TERRA, GIGA, MEGA, KILO, 1 };
            string[] units = new string[] { "TB", "GB", "MB", "KB", "B" };

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

            return result ?? "O B";
        }

        public static string getFormattedTime(long time)
        {
            string format = String.Format("%%0%dd", 2);
            time = time / 1000;
            string seconds = String.Format(format, time % 60);
            string minutes = String.Format(format, (time % 3600) / 60);
            string hours = String.Format(format, time / 3600);
            string formattedTime = hours + ":" + minutes + ":" + seconds;
            return formattedTime;
        }

        private static String formatDecimals(long value, long divider, String unit)
        {
            double result = divider > 1 ? (double)value / (double)divider : (double)value;
            return new DecimalFormat("#,##0.#").Format(result) + " " + unit;
        }

        private List<SKToolsDownloadItem> createDownloadItemsFromDownloadResources(List<DownloadResource> downloadResources)
        {
            List<SKToolsDownloadItem> downloadItems = new List<SKToolsDownloadItem>();
            foreach (DownloadResource currentDownloadResource in downloadResources)
            {
                SKToolsDownloadItem currentItem = currentDownloadResource.ToDownloadItem();
                if (currentDownloadResource.DownloadState == SKToolsDownloadItem.Queued)
                {
                    currentItem.CurrentStepIndex = ((sbyte)0);
                }
                else if ((currentDownloadResource.DownloadState == SKToolsDownloadItem.Paused) || (currentDownloadResource.DownloadState == SKToolsDownloadItem.Downloading))
                {
                    int downloadStepIndex = appContext.AppPrefs.GetIntPreference(ApplicationPreferences.DOWNLOAD_STEP_INDEX_PREF_KEY);
                    currentItem.CurrentStepIndex = ((sbyte)downloadStepIndex);
                }
                downloadItems.Add(currentItem);
            }
            return downloadItems;
        }

        [Export("onClick")]
        public void onClick(View view)
        {
            if (view.Id == Resource.Id.cancel_all_button)
            {
                bool cancelled = downloadManager.CancelAllDownloads();
                if (!cancelled)
                {
                    foreach (DownloadResource resource in activeDownloads)
                    {
                        resource.NoDownloadedBytes = (0);
                        resource.DownloadState = (SKToolsDownloadItem.NotQueued);
                    }
                    activeDownloads.Clear();
                    appContext.AppPrefs.SaveDownloadQueuePreference(activeDownloads);
                    mapsDAO.clearResourcesInDownloadQueue();
                    adapter.NotifyDataSetChanged();
                }
            }
        }
    }
}