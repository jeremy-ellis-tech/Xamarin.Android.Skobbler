using System.Text;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Skobbler.Ngx.Util;

namespace Skobbler.SDKDemo.Database
{
    public class ResourcesDAO : SQLiteOpenHelper
    {
        public static string DATABASE_NAME = "application_database";
        public static byte DATABASE_VERSION = 1;
        private static string TAG = "ResourcesDAO";
        private static ResourcesDAO databaseInstance;
        private SQLiteDatabase sqLiteDatabaseInstance;

        private ResourcesDAO(Context context)
            : base(context, DATABASE_NAME, null, DATABASE_VERSION)
        {
        }

        public static ResourcesDAO GetInstance(Context context)
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
            string createMapResourcesTable =
                    new StringBuilder("CREATE TABLE IF NOT EXISTS ").Append(MapsDAO.MAPS_TABLE).Append(" (")
                            .Append(MapsDAO.KEY).Append(" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                            "").Append(MapsDAO.CODE).Append(" TEXT UNIQUE, ").Append(MapsDAO.PARENT_CODE)
                            .Append(" TEXT, ").Append(MapsDAO.REGION).Append(" TEXT, ").Append(MapsDAO.NAMES).Append(" TEXT, " +
                            "").Append(MapsDAO.SKM_FILE_PATH).Append(" TEXT, ").Append(MapsDAO.ZIP_FILE_PATH)
                            .Append(" TEXT, ").Append(MapsDAO.TXG_FILE_PATH).Append(" TEXT, ")
                            .Append(MapsDAO.TXG_FILE_SIZE).Append(" INTEGER, " +
                            "").Append(MapsDAO.SKM_AND_ZIP_FILES_SIZE)
                            .Append(" INTEGER, ").Append(MapsDAO.SKM_FILE_SIZE).Append(" INTEGER, " +
                            "").Append(MapsDAO.UNZIPPED_FILE_SIZE)
                            .Append(" INTEGER, ").Append(MapsDAO.BOUNDING_BOX_LATITUDE_MAX).Append(" DOUBLE, ")
                            .Append(MapsDAO.BOUNDING_BOX_LATITUDE_MIN).Append(" DOUBLE, ")
                            .Append(MapsDAO.BOUNDING_BOX_LONGITUDE_MAX).Append(" DOUBLE, ")
                            .Append(MapsDAO.BOUNDING_BOX_LONGITUDE_MIN).Append(" DOUBLE, " +
                            "").Append(MapsDAO.SUBTYPE)
                            .Append(" TEXT, ").Append(MapsDAO.STATE).Append(" INTEGER, ")
                            .Append(MapsDAO.NO_DOWNLOADED_BYTES).Append(" INTEGER, ").Append(MapsDAO.FLAG_ID)
                            .Append(" INTEGER, ").Append(MapsDAO.DOWNLOAD_PATH).Append(" TEXT)").ToString();
            db.BeginTransaction();
            db.ExecSQL(createMapResourcesTable);
            db.SetTransactionSuccessful();
            db.EndTransaction();
        }

        public SQLiteDatabase getDatabase()
        {
            return sqLiteDatabaseInstance;
        }

        public void OpenDatabase()
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

        public override void OnUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) { }
    }
}