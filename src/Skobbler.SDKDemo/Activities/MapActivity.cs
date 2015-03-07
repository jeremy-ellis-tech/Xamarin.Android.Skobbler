using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Preferences;
using Android.Speech.Tts;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Skobbler.Ngx;
using Skobbler.Ngx.Map;
using Skobbler.Ngx.Map.RealReach;
using Skobbler.Ngx.Navigation;
using Skobbler.Ngx.PoiTracker;
using Skobbler.Ngx.Positioner;
using Skobbler.Ngx.ReverseGeocode;
using Skobbler.Ngx.Routing;
using Skobbler.Ngx.SDKTools.NavigationUI;
using Skobbler.Ngx.Search;
using Skobbler.Ngx.Util;
using Skobbler.Ngx.Versioning;
using Skobbler.SDKDemo.Application;
using Skobbler.SDKDemo.Util;
using System;
using System.Collections.Generic;

namespace Skobbler.SDKDemo.Activities
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation)]
    class MapActivity : Activity, ISKMapSurfaceListener, ISKRouteListener, ISKNavigationListener, ISKRealReachListener, ISKPOITrackerListener, ISKCurrentPositionListener, ISensorEventListener, ISKMapUpdateListener, ISKToolsNavigationListener
    {
        private const sbyte GREEN_PIN_ICON_ID = 0;

		private const sbyte RED_PIN_ICON_ID = 1;

		public const sbyte VIA_POINT_ICON_ID = 4;

		private const string TAG = "MapActivity";

		public const int TRACKS = 1;

		private enum MapOption
		{
			MAP_DISPLAY,
			MAP_OVERLAYS,
			ALTERNATIVE_ROUTES,
			MAP_STYLES,
			REAL_REACH,
			TRACKS,
			ANNOTATIONS,
			ROUTING_AND_NAVIGATION,
			POI_TRACKING,
			HEAT_MAP,
			MAP_INTERACTION,
			NAVI_UI
		}

		private enum MapAdvices
		{
			TEXT_TO_SPEECH,
			AUDIO_FILES
		}


		public static SKCategories.SKPOICategory[] heatMapCategories;

		/// <summary>
		/// Current option selected
		/// </summary>
		private MapOption currentMapOption = MapOption.MAP_DISPLAY;

		/// <summary>
		/// Application context object
		/// </summary>
		private DemoApplication app;

		/// <summary>
		/// Surface view for displaying the map
		/// </summary>
		private SKMapSurfaceView mapView;

		/// <summary>
		/// Options menu
		/// </summary>
		private View menu;

		/// <summary>
		/// View for selecting alternative routes
		/// </summary>
		private View altRoutesView;

		/// <summary>
		/// View for selecting the map style
		/// </summary>
		private LinearLayout mapStylesView;

		/// <summary>
		/// View for real reach time profile
		/// </summary>
		private RelativeLayout realReachTimeLayout;

		/// <summary>
		/// View for real reach energy profile
		/// </summary>
		private RelativeLayout realReachEnergyLayout;

		/// <summary>
		/// Buttons for selecting alternative routes
		/// </summary>
		private Button[] altRoutesButtons;

		/// <summary>
		/// Bottom button
		/// </summary>
		private Button bottomButton;

		/// <summary>
		/// The current position button
		/// </summary>
		private Button positionMeButton;

		/// <summary>
		/// Custom view for adding an annotation
		/// </summary>
		private RelativeLayout customView;

		/// <summary>
		/// The heading button
		/// </summary>
		private Button headingButton;

		/// <summary>
		/// The map popup view
		/// </summary>
		private SKCalloutView mapPopup;

		/// <summary>
		/// Custom callout view title
		/// </summary>
		private TextView popupTitleView;

		/// <summary>
		/// Custom callout view description
		/// </summary>
		private TextView popupDescriptionView;

		/// <summary>
		/// Ids for alternative routes
		/// </summary>
		private IList<int?> routeIds = new List<int?>();

		/// <summary>
		/// Tells if a navigation is ongoing
		/// </summary>
		private bool navigationInProgress;

		/// <summary>
		/// Tells if a navigation is ongoing
		/// </summary>
		private bool skToolsNavigationInProgress;

		/// <summary>
		/// Tells if a route calculation is ongoing
		/// </summary>
		private bool skToolsRouteCalculated;

		/// <summary>
		/// POIs to be detected on route
		/// </summary>
		private IDictionary<int?, SKTrackablePOI> trackablePOIs;

		/// <summary>
		/// Trackable POIs that are currently rendered on the map
		/// </summary>
		private IDictionary<int?, SKTrackablePOI> drawnTrackablePOIs;

		/// <summary>
		/// Tracker manager object
		/// </summary>
		private SKPOITrackerManager poiTrackingManager;

		/// <summary>
		/// Current position provider
		/// </summary>
		private SKCurrentPositionProvider currentPositionProvider;

		/// <summary>
		/// Current position
		/// </summary>
		private SKPosition currentPosition;

		/// <summary>
		/// Tells if heading is currently active
		/// </summary>
		private bool headingOn;


		/// <summary>
		/// Real reach range
		/// </summary>
		private int realReachRange;

		/// <summary>
		/// Real reach default vehicle type
		/// </summary>
		private sbyte realReachVehicleType = SKRealReachSettings.VehicleTypePedestrian;

		/// <summary>
		/// Pedestrian button
		/// </summary>
		private ImageButton pedestrianButton;

		/// <summary>
		/// Bike button
		/// </summary>
		private ImageButton bikeButton;

		/// <summary>
		/// Car button
		/// </summary>
		private ImageButton carButton;

		/// <summary>
		/// Navigation UI layout
		/// </summary>
		private RelativeLayout navigationUI;



		private bool isStartPointBtnPressed = false, isEndPointBtnPressed = false, isViaPointSelected = false;

		/// <summary>
		/// The start point(long/lat) for the route.
		/// </summary>
		private SKCoordinate startPoint;

		/// <summary>
		/// The destination(long/lat) point for the route
		/// </summary>
		private SKCoordinate destinationPoint;

		/// <summary>
		/// The via point(long/lat) for the route
		/// </summary>
		private SKViaPoint viaPoint;

		/// <summary>
		/// Text to speech engine
		/// </summary>
		private TextToSpeech textToSpeechEngine;

		private SKToolsNavigationManager navigationManager;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			DemoUtils.initializeLibrary(this);
			SetContentView(Resource.Layout.activity_map);

			app = (DemoApplication) Application;

			currentPositionProvider = new SKCurrentPositionProvider(this);
			currentPositionProvider.SetCurrentPositionListener(this);

			if (DemoUtils.hasGpsModule(this))
			{
				currentPositionProvider.RequestLocationUpdates(true, false, true);
			}
			else if (DemoUtils.hasNetworkModule(this))
			{
				currentPositionProvider.RequestLocationUpdates(false, true, true);
			}

			SKMapViewHolder mapViewGroup = (SKMapViewHolder) FindViewById(Resource.Id.view_group_map);
			mapView = mapViewGroup.MapSurfaceView;
            mapView.SetMapSurfaceListener(this);
			LayoutInflater inflater = (LayoutInflater) GetSystemService(Context.LayoutInflaterService);
			mapPopup = mapViewGroup.CalloutView;
			View view = inflater.Inflate(Resource.Layout.layout_popup, null);
			popupTitleView = (TextView) view.FindViewById(Resource.Id.top_text);
			popupDescriptionView = (TextView) view.FindViewById(Resource.Id.bottom_text);
			mapPopup.SetCustomView(view);


			applySettingsOnMapView();
			poiTrackingManager = new SKPOITrackerManager(this);

			menu = FindViewById(Resource.Id.options_menu);
			altRoutesView = FindViewById(Resource.Id.alt_routes);
			altRoutesButtons = new Button[]{(Button) FindViewById(Resource.Id.alt_route_1), (Button) FindViewById(Resource.Id.alt_route_2), (Button) FindViewById(Resource.Id.alt_route_3)};

			mapStylesView = (LinearLayout) FindViewById(Resource.Id.map_styles);
			bottomButton = (Button) FindViewById(Resource.Id.bottom_button);
			positionMeButton = (Button) FindViewById(Resource.Id.position_me_button);
			headingButton = (Button) FindViewById(Resource.Id.heading_button);

			pedestrianButton = (ImageButton) FindViewById(Resource.Id.real_reach_pedestrian_button);
			bikeButton = (ImageButton) FindViewById(Resource.Id.real_reach_bike_button);
			carButton = (ImageButton) FindViewById(Resource.Id.real_reach_car_button);

            SKVersioningManager.Instance.SetMapUpdateListener(this);

			TextView realReachTimeText = (TextView) FindViewById(Resource.Id.real_reach_time);
			TextView realReachEnergyText = (TextView) FindViewById(Resource.Id.real_reach_energy);

			SeekBar realReachSeekBar = (SeekBar) FindViewById(Resource.Id.real_reach_seekbar);
			realReachSeekBar.OnSeekBarChangeListener = new OnSeekBarChangeListenerAnonymousInnerClassHelper(this, realReachTimeText);
			SeekBar realReachEnergySeekBar = (SeekBar) FindViewById(Resource.Id.real_reach_energy_seekbar);
			realReachEnergySeekBar.OnSeekBarChangeListener = new OnSeekBarChangeListenerAnonymousInnerClassHelper2(this, realReachEnergyText);
			realReachTimeLayout = (RelativeLayout) FindViewById(Resource.Id.real_reach_time_layout);
			realReachEnergyLayout = (RelativeLayout) FindViewById(Resource.Id.real_reach_energy_layout);
			navigationUI = (RelativeLayout) FindViewById(Resource.Id.navigation_ui_layout);

			initializeTrackablePOIs();


		}

		private class OnSeekBarChangeListenerAnonymousInnerClassHelper : OnSeekBarChangeListener
		{
			private readonly MapActivity outerInstance;

			private TextView realReachTimeText;

			public OnSeekBarChangeListenerAnonymousInnerClassHelper(MapActivity outerInstance, TextView realReachTimeText)
			{
				this.outerInstance = outerInstance;
				this.realReachTimeText = realReachTimeText;
			}


			public override void onStartTrackingTouch(SeekBar arg0)
			{
			}

			public override void onStopTrackingTouch(SeekBar arg0)
			{
			}

			public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
			{

				outerInstance.realReachRange = progress;
				realReachTimeText.Text = outerInstance.realReachRange + " min";
				showRealReach(outerInstance.realReachVehicleType, outerInstance.realReachRange);

			}


		}

		private class OnSeekBarChangeListenerAnonymousInnerClassHelper2 : Android.Widget.SeekBar.IOnSeekBarChangeListener
		{
			private readonly MapActivity outerInstance;

			private TextView realReachEnergyText;

			public OnSeekBarChangeListenerAnonymousInnerClassHelper2(MapActivity outerInstance, TextView realReachEnergyText)
			{
				this.outerInstance = outerInstance;
				this.realReachEnergyText = realReachEnergyText;
			}


			public override void onStartTrackingTouch(SeekBar arg0)
			{
			}

			public override void onStopTrackingTouch(SeekBar arg0)
			{
			}

			public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
			{

				outerInstance.realReachRange = progress;
				realReachEnergyText.Text = outerInstance.realReachRange + "%";
				showRealReachEnergy(outerInstance.realReachRange);


			}


		}

		/// <summary>
		/// Customize the map view
		/// </summary>
		private void applySettingsOnMapView()
		{
			mapView.MapSettings.MapRotationEnabled = true;
			mapView.MapSettings.MapZoomingEnabled = true;
			mapView.MapSettings.MapPanningEnabled = true;
			mapView.MapSettings.ZoomWithAnchorEnabled = true;
			mapView.MapSettings.InertiaRotatingEnabled = true;
			mapView.MapSettings.InertiaZoomingEnabled = true;
			mapView.MapSettings.InertiaPanningEnabled = true;
		}

		private void initializeTrackablePOIs()
		/// <summary>
		/// Populate the collection of trackable POIs
		/// </summary>
		{

			trackablePOIs = new Dictionary<int?, SKTrackablePOI>();

			trackablePOIs[64142] = new SKTrackablePOI(64142, 0, 37.735610, -122.446434, -1, "Teresita Boulevard");
			trackablePOIs[64143] = new SKTrackablePOI(64143, 0, 37.732367, -122.442033, -1, "Congo Street");
			trackablePOIs[64144] = new SKTrackablePOI(64144, 0, 37.732237, -122.429190, -1, "John F Foran Freeway");
			trackablePOIs[64145] = new SKTrackablePOI(64145, 1, 37.738090, -122.401470, -1, "Revere Avenue");
			trackablePOIs[64146] = new SKTrackablePOI(64146, 0, 37.741128, -122.398562, -1, "McKinnon Ave");
			trackablePOIs[64147] = new SKTrackablePOI(64147, 1, 37.746154, -122.394077, -1, "Evans Ave");
			trackablePOIs[64148] = new SKTrackablePOI(64148, 0, 37.750057, -122.392287, -1, "Cesar Chavez Street");
			trackablePOIs[64149] = new SKTrackablePOI(64149, 1, 37.762823, -122.392957, -1, "18th Street");
			trackablePOIs[64150] = new SKTrackablePOI(64150, 0, 37.760242, -122.392495, 180, "20th Street");
			trackablePOIs[64151] = new SKTrackablePOI(64151, 0, 37.755157, -122.392196, 180, "23rd Street");

			trackablePOIs[64152] = new SKTrackablePOI(64152, 0, 37.773526, -122.452706, -1, "Shrader Street");
			trackablePOIs[64153] = new SKTrackablePOI(64153, 0, 37.786535, -122.444528, -1, "Pine Street");
			trackablePOIs[64154] = new SKTrackablePOI(64154, 1, 37.792242, -122.424426, -1, "Franklin Street");
			trackablePOIs[64155] = new SKTrackablePOI(64155, 0, 37.716146, -122.409480, -1, "Campbell Ave");
			trackablePOIs[64156] = new SKTrackablePOI(64156, 0, 37.719133, -122.388280, -1, "Fitzgerald Ave");

			drawnTrackablePOIs = new Dictionary<int?, SKTrackablePOI>();
		}

		protected internal override void onResume()
		{
			base.OnResume();
			mapView.OnResume();

			if (headingOn)
			{
				startOrientationSensor();
			}

			if (currentMapOption == MapOption.NAVI_UI)
			{
				ToggleButton selectStartPointBtn = (ToggleButton) FindViewById(Resource.Id.select_start_point_button);
				ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
				string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");
				if (prefNavigationType.Equals("0"))
				{ // real navi
					selectStartPointBtn.Visibility = ViewStates.Gone;
				}
				else if (prefNavigationType.Equals("1"))
				{
					selectStartPointBtn.Visibility = ViewStates.Visible;
				}
			}

			if (currentMapOption == MapOption.HEAT_MAP && heatMapCategories != null)
			{
				mapView.ShowHeatMapsWithPoiType(heatMapCategories);
			}
		}

		protected internal override void onPause()
		{
			base.OnPause();
			mapView.OnPause();
			if (headingOn)
			{
				stopOrientationSensor();
			}
		}

		protected internal override void onDestroy()
		{
			base.OnDestroy();
			currentPositionProvider.StopLocationUpdates();
			SKMaps.Instance.DestroySKMaps();
			if (textToSpeechEngine != null)
			{
				textToSpeechEngine.Stop();
				textToSpeechEngine.Shutdown();
			}
			Process.KillProcess(Process.MyPid());
		}

		public override void onSurfaceCreated()
		{
			View chessBackground = FindViewById(Resource.Id.chess_board_background);
			chessBackground.Visibility = ViewStates.Gone;
			mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;

		}

		public override void onBoundingBoxImageRendered(int i)
		{

		}


		public override void onGLInitializationError(string messsage)
		{

		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                switch (requestCode)
                {
                    case TRACKS:
                        if (currentMapOption.Equals(MapOption.TRACKS) && TrackElementsActivity.selectedTrackElement != null)
                        {
                            mapView.DrawTrackElement(TrackElementsActivity.selectedTrackElement);
                            mapView.FitTrackElementInView(TrackElementsActivity.selectedTrackElement, false);

                            SKRouteManager.Instance.SetRouteListener(this);
                            SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.selectedTrackElement, SKRouteSettings.SKRouteMode.BicycleFastest, true, true, false);
                        }
                        break;

                    default:
                        break;
                }
            }

        }

        public override bool onKeyDown(Keycode keyCode, KeyEvent @event)
		{
			if (keyCode == Keycode.Menu && !skToolsNavigationInProgress && !skToolsRouteCalculated)
			{
                if (keyCode == Keycode.Menu)
				{
					if (menu.Visibility == ViewStates.Visible)
					{
						menu.Visibility = ViewStates.Gone;
					}
					else if (menu.Visibility == ViewStates.Gone)
					{
						menu.Visibility = ViewStates.Visible;
						menu.BringToFront();
					}
				}
				return true;
			}
			else
			{
				return base.OnKeyDown(keyCode, @event);
			}
		}

        [Export("OnClick")]
		public virtual void onClick(View v)
		{

			switch (v.Id)
			{

				case Resource.Id.alt_route_1:
					selectAlternativeRoute(0);
					break;
				case Resource.Id.alt_route_2:
					selectAlternativeRoute(1);
					break;
				case Resource.Id.alt_route_3:
					selectAlternativeRoute(2);
					break;
				case Resource.Id.map_style_day:
					selectMapStyle(new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json"));
					break;
				case Resource.Id.map_style_night:
					selectMapStyle(new SKMapViewStyle(app.MapResourcesDirPath + "nightstyle/", "nightstyle.json"));
					break;
				case Resource.Id.map_style_outdoor:
					selectMapStyle(new SKMapViewStyle(app.MapResourcesDirPath + "outdoorstyle/", "outdoorstyle.json"));
					break;
				case Resource.Id.map_style_grayscale:
					selectMapStyle(new SKMapViewStyle(app.MapResourcesDirPath + "grayscalestyle/", "grayscalestyle.json"));
					break;
				case Resource.Id.bottom_button:
					if (currentMapOption == MapOption.ROUTING_AND_NAVIGATION || currentMapOption == MapOption.TRACKS)
					{
						if (bottomButton.Text.Equals(Resources.GetString(Resource.String.calculate_route)))
						{
							launchRouteCalculation();
						}
						else if (bottomButton.Text.Equals(Resources.GetString(Resource.String.start_navigation)))
						{
							(new AlertDialog.Builder(this)).SetMessage("Choose the advice type").SetCancelable(false).SetPositiveButton("Scout audio", new OnClickListenerAnonymousInnerClassHelper(this))
								   .SetNegativeButton("Text to speech", new OnClickListenerAnonymousInnerClassHelper2(this))
								   .Show();
							bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
						}
						else if (bottomButton.Text.Equals(Resources.GetString(Resource.String.stop_navigation)))
						{
							stopNavigation();
							bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
						}
					}
					break;
				case Resource.Id.position_me_button:
					if (headingOn)
					{
						Heading = false;
					}
					if (currentPosition != null)
					{
						mapView.CenterMapOnCurrentPositionSmooth(17, 500);
					}
					else
					{
						Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
					}
					break;
				case Resource.Id.heading_button:
					if (currentPosition != null)
					{
						Heading = true;
					}
					else
					{
						Toast.MakeText(this, Resources.GetString(Resource.String.no_position_available), ToastLength.Short).Show();
					}
					break;
				case Resource.Id.real_reach_pedestrian_button:
					realReachVehicleType = SKRealReachSettings.VehicleTypePedestrian;
					showRealReach(realReachVehicleType, realReachRange);
					pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
					bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					break;
				case Resource.Id.real_reach_bike_button:
					realReachVehicleType = SKRealReachSettings.VehicleTypeBicycle;
					showRealReach(realReachVehicleType, realReachRange);
					bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
					pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					break;
				case Resource.Id.real_reach_car_button:
					realReachVehicleType = SKRealReachSettings.VehicleTypeCar;
					showRealReach(realReachVehicleType, realReachRange);
					carButton.SetBackgroundColor(Resources.GetColor(Resource.Color.blue_filling));
					pedestrianButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					bikeButton.SetBackgroundColor(Resources.GetColor(Resource.Color.grey));
					break;
				case Resource.Id.exit_real_reach_time:
					realReachTimeLayout.Visibility = ViewStates.Gone;
					clearMap();
					break;
				case Resource.Id.exit_real_reach_energy:
					realReachEnergyLayout.Visibility = ViewStates.Gone;
					clearMap();
					break;
				case Resource.Id.navigation_ui_back_button:
					Button backButton = (Button) FindViewById(Resource.Id.navigation_ui_back_button);
					LinearLayout naviButtons = (LinearLayout) FindViewById(Resource.Id.navigation_ui_buttons);
					if (backButton.Text.Equals(">"))
					{
						naviButtons.Visibility = ViewStates.Visible;
						backButton.Text = "<";
					}
					else
					{
						naviButtons.Visibility = ViewStates.Gone;
						backButton.Text = ">";
					}
					break;
				case Resource.Id.calculate_routes_button:
					calculateRouteFromSKTools();
					break;

				case Resource.Id.settings_button:
					Intent intent = new Intent(this, typeof(SettingsActivity));
					StartActivity(intent);
					break;
				case Resource.Id.start_free_drive_button:
					startFreeDriveFromSKTools();
					break;
				case Resource.Id.clear_via_point_button:
					viaPoint = null;
					mapView.DeleteAnnotation(VIA_POINT_ICON_ID);
					FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Gone;
					break;
				case Resource.Id.position_me_navigation_ui_button:
					if (currentPosition != null)
					{
						mapView.CenterMapOnCurrentPositionSmooth(15, 1000);
						mapView.MapSettings.OrientationIndicatorType = SKMapSurfaceView.SKOrientationIndicatorType.Default;
						mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
					}
					else
					{
						Toast.MakeText(this, GetString(Resource.String.no_position_available), ToastLength.Long).Show();
					}
					break;
				default:
					break;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : IDialogInterfaceOnClickListener
		{
			private readonly MapActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MapActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(DialogInterface dialog, int id)
			{
				outerInstance.bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
				AdvicesAndStartNavigation = MapAdvices.AUDIO_FILES;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : IDialogInterfaceOnClickListener
		{
			private readonly MapActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(MapActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(DialogInterface dialog, int id)
			{
				if (outerInstance.textToSpeechEngine == null)
				{
					Toast.MakeText(outerInstance, "Initializing TTS engine", Toast.LENGTH_LONG).Show();
					outerInstance.textToSpeechEngine = new TextToSpeech(outerInstance, new OnInitListenerAnonymousInnerClassHelper(this));
				}
				else
				{
					outerInstance.bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
					AdvicesAndStartNavigation = MapAdvices.TEXT_TO_SPEECH;
				}

			}

			private class OnInitListenerAnonymousInnerClassHelper : TextToSpeech.OnInitListener
			{
				private readonly OnClickListenerAnonymousInnerClassHelper2 outerInstance;

				public OnInitListenerAnonymousInnerClassHelper(OnClickListenerAnonymousInnerClassHelper2 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void onInit(int status)
				{
					if (status == TextToSpeech.SUCCESS)
					{
						int result = outerInstance.outerInstance.textToSpeechEngine.setLanguage(Locale.ENGLISH);
						if (result == TextToSpeech.LANG_MISSING_DATA || result == TextToSpeech.LANG_NOT_SUPPORTED)
						{
							Toast.MakeText(outerInstance.outerInstance, "This Language is not supported", Toast.LENGTH_LONG).Show();
						}
					}
					outerInstance.outerInstance.bottomButton.Text = Resources.GetString(Resource.String.stop_navigation);
					AdvicesAndStartNavigation = MapAdvices.TEXT_TO_SPEECH;
				}
			}
		}

		private void startFreeDriveFromSKTools()
		{
			SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();

			ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
			string prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.K_DISTANCE_UNIT, "0");
			if (prefDistanceFormat.Equals("0"))
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
			}
			else if (prefDistanceFormat.Equals("1"))
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet;
			}
			else
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesYards;
			}

			//set speed in town
			string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.K_IN_TOWN_SPEED_WARNING, "0");
			if (prefSpeedInTown.Equals("0"))
			{
				configuration.SpeedWarningThresholdInCity = 5.0;
			}
			else if (prefSpeedInTown.Equals("1"))
			{
				configuration.SpeedWarningThresholdInCity = 10.0;
			}
			else if (prefSpeedInTown.Equals("2"))
			{
				configuration.SpeedWarningThresholdInCity = 15.0;
			}
			else if (prefSpeedInTown.Equals("3"))
			{
				configuration.SpeedWarningThresholdInCity = 20.0;
			}
			//set speed out
			string prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.K_OUT_TOWN_SPEED_WARNING, "0");
			if (prefSpeedOutTown.Equals("0"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 5.0;
			}
			else if (prefSpeedOutTown.Equals("1"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 10.0;
			}
			else if (prefSpeedOutTown.Equals("2"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 15.0;
			}
			else if (prefSpeedOutTown.Equals("3"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 20.0;
			}
			bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.K_AUTO_DAY_NIGHT, true);
			if (!dayNight)
			{
				configuration.AutomaticDayNight = false;
			}
			configuration.NavigationType = SKNavigationSettings.SKNavigationType.File;
			configuration.FreeDriveNavigationFilePath = app.MapResourcesDirPath + "logFile/Seattle.log";
			configuration.DayStyle = new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json");
			configuration.NightStyle = new SKMapViewStyle(app.MapResourcesDirPath + "nightstyle/", "nightstyle.json");

			navigationUI.Visibility = ViewStates.Gone;
			navigationManager = new SKToolsNavigationManager(this, Resource.Id.map_layout_root);
			navigationManager.NavigationListener = this;
			navigationManager.startFreeDriveWithConfiguration(configuration, mapView);

		}

		private void calculateRouteFromSKTools()
		{

			SKToolsNavigationConfiguration configuration = new SKToolsNavigationConfiguration();
			ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);

			//set navigation type
			string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");
			if (prefNavigationType.Equals("0"))
			{
				configuration.NavigationType = SKNavigationSettings.SKNavigationType.Real;
				if (currentPosition == null)
				{
					showNoCurrentPosDialog();
					return;
				}
				startPoint = new SKCoordinate(currentPosition.Longitude, currentPosition.Latitude);
			}
			else if (prefNavigationType.Equals("1"))
			{
				configuration.NavigationType = SKNavigationSettings.SKNavigationType.Simulation;

			}

			//set route type
			string prefRouteType = sharedPreferences.GetString(PreferenceTypes.K_ROUTE_TYPE, "2");
			if (prefRouteType.Equals("0"))
			{
				configuration.RouteType = SKRouteSettings.SKRouteMode.CarShortest;
			}
			else if (prefRouteType.Equals("1"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.CarFastest;
			}
			else if (prefRouteType.Equals("2"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.Efficient;
			}
			else if (prefRouteType.Equals("3"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.Pedestrian;
			}
			else if (prefRouteType.Equals("4"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleFastest;
			}
			else if (prefRouteType.Equals("5"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleShortest;
			}
			else if (prefRouteType.Equals("6"))
			{
                configuration.RouteType = SKRouteSettings.SKRouteMode.BicycleQuietest;
			}

			//set distance format
			string prefDistanceFormat = sharedPreferences.GetString(PreferenceTypes.K_DISTANCE_UNIT, "0");
			if (prefDistanceFormat.Equals("0"))
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitKilometerMeters;
			}
			else if (prefDistanceFormat.Equals("1"))
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesFeet;
			}
			else
			{
				configuration.DistanceUnitType = SKMaps.SKDistanceUnitType.DistanceUnitMilesYards;
			}

			//set speed in town
			string prefSpeedInTown = sharedPreferences.GetString(PreferenceTypes.K_IN_TOWN_SPEED_WARNING, "0");
			if (prefSpeedInTown.Equals("0"))
			{
				configuration.SpeedWarningThresholdInCity = 5.0;
			}
			else if (prefSpeedInTown.Equals("1"))
			{
				configuration.SpeedWarningThresholdInCity = 10.0;
			}
			else if (prefSpeedInTown.Equals("2"))
			{
				configuration.SpeedWarningThresholdInCity = 15.0;
			}
			else if (prefSpeedInTown.Equals("3"))
			{
				configuration.SpeedWarningThresholdInCity = 20.0;
			}

			//set speed out
			string prefSpeedOutTown = sharedPreferences.GetString(PreferenceTypes.K_OUT_TOWN_SPEED_WARNING, "0");
			if (prefSpeedOutTown.Equals("0"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 5.0;
			}
			else if (prefSpeedOutTown.Equals("1"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 10.0;
			}
			else if (prefSpeedOutTown.Equals("2"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 15.0;
			}
			else if (prefSpeedOutTown.Equals("3"))
			{
				configuration.SpeedWarningThresholdOutsideCity = 20.0;
			}
			bool dayNight = sharedPreferences.GetBoolean(PreferenceTypes.K_AUTO_DAY_NIGHT, true);
			if (!dayNight)
			{
				configuration.AutomaticDayNight = false;
			}
            bool tollRoads = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_TOLL_ROADS, false);
			if (tollRoads)
			{
				configuration.TollRoadsAvoided = true;
			}
            bool avoidFerries = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_FERRIES, false);
			if (avoidFerries)
			{
				configuration.FerriesAvoided = true;
			}
            bool highWays = sharedPreferences.GetBoolean(PreferenceTypes.K_AVOID_HIGHWAYS, false);
			if (highWays)
			{
				configuration.HighWaysAvoided = true;
			}
            bool freeDrive = sharedPreferences.GetBoolean(PreferenceTypes.K_FREE_DRIVE, true);
			if (!freeDrive)
			{
				configuration.ContinueFreeDriveAfterNavigationEnd = false;
			}

			navigationUI.Visibility = ViewStates.Gone;
			configuration.StartCoordinate = startPoint;
			configuration.DestinationCoordinate = destinationPoint;
			IList<SKViaPoint> viaPointList = new List<SKViaPoint>();
			if (viaPoint != null)
			{
				viaPointList.Add(viaPoint);
				configuration.ViaPointCoordinateList = viaPointList;
			}
			configuration.DayStyle = new SKMapViewStyle(app.MapResourcesDirPath + "daystyle/", "daystyle.json");
			configuration.NightStyle = new SKMapViewStyle(app.MapResourcesDirPath + "nightstyle/", "nightstyle.json");
			navigationManager = new SKToolsNavigationManager(this, Resource.Id.map_layout_root);
			navigationManager.NavigationListener = this;

			if (configuration.StartCoordinate != null && configuration.DestinationCoordinate != null)
			{
				navigationManager.launchRouteCalculation(configuration, mapView);
			}


		}


		protected internal override void onPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
		}

        [Export("OnMenuOptionClick")]
		public virtual void onMenuOptionClick(View v)
		{
			clearMap();
			switch (v.Id)
			{
				case Resource.Id.option_map_display:
					mapView.ClearHeatMapsDisplay();
					currentMapOption = MapOption.MAP_DISPLAY;
					bottomButton.Visibility = ViewStates.Gone;
					SKRouteManager.Instance.ClearCurrentRoute();
					break;
				case Resource.Id.option_overlays:
					currentMapOption = MapOption.MAP_OVERLAYS;
					drawShapes();
					mapView.SetZoom(14);
					mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
					break;
				case Resource.Id.option_alt_routes:
					currentMapOption = MapOption.ALTERNATIVE_ROUTES;
					altRoutesView.Visibility = ViewStates.Visible;
					launchAlternativeRouteCalculation();
					break;
				case Resource.Id.option_map_styles:
					currentMapOption = MapOption.MAP_STYLES;
					mapStylesView.Visibility = ViewStates.Visible;
					selectStyleButton();
					break;
				case Resource.Id.option_map_creator:
					currentMapOption = MapOption.MAP_DISPLAY;
					mapView.ApplySettingsFromFile(app.MapCreatorFilePath);
					break;
				case Resource.Id.option_tracks:
					currentMapOption = MapOption.TRACKS;
					Intent intent = new Intent(this, typeof(TracksActivity));
					StartActivityForResult(intent, TRACKS);
					break;
				case Resource.Id.option_real_reach:
					(new AlertDialog.Builder(this)).SetMessage("Choose the real reach type").SetCancelable(false).SetPositiveButton("Time profile", new OnClickListenerAnonymousInnerClassHelper3(this))
						   .SetNegativeButton("Energy profile", new OnClickListenerAnonymousInnerClassHelper4(this))
						   .Show();
					break;
				case Resource.Id.option_map_xml_and_downloads:
					if (DemoUtils.isInternetAvailable(this))
					{
						StartActivity(new Intent(this, typeof(ResourceDownloadsListActivity)));
					}
					else
					{
						Toast.MakeText(this, Resources.GetString(Resource.String.no_internet_connection), ToastLength.Short).Show();
					}
					break;
				case Resource.Id.option_reverse_geocoding:
					StartActivity(new Intent(this, typeof(ReverseGeocodingActivity)));
					break;
				case Resource.Id.option_address_search:
					StartActivity(new Intent(this, typeof(OfflineAddressSearchActivity)));
					break;
				case Resource.Id.option_nearby_search:
					StartActivity(new Intent(this, typeof(NearbySearchActivity)));
					break;
				case Resource.Id.option_annotations:
					currentMapOption = MapOption.ANNOTATIONS;
					prepareAnnotations();
					break;
				case Resource.Id.option_category_search:
					StartActivity(new Intent(this, typeof(CategorySearchResultsActivity)));
					break;
				case Resource.Id.option_routing_and_navigation:
					currentMapOption = MapOption.ROUTING_AND_NAVIGATION;
					bottomButton.Visibility = ViewStates.Visible;
					bottomButton.Text = Resources.GetString(Resource.String.calculate_route);
					break;
				case Resource.Id.option_poi_tracking:
					currentMapOption = MapOption.POI_TRACKING;
					if (trackablePOIs == null)
					{
						initializeTrackablePOIs();
					}
					launchRouteCalculation();
					break;
				case Resource.Id.option_heat_map:
					currentMapOption = MapOption.HEAT_MAP;
					StartActivity(new Intent(this, typeof(POICategoriesListActivity)));
					break;
				case Resource.Id.option_map_updates:
					SKVersioningManager.Instance.CheckNewVersion(3);
					break;
				case Resource.Id.option_map_interaction:
					currentMapOption = MapOption.MAP_INTERACTION;
					handleMapInteractionOption();
					break;
				case Resource.Id.option_navigation_ui:
					currentMapOption = MapOption.NAVI_UI;
					initializeNavigationUI(true);
					break;
				default:
					break;
			}
			if (currentMapOption != MapOption.MAP_DISPLAY)
			{
				positionMeButton.Visibility = ViewStates.Gone;
				headingButton.Visibility = ViewStates.Gone;
			}
			menu.Visibility = ViewStates.Gone;
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : IDialogInterfaceOnClickListener
		{
			private readonly MapActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(MapActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(DialogInterface dialog, int id)
			{
				outerInstance.currentMapOption = MapOption.REAL_REACH;
				outerInstance.realReachTimeLayout.Visibility = ViewStates.Visible;
				showRealReach(outerInstance.realReachVehicleType, outerInstance.realReachRange);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : IDialogInterfaceOnClickListener
		{
			private readonly MapActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(MapActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(DialogInterface dialog, int id)
			{
				outerInstance.currentMapOption = MapOption.REAL_REACH;
				outerInstance.realReachEnergyLayout.Visibility = ViewStates.Visible;
				showRealReachEnergy(outerInstance.realReachRange);

			}
		}

        private void initializeNavigationUI(bool showStartingAndDestinationAnnotations)
	{
		ToggleButton selectViaPointBtn = (ToggleButton) FindViewById(Resource.Id.select_via_point_button);
		ToggleButton selectStartPointBtn = (ToggleButton) FindViewById(Resource.Id.select_start_point_button);
		ToggleButton selectEndPointBtn = (ToggleButton) FindViewById(Resource.Id.select_end_point_button);

		ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
		string prefNavigationType = sharedPreferences.GetString(PreferenceTypes.K_NAVIGATION_TYPE, "1");
		if (prefNavigationType.Equals("0"))
		{ // real navi
			selectStartPointBtn.Visibility = ViewStates.Gone;
		}
		else if (prefNavigationType.Equals("1"))
		{
			selectStartPointBtn.Visibility = ViewStates.Visible;
		}

		if (showStartingAndDestinationAnnotations)
		{
			startPoint = new SKCoordinate(13.34615707397461, 52.513086884218325);
			SKAnnotation annotation = new SKAnnotation(GREEN_PIN_ICON_ID);
			annotation.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
			annotation.Location = startPoint;
			mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

			destinationPoint = new SKCoordinate(13.398685455322266, 52.50995268098114);
			annotation = new SKAnnotation(RED_PIN_ICON_ID);
			annotation.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
			annotation.Location = destinationPoint;
			mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);

		}
        mapView.SetZoom(11);
		mapView.CenterMapOnPosition(startPoint);


		selectStartPointBtn.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this, selectViaPointBtn, selectEndPointBtn);
		selectEndPointBtn.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper2(this, selectViaPointBtn, selectStartPointBtn);

		selectViaPointBtn.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper3(this, selectStartPointBtn, selectEndPointBtn);

		navigationUI.Visibility = ViewStates.Visible;
	}

	private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
	{
		private readonly MapActivity outerInstance;

		private ToggleButton selectViaPointBtn;
		private ToggleButton selectEndPointBtn;

		public OnCheckedChangeListenerAnonymousInnerClassHelper(MapActivity outerInstance, ToggleButton selectViaPointBtn, ToggleButton selectEndPointBtn)
		{
			this.outerInstance = outerInstance;
			this.selectViaPointBtn = selectViaPointBtn;
			this.selectEndPointBtn = selectEndPointBtn;
		}

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			if (isChecked)
			{
				isStartPointBtnPressed = true;
				isEndPointBtnPressed = false;
				isViaPointSelected = false;
				selectEndPointBtn.Checked = false;
				selectViaPointBtn.Checked = false;
				Toast.MakeText(outerInstance, GetString(Resource.String.long_tap_for_position), Toast.LENGTH_LONG).Show();
			}
			else
			{
				isStartPointBtnPressed = false;
			}
		}
	}

	private class OnCheckedChangeListenerAnonymousInnerClassHelper2 : CompoundButton.OnCheckedChangeListener
	{
		private readonly MapActivity outerInstance;

		private ToggleButton selectViaPointBtn;
		private ToggleButton selectStartPointBtn;

		public OnCheckedChangeListenerAnonymousInnerClassHelper2(MapActivity outerInstance, ToggleButton selectViaPointBtn, ToggleButton selectStartPointBtn)
		{
			this.outerInstance = outerInstance;
			this.selectViaPointBtn = selectViaPointBtn;
			this.selectStartPointBtn = selectStartPointBtn;
		}

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			if (isChecked)
			{
				isEndPointBtnPressed = true;
				isStartPointBtnPressed = false;
				isViaPointSelected = false;
				selectStartPointBtn.Checked = false;
				selectViaPointBtn.Checked = false;
				Toast.MakeText(outerInstance, GetString(Resource.String.long_tap_for_position), Toast.LENGTH_LONG).Show();
			}
			else
			{
				isEndPointBtnPressed = false;
			}
		}
	}

	private class OnCheckedChangeListenerAnonymousInnerClassHelper3 : CompoundButton.OnCheckedChangeListener
	{
		private readonly MapActivity outerInstance;

		private ToggleButton selectStartPointBtn;
		private ToggleButton selectEndPointBtn;

		public OnCheckedChangeListenerAnonymousInnerClassHelper3(MapActivity outerInstance, ToggleButton selectStartPointBtn, ToggleButton selectEndPointBtn)
		{
			this.outerInstance = outerInstance;
			this.selectStartPointBtn = selectStartPointBtn;
			this.selectEndPointBtn = selectEndPointBtn;
		}

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			if (isChecked)
			{
				isViaPointSelected = true;
				isStartPointBtnPressed = false;
				isEndPointBtnPressed = false;
				selectStartPointBtn.Checked = false;
				selectEndPointBtn.Checked = false;
				Toast.MakeText(outerInstance, GetString(Resource.String.long_tap_for_position), Toast.LENGTH_LONG).Show();
			}
			else
			{
				isViaPointSelected = false;
			}
		}
	}

	private void showNoCurrentPosDialog()
	{
		AlertDialog.Builder alert = new AlertDialog.Builder(this);
        //alert.setTitle("Really quit?");
		alert.SetMessage("There is no current position available");
		alert.SetNegativeButton("Ok", new OnClickListenerAnonymousInnerClassHelper(this));
		alert.Show();
	}

	private class OnClickListenerAnonymousInnerClassHelper : IDialogInterfaceOnClickListener
	{
		private readonly MapActivity outerInstance;

		public OnClickListenerAnonymousInnerClassHelper(MapActivity outerInstance)
		{
			this.outerInstance = outerInstance;
		}

		public virtual void onClick(DialogInterface dialog, int id)
		{
		}
	}

	private void handleMapInteractionOption()
	{

		mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));

		// get the annotation object
		SKAnnotation annotation1 = new SKAnnotation(10);
		// set annotation location
		annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
		// set minimum zoom level at which the annotation should be visible
		annotation1.MininumZoomLevel = 5;
		// set the annotation's type
		annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
		// render annotation on map
		mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);

		SKAnnotation annotation2 = new SKAnnotation(11);
		annotation2.Location = new SKCoordinate(-122.419789, 37.775428);
		annotation2.MininumZoomLevel = 5;
		annotation2.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
		mapView.AddAnnotation(annotation2, SKAnimationSettings.AnimationNone);

		float density = Resources.DisplayMetrics.Density;

		TextView topText = (TextView) mapPopup.FindViewById(Resource.Id.top_text);
		topText.Text = "Get details";
		topText.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		mapPopup.FindViewById(Resource.Id.bottom_text).Visibility = ViewStates.Gone;

		mapPopup.SetVerticalOffset(30 * density);
		mapPopup.ShowAtLocation(annotation1.Location, true);

	}

	private class OnClickListenerAnonymousInnerClassHelper : OnClickListener
	{
		private readonly MapActivity outerInstance;

		public OnClickListenerAnonymousInnerClassHelper(MapActivity outerInstance)
		{
			this.outerInstance = outerInstance;
		}


		public override void onClick(View v)
		{
			StartActivity(new Intent(outerInstance, typeof(InteractionMapActivity)));
		}
	}

	/// <summary>
	/// Launches a single route calculation
	/// </summary>
	private void launchRouteCalculation()
	{
		// get a route object and populate it with the desired properties
		SKRouteSettings route = new SKRouteSettings();
		// set start and destination points
		route.StartCoordinate = new SKCoordinate(-122.397674, 37.761278);
		route.DestinationCoordinate = new SKCoordinate(-122.448270, 37.738761);
		// set the number of routes to be calculated
		route.NoOfRoutes = 1;
		// set the route mode
		route.RouteMode = SKRouteSettings.SKRouteMode.CarFastest;
		// set whether the route should be shown on the map after it's computed
		route.RouteExposed = true;
		// set the route listener to be notified of route calculation
		// events
		SKRouteManager.Instance.SetRouteListener(this);
		// pass the route to the calculation routine
		SKRouteManager.Instance.CalculateRoute(route);
	}

	/// <summary>
	/// Launches the calculation of three alternative routes
	/// </summary>
	private void launchAlternativeRouteCalculation()
	{
		SKRouteSettings route = new SKRouteSettings();
		route.StartCoordinate = new SKCoordinate(-122.392284, 37.787189);
		route.DestinationCoordinate = new SKCoordinate(-122.484378, 37.856300);
		// number of alternative routes specified here
		route.NoOfRoutes = 3;
		route.RouteMode = SKRouteSettings.SKRouteMode.CarFastest;
		route.RouteExposed = true;
		SKRouteManager.Instance.SetRouteListener(this);
		SKRouteManager.Instance.CalculateRoute(route);
	}

	/// <summary>
	/// Initiate real reach time profile
	/// </summary>
	private void showRealReach(sbyte vehicleType, int range)
	{

		// set listener for real reach calculation events
		mapView.SetRealReachListener(this);
		// get object that can be used to specify real reach calculation
		// properties
		SKRealReachSettings realReachSettings = new SKRealReachSettings();
		// set center position for real reach
		SKCoordinate realReachCenter = new SKCoordinate(23.593957, 46.773361);
		realReachSettings.Location = realReachCenter;
		// set measurement unit for real reach
		realReachSettings.MeasurementUnit = SKRealReachSettings.UnitSecond;
		// set the range value (in the unit previously specified)
		realReachSettings.Range = range * 60;
		// set the transport mode
		realReachSettings.TransportMode = vehicleType;
		// initiate real reach
		mapView.DisplayRealReachWithSettings(realReachSettings);
	}

	/// <summary>
	/// The cunsumption values
	/// </summary>
	private float[] energyConsumption = new float[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (float) 3.7395504, (float) 4.4476889, (float) 5.4306439, (float) 6.722719, (float) 8.2830299, (float) 10.0275093, (float) 11.8820908, (float) 13.799201, (float) 15.751434, (float) 17.7231534, (float) 19.7051378, (float) 21.6916725, (float) 23.679014, (float) 25.6645696, (float) 27.6464437, (float) 29.6231796, (float) 31.5936073};

	/// <summary>
	/// Initiate real reach energy profile
	/// </summary>

	private void showRealReachEnergy(int range)
	{

		//set listener for real reach calculation events
		mapView.SetRealReachListener(this);
		// get object that can be used to specify real reach calculation
		// properties
		SKRealReachSettings realReachSettings = new SKRealReachSettings();
		SKCoordinate realReachCenter = new SKCoordinate(23.593957, 46.773361);
		realReachSettings.Location = realReachCenter;
		// set measurement unit for real reach
		realReachSettings.MeasurementUnit = SKRealReachSettings.UnitMiliwattHours;
		// set consumption values
		realReachSettings.SetConsumption(energyConsumption);
		// set the range value (in the unit previously specified)
		realReachSettings.Range = range * 100;
		// set the transport mode
		realReachSettings.TransportMode = SKRealReachSettings.VehicleTypeBicycle;
		// initiate real reach
		mapView.DisplayRealReachWithSettings(realReachSettings);

	}

	/// <summary>
	/// Draws annotations on map
	/// </summary>
	private void prepareAnnotations()
	{

		// Add annotation using texture ID - from the json files.
		// get the annotation object
		SKAnnotation annotation1 = new SKAnnotation(10);
		// set annotation location
		annotation1.Location = new SKCoordinate(-122.4200, 37.7765);
		// set minimum zoom level at which the annotation should be visible
		annotation1.MininumZoomLevel = 5;
		// set the annotation's type
		annotation1.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
		// render annotation on map
		mapView.AddAnnotation(annotation1, SKAnimationSettings.AnimationNone);


		// Add an annotation using the absolute path to the image.
		SKAnnotation annotation = new SKAnnotation(13);
		annotation.Location = new SKCoordinate(-122.434516, 37.770712);
		annotation.MininumZoomLevel = 5;


		DisplayMetrics metrics = new DisplayMetrics();
		WindowManager.DefaultDisplay.GetMetrics(metrics);
		if (metrics.DensityDpi < DisplayMetrics.DensityHigh)
		{
			annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@2x.png";
			// set the size of the image in pixels
			annotation.ImageSize = 128;
		}
		else
		{
			annotation.ImagePath = SKMaps.Instance.MapInitSettings.MapResourcesPath + "/.Common/icon_bluepin@3x.png";
			// set the size of the image in pixels
			annotation.ImageSize = 256;

		}
		// by default the center of the image corresponds with the location .annotation.setOffset can be use to position the image around the location.
		mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);


		// add an annotation with a drawable resource
		SKAnnotation annotationDrawable = new SKAnnotation(14);
		annotationDrawable.Location = new SKCoordinate(-122.437182, 37.777079);
		annotationDrawable.MininumZoomLevel = 5;


		SKAnnotationView annotationView = new SKAnnotationView();
		annotationView.DrawableResourceId = Resource.Drawable.icon_map_popup_navigate;
		// set the width and height of the image in pixels . If they are not power of 2 the actual size of the image will be the next power of 2 of max(width,height)
		annotationView.Width = 128;
		annotationView.Height = 128;
		annotationDrawable.AnnotationView = annotationView;
		mapView.AddAnnotation(annotationDrawable, SKAnimationSettings.AnimationNone);


		// // add an annotation with a view
		SKAnnotation annotationFromView = new SKAnnotation(15);
		annotationFromView.Location = new SKCoordinate(-122.423573, 37.761349);
		annotationFromView.MininumZoomLevel = 5;
		annotationView = new SKAnnotationView();
		customView = (RelativeLayout)((LayoutInflater) GetSystemService(Context.LayoutInflaterService)).inflate(Resource.Layout.layout_custom_view, null, false);
		//  If width and height of the view  are not power of 2 the actual size of the image will be the next power of 2 of max(width,height).
		annotationView.View = customView;
		annotationFromView.AnnotationView = annotationView;
		mapView.AddAnnotation(annotationFromView, SKAnimationSettings.AnimationNone);

		// set map zoom level
		mapView.SetZoom(13);
		// center map on a position
		mapView.CenterMapOnPosition(new SKCoordinate(-122.4200, 37.7765));
	}

	/// <summary>
	/// Draws shapes on map
	/// </summary>
	private void drawShapes()
	{

		// get a polygon shape object
		SKPolygon polygon = new SKPolygon();
		polygon.Identifier = 1;
		// set the polygon's nodes
		IList<SKCoordinate> nodes = new List<SKCoordinate>();
		nodes.Add(new SKCoordinate(-122.4342, 37.7765));
		nodes.Add(new SKCoordinate(-122.4141, 37.7765));
		nodes.Add(new SKCoordinate(-122.4342, 37.7620));
		polygon.Nodes = nodes;
		// set the outline size
		polygon.OutlineSize = 3;
		// set colors used to render the polygon
		polygon.SetOutlineColor(new float[]{1f, 0f, 0f, 1f});
		polygon.SetColor(new float[]{1f, 0f, 0f, 0.2f});
		// render the polygon on the map
		mapView.AddPolygon(polygon);

		// get a circle mask shape object
		SKCircle circleMask = new SKCircle();
		circleMask.Identifier = 2;
		// set the shape's mask scale
		circleMask.MaskedObjectScale = 1.3f;
		// set the colors
		circleMask.SetColor(new float[]{1f, 1f, 0.5f, 0.67f});
		circleMask.SetOutlineColor(new float[]{0f, 0f, 0f, 1f});
		circleMask.OutlineSize = 3;
		// set circle center and radius
		circleMask.CircleCenter = new SKCoordinate(-122.4200, 37.7665);
		circleMask.Radius = 300;
		// set outline properties
		circleMask.OutlineDottedPixelsSkip = 6;
		circleMask.OutlineDottedPixelsSolid = 10;
		// set the number of points for rendering the circle
		circleMask.NumberOfPoints = 150;
		// render the circle mask
		mapView.AddCircle(circleMask);


		// get a polyline object
		SKPolyline polyline = new SKPolyline();
		polyline.Identifier = 3;
		// set the nodes on the polyline
		nodes = new List<SKCoordinate>();
		nodes.Add(new SKCoordinate(-122.4342, 37.7898));
		nodes.Add(new SKCoordinate(-122.4141, 37.7898));
		nodes.Add(new SKCoordinate(-122.4342, 37.7753));
		polyline.Nodes = nodes;
		// set polyline color
		polyline.SetColor(new float[]{0f, 0f, 1f, 1f});
		// set properties for the outline
		polyline.SetOutlineColor(new float[]{0f, 0f, 1f, 1f});
		polyline.OutlineSize = 4;
		polyline.OutlineDottedPixelsSolid = 3;
		polyline.OutlineDottedPixelsSkip = 3;
		mapView.AddPolyline(polyline);
	}

	private void selectMapStyle(SKMapViewStyle newStyle)
	{
		mapView.MapSettings.MapStyle = newStyle;
		selectStyleButton();
	}

	/// <summary>
	/// Selects the style button for the current map style
	/// </summary>
	private void selectStyleButton()
	{
		for (int i = 0; i < mapStylesView.ChildCount; i++)
		{
			mapStylesView.GetChildAt(i).Selected = false;
		}
		SKMapViewStyle mapStyle = mapView.MapSettings.MapStyle;
		if (mapStyle == null || mapStyle.StyleFileName.Equals("daystyle.json"))
		{
			FindViewById(Resource.Id.map_style_day).Selected = true;
		}
		else if (mapStyle.StyleFileName.Equals("nightstyle.json"))
		{
			FindViewById(Resource.Id.map_style_night).Selected = true;
		}
		else if (mapStyle.StyleFileName.Equals("outdoorstyle.json"))
		{
			FindViewById(Resource.Id.map_style_outdoor).Selected = true;
		}
		else if (mapStyle.StyleFileName.Equals("grayscalestyle.json"))
		{
			FindViewById(Resource.Id.map_style_grayscale).Selected = true;
		}
	}

	/// <summary>
	/// Clears the map
	/// </summary>
	private void clearMap()
	{
		Heading = false;
		switch (currentMapOption)
		{
			case MapOption.MAP_DISPLAY:
				break;
            case MapOption.MAP_OVERLAYS:
				// clear all map overlays (shapes)
				mapView.ClearAllOverlays();
				break;
            case MapOption.ALTERNATIVE_ROUTES:
				hideAlternativeRoutesButtons();
				// clear the alternative routes
				SKRouteManager.Instance.ClearRouteAlternatives();
				// clear the selected route
				SKRouteManager.Instance.ClearCurrentRoute();
				routeIds.Clear();
				break;
            case MapOption.MAP_STYLES:
				mapStylesView.Visibility = ViewStates.Gone;
				break;
            case MapOption.TRACKS:
				if (navigationInProgress)
				{
					// stop the navigation
					stopNavigation();
				}
				bottomButton.Visibility = ViewStates.Gone;
				if (TrackElementsActivity.selectedTrackElement != null)
				{
					mapView.ClearTrackElement(TrackElementsActivity.selectedTrackElement);
					SKRouteManager.Instance.ClearCurrentRoute();
				}
				TrackElementsActivity.selectedTrackElement = null;
				break;
            case MapOption.REAL_REACH:
				// removes real reach from the map
				mapView.ClearRealReachDisplay();
				realReachTimeLayout.Visibility = ViewStates.Gone;
				realReachEnergyLayout.Visibility = ViewStates.Gone;
				break;
            case MapOption.ANNOTATIONS:
				mapPopup.Visibility = ViewStates.Gone;
				// removes the annotations and custom POIs currently rendered
				mapView.DeleteAllAnnotationsAndCustomPOIs();
                goto case MapOption.ROUTING_AND_NAVIGATION;
            case MapOption.ROUTING_AND_NAVIGATION:
				bottomButton.Visibility = ViewStates.Gone;
				SKRouteManager.Instance.ClearCurrentRoute();
				if (navigationInProgress)
				{
					// stop navigation if ongoing
					stopNavigation();
				}
				break;
            case MapOption.POI_TRACKING:
				if (navigationInProgress)
				{
					// stop the navigation
					stopNavigation();
				}
				SKRouteManager.Instance.ClearCurrentRoute();
				// remove the detected POIs from the map
				mapView.DeleteAllAnnotationsAndCustomPOIs();
				// stop the POI tracker
				poiTrackingManager.StopPOITracker();
				break;
            case MapOption.HEAT_MAP:
				heatMapCategories = null;
				mapView.ClearHeatMapsDisplay();
				break;
            case MapOption.MAP_INTERACTION:
				mapPopup.Visibility = ViewStates.Gone;
				mapView.DeleteAllAnnotationsAndCustomPOIs();
                ((TextView)FindViewById(Resource.Id.top_text)).SetOnClickListener(null);
				((TextView) FindViewById(Resource.Id.top_text)).Text = "Title text";
				((TextView) FindViewById(Resource.Id.bottom_text)).Text = "Subtitle text";
				break;
            case MapOption.NAVI_UI:
				navigationUI.Visibility = ViewStates.Gone;
				mapView.DeleteAllAnnotationsAndCustomPOIs();
				break;
			default:
				break;
		}
		currentMapOption = MapOption.MAP_DISPLAY;
		positionMeButton.Visibility = ViewStates.Visible;
		headingButton.Visibility = ViewStates.Visible;
	}

	private void deselectAlternativeRoutesButtons()
	{
		foreach (Button b in altRoutesButtons)
		{
			b.Selected = false;
		}
	}

	private void hideAlternativeRoutesButtons()
	{
		deselectAlternativeRoutesButtons();
		altRoutesView.Visibility = ViewStates.Gone;
		foreach (Button b in altRoutesButtons)
		{
			b.Text = "distance\ntime";
		}
	}

	private void selectAlternativeRoute(int routeIndex)
	{
		if (routeIds.Count > routeIndex)
		{
			deselectAlternativeRoutesButtons();
			altRoutesButtons[routeIndex].Selected = true;
			SKRouteManager.Instance.ZoomToRoute(1, 1, 110, 8, 8, 8);
			SKRouteManager.Instance.SetCurrentRouteByUniqueId(routeIds[routeIndex].Value);
		}

	}

	/// <summary>
	/// Launches a navigation on the current route
	/// </summary>
	private void launchNavigation()
	{
		if (TrackElementsActivity.selectedTrackElement != null)
		{
			mapView.ClearTrackElement(TrackElementsActivity.selectedTrackElement);

		}
		// get navigation settings object
		SKNavigationSettings navigationSettings = new SKNavigationSettings();
		// set the desired navigation settings
		navigationSettings.NavigationType = Skobbler.Ngx.Navigation.SKNavigationSettings.SKNavigationType.Simulation;
		navigationSettings.PositionerVerticalAlignment = -0.25f;
		navigationSettings.ShowRealGPSPositions = false;
		// get the navigation manager object
		SKNavigationManager navigationManager = SKNavigationManager.Instance;
        navigationManager.SetMapView(mapView);
		// set listener for navigation events
        navigationManager.SetNavigationListener(this);

		// start navigating using the settings
		navigationManager.StartNavigation(navigationSettings);
		navigationInProgress = true;
	}

	/// <summary>
	/// Setting the audio advices
	/// </summary>
	private MapAdvices AdvicesAndStartNavigation
	{
		set
		{
			SKAdvisorSettings advisorSettings = new SKAdvisorSettings();
			advisorSettings.Language = SKAdvisorSettings.SKAdvisorLanguage.LanguageEn;
			advisorSettings.AdvisorConfigPath = app.MapResourcesDirPath + "/Advisor";
			advisorSettings.ResourcePath = app.MapResourcesDirPath + "/Advisor/Languages";
			advisorSettings.AdvisorVoice = "en";
			switch (value)
			{
                case MapAdvices.AUDIO_FILES:
					advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.AudioFiles;
					break;
                case MapAdvices.TEXT_TO_SPEECH:
					advisorSettings.AdvisorType = SKAdvisorSettings.SKAdvisorType.TextToSpeech;
					break;
			}
			SKRouteManager.Instance.SetAudioAdvisorSettings(advisorSettings);
			launchNavigation();
    
		}
	}


	/// <summary>
	/// Stops the navigation
	/// </summary>
	private void stopNavigation()
	{
		navigationInProgress = false;
		routeIds.Clear();
		if (textToSpeechEngine != null && !textToSpeechEngine.IsSpeaking)
		{
			textToSpeechEngine.Stop();
		}
		if (currentMapOption.Equals(MapOption.TRACKS) && TrackElementsActivity.selectedTrackElement != null)
		{
			SKRouteManager.Instance.ClearCurrentRoute();
			mapView.DrawTrackElement(TrackElementsActivity.selectedTrackElement);
			mapView.FitTrackElementInView(TrackElementsActivity.selectedTrackElement, false);

            SKRouteManager.Instance.SetRouteListener(this);
			SKRouteManager.Instance.CreateRouteFromTrackElement(TrackElementsActivity.selectedTrackElement, SKRouteSettings.SKRouteMode.BicycleFastest, true, true, false);
		}
		SKNavigationManager.Instance.StopNavigation();

	}

	// route computation callbacks ...
	public override void onAllRoutesCompleted()
	{

		SKRouteManager.Instance.ZoomToRoute(1, 1, 8, 8, 8, 8);
		if (currentMapOption == MapOption.POI_TRACKING)
		{
			// start the POI tracker
			poiTrackingManager.StartPOITrackerWithRadius(10000, 0.5);
			// set warning rules for trackable POIs
			poiTrackingManager.AddWarningRulesforPoiType(SKTrackablePOIType.Speedcam);
			// launch navigation
			launchNavigation();
		}
	}


	public override void onReceivedPOIs(SKTrackablePOIType type, IList<SKDetectedPOI> detectedPois)
	{
		updateMapWithLatestDetectedPOIs(detectedPois);
	}

	/// <summary>
	/// Updates the map when trackable POIs are detected such that only the
	/// currently detected POIs are rendered on the map
	/// </summary>
	/// <param name="detectedPois"> </param>
	private void updateMapWithLatestDetectedPOIs(IList<SKDetectedPOI> detectedPois)
	{

		IList<int?> detectedIdsList = new List<int?>();
		foreach (SKDetectedPOI detectedPoi in detectedPois)
		{
			detectedIdsList.Add(detectedPoi.PoiID);
		}
		foreach (int detectedPoiId in detectedIdsList)
		{
			if (detectedPoiId == -1)
			{
				continue;
			}
			if (drawnTrackablePOIs.get(detectedPoiId) == null)
			{
				drawnTrackablePOIs.put(detectedPoiId, trackablePOIs.get(detectedPoiId));
				drawDetectedPOI(detectedPoiId);
			}
		}
		foreach (int drawnPoiId in new List<int?>(drawnTrackablePOIs.Keys))
		{
			if (!detectedIdsList.Contains(drawnPoiId))
			{
				drawnTrackablePOIs.Remove(drawnPoiId);
				mapView.DeleteAnnotation(drawnPoiId);
			}
		}
	}

	/// <summary>
	/// Draws a detected trackable POI as an annotation on the map
	/// </summary>
	/// <param name="poiId"> </param>
	private void drawDetectedPOI(int poiId)
	{
		SKAnnotation annotation = new SKAnnotation(poiId);
        SKTrackablePOI poi = trackablePOIs[poiId];
		annotation.Location = poi.Coordinate;
		annotation.MininumZoomLevel = 5;
		annotation.AnnotationType = SKAnnotation.SkAnnotationTypeMarker;
		mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
	}

	public override void onUpdatePOIsInRadius(double latitude, double longitude, int radius)
	{

		// set the POIs to be tracked by the POI tracker
		poiTrackingManager.SetTrackedPOIs(SKTrackablePOIType.Speedcam, new List<SKTrackablePOI>(trackablePOIs.Values));
	}

	public override void onSensorChanged(SensorEvent t)
	{
		mapView.ReportNewHeading(t.Values[0]);
	}

	/// <summary>
	/// Enables/disables heading mode
	/// </summary>
	/// <param name="enabled"> </param>
	private bool Heading
	{
		set
		{
			if (value)
			{
				headingOn = true;
				mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.PositionPlusHeading;
				startOrientationSensor();
			}
			else
			{
				headingOn = false;
                mapView.MapSettings.FollowerMode = SKMapSettings.SKMapFollowerMode.None;
				stopOrientationSensor();
			}
		}
	}

	/// <summary>
	/// Activates the orientation sensor
	/// </summary>
	private void startOrientationSensor()
	{
		SensorManager sensorManager = (SensorManager) GetSystemService(Context.SensorService);
		Sensor orientationSensor = sensorManager.GetDefaultSensor(SensorType.Orientation);
		sensorManager.RegisterListener(this, orientationSensor, SensorDelay.Ui);
	}

	/// <summary>
	/// Deactivates the orientation sensor
	/// </summary>
	private void stopOrientationSensor()
	{
		SensorManager sensorManager = (SensorManager) GetSystemService(Context.SensorService);
		sensorManager.UnregisterListener(this);
	}

	public override void onCurrentPositionUpdate(SKPosition currentPosition)
	{
		this.currentPosition = currentPosition;
		mapView.ReportNewGPSPosition(this.currentPosition);
	}

	public override void onOnlineRouteComputationHanging(int status)
	{

	}


	// map interaction callbacks ...
	public override void onActionPan()
	{
		if (headingOn)
		{
			Heading = false;
		}
	}

	public override void onActionZoom()
	{

	}


	public override void onConfigurationChanged(Configuration newConfig)
	{
		base.OnConfigurationChanged(newConfig);

		if (navigationManager != null && skToolsNavigationInProgress)
		{
			navigationManager.notifyOrientationChanged();
		}
	}

	public override void onAnnotationSelected(SKAnnotation annotation)
	{
		DisplayMetrics metrics = new DisplayMetrics();
		float density = Resources.DisplayMetrics.Density;
		if (navigationUI.Visibility == ViewStates.Visible)
		{
			return;
		}
		// show the popup at the proper position when selecting an
		// annotation
		switch (annotation.UniqueID)
		{
			case 10:
				if (density <= 1)
				{
					SKLogging.WriteLog(TAG, "Density 1 ", SKLogging.LogError);
					mapPopup.SetVerticalOffset(48 / density);
				}
				else if (density <= 2)
				{
                    SKLogging.WriteLog(TAG, "Density 2 ", SKLogging.LogError);
					mapPopup.SetVerticalOffset(96 / density);

				}
				else
				{
                    SKLogging.WriteLog(TAG, "Density 3 ", SKLogging.LogError);
					mapPopup.SetVerticalOffset(192 / density);
				}
				popupTitleView.Text = "Annotation using texture ID";
				popupDescriptionView.Text = " Red pin ";
				break;
			case 13:
				// because the location of the annotation is the center of the image the vertical offset has to be imageSize/2
				mapPopup.SetVerticalOffset(annotation.ImageSize / 2 / density);
				popupTitleView.Text = "Annotation using absolute \n image path";
				popupDescriptionView.Text = null;
				break;
			case 14:
				int properSize = calculateProperSizeForView(annotation.AnnotationView.Width, annotation.AnnotationView.Height);
				// If  imageWidth and imageHeight for the annotationView  are not power of 2 the actual size of the image will be the next power of 2 of max(width,
				// height) so the vertical offset
				// for the callout has to be half of the annotation's size
				mapPopup.SetVerticalOffset(properSize / 2 / density);
				popupTitleView.Text = "Annotation using  \n drawable resource ID ";
				popupDescriptionView.Text = null;
				break;
			case 15:
				properSize = calculateProperSizeForView(customView.Width, customView.Height);
				// If  width and height of the view  are not power of 2 the actual size of the image will be the next power of 2 of max(width,height) so the vertical offset
				// for the callout has to be half of the annotation's size
				mapPopup.SetVerticalOffset(properSize / 2 / density);
				popupTitleView.Text = "Annotation using custom view";
				popupDescriptionView.Text = null;
				break;

		}
		mapPopup.ShowAtLocation(annotation.Location, true);
	}


	private int calculateProperSizeForView(int width, int height)
	{
		int maxDimension = Math.Max(width, height);
		int power = 2;

		while (maxDimension > power)
		{
			power *= 2;
		}

		return power;

	}

	public override void onCustomPOISelected(SKMapCustomPOI customPoi)
	{

	}


	public override void onDoubleTap(SKScreenPoint point)
	{
		// zoom in on a position when double tapping
		mapView.ZoomInAt(point);
	}

	public override void onInternetConnectionNeeded()
	{

	}

	public override void onLongPress(SKScreenPoint point)
	{
		SKCoordinate poiCoordinates = mapView.PointToCoordinate(point);
		SKSearchResult place = SKReverseGeocoderManager.Instance.ReverseGeocodePosition(poiCoordinates);

		bool selectPoint = isStartPointBtnPressed || isEndPointBtnPressed || isViaPointSelected;
		if (poiCoordinates != null && place != null && selectPoint)
		{
			SKAnnotation annotation = new SKAnnotation(GREEN_PIN_ICON_ID);
			if (isStartPointBtnPressed)
			{
				annotation.UniqueID = GREEN_PIN_ICON_ID;
				annotation.AnnotationType = SKAnnotation.SkAnnotationTypeGreen;
				startPoint = place.Location;
			}
			else if (isEndPointBtnPressed)
			{
				annotation.UniqueID = RED_PIN_ICON_ID;
				annotation.AnnotationType = SKAnnotation.SkAnnotationTypeRed;
				destinationPoint = place.Location;
			}
			else if (isViaPointSelected)
			{
				annotation.UniqueID = VIA_POINT_ICON_ID;
				annotation.AnnotationType = SKAnnotation.SkAnnotationTypeMarker;
				viaPoint = new SKViaPoint(VIA_POINT_ICON_ID, place.Location);
				FindViewById(Resource.Id.clear_via_point_button).Visibility = ViewStates.Visible;
			}

			annotation.Location = place.Location;
			annotation.MininumZoomLevel = 5;
			mapView.AddAnnotation(annotation, SKAnimationSettings.AnimationNone);
		}

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

	public override void onMapRegionChanged(SKCoordinateRegion mapRegion)
	{
	}

	public override void onRotateMap()
	{

	}

	public override void onScreenOrientationChanged()
	{

	}

	public override void onSingleTap(SKScreenPoint point)
	{
		mapPopup.Visibility = ViewStates.Gone;
	}


	public override void onCompassSelected()
	{

	}

	public override void onInternationalisationCalled(int result)
	{

	}

	public override void onDestinationReached()
	{
		Toast.MakeText(this, "Destination reached", ToastLength.Short).Show();
		// clear the map when reaching destination
		clearMap();
	}


	public override void onFreeDriveUpdated(string countryCode, string streetName, SKNavigationState.SKStreetType streetType, double currentSpeed, double speedLimit)
	{

	}

	public override void onReRoutingStarted()
	{

	}

	public override void onSpeedExceededWithAudioFiles(string[] adviceList, bool speedExceeded)
	{

	}

	public override void onUpdateNavigationState(SKNavigationState navigationState)
	{
	}


	public override void onVisualAdviceChanged(bool firstVisualAdviceChanged, bool secondVisualAdviceChanged, SKNavigationState navigationState)
	{
	}

	public override void onRealReachCalculationCompleted(SKBoundingBox bbox)
	{
		// fit the reachable area on the screen when real reach calculataion
		// ends
		mapView.FitRealReachInView(bbox, false, 0);
	}


	public override void onPOIClusterSelected(SKPOICluster poiCluster)
	{
		// TODO Auto-generated method stub

	}

	public override void onAccuracyChanged(Sensor sensor, int accuracy)
	{
		// TODO Auto-generated method stub

	}

	public override void onTunnelEvent(bool tunnelEntered)
	{
		// TODO Auto-generated method stub

	}

	public override void onMapRegionChangeEnded(SKCoordinateRegion mapRegion)
	{
		// TODO Auto-generated method stub

	}

	public override void onMapRegionChangeStarted(SKCoordinateRegion mapRegion)
	{
		// TODO Auto-generated method stub

	}

	public override void onMapVersionSet(int newVersion)
	{
		// TODO Auto-generated method stub

	}

    public override void onNewVersionDetected(int newVersion)
	{
		AlertDialog alertDialog = new AlertDialog.Builder(this).Create();
		alertDialog.SetMessage("New map version available");
		alertDialog.SetCancelable(true);
		alertDialog.SetButton(AlertDialog.BUTTON_POSITIVE, GetString(Resource.String.update_label), new OnClickListenerAnonymousInnerClassHelper(this, newVersion));
		alertDialog.SetButton(AlertDialog.BUTTON_NEGATIVE, GetString(Resource.String.cancel_label), new OnClickListenerAnonymousInnerClassHelper2(this, alertDialog));
		alertDialog.Show();
	}

    private class OnClickListenerAnonymousInnerClassHelper : IDialogInterfaceOnClickListener
    {
        private readonly MapActivity outerInstance;

        private int newVersion;

        public OnClickListenerAnonymousInnerClassHelper(MapActivity outerInstance, int newVersion)
        {
            this.outerInstance = outerInstance;
            this.newVersion = newVersion;
        }


        public virtual void onClick(DialogInterface dialog, int id)
        {
            SKVersioningManager manager = SKVersioningManager.Instance;
            bool updated = manager.updateMapsVersion(newVersion);
            if (updated)
            {
                Toast.MakeText(outerInstance, "The map has been updated to version " + newVersion, ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(outerInstance, "An error occurred in updating the map ", ToastLength.Short).Show();
            }
        }
    }

    private class OnClickListenerAnonymousInnerClassHelper2 : IDialogInterfaceOnClickListener
    {
        private readonly MapActivity outerInstance;

        private AlertDialog alertDialog;

        public OnClickListenerAnonymousInnerClassHelper2(MapActivity outerInstance, AlertDialog alertDialog)
        {
            this.outerInstance = outerInstance;
            this.alertDialog = alertDialog;
        }


        public virtual void onClick(DialogInterface dialog, int id)
        {
            alertDialog.cancel();
        }
    }

    public override void onNoNewVersionDetected()
	{
		Toast.MakeText(this, "No new versions were detected", ToastLength.Short).Show();
	}

    public override void onVersionFileDownloadTimeout()
    {
        // TODO Auto-generated method stub

    }

    public override void onCurrentPositionSelected()
    {
        // TODO Auto-generated method stub

    }

    public override void onObjectSelected(int id)
    {
    }


    public override void onBackPressed()
	{
		// TODO Auto-generated method stub
		if (menu.Visibility == ViewStates.Visible)
		{
			menu.Visibility = ViewStates.Gone;
		}
		else if (skToolsNavigationInProgress || skToolsRouteCalculated)
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.Title = "Really quit?";
			alert.Message = "Do you want to exit navigation?";
			alert.setPositiveButton("Yes", new OnClickListenerAnonymousInnerClassHelper3(this));
			alert.setNegativeButton("Cancel", null);
			alert.show();
		}
		else
		{
			AlertDialog.Builder alert = new AlertDialog.Builder(this);
			alert.Title = "Really quit? ";
			alert.Message = "Do you really want to exit the app?";
			alert.setPositiveButton("Yes", new OnClickListenerAnonymousInnerClassHelper4(this));
			alert.setNegativeButton("Cancel", null);
			alert.show();

		}

	}

    private class OnClickListenerAnonymousInnerClassHelper3 : IDialogInterfaceOnClickListener
    {
        private readonly MapActivity outerInstance;

        public OnClickListenerAnonymousInnerClassHelper3(MapActivity outerInstance)
        {
            this.outerInstance = outerInstance;
        }


        public virtual void onClick(DialogInterface dialog, int id)
        {
            if (skToolsNavigationInProgress)
            {
                navigationManager.stopNavigation();
            }
            else
            {
                navigationManager.removeRouteCalculationScreen();
            }
            initializeNavigationUI(false);
            skToolsRouteCalculated = false;
            skToolsNavigationInProgress = false;
        }
    }

    private class OnClickListenerAnonymousInnerClassHelper4 : IDialogInterfaceOnClickListener
    {
        private readonly MapActivity outerInstance;

        public OnClickListenerAnonymousInnerClassHelper4(MapActivity outerInstance)
        {
            this.outerInstance = outerInstance;
        }


        public virtual void onClick(DialogInterface dialog, int id)
        {
            if (ResourceDownloadsListActivity.mapsDAO != null)
            {
                SKToolsDownloadManager downloadManager = SKToolsDownloadManager.getInstance(new SKToolsDownloadListenerAnonymousInnerClassHelper(this));
                if (downloadManager.DownloadProcessRunning)
                {
                    // pause downloads when exiting app if one is currently in progress
                    downloadManager.pauseDownloadThread();
                    return;
                }
            }
            finish();
        }

        private class SKToolsDownloadListenerAnonymousInnerClassHelper : SKToolsDownloadListener
        {
            private readonly OnClickListenerAnonymousInnerClassHelper4 outerInstance;

            public SKToolsDownloadListenerAnonymousInnerClassHelper(OnClickListenerAnonymousInnerClassHelper4 outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void onDownloadProgress(SKToolsDownloadItem currentDownloadItem)
            {

            }

            public override void onDownloadCancelled(string currentDownloadItemCode)
            {

            }

            public override void onDownloadPaused(SKToolsDownloadItem currentDownloadItem)
            {
                MapDownloadResource mapResource = (MapDownloadResource)ResourceDownloadsListActivity.allMapResources.get(currentDownloadItem.ItemCode);
                mapResource.DownloadState = currentDownloadItem.DownloadState;
                mapResource.NoDownloadedBytes = currentDownloadItem.NoDownloadedBytes;
                ResourceDownloadsListActivity.mapsDAO.updateMapResource(mapResource);
                app.AppPrefs.saveDownloadStepPreference(currentDownloadItem.CurrentStepIndex);
                finish();
            }

            public override void onInternetConnectionFailed(SKToolsDownloadItem currentDownloadItem, bool responseReceivedFromServer)
            {

            }

            public override void onAllDownloadsCancelled()
            {

            }

            public override void onNotEnoughMemoryOnCurrentStorage(SKToolsDownloadItem currentDownloadItem)
            {

            }

            public override void onInstallStarted(SKToolsDownloadItem currentInstallingItem)
            {

            }

            public override void onInstallFinished(SKToolsDownloadItem currentInstallingItem)
            {

            }
        }
    }

    public override void onRouteCalculationCompleted(SKRouteInfo routeInfo)
    {
        if (currentMapOption == MapOption.ALTERNATIVE_ROUTES)
        {
            int routeIndex = routeIds.size();
            routeIds.add(routeInfo.RouteID);
            altRoutesButtons[routeIndex].Text = DemoUtils.formatDistance(routeInfo.Distance) + "\n" + DemoUtils.formatTime(routeInfo.EstimatedTime);
            if (routeIndex == 0)
            {
                // select 1st alternative by default
                selectAlternativeRoute(0);
            }
        }
        else if (currentMapOption == MapOption.ROUTING_AND_NAVIGATION || currentMapOption == MapOption.POI_TRACKING || currentMapOption == MapOption.NAVI_UI)
        {
            // select the current route (on which navigation will run)
            SKRouteManager.Instance.CurrentRouteByUniqueId = routeInfo.RouteID;
            // zoom to the current route
            SKRouteManager.Instance.zoomToRoute(1, 1, 8, 8, 8, 8);

            if (currentMapOption == MapOption.ROUTING_AND_NAVIGATION)
            {
                bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
            }
        }
        else if (currentMapOption == MapOption.TRACKS)
        {
            SKRouteManager.Instance.zoomToRoute(1, 1, 8, 8, 8, 8);
            bottomButton.Visibility = ViewStates.Visible;
            bottomButton.Text = Resources.GetString(Resource.String.start_navigation);
        }
    }

    public override void onRouteCalculationFailed(SKRoutingErrorCode arg0)
	{
		Toast.MakeText(this, Resources.GetString(Resource.String.route_calculation_failed), ToastLength.Short).Show();
	}

    public override void onSignalNewAdviceWithAudioFiles(string[] audioFiles, bool specialSoundFile)
    {
        // a new navigation advice was received
        SKLogging.WriteLog(TAG, " onSignalNewAdviceWithAudioFiles " + audioFiles, Log.DEBUG);
        SKToolsAdvicePlayer.Instance.playAdvice(audioFiles, SKToolsAdvicePlayer.PRIORITY_NAVIGATION);
    }

    public override void onSignalNewAdviceWithInstruction(string instruction)
    {
        SKLogging.WriteLog(TAG, " onSignalNewAdviceWithInstruction " + instruction, Log.DEBUG);
        textToSpeechEngine.speak(instruction, TextToSpeech.QUEUE_ADD, null);
    }

    public override void onSpeedExceededWithInstruction(string instruction, bool speedExceeded)
    {
    }

    public override void onServerLikeRouteCalculationCompleted(SKRouteJsonAnswer arg0)
    {
        // TODO Auto-generated method stub

    }

    public override void onViaPointReached(int index)
    {
    }

    public override void onNavigationStarted()
    {
        skToolsNavigationInProgress = true;
        if (navigationUI.Visibility == ViewStates.Visible)
        {
            navigationUI.Visibility = ViewStates.Gone;
        }
    }

    public override void onNavigationEnded()
    {
        skToolsRouteCalculated = false;
        skToolsNavigationInProgress = false;
        initializeNavigationUI(false);
    }

    public override void onRouteCalculationStarted()
    {
        skToolsRouteCalculated = true;
    }

    public override void onRouteCalculationCompleted()
    {

    }


    public override void onRouteCalculationCanceled()
    {
        skToolsRouteCalculated = false;
        skToolsNavigationInProgress = false;
        initializeNavigationUI(false);
    }

    }
}