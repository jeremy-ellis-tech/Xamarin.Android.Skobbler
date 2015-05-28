using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Search;
using Skobbler.SDKDemo.Application;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class ReverseGeocodingActivity : Activity
    {

        private DemoApplication application;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_reverse_geocoding);
            application = Application as DemoApplication;
        }

        [Export("onClick")]
        public void onClick(View v)
        {
        switch (v.Id)
            {
            case Resource.Id.reverse_geocode_button:
                SKCoordinate position = GetPosition();
                if (position != null)
                {
                    SKSearchResult result = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(position);
                    string text = result != null ? result.Name : "NULL";
                    if (result != null && result.ParentsList != null)
                    {
                        string separator = ", ";
                        foreach (SKSearchResultParent parent in result.ParentsList)
                        {
                            text += separator + parent.ParentName;
                        }
                    }
                    
                    FindViewById<TextView>(Resource.Id.reverse_geocoding_result).Text = text;
                } else {
                    Toast.MakeText(this, "Invalid latitude or longitude was provided", ToastLength.Short).Show();
                }
                break;
            default:
                break;
        }
    }

        private SKCoordinate GetPosition()
    {
        string latString = FindViewById<TextView>(Resource.Id.latitude_field).Text;
        string longString = FindViewById<TextView>(Resource.Id.longitude_field).Text;

        double latitude, longitude;

        if(Double.TryParse(latString, out latitude) && Double.TryParse(longString, out longitude)
            && latitude < 90 && latitude > -90 && longitude < 180 && longitude > -180)
        {
            return new SKCoordinate(longitude, latitude);
        }
        else
	    {
            return null;
	    }
    }
    }
}