using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Skobbler.Ngx.Util;
using System.Text;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// Class responsible for creating and upgrading the application's database.
	/// </summary>
	public class ResourcesDAO : SQLiteOpenHelper
	{

		/// <summary>
		/// the name of the database
		/// </summary>
		public const string DATABASE_NAME = "application_database";

		/// <summary>
		/// the database version
		/// </summary>
		public const sbyte DATABASE_VERSION = 1;

		/// <summary>
		/// tag associated with current class
		/// </summary>
		private const string TAG = "ResourcesDAO";

		/// <summary>
		/// an instance of this class
		/// </summary>
		private static ResourcesDAO databaseInstance;

		/// <summary>
		/// SQLITE database instance
		/// </summary>
		private SQLiteDatabase sqLiteDatabaseInstance;

		/// <summary>
		/// creates a new ResourcesDAO object </summary>
		/// <param name="context"> the context of the application </param>
		private ResourcesDAO(Context context) : base(context, DATABASE_NAME, null, DATABASE_VERSION)
		{
		}

		/// <summary>
		/// Returns the {@code instance} </summary>
		/// <param name="context"> the context of the application </param>
		/// <returns> the instance of the class </returns>
		public static ResourcesDAO getInstance(Context context)
		{
			if (databaseInstance == null)
			{
				databaseInstance = new ResourcesDAO(context);
			}
			return databaseInstance;
		}

        public override void OnCreate(SQLiteDatabase db)
        {
            SKLogging.WriteLog(TAG, "On create resources database !!!", SKLogging.LogDebug);
            string createMapResourcesTable = (new StringBuilder("CREATE TABLE IF NOT EXISTS ")).Append(MapsDAO.MAPS_TABLE).Append(" (").Append(MapsDAO.KEY).Append(" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " + "").Append(MapsDAO.CODE).Append(" TEXT UNIQUE, ").Append(MapsDAO.PARENT_CODE).Append(" TEXT, ").Append(MapsDAO.REGION).Append(" TEXT, ").Append(MapsDAO.NAMES).Append(" TEXT, " + "").Append(MapsDAO.SKM_FILE_PATH).Append(" TEXT, ").Append(MapsDAO.ZIP_FILE_PATH).Append(" TEXT, ").Append(MapsDAO.TXG_FILE_PATH).Append(" TEXT, ").Append(MapsDAO.TXG_FILE_SIZE).Append(" INTEGER, " + "").Append(MapsDAO.SKM_AND_ZIP_FILES_SIZE).Append(" INTEGER, ").Append(MapsDAO.SKM_FILE_SIZE).Append(" INTEGER, " + "").Append(MapsDAO.UNZIPPED_FILE_SIZE).Append(" INTEGER, ").Append(MapsDAO.BOUNDING_BOX_LATITUDE_MAX).Append(" DOUBLE, ").Append(MapsDAO.BOUNDING_BOX_LATITUDE_MIN).Append(" DOUBLE, ").Append(MapsDAO.BOUNDING_BOX_LONGITUDE_MAX).Append(" DOUBLE, ").Append(MapsDAO.BOUNDING_BOX_LONGITUDE_MIN).Append(" DOUBLE, " + "").Append(MapsDAO.SUBTYPE).Append(" TEXT, ").Append(MapsDAO.STATE).Append(" INTEGER, ").Append(MapsDAO.NO_DOWNLOADED_BYTES).Append(" INTEGER, ").Append(MapsDAO.FLAG_ID).Append(" INTEGER, ").Append(MapsDAO.DOWNLOAD_PATH).Append(" TEXT)").ToString();
            db.BeginTransaction();
            db.ExecSQL(createMapResourcesTable);
            db.SetTransactionSuccessful();
            db.EndTransaction();
        }

		/// <summary>
		/// Returns the {@code database} </summary>
		/// <returns> the database </returns>
		public virtual SQLiteDatabase Database
		{
			get
			{
				return sqLiteDatabaseInstance;
			}
		}

		/// <summary>
		/// Opens the application database.
		/// </summary>
		public virtual void openDatabase()
		{
			try
			{
				if ((sqLiteDatabaseInstance == null) || !sqLiteDatabaseInstance.IsOpen)
				{
					sqLiteDatabaseInstance = WritableDatabase;
				}
			}
			catch (SQLException e)
			{
				SKLogging.WriteLog(TAG, "Error when opening database: " + e.Message, SKLogging.LogWarning);
				sqLiteDatabaseInstance = ReadableDatabase;
			}
		}

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            
        }
	}
}