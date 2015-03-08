using Java.IO;
using Org.Json;
using Skobbler.Ngx.Util;
using Skobbler.SDKDemo.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Console = System.Console;
using StringWriter = Java.IO.StringWriter;

namespace Skobbler.SDKDemo.Database
{

	/// <summary>
	/// This class provides methods for parsing the "Maps" json file
	/// </summary>
	public class SecondMapDataParser
	{

		/// <summary>
		/// names for maps items tags
		/// </summary>
		private const string REGIONS_ID = "regions";

		private const string REGION_CODE_ID = "regionCode";

		private const string SUB_REGIONS_ID = "subRegions";

		private const string SUB_REGION_CODE_ID = "subRegionCode";

		private const string PACKAGES_ID = "packages";

		private const string PACKAGE_CODE_ID = "packageCode";

		private const string FILE_ID = "file";

		private const string SIZE_ID = "size";

		private const string UNZIP_SIZE_ID = "unzipsize";

		private const string TYPE_ID = "type";

		private const string LANGUAGES_ID = "languages";

		private const string TL_NAME_ID = "tlName";

		private const string LNG_CODE_ID = "lngCode";

		private const string BBOX_ID = "bbox";

		private const string LAT_MIN_ID = "latMin";

		private const string LAT_MAX_ID = "latMax";

		private const string LONG_MIN_ID = "longMin";

		private const string LONG_MAX_ID = "longMax";

		private const string SKM_SIZE_ID = "skmsize";

		private const string NB_ZIP_ID = "nbzip";

		private const string TEXTURE_ID = "texture";

		private const string TEXTURES_BIG_FILE_ID = "texturesbigfile";

		private const string SIZE_BIG_FILE_ID = "sizebigfile";

		private const string WORLD_ID = "world";

		private const string CONTINENTS_ID = "continents";

		private const string COUNTRIES_ID = "countries";

		private const string CONTINENT_CODE_ID = "continentCode";

		private const string COUNTRY_CODE_ID = "countryCode";

		private const string CITY_CODES_ID = "cityCodes";

		private const string CITY_CODE_ID = "cityCode";

		private const string STATE_CODES_ID = "stateCodes";

		private const string STATE_CODE_ID = "stateCode";

		private const string TAG = "SKToolsMapDataParser";

		/// <summary>
		/// parses maps JSON data </summary>
		/// <param name="maps"> a list of SKToolsDownloadResource items that represents the maps defined in JSON file </param>
		/// <param name="mapsItemsCodes"> a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="inputStream"> input stream from JSON file </param>
		/// <exception cref="java.io.IOException"> </exception>
		public virtual void parseMapJsonData(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, System.IO.Stream inputStream)
		{
			Console.WriteLine("Catalin ; start parsing !!!");
			long startTime = DateTimeUtil.JavaTime();
			JSONObject reader = new JSONObject(convertJSONFileContentToAString(inputStream));
			JSONArray regionsArray = reader.GetJSONArray(REGIONS_ID);
			if (regionsArray != null)
			{
				readUSRegionsHierarchy(regionItemsCodes, regionsArray);
			}
			JSONArray packagesArray = reader.GetJSONArray(PACKAGES_ID);
			if (packagesArray != null)
			{
				readMapsPackages(maps, packagesArray);
			}
			JSONObject worldObject = reader.GetJSONObject(WORLD_ID);
			if (worldObject != null)
			{
				JSONArray continentsArray = worldObject.GetJSONArray(CONTINENTS_ID);
				if (continentsArray != null)
				{
					readWorldHierarchy(mapsItemsCodes, continentsArray);
				}
			}
			/*-for (Map.Entry<String, String> currentEntry : mapsItemsCodes.entrySet()) {
			    System.out.println("Catalin ; key = " + currentEntry.getKey() + " ; value = " + currentEntry.getValue());
			}*/
			Console.WriteLine("Catalin ; total loading time = " + (DateTimeUtil.JavaTime() - startTime) + " ; maps size = " + maps.Count);
		}

		/// <summary>
		/// read the JSON file and converts it to String using StringWriter </summary>
		/// <param name="inputStream"> JSON file stream </param>
		/// <exception cref="java.io.IOException"> </exception>
		private string convertJSONFileContentToAString(System.IO.Stream inputStream)
		{
			char[] buffer = new char[1024];
			Writer stringWriter = new StringWriter();
			try
			{
				var bufferedReader = new StreamReader(inputStream, Encoding.UTF8);
				int n;
                //while ((n = bufferedReader.Read(buffer)) != -1)
                //{
                //    stringWriter.Write(buffer, 0, n);
                //}
			}
			finally
			{
				stringWriter.Close();
			}
			return stringWriter.ToString();
		}

		/// <summary>
		/// read maps packages list </summary>
		/// <param name="maps"> a list of maps objects that will be read from JSON file </param>
		/// <param name="packagesArray"> packages array </param>
		private void readMapsPackages(IList<MapDownloadResource> maps, JSONArray packagesArray)
		{
			for (int i = 0; i < packagesArray.Length(); i++)
			{
				JSONObject currentPackageObject = null;
				try
				{
					currentPackageObject = packagesArray.GetJSONObject(i);
				}
				catch (JSONException ex)
				{
					SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
				}
				if (currentPackageObject != null)
				{
					MapDownloadResource currentMap = new MapDownloadResource();
					try
					{
						currentMap.Code = currentPackageObject.GetString(PACKAGE_CODE_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SubType = getMapType(currentPackageObject.GetInt(TYPE_ID));
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONArray currentMapNames = currentPackageObject.GetJSONArray(LANGUAGES_ID);
						if (currentMapNames != null)
						{
							for (int j = 0; j < currentMapNames.Length(); j++)
							{
								JSONObject currentMapNameObject = currentMapNames.GetJSONObject(j);
								if (currentMapNameObject != null)
								{
									string currentMapName = currentMapNameObject.GetString(TL_NAME_ID);
									if (currentMapName != null)
									{
										currentMap.setName(currentMapName, currentMapNameObject.GetString(LNG_CODE_ID));
									}
								}
							}
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONObject currentMapBoundingBox = currentPackageObject.GetJSONObject(BBOX_ID);
						if (currentMapBoundingBox != null)
						{
							currentMap.BbLatMax = currentMapBoundingBox.GetDouble(LAT_MAX_ID);
							currentMap.BbLatMin = currentMapBoundingBox.GetDouble(LAT_MIN_ID);
							currentMap.BbLongMax = currentMapBoundingBox.GetDouble(LONG_MAX_ID);
							currentMap.BbLongMin = currentMapBoundingBox.GetDouble(LONG_MIN_ID);
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmFileSize = currentPackageObject.GetLong(SKM_SIZE_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmFilePath = currentPackageObject.GetString(FILE_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.ZipFilePath = currentPackageObject.GetString(NB_ZIP_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.UnzippedFileSize = currentPackageObject.GetLong(UNZIP_SIZE_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONObject currentMapTXGDetails = currentPackageObject.GetJSONObject(TEXTURE_ID);
						if (currentMapTXGDetails != null)
						{
							currentMap.TXGFilePath = currentMapTXGDetails.GetString(TEXTURES_BIG_FILE_ID);
							currentMap.TXGFileSize = currentMapTXGDetails.GetLong(SIZE_BIG_FILE_ID);
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmAndZipFilesSize = currentPackageObject.GetLong(SIZE_ID);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
					}
					if ((currentMap.Code != null) && (currentMap.SubType != null))
					{
						removeNullValuesIfExist(currentMap);
						maps.Add(currentMap);
					}
				}
			}
		}

		/// <summary>
		/// read US regions hierarchy </summary>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="regionsArray"> regions array </param>
		/// <exception cref="org.json.JSONException"> </exception>
		private void readUSRegionsHierarchy(IDictionary<string, string> regionItemsCodes, JSONArray regionsArray)
		{
			for (int i = 0; i < regionsArray.Length(); i++)
			{
				JSONObject currentRegionObject = regionsArray.GetJSONObject(i);
				if (currentRegionObject != null)
				{
					string currentRegionCode = currentRegionObject.GetString(REGION_CODE_ID);
					if (currentRegionCode != null)
					{
						JSONArray subRegions = currentRegionObject.GetJSONArray(SUB_REGIONS_ID);
						if (subRegions != null)
						{
							for (int j = 0; j < subRegions.Length(); j++)
							{
								JSONObject currentSubRegionObject = subRegions.GetJSONObject(j);
								if (currentSubRegionObject != null)
								{
									string subRegionCode = currentSubRegionObject.GetString(SUB_REGION_CODE_ID);
									if (subRegionCode != null)
									{
										regionItemsCodes[subRegionCode] = currentRegionCode;
									}
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// read world hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="continentsArray"> continents array </param>
		private void readWorldHierarchy(IDictionary<string, string> mapsItemsCodes, JSONArray continentsArray)
		{
			for (int i = 0; i < continentsArray.Length(); i++)
			{
				try
				{
					JSONObject currentContinentObject = continentsArray.GetJSONObject(i);
					if (currentContinentObject != null)
					{
						try
						{
							string currentContinentCode = currentContinentObject.GetString(CONTINENT_CODE_ID);
							if (currentContinentCode != null)
							{
								mapsItemsCodes[currentContinentCode] = "";
								JSONArray countriesArray = null;
								try
								{
									countriesArray = currentContinentObject.GetJSONArray(COUNTRIES_ID);
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
								}
								if (countriesArray != null)
								{
									readCountriesHierarchy(mapsItemsCodes, currentContinentCode, countriesArray);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read countries hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentContinentCode"> current continent code </param>
		/// <param name="countriesArray"> countries array </param>
		private void readCountriesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentContinentCode, JSONArray countriesArray)
		{
			for (int i = 0; i < countriesArray.Length(); i++)
			{
				try
				{
					JSONObject currentCountryObject = countriesArray.GetJSONObject(i);
					if (currentCountryObject != null)
					{
						try
						{
							string currentCountryCode = currentCountryObject.GetString(COUNTRY_CODE_ID);
							if ((currentContinentCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentCountryCode] = currentContinentCode;
								try
								{
									JSONArray citiesArray = currentCountryObject.GetJSONArray(CITY_CODES_ID);
									if (citiesArray != null)
									{
										readCitiesHierarchy(mapsItemsCodes, currentCountryCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
								}
								try
								{
									JSONArray statesArray = currentCountryObject.GetJSONArray(STATE_CODES_ID);
									if (statesArray != null)
									{
										readStatesHierarchy(mapsItemsCodes, currentCountryCode, statesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read states hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentCountryCode"> current country code </param>
		/// <param name="statesArray"> states array </param>
		private void readStatesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentCountryCode, JSONArray statesArray)
		{
			for (int i = 0; i < statesArray.Length(); i++)
			{
				try
				{
					JSONObject currentStateObject = statesArray.GetJSONObject(i);
					if (currentStateObject != null)
					{
						try
						{
							string currentStateCode = currentStateObject.GetString(STATE_CODE_ID);
							if ((currentStateCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentStateCode] = currentCountryCode;
								try
								{
									JSONArray citiesArray = currentStateObject.GetJSONArray(CITY_CODES_ID);
									if (citiesArray != null)
									{
										readCitiesHierarchy(mapsItemsCodes, currentStateCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read cities hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentParentCode"> current parent code </param>
		/// <param name="citiesArray"> cities array </param>
		private void readCitiesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentParentCode, JSONArray citiesArray)
		{
			for (int i = 0; i < citiesArray.Length(); i++)
			{
				try
				{
					JSONObject currentCityObject = citiesArray.GetJSONObject(i);
					if (currentCityObject != null)
					{
						try
						{
							string currentCityCode = currentCityObject.GetString(CITY_CODE_ID);
							if ((currentCityCode != null) && (currentParentCode != null))
							{
								mapsItemsCodes[currentCityCode] = currentParentCode;
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <param name="mapTypeInt"> an integer associated with map type </param>
		/// <returns> the String associated with map type </returns>
		private string getMapType(int mapTypeInt)
		{
			switch (mapTypeInt)
			{
				case 0:
					return MapsDAO.COUNTRY_TYPE;
				case 1:
					return MapsDAO.CITY_TYPE;
				case 2:
					return MapsDAO.CONTINENT_TYPE;
				case 3:
					return MapsDAO.REGION_TYPE;
				case 4:
					return MapsDAO.STATE_TYPE;
				default:
					return "";
			}
		}

		/// <summary>
		/// removes null attributes for current map </summary>
		/// <param name="currentMap"> current map that is parsed </param>
		private void removeNullValuesIfExist(MapDownloadResource currentMap)
		{
			if (currentMap.ParentCode == null)
			{
				currentMap.ParentCode = "";
			}
			if (currentMap.DownloadPath == null)
			{
				currentMap.DownloadPath = "";
			}
			if (currentMap.SKMFilePath == null)
			{
				currentMap.SkmFilePath = "";
			}
			if (currentMap.ZipFilePath == null)
			{
				currentMap.ZipFilePath = "";
			}
			if (currentMap.TXGFilePath == null)
			{
				currentMap.TXGFilePath = "";
			}
		}
	}
}