
#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 05/15/2016
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using FontAwesome.WPF;
using HtmlAgilityPack;
using JoeBlogs;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace WeblogAddin
{
    public class WebLogAddin :  MarkdownMonsterAddin, IMarkdownMonsterAddin
    {
        private Post ActivePost { get; set; } = new Post();

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            var menuItem = new AddInMenuItem()
            {
                Caption = "Weblog Publishing",
                EditorCommand = "weblog",
                FontawesomeIcon = FontAwesomeIcon.Wordpress
            };
            menuItem.Execute = new Action<object>(WebLogAddin_Execute);

            this.MenuItems.Add(menuItem);
        }

        public void WebLogAddin_Execute(object sender)
        {
            
            var form = new WebLogForm()
            {
                Owner = Model.Window
            };
            form.Model.AppModel = Model;
            form.Model.Addin = this;                       
            form.Show();                       
        }

        /// <summary>
        /// High level method that sends posts to the Weblog
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SendPost(WeblogTypes type = WeblogTypes.MetaWeblogApi)
        {
            var editor = Model.ActiveEditor;
            if (editor == null)
                return false;

            var doc = editor.MarkdownDocument;

            ActivePost = new Post()
            {
                DateCreated = DateTime.Now
            };

            // start by retrieving the current Markdown from the editor
            string markdown = editor.GetMarkdown();

            // Retrieve Meta data from post and clean up the raw markdown
            // so we render without the config data
            var meta = GetPostConfigFromMarkdown(markdown);

            string html = doc.RenderHtml(meta.MarkdownBody, WeblogAddinConfiguration.Current.RenderLinksOpenExternal);

            var config = WeblogAddinConfiguration.Current;

            var kv = config.Weblogs.FirstOrDefault(kvl => kvl.Value.Name == meta.WeblogName);
            if (kv.Equals(default(KeyValuePair<string, WeblogInfo>)))
            {
                MessageBox.Show("Invalid Weblog configuration selected.", "Weblog Posting Failed", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            WeblogInfo weblogInfo = kv.Value;

            MetaWeblogWrapper wrapper;

            if (type == WeblogTypes.MetaWeblogApi)
                wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.Password) as MetaWeblogWrapper;
            else
                wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.Password) as MetaWeblogWrapper;

            ActivePost.Body = SendImages(html, doc.Filename, wrapper);

            try
            {
                if (ActivePost.PostID > 0)
                    wrapper.EditPost(ActivePost, true);
                else
                    ActivePost.PostID = wrapper.NewPost(ActivePost, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending post to Weblog: " + ex.Message,mmApp.ApplicationName,MessageBoxButton.OK,MessageBoxImage.Exclamation);
                return false;
            }

            meta.PostId = ActivePost.PostID.ToString();

            // retrieve the raw editor markdown
            markdown = editor.GetMarkdown();
            meta.RawMarkdownBody = markdown;

            // add the meta configuration to it
            markdown = SetConfigInMarkdown(meta);

            // write it back out to editor
            editor.SetMarkdown(markdown);

            // preview post
            if (!string.IsNullOrEmpty(weblogInfo.PreviewUrl))
            {
                var url = weblogInfo.PreviewUrl.Replace("{0}", ActivePost.PostID.ToString());
                ShellUtils.GoUrl(url);
            }

            return true;
        }

        /// <summary>
        /// Adds a post id to Weblog configuration in a weblog post document.
        /// Only works if [categories] key exists.
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public string AddPostId(string markdown, int postId)
        {
            markdown = markdown.Replace("</categories>",
                    "</categories>\r\n" +
                    "<postid>" + ActivePost.PostID + "</postid>");

            return markdown;
        }

        public string NewWeblogPost(WeblogPostMetadata meta)
        {
            if (meta == null)
            {
                meta = new WeblogPostMetadata()
                {
                    Title = "Post Title",
                };
            }

            if (string.IsNullOrEmpty(meta.WeblogName))
                meta.WeblogName = "Name of registered blog to post to";
            
            return
$@"# {meta.Title}



<!-- Post Configuration -->
<!--
```xml
<abstract>
</abstract>
<categories>
</categories>
<keywords>
</keywords>
<weblog>
{meta.WeblogName}
</weblog>
```
-->
<!-- End Post Configuration -->
";            
        }



        /// <summary>
        /// Parses each of the images in the document and posts them to the server.
        /// Updates the HTML with the returned Image Urls
        /// </summary>
        /// <param name="html"></param>
        /// <param name="filename"></param>
        /// <param name="wrapper"></param>
        /// <returns>update HTML string for the document with updated images</returns>
        private string SendImages(string html, string filename, MetaWeblogWrapper wrapper)
        {
            var basePath = Path.GetDirectoryName(filename);
            var baseName = Path.GetFileName(basePath);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // send up normalized path images as separate media items
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (HtmlNode img in images)
                {
                    string imgFile = img.Attributes["src"]?.Value as string;
                    if (imgFile == null)
                        continue;

                    if (!imgFile.StartsWith("http"))
                    {
                        imgFile = Path.Combine(basePath, imgFile.Replace("/", "\\"));
                        if (System.IO.File.Exists(imgFile))
                        {
                            var media = new MediaObject()
                            {
                                Type = "application/image",
                                Bits = System.IO.File.ReadAllBytes(imgFile),
                                Name = baseName + "/" + Path.GetFileName(imgFile)
                            };
                            var mediaResult = wrapper.NewMediaObject(media);
                            img.Attributes["src"].Value = mediaResult.URL;
                            ;
                        }
                    }
                }
                
                html = doc.DocumentNode.OuterHtml;
            }

            return html;
        }
        

        /// <summary>
        /// Strips the Markdown Meta data from the message and populates
        /// the post structure with the meta data values.
        /// </summary>
        /// <param name="markdown"></param>        
        /// <returns></returns>
        public WeblogPostMetadata GetPostConfigFromMarkdown(string markdown)
        {
            var meta = new WeblogPostMetadata()
            {
                RawMarkdownBody = markdown,
                MarkdownBody = markdown
            };


            string config = StringUtils.ExtractString(markdown,
                "<!-- Post Configuration -->",
                "<!-- End Post Configuration -->",
                caseSensitive: false, allowMissingEndDelimiter: true, returnDelimiters: true);
            if (string.IsNullOrEmpty(config))
                return meta;

            // strip the config section
            meta.MarkdownBody = meta.MarkdownBody.Replace(config, "");


            // check for title in first line and remove it 
            // since the body shouldn't render the title
            var lines = StringUtils.GetLines(markdown);
            if (lines.Length > 0 && lines[0].Trim().StartsWith("# "))
            {
                meta.MarkdownBody = meta.MarkdownBody.Replace(lines[0], "").Trim();
                meta.Title = lines[0].Trim().Replace("# ", "");
            }

            
            if (string.IsNullOrEmpty(meta.Title))
                meta.Title = StringUtils.ExtractString(config, "\n<title>", "\n</title>").Trim();
            meta.Abstract = StringUtils.ExtractString(config, "\n<abstract>", "\n</abstract>").Trim();
            meta.Keywords = StringUtils.ExtractString(config, "\n<keywords>", "\n</keywords>").Trim();
            meta.Categories = StringUtils.ExtractString(config, "\n<categories>", "\n</categories>").Trim();
            meta.PostId = StringUtils.ExtractString(config, "\n<postid>", "</postid>").Trim();
            meta.WeblogName = StringUtils.ExtractString(config, "\n<weblog>", "</weblog>").Trim();

            ActivePost.Title = meta.Title;
            ActivePost.PostID = StringUtils.ParseInt(meta.PostId, 0);            
            ActivePost.Categories = meta.Categories.Split(new [] { ','},StringSplitOptions.RemoveEmptyEntries);

            ActivePost.mt_excerpt = meta.Abstract;
            ActivePost.mt_keywords = meta.Keywords;
    
            return meta;
        }

        /// <summary>
        /// This method sets the RawMarkdownBody
        /// </summary>
        /// <param name="meta"></param>
        /// <returns>Updated Markdown - also sets the RawMarkdownBody and MarkdownBody</returns>
        public string SetConfigInMarkdown(WeblogPostMetadata meta)
        {
            string markdown = meta.RawMarkdownBody;
            string origConfig = StringUtils.ExtractString(markdown, "<!-- Post Configuration -->", "<!-- End Post Configuration -->", false, false, true);
            string newConfig = $@"<!-- Post Configuration -->
<!--
```xml
<abstract>
{meta.Abstract}
</abstract>
<categories>
{meta.Categories}
</categories>
<postid>{meta.PostId}</postid>
<keywords>
{meta.Keywords}
</keywords>
<weblog>
{meta.WeblogName}
</weblog>
```
-->
<!-- End Post Configuration -->
";

            if (string.IsNullOrEmpty(origConfig))
            {
                markdown += "\r\n" + newConfig;
            }
            else
                markdown = markdown.Replace(origConfig, newConfig);

            meta.RawMarkdownBody = markdown;
            meta.MarkdownBody = meta.RawMarkdownBody.Replace(newConfig, "");

            return markdown;
        }
    }

}
