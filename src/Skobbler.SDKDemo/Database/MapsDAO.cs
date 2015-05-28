using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Skobbler.Ngx.SDKTools.Download;
using Skobbler.Ngx.Util;

namespace Skobbler.SDKDemo.Database
{
    public class MapsDAO
    {

        /**
         * US country code
         */
        public static String US_CODE = "US";

        /**
         * ENGLISH language code
         */
        public static String ENGLISH_LANGUAGE_CODE = "en";

        /**
         * name of the maps data table
         */
        public static String MAPS_TABLE = "Maps";

        /**
         * map code column key
         */
        public static String CODE = "Code";

        /**
         * map parent code column key
         */
        public static String PARENT_CODE = "ParentCode";

        /**
         * region column key (has a value only for state column)
         */
        public static String REGION = "Region";

        /**
         * map names column key
         */
        public static String NAMES = "Names";

        /**
         * map SKM file path column key
         */
        public static String SKM_FILE_PATH = "SkmFilePath";

        /**
         * map ZIP file path column key
         */
        public static String ZIP_FILE_PATH = "ZipFilePath";

        /**
         * map TXG file path column key
         */
        public static String TXG_FILE_PATH = "TxgFilePath";

        /**
         * map TXG file size column key
         */
        public static String TXG_FILE_SIZE = "TxgFileSize";

        /**
         * map total size(SKM + ZIP) files size column key
         */
        public static String SKM_AND_ZIP_FILES_SIZE = "SkmAndZipFilesSize";

        /**
         * map SKM file size column key
         */
        public static String SKM_FILE_SIZE = "SkmFileSize";

        /**
         * map UNZIPPED file size column key
         */
        public static String UNZIPPED_FILE_SIZE = "UnzippedFileSize";

        /**
         * map sub-type column key
         */
        public static String SUBTYPE = "SubType";

        /**
         * map state column key
         */
        public static String STATE = "State";

        /**
         * Bounding box column keys
         */
        public static String BOUNDING_BOX_LONGITUDE_MIN = "LongMin";

        public static String BOUNDING_BOX_LONGITUDE_MAX = "LongMax";

        public static String BOUNDING_BOX_LATITUDE_MIN = "LatMin";

        public static String BOUNDING_BOX_LATITUDE_MAX = "LatMax";

        /**
         * map no bytes column key
         */
        public static String NO_DOWNLOADED_BYTES = "NoDownloadedBytes";

        /**
         * flag ID
         */
        public static String FLAG_ID = "FlagID";

        /**
         * download path
         */
        public static String DOWNLOAD_PATH = "DownloadPath";

        /**
         * Prefix for the flag image resources
         */
        public static String FLAG_BIG_ICON_PREFIX = "icon_flag_big_";

        /**
         * map type column values
         */
        public static String CONTINENT_TYPE = "continent";

        public static String COUNTRY_TYPE = "country";

        public static String REGION_TYPE = "region";

        public static String CITY_TYPE = "city";

        public static String STATE_TYPE = "state";

        /**
         * tag for the class
         */
        private static String TAG = "MapsDAO";

        /**
         * auto-increment column key (primary key ID)
         */
        public static String KEY = "Key";

        /**
         * the associated resources DAO
         */
        private ResourcesDAO resourcesDAO;

        /**
         * constructs an object of this type
         *
         * @param resourcesDAO resourcesDAO
         */
        public MapsDAO(ResourcesDAO resourcesDAO)
        {
            this.resourcesDAO = resourcesDAO;
        }

        public void insertMaps(List<MapDownloadResource> maps, Dictionary<string, string> mapsItemsCodes, Dictionary<string, string> regionItemsCodes, Context applicationContext)
        {
            try
            {
                if ((maps != null) && (mapsItemsCodes != null) && (regionItemsCodes != null))
                {
                    // create a compile statement for inserting the maps using transactions
                    StringBuilder insertCommand = new StringBuilder("INSERT INTO ");
                    insertCommand.Append(MAPS_TABLE).Append(" VALUES (?");
                    // the number of columns in maps table is 20
                    for (int i = 0; i < 20; i++)
                    {
                        insertCommand.Append(",?");
                    }
                    insertCommand.Append(");");
                    resourcesDAO.getDatabase().BeginTransaction();
                    SQLiteStatement insertStatement = resourcesDAO.getDatabase().CompileStatement(insertCommand.ToString());
                    int columnIndex, lineIndex = 0;
                    foreach (MapDownloadResource map in maps)
                    {
                        columnIndex = 1;
                        lineIndex++;
                        insertStatement.ClearBindings();
                        insertStatement.BindLong(columnIndex++, lineIndex);
                        insertStatement.BindString(columnIndex++, map.Code);
                        insertStatement.BindString(columnIndex++, mapsItemsCodes[map.Code]);
                        if ((map.getSubType() != null) && map.getSubType().Equals(STATE_TYPE)) //ignorecode
                        {
                            insertStatement.BindString(columnIndex++, regionItemsCodes[map.Code]);
                        }
                        else
                        {
                            insertStatement.BindString(columnIndex++, "");
                        }

                        // compute the string that contains all the name translations
                        StringBuilder nameInAllSpecifiedLanguages = new StringBuilder();

                        if (map.getNames() != null)
                        {
                            foreach (var currentEntry in map.getNames())
                            {
                                nameInAllSpecifiedLanguages.Append(currentEntry.Key).Append("=").Append(currentEntry.Value).Append(";");
                            }
                        }

                        if (nameInAllSpecifiedLanguages.Length > 1)
                        {
                            insertStatement.BindString(columnIndex++, nameInAllSpecifiedLanguages.ToString().Substring(0, nameInAllSpecifiedLanguages.Length - 1));
                        }
                        else
                        {
                            insertStatement.BindString(columnIndex++, "");
                        }
                        insertStatement.BindString(columnIndex++, map.getSKMFilePath());
                        insertStatement.BindString(columnIndex++, map.getZipFilePath());
                        insertStatement.BindString(columnIndex++, map.getTXGFilePath());
                        insertStatement.BindLong(columnIndex++, (int)map.getTXGFileSize());
                        insertStatement.BindLong(columnIndex++, (int)map.getSkmAndZipFilesSize());
                        insertStatement.BindLong(columnIndex++, (int)map.getSkmFileSize());
                        insertStatement.BindLong(columnIndex++, (int)map.getUnzippedFileSize());
                        insertStatement.BindDouble(columnIndex++, map.getBbLatMax());
                        insertStatement.BindDouble(columnIndex++, map.getBbLatMin());
                        insertStatement.BindDouble(columnIndex++, map.getBbLongMax());
                        insertStatement.BindDouble(columnIndex++, map.getBbLongMin());
                        insertStatement.BindString(columnIndex++, map.getSubType());
                        insertStatement.BindLong(columnIndex++, map.DownloadState);
                        insertStatement.BindLong(columnIndex++, map.NoDownloadedBytes);
                        insertStatement.BindLong(columnIndex++, 0);
                        insertStatement.BindString(columnIndex, map.DownloadPath);

                        insertStatement.Execute();
                    }
                }
            }
            finally
            {
                if ((maps != null) && (mapsItemsCodes != null))
                {
                    SKLogging.WriteLog(TAG, "Maps were inserted into database !!!", SKLogging.LogDebug);
                    // close the GENERAL transaction
                    resourcesDAO.getDatabase().SetTransactionSuccessful();
                    resourcesDAO.getDatabase().EndTransaction();
                }
            }
        }

        public void deleteMaps()
        {
            String deleteCommand = "DELETE FROM " + MAPS_TABLE;
            SQLiteStatement deleteStatement = resourcesDAO.getDatabase().CompileStatement(deleteCommand.ToString());
            deleteStatement.Execute();
        }

        public Dictionary<string, MapDownloadResource> getAvailableMapsForACertainType(params string[] mapType)
        {
            StringBuilder query =
                   new StringBuilder("SELECT ").Append(CODE).Append(", ").Append(PARENT_CODE).Append(", ").Append(REGION).Append(", ")
                           .Append(NAMES).Append(", ").Append(SKM_FILE_PATH).Append(", " +
                           "").Append(ZIP_FILE_PATH).Append(", ")
                           .Append(TXG_FILE_PATH).Append(", ").Append(TXG_FILE_SIZE).Append(", ")
                           .Append(SKM_AND_ZIP_FILES_SIZE).Append(", ").Append(SKM_FILE_SIZE).Append(", " +
                           "").Append(UNZIPPED_FILE_SIZE)
                           .Append(", ").Append(BOUNDING_BOX_LATITUDE_MAX).Append(", ").Append(BOUNDING_BOX_LATITUDE_MIN)
                           .Append(", ").Append(BOUNDING_BOX_LONGITUDE_MAX).Append(", ").Append(BOUNDING_BOX_LONGITUDE_MIN)
                           .Append(", ").Append(SUBTYPE).Append(", ").Append(STATE).Append(", " +
                           "").Append(NO_DOWNLOADED_BYTES)
                           .Append(", ").Append(FLAG_ID).Append(", ").Append(DOWNLOAD_PATH).Append(" FROM ").Append
                           (MAPS_TABLE);

            if (mapType != null && mapType.Length > 0)
            {
                query.Append(" WHERE ").Append(SUBTYPE).Append("=?");

                for (int i = 1; i < mapType.Length; i++)
                {
                    query.Append(" or ").Append(SUBTYPE).Append("=?");
                }
            }

            ICursor resultCursor = resourcesDAO.getDatabase().RawQuery(query.ToString(), mapType);
            if ((resultCursor != null) && (resultCursor.Count > 0))
            {
                Dictionary<String, MapDownloadResource> maps = new Dictionary<String, MapDownloadResource>();
                MapDownloadResource currentMap;
                try
                {
                    resultCursor.MoveToFirst();
                    while (!resultCursor.IsAfterLast)
                    {
                        currentMap = new MapDownloadResource();
                        currentMap.Code = (resultCursor.GetString(0));
                        currentMap.ParentCode = (resultCursor.GetString(1));
                        currentMap.setNames(resultCursor.GetString(3));
                        currentMap.setSkmFilePath(resultCursor.GetString(4));
                        currentMap.setZipFilePath(resultCursor.GetString(5));
                        currentMap.setTXGFilePath(resultCursor.GetString(6));
                        currentMap.setTXGFileSize(resultCursor.GetInt(7));
                        currentMap.setSkmAndZipFilesSize(resultCursor.GetInt(8));
                        currentMap.setSkmFileSize(resultCursor.GetInt(9));
                        currentMap.setUnzippedFileSize(resultCursor.GetInt(10));
                        currentMap.setBbLatMax(resultCursor.GetDouble(11));
                        currentMap.setBbLatMin(resultCursor.GetDouble(12));
                        currentMap.setBbLongMax(resultCursor.GetDouble(13));
                        currentMap.setBbLongMin(resultCursor.GetDouble(14));
                        currentMap.setSubType(resultCursor.GetString(15));
                        currentMap.DownloadState = (sbyte)resultCursor.GetInt(16);
                        currentMap.NoDownloadedBytes = (resultCursor.GetInt(17));
                        currentMap.FlagId = (resultCursor.GetInt(18));
                        currentMap.DownloadPath = (resultCursor.GetString(19));
                        maps.Add(currentMap.Code, currentMap);
                        resultCursor.MoveToNext();
                    }
                }
                finally
                {
                    resultCursor.Close();
                }

                return maps;
            }
            else
            {
                if (resultCursor != null)
                {
                    resultCursor.Close();
                }
                return null;
            }
        }

        public void updateMapResource(MapDownloadResource mapResource)
        {
            ContentValues values = new ContentValues();
            values.Put(STATE, mapResource.DownloadState);
            values.Put(NO_DOWNLOADED_BYTES, mapResource.NoDownloadedBytes);
            values.Put(SKM_FILE_PATH, mapResource.getSKMFilePath());
            values.Put(SKM_FILE_SIZE, mapResource.getSkmFileSize());
            values.Put(TXG_FILE_PATH, mapResource.getTXGFilePath());
            values.Put(TXG_FILE_SIZE, mapResource.getTXGFileSize());
            values.Put(ZIP_FILE_PATH, mapResource.getZipFilePath());
            values.Put(SKM_AND_ZIP_FILES_SIZE, mapResource.getSkmAndZipFilesSize());
            values.Put(UNZIPPED_FILE_SIZE, mapResource.getUnzippedFileSize());
            values.Put(DOWNLOAD_PATH, mapResource.DownloadPath);
            try
            {
                resourcesDAO.getDatabase().BeginTransaction();
                resourcesDAO.getDatabase().Update(MAPS_TABLE, values, CODE + "=?", new String[] { mapResource.Code });
                resourcesDAO.getDatabase().SetTransactionSuccessful();
            }
            catch (SQLException e)
            {
                SKLogging.WriteLog(TAG, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LogError);
            }
            finally
            {
                resourcesDAO.getDatabase().EndTransaction();
            }
        }

        public void clearResourcesInDownloadQueue()
        {
            ContentValues values = new ContentValues();
            values.Put(STATE, SKToolsDownloadItem.NotQueued);
            values.Put(NO_DOWNLOADED_BYTES, 0);
            try
            {
                resourcesDAO.getDatabase().BeginTransaction();
                resourcesDAO.getDatabase().Update(MAPS_TABLE, values, STATE + "=? OR " + STATE + "=? OR " + STATE + "=?", new string[] { SKToolsDownloadItem.Downloading.ToString(), SKToolsDownloadItem.Paused.ToString(), SKToolsDownloadItem.Queued.ToString() });
                resourcesDAO.getDatabase().SetTransactionSuccessful();
            }
            catch (SQLException e)
            {
                SKLogging.WriteLog(TAG, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LogError);
            }
            finally
            {
                resourcesDAO.getDatabase().EndTransaction();
            }
        }
    }
}