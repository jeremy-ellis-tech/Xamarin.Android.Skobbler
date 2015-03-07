namespace Skobbler.SDKDemo.Application
{
	/// <summary>
	/// Class that stores global application state
	/// </summary>
	public class DemoApplication : Application
	{

		/// <summary>
		/// Path to the map resources directory on the device
		/// </summary>
		private string mapResourcesDirPath;

		/// <summary>
		/// Absolute path to the file used for mapCreator - mapcreatorFile.json
		/// </summary>
		private string mapCreatorFilePath;

		/// <summary>
		/// Object for accessing application preferences
		/// </summary>
		private ApplicationPreferences appPrefs;

		public override void onCreate()
		{
			base.OnCreate();
			appPrefs = new ApplicationPreferences(this);
		}

		public virtual string MapResourcesDirPath
		{
			set
			{
				this.mapResourcesDirPath = value;
			}
			get
			{
				return mapResourcesDirPath;
			}
		}


		public virtual string MapCreatorFilePath
		{
			get
			{
				return mapCreatorFilePath;
			}
			set
			{
				this.mapCreatorFilePath = value;
			}
		}


		public virtual ApplicationPreferences AppPrefs
		{
			get
			{
				return appPrefs;
			}
			set
			{
				this.appPrefs = value;
			}
		}

	}

}