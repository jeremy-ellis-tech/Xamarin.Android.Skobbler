using Android.App;
using Android.Runtime;
using Skobbler.SDKDemo.Model;
using System;
using System.Collections.Generic;
using AndroidApplication = Android.App.Application;

namespace Skobbler.SDKDemo.Application
{
    [Application]
    class DemoApplication : AndroidApplication
    {
        public DemoApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {

        }

        public string MapResourcesDirPath { get; set; }
        public Dictionary<string, DownloadPackage> PackageMap { get; set; }
        public string MapCreatorFilePath { get; set; }
        public ApplicationPreferences AppPrefs { get; set; }

        public override void OnCreate()
        {
            base.OnCreate();
            AppPrefs = new ApplicationPreferences(this);
        }
    }
}