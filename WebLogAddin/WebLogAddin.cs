using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FontAwesome.WPF;
using HtmlAgilityPack;
using JoeBlogs;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace WebLogAddin
{
    public class WebLogAddin :  MarkdownMonsterAddin, IMarkdownMonsterAddin
    {
        private Post ActivePost { get; set; }
        private bool IsNewPost { get; set; }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            var menuItem = new AddInMenuItem()
            {
                Caption = "Publish to WebLog",
                EditorCommand = "weblog",
                FontawesomeIcon = FontAwesomeIcon.Wordpress
            };
            menuItem.Execute = new Action<object>(WebLogAddin_Execute);

            this.MenuItems.Add(menuItem);
        }

        
        public void WebLogAddin_Execute(object sender)
        {
            var editor = Model.ActiveEditor;
            if (editor == null)
                return;

            var doc = editor.MarkdownDocument;

            string markdown = editor.GetMarkdown();


            ActivePost = new Post()
            {
                DateCreated = DateTime.Now                                 
            };


            var wrapper = new MetaWeblogWrapper(WeblogApp.Configuration.WeblogApiUrl,
                WeblogApp.Configuration.WeblogUsername,
                WeblogApp.Configuration.WeblogPassword);

            // Retrieve Meta data from post
            markdown = GetPostConfigFromMarkdown(markdown);
            doc.RenderHtml(markdown);
            string html = doc.RenderHtml(markdown);
            ActivePost.Body = SendImages(html,doc.Filename,wrapper);
            
            if (ActivePost.PostID > 0)
                wrapper.EditPost(ActivePost, true);
            else
                ActivePost.PostID = wrapper.NewPost(ActivePost, true);
            
            MessageBox.Show("WebLogAddin Fired and ready to publish this post to your Web log","Markdown Monster",MessageBoxButton.OK,MessageBoxImage.Information);
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

            foreach (HtmlNode img in doc.DocumentNode.SelectNodes("//img"))
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
                            img.Attributes["src"].Value = mediaResult.URL; ;
                        }
                    }
            }

            html = doc.DocumentNode.OuterHtml;
            return html;
        }
        

        /// <summary>
        /// Strips the Markdown Meta data from the message and populates
        /// the post structure with the meta data values.
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private string GetPostConfigFromMarkdown(string markdown)
        {
            string config = StringUtils.ExtractString(markdown, "<!-- Post Configuration -->", "!@#!-1", true, true);
            if (string.IsNullOrEmpty(config))
                return markdown;

            string title = null;

            // check for title in first line
            var lines = StringUtils.GetLines(markdown);
            if (lines.Length > 0 && lines[0].Trim().StartsWith("# "))
            {
                markdown = markdown.Replace(lines[0], "").Trim();
                title = lines[0].Trim().Replace("# ", "");
            }

            // strip the config section
            markdown = markdown.Replace(config, "");

            if (string.IsNullOrEmpty(title))
                title = StringUtils.ExtractString(config, "\n<title>", "\n</title>").Trim();
            string abstact = StringUtils.ExtractString(config, "\n<abstract>", "\n</abstract>").Trim();
            string keywords = StringUtils.ExtractString(config, "\n<keywords>", "\n</keywords>").Trim();
            string categories = StringUtils.ExtractString(config, "\n<keywords>", "\n</keywords>").Trim();
            string postid = StringUtils.ExtractString(config, "\n<postid>", "\n</postid>").Trim();

            ActivePost.Title = title;
            ActivePost.PostID = StringUtils.ParseInt(postid, 0);            
            ActivePost.Categories = categories.Split(new [] { ','},StringSplitOptions.RemoveEmptyEntries);

            ActivePost.mt_excerpt = abstact;
            ActivePost.mt_keywords = keywords;
    
            return markdown;
        }
    }

}
