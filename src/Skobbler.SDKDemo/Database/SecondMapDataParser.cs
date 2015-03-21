using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Java.IO;
using Org.Json;
using Skobbler.Ngx.SDKTools.Extensions;
using Skobbler.Ngx.Util;
using Skobbler.SDKDemo.Util;
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
		private const string RegionsId = "regions";

		private const string RegionCodeId = "regionCode";

		private const string SubRegionsId = "subRegions";

		private const string SubRegionCodeId = "subRegionCode";

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

		private const string WorldId = "world";

		private const string ContinentsId = "continents";

		private const string CountriesId = "countries";

		private const string ContinentCodeId = "continentCode";

		private const string CountryCodeId = "countryCode";

		private const string CityCodesId = "cityCodes";

		private const string CityCodeId = "cityCode";

		private const string StateCodesId = "stateCodes";

		private const string StateCodeId = "stateCode";

		private const string Tag = "SKToolsMapDataParser";

		/// <summary>
		/// parses maps JSON data </summary>
		/// <param name="maps"> a list of SKToolsDownloadResource items that represents the maps defined in JSON file </param>
		/// <param name="mapsItemsCodes"> a map representing the maps hierarchy defined in JSON file </param>
		/// <param name="regionItemsCodes"> a map representing the regions hierarchy defined in JSON file </param>
		/// <param name="inputStream"> input stream from JSON file </param>
		/// <exception cref="java.io.IOException"> </exception>
		public virtual void ParseMapJsonData(IList<MapDownloadResource> maps, IDictionary<string, string> mapsItemsCodes, IDictionary<string, string> regionItemsCodes, Stream inputStream)
		{
			Console.WriteLine("Catalin ; start parsing !!!");
		    long startTime = DateTimeOffset.Now.CurrentTimeMillis();
			JSONObject reader = new JSONObject(ConvertJsonFileContentToAString(inputStream));
			JSONArray regionsArray = reader.GetJSONArray(RegionsId);
			if (regionsArray != null)
			{
				ReadUsRegionsHierarchy(regionItemsCodes, regionsArray);
			}
			JSONArray packagesArray = reader.GetJSONArray(PackagesId);
			if (packagesArray != null)
			{
				ReadMapsPackages(maps, packagesArray);
			}
			JSONObject worldObject = reader.GetJSONObject(WorldId);
			if (worldObject != null)
			{
				JSONArray continentsArray = worldObject.GetJSONArray(ContinentsId);
				if (continentsArray != null)
				{
					ReadWorldHierarchy(mapsItemsCodes, continentsArray);
				}
			}
			/*-for (Map.Entry<String, String> currentEntry : mapsItemsCodes.entrySet()) {
			    System.out.println("Catalin ; key = " + currentEntry.getKey() + " ; value = " + currentEntry.getValue());
			}*/
			Console.WriteLine("Catalin ; total loading time = " + (DateTimeOffset.Now.CurrentTimeMillis() - startTime) + " ; maps size = " + maps.Count);
		}

		/// <summary>
		/// read the JSON file and converts it to String using StringWriter </summary>
		/// <param name="inputStream"> JSON file stream </param>
		/// <exception cref="java.io.IOException"> </exception>
		private string ConvertJsonFileContentToAString(Stream inputStream)
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
		private void ReadMapsPackages(IList<MapDownloadResource> maps, JSONArray packagesArray)
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
					SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
				}
				if (currentPackageObject != null)
				{
					MapDownloadResource currentMap = new MapDownloadResource();
					try
					{
						currentMap.Code = currentPackageObject.GetString(PackageCodeId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SubType = GetMapType(currentPackageObject.GetInt(TypeId));
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONArray currentMapNames = currentPackageObject.GetJSONArray(LanguagesId);
						if (currentMapNames != null)
						{
							for (int j = 0; j < currentMapNames.Length(); j++)
							{
								JSONObject currentMapNameObject = currentMapNames.GetJSONObject(j);
								if (currentMapNameObject != null)
								{
									string currentMapName = currentMapNameObject.GetString(TlNameId);
									if (currentMapName != null)
									{
										currentMap.SetName(currentMapName, currentMapNameObject.GetString(LngCodeId));
									}
								}
							}
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONObject currentMapBoundingBox = currentPackageObject.GetJSONObject(BboxId);
						if (currentMapBoundingBox != null)
						{
							currentMap.BbLatMax = currentMapBoundingBox.GetDouble(LatMaxId);
							currentMap.BbLatMin = currentMapBoundingBox.GetDouble(LatMinId);
							currentMap.BbLongMax = currentMapBoundingBox.GetDouble(LongMaxId);
							currentMap.BbLongMin = currentMapBoundingBox.GetDouble(LongMinId);
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmFileSize = currentPackageObject.GetLong(SkmSizeId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmFilePath = currentPackageObject.GetString(FileId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.ZipFilePath = currentPackageObject.GetString(NbZipId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.UnzippedFileSize = currentPackageObject.GetLong(UnzipSizeId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						JSONObject currentMapTxgDetails = currentPackageObject.GetJSONObject(TextureId);
						if (currentMapTxgDetails != null)
						{
							currentMap.TxgFilePath = currentMapTxgDetails.GetString(TexturesBigFileId);
							currentMap.TxgFileSize = currentMapTxgDetails.GetLong(SizeBigFileId);
						}
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					try
					{
						currentMap.SkmAndZipFilesSize = currentPackageObject.GetLong(SizeId);
					}
					catch (JSONException ex)
					{
                        SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
					}
					if ((currentMap.Code != null) && (currentMap.SubType != null))
					{
						RemoveNullValuesIfExist(currentMap);
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
		private void ReadUsRegionsHierarchy(IDictionary<string, string> regionItemsCodes, JSONArray regionsArray)
		{
			for (int i = 0; i < regionsArray.Length(); i++)
			{
				JSONObject currentRegionObject = regionsArray.GetJSONObject(i);
				if (currentRegionObject != null)
				{
					string currentRegionCode = currentRegionObject.GetString(RegionCodeId);
					if (currentRegionCode != null)
					{
						JSONArray subRegions = currentRegionObject.GetJSONArray(SubRegionsId);
						if (subRegions != null)
						{
							for (int j = 0; j < subRegions.Length(); j++)
							{
								JSONObject currentSubRegionObject = subRegions.GetJSONObject(j);
								if (currentSubRegionObject != null)
								{
									string subRegionCode = currentSubRegionObject.GetString(SubRegionCodeId);
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
		private void ReadWorldHierarchy(IDictionary<string, string> mapsItemsCodes, JSONArray continentsArray)
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
							string currentContinentCode = currentContinentObject.GetString(ContinentCodeId);
							if (currentContinentCode != null)
							{
								mapsItemsCodes[currentContinentCode] = "";
								JSONArray countriesArray = null;
								try
								{
									countriesArray = currentContinentObject.GetJSONArray(CountriesId);
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
								}
								if (countriesArray != null)
								{
									ReadCountriesHierarchy(mapsItemsCodes, currentContinentCode, countriesArray);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read countries hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentContinentCode"> current continent code </param>
		/// <param name="countriesArray"> countries array </param>
		private void ReadCountriesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentContinentCode, JSONArray countriesArray)
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
							string currentCountryCode = currentCountryObject.GetString(CountryCodeId);
							if ((currentContinentCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentCountryCode] = currentContinentCode;
								try
								{
									JSONArray citiesArray = currentCountryObject.GetJSONArray(CityCodesId);
									if (citiesArray != null)
									{
										ReadCitiesHierarchy(mapsItemsCodes, currentCountryCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
								}
								try
								{
									JSONArray statesArray = currentCountryObject.GetJSONArray(StateCodesId);
									if (statesArray != null)
									{
										ReadStatesHierarchy(mapsItemsCodes, currentCountryCode, statesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read states hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentCountryCode"> current country code </param>
		/// <param name="statesArray"> states array </param>
		private void ReadStatesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentCountryCode, JSONArray statesArray)
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
							string currentStateCode = currentStateObject.GetString(StateCodeId);
							if ((currentStateCode != null) && (currentCountryCode != null))
							{
								mapsItemsCodes[currentStateCode] = currentCountryCode;
								try
								{
									JSONArray citiesArray = currentStateObject.GetJSONArray(CityCodesId);
									if (citiesArray != null)
									{
										ReadCitiesHierarchy(mapsItemsCodes, currentStateCode, citiesArray);
									}
								}
								catch (JSONException ex)
								{
                                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
								}
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
				}
			}
		}

		/// <summary>
		/// read cities hierarchy for maps items </summary>
		/// <param name="mapsItemsCodes"> a map of type (code ; parentCode) that contains all maps items codes </param>
		/// <param name="currentParentCode"> current parent code </param>
		/// <param name="citiesArray"> cities array </param>
		private void ReadCitiesHierarchy(IDictionary<string, string> mapsItemsCodes, string currentParentCode, JSONArray citiesArray)
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
							string currentCityCode = currentCityObject.GetString(CityCodeId);
							if ((currentCityCode != null) && (currentParentCode != null))
							{
								mapsItemsCodes[currentCityCode] = currentParentCode;
							}
						}
						catch (JSONException ex)
						{
                            SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
						}
					}
				}
				catch (JSONException ex)
				{
                    SKLogging.WriteLog(Tag, ex.Message, SKLogging.LogDebug);
				}
			}
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