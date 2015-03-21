using Skobbler.Ngx.SDKTools.Download;

namespace Skobbler.SDKDemo.Database
{
	/// <summary>
	/// Defines a resource that will be DOWNLOADED (e.g map, sound file)
	/// </summary>
	public abstract class DownloadResource
	{
	    /// <returns> the resource code </returns>
		public virtual string Code { get; set; }


	    /// <returns> the download path for current resource </returns>
		public virtual string DownloadPath { get; set; }


	    /// <returns> download state for current resource </returns>
		public virtual SKDownloadState DownloadState { get; set; }


	    /// <summary>
		/// gets the total number of DOWNLOADED bytes from current resource
		/// </summary>
		public virtual long NoDownloadedBytes { get; set; }


	    /// <returns> a SKToolsDownloadItem from current object </returns>
		public abstract SKToolsDownloadItem ToDownloadItem();
	}

}