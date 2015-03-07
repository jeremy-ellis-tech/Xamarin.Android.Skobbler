using System.Text;

namespace Skobbler.SDKDemo.Util
{




	using Context = android.content.Context;
	using AssetManager = android.content.res.AssetManager;
	using LocationManager = android.location.LocationManager;
	using ConnectivityManager = android.net.ConnectivityManager;
	using NetworkInfo = android.net.NetworkInfo;
	using ByteStreams = com.google.common.io.ByteStreams;
	using SKMaps = com.skobbler.ngx.SKMaps;
	using SKMapsInitSettings = com.skobbler.ngx.SKMapsInitSettings;
	using SKMapViewStyle = com.skobbler.ngx.map.SKMapViewStyle;
	using SKAdvisorSettings = com.skobbler.ngx.navigation.SKAdvisorSettings;
	using DemoApplication = com.skobbler.sdkdemo.application.DemoApplication;


	public class DemoUtils
	{

		private const string API_KEY = "";

		/// <summary>
		/// Gets formatted time from a given number of seconds </summary>
		/// <param name="timeInSec">
		/// @return </param>
		public static string formatTime(int timeInSec)
		{
			StringBuilder builder = new StringBuilder();
			int hours = timeInSec / 3600;
			int minutes = (timeInSec - hours * 3600) / 60;
			int seconds = timeInSec - hours * 3600 - minutes * 60;
			builder.Insert(0, seconds + "s");
			if (minutes > 0 || hours > 0)
			{
				builder.Insert(0, minutes + "m ");
			}
			if (hours > 0)
			{
				builder.Insert(0, hours + "h ");
			}
			return builder.ToString();
		}

		/// <summary>
		/// Formats a given distance value (given in meters) </summary>
		/// <param name="distInMeters">
		/// @return </param>
		public static string formatDistance(int distInMeters)
		{
			if (distInMeters < 1000)
			{
				return distInMeters + "m";
			}
			else
			{
				return ((float) distInMeters / 1000) + "km";
			}
		}

		/// <summary>
		/// Copies files from assets to destination folder </summary>
		/// <param name="assetManager"> </param>
		/// <param name="sourceFolder"> </param>
		/// <param name="destination"> </param>
		/// <exception cref="IOException"> </exception>
		public static void copyAssetsToFolder(AssetManager assetManager, string sourceFolder, string destinationFolder)
		{
			string[] assets = assetManager.list(sourceFolder);
			File destFolderFile = new File(destinationFolder);
			if (!destFolderFile.exists())
			{
				destFolderFile.mkdirs();
			}
			copyAsset(assetManager, sourceFolder, destinationFolder, assets);
		}

		/// <summary>
		/// Copies files from assets to destination folder </summary>
		/// <param name="assetManager"> </param>
		/// <param name="sourceFolder"> </param>
		/// <param name="assetsNames"> </param>
		/// <exception cref="IOException"> </exception>
		public static void copyAsset(AssetManager assetManager, string sourceFolder, string destinationFolder, params string[] assetsNames)
		{

			foreach (string assetName in assetsNames)
			{
				System.IO.Stream destinationStream = new System.IO.FileStream(destinationFolder + "/" + assetName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				string[] files = assetManager.list(sourceFolder + "/" + assetName);
				if (files == null || files.Length == 0)
				{

					System.IO.Stream asset = assetManager.open(sourceFolder + "/" + assetName);
					try
					{
						ByteStreams.copy(asset, destinationStream);
					}
					finally
					{
						asset.Close();
						destinationStream.Close();
					}
				}
			}
		}

		/// <summary>
		/// Copies files from assets to destination folder. </summary>
		/// <param name="assetManager"> </param>
		/// <param name="assetName"> the asset that needs to be copied </param>
		/// <param name="destinationFolder"> path to folder where you want to store the asset
		/// archive </param>
		/// <exception cref="IOException"> </exception>
		public static void copyAsset(AssetManager assetManager, string assetName, string destinationFolder)
		{

			System.IO.Stream destinationStream = new System.IO.FileStream(destinationFolder + "/" + assetName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			System.IO.Stream asset = assetManager.open(assetName);
			try
			{
				ByteStreams.copy(asset, destinationStream);
			}
			finally
			{
				asset.Close();
				destinationStream.Close();
			}
		}

		/// <summary>
		/// Tells if internet is currently available on the device </summary>
		/// <param name="currentContext">
		/// @return </param>
		public static bool isInternetAvailable(Context currentContext)
		{
			ConnectivityManager conectivityManager = (ConnectivityManager) currentContext.GetSystemService(Context.CONNECTIVITY_SERVICE);
			NetworkInfo networkInfo = conectivityManager.ActiveNetworkInfo;
			if (networkInfo != null)
			{
				if (networkInfo.Type == ConnectivityManager.TYPE_WIFI)
				{
					if (networkInfo.Connected)
					{
						return true;
					}
				}
				else if (networkInfo.Type == ConnectivityManager.TYPE_MOBILE)
				{
					if (networkInfo.Connected)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if the current device has a GPS module (hardware) </summary>
		/// <returns> true if the current device has GPS </returns>
		public static bool hasGpsModule(Context context)
		{
			LocationManager locationManager = (LocationManager) context.GetSystemService(Context.LOCATION_SERVICE);
			foreach (String provider in locationManager.AllProviders)
			{
				if (provider.Equals(LocationManager.GPS_PROVIDER))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Checks if the current device has a  NETWORK module (hardware) </summary>
		/// <returns> true if the current device has NETWORK </returns>
		public static bool hasNetworkModule(Context context)
		{
			LocationManager locationManager = (LocationManager) context.GetSystemService(Context.LOCATION_SERVICE);
			foreach (String provider in locationManager.AllProviders)
			{
				if (provider.Equals(LocationManager.NETWORK_PROVIDER))
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Initializes the SKMaps framework
		/// </summary>
		public static void initializeLibrary(Context context)
		{
			DemoApplication app = (DemoApplication)context.ApplicationContext;
			// get object holding map initialization settings
			SKMapsInitSettings initMapSettings = new SKMapsInitSettings();
			// set path to map resources and initial map style
			initMapSettings.setMapResourcesPaths(app.MapResourcesDirPath, new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json"));

			SKAdvisorSettings advisorSettings = initMapSettings.AdvisorSettings;
			advisorSettings.AdvisorConfigPath = app.MapResourcesDirPath + "/Advisor";
			advisorSettings.ResourcePath = app.MapResourcesDirPath + "/Advisor/Languages";
			advisorSettings.Language = SKAdvisorSettings.SKAdvisorLanguage.LANGUAGE_EN;
			advisorSettings.AdvisorVoice = "en";
			initMapSettings.AdvisorSettings = advisorSettings;

			// EXAMPLE OF ADDING PREINSTALLED MAPS
			// initMapSettings.setPreinstalledMapsPath(app.getMapResourcesDirPath()
			// + "/PreinstalledMaps");
			// initMapSettings.setConnectivityMode(SKMaps.CONNECTIVITY_MODE_OFFLINE);

			// Example of setting light maps
			 initMapSettings.MapDetailLevel = SKMapsInitSettings.SK_MAP_DETAIL_LIGHT;
			// initialize map using the settings object

			SKMaps.Instance.initializeSKMaps(context, initMapSettings, API_KEY);
		}

	}
}