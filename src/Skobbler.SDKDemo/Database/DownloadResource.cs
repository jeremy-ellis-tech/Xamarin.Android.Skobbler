using Skobbler.Ngx.SDKTools.Download;

namespace Skobbler.SDKDemo.Database
{
    public abstract class DownloadResource
    {
        public string Code { get; set; }
        public string DownloadPath { get; set; }
        public sbyte DownloadState { get; set; }
        public long NoDownloadedBytes { get; set; }
        public abstract SKToolsDownloadItem ToDownloadItem();
    }
}