using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using WeblogAddin.Annotations;
using Westwind.Utilities.Configuration;

namespace WeblogAddin
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


        public WeblogAddinConfiguration()
        {
            Weblogs = new Dictionary<string, WeblogInfo>();
        }

        public Dictionary<string,WeblogInfo> Weblogs { get; set; }


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
        private string _lastWeblogAccessed;


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


        /// <summary>
        /// When true renders links to open externally.
        /// </summary>
        public bool RenderLinksOpenExternal
        {
            get { return _RenderLinksOpenExternal; }
            set
            {
                if (value == _RenderLinksOpenExternal) return;
                _RenderLinksOpenExternal = value;
                OnPropertyChanged(nameof(RenderLinksOpenExternal));
            }
        }
        private bool _RenderLinksOpenExternal = true;

        
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
}