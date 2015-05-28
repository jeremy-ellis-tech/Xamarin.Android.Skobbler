using Android.Runtime;
using System;
using AndroidApplication = Android.App.Application;
using AndroidApplicationAttribute = Android.App.ApplicationAttribute;

namespace Skobbler.SDKDemo.Application
{
    [AndroidApplication]
    public class DemoApplication : AndroidApplication
    {
        public DemoApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();
            AppPrefs = new ApplicationPreferences(this);
        }

        public string MapResourcesDirPath { get; set; }
        public string MapCreatorFilePath { get; set; }
        public ApplicationPreferences AppPrefs { get; set; }

    }
}