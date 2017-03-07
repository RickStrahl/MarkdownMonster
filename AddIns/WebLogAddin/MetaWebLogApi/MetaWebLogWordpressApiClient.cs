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

        public readonly WeblogInfo WeblogInfo;

        public string PostUrl { get; set; }

        public MetaWebLogWordpressApiClient(WeblogInfo weblogInfo)
        {
            WeblogInfo = weblogInfo;
        }
        
        public bool PublishCompletePost(Post post,  
            string basePath = null, 
            bool sendAsDraft = false,
            string markdown = null)
        {            
            WeblogTypes type = WeblogInfo.Type;
            if (type == WeblogTypes.Unknown)
                type = WeblogInfo.Type;

            

            var wrapper = GetWrapper();

            string body = post.Body;
            try
            {
                body = SendImages(body, basePath, wrapper);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error sending images to Weblog at {WeblogInfo.ApiUrl}: " + ex.Message;
                mmApp.Log($"Error sending images to Weblog at {WeblogInfo.ApiUrl}: ", ex);
                return false;
            }

            if (body == null)
                return false;

            post.Body = body;

            var customFields = new List<CustomField>();
            if (!string.IsNullOrEmpty(markdown))
            {
                customFields.Add(
                    new CustomField()
                    {
                        ID = "mt_markdown",
                        Key = "mt_markdown",
                        Value = markdown
                    });
            }

            if (!string.IsNullOrEmpty(FeaturedImageUrl) || !string.IsNullOrEmpty(FeatureImageId))
            {
                var featuredImage = FeaturedImageUrl;
                if (!string.IsNullOrEmpty(FeatureImageId)) // id takes precedence
                    featuredImage = FeatureImageId;

                post.wp_post_thumbnail = featuredImage;
                customFields.Add(
                    new CustomField()
                    {
                        ID = "wp_post_thumbnail",
                        Key = "wp_post_thumbnail",
                        Value = featuredImage
                    });
            }

            post.CustomFields = customFields.ToArray();

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
                mmApp.Log($"Error sending post to Weblog at {WeblogInfo.ApiUrl}: ", ex);
                ErrorMessage = $"Error sending post to Weblog: " + ex.Message;
                return false;
            }

            return true;
        }

        

        /// <summary>
        /// Retrieves a post and gets the link for the post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public string GetPostUrl(object postId)
        {
            string link = null;
            

            try
            {
                var wrapper = GetWrapper();
                var postRaw = wrapper.GetPostRaw(postId);
                link = postRaw.link;
            }
            catch { }                
            

            if (string.IsNullOrEmpty(link) || (!link.StartsWith("http://") && !link.StartsWith("https://")))
                // just go to the base domain - assume posts are listed there
                link = new Uri(WeblogInfo.ApiUrl).GetLeftPart(UriPartial.Authority);
            
            return link;
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


        public IEnumerable<UserBlog> GetBlogs()
        {
            var wrapper = GetWrapper();
            return wrapper.GetUsersBlogs().ToList();
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

        /// <summary>
        /// Gets the appropriate wrapper object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        MetaWeblogWrapper GetWrapper()            
        {
            var type = WeblogInfo.Type;

            MetaWeblogWrapper wrapper;
            if (type == WeblogTypes.MetaWeblogApi)
                wrapper = new MetaWeblogWrapper(WeblogInfo.ApiUrl,
                    WeblogInfo.Username,
                    mmApp.DecryptString(WeblogInfo.Password),
                    WeblogInfo.BlogId);
            else
                wrapper = new WordPressWrapper(WeblogInfo.ApiUrl,
                    WeblogInfo.Username,
                    mmApp.DecryptString(WeblogInfo.Password));

            return wrapper;
        }
    }
}
