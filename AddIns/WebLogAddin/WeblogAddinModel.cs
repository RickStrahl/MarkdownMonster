using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster;
using WebLogAddin.MetaWebLogApi;
using Westwind.Utilities;

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
            NumberOfPostsToRetrieve = 20;
            
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
                OnPropertyChanged(nameof(IsUserPassVisible));
                OnPropertyChanged(nameof(IsTokenVisible));
                OnPropertyChanged(nameof(IsAbstractVisible));
                OnPropertyChanged(nameof(IsCategoriesVisible));
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
        private WeblogAddinConfiguration _Configuration = WeblogAddinConfiguration.Current;


        /// <summary>
        /// New title for a new blog post
        /// </summary>
        public string NewTitle
        {
            get { return _newTitle; }
            set
            {
                if (value == _newTitle) return;
                _newTitle = value;                
                OnPropertyChanged(nameof(NewTitle));
                NewFilename = mmFileUtils.SafeFilename(StringUtils.ToCamelCase(value)) + ".md";                
            }
        }


        /// <summary>
        /// Filename for new Blog Entry
        /// </summary>
        public string NewFilename
        {
            get { return _newFilename; }
            set
            {
                if (value == _newFilename) return;
                _newFilename = value;
                OnPropertyChanged(nameof(NewFilename));
            }
        }


        /// <summary>
        /// Number of posts to retrieve for the Get Web Log posts
        /// </summary>
        public int NumberOfPostsToRetrieve
        {
            get { return _numberOfPostsToRetrieve; }
            set
            {
                if (value == _numberOfPostsToRetrieve) return;
                _numberOfPostsToRetrieve = value;
                OnPropertyChanged(nameof(NumberOfPostsToRetrieve));
            }
        }

        public string PostListSearch
        {
            get { return _postListSearch; }
            set
            {
                _postListSearch = value;
                OnPropertyChanged(nameof(PostListSearch));
                OnPropertyChanged(nameof(PostList));
            }
        }


        public bool IsTokenVisible
        {
            get { return ActiveWeblogInfo.Type == WeblogTypes.Medium; }
        }
        public bool IsUserPassVisible
        {
            get { return ActiveWeblogInfo.Type != WeblogTypes.Medium; }
        }

        public bool IsAbstractVisible
        {
            get { return ActiveWeblogInfo.Type != WeblogTypes.Medium; }
        }

        public bool IsCategoriesVisible
        {
            get { return ActiveWeblogInfo.Type != WeblogTypes.Medium; }
        }


        public List<Post> PostList
        {
            get
            {
                if (string.IsNullOrEmpty(PostListSearch))
                    return _postList;

                string search = PostListSearch.Trim().ToLower();
                return _postList.Where(p => p.Title.ToLower().Contains(search) ||
                                            p.mt_excerpt.Contains(search) ).ToList();
            }
            set
            {
                if (value == _postList) return;
                _postList = value;
                OnPropertyChanged(nameof(PostList));
            }
        }
        private List<Post> _postList = new List<Post>();



        public WebLogForm Window { get; set; }


        public List<string> WeblogNames
        {
            get
            {
                return _WeblogNames;
            }
            set
            {
                //if (value == _WeblogNames) return;
                _WeblogNames = value;
                OnPropertyChanged(nameof(WeblogNames));
            }
        }    
        private List<string> _WeblogNames = new List<string>();
        private int _numberOfPostsToRetrieve;        
        private string _postListSearch;
        private string _newFilename;

        public void LoadWebLognames()
        {
            WeblogNames = Configuration.Weblogs.Select(wl => wl.Value.Name).ToList();            
        }
        public event PropertyChangedEventHandler PropertyChanged;


        
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
