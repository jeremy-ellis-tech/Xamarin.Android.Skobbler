using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using Skobbler.Ngx;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Util;
using Environment = Android.OS.Environment;
using IOException = Java.IO.IOException;
using System.Threading.Tasks;
using Android.Content.PM;
using Skobbler.Ngx.Map;
using File = Java.IO.File;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(MainLauncher = true, ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize), Theme = "@style/SplashActivityTheme")]
    public class SplashActivity : Activity, ISKPrepareMapTextureListener, ISKMapUpdateListener
    {

        public static string mapResourcesDirPath = "";

        private bool update = false;

        private static string TAG = "SplashActivity";
        public static int NewMapVersionDetected = 0;

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
            }
            catch (PackageManager.NameNotFoundException e)
            {
                e.PrintStackTrace();
            }
            if (multipleMapSupport)
            {
                SKMapSurfaceView.PreserveGLContext = false;
                DemoUtils.IsMultipleMapSupportEnabled = true;
            }

            string applicationPath = ChooseStoragePath(this);

            // determine path where map resources should be copied on the device
            if (applicationPath != null)
            {
                mapResourcesDirPath = applicationPath + "/" + "SKMaps/";
            }
            else
            {
                // show a dialog and then finish
            }
            ((DemoApplication)ApplicationContext).AppPrefs.SaveStringPreference("mapResourcesPath", mapResourcesDirPath);
            ((DemoApplication)Application).MapResourcesDirPath = mapResourcesDirPath;
            CheckForUpdate();
            if (!new File(mapResourcesDirPath).Exists())
            {
                // copy some other resource needed
                new SKPrepareMapTextureThread(this, mapResourcesDirPath, "SKMaps.zip", this).Start();
                CopyOtherResources();
                PrepareMapCreatorFile();
            }
            else if (!update)
            {
                DemoApplication app = (DemoApplication)Application;
                app.MapCreatorFilePath = mapResourcesDirPath + "MapCreator/mapcreatorFile.json";
                Toast.MakeText(this, "Map resources copied in a previous run", ToastLength.Short).Show();
                PrepareMapCreatorFile();
                DemoUtils.InitializeLibrary(this);
                SKVersioningManager.Instance.SetMapUpdateListener(this);
                Finish();
                StartActivity(new Intent(this, typeof(MapActivity)));

            }

        }

        public void OnMapTexturesPrepared(bool prepared)
        {
            SKVersioningManager.Instance.SetMapUpdateListener(this);
            Toast.MakeText(this, "Map resources were copied", ToastLength.Short).Show();

            if (DemoUtils.InitializeLibrary(this))
            {
                Finish();
                StartActivity(new Intent(this, typeof(MapActivity)));
            }
        }

        private void CopyOtherResources()
        {
            RunOnUiThread(() =>
            {
                try
                {
                    string tracksPath = mapResourcesDirPath + "GPXTracks";
                    File tracksDir = new File(tracksPath);
                    if (!tracksDir.Exists())
                    {
                        tracksDir.Mkdirs();
                    }
                    DemoUtils.CopyAssetsToFolder(Assets, "GPXTracks", mapResourcesDirPath + "GPXTracks");

                    string imagesPath = mapResourcesDirPath + "images";
                    File imagesDir = new File(imagesPath);
                    if (!imagesDir.Exists())
                    {
                        imagesDir.Mkdirs();
                    }
                    DemoUtils.CopyAssetsToFolder(Assets, "images", mapResourcesDirPath + "images");
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            });
        }

        private void PrepareMapCreatorFile()
        {
            DemoApplication app = (DemoApplication)Application;
            RunOnUiThread(() =>
            {
                try
                {
                    Android.OS.Process.SetThreadPriority(ThreadPriority.Background);

                    string mapCreatorFolderPath = mapResourcesDirPath + "MapCreator";
                    File mapCreatorFolder = new File(mapCreatorFolderPath);
                    // create the folder where you want to copy the json file
                    if (!mapCreatorFolder.Exists())
                    {
                        mapCreatorFolder.Mkdirs();
                    }
                    app.MapCreatorFilePath = mapCreatorFolderPath + "/mapcreatorFile.json";
                    DemoUtils.CopyAsset(Assets, "MapCreator", mapCreatorFolderPath, "mapcreatorFile.json");
                    // Copies the log file from assets to a storage.
                    string logFolderPath = mapResourcesDirPath + "logFile";
                    File logFolder = new File(logFolderPath);
                    if (!logFolder.Exists())
                    {
                        logFolder.Mkdirs();
                    }
                    DemoUtils.CopyAsset(Assets, "logFile", logFolderPath, "Seattle.log");
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            });
        }

        public void OnMapVersionSet(int newVersion)
        {
        }

        public void OnNewVersionDetected(int newVersion)
        {
            Log.Error("", "new version " + newVersion);
            NewMapVersionDetected = newVersion;
        }

        public void OnNoNewVersionDetected()
        {
        }

        public void OnVersionFileDownloadTimeout()
        {
        }

        public static long KILO = 1024;

        public static long MEGA = KILO * KILO;

        public static string ChooseStoragePath(Context context)
        {
            if (GetAvailableMemorySize(Environment.DataDirectory.Path) >= 50 * MEGA)
            {
                if (context != null && context.FilesDir != null)
                {
                    return context.FilesDir.Path;
                }
            }
            else
            {
                if ((context != null) && (context.GetExternalFilesDir(null) != null))
                {
                    if (GetAvailableMemorySize(context.GetExternalFilesDir(null).ToString()) >= 50 * MEGA)
                    {
                        return context.GetExternalFilesDir(null).ToString();
                    }
                }
            }

            SKLogging.WriteLog(TAG, "There is not enough memory on any storage, but return internal memory", SKLogging.LogDebug);

            if (context != null && context.FilesDir != null)
            {
                return context.FilesDir.Path;
            }
            else
            {
                if ((context != null) && (context.GetExternalFilesDir(null) != null))
                {
                    return context.GetExternalFilesDir(null).ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public void CheckForUpdate()
        {
            DemoApplication appContext = (DemoApplication)Application;
            int currentVersionCode = appContext.AppPrefs.GetIntPreference(ApplicationPreferences.CURRENT_VERSION_CODE);
            int versionCode = GetVersionCode();
            if (currentVersionCode == 0)
            {
                appContext.AppPrefs.SetCurrentVersionCode(versionCode);
            }

            if (0 < currentVersionCode && currentVersionCode < versionCode)
            {
                update = true;
                appContext.AppPrefs.SetCurrentVersionCode(versionCode);
                DemoUtils.DeleteFileOrDirectory(new File(mapResourcesDirPath));
                new SKPrepareMapTextureThread(this, mapResourcesDirPath, "SKMaps.zip", this).Start();
                CopyOtherResources();
                PrepareMapCreatorFile();
            }

        }

        public static long GetAvailableMemorySize(string path)
        {
            StatFs statFs = null;
            try
            {
                statFs = new StatFs(path);
            }
            catch (IllegalArgumentException ex)
            {
                SKLogging.WriteLog("SplashActivity", "Exception when creating StatF ; message = " + ex, SKLogging.LogDebug);
            }
            if (statFs != null)
            {
                Method getAvailableBytesMethod = null;
                try
                {
                    getAvailableBytesMethod = statFs.Class.GetMethod("getAvailableBytes");
                }
                catch (NoSuchMethodException e)
                {
                    SKLogging.WriteLog(TAG, "Exception at getAvailableMemorySize method = " + e.Message, SKLogging.LogDebug);
                }

                if (getAvailableBytesMethod != null)
                {
                    try
                    {
                        SKLogging.WriteLog(TAG, "Using new API for getAvailableMemorySize method !!!", SKLogging.LogDebug);
                        return (long)getAvailableBytesMethod.Invoke(statFs);
                    }
                    catch (IllegalAccessException)
                    {
                        return (long)statFs.AvailableBlocks * (long)statFs.BlockSize;
                    }
                    catch (InvocationTargetException)
                    {
                        return (long)statFs.AvailableBlocks * (long)statFs.BlockSize;
                    }
                }
                else
                {
                    return (long)statFs.AvailableBlocks * (long)statFs.BlockSize;
                }
            }
            else
            {
                return 0;
            }
        }

        public int GetVersionCode()
        {
            int v = 0;

            try
            {
                v = PackageManager.GetPackageInfo(PackageName, 0).VersionCode;
            }
            catch (PackageManager.NameNotFoundException)
            {
            }

            return v;
        }
    }
}