using Android.Util;
using System.Collections.Generic;
using System.Text;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// This class provides methods for parsing the "Maps" json file
	/// </summary>
	public class MapDataParser
	{

		/// <summary>
		/// names for maps items tags
		/// </summary>
		private const string REGIONS_ID = "regions";

		private const string REGION_CODE_ID = "regionCode";

		private const string SUB_REGIONS_ID = "subRegions";

		private const string SUB_REGION_CODE_ID = "subRegionCode";

		private const string VERSION_ID = "version";

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

		private const string XML_VERSION_ID = "xmlVersion";

		private const string WORLD_ID = "world";

		private const string CONTINENTS_ID = "continents";

		private const string COUNTRIES_ID = "countries";

		private const string CONTINENT_CODE_ID = "continentCode";

		private const string COUNTRY_CODE_ID = "countryCode";

		private const string CITY_CODES_ID = "cityCodes";

		private const string CITY_CODE_ID = "cityCode";

		private const string STATE_CODES_ID = "stateCodes";

		private const string STATE_CODE_ID = "stateCode";

		/// <summary>
		/// parses maps JSON data </summary>
		/// <param name="maps"> a list of SKToolsDownloadResource items that represents the maps defined in JSON file </param>
		/// <param name="mapsItemsCodes"> a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="inputStream"> input stream from JSON file </param>
		/// <exception cref="java.io.IOException"> </exception>
		public virtual void parseMapJsonData(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, System.IO.Stream inputStream)
		{
            JsonReader reader = null;// new JsonReader(new System.IO.StreamReader(inputStream, Encoding.UTF8));
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(VERSION_ID) || key.Equals(XML_VERSION_ID))
					{
						reader.SkipValue();
					}
					else if (key.Equals(PACKAGES_ID))
					{
						readMapsDetails(maps, reader);
					}
					else if (key.Equals(WORLD_ID))
					{
						reader.BeginObject();
					}
					else if (key.Equals(CONTINENTS_ID))
					{
						readWorldHierarchy(mapsItemsCodes, reader);
						reader.EndObject();
					}
					else if (key.Equals(REGIONS_ID))
					{
						readRegionsDetails(regionItemsCodes, reader);
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read regions details list </summary>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readRegionsDetails(IDictionary<string, string> regionItemsCodes, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				readCurrentRegionDetails(regionItemsCodes, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read regions details list </summary>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCurrentRegionDetails(IDictionary<string, string> regionItemsCodes, JsonReader reader)
		{
			reader.BeginObject();
			string currentRegionCode = null;
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(REGION_CODE_ID))
					{
						currentRegionCode = reader.NextString();
					}
					else if (key.Equals(SUB_REGIONS_ID))
					{
						if (currentRegionCode != null)
						{
							readSubRegionsForCurrentRegion(regionItemsCodes, currentRegionCode, reader);
						}
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read sub-regions for current region </summary>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="currentRegionCode"> current region code </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readSubRegionsForCurrentRegion(IDictionary<string, string> regionItemsCodes, string currentRegionCode, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				reader.BeginObject();
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(SUB_REGION_CODE_ID))
					{
						string subRegionCode = reader.NextString();
						if (subRegionCode != null)
						{
							regionItemsCodes[subRegionCode] = currentRegionCode;
						}
					}
				}
				reader.EndObject();
			}
			reader.EndArray();
		}

		/// <summary>
		/// read maps details list </summary>
		/// <param name="maps"> a list of maps objects that will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readMapsDetails(IList<MapDownloadResource> maps, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				readCurrentMapDetails(maps, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read current map details </summary>
		/// <param name="maps"> a list of maps objects that will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCurrentMapDetails(IList<MapDownloadResource> maps, JsonReader reader)
		{
			MapDownloadResource currentMap = new MapDownloadResource();
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(PACKAGE_CODE_ID))
					{
						currentMap.Code = reader.NextString();
					}
					else if (key.Equals(TYPE_ID))
					{
						currentMap.SubType = getMapType(reader.NextInt());
					}
					else if (key.Equals(LANGUAGES_ID))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							readCurrentMapNames(currentMap, reader);
						}
						reader.EndArray();
					}
					else if (key.Equals(BBOX_ID))
					{
						readCurrentMapBoundingBoxDetails(currentMap, reader);
					}
					else if (key.Equals(SKM_SIZE_ID))
					{
						currentMap.SkmFileSize = reader.NextLong();
					}
					else if (key.Equals(FILE_ID))
					{
						currentMap.SkmFilePath = reader.NextString();
					}
					else if (key.Equals(NB_ZIP_ID))
					{
						currentMap.ZipFilePath = reader.NextString();
					}
					else if (key.Equals(UNZIP_SIZE_ID))
					{
						currentMap.UnzippedFileSize = reader.NextLong();
					}
					else if (key.Equals(TEXTURE_ID))
					{
						readCurrentMapTXGDetails(currentMap, reader);
					}
					else if (key.Equals(SIZE_ID))
					{
						currentMap.SkmAndZipFilesSize = reader.NextLong();
					}
					else
					{
						// for now, we skip the elevation tag
						reader.SkipValue();
					}
				}
			}
			reader.EndObject();

			if ((currentMap.Code != null) && (currentMap.SubType != null))
			{
				removeNullValuesIfExist(currentMap);
				maps.Add(currentMap);
			}
		}

		/// <summary>
		/// read current map names </summary>
		/// <param name="currentMap"> current map whose name will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCurrentMapNames(MapDownloadResource currentMap, JsonReader reader)
		{
			string currentMapName = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(TL_NAME_ID))
					{
						currentMapName = reader.NextString();
					}
					else if (key.Equals(LNG_CODE_ID))
					{
						if (currentMapName != null)
						{
							currentMap.setName(currentMapName, reader.NextString());
						}
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read current map TXG details </summary>
		/// <param name="currentMap"> current map whose TXG details will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCurrentMapTXGDetails(MapDownloadResource currentMap, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(TEXTURES_BIG_FILE_ID))
					{
						currentMap.TXGFilePath = reader.NextString();
					}
					else if (key.Equals(SIZE_BIG_FILE_ID))
					{
						currentMap.TXGFileSize = reader.NextLong();
					}
					else
					{
						// for now, we skip the tags referring ZIP files details related to TXG files
						reader.SkipValue();
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read current map bounding box details </summary>
		/// <param name="currentMap"> current map whose bounding box will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCurrentMapBoundingBoxDetails(MapDownloadResource currentMap, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(LAT_MAX_ID))
					{
						currentMap.BbLatMax = reader.NextDouble();
					}
					else if (key.Equals(LAT_MIN_ID))
					{
						currentMap.BbLatMin = reader.NextDouble();
					}
					else if (key.Equals(LONG_MAX_ID))
					{
						currentMap.BbLongMax = reader.NextDouble();
					}
					else if (key.Equals(LONG_MIN_ID))
					{
						currentMap.BbLongMin = reader.NextDouble();
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read world hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readWorldHierarchy(IDictionary<string, string> mapsItemsCodes, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				readContinentsHierarchy(mapsItemsCodes, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read continents hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readContinentsHierarchy(IDictionary<string, string> mapsItemsCodes, JsonReader reader)
		{
			string currentContinentCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(CONTINENT_CODE_ID))
					{
						currentContinentCode = reader.NextString();
						if (currentContinentCode != null)
						{
							mapsItemsCodes[currentContinentCode] = "";
						}
					}
					else if (key.Equals(COUNTRIES_ID))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							readCountriesHierarchy(mapsItemsCodes, currentContinentCode, reader);
						}
						reader.EndArray();
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read countries hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentContinentCode"> current continent code </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCountriesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentContinentCode, JsonReader reader)
		{
			string currentCountryCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(COUNTRY_CODE_ID))
					{
						currentCountryCode = reader.NextString();
						if ((currentContinentCode != null) && (currentCountryCode != null))
						{
							mapsItemsCodes[currentCountryCode] = currentContinentCode;
						}
					}
					else if (key.Equals(CITY_CODES_ID))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							readCitiesHierarchy(mapsItemsCodes, currentCountryCode, reader);
						}
						reader.EndArray();
					}
					else if (key.Equals(STATE_CODES_ID))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							readStatesHierarchy(mapsItemsCodes, currentCountryCode, reader);
						}
						reader.EndArray();
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read states hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentCountryCode"> current country code </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readStatesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentCountryCode, JsonReader reader)
		{
			string currentStateCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(STATE_CODE_ID))
					{
						currentStateCode = reader.NextString();
						if ((currentStateCode != null) && (currentCountryCode != null))
						{
							mapsItemsCodes[currentStateCode] = currentCountryCode;
						}
					}
					else if (key.Equals(CITY_CODES_ID))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							readCitiesHierarchy(mapsItemsCodes, currentStateCode, reader);
						}
						reader.EndArray();
					}
				}
			}
			reader.EndObject();
		}

		/// <summary>
		/// read cities hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentParentCode"> current parent code </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void readCitiesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentParentCode, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(CITY_CODE_ID))
					{
						string currentCityCode = reader.NextString();
						if ((currentCityCode != null) && (currentParentCode != null))
						{
							mapsItemsCodes[currentCityCode] = currentParentCode;
						}
					}
				}
			}
			reader.EndObject();
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