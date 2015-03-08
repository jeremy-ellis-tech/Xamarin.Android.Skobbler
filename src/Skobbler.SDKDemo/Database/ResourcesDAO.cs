using System.Text;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Skobbler.Ngx.Util;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// Class responsible for creating and upgrading the application's database.
	/// </summary>
	public class ResourcesDao : SQLiteOpenHelper
	{

		/// <summary>
		/// the name of the database
		/// </summary>
		public const string DATABASE_NAME = "application_database";

		/// <summary>
		/// the database version
		/// </summary>
		public const sbyte DatabaseVersion = 1;

		/// <summary>
		/// tag associated with current class
		/// </summary>
		private const string Tag = "ResourcesDAO";

		/// <summary>
		/// an instance of this class
		/// </summary>
		private static ResourcesDao _databaseInstance;

		/// <summary>
		/// SQLITE database instance
		/// </summary>
		private SQLiteDatabase _sqLiteDatabaseInstance;

		/// <summary>
		/// creates a new ResourcesDAO object </summary>
		/// <param name="context"> the context of the application </param>
		private ResourcesDao(Context context) : base(context, DATABASE_NAME, null, DatabaseVersion)
		{
		}

		/// <summary>
		/// Returns the {@code instance} </summary>
		/// <param name="context"> the context of the application </param>
		/// <returns> the instance of the class </returns>
		public static ResourcesDao GetInstance(Context context)
		{
			if (_databaseInstance == null)
			{
				_databaseInstance = new ResourcesDao(context);
			}
			return _databaseInstance;
		}

        public override void OnCreate(SQLiteDatabase db)
        {
            SKLogging.WriteLog(Tag, "On create resources database !!!", SKLogging.LogDebug);
            string createMapResourcesTable = (new StringBuilder("CREATE TABLE IF NOT EXISTS ")).Append(MapsDao.MapsTable).Append(" (").Append(MapsDao.Key).Append(" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " + "").Append(MapsDao.Code).Append(" TEXT UNIQUE, ").Append(MapsDao.ParentCode).Append(" TEXT, ").Append(MapsDao.Region).Append(" TEXT, ").Append(MapsDao.Names).Append(" TEXT, " + "").Append(MapsDao.SkmFilePath).Append(" TEXT, ").Append(MapsDao.ZipFilePath).Append(" TEXT, ").Append(MapsDao.TxgFilePath).Append(" TEXT, ").Append(MapsDao.TxgFileSize).Append(" INTEGER, " + "").Append(MapsDao.SkmAndZipFilesSize).Append(" INTEGER, ").Append(MapsDao.SkmFileSize).Append(" INTEGER, " + "").Append(MapsDao.UnzippedFileSize).Append(" INTEGER, ").Append(MapsDao.BoundingBoxLatitudeMax).Append(" DOUBLE, ").Append(MapsDao.BoundingBoxLatitudeMin).Append(" DOUBLE, ").Append(MapsDao.BoundingBoxLongitudeMax).Append(" DOUBLE, ").Append(MapsDao.BoundingBoxLongitudeMin).Append(" DOUBLE, " + "").Append(MapsDao.Subtype).Append(" TEXT, ").Append(MapsDao.State).Append(" INTEGER, ").Append(MapsDao.NoDownloadedBytes).Append(" INTEGER, ").Append(MapsDao.FlagId).Append(" INTEGER, ").Append(MapsDao.DownloadPath).Append(" TEXT)").ToString();
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
				return _sqLiteDatabaseInstance;
			}
		}

		/// <summary>
		/// Opens the application database.
		/// </summary>
		public virtual void OpenDatabase()
		{
			try
			{
				if ((_sqLiteDatabaseInstance == null) || !_sqLiteDatabaseInstance.IsOpen)
				{
					_sqLiteDatabaseInstance = WritableDatabase;
				}
			}
			catch (SQLException e)
			{
				SKLogging.WriteLog(Tag, "Error when opening database: " + e.Message, SKLogging.LogWarning);
				_sqLiteDatabaseInstance = ReadableDatabase;
			}
		}

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
        {
            
        }
	}
}