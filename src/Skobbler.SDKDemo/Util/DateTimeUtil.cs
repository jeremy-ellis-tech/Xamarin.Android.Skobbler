using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Skobbler.SDKDemo.Util
{
    internal static class DateTimeUtil
    {
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        internal static long JavaTime()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }
    }
}