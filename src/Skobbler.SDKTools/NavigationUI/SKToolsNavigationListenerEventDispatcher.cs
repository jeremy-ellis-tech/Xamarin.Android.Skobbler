using System;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// Helper class to provide events that correspond to listener callbacks.
    /// Taken from how automatic event generation is done during jar binding.
    /// See: http://developer.xamarin.com/guides/android/advanced_topics/java_integration_overview/binding_a_java_library_(.jar)/
    /// </summary>
    internal class SKToolsNavigationListenerEventDispatcher : ISKToolsNavigationListener
    {
        private readonly object _sender;

        public SKToolsNavigationListenerEventDispatcher(object sender)
        {
            _sender = sender;
        }

        internal EventHandler NavigationStarted;
        internal EventHandler NavigationEnded;
        internal EventHandler RouteCalculationStarted;
        internal EventHandler RouteCalculationCompleted;
        internal EventHandler RouteCalculationCanceled;

        public void OnNavigationStarted()
        {
            var handler = NavigationStarted;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }

        public void OnNavigationEnded()
        {
            var handler = NavigationEnded;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }

        public void OnRouteCalculationStarted()
        {
            var handler = RouteCalculationStarted;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }

        public void OnRouteCalculationCompleted()
        {
            var handler = RouteCalculationCompleted;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }

        public void OnRouteCalculationCanceled()
        {
            var handler = RouteCalculationCanceled;

            if (handler != null)
            {
                handler(_sender, EventArgs.Empty);
            }
        }
    }
}