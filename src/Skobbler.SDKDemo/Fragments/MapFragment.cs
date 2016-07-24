using Android.App;
using Android.OS;
using Android.Views;
using Skobbler.SDKDemo.Activities;

namespace Skobbler.SDKDemo.Fragments
{
    public class MapFragment : Fragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_map, null);
            (Activity as MapActivity).Initialize(view);

            return view;
        }
    }
}