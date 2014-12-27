Xamarin.Android.Skobbler
========================

## C# Xamarin.Android bindings for Skobbler ##

This repository and corresponding nuget package are not associated with either [Xamarin .inc](http://xamarin.com/) or [Skobbler](http://www.skobbler.com/). All rights belong to their respective owners.

This repository also includes a C# translation of the demo app included with the Skobbler SDK. This currently has a few bugs which I think are a result of my [mis]translation from the Java demo. I will be working to remove these shortly.

## Usage ##

The easiest way is to grab the [nuget package](https://www.nuget.org/packages/Xamarin.Android.Skobbler/). Skobbler.dll should be added to your references on installation. You can also clone this repo and build from source should you wish to remove any ABIs to cut down on assembly size.

Fantastic documentation is [available from Skobbler](http://developer.skobbler.com/getting-started/android). The main difference you will find is that get/set method pairs in Java have been changed to  C# properties. The automatic binding generation process will also add events that correspond to callback interfaces.

###The Skobbler sdk requires you to have a string resource called "app_name", which your manifest application label points at. If you do not add this your app will crash on initialization. ###
ie. Resources\values\Strings.xml


    <?xml version="1.0" encoding="utf-8"?>
    <resources>
    
      <string name="app_name">AndroidOpenSourceDemo</string>
      <!-- Other resources here -->
    </resources>
Properties\AndroidManifest.xml

    <?xml version="1.0" encoding="utf-8"?>
    <manifest xmlns:android="http://schemas.android.com/apk/res/android" package="Skobbler.SdkDemo" android:versionCode="1" android:versionName="1.0">
    <uses-sdk android:minSdkVersion="8" android:targetSdkVersion="10" />
    <application android:name="skobbler.demo.application.DemoApplication" android:label="@string/app_name" android:icon="@drawable/ic_launcher" android:allowBackup="true"></application>
    <!--Permissions go here -->
    </manifest>

## License ##
Bindings provided under the MIT license. See LICENSE for details.

## Thanks ##
[Skobbler](http://www.skobbler.com/)

[Open Street Maps](http://www.openstreetmap.org/)

[Xamarin inc.](http://xamarin.com/)