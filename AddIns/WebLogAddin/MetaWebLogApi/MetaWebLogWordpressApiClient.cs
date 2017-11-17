using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using MarkdownMonster;
using MarkdownMonster.Windows;
using WeblogAddin;
using Westwind.Utilities;

namespace WebLogAddin.MetaWebLogApi
{
    public class MetaWebLogWordpressApiClient
    {
        public  string ErrorMessage { get; set; }

        /// <summary>
        /// If true tries to use the first image as the featured image.
        /// If false, no featured image is implicitly assigned.        
        /// </summary>
        public bool DontInferFeaturedImage { get; set; } = true;
        
        /// <summary>
        /// Featured image Id captured in the request
        /// </summary>
        public string FeatureImageId { get; set; }

        /// <summary>
        /// Featured image url captured in the request
        /// </summary>
        public string FeaturedImageUrl { get; set; }

        
        /// <summary>
        /// The URL of the new post created
        /// </summary>
        public string PostUrl { get; set; }

        private readonly WeblogInfo WeblogInfo;

        public MetaWebLogWordpressApiClient(WeblogInfo weblogInfo)
        {
            WeblogInfo = weblogInfo;
        }


        /// <summary>
        /// Sends a complete post to a server. Parses the post and sends 
        /// embedded images as media attachments.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="basePath"></param>
        /// <param name="sendAsDraft"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
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

            if (!string.IsNullOrEmpty(FeaturedImageUrl) || !string.IsNullOrEmpty(FeatureImageId))
            {
                var featuredImage = FeaturedImageUrl;
                if (!string.IsNullOrEmpty(FeatureImageId)) // id takes precedence
                    featuredImage = FeatureImageId;

                post.wp_post_thumbnail = featuredImage;

                var thumbnailCustomField = post.CustomFields.FirstOrDefault(cf => cf.Key == "wp_post_thumbnail");
                if (thumbnailCustomField != null)
                {
                    thumbnailCustomField.Value = featuredImage;
                }
                else
                {
                    var cfl = post.CustomFields.ToList();
                    cfl.Add(
                        new CustomField()
                        {
                            Key = "wp_post_thumbnail",
                            Value = featuredImage
                        });
                    post.CustomFields = cfl.ToArray();
                }
            }
            else
            {
                post.wp_post_thumbnail = null;
            }



            bool isNewPost = IsNewPost(post.PostId);
            try
            {
                if (!isNewPost)
                    wrapper.EditPost(post, !sendAsDraft);
                else
                    post.PostId = wrapper.NewPost(post, !sendAsDraft);
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
        /// Retrieves a post and gets the link for the post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post GetPost(object postId)
        {
            try
            {
                var wrapper = GetWrapper();
                return wrapper.GetPost(postId);
            }
            catch(Exception ex)
            {
                ErrorMessage = "Unable to retrieve URL: " + ex.GetBaseException().Message;
                return null;
            }

        }

        /// <summary>
        /// Retrieves a se
        /// </summary>
        /// <param name="numberOfPosts"></param>
        /// <returns></returns>
        public IEnumerable<Post> GetRecentPosts(int numberOfPosts = 20)
        {
            try
            {
                var wrapper = GetWrapper();
                return wrapper.GetRecentPosts(numberOfPosts);
            }
            catch (Exception ex)
            {
                ErrorMessage = "Unable to download posts Posts: " + ex.GetBaseException().Message;
                return null;
            }

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
            baseName = FileUtils.SafeFilename(baseName).Replace(" ", "-");

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
                        string imgFile = img.Attributes["src"]?.Value;
                        imgFile = StringUtils.UrlDecode(imgFile);                        

                        if (imgFile == null)
                            continue;

                        if (!imgFile.StartsWith("http://") && !imgFile.StartsWith("https://"))
                        {
                            if (!imgFile.Contains(":\\"))
                                imgFile = Path.Combine(basePath, imgFile.Replace("/", "\\"));

                            if (System.IO.File.Exists(imgFile))
                            {
                                var uploadFilename = Path.GetFileName(imgFile);
                                var media = new MediaObject()
                                {
                                    Type = ImageUtils.GetImageMediaTypeFromFilename(imgFile),
                                    Bits = System.IO.File.ReadAllBytes(imgFile),
                                    Name = baseName + "/" + uploadFilename
                                };
                                var mediaResult = wrapper.NewMediaObject(media);
                                img.Attributes["src"].Value = mediaResult.URL;

                                // use first image as featured image
                                if (!DontInferFeaturedImage)
                                {
                                    if (string.IsNullOrEmpty(FeaturedImageUrl))
                                        FeaturedImageUrl = mediaResult.URL;
                                    if (string.IsNullOrEmpty(FeatureImageId))
                                        FeatureImageId = mediaResult.Id;
                                }
                            }
                           
                        }
						WindowUtilities.DoEvents();
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

            MetaWeblogWrapper wrapper;
            if (WeblogInfo.Type == WeblogTypes.MetaWeblogApi)
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
