using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx;

namespace Skobbler.SDKDemo.Activities
{
    public class MapCacheActivity : Activity, ISKMapSurfaceListener, ISKNavigationListener, ISKRouteListener
    {
        public static string TAG = "MapCacheActivity";
        private SKMapSurfaceView _mapView;
        private SKMapViewHolder _mapViewGroup;
        private SKCoordinate _currentPosition = new SKCoordinate(23.593823f, 46.773716f);
        private SKCoordinate _routeDestinationPoint = new SKCoordinate(23.596824f, 46.770088f);


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_map_cache);
            _mapViewGroup = FindViewById<SKMapViewHolder>(Resource.Id.view_group_map);
            _mapViewGroup.SetMapSurfaceListener(this);
        }


        protected override void OnResume()
        {
            base.OnResume();
            _mapViewGroup.OnResume();
        }


        protected override void OnPause()
        {
            base.OnPause();
            _mapViewGroup.OnPause();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mapViewGroup = null;
        }

        public void OnSurfaceCreated(SKMapViewHolder skMapViewHolder)
        {
            _mapView = skMapViewHolder.MapSurfaceView;
            FindViewById<RelativeLayout>(Resource.Id.chess_board_background).Visibility = ViewStates.Gone;
            _mapView = _mapViewGroup.MapSurfaceView;
            _mapView.SetPositionAsCurrent(_currentPosition, 0, true);
            _mapView.SetZoom(17.0F);
            SKNavigationManager.Instance.SetNavigationListener(this);
            AddStartDestinationPins();
            LaunchRouteCalculation(_currentPosition, _routeDestinationPoint);
        }

        private void AddStartDestinationPins()
        {
            SKAnnotation annotation1 = new SKAnnotation(10)
            {
                Location = _routeDestinationPoint,
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeRed
            };

            _mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

            SKAnnotation annotation2 = new SKAnnotation(11)
            {
                Location = _currentPosition,
                MininumZoomLevel = 5,
                AnnotationType = SKAnnotation.SkAnnotationTypeGreen
            };

            _mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);
        }

        private void LaunchRouteCalculation(SKCoordinate startPoint, SKCoordinate destinationPoint)
        {
            SKRouteSettings route = new SKRouteSettings
            {
                StartCoordinate = startPoint,
                DestinationCoordinate = destinationPoint,
                NoOfRoutes = 1,
                RouteMode = SKRouteSettings.SKRouteMode.CarFastest,
                RouteExposed = true
            };

            SKRouteManager.Instance.SetRouteListener(this);
            SKRouteManager.Instance.CalculateRoute(route);
        }

        #region Empty callback methods

        public void OnActionPan()
        {
        }


        public void OnActionZoom()
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


        public void OnMapPOISelected(SKMapPOI mapPOI)
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


        public void OnCustomPOISelected(SKMapCustomPOI customPoi)
        {
        }


        public void OnDestinationReached()
        {
        }


        public void OnUpdateNavigationState(SKNavigationState navigationState)
        {

        }


        public void OnReRoutingStarted()
        {
        }


        public void OnFreeDriveUpdated(string countryCode, string streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
        {
        }


        public void OnViaPointReached(int index)
        {
        }


        public void OnVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState navigationState)
        {
        }


        public void OnPOIClusterSelected(SKPOICluster arg0)
        {
        }


        public void OnTunnelEvent(bool arg0)
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


        public void OnSignalNewAdviceWithAudioFiles(string[] arg0, bool arg1)
        {


        }


        public void OnSignalNewAdviceWithInstruction(string arg0)
        {


        }


        public void OnSpeedExceededWithAudioFiles(string[] arg0, bool arg1)
        {


        }


        public void OnSpeedExceededWithInstruction(string arg0, bool arg1)
        {


        }


        public void OnBoundingBoxImageRendered(int arg0)
        {


        }


        public void OnGLInitializationError(string arg0)
        {


        }


        public void OnRouteCalculationCompleted(SKRouteInfo skRouteInfo)
        {

        }


        public void OnRouteCalculationFailed(SKRouteListenerSKRoutingErrorCode skRoutingErrorCode)
        {

        }


        public void OnAllRoutesCompleted()
        {

        }


        public void OnServerLikeRouteCalculationCompleted(SKRouteJsonAnswer skRouteJsonAnswer)
        {

        }


        public void OnOnlineRouteComputationHanging(int i)
        {

        }
        #endregion
    }
}