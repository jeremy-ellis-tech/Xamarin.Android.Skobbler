using System;
using Android.Runtime;
using AndroidApplication = Android.App.Application;
using AndroidApplicationAttribute = Android.App.ApplicationAttribute;

namespace Skobbler.SDKDemo.Application
{
	/// <summary>
	/// Class that stores global application state
	/// </summary>
    [AndroidApplication]
    public class DemoApplication : AndroidApplication
	{
        public DemoApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

		/// <summary>
		/// Path to the map resources directory on the device
		/// </summary>
		private string _mapResourcesDirPath;

		/// <summary>
		/// Absolute path to the file used for mapCreator - mapcreatorFile.json
		/// </summary>
		private string _mapCreatorFilePath;

		/// <summary>
		/// Object for accessing application preferences
		/// </summary>
		private ApplicationPreferences _appPrefs;

        public override void OnCreate()
        {
            base.OnCreate();
            _appPrefs = new ApplicationPreferences(this);
        }

		public virtual string MapResourcesDirPath
		{
			set
			{
				_mapResourcesDirPath = value;
			}
			get
			{
				return _mapResourcesDirPath;
			}
		}


		public virtual string MapCreatorFilePath
		{
			get
			{
				return _mapCreatorFilePath;
			}
			set
			{
				_mapCreatorFilePath = value;
			}
		}


		public virtual ApplicationPreferences AppPrefs
		{
			get
			{
				return _appPrefs;
			}
			set
			{
				_appPrefs = value;
			}
		}

	}

}