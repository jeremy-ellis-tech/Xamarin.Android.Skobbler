
using Android.Content;
using Android.Content.Res;
using Android.Locations;
using Android.Net;
using Java.IO;
using System.IO;
using System.Text;
using File = Java.IO.File;

namespace Skobbler.SdkDemo.Util
{
    static class DemoUtils
    {
        public static bool HasGpsModule(Context context)
        {
            LocationManager locationManager = context.GetSystemService(Context.LocationService) as LocationManager;

            foreach (var provider in locationManager.AllProviders)
            {
                if (provider == LocationManager.GpsProvider)
                {
                    return true;
                }
            }

            return false;
        }

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

        public static void CopyAsset(AssetManager assetManager, string assetName, string destinationFolder)
        {

            FileOutputStream destinationStream = new FileOutputStream(new File(destinationFolder + "/" + assetName));
            Stream asset = assetManager.Open(assetName);
            try
            {
                byte[] buff = new byte[32768];
                int read;
                while ((read = asset.Read(buff, 0, buff.Length)) > 0)
                {
                    destinationStream.Write(buff, 0, 0);
                }
            }
            finally
            {
                asset.Close();
                destinationStream.Close();
            }
        }

        public static void CopyAssetsToFolder(AssetManager assetManager, string sourceFolder, string destinationFolder)
        {
            string[] assests = assetManager.List(sourceFolder);

            var destFolderFile = new File(destinationFolder);

            if (!destFolderFile.Exists())
            {
                destFolderFile.Mkdirs();
            }

            CopyAsset(assetManager, sourceFolder, destinationFolder, assests);
        }

        public static void CopyAsset(AssetManager assetManager, string sourceFolder, string destinationFolder, params string[] assestsNames)
        {
            foreach (var assetName in assestsNames)
            {
                var destinationStream = new FileOutputStream(new File(destinationFolder + "/" + assetName));
                string[] files = assetManager.List(sourceFolder + "/" + assetName);

                if (files == null || files.Length == 0)
                {
                    var asset = assetManager.Open(sourceFolder + "/" + assetName);

                    try
                    {
                        byte[] buff = new byte[32768];
                        int read;
                        while ((read = asset.Read(buff, 0, buff.Length)) > 0)
                        {
                            destinationStream.Write(buff, 0, 0);
                        }
                    }
                    finally
                    {
                        asset.Close();
                        destinationStream.Close();
                    }
                }
            }
        }

        internal static bool IsInternetAvailable(Context context)
        {
            ConnectivityManager conectivityManager = context.GetSystemService(Context.ConnectivityService) as ConnectivityManager;
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
    }
}