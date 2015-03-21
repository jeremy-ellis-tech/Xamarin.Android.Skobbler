namespace Skobbler.Ngx.SDKTools.Download
{
    public enum SKDownloadState : sbyte
    {
        NotQueued = 0,
        Queued = 1,
        Downloading = 2,
        Paused = 3,
        Downloaded = 4,
        Installing = 5,
        Installed = 6
    }
}