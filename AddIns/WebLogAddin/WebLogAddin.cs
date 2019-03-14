
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using FontAwesome.WPF;
using HtmlAgilityPack;
using WebLogAddin.MetaWebLogApi;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using WebLogAddin.Medium;
using Westwind.Utilities;
using File = System.IO.File;

namespace WeblogAddin
{
    public class WebLogAddin :  MarkdownMonsterAddin, IMarkdownMonsterAddin
    {
        public WeblogAddinModel WeblogModel { get; set; } = new WeblogAddinModel();

        public WeblogForm WeblogForm { get; set; }


        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            WeblogModel = new WeblogAddinModel()
            {
                Addin = this,
            };

            Id = "weblog";
            Name = "Weblog Publishing Addin";

            // Create addin and automatically hook menu events
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "Weblog Publishing",
                FontawesomeIcon = FontAwesomeIcon.Rss,
                KeyboardShortcut = WeblogAddinConfiguration.Current.KeyboardShortcut
            };
            try
            {
                menuItem.IconImageSource = new ImageSourceConverter()
                        .ConvertFromString("pack://application:,,,/WeblogAddin;component/icon_22.png") as ImageSource;
            }
            catch { }


            MenuItems.Add(menuItem);
        }

        public override void OnWindowLoaded()
        {
            base.OnWindowLoaded();

            AddMainMenuItems();
        }

        public override void OnExecute(object sender)
        {
            // read settings on startup
            WeblogAddinConfiguration.Current.Read();

            WeblogForm?.Close();
            WeblogForm = new WeblogForm(WeblogModel)
            {
                Owner = Model.Window
            };
            WeblogModel.AppModel = Model;

            WeblogForm.Show();
        }


        public override bool OnCanExecute(object sender)
        {
            return Model.IsEditorActive;
        }

        public override void OnExecuteConfiguration(object sender)
        {
            string file = Path.Combine(mmApp.Configuration.CommonFolder, "weblogaddin.json");
            Model.Window.OpenTab(file);
        }


        public override void OnNotifyAddin(string command, object parameter)
        {
            if (command == "newweblogpost")
                WeblogFormCommand.Execute("newweblogpost");
        }

        #region Post Send Operations

        /// <summary>
        /// High level method that sends posts to the Weblog
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendPost(WeblogInfo weblogInfo, bool sendAsDraft = false)
        {
            var editor = Model.ActiveEditor;
            if (editor == null)
                return false;

            var doc = editor.MarkdownDocument;

            WeblogModel.ActivePost = new Post()
            {
                DateCreated = DateTime.Now
            };

            // start by retrieving the current Markdown from the editor
            string markdown = editor.MarkdownDocument.CurrentText;

            // Retrieve Meta data from post and clean up the raw markdown
            // so we render without the config data
            var meta = WeblogPostMetadata.GetPostConfigFromMarkdown(markdown, WeblogModel.ActivePost, weblogInfo);

            string html = doc.RenderHtml(meta.MarkdownBody, WeblogAddinConfiguration.Current.RenderLinksOpenExternal);
            WeblogModel.ActivePost.Body = html;
            WeblogModel.ActivePost.PostId = meta.PostId;
	        WeblogModel.ActivePost.PostStatus = meta.PostStatus;
            WeblogModel.ActivePost.Permalink = meta.Permalink;

            // Custom Field Processing:
            // Add custom fields from existing post
            // then add or update our custom fields
            var customFields = new Dictionary<string, CustomField>();

            // load existing custom fields from post online if possible
            if (!string.IsNullOrEmpty(meta.PostId))
            {
                var existingPost = GetPost(meta.PostId, weblogInfo);
                if (existingPost != null && meta.CustomFields != null && existingPost.CustomFields != null)
                    customFields = existingPost.CustomFields
                        .ToDictionary(cf => cf.Key, cf => cf);
            }
            // add custom fields from Weblog configuration
            if (weblogInfo.CustomFields != null)
            {
                foreach (var kvp in weblogInfo.CustomFields)
                {
                    if (!customFields.ContainsKey(kvp.Key))
                        AddOrUpdateCustomField(customFields, kvp.Key, kvp.Value);
                }
            }
            // add custom fields from Meta data
            if (meta.CustomFields != null)
            {
                foreach (var kvp in meta.CustomFields)
                {
                    AddOrUpdateCustomField(customFields, kvp.Key, kvp.Value.Value);
                }
            }
            if (!string.IsNullOrEmpty(markdown))
                AddOrUpdateCustomField(customFields, "mt_markdown", markdown);

            WeblogModel.ActivePost.CustomFields = customFields.Values.ToArray();

            var config = WeblogAddinConfiguration.Current;

            var kv = config.Weblogs.FirstOrDefault(kvl => kvl.Value.Name == meta.WeblogName);
            if (kv.Equals(default(KeyValuePair<string, WeblogInfo>)))
            {
                MessageBox.Show("Invalid Weblog configuration selected.",
                    "Weblog Posting Failed",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
            weblogInfo = kv.Value;

            var type = weblogInfo.Type;
            if (type == WeblogTypes.Unknown)
                type = weblogInfo.Type;


            string basePath = Path.GetDirectoryName(doc.Filename);
            string postUrl = null;

            if (type == WeblogTypes.MetaWeblogApi || type == WeblogTypes.Wordpress)
            {
                MetaWebLogWordpressApiClient client;
                client = new MetaWebLogWordpressApiClient(weblogInfo);

                // if values are already configured don't overwrite them again
                client.DontInferFeaturedImage = meta.DontInferFeaturedImage;
                client.FeaturedImageUrl = meta.FeaturedImageUrl;
                client.FeatureImageId = meta.FeaturedImageId;


                var result = await Task.Run<bool>(() => client.PublishCompletePost(WeblogModel.ActivePost, basePath,
                    sendAsDraft, markdown));

                //if (!client.PublishCompletePost(WeblogModel.ActivePost, basePath,
                //    sendAsDraft, markdown))
                if (!result)
                {
                    mmApp.Log($"Error sending post to Weblog at {weblogInfo.ApiUrl}: " + client.ErrorMessage);
                    MessageBox.Show("Error sending post to Weblog: " + client.ErrorMessage,
                        mmApp.ApplicationName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }

                var post = client.GetPost(WeblogModel.ActivePost.PostId);
                if (post != null)
                {
                    postUrl = post.Url;
                    meta.Permalink = post.Permalink;
                }
            }
            if (type == WeblogTypes.Medium)
            {
                var client = new MediumApiClient(weblogInfo);
                var result = client.PublishCompletePost(WeblogModel.ActivePost, basePath, sendAsDraft);
                if (result == null)
                {
                    mmApp.Log($"Error sending post to Weblog at {weblogInfo.ApiUrl}: " + client.ErrorMessage);
                    MessageBox.Show($"Error sending post to Weblog: " + client.ErrorMessage,
                        mmApp.ApplicationName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Exclamation);
                    return false;
                }

                // this is null
                postUrl = client.PostUrl;
            }

            meta.PostId = WeblogModel.ActivePost.PostId?.ToString();


            // retrieve the raw editor markdown
            markdown = editor.MarkdownDocument.CurrentText;
            meta.RawMarkdownBody = markdown;

            // add the meta configuration to it
            markdown = meta.SetPostYaml();

            // write it back out to editor
            editor.SetMarkdown(markdown, updateDirtyFlag: true, keepUndoBuffer: true);

            try
            {
                // preview post
                if (!string.IsNullOrEmpty(weblogInfo.PreviewUrl))
                {
                    var url = weblogInfo.PreviewUrl.Replace("{0}", WeblogModel.ActivePost.PostId.ToString());
                    ShellUtils.GoUrl(url);
                }
                else
                {
                    if (!string.IsNullOrEmpty(postUrl))
                        ShellUtils.GoUrl(postUrl);
                    else
                        ShellUtils.GoUrl(new Uri(weblogInfo.ApiUrl).GetLeftPart(UriPartial.Authority));
                }
            }
            catch
			{
                mmApp.Log("Failed to display Weblog Url after posting: " +
                          weblogInfo.PreviewUrl ?? postUrl ?? weblogInfo.ApiUrl);
            }

            return true;
        }


        /// <summary>
        /// Returns a Post by Id
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="weblogInfo"></param>
        /// <returns></returns>
        public Post GetPost(string postId, WeblogInfo weblogInfo)
        {
            if (weblogInfo.Type == WeblogTypes.MetaWeblogApi || weblogInfo.Type == WeblogTypes.Wordpress)
            {
                MetaWebLogWordpressApiClient client;
                client = new MetaWebLogWordpressApiClient(weblogInfo);
                return client.GetPost(postId);
            }

            // Medium doesn't support post retrieval so return null
            return null;
        }

        private void AddOrUpdateCustomField(IDictionary<string, CustomField> customFields, string key, string value)
        {
            CustomField cf;
            if (customFields.TryGetValue(key, out cf))
            {
                cf.Value = value;
            }
            else
            {
                customFields.Add(key, new CustomField { Key = key, Value = value });
            }
        }

#endregion

#region Local Post Creation

        public string NewWeblogPost(WeblogPostMetadata meta)
        {
            if (meta == null)
            {
                meta = new WeblogPostMetadata()
                {
                    Title = "Post Title",
                    MarkdownBody = string.Empty
                };
            }


            if (string.IsNullOrEmpty(meta.WeblogName))
                meta.WeblogName = "Name of registered blog to post to";

            bool hasFrontMatter = meta.MarkdownBody != null &&
                                  (meta.MarkdownBody.TrimStart().StartsWith("---\n") ||
                                   meta.MarkdownBody.TrimStart().StartsWith("---\r"));
            string post;

            if (hasFrontMatter)
                post = meta.MarkdownBody;
            else
                post =
                $@"# {meta.Title}

{meta.MarkdownBody}
";
            meta.RawMarkdownBody = post;
            meta.MarkdownBody = post;

            if(!hasFrontMatter)
                post = meta.SetPostYaml();

            return post;
        }

        public void CreateNewPostOnDisk(string title, string postFilename, string weblogName)
        {
            string filename = FileUtils.SafeFilename(postFilename);
            string titleFilename = FileUtils.SafeFilename(title);
            titleFilename = titleFilename.Replace(" ", "-").Replace("'", "").Replace("&","and");

            var folder = Path.Combine(WeblogAddinConfiguration.Current.PostsFolder,DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("00"),titleFilename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var outputFile = Path.Combine(folder, filename);

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
                    "<postid>" + WeblogModel.ActivePost.PostId + "</postid>");

            return markdown;
        }

#endregion



#region Downloaded Post Handling

        public void CreateDownloadedPostOnDisk(Post post, string weblogName)
        {
            string filename = FileUtils.SafeFilename(post.Title);

            var folder = Path.Combine(WeblogAddinConfiguration.Current.PostsFolder,
                "Downloaded",weblogName,
                filename);

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var outputFile = Path.Combine(folder, StringUtils.ToCamelCase(filename) + ".md");


            bool isMarkdown = false;
            string body = post.Body;
            string featuredImage = null;

            if (post.CustomFields != null)
            {
                var cf = post.CustomFields.FirstOrDefault(custf => custf.Id == "mt_markdown");
                if (cf != null)
                {
                    body = cf.Value;
                    isMarkdown = true;
                }

                cf = post.CustomFields.FirstOrDefault(custf => custf.Id == "wp_post_thumbnail");
                if (cf != null)
                    featuredImage = cf.Value;
            }
            if (!isMarkdown)
            {
                if (!string.IsNullOrEmpty(post.mt_text_more))
                {
                    // Wordpress ReadMore syntax - SERIOUSLY???
                    if (string.IsNullOrEmpty(post.mt_excerpt))
                        post.mt_excerpt = HtmlUtils.StripHtml(post.Body);

                    body = MarkdownUtilities.HtmlToMarkdown(body) +
                            "\n\n<!--more-->\n\n" +
                            MarkdownUtilities.HtmlToMarkdown(post.mt_text_more);
                }
                else
                    body = MarkdownUtilities.HtmlToMarkdown(body);

            }

            string categories = null;
            if (post.Categories != null && post.Categories.Length > 0)
                categories = string.Join(",", post.Categories);


            // Create the new post by creating a file with title preset
            var meta = new WeblogPostMetadata()
            {
                Title = post.Title,
                MarkdownBody = body,
                Categories = categories,
                Keywords = post.mt_keywords,
                Abstract = post.mt_excerpt,
                PostId = post.PostId.ToString(),
                WeblogName = weblogName,
                FeaturedImageUrl = featuredImage,
                PostDate = post.DateCreated,
                PostStatus = post.PostStatus,
                Permalink = post.Permalink
            };

            string newPostMarkdown = NewWeblogPost(meta);
            File.WriteAllText(outputFile, newPostMarkdown);

            mmApp.Configuration.LastFolder = Path.GetDirectoryName(outputFile);

            if (isMarkdown)
            {
                string html = post.Body;
                string path = mmApp.Configuration.LastFolder;

                // do this synchronously so images show up :-<
                ShowStatus("Downloading post images...", mmApp.Configuration.StatusMessageTimeout);
                SaveMarkdownImages(html, path);
                ShowStatus("Post download complete.", mmApp.Configuration.StatusMessageTimeout);

                //new Action<string,string>(SaveImages).BeginInvoke(html,path,null, null);
            }

            Model.Window.OpenTab(outputFile);
        }

        private void SaveMarkdownImages(string htmlText, string basePath)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlText);

                // send up normalized path images as separate media items
                var images = doc.DocumentNode.SelectNodes("//img");
                if (images != null)
                {
                    foreach (HtmlNode img in images)
                    {
                        string imgFile = img.Attributes["src"]?.Value;
                        if (imgFile == null)
                            continue;

                        if (imgFile.StartsWith("http://") || imgFile.StartsWith("https://"))
                        {
                            string imageDownloadPath = Path.Combine(basePath, Path.GetFileName(imgFile));

                            try
                            {
                                var http = new HttpUtilsWebClient();
                                http.DownloadFile(imgFile, imageDownloadPath);
                            }
                            catch // just continue on errorrs
                            { }
                        }
                    }
                }
            }
            catch // catch so thread doesn't crash
            {
            }
        }

        #endregion


        #region Main Menu Pad for WebLog

        void AddMainMenuItems()
        {

            // create commands
            Command_WeblogForm();


            var mainMenuItem = new MenuItem
            {
                Header = "Web_log"
            };
            AddMenuItem(mainMenuItem, "MainMenuTools", mode: 0);

            var mi = new MenuItem
            {
                Header = "_Post to Weblog",
                Command = WeblogFormCommand,
                CommandParameter = "posttoweblog",
            };
            mainMenuItem.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "_New Weblog Post",
                Command = WeblogFormCommand,
                CommandParameter = "newweblogpost"
            };
            mainMenuItem.Items.Add(mi);


            mi = new MenuItem
            {
                Header = "_Download Weblog Posts",
                Command = WeblogFormCommand,
                CommandParameter = "downloadweblogpost"
            };
            mainMenuItem.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "_Open Weblog Posts Folder",
                Command = WeblogFormCommand,
                CommandParameter = "openweblogfolder"
            };
            mainMenuItem.Items.Add(mi);

            mainMenuItem.Items.Add(new Separator());

            mi = new MenuItem
            {
                Header = "_Configure Weblogs",
                Command = WeblogFormCommand,
                CommandParameter = "configureweblog"
            };
            mainMenuItem.Items.Add(mi);

        }

        public CommandBase WeblogFormCommand { get; set; }

        void Command_WeblogForm()
        {
            WeblogFormCommand = new CommandBase((parameter, command) =>
            {
                var action = parameter as string;
                if (string.IsNullOrEmpty(action))
                    return;

                if (action == "openweblogfolder")
                {
                    ShellUtils.OpenFileInExplorer(WeblogModel.Configuration.PostsFolder);
                    return;
                }

                // actions that require form to be open
                var form = new WeblogForm(WeblogModel)
                {
                    Owner = Model.Window
                };
                form.Model.AppModel = Model;
                form.Show();

                switch (action)
                {
                    case "posttoweblog":
                        form.TabControl.SelectedIndex = 0;
                        break;
                    case "newweblogpost":
                        form.TabControl.SelectedIndex = 1;
                        break;
                    case "downloadweblogpost":
                        form.TabControl.SelectedIndex = 2;
                        break;
                    case "configureweblog":
                        form.TabControl.SelectedIndex = 3;
                        break;
                }
            }, (p, c) =>
            {
                var action = p as string;
                if (string.IsNullOrEmpty(action))
                    return true;

                if (action == "posttoweblog")
                    return Model.ActiveEditor != null;

                return true;
            });
        }

        #endregion
    }



}
