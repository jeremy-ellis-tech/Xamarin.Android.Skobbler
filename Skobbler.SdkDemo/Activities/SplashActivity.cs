using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Java.IO;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SdkDemo.Application;
using Skobbler.SdkDemo.Util;
using System.Threading.Tasks;
using Exception = System.Exception;
using String = System.String;

namespace Skobbler.SdkDemo.Activities
{
    [Activity(Label = "Skobbler.Demo", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class SplashActivity : Activity, ISKPrepareMapTextureListener, ISKMapUpdateListener
    {
        private const string ApiKey = "API_KEY_HERE";
        private string _mapResourcesDirPath = String.Empty;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_splash);

            SKLogging.EnableLogs(true);

            File externalDir = GetExternalFilesDir(null);

            if (externalDir != null)
            {
                _mapResourcesDirPath = externalDir + "/" + "SKMaps/";
            }
            else
            {
                _mapResourcesDirPath = externalDir + "/" + "SKMaps/";
            }

            var app = Application as DemoApplication;

            if (app != null)
            {
                app.MapResourcesDirPath = _mapResourcesDirPath;
            }

            if (!new File(_mapResourcesDirPath).Exists())
            {
                new SKPrepareMapTextureThread(this, _mapResourcesDirPath, "SKMaps.zip", this).Start();
                CopyOtherResources();
                PrepareMapCreatorFile();
            }
            else
            {
                Toast.MakeText(this, "Map resources copied in a previous run", ToastLength.Short).Show();
                PrepareMapCreatorFile();
                InitializeLibrary();
                Finish();
                StartActivity(new Intent(this, typeof(MapActivity)));
            }
        }

        private void PrepareMapCreatorFile()
        {
            DemoApplication app = Application as DemoApplication;
            Task.Run(() =>
            {
                try
                {
                    string mapCreatorFolderPath = _mapResourcesDirPath + "MapCreator";
                    File mapCreatorFolder = new File(mapCreatorFolderPath);

                    if (!mapCreatorFolder.Exists())
                    {
                        mapCreatorFolder.Mkdirs();
                    }

                    app.MapCreatorFilePath = mapCreatorFolderPath + "/mapcreatorFile.json";
                    DemoUtils.CopyAsset(Assets, "MapCreator", mapCreatorFolderPath, "mapcreatorFile.json");
                }
                catch (Exception)
                {
                }
            });
        }

        public void OnMapTexturesPrepared(bool prepared)
        {
            InitializeLibrary();

            RunOnUiThread(() =>
            {
                Toast.MakeText(this, "Map resources were copied", ToastLength.Short).Show();
                Finish();
                StartActivity(new Intent(this, typeof(MapActivity)));
            });
        }

        private void InitializeLibrary()
        {
            var app = Application as DemoApplication;

            if (app == null) return;

            var initMapSettings = new SKMapsInitSettings();
            initMapSettings.SetMapResourcesPaths(app.MapResourcesDirPath, new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json"));

            SKAdvisorSettings advisorSettings = initMapSettings.AdvisorSettings;
            advisorSettings.Language = "en";
            advisorSettings.AdvisorVoice = "en";
            initMapSettings.AdvisorSettings = advisorSettings;

            SKVersioningManager.Instance.SetMapUpdateListener(this);
            SKMaps.Instance.InitializeSKMaps(this, initMapSettings, ApiKey);
        }

        private void CopyOtherResources()
        {
            Task.Run(() =>
            {
                try
                {
                    string tracksPath = _mapResourcesDirPath + "GPXTracks";
                    File tracksDir = new File(tracksPath);
                    if(!tracksDir.Exists())
                    {
                        tracksDir.Mkdirs();
                    }

                    DemoUtils.CopyAssetsToFolder(Assets, "GPXTracks", _mapResourcesDirPath + "GPXTracks");

                    string imagesPath = _mapResourcesDirPath + "images";
                    File imagesDir = new File(imagesPath);

                    if (!imagesDir.Exists())
                    {
                        imagesDir.Mkdirs();
                    }

                    DemoUtils.CopyAssetsToFolder(Assets, "images", _mapResourcesDirPath + "images");
                }
                catch (Exception ex)
                {
                    Log.Error("", ex.ToString());
                }
            });
        }

        public void OnMapVersionSet(int newVersion)
        {

        }

        public void OnNewVersionDetected(int newVersion)
        {
            Log.Error("", "new version " + newVersion);
        }

        public void OnNoNewVersionDetected()
        {

        }

        public void OnVersionFileDownloadTimeout()
        {

        }
    }
}

