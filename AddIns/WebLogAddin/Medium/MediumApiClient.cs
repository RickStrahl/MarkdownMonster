using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using MarkdownMonster;
using Newtonsoft.Json;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities.InternetTools;
using File = System.IO.File;

namespace WebLogAddin.Medium
{


    /// <summary>
    /// Very basic implementation of the Medium Blog API 
    /// for posting new stories. 
    ///     
    /// </summary>
    /// <remarks>
    /// Medium doesn't support updating of posts so posting
    /// is a one way trip. Posts are sent as HTML and can
    /// be edited on Medium after initial publish
    /// </remarks>
    public class MediumApiClient
    {
        public static string MediumApiUrl = "https://api.medium.com/v1/";

        public string MediumApiUserToken = null;

        public string ErrorMessage { get; set; }

        
        /// <summary>
        /// Access to the full user after initial call to GetUser()
        /// </summary>
        public MediumUser User { get; set; }

        /// <summary>
        /// Access to the UserId
        /// </summary>
        public string UserId => User.id;
        

        /// <summary>
        /// Internally used current url
        /// </summary>
        private string CurrentUrl;


        public MediumApiClient(string userToken)
        {
            MediumApiUserToken = userToken;
        }


        /// <summary>
        /// Gets user based on token and fills 
        /// </summary>
        /// <returns></returns>
        public bool GetUser()
        {
            if (User != null)
                return true;

            var url = "https://api.medium.com/v1/me";

            var http = CreateHttpClient(url);

            var httpResult = http.DownloadString(url);
            if (http.Error)
            {
                ErrorMessage = http.ErrorMessage;
                return false;
            }

            var result = JsonConvert.DeserializeObject<MediumUserResult>(httpResult);
            if (result == null)
            {
                ErrorMessage = "Unable to convert user data.";
                return false;
            }

            User = result.data;
            if (User == null)
            {
                ErrorMessage = "Unable to convert user data.";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// High level method that posts a Post to Medium including uploading of
        /// images.
        /// </summary>
        /// <param name="post">An instance of the post to post to the server</param>
        /// <param name="publicationId">Publication/Blog id</param>
        /// <param name="documentBasePath">Base path for images to find relatively pathed images</param>
        /// <returns></returns>
        public MediumPost PublishCompletePost(MediumPost post, string publicationId = null, string documentBasePath = null)
        {
            if (!GetUser())
                return null;

            if (!PublishPostImages(post, documentBasePath))
                return null;

            return PublishPost(post, publicationId);                        

            //if (type == WeblogTypes.Unknown)
            //    type = weblogInfo.Type;

            //MetaWeblogWrapper wrapper;

            //if (type == WeblogTypes.MetaWeblogApi)
            //    wrapper = new MetaWeblogWrapper(weblogInfo.ApiUrl,
            //        weblogInfo.Username,
            //        weblogInfo.DecryptPassword(weblogInfo.Password),
            //        weblogInfo.BlogId);
            //else
            //    wrapper = new WordPressWrapper(weblogInfo.ApiUrl,
            //        weblogInfo.Username,
            //        weblogInfo.DecryptPassword(weblogInfo.Password));


            //string body;
            //try
            //{
            //    body = SendImages(html, doc.Filename, wrapper, meta);
            //}
            //catch (Exception ex)
            //{
            //    mmApp.Log($"Error sending images to Weblog at {weblogInfo.ApiUrl}: ", ex);
            //    return false;
            //}

            //if (body == null)
            //    return false;

            //ActivePost.Body = body;
            //ActivePost.PostID = meta.PostId;

            //var customFields = new List<CustomField>();


            //customFields.Add(
            //    new CustomField()
            //    {
            //        ID = "mt_markdown",
            //        Key = "mt_markdown",
            //        Value = meta.MarkdownBody
            //    });

            //if (!string.IsNullOrEmpty(meta.FeaturedImageUrl) || !string.IsNullOrEmpty(meta.FeatureImageId))
            //{
            //    var featuredImage = meta.FeaturedImageUrl;
            //    if (!string.IsNullOrEmpty(meta.FeatureImageId)) // id takes precedence
            //        featuredImage = meta.FeatureImageId;

            //    ActivePost.wp_post_thumbnail = featuredImage;
            //    customFields.Add(
            //        new CustomField()
            //        {
            //            ID = "wp_post_thumbnail",
            //            Key = "wp_post_thumbnail",
            //            Value = featuredImage
            //        });
            //}
            //ActivePost.CustomFields = customFields.ToArray();

            //bool isNewPost = IsNewPost(ActivePost.PostID);
            //try
            //{
            //    if (!isNewPost)
            //        wrapper.EditPost(ActivePost, !sendAsDraft);
            //    else
            //        ActivePost.PostID = wrapper.NewPost(ActivePost, !sendAsDraft);
            //}
            //catch (Exception ex)
            //{
            //    mmApp.Log($"Error sending post to Weblog at {weblogInfo.ApiUrl}: ", ex);
            //    MessageBox.Show($"Error sending post to Weblog: " + ex.Message,
            //        mmApp.ApplicationName,
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Exclamation);

            //    mmApp.Log(ex);
            //    return false;
            //}

            //meta.PostId = ActivePost.PostID.ToString();            
        }

        bool PublishPostImages(MediumPost post, string basePath)
        {
            // base folder name for uploads - just the folder name of the image
            var baseName = Path.GetFileName(basePath);
            baseName = mmFileUtils.SafeFilename(baseName).Replace(" ", "-");

            var doc = new HtmlDocument();
            doc.LoadHtml(post.content);
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
                            if(!imgFile.Contains(":\\"))
                                imgFile = Path.Combine(basePath, imgFile.Replace("/", "\\"));

                            if (File.Exists(imgFile))
                            {
                                var imgUrl  = PublishImage(imgFile);                                                                
                                img.Attributes["src"].Value = imgUrl;                                
                            }
                        }
                    }

                    post.content = doc.DocumentNode.OuterHtml;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error posting images to Medium: " + ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the Image Url from the Post operation
        /// </summary>
        /// <param name="post"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MediumPost PublishPost(MediumPost post, string publicationId = null)
        {
            
            if (string.IsNullOrEmpty(UserId))
            {
                ErrorMessage = "UserId has not been set yet. Make sure to call GetUser first.";
                return null;
            }
            
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(post,settings);


            string url = MediumApiUrl + "users/" + UserId + "/posts";
            if (!string.IsNullOrEmpty(publicationId))
                url = MediumApiUrl + "publications/" + publicationId + "/posts";

            var http = CreateHttpClient(url,json);            

            var httpResult = http.DownloadString(url);
            if (http.Error)
            {
                ErrorMessage = http.ErrorMessage;
                return null;
            }

            var postResult = JsonConvert.DeserializeObject<MediumPostResult>(httpResult);
            return postResult.data;
        }
        


        /// <summary>
        /// Posts an image to the 
        /// </summary>       
        public string PublishImage(string filename)
        {
            if (!File.Exists(filename))
            {
                ErrorMessage = "Invalid file name.";
                return null;
            }

            string url = MediumApiUrl + "images";

            var http = CreateHttpClient(url);
            http.PostMode = HttpPostMode.MultiPart;
            http.AddPostFile("image", filename, "image/png");
            
            var json = http.DownloadString(url);
            if (http.Error)
            {
                ErrorMessage = http.ErrorMessage;
                return null;
            }
            var result = JsonConvert.DeserializeObject<MediumImageResult>(json);
            return result.data.url;
        }

       
        public List<MediumPublication>  GetPublications()
        {
            string url = MediumApiUrl + "users/" + User.id + "/publications";

            var http = CreateHttpClient(url);

            var httpResult = http.DownloadString(url);
            if (http.Error)
            {
                ErrorMessage = http.ErrorMessage;
                return null;
            }

            var result = JsonConvert.DeserializeObject<MediumPublicationsResult>(httpResult);
            if (result == null)
            {
                ErrorMessage = "Unable to convert user data.";
                return null;
            }
            
            return result.data;
        }

        #region Helpers

        private HttpClient CreateHttpClient(string url, string json = null)
        {
            var http = new Westwind.Utilities.InternetTools.HttpClient();
            http.CreateWebRequestObject(url);
            CurrentUrl = url;
            http.PostMode = HttpPostMode.Json;
            http.WebRequest.Headers.Add("Accept-Charset", "utf-8");
            http.WebRequest.ContentType = "application/json";
            http.WebRequest.Headers.Add("Authorization", "Bearer " + MediumApiUserToken);


            if (!string.IsNullOrEmpty(json))            
                http.AddPostKey(json);
            
            return http;
        }

        #endregion


    }

}
