#region License
/*
 **************************************************************
 *  Author: Rick Strahl
 *          © West Wind Technologies,
 *          http://www.west-wind.com/
 *
 * Created: 08/28/2018
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************
*/
#endregion


using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MarkdownMonster;
using Westwind.Utilities;


namespace Westwind.HtmlPackager
{

    /// <summary>
    /// A utility class that can package HTML and all of its dependencies
    /// into either a single file with embedded text and binary resources,
    /// or into a self contained folder that holds the HTML plus its
    /// external dependencies.
    /// </summary>
    public class HtmlPackager
    {
        /// <summary>
        /// A Url or File to load for packaging
        /// </summary>
        public string SourceUrlOrFile { get; set; }


        /// <summary>
        /// The output path where the result HTML file is to be created.
        /// If creating external depedendencies, the dependencies are dumped
        /// into the same folder
        /// </summary>
        public string OutputPath { get; set; }



        /// <summary>
        /// Internal flag to determine if files are
        /// </summary>
        bool CreateExternalFiles { get; set; }

        /// <summary>
        /// Internally tracked base URI for the file or URL so
        /// resources can be discovered and found.
        /// </summary>
        Uri BaseUri { get; set; }


        /// <summary>
        /// Internal naming counter
        /// </summary>
        int ctr = 0;

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

            SourceUrlOrFile = urlOrFile;

            CreateExternalFiles = createExternalFiles;

            HtmlDocument doc;
            var tempFile = Path.GetTempFileName();
            if (urlOrFile.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && urlOrFile.Contains("://"))
            {

                BaseUri = new Uri(SourceUrlOrFile);

                HtmlWeb web = null;
                try
                {
                    web = new HtmlWeb();
                    doc = web.Load(urlOrFile);
                }
                catch (Exception ex)
                {
                    SetError($"Error loading Url: urlOrFile: {ex.Message}");
                    return null;
                }

                var docBase = doc.DocumentNode.SelectSingleNode("//base");
                if (docBase != null)
                {
                    basePath = docBase.Attributes["href"]?.Value;
                    BaseUri = new Uri(baseUri: new Uri(urlOrFile),relativeUri: basePath);
                }

                docBase?.Remove();

                ctr = 0;
                ProcessCss(doc);
                ProcessScripts(doc);
                ProcessImages(doc);
            }
            else
            {

                try
                {
                    doc = new HtmlDocument();
                    doc.Load(urlOrFile);
                }
                catch (Exception ex)
                {
                    SetError($"Error loading HTML file: {ex.Message}");
                    return null;
                }

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

                string oldPath = App.InitialStartDirectory;
                try
                {
                    if (string.IsNullOrEmpty(basePath))
                        basePath = Path.GetDirectoryName(urlOrFile);

                    Directory.SetCurrentDirectory(basePath);
                    BaseUri = new Uri("file:///" + basePath);

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
                var dir = Path.GetDirectoryName(outputFile);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(outputFile, html);
            }
            catch(Exception ex)
            {
                SetError($"Error writing out HTML file: {ex.Message}");
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
        /// <param name="deleteFolderContents">If true deletes folder contents first</param>
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
                Directory.CreateDirectory(OutputPath);


            return PackageHtmlToFile(urlOrFile, outputFile, basePath, true);
        }

        /// <summary>
        /// Packages HTML files to a zip file.
        /// </summary>
        /// <param name="urlOrFile"></param>
        /// <param name="outputZipFile"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public bool PackageHtmlToZipFile(string urlOrFile, string outputZipFile, string basePath = null)
        {
            if (File.Exists(outputZipFile))
                File.Delete(outputZipFile);

            var folder = Path.Combine(Path.GetTempPath(), "_" + DataUtils.GenerateUniqueId());
            var htmlFile = Path.Combine(folder,Path.GetFileName(Path.ChangeExtension(outputZipFile, "html")));

            Directory.CreateDirectory(folder);

            if (!PackageHtmlToFolder(urlOrFile, htmlFile, basePath))
                return false;

            try
            {
                ZipFile.CreateFromDirectory(folder, outputZipFile, CompressionLevel.Fastest, false);
            }
            catch (Exception ex)
            {
                SetError("Unable to package output files: " + ex.Message);
                return false;
            }

            Directory.Delete(folder, true);

            return true;
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

                try
                {
                    if (url.StartsWith("http"))
                    {
                        using (var http = new WebClient())
                        {
                            cssText = http.DownloadString(url);
                        }
                    }
                    else if (url.StartsWith("file:///"))
                    {
                        url = url.Substring(8);
                        cssText = File.ReadAllText(WebUtility.UrlDecode(url));
                    }
                    else // Relative Path
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            using (var http = new WebClient())
                            {
                                cssText = http.DownloadString(url);
                            }
                        }
                        else
                            cssText = File.ReadAllText(uri.LocalPath);
                    }
                }
                catch
                {
                    // Error occurred retrieving a file - continue processing
                    continue;
                }

                cssText = ProcessUrls(cssText, url);

                if (CreateExternalFiles)
                {
                    var justFilename = Path.GetFileName(url);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = DataUtils.GenerateUniqueId(10) + ".css";

                    var fullPath = Path.Combine(OutputPath, justFilename);
                    File.WriteAllText(fullPath, cssText);
                    link.Attributes["href"].Value = justFilename;
                }
                else
                {
                    var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                    el.Name = "style";
                    el.InnerHtml = Environment.NewLine + cssText + Environment.NewLine;

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


                try
                {
                    if (url.StartsWith("http"))
                    {
                        using (var http = new WebClient())
                        {
                            scriptData = http.DownloadData(url);
                        }
                    }
                    else if (url.StartsWith("file:///"))
                    {
                        url = url.Substring(8);
                        scriptData = File.ReadAllBytes(WebUtility.UrlDecode(url));
                    }
                    else // Relative Path
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            using (var http = new WebClient())
                            {
                                scriptData = http.DownloadData(url);
                            }
                        }
                        else
                            scriptData = File.ReadAllBytes(uri.LocalPath);
                    }
                }
                catch
                {
                    // Error retrieving flie
                    continue;
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

                try
                {
                    if (url.StartsWith("http"))
                    {
                        using (var http = new WebClient())
                        {
                            imageData = http.DownloadData(url);
                            contentType = http.ResponseHeaders[System.Net.HttpResponseHeader.ContentType];
                        }
                    }
                    else if (url.StartsWith("file:///"))
                    {
                        url = WebUtility.UrlDecode(url.Substring(8));
                        imageData = File.ReadAllBytes(url);
                        contentType = ImageUtils.GetImageMediaTypeFromFilename(url);
                    }
                    else // Relative Path
                    {
                        var uri = new Uri(BaseUri, url);
                        
                        if (uri.Scheme.StartsWith("http"))
                        {
                            using (var http = new WebClient())
                            {
                                imageData = http.DownloadData(uri.AbsoluteUri);
                            }
                        }
                        else
                            imageData = File.ReadAllBytes(uri.LocalPath);

                        contentType = ImageUtils.GetImageMediaTypeFromFilename(url);
                    }
                }
                catch
                {
                    // error retrieving file - just go on
                    continue;
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

            foreach (Match match in matches)
            {
                string matched = match.Value;
                if (string.IsNullOrEmpty(matched))
                    continue;

                var url = StringUtils.ExtractString(matched, "(", ")")?.Trim(new char[] { '\'', '\"' }).Replace("&amp;", "").Replace("quot;", "");
                if (url.Contains("?"))
                    url = StringUtils.ExtractString(url, "", "?");


                if (url.EndsWith(".eot") || url.EndsWith(".ttf"))
                    continue;


                byte[] linkData = null;
                string contentType = null;
                try
                {
                    if (url.StartsWith("http"))
                    {
                        using (var http = new WebClient())
                        {
                            linkData = http.DownloadData(url);
                            contentType = http.ResponseHeaders[HttpResponseHeader.ContentType];
                        }
                    }
                    else if (url.StartsWith("file:///"))
                    {
                        var uri = new Uri(BaseUri, new Uri(url));
                        url = uri.AbsoluteUri;
                        
                        contentType = ImageUtils.GetImageMediaTypeFromFilename(url);
                        if (contentType == "application/image")
                            continue;

                        linkData = File.ReadAllBytes(uri.LocalPath);
                    }
                    else
                    {
                        var uri = new Uri(BaseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            using (var http = new WebClient())
                            {
                                linkData = http.DownloadData(url);
                            }
                        }
                        else
                            linkData = File.ReadAllBytes(uri.LocalPath);

                        contentType = ImageUtils.GetImageMediaTypeFromFilename(url);
                    }
                }
                catch
                {
                    // if a file can't be found or download - just continue without
                    continue;
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
        #endregion

        #region Error Handler

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            this.SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                this.ErrorMessage = string.Empty;
                return;
            }
            this.ErrorMessage += message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                this.ErrorMessage = string.Empty;
            else
            {
                Exception e = ex;
                if (checkInner)
                    e = e.GetBaseException();

                ErrorMessage = e.Message;
            }
        }
        #endregion
    }

}
