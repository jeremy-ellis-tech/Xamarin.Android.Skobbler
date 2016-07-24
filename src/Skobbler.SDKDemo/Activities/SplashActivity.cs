using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Java.IO;
using Skobbler.DebugKit.Activity;
using Skobbler.DebugKit.Util;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Util;
using System.Threading.Tasks;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(MainLauncher = true, ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class SplashActivity : Activity, ISKMapsInitializationListener, ISKMapVersioningListener
    {

        private static string TAG = "SplashActivity";
        public static int _newMapVersionDetected = 0;

        private bool _update = false;
        private long _startLibInitTime;
        /**
         * flag that shows whether the debug kit is enabled or not
         */
        private bool _debugKitEnabled;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);

            SKLogging.EnableLogs(true);
            bool multipleMapSupport = false;

            try
            {
                ApplicationInfo applicationInfo = PackageManager.GetApplicationInfo(PackageName, PackageInfoFlags.MetaData);
                Bundle bundle = applicationInfo.MetaData;
                multipleMapSupport = bundle.GetBoolean("provideMultipleMapSupport");
                _debugKitEnabled = bundle.GetBoolean(DebugKitConfig.EnableDebugKitKey);
            }
            catch (PackageManager.NameNotFoundException e)
            {
                _debugKitEnabled = false;
                e.PrintStackTrace();
            }
            if (multipleMapSupport)
            {
                SKMapSurfaceView.PreserveGLContext = false;
                DemoUtils.IsMultipleMapSupportEnabled = true;
            }

            try
            {
                SKLogging.WriteLog(TAG, "Initialize SKMaps", SKLogging.LogDebug);
                _startLibInitTime = DemoUtils.CurrentTimeMillis();
                CheckForSDKUpdate();
                //            SKMapsInitSettings mapsInitSettings = new SKMapsInitSettings();
                //            mapsInitSettings.setMapResourcesPath(getExternalFilesDir(null).toString()+"/SKMaps/");
                //  mapsInitSettings.setConnectivityMode(SKMaps.CONNECTIVITY_MODE_OFFLINE);
                //  mapsInitSettings.setPreinstalledMapsPath(getExternalFilesDir(null).toString()+"/SKMaps/PreinstalledMaps/");
                SKMaps.Instance.InitializeSKMaps(Application, this);
            }
            catch (SKDeveloperKeyException exception)
            {
                exception.PrintStackTrace();
                DemoUtils.ShowApiKeyErrorDialog(this);
            }

        }

        public void OnLibraryInitialized(bool isSuccessful)
        {
            SKLogging.WriteLog(TAG, " SKMaps library initialized isSuccessful= " + isSuccessful + " time= " + (DemoUtils.CurrentTimeMillis() - _startLibInitTime), SKLogging.LogDebug);
            if (isSuccessful)
            {
                DemoApplication app = Application as DemoApplication;
                app.MapCreatorFilePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "MapCreator/mapcreatorFile.json";
                app.MapResourcesDirPath = SKMaps.Instance.MapInitSettings.MapResourcesPath;
                CopyOtherResources();
                PrepareMapCreatorFile();
                //everything ok. proceed
                SKVersioningManager.Instance.SetMapUpdateListener(this);
                GoToMap();
            }
            else {
                //map was not initialized successfully
                Finish();
            }
        }

        private void GoToMap()
        {
            Finish();
            if (!_debugKitEnabled)
            {
                StartActivity(new Intent(this, typeof(MapActivity)));
            }
            else
            {
                Intent intent = new Intent(this, typeof(DebugMapActivity));
                intent.PutExtra("mapResourcesPath", SKMaps.Instance.MapInitSettings.MapResourcesPath);
                StartActivity(intent);
            }
        }

        /**
         * Copy some additional resources from assets
         */
        private async void CopyOtherResources()
        {
            string mapResourcesDirPath = SKMaps.Instance.MapInitSettings.MapResourcesPath;
            await Task.Run(() =>
            {
                try
                {
                    bool resAlreadyExist;

                    string tracksPath = mapResourcesDirPath + "GPXTracks";
                    File tracksDir = new File(tracksPath);
                    resAlreadyExist = tracksDir.Exists();
                    if (!resAlreadyExist || _update)
                    {
                        if (!resAlreadyExist)
                        {
                            tracksDir.Mkdirs();
                        }
                        DemoUtils.CopyAssetsToFolder(Assets, "GPXTracks", mapResourcesDirPath + "GPXTracks");
                    }

                    string imagesPath = mapResourcesDirPath + "images";
                    File imagesDir = new File(imagesPath);
                    resAlreadyExist = imagesDir.Exists();
                    if (!resAlreadyExist || _update)
                    {
                        if (!resAlreadyExist)
                        {
                            imagesDir.Mkdirs();
                        }
                        DemoUtils.CopyAssetsToFolder(Assets, "images", mapResourcesDirPath + "images");
                    }
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            });
        }

        /**
         * Copies the map creator file and logFile from assets to a storage.
         */
        private async void PrepareMapCreatorFile()
        {
            string mapResourcesDirPath = SKMaps.Instance.MapInitSettings.MapResourcesPath;
            DemoApplication app = Application as DemoApplication;
            await Task.Run(() =>
            {
                try
                {
                    bool resAlreadyExist;

                    string mapCreatorFolderPath = mapResourcesDirPath + "MapCreator";
                    // create the folder where you want to copy the json file
                    File mapCreatorFolder = new File(mapCreatorFolderPath);

                    resAlreadyExist = mapCreatorFolder.Exists();
                    if (!resAlreadyExist || _update)
                    {
                        if (!resAlreadyExist)
                        {
                            mapCreatorFolder.Mkdirs();
                        }
                        app.MapCreatorFilePath = mapCreatorFolderPath + "/mapcreatorFile.json";
                        DemoUtils.CopyAsset(Assets, "MapCreator", mapCreatorFolderPath, "mapcreatorFile.json");
                    }

                    // Copies the log file from assets to a storage.
                    string logFolderPath = mapResourcesDirPath + "logFile";
                    File logFolder = new File(logFolderPath);
                    resAlreadyExist = logFolder.Exists();
                    if (!resAlreadyExist || _update)
                    {
                        if (!resAlreadyExist)
                        {
                            logFolder.Mkdirs();
                        }
                        DemoUtils.CopyAsset(Assets, "logFile", logFolderPath, "Seattle.log");
                    }
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            });
        }


        /**
         * Checks if the current version code is grater than the previous and performs an SDK update.
         */
        public void CheckForSDKUpdate()
        {
            DemoApplication appContext = Application as DemoApplication;
            int currentVersionCode = appContext.AppPrefs.GetIntPreference(ApplicationPreferences.CURRENT_VERSION_CODE);
            int versionCode = VersionCode;
            if (currentVersionCode == 0)
            {
                appContext.AppPrefs.SetCurrentVersionCode(versionCode);
            }

            if (0 < currentVersionCode && currentVersionCode < versionCode)
            {
                SKMaps.UpdateToLatestSDKVersion = true;
                appContext.AppPrefs.SetCurrentVersionCode(versionCode);
            }
        }

        /**
         * Returns the current version code
         *
         * @return
         */
        public int VersionCode
        {
            get
            {
                int v = 0;
                try
                {
                    v = PackageManager.GetPackageInfo(PackageName, (PackageInfoFlags)0).VersionCode;
                }
                catch (PackageManager.NameNotFoundException e)
                {
                }
                return v;
            }
        }

        public void OnNewVersionDetected(int i)
        {
            Log.Error("", " New version = " + i);
            _newMapVersionDetected = i;

        }

        public void OnMapVersionSet(int i)
        {

        }

        public void OnVersionFileDownloadTimeout()
        {

        }

        public void OnNoNewVersionDetected()
        {

        }
    }
}