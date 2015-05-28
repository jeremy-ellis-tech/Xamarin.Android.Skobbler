using System.Collections.Generic;
using System.IO;
using Android.Util;
using Java.IO;

namespace Skobbler.SDKDemo.Database
{
    public class MapDataParser
    {
        private static readonly string REGIONS_ID = "regions";

        private static readonly string REGION_CODE_ID = "regionCode";

        private static readonly string SUB_REGIONS_ID = "subRegions";

        private static readonly string SUB_REGION_CODE_ID = "subRegionCode";

        private static readonly string VERSION_ID = "version";

        private static readonly string PACKAGES_ID = "packages";

        private static readonly string PACKAGE_CODE_ID = "packageCode";

        private static readonly string FILE_ID = "file";

        private static readonly string SIZE_ID = "size";

        private static readonly string UNZIP_SIZE_ID = "unzipsize";

        private static readonly string TYPE_ID = "type";

        private static readonly string LANGUAGES_ID = "languages";

        private static readonly string TL_NAME_ID = "tlName";

        private static readonly string LNG_CODE_ID = "lngCode";

        private static readonly string BBOX_ID = "bbox";

        private static readonly string LAT_MIN_ID = "latMin";

        private static readonly string LAT_MAX_ID = "latMax";

        private static readonly string LONG_MIN_ID = "longMin";

        private static readonly string LONG_MAX_ID = "longMax";

        private static readonly string SKM_SIZE_ID = "skmsize";

        private static readonly string NB_ZIP_ID = "nbzip";

        private static readonly string TEXTURE_ID = "texture";

        private static readonly string TEXTURES_BIG_FILE_ID = "texturesbigfile";

        private static readonly string SIZE_BIG_FILE_ID = "sizebigfile";

        private static readonly string XML_VERSION_ID = "xmlVersion";

        private static readonly string WORLD_ID = "world";

        private static readonly string CONTINENTS_ID = "continents";

        private static readonly string COUNTRIES_ID = "countries";

        private static readonly string CONTINENT_CODE_ID = "continentCode";

        private static readonly string COUNTRY_CODE_ID = "countryCode";

        private static readonly string CITY_CODES_ID = "cityCodes";

        private static readonly string CITY_CODE_ID = "cityCode";

        private static readonly string STATE_CODES_ID = "stateCodes";

        private static readonly string STATE_CODE_ID = "stateCode";

        public void parseMapJsonData(List<MapDownloadResource> maps, Dictionary<string, string> mapsItemsCodes, Dictionary<string, string> regionItemsCodes, Stream inputStream)
        {
            JsonReader reader = new JsonReader(new InputStreamReader(inputStream, "UTF-8"));
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

        private void readRegionsDetails(Dictionary<string, string> regionItemsCodes, JsonReader reader)
        {
            reader.BeginArray();
            while (reader.HasNext)
            {
                readCurrentRegionDetails(regionItemsCodes, reader);
            }
            reader.EndArray();
        }

        private void readCurrentRegionDetails(Dictionary<string, string> regionItemsCodes, JsonReader reader)
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

        private void readSubRegionsForCurrentRegion(Dictionary<string, string> regionItemsCodes, string currentRegionCode, JsonReader reader)
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
                            regionItemsCodes.Add(subRegionCode, currentRegionCode);
                        }
                    }
                }
                reader.EndObject();
            }
            reader.EndArray();
        }

        private void readMapsDetails(List<MapDownloadResource> maps, JsonReader reader)
        {
            reader.BeginArray();
            while (reader.HasNext)
            {
                readCurrentMapDetails(maps, reader);
            }
            reader.EndArray();
        }

        private void readCurrentMapDetails(List<MapDownloadResource> maps, JsonReader reader)
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
                        currentMap.setSubType(getMapType(reader.NextInt()));
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
                        currentMap.setSkmFileSize(reader.NextLong());
                    }
                    else if (key.Equals(FILE_ID))
                    {
                        currentMap.setSkmFilePath(reader.NextString());
                    }
                    else if (key.Equals(NB_ZIP_ID))
                    {
                        currentMap.setZipFilePath(reader.NextString());
                    }
                    else if (key.Equals(UNZIP_SIZE_ID))
                    {
                        currentMap.setUnzippedFileSize(reader.NextLong());
                    }
                    else if (key.Equals(TEXTURE_ID))
                    {
                        readCurrentMapTXGDetails(currentMap, reader);
                    }
                    else if (key.Equals(SIZE_ID))
                    {
                        currentMap.setSkmAndZipFilesSize(reader.NextLong());
                    }
                    else
                    {
                        // for now, we skip the elevation tag
                        reader.SkipValue();
                    }
                }
            }
            reader.EndObject();

            if ((currentMap.Code != null) && (currentMap.getSubType() != null))
            {
                removeNullValuesIfExist(currentMap);
                maps.Add(currentMap);
            }
        }

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
                        currentMap.setTXGFilePath(reader.NextString());
                    }
                    else if (key.Equals(SIZE_BIG_FILE_ID))
                    {
                        currentMap.setTXGFileSize(reader.NextLong());
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
                        currentMap.setBbLatMax(reader.NextDouble());
                    }
                    else if (key.Equals(LAT_MIN_ID))
                    {
                        currentMap.setBbLatMin(reader.NextDouble());
                    }
                    else if (key.Equals(LONG_MAX_ID))
                    {
                        currentMap.setBbLongMax(reader.NextDouble());
                    }
                    else if (key.Equals(LONG_MIN_ID))
                    {
                        currentMap.setBbLongMin(reader.NextDouble());
                    }
                }
            }
            reader.EndObject();
        }

        private void readWorldHierarchy(Dictionary<string, string> mapsItemsCodes, JsonReader reader)
        {
            reader.BeginArray();
            while (reader.HasNext)
            {
                readContinentsHierarchy(mapsItemsCodes, reader);
            }
            reader.EndArray();
        }

        private void readContinentsHierarchy(Dictionary<string, string> mapsItemsCodes, JsonReader reader)
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
                            mapsItemsCodes.Add(currentContinentCode, "");
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

        private void readCountriesHierarchy(Dictionary<string, string> mapsItemsCodes, string currentContinentCode, JsonReader reader)
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
                            mapsItemsCodes.Add(currentCountryCode, currentContinentCode);
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

        private void readStatesHierarchy(Dictionary<string, string> mapsItemsCodes, string currentCountryCode, JsonReader reader)
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
                            mapsItemsCodes.Add(currentStateCode, currentCountryCode);
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

        private void readCitiesHierarchy(Dictionary<string, string> mapsItemsCodes, string currentParentCode, JsonReader reader)
        {
            reader.BeginArray();
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
                            mapsItemsCodes.Add(currentCityCode, currentParentCode);
                        }
                    }
                }
            }
            reader.EndObject();
        }

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
            if (currentMap.getSKMFilePath() == null)
            {
                currentMap.setSkmFilePath("");
            }
            if (currentMap.getZipFilePath() == null)
            {
                currentMap.setZipFilePath("");
            }
            if (currentMap.getTXGFilePath() == null)
            {
                currentMap.setTXGFilePath("");
            }
        }
    }
}