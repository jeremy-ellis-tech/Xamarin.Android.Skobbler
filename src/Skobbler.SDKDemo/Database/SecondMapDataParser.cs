using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Java.IO;
using Org.Json;
using Skobbler.Ngx.Util;
using Console = System.Console;
using StringWriter = Java.IO.StringWriter;
using Skobbler.SDKDemo.Util;

namespace Skobbler.SDKDemo.Database
{
    public class SecondMapDataParser
    {
        private static readonly string REGIONS_ID = "regions";

        private static readonly string REGION_CODE_ID = "regionCode";

        private static readonly string SUB_REGIONS_ID = "subRegions";

        private static readonly string SUB_REGION_CODE_ID = "subRegionCode";

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

        private static readonly string WORLD_ID = "world";

        private static readonly string CONTINENTS_ID = "continents";

        private static readonly string COUNTRIES_ID = "countries";

        private static readonly string CONTINENT_CODE_ID = "continentCode";

        private static readonly string COUNTRY_CODE_ID = "countryCode";

        private static readonly string CITY_CODES_ID = "cityCodes";

        private static readonly string CITY_CODE_ID = "cityCode";

        private static readonly string STATE_CODES_ID = "stateCodes";

        private static readonly string STATE_CODE_ID = "stateCode";

        private static readonly string TAG = "SKToolsMapDataParser";

        public void ParseMapJsonData(List<MapDownloadResource> maps, Dictionary<String, String> mapsItemsCodes, Dictionary<String, String> regionItemsCodes, InputStream inputStream)
        {
            Console.WriteLine("Catalin ; start parsing !!!");
            long startTime = DemoUtils.CurrentTimeMillis();
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
            Console.WriteLine("Catalin ; total loading time = " + (DemoUtils.CurrentTimeMillis() - startTime) + " ; maps size = " + maps.Count);
        }

        private string convertJSONFileContentToAString(InputStream inputStream)
        {
            char[] buffer = new char[1024];
            Writer stringWriter = new StringWriter();
            try
            {
                //Reader bufferedReader = new BufferedReader(new InputStreamReader(inputStream, "UTF-8"));
                Reader bufferedReader = null; //new BufferedReader(new InputStreamReader(inputStream));
                int n;
                while ((n = bufferedReader.Read(buffer)) != -1)
                {
                    stringWriter.Write(buffer, 0, n);
                }
            }
            finally
            {
                stringWriter.Close();
            }
            return stringWriter.ToString();
        }

        /**
         * read maps packages list
         * @param maps a list of maps objects that will be read from JSON file
         * @param packagesArray packages array
         */
        private void readMapsPackages(List<MapDownloadResource> maps, JSONArray packagesArray)
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
                        currentMap.Code = (currentPackageObject.GetString(PACKAGE_CODE_ID));
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setSubType(GetMapType(currentPackageObject.GetInt(TYPE_ID)));
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
                                    String currentMapName = currentMapNameObject.GetString(TL_NAME_ID);
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
                            currentMap.setBbLatMax(currentMapBoundingBox.GetDouble(LAT_MAX_ID));
                            currentMap.setBbLatMin(currentMapBoundingBox.GetDouble(LAT_MIN_ID));
                            currentMap.setBbLongMax(currentMapBoundingBox.GetDouble(LONG_MAX_ID));
                            currentMap.setBbLongMin(currentMapBoundingBox.GetDouble(LONG_MIN_ID));
                        }
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setSkmFileSize(currentPackageObject.GetLong(SKM_SIZE_ID));
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setSkmFilePath(currentPackageObject.GetString(FILE_ID));
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setZipFilePath(currentPackageObject.GetString(NB_ZIP_ID));
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setUnzippedFileSize(currentPackageObject.GetLong(UNZIP_SIZE_ID));
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
                            currentMap.setTXGFilePath(currentMapTXGDetails.GetString(TEXTURES_BIG_FILE_ID));
                            currentMap.setTXGFileSize(currentMapTXGDetails.GetLong(SIZE_BIG_FILE_ID));
                        }
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    try
                    {
                        currentMap.setSkmAndZipFilesSize(currentPackageObject.GetLong(SIZE_ID));
                    }
                    catch (JSONException ex)
                    {
                        SKLogging.WriteLog(TAG, ex.Message, SKLogging.LogDebug);
                    }
                    if ((currentMap.Code != null) && (currentMap.getSubType() != null))
                    {
                        removeNullValuesIfExist(currentMap);
                        maps.Add(currentMap);
                    }
                }
            }
        }

        private void readUSRegionsHierarchy(Dictionary<String, String> regionItemsCodes, JSONArray regionsArray)
        {
            for (int i = 0; i < regionsArray.Length(); i++)
            {
                JSONObject currentRegionObject = regionsArray.GetJSONObject(i);
                if (currentRegionObject != null)
                {
                    String currentRegionCode = currentRegionObject.GetString(REGION_CODE_ID);
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
                                    String subRegionCode = currentSubRegionObject.GetString(SUB_REGION_CODE_ID);
                                    if (subRegionCode != null)
                                    {
                                        regionItemsCodes.Add(subRegionCode, currentRegionCode);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void readWorldHierarchy(Dictionary<String, String> mapsItemsCodes, JSONArray continentsArray)
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
                            String currentContinentCode = currentContinentObject.GetString(CONTINENT_CODE_ID);
                            if (currentContinentCode != null)
                            {
                                mapsItemsCodes.Add(currentContinentCode, "");
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

        private void readCountriesHierarchy(Dictionary<String, String> mapsItemsCodes, string currentContinentCode, JSONArray countriesArray)
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
                            String currentCountryCode = currentCountryObject.GetString(COUNTRY_CODE_ID);
                            if ((currentContinentCode != null) && (currentCountryCode != null))
                            {
                                mapsItemsCodes.Add(currentCountryCode, currentContinentCode);
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

        private void readStatesHierarchy(Dictionary<String, String> mapsItemsCodes, string currentCountryCode, JSONArray statesArray)
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
                            String currentStateCode = currentStateObject.GetString(STATE_CODE_ID);
                            if ((currentStateCode != null) && (currentCountryCode != null))
                            {
                                mapsItemsCodes.Add(currentStateCode, currentCountryCode);
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

        private void readCitiesHierarchy(Dictionary<string, string> mapsItemsCodes, String currentParentCode, JSONArray citiesArray)
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
                            String currentCityCode = currentCityObject.GetString(CITY_CODE_ID);
                            if ((currentCityCode != null) && (currentParentCode != null))
                            {
                                mapsItemsCodes.Add(currentCityCode, currentParentCode);
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

        private string GetMapType(int mapTypeInt)
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
                currentMap.ParentCode = ("");
            }
            if (currentMap.DownloadPath == null)
            {
                currentMap.DownloadPath = ("");
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