Xamarin.Android.Skobbler
========================

## C# Xamarin.Android bindings for Skobbler ##

This repository and corresponding nuget package is not associated with either [Xamarin .inc](http://xamarin.com/) or [Skobbler](http://www.skobbler.com/). All rights belong to their respective owners.

This repository also includes a C# translation of the demo app included with the Skobbler SDK. This currently has a few bugs which I think are a result of my [mis]translation from the Java demo. I will be working to remove these shortly.

Please note: these bindings are in very early stages and have not yet been thoroughly tested.

This version currently uses v2.3.0 of the Skobbler Android SDK.

## Installation ##

The easiest way is to add the [nuget package](https://www.nuget.org/packages/Xamarin.Android.Skobbler/). Skobbler.dll should be added to your references on installation. You can also clone this repo and build from source should you wish to remove any ABIs to cut down on assembly size.

Fantastic documentation is [available from Skobbler](http://developer.skobbler.com/getting-started/android). The main difference you will find is that get/set method pairs in Java have been changed to  C# properties. The automatic binding generation process will also add events that correspond to callback interfaces.

####N.B. The Skobbler sdk *requires* you to have a string resource called "app_name", which your manifest's application label points at. *If you do not add this your app will crash on initialization.* ####
ie. in Resources\values\Strings.xml


    <?xml version="1.0" encoding="utf-8"?>
    <resources>
    
      <string name="app_name">AndroidOpenSourceDemo</string>
      <!-- Other resources here -->
    </resources>
and in Properties\AndroidManifest.xml

    <?xml version="1.0" encoding="utf-8"?>
    <manifest xmlns:android="http://schemas.android.com/apk/res/android" ... >
    <application android:label="@string/app_name" ... ></application>
      ...
    </manifest>


I believe this is required for analytics purposes.

You will also need to manually copy the SKMaps.zip file to your assets folder, with a build configuration of an Android asset. The zip is available in the Android SDK from [Skobbler](http://developer.skobbler.com/support#download). See the demo app for a working example.

## Additions ##

I'm looking to add `async/await` methods ontop of the existing Java interface callbacks to make things cleaner and more .NET friendly. I've added an untested implementation for nearby searches and will be adding more soon.

    try
    {
    	var searchManager = new SKSearchManager(); //No listener needed in the constructor for async calls;
    	IList<SKSearchResult> results = await searchManager.NearbySearchAsync(searchObj);
    }
    catch(Exception)
    {
    	//Catch invalid status' & other exceptions here.
    }
    


## License ##
Bindings provided under the MIT license. See LICENSE for details.

## Thanks ##
[Skobbler](http://www.skobbler.com/)

[Open Street Maps](http://www.openstreetmap.org/)

[Xamarin inc.](http://xamarin.com/)