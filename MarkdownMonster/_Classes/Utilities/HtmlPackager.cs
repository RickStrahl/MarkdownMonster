using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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
        
        public string UrlOrFile { get; set; }

        Uri BaseUri { get; set; }


        /// <summary>
        /// Packages an HTML document into a large single file package
        /// that embeds all images, css, scripts, fonts and other url()
        /// loaded entries into the HTML document.
        ///
        /// The result is a very large document that is fully self-contained        
        /// </summary>
        /// <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        /// <param name="basePath">
        /// An optional basePath for the document which helps resolve relative
        /// paths. Unless there's a special use case, you should leave this
        /// value blank and let the default use either the value from a
        /// BASE tag or the base location of the document.
        ///
        /// If the document itself contains a BASE tag this value is not used.
        /// </param>
        /// <returns></returns>
        public string PackageHtml(string urlOrFile, string basePath = null)
        {
            if (string.IsNullOrEmpty(urlOrFile))
                return urlOrFile;

            UrlOrFile = urlOrFile;

            HtmlDocument doc;
            var tempFile = Path.GetTempFileName();
            if (urlOrFile.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && urlOrFile.Contains("://"))
            {

                BaseUri = new Uri(UrlOrFile);

                var web = new HtmlWeb();
                doc = web.Load(urlOrFile);

                var docBase = doc.DocumentNode.SelectSingleNode("//base");
                if (docBase != null)
                {
                    basePath = docBase.Attributes["href"]?.Value;
                    BaseUri = new Uri(basePath);
                }

                docBase?.Remove();

                ctr = 0;
                ProcessCss(doc);
                ProcessScripts(doc);
                ProcessImages(doc);
            }
            else
            {
                doc = new HtmlDocument();
                doc.Load(urlOrFile);

                var docBase = doc.DocumentNode.SelectSingleNode("//base");
                if (docBase != null)
                {
                    var url = docBase.Attributes["href"]?.Value;
                    if (url.StartsWith("file:///"))
                        basePath = url.Replace("file:///", "");
                }
                docBase?.Remove();

                string oldPath = Environment.CurrentDirectory;
                try
                {
                    if (string.IsNullOrEmpty(basePath))
                        basePath = Path.GetDirectoryName(urlOrFile);

                    Directory.SetCurrentDirectory(basePath);
                    BaseUri = new Uri(basePath);

                    ctr = 0;
                    ProcessCss(doc);
                    ProcessScripts(doc);
                    ProcessImages(doc);
                }
                finally
                {
                    Directory.SetCurrentDirectory(oldPath);
                }
            }


            var html = doc.DocumentNode.InnerHtml;
            //html = ProcessUrls(html, basePath);

            if (tempFile != null)
                File.Delete(tempFile);

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
                {
                    var http = new WebClient();
                    linkData = http.DownloadString(url);
                }
                else if (url.StartsWith("file:///"))
                {                                       
                   url = url.Substring(8);
                   linkData = File.ReadAllText(url);
                }
                else // Relative Path
                {
                    var uri = new Uri(BaseUri, url);
                    url = uri.AbsoluteUri;
                    if (url.StartsWith("http") && url.Contains("://"))
                    {
                        var http = new WebClient();
                        linkData = http.DownloadString(url);
                    }
                    else
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
                    var http = new WebClient();
                    scriptData = http.DownloadData(url);                    
                }
                else if (url.StartsWith("file:///"))
                {
                    url = url.Substring(8);
                    scriptData = File.ReadAllBytes(url);
                }
                else // Relative Path
                {
                    try
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            var http = new WebClient();
                            scriptData = http.DownloadData(url);
                        }
                        else
                            scriptData = File.ReadAllBytes(url);
                    }
                    catch
                    {
                        continue;
                    }
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

                byte[] imageData;
                if (url.StartsWith("http"))
                {
                    var http = new WebClient();
                    imageData = http.DownloadData(url);
                    contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                }
                else if(url.StartsWith("file:///"))
                {
                    url = url.Substring(8);

                    try
                    {
                        imageData = File.ReadAllBytes(url);
                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else // Relative Path
                {
                    try
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            var http = new WebClient();
                            imageData = http.DownloadData(url);
                        }
                        else
                            imageData = File.ReadAllBytes(url.Replace("file:///", ""));

                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (imageData == null)
                    continue;

                string data = $"data:{contentType};base64,{Convert.ToBase64String(imageData)}";

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

                var url = StringUtils.ExtractString(matched,"(", ")")?.Trim(new char[] {'\'', '\"'}).Replace("&amp;","").Replace("quot;","");
                                
                
                if (url.StartsWith("http"))
                {
                    var http = new WebClient();
                    linkData = http.DownloadData(url);
                    contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                }
                else if(url.StartsWith("file:///"))
                {

                    url = new Uri(BaseUri, new Uri(url)).AbsoluteUri;
                    
                    try
                    {         
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
                else
                {
                    try
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            var http = new WebClient();
                            linkData = http.DownloadData(url);
                        }
                        else
                            linkData = File.ReadAllBytes(url.Replace("file:///",""));

                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
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
