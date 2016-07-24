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
using Android.App;
using Skobbler.Ngx.Util;
using Android.OS;
using Android.Views;
using Android.Util;
using Android.Content.PM;
using Java.IO;
using System;

namespace Skobbler.SDKDemo.Util
{
    public class DemoUtils
    {

        public static bool IsMultipleMapSupportEnabled;

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

        public static string FormatDistance(int distInMeters)
        {
            if (distInMeters < 1000)
            {
                return distInMeters + "m";
            }
            else
            {
                return ((float)distInMeters / 1000) + "km";
            }
        }

        public static void CopyAssetsToFolder(AssetManager assetManager, string sourceFolder, string destinationFolder)
        {
            string[] assets = assetManager.List(sourceFolder);

            File destFolderFile = new File(destinationFolder);
            if (!destFolderFile.Exists())
            {
                destFolderFile.Mkdirs();
            }
            CopyAsset(assetManager, sourceFolder, destinationFolder, assets);
        }

        public static void CopyAsset(AssetManager assetManager, string sourceFolder, string destinationFolder, params string[] assetsNames)
        {

            foreach (string assetName in assetsNames)
            {
                var destinationStream = new FileStream(destinationFolder + "/" + assetName, FileMode.Create);

                string[] files = assetManager.List(sourceFolder + "/" + assetName);

                if (files == null || files.Length == 0)
                {
                    var asset = assetManager.Open(sourceFolder + "/" + assetName);

                    try
                    {
                        asset.CopyTo(destinationStream);
                    }
                    finally
                    {
                        asset.Close();
                        destinationStream.Close();
                    }
                }
            }
        }

        public static bool IsInternetAvailable(Context currentContext)
        {
            ConnectivityManager conectivityManager = currentContext.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
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

        public static bool HasGpsModule(Context context)
        {
            LocationManager locationManager = context.GetSystemService(Context.LocationService) as LocationManager;
            foreach (string provider in locationManager.AllProviders)
            {
                if (provider.Equals(LocationManager.GpsProvider))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasNetworkModule(Context context)
        {
            LocationManager locationManager = context.GetSystemService(Context.LocationService) as LocationManager;
            foreach (string provider in locationManager.AllProviders)
            {
                if (provider.Equals(LocationManager.NetworkProvider))
                {
                    return true;
                }
            }
            return false;
        }

        public static void ShowApiKeyErrorDialog(Activity currentActivity)
        {
            new AlertDialog.Builder(currentActivity)
                .SetTitle("Error")
                .SetMessage("API_KEY not set")
                .SetCancelable(false)
                .SetPositiveButton(currentActivity.Resources.GetString(Resource.String.ok_label), (s, e) => Process.KillProcess(Process.MyPid()))
                .Show();
        }

        public static ScreenOrientation GetExactScreenOrientation(Activity activity)
        {
            Display defaultDisplay = activity.WindowManager.DefaultDisplay;
            SurfaceOrientation rotation = defaultDisplay.Rotation;
            DisplayMetrics dm = new DisplayMetrics();
            defaultDisplay.GetMetrics(dm);
            int width = dm.WidthPixels;
            int height = dm.HeightPixels;
            ScreenOrientation orientation;
            // if the device's natural orientation is portrait:
            if ((rotation == SurfaceOrientation.Rotation0 || rotation == SurfaceOrientation.Rotation180) && height > width || (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270) &&
                    width > height)
            {
                switch (rotation)
                {
                    case SurfaceOrientation.Rotation0:
                        orientation = ScreenOrientation.Portrait;
                        break;
                    case SurfaceOrientation.Rotation90:
                        orientation = ScreenOrientation.Landscape;
                        break;
                    case SurfaceOrientation.Rotation180:
                        orientation = ScreenOrientation.Portrait;
                        break;
                    case SurfaceOrientation.Rotation270:
                        orientation = ScreenOrientation.ReverseLandscape;
                        break;
                    default:
                        // Logging.writeLog(TAG, "Unknown screen orientation. Defaulting to " + "portrait.", Logging.LOG_DEBUG);
                        orientation = ScreenOrientation.Portrait;
                        break;
                }
            }
            // if the device's natural orientation is landscape or if the device
            // is square:
            else
            {
                switch (rotation)
                {
                    case SurfaceOrientation.Rotation0:
                        orientation = ScreenOrientation.Landscape;
                        break;
                    case SurfaceOrientation.Rotation90:
                        orientation = ScreenOrientation.Portrait;
                        break;
                    case SurfaceOrientation.Rotation180:
                        orientation = ScreenOrientation.ReverseLandscape;
                        break;
                    case SurfaceOrientation.Rotation270:
                        orientation = ScreenOrientation.ReversePortrait;
                        break;
                    default:
                        orientation = ScreenOrientation.Landscape;
                        break;
                }
            }

            return orientation;
        }

        public static void DeleteFileOrDirectory(File file)
        {
            if (file.IsDirectory)
            {
                string[] children = file.List();
                for (int i = 0; i < children.Length; i++)
                {
                    if (new File(file, children[i]).IsDirectory && !children[i].Equals("PreinstalledMaps") && !children[i].Equals("Maps"))
                    {
                        DeleteFileOrDirectory(new File(file, children[i]));
                    }
                    else
                    {
                        new File(file, children[i]).Delete();
                    }
                }
            }
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }

    }
}