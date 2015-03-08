using Android.Content;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// This class provides methods for accessing the database tables
	/// </summary>
	public class ResourcesDaoHandler
	{

		/// <summary>
		/// Singleton instance for current class
		/// </summary>
		private static ResourcesDaoHandler _instance;

		/// <summary>
		/// the database object for maps table
		/// </summary>
		private MapsDao _mapsDao;

		/// <summary>
		/// constructs a ResourcesDAOHandler object </summary>
		/// <param name="context"> application context </param>
		private ResourcesDaoHandler(Context context)
		{
			ResourcesDao resourcesDao = ResourcesDao.GetInstance(context);
			resourcesDao.OpenDatabase();
			_mapsDao = new MapsDao(resourcesDao);
		}

		/// <summary>
		/// gets an instance of ResourcesDAOHandler object </summary>
		/// <param name="context"> application context </param>
		/// <returns> an instance of ResourcesDAOHandler object </returns>
		public static ResourcesDaoHandler GetInstance(Context context)
		{
			if (_instance == null)
			{
				_instance = new ResourcesDaoHandler(context);
			}
			return _instance;
		}

		/// <summary>
		/// gets the maps DAO object </summary>
		/// <returns> maps DAO object </returns>
		public virtual MapsDao MapsDao
		{
			get
			{
				return _mapsDao;
			}
		}
	}
}