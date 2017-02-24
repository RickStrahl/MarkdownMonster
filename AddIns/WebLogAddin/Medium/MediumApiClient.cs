using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Westwind.Utilities.InternetTools;

namespace WebLogAddin.Medium
{
    public class MediumApiClient
    {
        public static string MediumApiUrl = "https://api.medium.com/v1/";

        public string MediumApiUserToken = null;

        public string ErrorMessage { get; set; }

        
        private string CurrentUrl { get; set; }

        public MediumUser User { get; set; }

        public string UserId => User.id;

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
        /// Returns the Image Url from the Post operation
        /// </summary>
        /// <param name="post"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MediumPost PublishPost(MediumPost post)
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
