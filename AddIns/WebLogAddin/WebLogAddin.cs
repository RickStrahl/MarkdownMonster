
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
using WebLogAddin.MetaWebLogApi;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Utilities;
using File = System.IO.File;

namespace WeblogAddin
{
    public class WebLogAddin :  MarkdownMonsterAddin, IMarkdownMonsterAddin
    {
        private Post ActivePost { get; set; } = new Post();

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "weblog";

            // Create addin and automatically hook menu events
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Weblog Publishing",                
                FontawesomeIcon = FontAwesomeIcon.Wordpress
            };

            // Don't need a configuration dropdown
            menuItem.ExecuteConfiguration = null;

            this.MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
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
                MessageBox.Show("Invalid Weblog configuration selected.",
                                "Weblog Posting Failed",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            WeblogInfo weblogInfo = kv.Value;

            MetaWeblogWrapper wrapper;

            if (type == WeblogTypes.MetaWeblogApi)
                wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.Password,
                    weblogInfo.BlogId) as MetaWeblogWrapper;
            else
                wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.Password) as MetaWeblogWrapper;

            
            string body  = SendImages(html, doc.Filename, wrapper);
            if (body == null)
                return false;

            ActivePost.Body = body;
            ActivePost.PostID = meta.PostId;

            ActivePost.CustomFields = new CustomField[1]
            {
                new CustomField()
                {
                    ID = "mt_markdown",
                    Key = "mt_markdown",
                    Value = meta.MarkdownBody
                }
            };

            bool isNewPost = IsNewPost(ActivePost.PostID);
            try
            {
                if (!isNewPost)
                    wrapper.EditPost(ActivePost, true);
                else
                    ActivePost.PostID = wrapper.NewPost(ActivePost, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending post to Weblog: " + ex.Message,
                    mmApp.ApplicationName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);

                mmApp.Log(ex);
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
        /// determines whether post is a new post based on
        /// a postId of various types
        /// </summary>
        /// <param name="postId">Integer or String or null</param>
        /// <returns></returns>
        bool IsNewPost(object postId)
        {
            if (postId == null)
                return true;

            if (postId is string)
                return string.IsNullOrEmpty((string) postId);

            if (postId is int && (int)postId < 1)
                return true;

            return false;

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

{meta.MarkdownBody}

<!-- Post Configuration -->
<!--
```xml
<abstract>
{meta.Abstract}
</abstract>
<categories>
{meta.Categories}
</categories>
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
            try
            {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error posting images to Weblog: " + ex.Message,
                   mmApp.ApplicationName,
                   MessageBoxButton.OK,
                   MessageBoxImage.Exclamation);
                mmApp.Log(ex);
                return null;
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
<!-- End Post Configuration -->";

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

        public void CreateNewPostOnDisk(string title, string weblogName)
        {

            // strip path of invalid characters
            var invalids = Path.GetInvalidFileNameChars();
            string filename = null;
            foreach (char c in invalids)
                filename = title.Replace(c, '-');

            var folder = Path.Combine(WeblogAddinConfiguration.Current.PostsFolder,DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("00"), filename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var outputFile = Path.Combine(folder, filename + ".md");

            // Create the new post by creating a file with title preset
            string newPostMarkdown = NewWeblogPost(new WeblogPostMetadata()
            {
                Title = title,
                WeblogName = weblogName
            });
            File.WriteAllText(outputFile, newPostMarkdown);
            Model.Window.OpenTab(outputFile);

            mmApp.Configuration.LastFolder = Path.GetDirectoryName(outputFile);

        }

        public void CreateDownloadedPostOnDisk(Post post, string weblogName)
        {
            // strip path of invalid characters
            var invalids = Path.GetInvalidFileNameChars();
            string filename = null;
            foreach (char c in invalids)
                filename = post.Title.Replace(c, '-');

            var folder = Path.Combine(WeblogAddinConfiguration.Current.PostsFolder,"Downloaded Posts",filename + " - " + weblogName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var outputFile = Path.Combine(folder, filename + ".md");

           
            string body = post.Body;
            if (post.CustomFields != null)
            {
                var cf = post.CustomFields.FirstOrDefault(custf => custf.ID == "mt_markdown");
                if (cf != null)
                    body = cf.Value;
            }
            else
                body = MarkdownUtilities.HtmlToMarkdown(body);


            string categories = null;
            if (post.Categories != null && post.Categories.Length > 0)
                categories = string.Join(",", post.Categories);


            // Create the new post by creating a file with title preset
            string newPostMarkdown = NewWeblogPost(new WeblogPostMetadata()
            {
                Title = post.Title,
                MarkdownBody = body,
                Categories = categories,
                Keywords = post.mt_keywords,
                Abstract = post.mt_excerpt,
                PostId = post.PostID.ToString(),                
                WeblogName = weblogName
            });
            File.WriteAllText(outputFile, newPostMarkdown);
            Model.Window.OpenTab(outputFile);

            mmApp.Configuration.LastFolder = Path.GetDirectoryName(outputFile);
        }
    }

}
