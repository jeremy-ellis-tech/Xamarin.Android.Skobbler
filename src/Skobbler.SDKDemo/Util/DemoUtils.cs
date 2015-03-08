using System.IO;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Content.Res;
using Android.Locations;
using Android.Net;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.SDKDemo.Application;
using File = Java.IO.File;

namespace Skobbler.SDKDemo.Util
{
	public class DemoUtils
	{

		private const string ApiKey = "API_KEY_HERE";

		/// <summary>
		/// Gets formatted time from a given number of seconds </summary>
		/// <param name="timeInSec">
		/// @return </param>
		public static string FormatTime(int timeInSec)
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
		public static string FormatDistance(int distInMeters)
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
		/// <exception cref="Java.IO.IOException"> </exception>
		public static void CopyAssetsToFolder(AssetManager assetManager, string sourceFolder, string destinationFolder)
		{
			string[] assets = assetManager.List(sourceFolder);
			File destFolderFile = new File(destinationFolder);
			if (!destFolderFile.Exists())
			{
				destFolderFile.Mkdirs();
			}
			copyAsset(assetManager, sourceFolder, destinationFolder, assets);
		}

		/// <summary>
		/// Copies files from assets to destination folder </summary>
		/// <param name="assetManager"> </param>
		/// <param name="sourceFolder"> </param>
		/// <param name="assetsNames"> </param>
		/// <exception cref="Java.IO.IOException"> </exception>
		public static void copyAsset(AssetManager assetManager, string sourceFolder, string destinationFolder, params string[] assetsNames)
		{

			foreach (string assetName in assetsNames)
			{
				Stream destinationStream = new FileStream(destinationFolder + "/" + assetName, FileMode.Create, FileAccess.Write);
				string[] files = assetManager.List(sourceFolder + "/" + assetName);
				if (files == null || files.Length == 0)
				{

					Stream asset = assetManager.Open(sourceFolder + "/" + assetName);
					try
					{
						//ByteStreams.copy(asset, destinationStream);
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
		/// <exception cref="Java.IO.IOException"> </exception>
		public static void copyAsset(AssetManager assetManager, string assetName, string destinationFolder)
		{

			Stream destinationStream = new FileStream(destinationFolder + "/" + assetName, FileMode.Create, FileAccess.Write);
			Stream asset = assetManager.Open(assetName);
			try
			{
				//ByteStreams.copy(asset, destinationStream);
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
		public static bool IsInternetAvailable(Context currentContext)
		{
			ConnectivityManager conectivityManager = (ConnectivityManager) currentContext.GetSystemService(Context.ConnectivityService);
			NetworkInfo networkInfo = conectivityManager.ActiveNetworkInfo;
			if (networkInfo != null)
			{
				if (networkInfo.Type == ConnectivityType.Wifi)
				{
					if (networkInfo.IsConnected)
					{
						return true;
					}
				}
                else if (networkInfo.Type == ConnectivityType.Mobile)
				{
                    if (networkInfo.IsConnected)
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
		public static bool HasGpsModule(Context context)
		{
			LocationManager locationManager = (LocationManager) context.GetSystemService(Context.LocationService);
			foreach (string provider in locationManager.AllProviders)
			{
				if (provider.Equals(LocationManager.GpsProvider))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Checks if the current device has a  NETWORK module (hardware) </summary>
		/// <returns> true if the current device has NETWORK </returns>
		public static bool HasNetworkModule(Context context)
		{
			LocationManager locationManager = (LocationManager) context.GetSystemService(Context.LocationService);
		    return locationManager.AllProviders.Any(provider => provider.Equals(LocationManager.NetworkProvider));
		}


		/// <summary>
		/// Initializes the SKMaps framework
		/// </summary>
		public static void InitializeLibrary(Context context)
		{
			DemoApplication app = (DemoApplication)context.ApplicationContext;
			// get object holding map initialization settings
			SKMapsInitSettings initMapSettings = new SKMapsInitSettings();
			// set path to map resources and initial map style
			initMapSettings.SetMapResourcesPaths(app.MapResourcesDirPath, new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json"));

			SKAdvisorSettings advisorSettings = initMapSettings.AdvisorSettings;
			advisorSettings.AdvisorConfigPath = app.MapResourcesDirPath + "/Advisor";
			advisorSettings.ResourcePath = app.MapResourcesDirPath + "/Advisor/Languages";
			advisorSettings.Language = SKAdvisorSettings.SKAdvisorLanguage.LanguageEn;
			advisorSettings.AdvisorVoice = "en";
			initMapSettings.AdvisorSettings = advisorSettings;

			// EXAMPLE OF ADDING PREINSTALLED MAPS
			// initMapSettings.setPreinstalledMapsPath(app.getMapResourcesDirPath()
			// + "/PreinstalledMaps");
			// initMapSettings.setConnectivityMode(SKMaps.CONNECTIVITY_MODE_OFFLINE);

			// Example of setting light maps
			 initMapSettings.MapDetailLevel = SKMapsInitSettings.SkMapDetailLight;
			// initialize map using the settings object

			SKMaps.Instance.InitializeSKMaps(context, initMapSettings, ApiKey);
		}

	}
}