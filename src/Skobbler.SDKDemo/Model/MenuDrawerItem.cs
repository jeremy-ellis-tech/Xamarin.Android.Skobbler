using Skobbler.SDKDemo.Activities;

namespace Skobbler.SDKDemo.Model
{
    public class MenuDrawerItem
    {
        public static readonly int ItemTypeType = 1;
        public static readonly int SectionType = 2;

        public MenuDrawerItem(MapActivity.MapOption mapOption)
        {
            MapOption = mapOption;
        }

        public MapActivity.MapOption MapOption { get; private set; }
        public string Label { get; set; }
        public int ItemType { get; set; }
    }
}