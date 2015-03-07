using System.Threading;

namespace Skobbler.SDKDemo.Activity
{
	/// <summary>
	/// Activity that installs required resources (from assets/MapResources.zip) to
	/// the device
	/// </summary>
	public class SplashActivity : Activity, SKPrepareMapTextureListener, SKMapUpdateListener
	{

		/// <summary>
		/// Path to the MapResources directory
		/// </summary>
		public static string mapResourcesDirPath = "";

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_splash;

			SKLogging.enableLogs(true);

			string applicationPath = chooseStoragePath(this);

			// determine path where map resources should be copied on the device
			if (applicationPath != null)
			{
				mapResourcesDirPath = applicationPath + "/" + "SKMaps/";
			}
			else
			{
				// show a dialog and then finish
			}

			((DemoApplication) Application).MapResourcesDirPath = mapResourcesDirPath;


			if (!System.IO.Directory.Exists(mapResourcesDirPath) || System.IO.File.Exists(mapResourcesDirPath))
			{
				// if map resources are not already present copy them to
				// mapResourcesDirPath in the following thread
				(new SKPrepareMapTextureThread(this, mapResourcesDirPath, "SKMaps.zip", this)).start();
				// copy some other resource needed
				copyOtherResources();
				prepareMapCreatorFile();
			}
			else
			{
				// map resources have already been copied - start the map activity
				Toast.makeText(SplashActivity.this, "Map resources copied in a previous run", Toast.LENGTH_SHORT).show();
				prepareMapCreatorFile();
				DemoUtils.initializeLibrary(this);
				SKVersioningManager.Instance.MapUpdateListener = this;
				finish();
				startActivity(new Intent(this, typeof(MapActivity)));
			}
		}

		public override void onMapTexturesPrepared(bool prepared)
		{
			DemoUtils.initializeLibrary(this);
			SKVersioningManager.Instance.MapUpdateListener = this;
			Toast.makeText(SplashActivity.this, "Map resources were copied", Toast.LENGTH_SHORT).show();
			finish();
			startActivity(new Intent(SplashActivity.this, typeof(MapActivity)));
		}

		/// <summary>
		/// Copy some additional resources from assets
		/// </summary>
		private void copyOtherResources()
		{
			new ThreadAnonymousInnerClassHelper(this)
			.start();
		}

		private class ThreadAnonymousInnerClassHelper : System.Threading.Thread
		{
			private readonly SplashActivity outerInstance;

			public ThreadAnonymousInnerClassHelper(SplashActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void run()
			{
				try
				{
					string tracksPath = mapResourcesDirPath + "GPXTracks";
					File tracksDir = new File(tracksPath);
					if (!tracksDir.exists())
					{
						tracksDir.mkdirs();
					}
					DemoUtils.copyAssetsToFolder(Assets, "GPXTracks", mapResourcesDirPath + "GPXTracks");

					string imagesPath = mapResourcesDirPath + "images";
					File imagesDir = new File(imagesPath);
					if (!imagesDir.exists())
					{
						imagesDir.mkdirs();
					}
					DemoUtils.copyAssetsToFolder(Assets, "images", mapResourcesDirPath + "images");
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		/// <summary>
		/// Copies the map creator file and logFile from assets to a storage.
		/// </summary>
		private void prepareMapCreatorFile()
		{
			DemoApplication app = (DemoApplication) Application;
			Thread prepareGPXFileThread = new Thread(new RunnableAnonymousInnerClassHelper(this, app));
			prepareGPXFileThread.Start();
		}

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			private readonly SplashActivity outerInstance;

			private DemoApplication app;

			public RunnableAnonymousInnerClassHelper(SplashActivity outerInstance, DemoApplication app)
			{
				this.outerInstance = outerInstance;
				this.app = app;
			}


			public override void run()
			{
				try
				{
					android.os.Process.ThreadPriority = android.os.Process.THREAD_PRIORITY_BACKGROUND;
					string mapCreatorFolderPath = mapResourcesDirPath + "MapCreator";
					File mapCreatorFolder = new File(mapCreatorFolderPath);
					// create the folder where you want to copy the json file
					if (!mapCreatorFolder.exists())
					{
						mapCreatorFolder.mkdirs();
					}
					app.MapCreatorFilePath = mapCreatorFolderPath + "/mapcreatorFile.json";
					DemoUtils.copyAsset(Assets, "MapCreator", mapCreatorFolderPath, "mapcreatorFile.json");
					// Copies the log file from assets to a storage.
					string logFolderPath = mapResourcesDirPath + "logFile";
					File logFolder = new File(logFolderPath);
					if (!logFolder.exists())
					{
						logFolder.mkdirs();
					}
					DemoUtils.copyAsset(Assets, "logFile", logFolderPath, "Seattle.log");
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

			}
		}


		public override void onMapVersionSet(int newVersion)
		{
			// TODO Auto-generated method stub

		}

		public override void onNewVersionDetected(int newVersion)
		{
			// TODO Auto-generated method stub
			Log.e("", "new version " + newVersion);
		}

		public override void onNoNewVersionDetected()
		{
			// TODO Auto-generated method stub

		}

		public override void onVersionFileDownloadTimeout()
		{
			// TODO Auto-generated method stub

		}

		public const long KILO = 1024;

		public static readonly long MEGA = KILO * KILO;

		public static string chooseStoragePath(Context context)
		{
			if (getAvailableMemorySize(Environment.DataDirectory.Path) >= 50 * MEGA)
			{
				if (context != null && context.FilesDir != null)
				{
					return context.FilesDir.Path;
				}
			}
			else
			{
				if ((context != null) && (context.getExternalFilesDir(null) != null))
				{
					if (getAvailableMemorySize(context.getExternalFilesDir(null).ToString()) >= 50 * MEGA)
					{
						return context.getExternalFilesDir(null).ToString();
					}
				}
			}

			SKLogging.writeLog(TAG, "There is not enough memory on any storage, but return internal memory", SKLogging.LOG_DEBUG);

			if (context != null && context.FilesDir != null)
			{
				return context.FilesDir.Path;
			}
			else
			{
				if ((context != null) && (context.getExternalFilesDir(null) != null))
				{
					return context.getExternalFilesDir(null).ToString();
				}
				else
				{
					return null;
				}
			}
		}

		private const string TAG = "SplashActivity";

		/// <summary>
		/// get the available internal memory size
		/// </summary>
		/// <returns> available memory size in bytes </returns>
		public static long getAvailableMemorySize(string path)
		{
			StatFs statFs = null;
			try
			{
				statFs = new StatFs(path);
			}
			catch (System.ArgumentException ex)
			{
				SKLogging.writeLog("SplashActivity", "Exception when creating StatF ; message = " + ex, SKLogging.LOG_DEBUG);
			}
			if (statFs != null)
			{
				Method getAvailableBytesMethod = null;
				try
				{
					getAvailableBytesMethod = statFs.GetType().GetMethod("getAvailableBytes");
				}
				catch (NoSuchMethodException e)
				{
					SKLogging.writeLog(TAG, "Exception at getAvailableMemorySize method = " + e.Message, SKLogging.LOG_DEBUG);
				}

				if (getAvailableBytesMethod != null)
				{
					try
					{
						SKLogging.writeLog(TAG, "Using new API for getAvailableMemorySize method !!!", SKLogging.LOG_DEBUG);
						return (long?) getAvailableBytesMethod.invoke(statFs);
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