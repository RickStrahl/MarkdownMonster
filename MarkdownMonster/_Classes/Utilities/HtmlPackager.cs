using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Westwind.Utilities;

namespace MarkdownMonster.Utilities
{

    /// <summary>
    /// Class that packages up an HTML file into a single file by
    /// pulling scripts, css and image resources inline.
    /// </summary>
    public class HtmlPackager
    {

        int ctr = 0;

        public string PackageLocalHtml(string filename, string basePath = null)
        {
            var doc = new HtmlDocument();
            doc.Load(filename);

            if (string.IsNullOrEmpty(basePath))
                basePath = Path.GetDirectoryName(filename);

            var docBase = doc.DocumentNode.SelectSingleNode("//base");
            if (docBase != null)
            {
                var url = docBase.Attributes["href"]?.Value;
                if (url.StartsWith("file:///"))
                    basePath = url.Replace("file:///", "");
            }
            
            string oldPath = Environment.CurrentDirectory;
            try
            {
                Directory.SetCurrentDirectory(basePath);

                ctr = 0;
                ProcessCss(doc);
                ProcessScripts(doc);
                ProcessImages(doc);
            }
            finally
            {
                Directory.SetCurrentDirectory(oldPath);
            }
            
            docBase?.Remove();

            var html = doc.DocumentNode.InnerHtml;
            //html = ProcessUrls(html, basePath);
            return html;
        }

        private void ProcessCss(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//link");
            if (links == null)
                return;
           
            foreach (var link in links)
            {
                var url = link.Attributes["href"]?.Value;
                var originalUrl = url;

                if (url == null)
                    continue;

                string linkData;
                if (url.StartsWith("http"))
                    linkData = HttpUtils.HttpRequestString(url);
                else
                {                    
                    if (url.StartsWith("file:///"))
                        url = url.Substring(8);

                    linkData = File.ReadAllText(url);
                }

                linkData = ProcessUrls(linkData, originalUrl);
                
                var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                el.Name = "style";
                el.InnerHtml = "\r\n" + linkData + "\r\n";

                link.ParentNode.InsertAfter(el, link);
                link.Remove();
            }            
        }

        private void ProcessScripts(HtmlDocument doc)
        {
            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts == null || scripts.Count < 1)
                return;

            
            foreach (var script in scripts)
            {
                var url = script.Attributes["src"]?.Value;
                if (url == null)
                    continue;

                byte[] scriptData;
                if (url.StartsWith("http"))
                {
                    var settings = new HttpRequestSettings
                    {
                        Url = url,
                    };

                    var http = new WebClient();
                    scriptData = http.DownloadData(url);                    
                }
                else
                {
                    if (url.StartsWith("file:///"))
                        url = url.Substring(8);

                    scriptData = File.ReadAllBytes(url);
                }



                string data = $"data:text/javascript;base64,{Convert.ToBase64String(scriptData)}";

                var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                el.Name = "script";
                el.Attributes.Add("src", data);
                
                script.ParentNode.InsertAfter(el, script);
                script.Remove();

                el = null;
            }
        }

        private void ProcessImages(HtmlDocument doc)
        {
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images == null || images.Count < 1)
                return;

            string contentType;
            foreach (var image in images)
            {
                var url = image.Attributes["src"]?.Value;
                if (url == null)
                    continue;

                byte[] linkData;
                if (url.StartsWith("http"))
                {
                    var settings = new HttpRequestSettings
                    {
                        Url = url,
                    };

                    var http = new WebClient();
                    linkData = http.DownloadData(url);
                    contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                }
                else
                {
                    if (url.StartsWith("file:///"))
                        url = url.Substring(8);

                    try
                    {
                        linkData = File.ReadAllBytes(url);

                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
                    }
                    catch { continue; }
                }

                if (linkData == null)
                    continue;

                string data = $"data:{contentType};base64,{Convert.ToBase64String(linkData)}";

                var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                el.Name = "img";
                el.Attributes.Add("src",data);

                image.ParentNode.InsertAfter(el, image);
                image.Remove();

                el = null;
            }
        }

        private static Regex urlRegEx = new Regex("url\\(.*?\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);


        private string ProcessUrls(string html, string baseUrl)
        {
            var matches = urlRegEx.Matches(html);
            string contentType = null;
            byte[] linkData = null;

            foreach (Match match in matches)
            {
                string matched = match.Value;
                if (string.IsNullOrEmpty(matched))
                    continue;

                var url = matched.Substring(5, matched.Length - 7);
                                
                
                if (url.StartsWith("http"))
                {
                    var http = new WebClient();
                    linkData = http.DownloadData(url);
                    contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                }
                else
                {
                    if (url.StartsWith("file:///"))
                        url = url.Substring(8);

                    if (baseUrl.StartsWith("file:///"))
                        baseUrl = baseUrl.Substring(8);

                    var basePath = Path.GetDirectoryName(baseUrl);
                    basePath += "\\";

                    if (!string.IsNullOrEmpty(basePath) && !basePath.EndsWith("/") && !basePath.EndsWith("\\") )
                        basePath += "\\";

                    int atQ = url.IndexOf("?");
                    if (atQ > -1)
                        url = url.Substring(0, atQ);

                    try
                    {

                        url = FileUtils.NormalizePath(basePath + url);
                        url = FileUtils.GetPhysicalPath(url);


                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);                        
                        if (contentType == "application/image")
                            continue;

                         linkData = File.ReadAllBytes(url);                     
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (linkData == null)
                    continue;

                string data = $"data:{contentType};base64,{Convert.ToBase64String(linkData)}";
                var replace = "url('" + data + "')";

                html = html.Replace(matched, replace);                
            }

            return html;
        }
    }
}
