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
using WeblogAddin;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities.InternetTools;
using File = System.IO.File;

namespace WebLogAddin.Medium
{


    /// <summary>
    /// Very basic implementation of the Medium Blog API 
    /// for posting new stories. 
    /// 
    /// For these tests to work make sure to first create 
    /// a Medium account named "Medium Markdown Monster Test"
    /// in Markdown Monster and add it    
    /// </summary>
    /// <remarks>
    /// Medium doesn't support updating of posts so posting
    /// is a one way trip. Posts are sent as HTML and can
    /// be edited on Medium after initial publish
    /// </remarks>
    public class MediumApiClient
    {
        public static string MediumApiUrl = "https://api.medium.com/v1/";

        

        public string ErrorMessage { get; set; }

        
        /// <summary>
        /// Access to the full user after initial call to GetUser()
        /// </summary>
        public MediumUser User { get; set; }

        /// <summary>
        /// Access to the UserId
        /// </summary>
        public string UserId => User.id;

        public string PostUrl { get; set; }

        
        /// <summary>
        /// The Api token that is sent to server 
        /// in Authorization header as bearer token
        /// </summary>
        private string MediumApiUserToken {get; set; }

        
        /// <summary>
        /// Internally used current url
        /// </summary>
        private string CurrentUrl;

        private readonly WeblogInfo WeblogInfo;


        public MediumApiClient(WeblogInfo weblogInfo)
        {
            WeblogInfo = weblogInfo;
            WeblogInfo.ApiUrl = MediumApiUrl;

            MediumApiUserToken = WeblogInfo.DecryptPassword(WeblogInfo.AccessToken);

            GetUser();
        }


        /// <summary>
        /// Gets user based on token and fills 
        /// </summary>
        /// <returns></returns>
        public bool GetUser()
        {
            if (string.IsNullOrEmpty(MediumApiUserToken))
            {
                ErrorMessage = "Missing Medium API Access Token. Make sure you set up the token before other operations.";
                return false;
            }

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
        public MediumPost PublishCompletePost(Post post,
            string documentBasePath = null, 
            bool sendasDraft = false)
        {
            var mediumPost = new MediumPost
            {
                title = post.Title,
                content = post.Body,
                 contentFormat = "html",
                 publishStatus = sendasDraft ? "draft" : "public",
                 tags = post.Tags
            };

            if (!GetUser())
                return null;

            if (!PublishPostImages(mediumPost, documentBasePath))
                return null;

            return PublishPost(mediumPost, WeblogInfo.BlogId.ToString());                        
            
        }


        /// <summary>
        /// Scans the HTML document and uploads any local files to the Medium
        /// API and returns URL that are embedded as http links into the document
        /// </summary>
        /// <param name="post"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
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
            var json = JsonConvert.SerializeObject(post, settings);


            string url = MediumApiUrl + "users/" + UserId + "/posts";
            if (!string.IsNullOrEmpty(publicationId))
                url = MediumApiUrl + "publications/" + publicationId + "/posts";

            var http = CreateHttpClient(url, json);

            var httpResult = http.DownloadString(url);
            if (http.Error)
            {
                ErrorMessage = http.ErrorMessage;
                return null;
            }
            if (httpResult.Contains("\"errors\":"))
            {
                //{ "errors":[{"message":"Publication does not exist or user not allowed to publish in it","code":2006}]}
                var errorResult = JsonConvert.DeserializeObject<ErrorResult>(httpResult);

                StringBuilder sb = new StringBuilder();
                foreach (var err in errorResult.errors)
                    sb.AppendLine(err.message);

                ErrorMessage = sb.ToString();
                return null;
            }


            var postResult = JsonConvert.DeserializeObject<MediumPostResult>(httpResult);
            if (postResult.data == null)
            {
                ErrorMessage = "Unable to parse publishing API response.";
                return null;
            }

            this.PostUrl = postResult.data.url;
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

       
        public IEnumerable<UserBlog>  GetBlogs()
        {
            if (!GetUser())
                return null;

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

            var blogs = new List<UserBlog>();
            foreach (var blog in result.data)
            {
                blogs.Add(new UserBlog()
                {
                     BlogId = blog.id,
                     BlogName = blog.name,                     
                });
            }

            return blogs;
        }

        public string GetPostUrl(object postId)
        {
            return PostUrl;            
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
