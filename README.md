Xamarin.Android.Skobbler
========================

## C#  bindings for the Skobbler Android SDK ##

I am not associated with either [Skobbler](http://www.skobbler.com/) or [Xamarin .inc](http://xamarin.com/). All rights belong to their respective owners.

This repository includes a C# translation of the demo app included with the Skobbler SDK. This currently has a few bugs which I think are a result of my [mis]translation. I will be working to remove these bugs shortly.

The bindings are in very early stages so event names may change and async methods may be added until I feel it is good enough to tag a release. Keep in mind it is a relatively big project so please have patience.

####This version currently uses v 2.3.0 of the Skobbler Android SDK.####

## Installation ##

Clone the repo and build the Skobbler.Binding project. "Skobbler.dll" will then be found under "Xamarin.Android.Skobbler\Skobbler.Binding\bin\\[Debug|Release]\Skobbler.dll". Reference the assembly from your project. You can also remove a few ABIs should you wish to cut down on assembly size.

Fantastic documentation is [available from Skobbler](http://developer.skobbler.com/getting-started/android). The main difference you will find is that get/set method pairs in Java have been changed to  C# properties. The automatic binding generation process will also add events that correspond to callback interfaces.

####The Skobbler sdk *requires* you to have a string resource called "app_name", which your manifest's application label points at. *If you do not add this your app will crash on initialization.* ####
ie. in Resources\values\Strings.xml

    <resources>
    	...
      <string name="app_name">AndroidOpenSourceDemo</string>
		...
    </resources>
and in Properties\AndroidManifest.xml

    <?xml version="1.0" encoding="utf-8"?>
    <manifest xmlns:android="http://schemas.android.com/apk/res/android" ... >
    	<application android:label="@string/app_name" ... ></application>
      ...
    </manifest>



## Assets ##

You will need to manually copy the SKMaps.zip file to your assets folder, with a build configuration of an Android asset. The zip is available in the Android SDK from [Skobbler](http://developer.skobbler.com/support#download). See the demo app for an example.

## Additions & Alterations ##

I will be adding `async/await` methods to make things cleaner and more .NET friendly should you wish to use them. Here's an example using `NearbySearchAsync()` instead of `NearbySearch()`:

    try
    {
    	var searchManager = new SKSearchManager(); //No listener needed in the constructor for async calls;
    	IList<SKSearchResult> results = await searchManager.NearbySearchAsync(searchObj);
    }
	catch(SKSearchStatusException)
	{
		//Catch invalid search status here.
	}
    
`com.skobbler.ngx.config` has been renamed to `Skobbler.Ngx.Configuration` to avoid a naming warning. All other namespace names should be the same as their respective Java packages, minus the `com` prefix and capitalization.

## License ##
Bindings & demo provided under the MIT license. See LICENSE for details.

## Thanks ##
[Skobbler](http://www.skobbler.com/)

[Open Street Maps](http://www.openstreetmap.org/)

[Xamarin inc.](http://xamarin.com/)