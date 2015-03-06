using System.Collections.Generic;


namespace Skobbler.SDKDemo.Model
{
    class DownloadPackage
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string Code { get; set; }
        public string ParentCode { get; set; }
        public List<string> ChildrenCodes { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}