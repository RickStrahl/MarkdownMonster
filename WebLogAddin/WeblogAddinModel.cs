using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;

namespace WeblogAddin
{
    public class WeblogAddinModel : INotifyPropertyChanged
    {
        private WebLogAddin _addin;
        private WeblogPostMetadata _activePostMetadata;
        private AppModel _appModel;
    
        private string _newTitle;

        public WeblogAddinModel()
        {
            ActiveWeblogInfo = new WeblogInfo()
            {
                Name = "New Weblog",
                Type = WeblogTypes.MetaWeblogApi
            };

        }

        public AppModel AppModel
        {
            get { return _appModel; }
            set
            {
                if (Equals(value, _appModel)) return;
                _appModel = value;
                OnPropertyChanged(nameof(AppModel));
            }
        }



        public WebLogAddin Addin
        {
            get { return _addin; }
            set
            {
                if (Equals(value, _addin)) return;
                _addin = value;
                OnPropertyChanged(nameof(Addin));
            }
        }

        public WeblogPostMetadata ActivePostMetadata
        {
            get { return _activePostMetadata; }
            set
            {
                if (Equals(value, _activePostMetadata)) return;
                _activePostMetadata = value;
                OnPropertyChanged(nameof(ActivePostMetadata));
            }
        }


        public WeblogInfo ActiveWeblogInfo
        {
            get { return _activeWeblogInfo; }
            set
            {
                if (value == _activeWeblogInfo) return;
                _activeWeblogInfo = value;
                OnPropertyChanged(nameof(ActiveWeblogInfo));
            }
        }
        private WeblogInfo _activeWeblogInfo;

        public WeblogAddinConfiguration Configuration
        {
            get { return _Configuration; }
            set
            {
                if (value == _Configuration) return;
                _Configuration = value;
                OnPropertyChanged(nameof(Configuration));
            }
        }
        private WeblogAddinConfiguration _Configuration = WeblogApp.Configuration;

        public string NewTitle
        {
            get { return _newTitle; }
            set
            {
                if (value == _newTitle) return;
                _newTitle = value;
                OnPropertyChanged(nameof(NewTitle));
            }
        }

        public string Name { get; set; } = "TESTTTSDTS";

        public WebLogForm Window { get; set; }


        public List<string> WeblogNames
        {
            get
            {
                return _WeblogNames;
            }
            set
            {
                if (value == _WeblogNames) return;
                _WeblogNames = value;
                OnPropertyChanged(nameof(WeblogNames));
            }
        }    
        private List<string> _WeblogNames = new List<string>();
        
        public void LoadWebLognames()
        {
            WeblogNames = Configuration.Weblogs.Select(wl => wl.Value.Name).ToList();            
        }
        public event PropertyChangedEventHandler PropertyChanged;


        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
