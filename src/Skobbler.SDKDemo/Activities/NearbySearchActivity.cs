using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.AdjustPan, ConfigurationChanges = (ConfigChanges.Orientation | ConfigChanges.ScreenSize))]
    public class NearbySearchActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_nearby_search);
        }

        [Export("onClick")]
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.search_button:
                    if (ValidateCoordinates())
                    {
                        short radius;
                        double latitude, longitude;
                        if (Int16.TryParse(FindViewById<TextView>(Resource.Id.radius_field).Text, out radius)
                            && Double.TryParse(FindViewById<TextView>(Resource.Id.latitude_field).Text, out latitude)
                            && Double.TryParse(FindViewById<TextView>(Resource.Id.longitude_field).Text, out longitude))
                        {
                            Intent intent = new Intent(this, typeof(NearbySearchResultsActivity));

                            intent.PutExtra("radius", radius);
                            intent.PutExtra("latitude", latitude);
                            intent.PutExtra("longitude", longitude);
                            intent.PutExtra("searchTopic", FindViewById<TextView>(Resource.Id.search_topic_field).Text);

                            StartActivity(intent);
                        }
                        else
                        {
                            Toast.MakeText(this, "Provide a short value, maximum value is 32 767.", ToastLength.Short).Show();
                        }
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

        private bool ValidateCoordinates()
        {
            string latString = FindViewById<TextView>(Resource.Id.latitude_field).Text;
            string longString = FindViewById<TextView>(Resource.Id.longitude_field).Text;

            double latitude, longitude;

            if (Double.TryParse(latString, out latitude) && Double.TryParse(longString, out longitude))
            {
                return (latitude < 90 && latitude > -90 && longitude < 180 && longitude > -180);
            }
            else return false;

        }

    }
}