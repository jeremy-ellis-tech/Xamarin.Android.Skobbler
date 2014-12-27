using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Search;
using Skobbler.SdkDemo.Application;
using System;

namespace Skobbler.SdkDemo.Activities
{
    [Activity(Label = "ReverseGeocodingActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class ReverseGeocodingActivity : Activity
    {
        private DemoApplication _application;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_reverse_geocoding);

            _application = Application as DemoApplication;
        }

        [Export("OnClick")]
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.reverse_geocode_button:
                    SKCoordinate poistion = GetPosition();
                    if(poistion != null)
                    {
                        SKSearchResult result = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(poistion);
                        string text = result != null ? result.Name : "NULL";
                        if(result != null && result.ParentsList != null)
                        {
                            string separator = ", ";
                            foreach (var parent in result.ParentsList)
                            {
                                text += separator + parent.ParentName;
                            }
                        }

                        FindViewById<TextView>(Resource.Id.reverse_geocoding_result).Text = text;
                    }
                    else
                    {
                        Toast.MakeText(this, "Invalid latitude or longitude was provided", ToastLength.Short).Show();
                    }
                    break;
                default:
                    break;
            }
        }

        private SKCoordinate GetPosition()
        {
            try
            {
                string latString = FindViewById<TextView>(Resource.Id.latitude_field).Text.ToString();
                string longString = FindViewById<TextView>(Resource.Id.longitude_field).Text.ToString();
                double latitude = Double.Parse(latString);
                double longitude = Double.Parse(longString);

                if(latitude > 90 || latitude < -90)
                {
                    return null;
                }
                if(longitude > 180 || longitude < -180)
                {
                    return null;
                }

                return new SKCoordinate(longitude, latitude);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}