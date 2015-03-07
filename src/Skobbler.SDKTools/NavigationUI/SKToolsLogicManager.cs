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
using Skobbler.Ngx.Positioner;
using Skobbler.SDKTools.NavigationUI;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// This class handles the logic related to the navigation and route calculation.
    /// </summary>
    public class SKToolsLogicManager : ISKMapSurfaceListener, ISKNavigationListener, ISKRouteListener, ISKCurrentPositionListener
    {
        private static SKToolsLogicManager _instance = null;

        private SKMapSurfaceView _mapView;
        private Activity _currentActivity;
        private SKCurrentPositionProvider _currentPositionProvider;
        private SKNavigationManager _naviManager;
        private SKToolsNavigationConfiguration _configuration;
        public int NumberOfSettingsOptionsPressed { get; }
        private string[] _lastAudioAdvices;
        private long _navigationCurrentDistance;
        private bool _roadBlocked;
        private bool _reRoutingInProgress = false;
        private static SKPosition _lastUserPosition;
        private bool _navigationStopped;
        private ISKMapSurfaceListener _previousMapSurfaceListener;

        private List<SKRouteInfo> _skRouteInfoList = new List<SKRouteInfo>();
        private ISKToolsNavigationListener _navigationListener;
        private SKMapViewStyle _currentMapStyle;
        private SKMapSettings.SKMapDisplayMode _currentUserDisplayMode;

        private static readonly object _lock = new object();

        public static SKToolsLogicManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock (_lock)
                    {
                        if(_instance == null)
                        {
                            _instance = new SKToolsLogicManager();
                        }
                    }
                }

                return _instance;
            }
        }

        private SKToolsLogicManager()
        {
            _naviManager = SKNavigationManager.Instance;
        }
    }
}