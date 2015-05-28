using System;
using System.Collections.Generic;
using System.Text;
using Java.Util;
using Skobbler.Ngx.Packages;
using Skobbler.Ngx.SDKTools.Download;

namespace Skobbler.SDKDemo.Database
{
    public class MapDownloadResource : DownloadResource, IComparable<MapDownloadResource>
    {

        /**
         * resource parent code
         */
        private string parentCode;

        /**
         * resource name in different languages
         */
        private Dictionary<string, string> names;

        /**
         * resource sub-type (e.g. continent, country, city, state for map resource)
         */
        private string subType;

        /**
         * SKM file size
         */
        private long skmFileSize;

        /**
         * SKM + ZIP file size
         */
        private long skmAndZipFilesSize;

        /**
         * txg file size
         */
        private long txgFileSize;

        /**
         * UNZIPPED file size for zip file
         */
        private long unzippedFileSize;

        /**
         * .SKM file path
         */
        private string skmFilePath;

        /**
         * .ZIP file path
         */
        private string zipFilePath;

        /**
         * .TXG file path
         */
        private string txgFilePath;

        /**
         * bounding box minimum longitude
         */
        private double bbLongMin;

        /**
         * bounding box maximum longitude
         */
        private double bbLongMax;

        /**
         * bounding box minimum latitude
         */
        private double bbLatMin;

        /**
         * bounding box maximum latitude
         */
        private double bbLatMax;

        /**
         * constructs an object of SKDownloadResource type
         */
        public MapDownloadResource()
        {
            names = new Dictionary<string, string>();
        }

        public string ParentCode { get; set; }

        /**
         * @return the resource name
         */
        public String getName()
        {
            String localLanguage = Locale.Default.Language;
            if (localLanguage.StartsWith(MapsDAO.ENGLISH_LANGUAGE_CODE))
            {
                localLanguage = MapsDAO.ENGLISH_LANGUAGE_CODE;
            }
            if (names != null)
            {
                if (names[localLanguage] == null)
                {
                    return names[Locale.English.Language];
                }
                else
                {
                    return names[localLanguage];
                }
            }
            else
            {
                return "";
            }
        }

        /**
         * sets the name in all languages
         * @param newNames resource names in all languages
         */
        public void setNames(string newNames)
        {
            this.names = new Dictionary<string, string>();
            string[] keyValuePairs = newNames.Split(';');
            foreach (string keyValue in keyValuePairs)
            {
                string[] newName = keyValue.Split('=');
                this.names.Add(newName[0], newName[1]);
            }
        }

        /**
         * @param name the resource name for a certain language
         */
        public void setName(String name, String language)
        {
            names.Add(language, name);
        }

        /**
         * @return names in all languages for current resource
         */
        public Dictionary<string, string> getNames()
        {
            return names;
        }

        /**
         * @return the resource sub-type
         */
        public String getSubType()
        {
            return subType;
        }

        /**
         * @param subType the resource sub-type
         */
        public void setSubType(String subType)
        {
            this.subType = subType;
        }

        /**
         * @return SKM file size
         */
        public long getSkmFileSize()
        {
            return skmFileSize;
        }

        /**
         * @param skmFileSize SKM file size
         */
        public void setSkmFileSize(long skmFileSize)
        {
            this.skmFileSize = skmFileSize;
        }

        /**
         * @return the SKM + ZIP files size
         */
        public long getSkmAndZipFilesSize()
        {
            return skmAndZipFilesSize;
        }

        /**
         * @param skmAndZipFilesSize the SKM + ZIP files size
         */
        public void setSkmAndZipFilesSize(long skmAndZipFilesSize)
        {
            this.skmAndZipFilesSize = skmAndZipFilesSize;
        }

        /**
         * @return the TXG file size
         */
        public long getTXGFileSize()
        {
            return txgFileSize;
        }

        /**
         * @param txgFileSize the TXG file size
         */
        public void setTXGFileSize(long txgFileSize)
        {
            this.txgFileSize = txgFileSize;
        }

        /**
         * @return the unzippedSize for ZIP file
         */
        public long getUnzippedFileSize()
        {
            return unzippedFileSize;
        }

        /**
         * @param unzippedFileSize the unzippedSize for ZIP file
         */
        public void setUnzippedFileSize(long unzippedFileSize)
        {
            this.unzippedFileSize = unzippedFileSize;
        }

        /**
         * @return the SKM file path
         */
        public String getSKMFilePath()
        {
            return skmFilePath;
        }

        /**
         * @param skmFilePath the SKM file path
         */
        public void setSkmFilePath(String skmFilePath)
        {
            this.skmFilePath = skmFilePath;
        }

        /**
         * @return the ZIP file path
         */
        public String getZipFilePath()
        {
            return zipFilePath;
        }

        /**
         * @param zipFilePath the ZIP file path
         */
        public void setZipFilePath(String zipFilePath)
        {
            this.zipFilePath = zipFilePath;
        }

        /**
         * @return the TXG file path
         */
        public String getTXGFilePath()
        {
            return txgFilePath;
        }

        /**
         * @param txgFilePath the TXG file path
         */
        public void setTXGFilePath(String txgFilePath)
        {
            this.txgFilePath = txgFilePath;
        }

        /**
         * @return bounding-box longitude minim
         */
        public double getBbLongMin()
        {
            return bbLongMin;
        }

        /**
         * @param bbLongMin bounding-box longitude minim
         */
        public void setBbLongMin(double bbLongMin)
        {
            this.bbLongMin = bbLongMin;
        }

        /**
         * @return bounding-box longitude maxim
         */
        public double getBbLongMax()
        {
            return bbLongMax;
        }

        /**
         * @param bbLongMax bounding-box longitude maxim
         */
        public void setBbLongMax(double bbLongMax)
        {
            this.bbLongMax = bbLongMax;
        }

        /**
         * @return bounding-box latitude minim
         */
        public double getBbLatMin()
        {
            return bbLatMin;
        }

        /**
         * @param bbLatMin bounding-box latitude minim
         */
        public void setBbLatMin(double bbLatMin)
        {
            this.bbLatMin = bbLatMin;
        }

        /**
         * @return bounding-box latitude maxim
         */
        public double getBbLatMax()
        {
            return bbLatMax;
        }

        /**
         * @param bbLatMax bounding-box latitude maxim
         */
        public void setBbLatMax(double bbLatMax)
        {
            this.bbLatMax = bbLatMax;
        }

        public int FlagId { get; set; }

        public int CompareTo(MapDownloadResource another)
        {
            String firstName = getName(), secondName = (another != null) ? another.getName() : null;
            if ((firstName != null) && (secondName != null))
            {
                return firstName.ToLower().CompareTo(secondName.ToLower());
            }
            else if (firstName != null)
            {
                return -1;
            }
            else if (secondName != null)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public override bool Equals(Object another)
        {
            if (another == null)
            {
                return false;
            }
            else if (!(another is MapDownloadResource))
            {
                return false;
            }
            else
            {
                MapDownloadResource anotherResource = (MapDownloadResource)another;
                return this.Code.Equals(anotherResource.Code);
            }
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override SKToolsDownloadItem ToDownloadItem()
        {
            SKPackageURLInfo info = SKPackageManager.Instance.GetURLInfoForPackageWithCode(Code);
            List<SKToolsFileDownloadStep> downloadSteps = new List<SKToolsFileDownloadStep>();
            downloadSteps.Add(new SKToolsFileDownloadStep(info.MapURL, new StringBuilder(DownloadPath).Append(Code)
                    .Append(SKToolsDownloadManager.SkmFileExtension).ToString(), skmFileSize));
            if (txgFileSize != 0)
            {
                downloadSteps.Add(new SKToolsFileDownloadStep(info.TexturesURL, new StringBuilder(DownloadPath).Append(Code)
                        .Append(SKToolsDownloadManager.TxgFileExtension).ToString(), txgFileSize));
            }
            if (unzippedFileSize != 0)
            {
                downloadSteps.Add(new SKToolsFileDownloadStep(info.NameBrowserFilesURL, new StringBuilder(DownloadPath).Append(Code)
                        .Append(SKToolsDownloadManager.ZipFileExtension).ToString(), (skmAndZipFilesSize - skmFileSize)));
            }
            SKToolsDownloadItem currentItem = new SKToolsDownloadItem(Code, downloadSteps, DownloadState, (unzippedFileSize != 0), true);
            currentItem.NoDownloadedBytes = (NoDownloadedBytes);
            return currentItem;
        }
    }
}