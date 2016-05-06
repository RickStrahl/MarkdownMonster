using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
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

    public class WeblogAddinConfiguration : AppConfiguration
    {

        public Dictionary<string,WeblogInfo> WebLogs { get; set; }

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