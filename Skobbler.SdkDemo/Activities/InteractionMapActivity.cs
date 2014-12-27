using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;

namespace Skobbler.SdkDemo.Activities
{
    [Activity(Label = "InteractionMapActivity", ConfigurationChanges = ConfigChanges.Orientation)]
    public class InteractionMapActivity : Activity, ISKMapSurfaceListener
    {
        private SKMapSurfaceView _mapView;
        private bool _mapSurfaceCreated;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.activity_new_map);

            SKMapViewHolder mapViewGroup = FindViewById<SKMapViewHolder>(Resource.Id.view_group_map);
            _mapView = mapViewGroup.MapSurfaceView;
            _mapView.SetMapSurfaceListener(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _mapView.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _mapView.OnPause();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mapView = null;
        }

        public void OnActionPan()
        {

        }

        public void OnActionZoom()
        {

        }

        public void OnAnnotationSelected(SKAnnotation annotation)
        {

        }

        public void OnCompassSelected()
        {

        }

        public void OnCurrentPositionSelected()
        {

        }

        public void OnCustomPOISelected(SKMapCustomPOI customPoi)
        {

        }

        public void OnDoubleTap(SKScreenPoint point)
        {

        }

        public void OnInternationalisationCalled(int result)
        {

        }

        public void OnInternetConnectionNeeded()
        {

        }

        public void OnLongPress(SKScreenPoint point)
        {

        }

        public void OnMapActionDown(SKScreenPoint point)
        {

        }

        public void OnMapActionUp(SKScreenPoint point)
        {

        }

        public void OnMapPOISelected(SKMapPOI mapPOI)
        {

        }

        public void OnMapRegionChangeEnded(SKCoordinateRegion mapRegion)
        {

        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion mapRegion)
        {

        }

        public void OnMapRegionChanged(SKCoordinateRegion mapVisibleRegion)
        {

        }

        public void OnObjectSelected(int objectId)
        {

        }

        public void OnOffportRequestCompleted(int requestId)
        {
        }

        public void OnPOIClusterSelected(SKPOICluster poiCluster)
        {

        }

        public void OnRotateMap()
        {

        }

        public void OnScreenOrientationChanged()
        {

        }

        public void OnSingleTap(SKScreenPoint point)
        {

        }

        public void OnSurfaceCreated()
        {
            RunOnUiThread(() =>
            {
                if(!_mapSurfaceCreated)
                {
                    _mapSurfaceCreated = true;

                    RelativeLayout chessBackground = FindViewById<RelativeLayout>(Resource.Id.chess_table_background);
                    chessBackground.Visibility = ViewStates.Gone;

                    _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
                }
            });
        }
    }
}