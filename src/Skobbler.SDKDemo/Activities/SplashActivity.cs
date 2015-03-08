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

namespace Skobbler.SDKDemo.Activities
{
	/// <summary>
	/// Activity that installs required resources (from assets/MapResources.zip) to
	/// the device
	/// </summary>
    [Activity(MainLauncher = true)]
	public class SplashActivity : Activity, ISKPrepareMapTextureListener, ISKMapUpdateListener
	{

		/// <summary>
		/// Path to the MapResources directory
		/// </summary>
		public static string MapResourcesDirPath = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_splash);

            SKLogging.EnableLogs(true);

            string applicationPath = ChooseStoragePath(this);

            // determine path where map resources should be copied on the device
            if (applicationPath != null)
            {
                MapResourcesDirPath = applicationPath + "/" + "SKMaps/";
            }
            else
            {
                // show a dialog and then finish
            }

            ((DemoApplication)Application).MapResourcesDirPath = MapResourcesDirPath;


            if (!Directory.Exists(MapResourcesDirPath) || File.Exists(MapResourcesDirPath))
            {
                // if map resources are not already present copy them to
                // mapResourcesDirPath in the following thread
                (new SKPrepareMapTextureThread(this, MapResourcesDirPath, "SKMaps.zip", this)).Start();
                // copy some other resource needed
                CopyOtherResources();
                PrepareMapCreatorFile();
            }
            else
            {
                // map resources have already been copied - start the map activity
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
			DemoUtils.InitializeLibrary(this);
			SKVersioningManager.Instance.SetMapUpdateListener(this);
			Toast.MakeText(this, "Map resources were copied", ToastLength.Short).Show();
			Finish();
			StartActivity(new Intent(this, typeof(MapActivity)));
		}

		/// <summary>
		/// Copy some additional resources from assets
		/// </summary>
		private void CopyOtherResources()
		{
            new Thread(() =>
            {
                try
                {
                    string tracksPath = MapResourcesDirPath + "GPXTracks";
                    Java.IO.File tracksDir = new Java.IO.File(tracksPath);
                    if (!tracksDir.Exists())
                    {
                        tracksDir.Mkdirs();
                    }
                    DemoUtils.CopyAssetsToFolder(Assets, "GPXTracks", MapResourcesDirPath + "GPXTracks");

                    string imagesPath = MapResourcesDirPath + "images";
                    Java.IO.File imagesDir = new Java.IO.File(imagesPath);
                    if (!imagesDir.Exists())
                    {
                        imagesDir.Mkdirs();
                    }
                    DemoUtils.CopyAssetsToFolder(Assets, "images", MapResourcesDirPath + "images");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }).Start();
		}

		/// <summary>
		/// Copies the map creator file and logFile from assets to a storage.
		/// </summary>
		private void PrepareMapCreatorFile()
		{
			DemoApplication app = (DemoApplication) Application;

            Thread prepareGpxFileThread = new Thread(() =>
            {
                try
                {
                    //android.os.Process.ThreadPriority = android.os.Process.THREAD_PRIORITY_BACKGROUND;
                    string mapCreatorFolderPath = MapResourcesDirPath + "MapCreator";
                    Java.IO.File mapCreatorFolder = new Java.IO.File(mapCreatorFolderPath);
                    // create the folder where you want to copy the json file
                    if (!mapCreatorFolder.Exists())
                    {
                        mapCreatorFolder.Mkdirs();
                    }
                    app.MapCreatorFilePath = mapCreatorFolderPath + "/mapcreatorFile.json";
                    DemoUtils.copyAsset(Assets, "MapCreator", mapCreatorFolderPath, "mapcreatorFile.json");
                    // Copies the log file from assets to a storage.
                    string logFolderPath = MapResourcesDirPath + "logFile";
                    Java.IO.File logFolder = new Java.IO.File(logFolderPath);
                    if (!logFolder.Exists())
                    {
                        logFolder.Mkdirs();
                    }
                    DemoUtils.copyAsset(Assets, "logFile", logFolderPath, "Seattle.log");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            });

			prepareGpxFileThread.Start();
		}

		public void OnMapVersionSet(int newVersion)
		{
			// TODO Auto-generated method stub

		}

		public void OnNewVersionDetected(int newVersion)
		{
			// TODO Auto-generated method stub
			Log.Error("", "new version " + newVersion);
		}

		public void OnNoNewVersionDetected()
		{
			// TODO Auto-generated method stub

		}

		public void OnVersionFileDownloadTimeout()
		{
			// TODO Auto-generated method stub

		}

		public const long Kilo = 1024;

		public static readonly long Mega = Kilo * Kilo;

		public static string ChooseStoragePath(Context context)
		{
			if (GetAvailableMemorySize(Environment.DataDirectory.Path) >= 50 * Mega)
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
					if (GetAvailableMemorySize(context.GetExternalFilesDir(null).ToString()) >= 50 * Mega)
					{
						return context.GetExternalFilesDir(null).ToString();
					}
				}
			}

			SKLogging.WriteLog(Tag, "There is not enough memory on any storage, but return internal memory", SKLogging.LogDebug);

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

		private const string Tag = "SplashActivity";

		/// <summary>
		/// get the available internal memory size
		/// </summary>
		/// <returns> available memory size in bytes </returns>
		public static long GetAvailableMemorySize(string path)
		{
			StatFs statFs = null;
			try
			{
				statFs = new StatFs(path);
			}
			catch (ArgumentException ex)
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
					SKLogging.WriteLog(Tag, "Exception at getAvailableMemorySize method = " + e.Message, SKLogging.LogDebug);
				}

				if (getAvailableBytesMethod != null)
				{
					try
					{
						SKLogging.WriteLog(Tag, "Using new API for getAvailableMemorySize method !!!", SKLogging.LogDebug);
						return (long) getAvailableBytesMethod.Invoke(statFs);
					}
					catch (IllegalAccessException)
					{
						return (long) statFs.AvailableBlocks * (long) statFs.BlockSize;
					}
					catch (InvocationTargetException)
					{
						return (long) statFs.AvailableBlocks * (long) statFs.BlockSize;
					}
				}
				else
				{
					return (long) statFs.AvailableBlocks * (long) statFs.BlockSize;
				}
			}
			else
			{
				return 0;
			}
		}
	}

}