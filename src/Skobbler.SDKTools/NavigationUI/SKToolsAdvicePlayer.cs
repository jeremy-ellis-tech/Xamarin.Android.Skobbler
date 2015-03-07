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
using JavaObject = Java.Lang.Object;
using Android.Media;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    /// <summary>
    /// The purpose of this class is to play an advice.
    /// An advice basically consists of a series of sound files
    /// that combined, represent the advice that should be played to the user.
    /// </summary>
    public class SKToolsAdvicePlayer : JavaObject, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener
    {
        public void OnCompletion(MediaPlayer mp)
        {
            
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            return true;
        }
    }
}