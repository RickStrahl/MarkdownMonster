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

        bool CreateExternalFiles { get; set; }

        public string OutputPath { get; set; }
        Uri BaseUri { get; set; }

        #region Main API

        ///  <summary>
        ///  Packages an HTML document into a large single file package
        ///  that embeds all images, css, scripts, fonts and other url()
        ///  loaded entries into the HTML document.
        /// 
        ///  The result is a very large document that is fully self-contained        
        ///  </summary>
        ///  <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        ///  <param name="basePath">
        ///  An optional basePath for the document which helps resolve relative
        ///  paths. Unless there's a special use case, you should leave this
        ///  value blank and let the default use either the value from a
        ///  BASE tag or the base location of the document.
        /// 
        ///  If the document itself contains a BASE tag this value is not used.
        ///  </param>
        /// <param name="createExternalFiles"></param>
        /// <returns>HTML string or null</returns>
        public string PackageHtml(string urlOrFile, string basePath = null, bool createExternalFiles = false)
        {
            if (string.IsNullOrEmpty(urlOrFile))
                return urlOrFile;

            if (string.IsNullOrEmpty(basePath))
                basePath = Path.GetTempPath();

            UrlOrFile = urlOrFile;

            CreateExternalFiles = createExternalFiles;

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
                    {
                        var tBasePath = url.Replace("file:///", "");
                        if (!string.IsNullOrEmpty(tBasePath) && tBasePath != "\\")
                            basePath = tBasePath;
                    }
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
        /// <returns>HTML string or null</returns>
        public bool PackageHtmlToFile(string urlOrFile, string outputFile, string basePath = null, bool createExternalFiles = false)
        {
            var html = PackageHtml(urlOrFile, basePath, createExternalFiles);
            if (string.IsNullOrEmpty(html))
                return false;

            try
            {
                File.WriteAllText(outputFile, html);
            }
            catch
            {                
                return false;
            }

            return true;
        }

        /// <summary>
        /// Packages an HTML document into a file with all dependencies
        /// dumped into the file's output folder and adjusted for the
        /// same local path.       
        /// </summary>
        /// <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        /// <param name="outputFile">Location for the output file. Folder is created if it doesn't exist. All dependencies are dumped into this folder</param>
        /// <param name="basePath">
        /// An optional basePath for the document which helps resolve relative
        /// paths. Unless there's a special use case, you should leave this
        /// value blank and let the default use either the value from a
        /// BASE tag or the base location of the document.
        ///
        /// If the document itself contains a BASE tag this value is not used.
        /// </param>
        /// <returns>HTML string or null</returns>
        public bool PackageHtmlToFolder(string urlOrFile, string outputFile, string basePath = null, bool deleteFolderContents = false)
        {
            OutputPath = Path.GetDirectoryName(outputFile);

            if (deleteFolderContents && Directory.Exists(OutputPath))
            {
                foreach (var file in Directory.GetFiles(OutputPath))
                    File.Delete(file);
            }
            

            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);                
            }
            
            return PackageHtmlToFile(urlOrFile, outputFile, basePath, true);
        }

        #endregion

        #region Processors

        private void ProcessCss(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//link");
            if (links == null)
                return;
           
            foreach (var link in links)
            {
                var url = link.Attributes["href"]?.Value;
                var rel = link.Attributes["rel"]?.Value;

                if (rel != "stylesheet")
                    continue;

                var originalUrl = url;

                if (url == null)
                    continue;

                string cssText;
                string justFilename = null;

                if (url.StartsWith("http"))
                {
                    var http = new WebClient();
                    cssText = http.DownloadString(url);
                }
                else if (url.StartsWith("file:///"))
                {                                       
                   url = url.Substring(8);
                   cssText = File.ReadAllText(WebUtility.UrlDecode(url));
                   justFilename = Path.GetFileName(url);
                }
                else // Relative Path
                {
                    var uri = new Uri(BaseUri, url);
                    url = uri.AbsoluteUri;
                    if (url.StartsWith("http") && url.Contains("://"))
                    {
                        var http = new WebClient();
                        cssText = http.DownloadString(url);
                    }
                    else
                        cssText = File.ReadAllText(WebUtility.UrlDecode(url));

                    justFilename = Path.GetFileName(url);                    
                }

                cssText = ProcessUrls(cssText, url);

                if (CreateExternalFiles)
                {
                    if (string.IsNullOrEmpty(justFilename))
                        justFilename = DataUtils.GenerateUniqueId(10) + ".css";

                    var fullPath = Path.Combine(OutputPath, justFilename);
                    File.WriteAllText(fullPath, cssText);
                    link.Attributes["href"].Value = justFilename;                    
                }
                else
                {
                    var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                    el.Name = "style";                    
                    el.InnerHtml = "\r\n" + cssText + "\r\n";

                    link.ParentNode.InsertAfter(el, link);
                    link.Remove();
                    el = null;
                }                
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
                    scriptData = File.ReadAllBytes(WebUtility.UrlDecode(url));                    ;
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
                            scriptData = File.ReadAllBytes(WebUtility.UrlDecode(url));
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                if (CreateExternalFiles)
                {
                    var justFilename = Path.GetFileName(url);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = DataUtils.GenerateUniqueId(10) + ".js";

                    var fullPath = Path.Combine(OutputPath,justFilename);
                    File.WriteAllBytes(fullPath,scriptData);
                    script.Attributes["src"].Value = justFilename;
                }
                else
                {
                    string data = $"data:text/javascript;base64,{Convert.ToBase64String(scriptData)}";
                    script.Attributes["src"].Value = data;
                }
            }
        }

        private void ProcessImages(HtmlDocument doc)
        {
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images == null || images.Count < 1)
                return;

            foreach (var image in images)
            {
                var url = image.Attributes["src"]?.Value;
                if (url == null)
                    continue;

                byte[] imageData;
                string contentType;
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
                            imageData = File.ReadAllBytes(WebUtility.UrlDecode(url.Replace("file:///", "")));

                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (imageData == null)
                    continue;


                var el = image;
                el.Name = "img";

                if (CreateExternalFiles)
                {
                    var ext = "png";
                    if (contentType == "image/jpeg")
                        ext = "jpg";
                    else if (contentType == "image/gif")
                        ext = "gif";

                    string justFilename = Path.GetFileName(url);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))                         
                        justFilename = DataUtils.GenerateUniqueId(10) + "." + ext;

                    var fullPath = Path.Combine(OutputPath, justFilename);
                    File.WriteAllBytes(fullPath, imageData);
                    el.Attributes["src"].Value = justFilename;
                }
                else
                {
                    string data = $"data:{contentType};base64,{Convert.ToBase64String(imageData)}";
                    el.Attributes["src"].Value = data;
                }              
            }
        }

        private static Regex urlRegEx = new Regex("url\\(.*?\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Processes embedded url('link') links and embeds the data
        /// and returns the expanded HTML string either with embedded
        /// content, or externalized links.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
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
                if (url.Contains("?"))
                    url = StringUtils.ExtractString(url, "", "?");
                                
                
                if (url.StartsWith("http"))
                {
                    var http = new WebClient();
                    linkData = http.DownloadData(url);
                    contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                }
                else if(url.StartsWith("file:///"))
                {
                    var baseUri = new Uri(baseUrl);
                    url = new Uri(baseUri, new Uri(url)).AbsoluteUri;
                    
                    try
                    {         
                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);                        
                        if (contentType == "application/image")
                            continue;

                         linkData = File.ReadAllBytes(WebUtility.UrlDecode(url));                     
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
                        var baseUri = new Uri(baseUrl);
                        var uri = new Uri(baseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            var http = new WebClient();
                            linkData = http.DownloadData(url);
                        }
                        else
                            linkData = File.ReadAllBytes(WebUtility.UrlDecode(url.Replace("file:///","")));

                        contentType = mmFileUtils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (linkData == null)
                    continue;

                string urlContent = null;
                if (CreateExternalFiles)
                {
                    var ext = "png";
                    if (contentType == "image/jpeg")
                        ext = "jpg";
                    else if (contentType == "image/gif")
                        ext = "gif";

                    string justFilename = Path.GetFileName(url);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = DataUtils.GenerateUniqueId(10) + "." + ext;
                    urlContent = "url('" + justFilename + "')";

                    File.WriteAllBytes(Path.Combine(OutputPath, justFilename),linkData);
                }
                else
                {
                    string data = $"data:{contentType};base64,{Convert.ToBase64String(linkData)}";
                    urlContent = "url('" + data + "')";
                }

                html = html.Replace(matched, urlContent);                
            }

            return html;
        }
    }
    #endregion
}
