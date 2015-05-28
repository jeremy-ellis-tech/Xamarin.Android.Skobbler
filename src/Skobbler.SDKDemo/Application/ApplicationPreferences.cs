using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;
using Skobbler.SDKDemo.Database;
using System.Linq;

namespace Skobbler.SDKDemo.Application
{
    public class ApplicationPreferences
    {
        public static string DOWNLOAD_STEP_INDEX_PREF_KEY = "downloadStepIndex";
        public static string DOWNLOAD_QUEUE_PREF_KEY = "downloadQueue";
        public static string CURRENT_VERSION_CODE = "currentVersionCode";
        public static string MAP_RESOURCES_UPDATE_NEEDED = "mapResourcesUpdateNeeded";
        public static string PREFS_NAME = "demoAppPrefs";

        private ISharedPreferencesEditor _prefsEditor;
        private ISharedPreferences _prefs;
        private Context _context;

        public ApplicationPreferences(Context context)
        {
            _context = context;
            _prefs = _context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            _prefsEditor = _prefs.Edit();
        }

        public int GetIntPreference(string key)
        {
            return _prefs.GetInt(key, 0);
        }

        public string GetStringPreference(string key)
        {
            return _prefs.GetString(key, "");
        }

        public bool GetBooleanPreference(string key)
        {
            return _prefs.GetBoolean(key, false);
        }

        public void SaveDownloadStepPreference(int downloadStepIndex)
        {
            _prefsEditor.PutInt(DOWNLOAD_STEP_INDEX_PREF_KEY, downloadStepIndex);
            _prefsEditor.Commit();
        }

        public void SetCurrentVersionCode(int versionCode)
        {
            _prefsEditor.PutInt(CURRENT_VERSION_CODE, versionCode);
            _prefsEditor.Commit();
        }

        public void SaveDownloadQueuePreference(IList<DownloadResource> downloads)
        {
            var resourceCodes = downloads.Select(x => x.Code);
            //_prefsEditor.PutString(DOWNLOAD_QUEUE_PREF_KEY, new Gson().toJson(resourceCodes));
            _prefsEditor.PutString(DOWNLOAD_QUEUE_PREF_KEY, JsonConvert.SerializeObject(resourceCodes));
            _prefsEditor.Commit();
        }

        public void SaveStringPreference(string key, string value)
        {
            _prefsEditor.PutString(key, value);
            _prefsEditor.Commit();
        }

        public void SaveBooleanPreference(string key, bool value)
        {
            _prefsEditor.PutBoolean(key, value);
            _prefsEditor.Commit();
        }
    }
}