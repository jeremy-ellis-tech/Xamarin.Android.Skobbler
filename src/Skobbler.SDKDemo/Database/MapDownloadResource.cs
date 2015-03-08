using System;
using System.Collections.Generic;
using System.Text;
using Java.Util;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.SDKTools.Download;

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
		private string _parentCode;

		/// <summary>
		/// resource name in different languages
		/// </summary>
		private IDictionary<string, string> _names;

		/// <summary>
		/// resource sub-type (e.g. continent, country, city, state for map resource)
		/// </summary>
		private string _subType;

		/// <summary>
		/// SKM file size
		/// </summary>
		private long _skmFileSize;

		/// <summary>
		/// SKM + ZIP file size
		/// </summary>
		private long _skmAndZipFilesSize;

		/// <summary>
		/// txg file size
		/// </summary>
		private long _txgFileSize;

		/// <summary>
		/// UNZIPPED file size for zip file
		/// </summary>
		private long _unzippedFileSize;

		/// <summary>
		/// .SKM file path
		/// </summary>
		private string _skmFilePath;

		/// <summary>
		/// .ZIP file path
		/// </summary>
		private string _zipFilePath;

		/// <summary>
		/// .TXG file path
		/// </summary>
		private string _txgFilePath;

		/// <summary>
		/// bounding box minimum longitude
		/// </summary>
		private double _bbLongMin;

		/// <summary>
		/// bounding box maximum longitude
		/// </summary>
		private double _bbLongMax;

		/// <summary>
		/// bounding box minimum latitude
		/// </summary>
		private double _bbLatMin;

		/// <summary>
		/// bounding box maximum latitude
		/// </summary>
		private double _bbLatMax;

		/// <summary>
		/// flag resource id
		/// </summary>
		private int _flagId;

		/// <summary>
		/// constructs an object of SKDownloadResource type
		/// </summary>
		public MapDownloadResource()
		{
			_names = new Dictionary<string, string>();
		}

		/// <returns> the resource parent code </returns>
		public virtual string ParentCode
		{
			get
			{
				return _parentCode;
			}
			set
			{
				_parentCode = value;
			}
		}


		/// <returns> the resource name </returns>
		public virtual string Name
		{
			get
			{
				string localLanguage = Locale.Default.Language;
				if (localLanguage.StartsWith(MapsDao.EnglishLanguageCode, StringComparison.Ordinal))
				{
					localLanguage = MapsDao.EnglishLanguageCode;
				}
				if (_names != null)
				{
				    if (_names[localLanguage] == null)
					{
						return _names[Locale.English.Language];
					}
				    return _names[localLanguage];
				}
			    return "";
			}
		}

		/// <summary>
		/// sets the name in all languages </summary>
		/// <param name="newNames"> resource names in all languages </param>
		public virtual void SetNames(string newNames)
		{
			_names = new Dictionary<string, string>();
			string[] keyValuePairs = newNames.Split(';');
			foreach (String keyValue in keyValuePairs)
			{
				string[] newName = keyValue.Split('=');
				_names[newName[0]] = newName[1];
			}
		}

		/// <param name="name"> the resource name for a certain language </param>
		public virtual void SetName(string name, string language)
		{
			_names[language] = name;
		}

		/// <returns> names in all languages for current resource </returns>
		public virtual IDictionary<string, string> GetNames()
		{
			return _names;
		}

		/// <returns> the resource sub-type </returns>
		public virtual string SubType
		{
			get
			{
				return _subType;
			}
			set
			{
				_subType = value;
			}
		}


		/// <returns> SKM file size </returns>
		public virtual long SkmFileSize
		{
			get
			{
				return _skmFileSize;
			}
			set
			{
				_skmFileSize = value;
			}
		}


		/// <returns> the SKM + ZIP files size </returns>
		public virtual long SkmAndZipFilesSize
		{
			get
			{
				return _skmAndZipFilesSize;
			}
			set
			{
				_skmAndZipFilesSize = value;
			}
		}


		/// <returns> the TXG file size </returns>
		public virtual long TxgFileSize
		{
			get
			{
				return _txgFileSize;
			}
			set
			{
				_txgFileSize = value;
			}
		}


		/// <returns> the unzippedSize for ZIP file </returns>
		public virtual long UnzippedFileSize
		{
			get
			{
				return _unzippedFileSize;
			}
			set
			{
				_unzippedFileSize = value;
			}
		}


		/// <returns> the SKM file path </returns>
		public virtual string SKMFilePath
		{
			get
			{
				return _skmFilePath;
			}
		}

		/// <param name="skmFilePath"> the SKM file path </param>
		public virtual string SkmFilePath
		{
			set
			{
				_skmFilePath = value;
			}
		}

		/// <returns> the ZIP file path </returns>
		public virtual string ZipFilePath
		{
			get
			{
				return _zipFilePath;
			}
			set
			{
				_zipFilePath = value;
			}
		}


		/// <returns> the TXG file path </returns>
		public virtual string TxgFilePath
		{
			get
			{
				return _txgFilePath;
			}
			set
			{
				_txgFilePath = value;
			}
		}


		/// <returns> bounding-box longitude minim </returns>
		public virtual double BbLongMin
		{
			get
			{
				return _bbLongMin;
			}
			set
			{
				_bbLongMin = value;
			}
		}


		/// <returns> bounding-box longitude maxim </returns>
		public virtual double BbLongMax
		{
			get
			{
				return _bbLongMax;
			}
			set
			{
				_bbLongMax = value;
			}
		}


		/// <returns> bounding-box latitude minim </returns>
		public virtual double BbLatMin
		{
			get
			{
				return _bbLatMin;
			}
			set
			{
				_bbLatMin = value;
			}
		}


		/// <returns> bounding-box latitude maxim </returns>
		public virtual double BbLatMax
		{
			get
			{
				return _bbLatMax;
			}
			set
			{
				_bbLatMax = value;
			}
		}


		/// <returns> the flag ID for current resource </returns>
		public virtual int FlagId
		{
			get
			{
				return _flagId;
			}
			set
			{
				_flagId = value;
			}
		}


		public virtual int CompareTo(MapDownloadResource another)
		{
			string firstName = Name, secondName = (another != null) ? another.Name : null;
			if ((firstName != null) && (secondName != null))
			{
				return firstName.ToLower().CompareTo(secondName.ToLower());
			}
		    if (firstName != null)
		    {
		        return -1;
		    }
		    if (secondName != null)
		    {
		        return 1;
		    }
		    return 0;
		}

		public override bool Equals(object another)
		{
		    if (another == null)
			{
				return false;
			}
		    if (!(another is MapDownloadResource))
		    {
		        return false;
		    }
		    MapDownloadResource anotherResource = (MapDownloadResource) another;
		    return Code.Equals(anotherResource.Code);
		}

	    public override SKToolsDownloadItem ToDownloadItem()
		{
			SKPackageURLInfo info = SKPackageManager.Instance.GetURLInfoForPackageWithCode(code);
			IList<SKToolsFileDownloadStep> downloadSteps = new List<SKToolsFileDownloadStep>();
			downloadSteps.Add(new SKToolsFileDownloadStep(info.MapURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.SkmFileExtension).ToString(), _skmFileSize));
			if (_txgFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.TexturesURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.TxgFileExtension).ToString(), _txgFileSize));
			}
			if (_unzippedFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.NameBrowserFilesURL, (new StringBuilder(downloadPath)).Append(code).Append(SKToolsDownloadManager.ZipFileExtension).ToString(), (_skmAndZipFilesSize - _skmFileSize)));
			}
			SKToolsDownloadItem currentItem = new SKToolsDownloadItem(code, downloadSteps, DownloadState, (_unzippedFileSize != 0), true);
			currentItem.NoDownloadedBytes = NoDownloadedBytes;
			return currentItem;
		}
	}
}