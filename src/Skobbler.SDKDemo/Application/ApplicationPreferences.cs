using Android.Content;
using Skobbler.SDKDemo.Database;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Application
{
	public class ApplicationPreferences
	{

		public const string DOWNLOAD_STEP_INDEX_PREF_KEY = "downloadStepIndex";

		public const string DOWNLOAD_QUEUE_PREF_KEY = "downloadQueue";

		/// <summary>
		/// preference name
		/// </summary>
		public const string PREFS_NAME = "demoAppPrefs";

		/// <summary>
		/// used for modifying values in a SharedPreferences prefs
		/// </summary>
		private ISharedPreferencesEditor prefsEditor;

		/// <summary>
		/// reference to preference
		/// </summary>
		private ISharedPreferences prefs;

		/// <summary>
		/// the context
		/// </summary>
		private Context context;

		public ApplicationPreferences(Context context)
		{
			this.context = context;
			prefs = context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
			prefsEditor = prefs.Edit();
		}

		public virtual int getIntPreference(string key)
		{
			return prefs.GetInt(key, 0);
		}

		public virtual string getStringPreference(string key)
		{
			return prefs.GetString(key, "");
		}

		public virtual void saveDownloadStepPreference(int downloadStepIndex)
		{
			prefsEditor.PutInt(DOWNLOAD_STEP_INDEX_PREF_KEY, downloadStepIndex);
			prefsEditor.Commit();
		}

		public virtual void saveDownloadQueuePreference(IList<DownloadResource> downloads)
		{
			string[] resourceCodes = new string[downloads.Count];
			for (int i = 0; i < downloads.Count; i++)
			{
				resourceCodes[i] = downloads[i].Code;
			}
			prefsEditor.PutString(DOWNLOAD_QUEUE_PREF_KEY, (new Gson()).toJson(resourceCodes));
			prefsEditor.Commit();
		}
	}

}