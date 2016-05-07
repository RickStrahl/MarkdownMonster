using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using WebLogAddin.Annotations;
using Westwind.Utilities.Configuration;

namespace WebLogAddin
{
    internal class WeblogApp
    {
        public static WeblogAddinConfiguration Configuration;


        static WeblogApp()
        {
            Configuration = new WeblogAddinConfiguration();
            Configuration.Initialize();
        }
    }

    public class WeblogAddinConfiguration : AppConfiguration, INotifyPropertyChanged
    {

        public Dictionary<string,WeblogInfo> WebLogs { get; set; }


        public string LastWeblogAccessed
        {
            get { return _lastWeblogAccessed; }
            set
            {
                if (value == _lastWeblogAccessed) return;
                _lastWeblogAccessed = value;
                OnPropertyChanged(nameof(LastWeblogAccessed));
            }
        }


        public string PostsFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_postsFolder))
                {
                    var basePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "DropBox");
                    if (!Directory.Exists(basePath))
                        basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                    basePath = basePath + "\\Markdown Monster Weblog Posts";
                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);
                    _postsFolder = basePath;
                }
                return _postsFolder;
            }
            set
            {
                _postsFolder = value;
            }
        }
        private string _postsFolder;
        private string _lastWeblogAccessed;

        public WeblogAddinConfiguration()
        {
            WebLogs = new Dictionary<string, WeblogInfo>();
            WebLogs.Add("New Blog",
                new WeblogInfo()
                {
                    ApiUrl = "http://mysite.com/metaweblogapi/",
                    Username = "username",
                    Password = "Password",
                    Name = "Test WebLog"
                });
        }
        
        protected override IConfigurationProvider OnCreateDefaultProvider(string sectionName, object configData)
        {
            var provider = new JsonFileConfigurationProvider<WeblogAddinConfiguration>()
            {
                JsonConfigurationFile = Path.Combine(mmApp.Configuration.CommonFolder, "WebLogAddIn.json")                                
            };

            if (!File.Exists(provider.JsonConfigurationFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(provider.JsonConfigurationFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(provider.JsonConfigurationFile));
            }

            return provider;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WeblogInfo
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiUrl { get; set; }

        /// <summary>
        /// Url used to preview the post. The postId can be embedded into 
        /// the value by using {0}.
        /// </summary>
        public string PreviewUrl { get; set; }
    }
}