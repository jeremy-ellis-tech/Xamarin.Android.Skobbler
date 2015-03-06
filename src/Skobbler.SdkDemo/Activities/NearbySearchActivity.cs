using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using System;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(Label = "NearbySearchActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class NearbySearchActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_nearby_search);
        }

        [Export("OnClick")]
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.search_button:
                    if(ValidateCoordinates())
                    {
                        int radius = Int32.Parse(FindViewById<TextView>(Resource.Id.radius_field).Text.ToString());
                        var intent = new Intent(this, typeof(NearbySearchResultsActivity));
                        intent.PutExtra("radius", radius);
                        intent.PutExtra("latitude", Double.Parse(FindViewById<TextView>(Resource.Id.latitude_field).Text.ToString()));
                        intent.PutExtra("longitude", Double.Parse(FindViewById<TextView>(Resource.Id.longitude_field).Text.ToString()));
                        intent.PutExtra("searchTopic", FindViewById<TextView>(Resource.Id.search_topic_field).Text.ToString());

                        StartActivity(intent);
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
            try
            {
                string latString = FindViewById<TextView>(Resource.Id.latitude_field).Text.ToString();
                string longString = FindViewById<TextView>(Resource.Id.longitude_field).Text.ToString();

                double latitude = Double.Parse(latString);
                double longitude = Double.Parse(longString);

                if(latitude > 90 || latitude < -90)
                {
                    return false;
                }
                if(longitude > 180 || longitude < -180)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}