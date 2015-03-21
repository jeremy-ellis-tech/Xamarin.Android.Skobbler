using System;
using System.IO;
using Android.OS;
using Java.IO;
using Java.Lang;
using Java.Lang.Reflect;
using Skobbler.Ngx.Util;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using File = System.IO.File;
using IOException = System.IO.IOException;

namespace Skobbler.Ngx.SDKTools.Download
{
    public class SKToolsDownloadUtils
    {

        /// <summary>
        /// The values used to convert the memory amount to the proper units(KB, MB, GB...)
        /// </summary>
        public static readonly long Kilo = 1024;
        public static readonly long Mega = Kilo * Kilo;
        public static readonly long Giga = Mega * Kilo;
        public static readonly long Terra = Giga * Kilo;
        public static readonly long MinimumFreeMemory = 20 * Mega;

        /// <summary>
        /// removes files/folders corresponding to a certain path </summary>
        /// <param name="currentLocationPath"> current location path </param>
        public static void RemoveCurrentLocationFromDisk(string currentLocationPath)
        {
            string deleteCmd = "rm -r " + currentLocationPath;
            Runtime runtime = Runtime.GetRuntime();

            try
            {
                runtime.Exec(deleteCmd);
                SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "The file was deleted from its current installation folder", SKLogging.LogDebug);
            }
            catch (IOException)
            {
                SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "The file couldn't be deleted !!!", SKLogging.LogDebug);
            }
        }

        /// <summary>
        /// Gets the bytes needed to perform a download </summary>
        /// <param name="neededBytes"> Number of bytes that should be available, on the device, for performing a download </param>
        /// <param name="path"> The path where resources will be downloaded </param>
        /// <returns> Needed bytes in order to perform the current download, or 0 if there are enough available bytes is -1 if given path is wrong </returns>
        public static long GetNeededBytesForADownload(long neededBytes, string path)
        {
            if (path == null)
            {
                return -1;
            }
            if (!IsDataAccessible(path))
            {
                return -1;
            }
            long availableMemorySize;
            if (path.StartsWith("/data", StringComparison.Ordinal))
            { // resources are on internal memory
                availableMemorySize = GetAvailableMemorySize(Environment.DataDirectory.Path);
                if ((neededBytes + MinimumFreeMemory) <= availableMemorySize)
                {
                    return 0;
                }
                return (neededBytes + MinimumFreeMemory - availableMemorySize);
            }
            // resources are on other storage
            string memoryPath = null;
            int androidFolderIndex = path.IndexOf("/Android", StringComparison.Ordinal);
            if ((androidFolderIndex > 0) && (androidFolderIndex < path.Length))
            {
                memoryPath = path.Substring(0, androidFolderIndex);
            }
            if (memoryPath == null)
            {
                return -1;
            }
            availableMemorySize = GetAvailableMemorySize(memoryPath);
            if ((neededBytes + MinimumFreeMemory) <= availableMemorySize)
            {
                return 0;
            }
            return (neededBytes + MinimumFreeMemory - availableMemorySize);
        }

        /// <summary>
        /// gets the available internal memory size </summary>
        /// <returns> available memory size in bytes </returns>
        public static long GetAvailableMemorySize(string path)
        {
            StatFs statFs = null;
            try
            {
                statFs = new StatFs(path);
            }
            catch (ArgumentException ex)
            {
                SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "Exception when creating StatF ; message = " + ex, SKLogging.LogDebug);
            }
            if (statFs != null)
            {
                Method getAvailableBytesMethod = null;
                try
                {
                    getAvailableBytesMethod = statFs.Class.GetMethod("getAvailableBytes");
                }
                catch (NoSuchMethodException e)
                {
                    SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "Exception at getAvailableMemorySize method = " + e.Message, SKLogging.LogDebug);
                }

                if (getAvailableBytesMethod != null)
                {
                    try
                    {
                        SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "Using new API for getAvailableMemorySize method !!!", SKLogging.LogDebug);
                        return (long)getAvailableBytesMethod.Invoke(statFs);
                    }
                    catch (IllegalAccessException)
                    {
                        return statFs.AvailableBlocks * (long)statFs.BlockSize;
                    }
                    catch (InvocationTargetException)
                    {
                        return statFs.AvailableBlocks * (long)statFs.BlockSize;
                    }
                }
                return statFs.AvailableBlocks * (long)statFs.BlockSize;
            }
            return 0;
        }

        /// <summary>
        /// Checks if data on this path is accessible
        /// </summary>
        /// <param name="path"> The path whose availability is checked </param>
        /// <returns> True if the data from the given path is accessible, false otherwise (data erased, SD card removed, etc) </returns>
        public static bool IsDataAccessible(string path)
        {
            // if file is on internal memory, check its existence
            if (path != null)
            {
                if (path.StartsWith("/data", StringComparison.Ordinal))
                {
                    return Directory.Exists(path) || File.Exists(path);
                }
                string memoryPath = null;
                int androidFolderIndex = path.IndexOf("/Android", StringComparison.Ordinal);
                if (androidFolderIndex > 0 && androidFolderIndex < path.Length)
                {
                    memoryPath = path.Substring(0, androidFolderIndex);
                }
                if (memoryPath != null)
                {
                    bool check = false;
                    try
                    {
                        FileStream fs = new FileStream("/proc/mounts", FileMode.OpenOrCreate); 
                        StreamReader @in = new StreamReader(fs);
                        //BufferedReader br = new BufferedReader(new InputStreamReader(@in));
                        BufferedReader br = null;
                        string strLine;
                        while ((strLine = br.ReadLine()) != null && !check)
                        {
                            if (strLine.Contains(memoryPath))
                            {
                                check = true;
                            }
                        }
                        br.Close();
                    }
                    catch (Exception e)
                    {
                        SKLogging.WriteLog(SKToolsDownloadPerformer.Tag, "Exception in isDataAccessible method ; message = " + e.Message, SKLogging.LogDebug);
                    }
                    return check && Directory.Exists(path) || File.Exists(path);
                }
                return false;
            }
            return false;
        }
    }
}