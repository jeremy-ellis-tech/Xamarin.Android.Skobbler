using Javax.Xml.Parsers;
using Org.Xml.Sax;
using Org.Xml.Sax.Helpers;
using Skobbler.SDKDemo.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Skobbler.SDKDemo.Util
{
    class MapDataParser
    {

        private SAXParser saxParser;
        private String _url;
        private Dictionary<string, DownloadPackage> packageMap = new Dictionary<string, DownloadPackage>();

        public MapDataParser(String url)
        {
            _url = url;

            try
            {
                saxParser = SAXParserFactory.NewInstance().NewSAXParser();
            }
            catch (Exception)
            {
            }
        }

        public Dictionary<string, DownloadPackage> PackageMap
        {
            get { return packageMap; }
        }

        public async void Parse()
        {
            using (var httpClient = new HttpClient())
            {
                Stream responseStream;
                try
                {
                    responseStream = await httpClient.GetStreamAsync(_url);
                    InputSource inputSource = new InputSource(responseStream);
                    inputSource.Encoding = "UTF-8";
                    var parsingHandler = new MapsXMLParserHandler(this);
                    saxParser.Parse(inputSource, parsingHandler);
                }
                catch (Exception)
                {
                }

            }
        }

        private class MapsXMLParserHandler : DefaultHandler
        {
            private const string TagPackages = "packages";
            private const string TagWorld = "world";
            private const string TagType = "type";
            private const string TagSize = "size";
            private const string TagEnglishName = "en";
            private Stack<String> tagStack = new Stack<String>();
            private DownloadPackage currentPackage;
            private MapDataParser _mapDataParser;

            public MapsXMLParserHandler(MapDataParser mapDataParser)
            {
                _mapDataParser = mapDataParser;
            }

            public override void StartElement(string uri, string localName, string qName, IAttributes attributes)
            {
                if (tagStack.Contains(TagPackages) && tagStack.Peek() == TagPackages)
                {
                    currentPackage = new DownloadPackage();
                    currentPackage.Code = localName;
                }

                if (tagStack.Contains(TagWorld) && tagStack.ElementAt(tagStack.Count - 1) != TagWorld)
                {
                    string parentCode = tagStack.Peek();
                    _mapDataParser.PackageMap[localName].ParentCode = parentCode;
                    _mapDataParser.PackageMap[parentCode].ChildrenCodes.Add(localName);
                }

                tagStack.Push(localName);
            }

            public void EndElement(String uri, String localName, String qName)
            {
                tagStack.Pop();
                if (tagStack.Contains(TagPackages) && tagStack.Peek() == TagPackages)
                {
                    _mapDataParser.PackageMap.Add(currentPackage.Code, currentPackage);
                }
            }

            public void Characters(char[] ch, int start, int length)
            {
                String content = new String(ch, start, length);
                if (tagStack.Peek() == TagEnglishName)
                {
                    currentPackage.Name = content;
                }
                else if (tagStack.Peek() == TagType)
                {
                    currentPackage.Type = content;
                }
                else if (tagStack.Peek() == TagSize && tagStack.ElementAt(tagStack.Count - 2) == currentPackage.Code)
                {
                    currentPackage.Size = Int32.Parse(content);
                }
            }
        }
    }
}