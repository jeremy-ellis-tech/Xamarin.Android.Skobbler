using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Util;
using Java.IO;
using Skobbler.Ngx.Navigation;
using Console = System.Console;
using File = Java.IO.File;
using IOException = Java.IO.IOException;
using JavaObject = Java.Lang.Object;
using Stream = Android.Media.Stream;

namespace Skobbler.Ngx.SDKTools.NavigationUI
{
    public class SKToolsAdvicePlayer : JavaObject, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnErrorListener
    {

        private const string Tag = "SKToolsAdvicePlayer";

        // constants for advice priority - user requested advices have the highest,
        // speed warnings the lowest
        public const int PriorityUser = 0;

        public const int PriorityNavigation = 1;

        public const int PrioritySpeedWarning = 2;

        /// <summary>
        /// The singleton instance of the advice player.
        /// </summary>
        private static SKToolsAdvicePlayer _instance;

        /// <summary>
        /// The single player.
        /// </summary>
        private MediaPlayer _player;

        /// <summary>
        /// The temporary file for storing the current advice
        /// </summary>
        private string _tempAdviceFile;

        /// <summary>
        /// Queued advice that will be played after the player finishes playing the
        /// current advice.
        /// </summary>
        private string[] _nextAdvice;

        /// <summary>
        /// The priority of the queued advice.
        /// </summary>
        private int _nextAdvicePriority;

        /// <summary>
        /// Indicates if the user has chosen to mute the advices.
        /// </summary>
        private bool _isMuted;

        /// <summary>
        /// Indicates whether the player is busy playing an advice.
        /// </summary>
        private bool _isBusy;

        public static SKToolsAdvicePlayer Instance
        {
            get { return _instance ?? (_instance = new SKToolsAdvicePlayer()); }
        }

        private SKToolsAdvicePlayer()
        {
            _player = new MediaPlayer();
            _player.SetAudioStreamType(Stream.Music);
            _player.SetOnCompletionListener(this);
            _player.SetOnErrorListener(this);
        }

        /// <summary>
        /// method that retrieves the current volume level of the device audio
        /// manager with stream type STREAM_MUSIC </summary>
        /// <param name="activity">
        /// @return </param>
        public static int GetCurrentDeviceVolume(Activity activity)
        {
            AudioManager audioManager = (AudioManager)activity.GetSystemService(Context.AudioService);
            return audioManager.GetStreamVolume(Stream.Music);
        }

        /// <summary>
        /// method that retrieves the maximum volume level of the device audio
        /// manager with the stream type STREAM_MUSIC </summary>
        /// <param name="activity"> - the current activity
        /// @return </param>
        public static int GetMaximAudioLevel(Activity activity)
        {
            AudioManager audioManager = (AudioManager)activity.GetSystemService(Context.AudioService);
            return audioManager.GetStreamMaxVolume(Stream.Music);
        }

        public virtual void EnableMute()
        {
            _isMuted = true;
        }

        public virtual void DisableMute()
        {
            _isMuted = false;
        }

        public virtual bool Muted
        {
            get
            {
                return _isMuted;
            }
        }

        /// <summary>
        /// Plays an advice. The individual sound files to play are contained in an
        /// array list.
        /// </summary>
        /// <param name="adviceParts"> an array list of sound file names </param>
        public virtual void PlayAdvice(string[] adviceParts, int priority)
        {
            if (_isMuted || adviceParts == null)
            {
                return;
            }

            if (_isBusy)
            {
                if (_nextAdvice == null || (priority <= _nextAdvicePriority))
                {
                    _nextAdvice = adviceParts;
                    _nextAdvicePriority = priority;
                }
                return;
            }

            SKAdvisorSettings advisorSettings = SKMaps.Instance.MapInitSettings.AdvisorSettings;
            string soundFilesDirPath = advisorSettings.ResourcePath + advisorSettings.Language.Value + "/sound_files/";

            _tempAdviceFile = soundFilesDirPath + "temp.mp3";
            bool validTokensFound = false;
            ByteArrayOutputStream stream = new ByteArrayOutputStream();
            for (int i = 0; i < adviceParts.Length; i++)
            {
                string soundFilePath = soundFilesDirPath + adviceParts[i] + ".mp3";
                try
                {
                    System.IO.Stream @is = new FileStream(soundFilePath, FileMode.Open, FileAccess.Read);
                    int availableBytes = 0;// @is.Available();
                    byte[] tmp = new byte[availableBytes];
                    @is.Read(tmp, 0, availableBytes);
                    if (stream != null)
                    {
                        stream.Write(tmp);
                    }
                    @is.Close();
                    validTokensFound = true;
                }
                catch (IOException ioe)
                {
                    Console.WriteLine(ioe.ToString());
                    Console.Write(ioe.StackTrace);
                }
            }

            if (validTokensFound)
            {
                // valid tokens were found - set busy state until finishing to play
                // advice
                _isBusy = true;
            }
            else
            {
                // valid tokens not found - return without playing anything
                return;
            }

            WriteFile(stream.ToByteArray(), _tempAdviceFile);
            PlayFile(_tempAdviceFile);
        }

        public virtual void Reset()
        {
            Log.Warn(Tag, "Entering reset");
            if (_player != null)
            {
                try
                {
                    _player.Reset();
                    DeleteTempFile();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.Write(ex.StackTrace);
                }
            }
            _isBusy = false;
        }

        /// <summary>
        /// Deletes the temporary file stored at "tempAdviceFile" path
        /// </summary>
        private void DeleteTempFile()
        {
            File fc = new File(_tempAdviceFile);
            if (fc.Exists())
            {
                fc.Delete();
            }
        }

        /// <summary>
        /// Stops playing the current advice
        /// </summary>
        public virtual void Stop()
        {
            _isBusy = false;
            _player.Stop();
        }

        /// <summary>
        /// Writes "data" to the "filePath" path on the disk
        /// </summary>
        private void WriteFile(byte[] data, string filePath)
        {
            System.IO.Stream @out = null;
            try
            {
                @out = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                @out.Write(data, 0, data.Length);
                try
                {
                    @out.Flush();
                    @out.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

            }
            catch (IOException ioe)
            {
                Console.WriteLine(ioe.ToString());
                Console.Write(ioe.StackTrace);
            }
        }

        /// <summary>
        /// Plays an .mp3 file which should be found at filePath </summary>
        /// <param name="filePath"> </param>
        private void PlayFile(string filePath)
        {
            try
            {
                _player.Reset();

                FileInputStream fileInputStream = new FileInputStream(filePath);
                FileDescriptor fileDescriptor = fileInputStream.FD;

                try
                {
                    _player.SetDataSource(fileDescriptor);
                }
                catch (InvalidOperationException)
                {
                    _player.Reset();
                    _player.SetDataSource(fileDescriptor);
                }

                fileInputStream.Close();

                _player.Prepare();
                _player.Start();
            }
            catch (IOException ioe)
            {
                Console.WriteLine(ioe.ToString());
                Console.Write(ioe.StackTrace);
            }
        }

        public void OnCompletion(MediaPlayer mp)
        {
            Reset();
            if (_nextAdvice != null)
            {
                string[] adviceToPlay = _nextAdvice;
                _nextAdvice = null;
                PlayAdvice(adviceToPlay, _nextAdvicePriority);
            }
        }

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            return true; //error was handled
        }
    }
}