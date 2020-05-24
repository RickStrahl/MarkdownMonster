using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using MarkdownMonster;
using MarkdownMonster.Annotations;
using WebLogAddin.Medium;
using Westwind.Utilities;

namespace WeblogAddin
{
    public class WeblogInfo : INotifyPropertyChanged
    {
       
        public WeblogInfo()
        {
            Id = DataUtils.GenerateUniqueId(8);
            BlogId = "1";
        }


        /// <summary>
        /// A unique generated Id for this instance
        /// </summary>
        public string Id { get; set; }

        
        /// <summary>
        /// Name of the Weblog as displayed in the selection list
        /// </summary>

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _name = "New Weblog";

        

        /// <summary>
        /// Username if username and password authentication is ued.
        /// </summary>
        public string Username
        {
            get { return _username; }
            set
            {
                if (value == _username) return;
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        private string _username;


        /// <summary>
        /// Password if user name and password authentication is used
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                _password = mmApp.EncryptString(value);
                OnPropertyChanged(nameof(Password));
            }
        }


        /// <summary>
        /// Access token if token based authentication is used
        /// </summary>
        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                if (value == _accessToken) return;
                _accessToken = value;
                OnPropertyChanged(nameof(AccessToken));
            }
        }
        private string _accessToken;


        /// <summary>
        /// The API URL endpoint or local folder path for local copy distributions
        /// </summary>
        public string ApiUrl
        {
            get { return _apiUrl; }
            set
            {
                if (value == _apiUrl) return;
                _apiUrl = value;
                OnPropertyChanged(nameof(ApiUrl));
            }
        }
        private string _apiUrl;


        /// <summary>
        /// A root site path for the Web site that can be used to fix up
        /// URLs explicitly for providers that otherwise do not have access
        /// to live site info - mainly static providers.
        /// </summary>
        public string WebLogRootSitePath
        {
            get { return _WebLogRootSitePath; }
            set
            {
                if (value == _WebLogRootSitePath) return;
                _WebLogRootSitePath = value;
                OnPropertyChanged(nameof(WebLogRootSitePath));
            }
        }
        private string _WebLogRootSitePath;

        /// <summary>
        /// The ID of the blog or publication also knonw as Publication ID sometimes
        /// </summary>
        public object BlogId
        {
            get { return _blogId; }
            set
            {
                if (Equals(value, _blogId)) return;
                _blogId = value;
                OnPropertyChanged(nameof(BlogId));
            }
        }
        private object _blogId;

        
        /// <summary>
        /// The type of blog: MetaWeblog, Wordpress, Medium etc.
        /// </summary>
        public WeblogTypes Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                OnPropertyChanged(nameof(Type));

                if (Type == WeblogTypes.Medium)
                    ApiUrl = MediumApiClient.MediumApiUrl;
                else if (Type == WeblogTypes.LocalJekyll)
                {
                    if (string.IsNullOrEmpty(ApiUrl))
                        ApiUrl = "<Local Path To your Jekyll Project>";
                    if (string.IsNullOrEmpty(LaunchCommand))
                        LaunchCommand = @"c:\windows\SysNative\bash.exe -c ""cd <Path: /mnt/c/projects/jekyll/yourproject>; bundle exec jekyll server""";
                    if (string.IsNullOrEmpty(PreviewUrl))
                        PreviewUrl = "http://localhost:4000/{0}";
                }
            }
        }
        private WeblogTypes _type = WeblogTypes.MetaWeblogApi;


        /// <summary>
        /// Determines whether this blog type requires an API 
        /// token or username and password to log in.
        /// </summary>
        public AuthenticationType AuthenticationType
        {
            get { return _authenticationType; }
            set
            {
                if (value == _authenticationType) return;
                _authenticationType = value;
                OnPropertyChanged(nameof(AuthenticationType));
            }
        }
        private AuthenticationType _authenticationType = AuthenticationType.UsernamePassword;

        /// <summary>
        /// An executable command that can be launched after the publish is complete
        ///
        /// This is useful to launch a build process after generating to a local
        /// repository.
        /// </summary>
        public string LaunchCommand
        {
            get { return _LaunchCommand; }
            set
            {
                if (value == _LaunchCommand) return;
                _LaunchCommand = value;
                OnPropertyChanged(nameof(LaunchCommand));
            }
        }
        private string _LaunchCommand;


        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl
        {
            get { return _previewUrl; }
            set
            {
                if (value == _previewUrl) return;
                _previewUrl = value;
                OnPropertyChanged(nameof(PreviewUrl));
            }
        }
        private string _previewUrl;        
        private string postFix = "*~~*";
        private string _password;


        /// <summary>
        /// Custom fields to add for all posts on this blog
        /// </summary>
        public IDictionary<string, string> CustomFields { get; set; }

        /// <summary>
        /// Encrypts the password using two encryption
        /// </summary>
        /// <param name="password">password text/param>
        /// <returns></returns>
        public string EncryptPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            if (password.EndsWith(postFix))
                return password;

            return Encryption.EncryptString(password,mmApp.EncryptionMachineKey) + postFix;
        }

        //public string DecryptPassword(string encrypted)
        //{
        //    if (string.IsNullOrEmpty(encrypted))
        //        return string.Empty;

        //    if (!encrypted.EndsWith(postFix))
        //        return encrypted;

        //    encrypted = encrypted.Replace(postFix, "");

        //    return Encryption.DecryptString(encrypted, mmApp.EncryptionMachineKey);
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum WeblogTypes
    {
        MetaWeblogApi,
        Wordpress,
        Unknown,
        Medium,
        LocalJekyll
    }

    public enum AuthenticationType
    {
        UsernamePassword,
        AccessToken
    }
}
