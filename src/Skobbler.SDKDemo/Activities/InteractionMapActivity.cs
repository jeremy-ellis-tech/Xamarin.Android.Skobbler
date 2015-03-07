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

		private SKMapSurfaceView mapView;

		private bool mapSurfaceCreated;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_new_map);
			SKMapViewHolder mapViewGroup = (SKMapViewHolder) FindViewById(Resource.Id.view_group_map);
			mapView = mapViewGroup.MapSurfaceView;
			mapView.MapSurfaceListener = this;
		}

		public override void onResume()
		{
			base.OnResume();
			mapView.OnResume();
		}

		public override void onPause()
		{
			base.OnPause();
			mapView.OnPause();
		}

		protected internal override void onDestroy()
		{
			base.OnDestroy();
			mapView = null;
		}

		public override void onActionPan()
		{
		}

		public override void onActionZoom()
		{
		}

		public override void onSurfaceCreated()
		{
			// a chess background is displayed until the map becomes available
			if (!mapSurfaceCreated)
			{
				mapSurfaceCreated = true;
				// hiding the chess background when map is available
				RelativeLayout chessBackground = (RelativeLayout) FindViewById(Resource.Id.chess_table_background);
				chessBackground.Visibility = ViewStates.Gone;

				mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
			}
		}

		public override void onScreenOrientationChanged()
		{
		}

		public override void onMapRegionChanged(SKCoordinateRegion region)
		{
		}

		public override void onDoubleTap(SKScreenPoint point)
		{
		}

		public override void onSingleTap(SKScreenPoint point)
		{
		}

		public override void onRotateMap()
		{
		}

		public override void onLongPress(SKScreenPoint point)
		{
		}

		public override void onInternetConnectionNeeded()
		{
		}

		public override void onMapActionDown(SKScreenPoint point)
		{
		}

		public override void onMapActionUp(SKScreenPoint point)
		{
		}

		public override void onMapPOISelected(SKMapPOI mapPOI)
		{
		}

		public override void onAnnotationSelected(SKAnnotation annotation)
		{
		}

		public override void onCompassSelected()
		{
		}

		public override void onInternationalisationCalled(int result)
		{
		}

		public override void onCustomPOISelected(SKMapCustomPOI customPoi)
		{
		}

		public override void onPOIClusterSelected(SKPOICluster arg0)
		{
		}

		public override void onMapRegionChangeEnded(SKCoordinateRegion arg0)
		{
		}

		public override void onMapRegionChangeStarted(SKCoordinateRegion arg0)
		{
		}


		public override void onCurrentPositionSelected()
		{
		}

		public override void onObjectSelected(int arg0)
		{
		}



		public override void onBoundingBoxImageRendered(int i)
		{

		}

		public override void onGLInitializationError(string messsage)
		{

		}

	}

}