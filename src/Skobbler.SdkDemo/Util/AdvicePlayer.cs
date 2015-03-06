using Android.Media;
using Java.IO;
using Skobbler.Ngx;
using Skobbler.Ngx.Navigation;
using System;


namespace Skobbler.SDKDemo.Util
{
    class AdvicePlayer
    {
        private static AdvicePlayer _instance;
        public static AdvicePlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AdvicePlayer();
                }

                return _instance;
            }
        }

        private MediaPlayer _player;

        private AdvicePlayer()
        {
            _player = new MediaPlayer();
            _player.SetAudioStreamType(Stream.Music);
        }

        public void PlayAdvice(String[] adviceParts)
        {
            SKAdvisorSettings advisorSettings = SKMaps.Instance.MapInitSettings.AdvisorSettings;
            string soundFilesDirPath = advisorSettings.ResourcePath + advisorSettings.Language + "/sound_files/";

            string temporaryFilePath = soundFilesDirPath + "temp.mp3";

            ByteArrayOutputStream stream = new ByteArrayOutputStream();

            for (int i = 0; i < adviceParts.Length; i++)
            {
                string soundFilePath = soundFilesDirPath + adviceParts[i] + ".mp3";
                try
                {
                    InputStream inputStream = new FileInputStream(new File(soundFilePath));
                    int availableBytes = inputStream.Available();
                    byte[] tmp = new byte[availableBytes];
                    inputStream.Read(tmp, 0, availableBytes);
                    if (stream != null)
                    {
                        stream.Write(tmp);
                    }
                    inputStream.Close();
                }
                catch (Exception)
                {
                }
            }

            WriteFile(stream.ToByteArray(), temporaryFilePath);

            PlayFile(temporaryFilePath);
        }

        private void WriteFile(byte[] data, String filePath)
        {
            OutputStream outputStream = null;
            try
            {
                outputStream = new FileOutputStream(new File(filePath));
                outputStream.Write(data);
                try
                {
                    outputStream.Flush();
                    outputStream.Close();
                }
                catch (Exception)
                {
                }

            }
            catch (Exception)
            {

            }
        }

        private void PlayFile(string filePath)
        {
            try
            {
                _player.Reset();
                File file = new File(filePath);
                FileInputStream fileInputStream = new FileInputStream(file);
                FileDescriptor fileDescriptor = fileInputStream.FD;
                try
                {
                    _player.SetDataSource(fileDescriptor);
                }
                catch (Exception)
                {
                    _player.Reset();
                    _player.SetDataSource(fileDescriptor);
                }
                fileInputStream.Close();

                _player.Prepare();
                _player.Start();
            }
            catch (Exception)
            {

            }
        }
    }
}