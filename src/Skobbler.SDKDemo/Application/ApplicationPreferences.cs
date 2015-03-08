using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;
using Skobbler.SDKDemo.Database;

namespace Skobbler.SDKDemo.Application
{
	public class ApplicationPreferences
	{

		public const string DownloadStepIndexPrefKey = "downloadStepIndex";

		public const string DownloadQueuePrefKey = "downloadQueue";

		/// <summary>
		/// preference name
		/// </summary>
		public const string PrefsName = "demoAppPrefs";

		/// <summary>
		/// used for modifying values in a SharedPreferences prefs
		/// </summary>
		private ISharedPreferencesEditor _prefsEditor;

		/// <summary>
		/// reference to preference
		/// </summary>
		private ISharedPreferences _prefs;

		/// <summary>
		/// the context
		/// </summary>
		private Context _context;

		public ApplicationPreferences(Context context)
		{
			_context = context;
			_prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
			_prefsEditor = _prefs.Edit();
		}

		public virtual int GetIntPreference(string key)
		{
			return _prefs.GetInt(key, 0);
		}

		public virtual string GetStringPreference(string key)
		{
			return _prefs.GetString(key, "");
		}

		public virtual void SaveDownloadStepPreference(int downloadStepIndex)
		{
			_prefsEditor.PutInt(DownloadStepIndexPrefKey, downloadStepIndex);
			_prefsEditor.Commit();
		}

		public virtual void SaveDownloadQueuePreference(IList<DownloadResource> downloads)
		{
			string[] resourceCodes = new string[downloads.Count];
			for (int i = 0; i < downloads.Count; i++)
			{
				resourceCodes[i] = downloads[i].Code;
			}
            _prefsEditor.PutString(DownloadQueuePrefKey, JsonConvert.SerializeObject(resourceCodes));
			_prefsEditor.Commit();
		}
	}

}