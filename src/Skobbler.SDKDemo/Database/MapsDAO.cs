using System;
using System.Collections.Generic;
using System.Text;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// This class provides methods for accessing the "Maps" table
	/// </summary>
	public class MapsDAO
	{

		/// <summary>
		/// US country code
		/// </summary>
		public const string US_CODE = "US";

		/// <summary>
		/// ENGLISH language code
		/// </summary>
		public const string ENGLISH_LANGUAGE_CODE = "en";

		/// <summary>
		/// name of the maps data table
		/// </summary>
		public const string MAPS_TABLE = "Maps";

		/// <summary>
		/// map code column key
		/// </summary>
		public const string CODE = "Code";

		/// <summary>
		/// map parent code column key
		/// </summary>
		public const string PARENT_CODE = "ParentCode";

		/// <summary>
		/// region column key (has a value only for state column)
		/// </summary>
		public const string REGION = "Region";

		/// <summary>
		/// map names column key
		/// </summary>
		public const string NAMES = "Names";

		/// <summary>
		/// map SKM file path column key
		/// </summary>
		public const string SKM_FILE_PATH = "SkmFilePath";

		/// <summary>
		/// map ZIP file path column key
		/// </summary>
		public const string ZIP_FILE_PATH = "ZipFilePath";

		/// <summary>
		/// map TXG file path column key
		/// </summary>
		public const string TXG_FILE_PATH = "TxgFilePath";

		/// <summary>
		/// map TXG file size column key
		/// </summary>
		public const string TXG_FILE_SIZE = "TxgFileSize";

		/// <summary>
		/// map total size(SKM + ZIP) files size column key
		/// </summary>
		public const string SKM_AND_ZIP_FILES_SIZE = "SkmAndZipFilesSize";

		/// <summary>
		/// map SKM file size column key
		/// </summary>
		public const string SKM_FILE_SIZE = "SkmFileSize";

		/// <summary>
		/// map UNZIPPED file size column key
		/// </summary>
		public const string UNZIPPED_FILE_SIZE = "UnzippedFileSize";

		/// <summary>
		/// map sub-type column key
		/// </summary>
		public const string SUBTYPE = "SubType";

		/// <summary>
		/// map state column key
		/// </summary>
		public const string STATE = "State";

		/// <summary>
		/// Bounding box column keys
		/// </summary>
		public const string BOUNDING_BOX_LONGITUDE_MIN = "LongMin";

		public const string BOUNDING_BOX_LONGITUDE_MAX = "LongMax";

		public const string BOUNDING_BOX_LATITUDE_MIN = "LatMin";

		public const string BOUNDING_BOX_LATITUDE_MAX = "LatMax";

		/// <summary>
		/// map no bytes column key
		/// </summary>
		public const string NO_DOWNLOADED_BYTES = "NoDownloadedBytes";

		/// <summary>
		/// flag ID
		/// </summary>
		public const string FLAG_ID = "FlagID";

		/// <summary>
		/// download path
		/// </summary>
		public const string DOWNLOAD_PATH = "DownloadPath";

		/// <summary>
		/// Prefix for the flag image resources
		/// </summary>
		public const string FLAG_BIG_ICON_PREFIX = "icon_flag_big_";

		/// <summary>
		/// map type column values
		/// </summary>
		public const string CONTINENT_TYPE = "continent";

		public const string COUNTRY_TYPE = "country";

		public const string REGION_TYPE = "region";

		public const string CITY_TYPE = "city";

		public const string STATE_TYPE = "state";

		/// <summary>
		/// tag for the class
		/// </summary>
		private const string TAG = "MapsDAO";

		/// <summary>
		/// auto-increment column key (primary key ID)
		/// </summary>
		public const string KEY = "Key";

		/// <summary>
		/// the associated resources DAO
		/// </summary>
		private readonly ResourcesDAO resourcesDAO;

		/// <summary>
		/// constructs an object of this type
		/// </summary>
		/// <param name="resourcesDAO"> resourcesDAO </param>
		public MapsDAO(ResourcesDAO resourcesDAO)
		{
			this.resourcesDAO = resourcesDAO;
		}

		/// <summary>
		/// insert the maps and codes into resources database (maps table)
		/// </summary>
		/// <param name="maps">               map objects that will be inserted into database </param>
		/// <param name="mapsItemsCodes">     a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes">   a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="applicationContext"> application context </param>
		public virtual void insertMaps(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, Context applicationContext)
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
					resourcesDAO.Database.beginTransaction();
					SQLiteStatement insertStatement = resourcesDAO.Database.compileStatement(insertCommand.ToString());
					int columnIndex , lineIndex = 0;
					foreach (MapDownloadResource map in maps)
					{
						columnIndex = 1;
						lineIndex++;
						insertStatement.clearBindings();
						insertStatement.bindLong(columnIndex++, lineIndex);
						insertStatement.bindString(columnIndex++, map.Code);
						insertStatement.bindString(columnIndex++, mapsItemsCodes[map.Code]);
						if ((map.SubType != null) && map.SubType.Equals(STATE_TYPE, StringComparison.CurrentCultureIgnoreCase))
						{
							insertStatement.bindString(columnIndex++, regionItemsCodes[map.Code]);
						}
						else
						{
							insertStatement.bindString(columnIndex++, "");
						}

						// compute the string that contains all the name translations
						StringBuilder nameInAllSpecifiedLanguages = new StringBuilder();

						if (map.getNames() != null)
						{
							foreach (KeyValuePair<string, string> currentEntry in map.getNames().SetOfKeyValuePairs())
							{
								nameInAllSpecifiedLanguages.Append(currentEntry.Key).Append("=").Append(currentEntry.Value).Append(";");
							}
						}

						if (nameInAllSpecifiedLanguages.Length > 1)
						{
							insertStatement.bindString(columnIndex++, nameInAllSpecifiedLanguages.Substring(0, nameInAllSpecifiedLanguages.Length - 1));
						}
						else
						{
							insertStatement.bindString(columnIndex++, "");
						}
						insertStatement.bindString(columnIndex++, map.SKMFilePath);
						insertStatement.bindString(columnIndex++, map.ZipFilePath);
						insertStatement.bindString(columnIndex++, map.TXGFilePath);
						insertStatement.bindLong(columnIndex++, (int) map.TXGFileSize);
						insertStatement.bindLong(columnIndex++, (int) map.SkmAndZipFilesSize);
						insertStatement.bindLong(columnIndex++, (int) map.SkmFileSize);
						insertStatement.bindLong(columnIndex++, (int) map.UnzippedFileSize);
						insertStatement.bindDouble(columnIndex++, map.BbLatMax);
						insertStatement.bindDouble(columnIndex++, map.BbLatMin);
						insertStatement.bindDouble(columnIndex++, map.BbLongMax);
						insertStatement.bindDouble(columnIndex++, map.BbLongMin);
						insertStatement.bindString(columnIndex++, map.SubType);
						insertStatement.bindLong(columnIndex++, map.DownloadState);
						insertStatement.bindLong(columnIndex++, map.NoDownloadedBytes);
						insertStatement.bindLong(columnIndex++, 0);
						insertStatement.bindString(columnIndex, map.DownloadPath);
						insertStatement.execute();
					}
				}
			}
			finally
			{
				if ((maps != null) && (mapsItemsCodes != null))
				{
					SKLogging.writeLog(TAG, "Maps were inserted into database !!!", SKLogging.LOG_DEBUG);
					// close the GENERAL transaction
					resourcesDAO.Database.setTransactionSuccessful();
					resourcesDAO.Database.endTransaction();
				}
			}
		}

		/// <summary>
		/// get all maps from DB (countries, cities or us states)
		/// </summary>
		/// <returns> all maps of a certain type from database </returns>
		public virtual IDictionary<string, MapDownloadResource> getAvailableMapsForACertainType(params string[] mapType)
		{
			StringBuilder query = (new StringBuilder("SELECT ")).Append(CODE).Append(", ").Append(PARENT_CODE).Append(", ").Append(REGION).Append(", ").Append(NAMES).Append(", ").Append(SKM_FILE_PATH).Append(", " + "").Append(ZIP_FILE_PATH).Append(", ").Append(TXG_FILE_PATH).Append(", ").Append(TXG_FILE_SIZE).Append(", ").Append(SKM_AND_ZIP_FILES_SIZE).Append(", ").Append(SKM_FILE_SIZE).Append(", " + "").Append(UNZIPPED_FILE_SIZE).Append(", ").Append(BOUNDING_BOX_LATITUDE_MAX).Append(", ").Append(BOUNDING_BOX_LATITUDE_MIN).Append(", ").Append(BOUNDING_BOX_LONGITUDE_MAX).Append(", ").Append(BOUNDING_BOX_LONGITUDE_MIN).Append(", ").Append(SUBTYPE).Append(", ").Append(STATE).Append(", " + "").Append(NO_DOWNLOADED_BYTES).Append(", ").Append(FLAG_ID).Append(", ").Append(DOWNLOAD_PATH).Append(" FROM ").Append(MAPS_TABLE);
			if ((mapType != null) && (mapType.Length > 0))
			{
				query.Append(" WHERE ").Append(SUBTYPE).Append("=?");
				for (int i = 1; i < mapType.Length; i++)
				{
					query.Append(" or ").Append(SUBTYPE).Append("=?");
				}
			}
			Cursor resultCursor = resourcesDAO.Database.rawQuery(query.ToString(), mapType);
			if ((resultCursor != null) && (resultCursor.Count > 0))
			{
				IDictionary<string, MapDownloadResource> maps = new Dictionary<string, MapDownloadResource>();
				MapDownloadResource currentMap;
				try
				{
					resultCursor.moveToFirst();
					while (!resultCursor.AfterLast)
					{
						currentMap = new MapDownloadResource();
						currentMap.Code = resultCursor.getString(0);
						currentMap.ParentCode = resultCursor.getString(1);
						currentMap.setNames(resultCursor.getString(3));
						currentMap.SkmFilePath = resultCursor.getString(4);
						currentMap.ZipFilePath = resultCursor.getString(5);
						currentMap.TXGFilePath = resultCursor.getString(6);
						currentMap.TXGFileSize = resultCursor.getInt(7);
						currentMap.SkmAndZipFilesSize = resultCursor.getInt(8);
						currentMap.SkmFileSize = resultCursor.getInt(9);
						currentMap.UnzippedFileSize = resultCursor.getInt(10);
						currentMap.BbLatMax = resultCursor.getDouble(11);
						currentMap.BbLatMin = resultCursor.getDouble(12);
						currentMap.BbLongMax = resultCursor.getDouble(13);
						currentMap.BbLongMin = resultCursor.getDouble(14);
						currentMap.SubType = resultCursor.getString(15);
						currentMap.DownloadState = (sbyte) resultCursor.getInt(16);
						currentMap.NoDownloadedBytes = resultCursor.getInt(17);
						currentMap.FlagID = resultCursor.getInt(18);
						currentMap.DownloadPath = resultCursor.getString(19);
						maps[currentMap.Code] = currentMap;
						resultCursor.moveToNext();
					}
				}
				finally
				{
					resultCursor.close();
				}

				return maps;
			}
			else
			{
				if (resultCursor != null)
				{
					resultCursor.close();
				}
				return null;
			}
		}

		/// <summary>
		/// Updates the database record corresponding to the map resource given as parameter
		/// </summary>
		/// <param name="mapResource"> </param>
		public virtual void updateMapResource(MapDownloadResource mapResource)
		{
			ContentValues values = new ContentValues();
			values.put(STATE, mapResource.DownloadState);
			values.put(NO_DOWNLOADED_BYTES, mapResource.NoDownloadedBytes);
			values.put(SKM_FILE_PATH, mapResource.SKMFilePath);
			values.put(SKM_FILE_SIZE, mapResource.SkmFileSize);
			values.put(TXG_FILE_PATH, mapResource.TXGFilePath);
			values.put(TXG_FILE_SIZE, mapResource.TXGFileSize);
			values.put(ZIP_FILE_PATH, mapResource.ZipFilePath);
			values.put(SKM_AND_ZIP_FILES_SIZE, mapResource.SkmAndZipFilesSize);
			values.put(UNZIPPED_FILE_SIZE, mapResource.UnzippedFileSize);
			values.put(DOWNLOAD_PATH, mapResource.DownloadPath);
			try
			{
				resourcesDAO.Database.beginTransaction();
				resourcesDAO.Database.update(MAPS_TABLE, values, CODE + "=?", new string[]{mapResource.Code});
				resourcesDAO.Database.setTransactionSuccessful();
			}
			catch (SQLException e)
			{
				SKLogging.writeLog(TAG, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LOG_ERROR);
			}
			finally
			{
				resourcesDAO.Database.endTransaction();
			}
		}

		/// <summary>
		/// Marks resources that are presently in the download queue as not queued in the database table
		/// </summary>
		public virtual void clearResourcesInDownloadQueue()
		{
			ContentValues values = new ContentValues();
			values.put(STATE, SKToolsDownloadItem.NOT_QUEUED);
			values.put(NO_DOWNLOADED_BYTES, 0);
			try
			{
				resourcesDAO.Database.beginTransaction();
				resourcesDAO.Database.update(MAPS_TABLE, values, STATE + "=? OR " + STATE + "=? OR " + STATE + "=?", new string[]{Convert.ToString(SKToolsDownloadItem.DOWNLOADING), Convert.ToString(SKToolsDownloadItem.PAUSED), Convert.ToString(SKToolsDownloadItem.QUEUED)});
				resourcesDAO.Database.setTransactionSuccessful();
			}
			catch (SQLException e)
			{
				SKLogging.writeLog(TAG, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LOG_ERROR);
			}
			finally
			{
				resourcesDAO.Database.endTransaction();
			}
		}
	}
}