using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    public class InteractionMapActivity : Activity, ISKMapSurfaceListener
    {

        private SKMapSurfaceView _mapView;

        private bool _mapSurfaceCreated;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_new_map);
            SKMapViewHolder mapViewGroup = (SKMapViewHolder)FindViewById(Resource.Id.view_group_map);
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

        public void OnSurfaceCreated()
        {
            // a chess background is displayed until the map becomes available
            if (!_mapSurfaceCreated)
            {
                _mapSurfaceCreated = true;
                // hiding the chess background when map is available
                RelativeLayout chessBackground = (RelativeLayout)FindViewById(Resource.Id.chess_table_background);
                chessBackground.Visibility = ViewStates.Gone;

                _mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
            }
        }

        public void OnScreenOrientationChanged()
        {
        }

        public void OnMapRegionChanged(SKCoordinateRegion region)
        {
        }

        public void OnDoubleTap(SKScreenPoint point)
        {
        }

        public void OnSingleTap(SKScreenPoint point)
        {
        }

        public void OnRotateMap()
        {
        }

        public void OnLongPress(SKScreenPoint point)
        {
        }

        public void OnInternetConnectionNeeded()
        {
        }

        public void OnMapActionDown(SKScreenPoint point)
        {
        }

        public void OnMapActionUp(SKScreenPoint point)
        {
        }

        public void OnMapPOISelected(SKMapPOI skMapPoi)
        {
        }

        public void OnAnnotationSelected(SKAnnotation annotation)
        {
        }

        public void OnCompassSelected()
        {
        }

        public void OnInternationalisationCalled(int result)
        {
        }

        public void OnCustomPOISelected(SKMapCustomPOI skMapCustomPoi)
        {
        }

        public void OnPOIClusterSelected(SKPOICluster arg0)
        {
        }

        public void OnMapRegionChangeEnded(SKCoordinateRegion arg0)
        {
        }

        public void OnMapRegionChangeStarted(SKCoordinateRegion arg0)
        {
        }


        public void OnCurrentPositionSelected()
        {
        }

        public void OnObjectSelected(int arg0)
        {
        }



        public void OnBoundingBoxImageRendered(int i)
        {

        }

        public void OnGLInitializationError(string messsage)
        {

        }

    }

}