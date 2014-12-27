using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx.Packages;
using Skobbler.SdkDemo.Application;
using Skobbler.SdkDemo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skobbler.SdkDemo.Activities
{
    [Activity(Label = "DownloadActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class DownloadActivity : Activity
    {
        private const int NoBytesIntoOneMB = 1048576;

        private static string _packagesPath;

        private DemoApplication _application;
        private ProgressBar _progressBar;
        private Button _startDownloadButotn;
        private TextView _downloadPercentage;

        private DownloadPackage _downloadPackage;
        private int _downloadResourceIndex;

        private List<string> _downloadResourceUrls;
        private List<string> _downloadResourceExtensions;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _application = Application as DemoApplication;

            SetContentView(Resource.Layout.activity_download);
            _packagesPath = _application.MapResourcesDirPath + "/Maps/downloads/";
            _progressBar = FindViewById<ProgressBar>(Resource.Id.download_progress_bar);
            _startDownloadButotn = FindViewById<Button>(Resource.Id.download_button);
            _downloadPercentage = FindViewById<TextView>(Resource.Id.download_percentage_text);
            _downloadPackage = _application.PackageMap[Intent.GetStringExtra("packageCode")];
            _startDownloadButotn.Text = Resources.GetString(Resource.String.label_download) + " " + _downloadPackage.Name;
            PrepareDownloadResources();
        }

        private void PrepareDownloadResources()
        {
            _downloadResourceUrls = new List<string>();
            _downloadResourceExtensions = new List<string>();

            _downloadResourceIndex = 0;

            SKPackageURLInfo info = SKPackageManager.Instance.GetURLInfoForPackageWithCode(_downloadPackage.Code);
            string mapURL = info.MapURL;
            string texturesURL = info.TexturesURL;
            string nbFilesZipUrl = info.NameBrowserFilesURL;

            _downloadResourceUrls.Add(mapURL);
            _downloadResourceExtensions.Add(".skm");

            _downloadResourceUrls.Add(texturesURL);
            _downloadResourceExtensions.Add(".txg");

            _downloadResourceUrls.Add(nbFilesZipUrl.Replace("\\.zip", ".ngi"));
            _downloadResourceExtensions.Add(".ngi");

            _downloadResourceUrls.Add(nbFilesZipUrl.Replace("\\.zip", ".ngi.dat"));
            _downloadResourceExtensions.Add(".ngi.dat");
        }

        [Export("OnClick")]
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.download_button:
                    _startDownloadButotn.Enabled = false;
                    DownloadResource(_downloadResourceUrls[0], _downloadResourceExtensions[0]);
                    break;
                default:
                    break;
            }
        }

        private void DownloadResource(string url, string extension)
        {
            Task.Run(() =>
            {
                long lastProgressUpdateTime = SystemClock.CurrentThreadTimeMillis();

                //using (var httpClient = new HttpClient())
                //{

                //}
            });
        }

        private void UpdateDownloadProgress(long downloadedSize, long totalSize)
        {
            RunOnUiThread(() =>
            {
                int progress = (int)(_progressBar.Max * ((float)downloadedSize / (float)totalSize));
                _progressBar.Progress = progress;
                _downloadPercentage.Text = ((float)progress / 10 + "%");
            });
        }

        private void UpdateOnFinishDownload()
        {
            RunOnUiThread(() =>
            {
                _progressBar.Progress = _progressBar.Max;

                if(_downloadResourceExtensions[_downloadResourceIndex] == ".skm")
                {
                    _downloadPercentage.Text = "100%";
                }

                if(_downloadResourceIndex >= _downloadResourceUrls.Count - 1)
                {
                    SKPackageManager.Instance.AddOfflinePackage(_packagesPath, _downloadPackage.Code);
                    Toast.MakeText(this, "Map of " + _downloadPackage + " is now availible offline", ToastLength.Short).Show();
                }
                else
                {
                    _downloadResourceIndex++;
                    DownloadResource(_downloadResourceUrls[_downloadResourceIndex], _downloadResourceExtensions[_downloadResourceIndex]);
                }
            });
        }
    }
}