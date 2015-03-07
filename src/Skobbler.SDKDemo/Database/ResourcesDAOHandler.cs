namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// This class provides methods for accessing the database tables
	/// </summary>
	public class ResourcesDAOHandler
	{

		/// <summary>
		/// Singleton instance for current class
		/// </summary>
		private static ResourcesDAOHandler instance;

		/// <summary>
		/// the database object for maps table
		/// </summary>
		private MapsDAO mapsDAO;

		/// <summary>
		/// constructs a ResourcesDAOHandler object </summary>
		/// <param name="context"> application context </param>
		private ResourcesDAOHandler(Context context)
		{
			ResourcesDAO resourcesDAO = ResourcesDAO.getInstance(context);
			resourcesDAO.openDatabase();
			mapsDAO = new MapsDAO(resourcesDAO);
		}

		/// <summary>
		/// gets an instance of ResourcesDAOHandler object </summary>
		/// <param name="context"> application context </param>
		/// <returns> an instance of ResourcesDAOHandler object </returns>
		public static ResourcesDAOHandler getInstance(Context context)
		{
			if (instance == null)
			{
				instance = new ResourcesDAOHandler(context);
			}
			return instance;
		}

		/// <summary>
		/// gets the maps DAO object </summary>
		/// <returns> maps DAO object </returns>
		public virtual MapsDAO MapsDAO
		{
			get
			{
				return mapsDAO;
			}
		}
	}
}