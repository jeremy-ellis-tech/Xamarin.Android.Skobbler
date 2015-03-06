using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Skobbler.SDKDemo.Application
{
    class ApplicationPreferences
    {
        public static readonly string DownloadStepIndexPrefKey = "downloadStepIndex";
        public static readonly string DownloadQueuePrefKey = "downloadQueue";

        public static readonly string PrefsName = "demoAppPrefs";

        private SharedPreferences.Editor _prefsEditor;
        private SharedPreferences _prefs;
        private Context _context;

        public ApplicationPreferences(Context context)
        {
            _context = context;
            _prefs = _context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
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

        public void SaveDownloadStepPreferences(int downloadStepIndex)
        {
            _prefsEditor.PutInt(DownloadStepIndexPrefKey, downloadStepIndex);
            _prefsEditor.Commit();
        }

        public void SaveDownloadQueuePreference(List<DownloadResource> downloads)
        {
            IEnumerable<string> resourcesCodes = downloads.Select(x => x.Get().GetCode());
            _prefsEditor.PutString(DownloadQueuePrefKey, new Gson().toJson(resourcesCodes));
            _prefsEditor.Commit();
        }
    }
}