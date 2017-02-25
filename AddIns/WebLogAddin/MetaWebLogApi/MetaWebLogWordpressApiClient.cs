using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using MarkdownMonster;
using WeblogAddin;

namespace WebLogAddin.MetaWebLogApi
{
    public class MetaWebLogWordpressApiClient
    {
        public  string ErrorMessage { get; set; }


        public string FeatureImageId { get; set; }

        public string FeaturedImageUrl { get; set; }



        public bool PublishCompletePost(Post post, WeblogInfo weblogInfo, string basePath = null, bool sendAsDraft = false)
        {
            WeblogTypes type = weblogInfo.Type;
            if (type == WeblogTypes.Unknown)
                type = weblogInfo.Type;

            MetaWeblogWrapper wrapper;

            if (type == WeblogTypes.MetaWeblogApi)
                wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.DecryptPassword(weblogInfo.Password),
                    weblogInfo.BlogId);
            else
                wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
                    weblogInfo.Username,
                    weblogInfo.DecryptPassword(weblogInfo.Password));


            string body = post.Body;
            try
            {
                body = SendImages(body, basePath, wrapper);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error sending images to Weblog at {weblogInfo.ApiUrl}: " + ex.Message;
                mmApp.Log($"Error sending images to Weblog at {weblogInfo.ApiUrl}: ", ex);
                return false;
            }

            if (body == null)
                return false;

            post.Body = body;
            
            
            bool isNewPost = IsNewPost(post.PostID);
            try
            {
                if (!isNewPost)
                    wrapper.EditPost(post, !sendAsDraft);
                else
                    post.PostID = wrapper.NewPost(post, !sendAsDraft);
            }
            catch (Exception ex)
            {
                mmApp.Log($"Error sending post to Weblog at {weblogInfo.ApiUrl}: ", ex);
                ErrorMessage = $"Error sending post to Weblog: " + ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses each of the images in the document and posts them to the server.
        /// Updates the HTML with the returned Image Urls
        /// </summary>
        /// <param name="html">HTML that contains images</param>
        /// <param name="basePath">image file name</param>
        /// <param name="wrapper">blog wrapper instance that sends</param>
        /// <param name="metaData">metadata containing post info</param>
        /// <returns>update HTML string for the document with updated images</returns>
        private string SendImages(string html, string basePath,
                                  MetaWeblogWrapper wrapper)
        {
            
            // base folder name for uploads - just the folder name of the image
            var baseName = Path.GetFileName(basePath);
            baseName = mmFileUtils.SafeFilename(baseName).Replace(" ", "-");

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

                        if (!imgFile.StartsWith("http://") && !imgFile.StartsWith("https://"))
                        {
                            if (!imgFile.Contains(":\\"))
                                imgFile = Path.Combine(basePath, imgFile.Replace("/", "\\"));

                            if (System.IO.File.Exists(imgFile))
                            {
                                var media = new MediaObject()
                                {
                                    Type = mmFileUtils.GetImageMediaTypeFromFilename(imgFile),
                                    Bits = System.IO.File.ReadAllBytes(imgFile),
                                    Name = baseName + "/" + Path.GetFileName(imgFile)
                                };
                                var mediaResult = wrapper.NewMediaObject(media);
                                img.Attributes["src"].Value = mediaResult.URL;

                                // use first image as featured image
                                if (string.IsNullOrEmpty(this.FeaturedImageUrl))
                                    FeaturedImageUrl = mediaResult.URL;
                                if (string.IsNullOrWhiteSpace(FeatureImageId))
                                    FeatureImageId = mediaResult.Id;

                            }
                        }
                    }

                    html = doc.DocumentNode.OuterHtml;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error posting images to Weblog: " + ex.Message;
                return null;
            }

            return html;
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
                return string.IsNullOrEmpty((string)postId);

            if (postId is int && (int)postId < 1)
                return true;

            return false;
        }
    }
}
