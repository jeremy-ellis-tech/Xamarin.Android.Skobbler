using Android.Content;

namespace Skobbler.SDKDemo.Database
{
    public class ResourcesDAOHandler
    {
        private static ResourcesDAOHandler instance;

        private MapsDAO mapsDAO;

        private ResourcesDAOHandler(Context context)
        {
            ResourcesDAO resourcesDAO = ResourcesDAO.GetInstance(context);
            resourcesDAO.OpenDatabase();
            mapsDAO = new MapsDAO(resourcesDAO);
        }

        public static ResourcesDAOHandler GetInstance(Context context)
        {
            if (instance == null)
            {
                instance = new ResourcesDAOHandler(context);
            }
            return instance;
        }

        public MapsDAO getMapsDAO()
        {
            return mapsDAO;
        }
    }
}