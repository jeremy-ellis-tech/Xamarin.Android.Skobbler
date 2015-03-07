using System;
using System.Collections.Generic;
using System.Text;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// Defines a map resource that will be DOWNLOADED
	/// </summary>
	public class MapDownloadResource : DownloadResource, IComparable<MapDownloadResource>
	{

		/// <summary>
		/// resource parent code
		/// </summary>
		private string parentCode;

		/// <summary>
		/// resource name in different languages
		/// </summary>
		private IDictionary<string, string> names;

		/// <summary>
		/// resource sub-type (e.g. continent, country, city, state for map resource)
		/// </summary>
		private string subType;

		/// <summary>
		/// SKM file size
		/// </summary>
		private long skmFileSize;

		/// <summary>
		/// SKM + ZIP file size
		/// </summary>
		private long skmAndZipFilesSize;

		/// <summary>
		/// txg file size
		/// </summary>
		private long txgFileSize;

		/// <summary>
		/// UNZIPPED file size for zip file
		/// </summary>
		private long unzippedFileSize;

		/// <summary>
		/// .SKM file path
		/// </summary>
		private string skmFilePath;

		/// <summary>
		/// .ZIP file path
		/// </summary>
		private string zipFilePath;

		/// <summary>
		/// .TXG file path
		/// </summary>
		private string txgFilePath;

		/// <summary>
		/// bounding box minimum longitude
		/// </summary>
		private double bbLongMin;

		/// <summary>
		/// bounding box maximum longitude
		/// </summary>
		private double bbLongMax;

		/// <summary>
		/// bounding box minimum latitude
		/// </summary>
		private double bbLatMin;

		/// <summary>
		/// bounding box maximum latitude
		/// </summary>
		private double bbLatMax;

		/// <summary>
		/// flag resource id
		/// </summary>
		private int flagID;

		/// <summary>
		/// constructs an object of SKDownloadResource type
		/// </summary>
		public MapDownloadResource()
		{
			names = new LinkedHashMap<string, string>();
		}

		/// <returns> the resource parent code </returns>
		public virtual string ParentCode
		{
			get
			{
				return parentCode;
			}
			set
			{
				this.parentCode = value;
			}
		}


		/// <returns> the resource name </returns>
		public virtual string Name
		{
			get
			{
				string localLanguage = Locale.Default.Language;
				if (localLanguage.StartsWith(MapsDAO.ENGLISH_LANGUAGE_CODE, StringComparison.Ordinal))
				{
					localLanguage = MapsDAO.ENGLISH_LANGUAGE_CODE;
				}
				if (names != null)
				{
					if (names[localLanguage] == null)
					{
						return names[Locale.ENGLISH.Language];
					}
					else
					{
						return names[localLanguage];
					}
				}
				else
				{
					return "";
				}
			}
		}

		/// <summary>
		/// sets the name in all languages </summary>
		/// <param name="newNames"> resource names in all languages </param>
		public virtual void setNames(string newNames)
		{
			this.names = new LinkedHashMap<string, string>();
			string[] keyValuePairs = newNames.Split(";", true);
			foreach (String keyValue in keyValuePairs)
			{
				string[] newName = keyValue.Split("=");
				this.names[newName[0]] = newName[1];
			}
		}

		/// <param name="name"> the resource name for a certain language </param>
		public virtual void setName(string name, string language)
		{
			names[language] = name;
		}

		/// <returns> names in all languages for current resource </returns>
		public virtual IDictionary<string, string> getNames()
		{
			return names;
		}

		/// <returns> the resource sub-type </returns>
		public virtual string SubType
		{
			get
			{
				return subType;
			}
			set
			{
				this.subType = value;
			}
		}


		/// <returns> SKM file size </returns>
		public virtual long SkmFileSize
		{
			get
			{
				return skmFileSize;
			}
			set
			{
				this.skmFileSize = value;
			}
		}


		/// <returns> the SKM + ZIP files size </returns>
		public virtual long SkmAndZipFilesSize
		{
			get
			{
				return skmAndZipFilesSize;
			}
			set
			{
				this.skmAndZipFilesSize = value;
			}
		}


		/// <returns> the TXG file size </returns>
		public virtual long TXGFileSize
		{
			get
			{
				return txgFileSize;
			}
			set
			{
				this.txgFileSize = value;
			}
		}


		/// <returns> the unzippedSize for ZIP file </returns>
		public virtual long UnzippedFileSize
		{
			get
			{
				return unzippedFileSize;
			}
			set
			{
				this.unzippedFileSize = value;
			}
		}


		/// <returns> the SKM file path </returns>
		public virtual string SKMFilePath
		{
			get
			{
				return skmFilePath;
			}
		}

		/// <param name="skmFilePath"> the SKM file path </param>
		public virtual string SkmFilePath
		{
			set
			{
				this.skmFilePath = value;
			}
		}

		/// <returns> the ZIP file path </returns>
		public virtual string ZipFilePath
		{
			get
			{
				return zipFilePath;
			}
			set
			{
				this.zipFilePath = value;
			}
		}


		/// <returns> the TXG file path </returns>
		public virtual string TXGFilePath
		{
			get
			{
				return txgFilePath;
			}
			set
			{
				this.txgFilePath = value;
			}
		}


		/// <returns> bounding-box longitude minim </returns>
		public virtual double BbLongMin
		{
			get
			{
				return bbLongMin;
			}
			set
			{
				this.bbLongMin = value;
			}
		}


		/// <returns> bounding-box longitude maxim </returns>
		public virtual double BbLongMax
		{
			get
			{
				return bbLongMax;
			}
			set
			{
				this.bbLongMax = value;
			}
		}


		/// <returns> bounding-box latitude minim </returns>
		public virtual double BbLatMin
		{
			get
			{
				return bbLatMin;
			}
			set
			{
				this.bbLatMin = value;
			}
		}


		/// <returns> bounding-box latitude maxim </returns>
		public virtual double BbLatMax
		{
			get
			{
				return bbLatMax;
			}
			set
			{
				this.bbLatMax = value;
			}
		}


		/// <returns> the flag ID for current resource </returns>
		public virtual int FlagID
		{
			get
			{
				return flagID;
			}
			set
			{
				this.flagID = value;
			}
		}


		public virtual int CompareTo(MapDownloadResource another)
		{
			string firstName = Name, secondName = (another != null) ? another.Name : null;
			if ((firstName != null) && (secondName != null))
			{
				return firstName.ToLower().CompareTo(secondName.ToLower());
			}
			else if (firstName != null)
			{
				return -1;
			}
			else if (secondName != null)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		public override bool Equals(object another)
		{
			if (another == null)
			{
				return false;
			}
			else if (!(another is MapDownloadResource))
			{
				return false;
			}
			else
			{
				MapDownloadResource anotherResource = (MapDownloadResource) another;
				return this.Code.Equals(anotherResource.Code);
			}
		}

		public override SKToolsDownloadItem toDownloadItem()
		{
			SKPackageURLInfo info = SKPackageManager.Instance.getURLInfoForPackageWithCode(code);
			IList<SKToolsFileDownloadStep> downloadSteps = new List<SKToolsFileDownloadStep>();
			downloadSteps.Add(new SKToolsFileDownloadStep(info.MapURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.SKM_FILE_EXTENSION).ToString(), skmFileSize));
			if (txgFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.TexturesURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.TXG_FILE_EXTENSION).ToString(), txgFileSize));
			}
			if (unzippedFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.NameBrowserFilesURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.ZIP_FILE_EXTENSION).ToString(), (skmAndZipFilesSize - skmFileSize)));
			}
			SKToolsDownloadItem currentItem = new SKToolsDownloadItem(code, downloadSteps, DownloadState, (unzippedFileSize != 0), true);
			currentItem.NoDownloadedBytes = NoDownloadedBytes;
			return currentItem;
		}
	}
}