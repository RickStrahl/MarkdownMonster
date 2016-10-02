using System.Text;
using Newtonsoft.Json;
using Westwind.Utilities;

namespace WeblogAddin
{
    public class WeblogInfo
    {
       
        public WeblogInfo()
        {
            Id = DataUtils.GenerateUniqueId(8);
            BlogId = "1";
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public string Username { get; set; }

        public string Password
        {
            get { return _password; }
            set { _password = EncryptPassword(value); }
        }

        public string ApiUrl { get; set; }

        public object BlogId { get; set; }

        public WeblogTypes Type { get; set; } = WeblogTypes.MetaWeblogApi;

        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl { get; set; }

        string pb = Encoding.Default.GetString( new byte[] {  44, 33, 29,233, 255, 78, 33, 89, 88, 235, 121, 187});
        private string postFix = "*~~*";
        private string _password;

        public string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            if (password.EndsWith(postFix))
                return password;

            return Encryption.EncryptString(password,pb) + postFix;
        }

        public string DecryptPassword(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;

            if (!encrypted.EndsWith(postFix))
                return encrypted;

            encrypted = encrypted.Replace(postFix, "");

            return Encryption.DecryptString(encrypted, pb);
        }
    }

    public enum WeblogTypes
    {
        MetaWeblogApi,
        Wordpress,
        Unknown
    }
}