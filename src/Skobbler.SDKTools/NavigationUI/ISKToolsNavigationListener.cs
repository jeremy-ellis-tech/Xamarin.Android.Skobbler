namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public interface ISKToolsNavigationListener
    {
        void OnNavigationStarted();
        void OnNavigationEnded();
        void OnRouteCalculationStarted();
        void OnRouteCalculationCompleted();
        void OnRouteCalculationCanceled();
    }
}