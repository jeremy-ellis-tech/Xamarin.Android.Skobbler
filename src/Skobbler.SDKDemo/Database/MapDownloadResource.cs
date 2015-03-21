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
		/// resource name in different languages
		/// </summary>
		private IDictionary<string, string> _names;

	    /// <summary>
		/// constructs an object of SKDownloadResource type
		/// </summary>
		public MapDownloadResource()
		{
			_names = new Dictionary<string, string>();
		}

		/// <returns> the resource parent code </returns>
		public virtual string ParentCode { get; set; }


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
		    try
		    {
                string[] keyValuePairs = newNames.Split(';');
                foreach (String keyValue in keyValuePairs)
                {
                    string[] newName = keyValue.Split('=');
                    _names.Add(newName[0], newName[1]);
                }
		    }
		    catch (Exception)
		    {
		        // ignored
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
		public virtual string SubType { get; set; }


	    /// <returns> SKM file size </returns>
		public virtual long SkmFileSize { get; set; }


	    /// <returns> the SKM + ZIP files size </returns>
		public virtual long SkmAndZipFilesSize { get; set; }


	    /// <returns> the TXG file size </returns>
		public virtual long TxgFileSize { get; set; }


	    /// <returns> the unzippedSize for ZIP file </returns>
		public virtual long UnzippedFileSize { get; set; }


	    /// <returns> the SKM file path </returns>
		public virtual string SKMFilePath { get; private set; }

	    /// <param name="skmFilePath"> the SKM file path </param>
        public virtual void SetSkmFilePath(string skmFilePath)
	    {
            SKMFilePath = skmFilePath;
	    }

	    /// <returns> the ZIP file path </returns>
		public virtual string ZipFilePath { get; set; }


	    /// <returns> the TXG file path </returns>
		public virtual string TxgFilePath { get; set; }


	    /// <returns> bounding-box longitude minim </returns>
		public virtual double BbLongMin { get; set; }


	    /// <returns> bounding-box longitude maxim </returns>
		public virtual double BbLongMax { get; set; }


	    /// <returns> bounding-box latitude minim </returns>
		public virtual double BbLatMin { get; set; }


	    /// <returns> bounding-box latitude maxim </returns>
		public virtual double BbLatMax { get; set; }


	    /// <returns> the flag ID for current resource </returns>
		public virtual int FlagId { get; set; }


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

		public override bool Equals(object obj)
		{
		    if (ReferenceEquals(obj, this)) return true;

		    var other = obj as MapDownloadResource;

		    if (other == null) return false;

		    return Equals(other.Code, Code);
		}

	    public override int GetHashCode()
	    {
	        return Code.GetHashCode();
	    }

	    public override SKToolsDownloadItem ToDownloadItem()
		{
			SKPackageURLInfo info = SKPackageManager.Instance.GetURLInfoForPackageWithCode(Code);
			IList<SKToolsFileDownloadStep> downloadSteps = new List<SKToolsFileDownloadStep>();
			downloadSteps.Add(new SKToolsFileDownloadStep(info.MapURL, (new StringBuilder(DownloadPath)).Append(Code).Append(SKToolsDownloadManager.SkmFileExtension).ToString(), SkmFileSize));
			if (TxgFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.TexturesURL, (new StringBuilder(DownloadPath)).Append(Code).Append(SKToolsDownloadManager.TxgFileExtension).ToString(), TxgFileSize));
			}
			if (UnzippedFileSize != 0)
			{
				downloadSteps.Add(new SKToolsFileDownloadStep(info.NameBrowserFilesURL, (new StringBuilder(DownloadPath)).Append(Code).Append(SKToolsDownloadManager.ZipFileExtension).ToString(), (SkmAndZipFilesSize - SkmFileSize)));
			}
			SKToolsDownloadItem currentItem = new SKToolsDownloadItem(Code, downloadSteps, DownloadState, (UnzippedFileSize != 0), true);
			currentItem.NoDownloadedBytes = NoDownloadedBytes;
			return currentItem;
		}
	}
}