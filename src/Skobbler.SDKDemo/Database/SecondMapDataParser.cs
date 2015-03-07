using System;
using System.Collections.Generic;
using System.Text;

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
			long startTime = DateTimeHelperClass.CurrentUnixTimeMillis();
			JSONObject reader = new JSONObject(convertJSONFileContentToAString(inputStream));
			JSONArray regionsArray = reader.getJSONArray(REGIONS_ID);
			if (regionsArray != null)
			{
				readUSRegionsHierarchy(regionItemsCodes, regionsArray);
			}
			JSONArray packagesArray = reader.getJSONArray(PACKAGES_ID);
			if (packagesArray != null)
			{
				readMapsPackages(maps, packagesArray);
			}
			JSONObject worldObject = reader.getJSONObject(WORLD_ID);
			if (worldObject != null)
			{
				JSONArray continentsArray = worldObject.getJSONArray(CONTINENTS_ID);
				if (continentsArray != null)
				{
					readWorldHierarchy(mapsItemsCodes, continentsArray);
				}
			}
			/*-for (Map.Entry<String, String> currentEntry : mapsItemsCodes.entrySet()) {
			    System.out.println("Catalin ; key = " + currentEntry.getKey() + " ; value = " + currentEntry.getValue());
			}*/
			Console.WriteLine("Catalin ; total loading time = " + (DateTimeUtil.CurrentUnixTimeMillis() - startTime) + " ; maps size = " + maps.Count);
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
				Reader bufferedReader = new System.IO.StreamReader(inputStream, Encoding.UTF8);
				int n;
				while ((n = bufferedReader.read(buffer)) != -1)
				{
					stringWriter.write(buffer, 0, n);
				}
			}
			finally
			{
				stringWriter.close();
			}
			return stringWriter.ToString();
		}

		/// <summary>
		/// read maps packages list </summary>
		/// <param name="maps"> a list of maps objects that will be read from JSON file </param>
		/// <param name="packagesArray"> packages array </param>
		private void readMapsPackages(IList<MapDownloadResource> maps, JSONArray packagesArray)
		{
			for (int i = 0; i < packagesArray.length(); i++)
			{
				JSONObject currentPackageObject = null;
				try
				{
					currentPackageObject = packagesArray.getJSONObject(i);
				}
				catch (JSONException ex)
				{
					SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
				}
				if (currentPackageObject != null)
				{
					MapDownloadResource currentMap = new MapDownloadResource();
					try
					{
						currentMap.Code = currentPackageObject.getString(PACKAGE_CODE_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.SubType = getMapType(currentPackageObject.getInt(TYPE_ID));
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						JSONArray currentMapNames = currentPackageObject.getJSONArray(LANGUAGES_ID);
						if (currentMapNames != null)
						{
							for (int j = 0; j < currentMapNames.length(); j++)
							{
								JSONObject currentMapNameObject = currentMapNames.getJSONObject(j);
								if (currentMapNameObject != null)
								{
									string currentMapName = currentMapNameObject.getString(TL_NAME_ID);
									if (currentMapName != null)
									{
										currentMap.setName(currentMapName, currentMapNameObject.getString(LNG_CODE_ID));
									}
								}
							}
						}
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						JSONObject currentMapBoundingBox = currentPackageObject.getJSONObject(BBOX_ID);
						if (currentMapBoundingBox != null)
						{
							currentMap.BbLatMax = currentMapBoundingBox.getDouble(LAT_MAX_ID);
							currentMap.BbLatMin = currentMapBoundingBox.getDouble(LAT_MIN_ID);
							currentMap.BbLongMax = currentMapBoundingBox.getDouble(LONG_MAX_ID);
							currentMap.BbLongMin = currentMapBoundingBox.getDouble(LONG_MIN_ID);
						}
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.SkmFileSize = currentPackageObject.getLong(SKM_SIZE_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.SkmFilePath = currentPackageObject.getString(FILE_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.ZipFilePath = currentPackageObject.getString(NB_ZIP_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.UnzippedFileSize = currentPackageObject.getLong(UNZIP_SIZE_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						JSONObject currentMapTXGDetails = currentPackageObject.getJSONObject(TEXTURE_ID);
						if (currentMapTXGDetails != null)
						{
							currentMap.TXGFilePath = currentMapTXGDetails.getString(TEXTURES_BIG_FILE_ID);
							currentMap.TXGFileSize = currentMapTXGDetails.getLong(SIZE_BIG_FILE_ID);
						}
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
					}
					try
					{
						currentMap.SkmAndZipFilesSize = currentPackageObject.getLong(SIZE_ID);
					}
					catch (JSONException ex)
					{
						SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
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
			for (int i = 0; i < regionsArray.length(); i++)
			{
				JSONObject currentRegionObject = regionsArray.getJSONObject(i);
				if (currentRegionObject != null)
				{
					string currentRegionCode = currentRegionObject.getString(REGION_CODE_ID);
					if (currentRegionCode != null)
					{
						JSONArray subRegions = currentRegionObject.getJSONArray(SUB_REGIONS_ID);
						if (subRegions != null)
						{
							for (int j = 0; j < subRegions.length(); j++)
							{
								JSONObject currentSubRegionObject = subRegions.getJSONObject(j);
								if (currentSubRegionObject != null)
								{
									string subRegionCode = currentSubRegionObject.getString(SUB_REGION_CODE_ID);
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
			for (int i = 0; i < continentsArray.length(); i++)
			{
				try
				{
					JSONObject currentContinentObject = continentsArray.getJSONObject(i);
					if (currentContinentObject != null)
					{
						try
						{
							string currentContinentCode = currentContinentObject.getString(CONTINENT_CODE_ID);
							if (currentContinentCode != null)
							{
								mapsItemsCodes[currentContinentCode] = "";
								JSONArray countriesArray = null;
								try
								{
									countriesArray = currentContinentObject.getJSONArray(COUNTRIES_ID);
								}
								catch (JSONException ex)
								{
									SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
								}
								if (countriesArray != null)
								{
									readCountriesHierarchy(mapsItemsCodes, currentContinentCode, countriesArray);
								}
							}
						}
						catch (JSONException ex)
						{
							SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
						}
					}
				}
				catch (JSONException ex)
				{
					SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
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
			for (int i = 0; i < countriesArray.length(); i++)
			{
				try
				{
					JSONObject currentCountryObject = countriesArray.getJSONObject(i);
					if (currentCountryObject != null)
					{
						try
						{
							string currentCountryCode = currentCountryObject.getString(COUNTRY_CODE_ID);
							if ((currentContinentCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentCountryCode] = currentContinentCode;
								try
								{
									JSONArray citiesArray = currentCountryObject.getJSONArray(CITY_CODES_ID);
									if (citiesArray != null)
									{
										readCitiesHierarchy(mapsItemsCodes, currentCountryCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
									SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
								}
								try
								{
									JSONArray statesArray = currentCountryObject.getJSONArray(STATE_CODES_ID);
									if (statesArray != null)
									{
										readStatesHierarchy(mapsItemsCodes, currentCountryCode, statesArray);
									}
								}
								catch (JSONException ex)
								{
									SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
								}
							}
						}
						catch (JSONException ex)
						{
							SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
						}
					}
				}
				catch (JSONException ex)
				{
					SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
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
			for (int i = 0; i < statesArray.length(); i++)
			{
				try
				{
					JSONObject currentStateObject = statesArray.getJSONObject(i);
					if (currentStateObject != null)
					{
						try
						{
							string currentStateCode = currentStateObject.getString(STATE_CODE_ID);
							if ((currentStateCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentStateCode] = currentCountryCode;
								try
								{
									JSONArray citiesArray = currentStateObject.getJSONArray(CITY_CODES_ID);
									if (citiesArray != null)
									{
										readCitiesHierarchy(mapsItemsCodes, currentStateCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
									SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
								}
							}
						}
						catch (JSONException ex)
						{
							SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
						}
					}
				}
				catch (JSONException ex)
				{
					SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
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
			for (int i = 0; i < citiesArray.length(); i++)
			{
				try
				{
					JSONObject currentCityObject = citiesArray.getJSONObject(i);
					if (currentCityObject != null)
					{
						try
						{
							string currentCityCode = currentCityObject.getString(CITY_CODE_ID);
							if ((currentCityCode != null) && (currentParentCode != null))
							{
								mapsItemsCodes[currentCityCode] = currentParentCode;
							}
						}
						catch (JSONException ex)
						{
							SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
						}
					}
				}
				catch (JSONException ex)
				{
					SKLogging.writeLog(TAG, ex.Message, SKLogging.LOG_DEBUG);
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