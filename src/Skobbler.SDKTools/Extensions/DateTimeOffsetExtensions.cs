using System;

namespace Skobbler.Ngx.SDKTools.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static long CurrentTimeMillis(this DateTimeOffset value)
        {
            return (long)(value - UnixEpoch).TotalMilliseconds;
        }
    }
}