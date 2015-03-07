namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// Defines a resource that will be DOWNLOADED (e.g map, sound file)
	/// </summary>
	public abstract class DownloadResource
	{

		/// <summary>
		/// resource code
		/// </summary>
		protected internal string code;

		/// <summary>
		/// storage path where resource will be downloaded
		/// </summary>
		protected internal string downloadPath;

		/// <summary>
		/// resource state (e.g. NOT_QUEUED, QUEUED, DOWNLOADING, ZIPPED, INSTALLING, DOWNLOADED)
		/// </summary>
		private sbyte downloadState;

		/// <summary>
		/// total number of DOWNLOADED bytes
		/// </summary>
		private long noDownloadedBytes;

		/// <returns> the resource code </returns>
		public virtual string Code
		{
			get
			{
				return code;
			}
			set
			{
				this.code = value;
			}
		}


		/// <returns> the download path for current resource </returns>
		public virtual string DownloadPath
		{
			get
			{
				return downloadPath;
			}
			set
			{
				this.downloadPath = value;
			}
		}


		/// <returns> download state for current resource </returns>
		public virtual sbyte DownloadState
		{
			get
			{
				return downloadState;
			}
			set
			{
				this.downloadState = value;
			}
		}


		/// <summary>
		/// gets the total number of DOWNLOADED bytes from current resource
		/// </summary>
		public virtual long NoDownloadedBytes
		{
			get
			{
				return noDownloadedBytes;
			}
			set
			{
				this.noDownloadedBytes = value;
			}
		}


		/// <returns> a SKToolsDownloadItem from current object </returns>
		public abstract SKToolsDownloadItem toDownloadItem();
	}

}