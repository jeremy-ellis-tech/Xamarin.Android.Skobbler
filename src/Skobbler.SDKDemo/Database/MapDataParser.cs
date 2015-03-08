using System.Collections.Generic;
using System.Runtime.InteropServices;
using Android.Media;
using Android.Util;
using Java.IO;
using Java.Nio.Charset;
using Stream = System.IO.Stream;

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
		private const string RegionsId = "regions";

		private const string RegionCodeId = "regionCode";

		private const string SubRegionsId = "subRegions";

		private const string SubRegionCodeId = "subRegionCode";

		private const string VersionId = "version";

		private const string PackagesId = "packages";

		private const string PackageCodeId = "packageCode";

		private const string FileId = "file";

		private const string SizeId = "size";

		private const string UnzipSizeId = "unzipsize";

		private const string TypeId = "type";

		private const string LanguagesId = "languages";

		private const string TlNameId = "tlName";

		private const string LngCodeId = "lngCode";

		private const string BboxId = "bbox";

		private const string LatMinId = "latMin";

		private const string LatMaxId = "latMax";

		private const string LongMinId = "longMin";

		private const string LongMaxId = "longMax";

		private const string SkmSizeId = "skmsize";

		private const string NbZipId = "nbzip";

		private const string TextureId = "texture";

		private const string TexturesBigFileId = "texturesbigfile";

		private const string SizeBigFileId = "sizebigfile";

		private const string XmlVersionId = "xmlVersion";

		private const string WorldId = "world";

		private const string ContinentsId = "continents";

		private const string CountriesId = "countries";

		private const string ContinentCodeId = "continentCode";

		private const string CountryCodeId = "countryCode";

		private const string CityCodesId = "cityCodes";

		private const string CityCodeId = "cityCode";

		private const string StateCodesId = "stateCodes";

		private const string StateCodeId = "stateCode";

		/// <summary>
		/// parses maps JSON data </summary>
		/// <param name="maps"> a list of SKToolsDownloadResource items that represents the maps defined in JSON file </param>
		/// <param name="mapsItemsCodes"> a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="inputStream"> input stream from JSON file </param>
		/// <exception cref="java.io.IOException"> </exception>
		public virtual void ParseMapJsonData(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, Stream inputStream)
		{
            JsonReader reader = new JsonReader(new InputStreamReader(inputStream, "UTF-8"));
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(VersionId) || key.Equals(XmlVersionId))
					{
						reader.SkipValue();
					}
					else if (key.Equals(PackagesId))
					{
						ReadMapsDetails(maps, reader);
					}
					else if (key.Equals(WorldId))
					{
						reader.BeginObject();
					}
					else if (key.Equals(ContinentsId))
					{
						ReadWorldHierarchy(mapsItemsCodes, reader);
						reader.EndObject();
					}
					else if (key.Equals(RegionsId))
					{
						ReadRegionsDetails(regionItemsCodes, reader);
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
		private void ReadRegionsDetails(IDictionary<string, string> regionItemsCodes, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				ReadCurrentRegionDetails(regionItemsCodes, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read regions details list </summary>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void ReadCurrentRegionDetails(IDictionary<string, string> regionItemsCodes, JsonReader reader)
		{
			reader.BeginObject();
			string currentRegionCode = null;
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(RegionCodeId))
					{
						currentRegionCode = reader.NextString();
					}
					else if (key.Equals(SubRegionsId))
					{
						if (currentRegionCode != null)
						{
							ReadSubRegionsForCurrentRegion(regionItemsCodes, currentRegionCode, reader);
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
		private void ReadSubRegionsForCurrentRegion(IDictionary<string, string> regionItemsCodes, string currentRegionCode, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				reader.BeginObject();
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(SubRegionCodeId))
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
		private void ReadMapsDetails(IList<MapDownloadResource> maps, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				ReadCurrentMapDetails(maps, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read current map details </summary>
		/// <param name="maps"> a list of maps objects that will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void ReadCurrentMapDetails(IList<MapDownloadResource> maps, JsonReader reader)
		{
			MapDownloadResource currentMap = new MapDownloadResource();
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(PackageCodeId))
					{
						currentMap.Code = reader.NextString();
					}
					else if (key.Equals(TypeId))
					{
						currentMap.SubType = GetMapType(reader.NextInt());
					}
					else if (key.Equals(LanguagesId))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							ReadCurrentMapNames(currentMap, reader);
						}
						reader.EndArray();
					}
					else if (key.Equals(BboxId))
					{
						ReadCurrentMapBoundingBoxDetails(currentMap, reader);
					}
					else if (key.Equals(SkmSizeId))
					{
						currentMap.SkmFileSize = reader.NextLong();
					}
					else if (key.Equals(FileId))
					{
						currentMap.SkmFilePath = reader.NextString();
					}
					else if (key.Equals(NbZipId))
					{
						currentMap.ZipFilePath = reader.NextString();
					}
					else if (key.Equals(UnzipSizeId))
					{
						currentMap.UnzippedFileSize = reader.NextLong();
					}
					else if (key.Equals(TextureId))
					{
						ReadCurrentMapTxgDetails(currentMap, reader);
					}
					else if (key.Equals(SizeId))
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
				RemoveNullValuesIfExist(currentMap);
				maps.Add(currentMap);
			}
		}

		/// <summary>
		/// read current map names </summary>
		/// <param name="currentMap"> current map whose name will be read from JSON file </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void ReadCurrentMapNames(MapDownloadResource currentMap, JsonReader reader)
		{
			string currentMapName = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(TlNameId))
					{
						currentMapName = reader.NextString();
					}
					else if (key.Equals(LngCodeId))
					{
						if (currentMapName != null)
						{
							currentMap.SetName(currentMapName, reader.NextString());
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
		private void ReadCurrentMapTxgDetails(MapDownloadResource currentMap, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(TexturesBigFileId))
					{
						currentMap.TxgFilePath = reader.NextString();
					}
					else if (key.Equals(SizeBigFileId))
					{
						currentMap.TxgFileSize = reader.NextLong();
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
		private void ReadCurrentMapBoundingBoxDetails(MapDownloadResource currentMap, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(LatMaxId))
					{
						currentMap.BbLatMax = reader.NextDouble();
					}
					else if (key.Equals(LatMinId))
					{
						currentMap.BbLatMin = reader.NextDouble();
					}
					else if (key.Equals(LongMaxId))
					{
						currentMap.BbLongMax = reader.NextDouble();
					}
					else if (key.Equals(LongMinId))
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
		private void ReadWorldHierarchy(IDictionary<string, string> mapsItemsCodes, JsonReader reader)
		{
			reader.BeginArray();
			while (reader.HasNext)
			{
				ReadContinentsHierarchy(mapsItemsCodes, reader);
			}
			reader.EndArray();
		}

		/// <summary>
		/// read continents hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="reader"> JSON file reader </param>
		/// <exception cref="java.io.IOException"> </exception>
		private void ReadContinentsHierarchy(IDictionary<string, string> mapsItemsCodes, JsonReader reader)
		{
			string currentContinentCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(ContinentCodeId))
					{
						currentContinentCode = reader.NextString();
						if (currentContinentCode != null)
						{
							mapsItemsCodes[currentContinentCode] = "";
						}
					}
					else if (key.Equals(CountriesId))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							ReadCountriesHierarchy(mapsItemsCodes, currentContinentCode, reader);
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
		private void ReadCountriesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentContinentCode, JsonReader reader)
		{
			string currentCountryCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(CountryCodeId))
					{
						currentCountryCode = reader.NextString();
						if ((currentContinentCode != null) && (currentCountryCode != null))
						{
							mapsItemsCodes[currentCountryCode] = currentContinentCode;
						}
					}
					else if (key.Equals(CityCodesId))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							ReadCitiesHierarchy(mapsItemsCodes, currentCountryCode, reader);
						}
						reader.EndArray();
					}
					else if (key.Equals(StateCodesId))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							ReadStatesHierarchy(mapsItemsCodes, currentCountryCode, reader);
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
		private void ReadStatesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentCountryCode, JsonReader reader)
		{
			string currentStateCode = null;
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(StateCodeId))
					{
						currentStateCode = reader.NextString();
						if ((currentStateCode != null) && (currentCountryCode != null))
						{
							mapsItemsCodes[currentStateCode] = currentCountryCode;
						}
					}
					else if (key.Equals(CityCodesId))
					{
						reader.BeginArray();
						while (reader.HasNext)
						{
							ReadCitiesHierarchy(mapsItemsCodes, currentStateCode, reader);
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
		private void ReadCitiesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentParentCode, JsonReader reader)
		{
			reader.BeginObject();
			while (reader.HasNext)
			{
				string key = reader.NextName();
				if (key != null)
				{
					if (key.Equals(CityCodeId))
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
		private string GetMapType(int mapTypeInt)
		{
			switch (mapTypeInt)
			{
				case 0:
					return MapsDao.CountryType;
				case 1:
					return MapsDao.CityType;
				case 2:
					return MapsDao.ContinentType;
				case 3:
					return MapsDao.RegionType;
				case 4:
					return MapsDao.StateType;
				default:
					return "";
			}
		}

		/// <summary>
		/// removes null attributes for current map </summary>
		/// <param name="currentMap"> current map that is parsed </param>
		private void RemoveNullValuesIfExist(MapDownloadResource currentMap)
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
			if (currentMap.TxgFilePath == null)
			{
				currentMap.TxgFilePath = "";
			}
		}
	}
}