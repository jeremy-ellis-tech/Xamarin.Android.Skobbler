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
	/// <summary>
	/// This class provides methods for accessing the "Maps" table
	/// </summary>
	public class MapsDao
	{

		/// <summary>
		/// US country code
		/// </summary>
		public const string UsCode = "US";

		/// <summary>
		/// ENGLISH language code
		/// </summary>
		public const string EnglishLanguageCode = "en";

		/// <summary>
		/// name of the maps data table
		/// </summary>
		public const string MapsTable = "Maps";

		/// <summary>
		/// map code column key
		/// </summary>
		public const string Code = "Code";

		/// <summary>
		/// map parent code column key
		/// </summary>
		public const string ParentCode = "ParentCode";

		/// <summary>
		/// region column key (has a value only for state column)
		/// </summary>
		public const string Region = "Region";

		/// <summary>
		/// map names column key
		/// </summary>
		public const string Names = "Names";

		/// <summary>
		/// map SKM file path column key
		/// </summary>
		public const string SkmFilePath = "SkmFilePath";

		/// <summary>
		/// map ZIP file path column key
		/// </summary>
		public const string ZipFilePath = "ZipFilePath";

		/// <summary>
		/// map TXG file path column key
		/// </summary>
		public const string TxgFilePath = "TxgFilePath";

		/// <summary>
		/// map TXG file size column key
		/// </summary>
		public const string TxgFileSize = "TxgFileSize";

		/// <summary>
		/// map total size(SKM + ZIP) files size column key
		/// </summary>
		public const string SkmAndZipFilesSize = "SkmAndZipFilesSize";

		/// <summary>
		/// map SKM file size column key
		/// </summary>
		public const string SkmFileSize = "SkmFileSize";

		/// <summary>
		/// map UNZIPPED file size column key
		/// </summary>
		public const string UnzippedFileSize = "UnzippedFileSize";

		/// <summary>
		/// map sub-type column key
		/// </summary>
		public const string Subtype = "SubType";

		/// <summary>
		/// map state column key
		/// </summary>
		public const string State = "State";

		/// <summary>
		/// Bounding box column keys
		/// </summary>
		public const string BoundingBoxLongitudeMin = "LongMin";

		public const string BoundingBoxLongitudeMax = "LongMax";

		public const string BoundingBoxLatitudeMin = "LatMin";

		public const string BoundingBoxLatitudeMax = "LatMax";

		/// <summary>
		/// map no bytes column key
		/// </summary>
		public const string NoDownloadedBytes = "NoDownloadedBytes";

		/// <summary>
		/// flag ID
		/// </summary>
		public const string FlagId = "FlagID";

		/// <summary>
		/// download path
		/// </summary>
		public const string DownloadPath = "DownloadPath";

		/// <summary>
		/// Prefix for the flag image resources
		/// </summary>
		public const string FlagBigIconPrefix = "icon_flag_big_";

		/// <summary>
		/// map type column values
		/// </summary>
		public const string ContinentType = "continent";

		public const string CountryType = "country";

		public const string RegionType = "region";

		public const string CityType = "city";

		public const string StateType = "state";

		/// <summary>
		/// tag for the class
		/// </summary>
		private const string Tag = "MapsDAO";

		/// <summary>
		/// auto-increment column key (primary key ID)
		/// </summary>
		public const string Key = "Key";

		/// <summary>
		/// the associated resources DAO
		/// </summary>
		private readonly ResourcesDao _resourcesDao;

		/// <summary>
		/// constructs an object of this type
		/// </summary>
		/// <param name="resourcesDao"> resourcesDAO </param>
		public MapsDao(ResourcesDao resourcesDao)
		{
			this._resourcesDao = resourcesDao;
		}

		/// <summary>
		/// insert the maps and codes into resources database (maps table)
		/// </summary>
		/// <param name="maps">               map objects that will be inserted into database </param>
		/// <param name="mapsItemsCodes">     a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes">   a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="applicationContext"> application context </param>
		public virtual void InsertMaps(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, Context applicationContext)
		{
			try
			{
				if ((maps != null) && (mapsItemsCodes != null) && (regionItemsCodes != null))
				{
					// create a compile statement for inserting the maps using transactions
					StringBuilder insertCommand = new StringBuilder("INSERT INTO ");
					insertCommand.Append(MapsTable).Append(" VALUES (?");
					// the number of columns in maps table is 20
					for (int i = 0; i < 20; i++)
					{
						insertCommand.Append(",?");
					}
					insertCommand.Append(");");
					_resourcesDao.Database.BeginTransaction();
					SQLiteStatement insertStatement = _resourcesDao.Database.CompileStatement(insertCommand.ToString());
					int columnIndex , lineIndex = 0;
					foreach (MapDownloadResource map in maps)
					{
						columnIndex = 1;
						lineIndex++;
						insertStatement.ClearBindings();
						insertStatement.BindLong(columnIndex++, lineIndex);
						insertStatement.BindString(columnIndex++, map.Code);
						insertStatement.BindString(columnIndex++, mapsItemsCodes[map.Code]);
						if ((map.SubType != null) && map.SubType.Equals(StateType, StringComparison.CurrentCultureIgnoreCase))
						{
							insertStatement.BindString(columnIndex++, regionItemsCodes[map.Code]);
						}
						else
						{
							insertStatement.BindString(columnIndex++, "");
						}

						// compute the string that contains all the name translations
						StringBuilder nameInAllSpecifiedLanguages = new StringBuilder();

						if (map.GetNames() != null)
						{
							foreach (KeyValuePair<string, string> currentEntry in map.GetNames())
							{
								nameInAllSpecifiedLanguages.Append(currentEntry.Key).Append("=").Append(currentEntry.Value).Append(";");
							}
						}

						if (nameInAllSpecifiedLanguages.Length > 1)
						{
							//insertStatement.BindString(columnIndex++, nameInAllSpecifiedLanguages.Substring(0, nameInAllSpecifiedLanguages.Length - 1));
						}
						else
						{
							insertStatement.BindString(columnIndex++, "");
						}
						insertStatement.BindString(columnIndex++, map.SKMFilePath);
                        insertStatement.BindString(columnIndex++, map.ZipFilePath);
                        insertStatement.BindString(columnIndex++, map.TxgFilePath);
						insertStatement.BindLong(columnIndex++, (int) map.TxgFileSize);
                        insertStatement.BindLong(columnIndex++, (int)map.SkmAndZipFilesSize);
                        insertStatement.BindLong(columnIndex++, (int)map.SkmFileSize);
                        insertStatement.BindLong(columnIndex++, (int)map.UnzippedFileSize);
						insertStatement.BindDouble(columnIndex++, map.BbLatMax);
                        insertStatement.BindDouble(columnIndex++, map.BbLatMin);
                        insertStatement.BindDouble(columnIndex++, map.BbLongMax);
                        insertStatement.BindDouble(columnIndex++, map.BbLongMin);
						insertStatement.BindString(columnIndex++, map.SubType);
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
					SKLogging.WriteLog(Tag, "Maps were inserted into database !!!", SKLogging.LogDebug);
					// close the GENERAL transaction
					_resourcesDao.Database.SetTransactionSuccessful();
					_resourcesDao.Database.EndTransaction();
				}
			}
		}

		/// <summary>
		/// get all maps from DB (countries, cities or us states)
		/// </summary>
		/// <returns> all maps of a certain type from database </returns>
		public virtual IDictionary<string, MapDownloadResource> GetAvailableMapsForACertainType(params string[] mapType)
		{
			StringBuilder query = (new StringBuilder("SELECT ")).Append(Code).Append(", ").Append(ParentCode).Append(", ").Append(Region).Append(", ").Append(Names).Append(", ").Append(SkmFilePath).Append(", " + "").Append(ZipFilePath).Append(", ").Append(TxgFilePath).Append(", ").Append(TxgFileSize).Append(", ").Append(SkmAndZipFilesSize).Append(", ").Append(SkmFileSize).Append(", " + "").Append(UnzippedFileSize).Append(", ").Append(BoundingBoxLatitudeMax).Append(", ").Append(BoundingBoxLatitudeMin).Append(", ").Append(BoundingBoxLongitudeMax).Append(", ").Append(BoundingBoxLongitudeMin).Append(", ").Append(Subtype).Append(", ").Append(State).Append(", " + "").Append(NoDownloadedBytes).Append(", ").Append(FlagId).Append(", ").Append(DownloadPath).Append(" FROM ").Append(MapsTable);
			if ((mapType != null) && (mapType.Length > 0))
			{
				query.Append(" WHERE ").Append(Subtype).Append("=?");
				for (int i = 1; i < mapType.Length; i++)
				{
					query.Append(" or ").Append(Subtype).Append("=?");
				}
			}
			ICursor resultCursor = _resourcesDao.Database.RawQuery(query.ToString(), mapType);
			if ((resultCursor != null) && (resultCursor.Count > 0))
			{
				IDictionary<string, MapDownloadResource> maps = new Dictionary<string, MapDownloadResource>();
				MapDownloadResource currentMap;
				try
				{
					resultCursor.MoveToFirst();
					while (!resultCursor.IsAfterLast)
					{
						currentMap = new MapDownloadResource();
						currentMap.Code = resultCursor.GetString(0);
                        currentMap.ParentCode = resultCursor.GetString(1);
                        currentMap.SetNames(resultCursor.GetString(3));
                        currentMap.SkmFilePath = resultCursor.GetString(4);
                        currentMap.ZipFilePath = resultCursor.GetString(5);
                        currentMap.TxgFilePath = resultCursor.GetString(6);
						currentMap.TxgFileSize = resultCursor.GetInt(7);
                        currentMap.SkmAndZipFilesSize = resultCursor.GetInt(8);
                        currentMap.SkmFileSize = resultCursor.GetInt(9);
                        currentMap.UnzippedFileSize = resultCursor.GetInt(10);
						currentMap.BbLatMax = resultCursor.GetDouble(11);
                        currentMap.BbLatMin = resultCursor.GetDouble(12);
                        currentMap.BbLongMax = resultCursor.GetDouble(13);
                        currentMap.BbLongMin = resultCursor.GetDouble(14);
						currentMap.SubType = resultCursor.GetString(15);
						currentMap.DownloadState = (sbyte) resultCursor.GetInt(16);
						currentMap.NoDownloadedBytes = resultCursor.GetInt(17);
						currentMap.FlagId = resultCursor.GetInt(18);
						currentMap.DownloadPath = resultCursor.GetString(19);
						maps[currentMap.Code] = currentMap;
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

		/// <summary>
		/// Updates the database record corresponding to the map resource given as parameter
		/// </summary>
		/// <param name="mapResource"> </param>
		public virtual void UpdateMapResource(MapDownloadResource mapResource)
		{
			ContentValues values = new ContentValues();
			values.Put(State, mapResource.DownloadState);
            values.Put(NoDownloadedBytes, mapResource.NoDownloadedBytes);
            values.Put(SkmFilePath, mapResource.SKMFilePath);
            values.Put(SkmFileSize, mapResource.SkmFileSize);
            values.Put(TxgFilePath, mapResource.TxgFilePath);
            values.Put(TxgFileSize, mapResource.TxgFileSize);
            values.Put(ZipFilePath, mapResource.ZipFilePath);
            values.Put(SkmAndZipFilesSize, mapResource.SkmAndZipFilesSize);
            values.Put(UnzippedFileSize, mapResource.UnzippedFileSize);
            values.Put(DownloadPath, mapResource.DownloadPath);
			try
			{
				_resourcesDao.Database.BeginTransaction();
				_resourcesDao.Database.Update(MapsTable, values, Code + "=?", new string[]{mapResource.Code});
				_resourcesDao.Database.SetTransactionSuccessful();
			}
			catch (SQLException e)
			{
				SKLogging.WriteLog(Tag, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LogError);
			}
			finally
			{
				_resourcesDao.Database.EndTransaction();
			}
		}

		/// <summary>
		/// Marks resources that are presently in the download queue as not queued in the database table
		/// </summary>
		public virtual void ClearResourcesInDownloadQueue()
		{
			ContentValues values = new ContentValues();
			values.Put(State, SKToolsDownloadItem.NotQueued);
			values.Put(NoDownloadedBytes, 0);
			try
			{
				_resourcesDao.Database.BeginTransaction();
				_resourcesDao.Database.Update(MapsTable, values, State + "=? OR " + State + "=? OR " + State + "=?", new string[]{Convert.ToString(SKToolsDownloadItem.Downloading), Convert.ToString(SKToolsDownloadItem.Paused), Convert.ToString(SKToolsDownloadItem.Queued)});
				_resourcesDao.Database.SetTransactionSuccessful();
			}
			catch (SQLException e)
			{
				SKLogging.WriteLog(Tag, "SQL EXCEPTION SAVE MAP DATA " + e.Message, SKLogging.LogError);
			}
			finally
			{
				_resourcesDao.Database.EndTransaction();
			}
		}
	}
}