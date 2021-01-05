using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.Annotations;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Windows.FileSearch
{
    public class FileSearchModel : INotifyPropertyChanged
    {

        public AppModel AppModel { get; set; }
        public MainWindow Window { get; set; }

        public string SearchPhrase
        {
            get { return _SearchPhrase; }
            set
            {
                if (value == _SearchPhrase) return;
                _SearchPhrase = value;
                OnPropertyChanged(nameof(SearchPhrase));
                OnPropertyChanged(nameof(IsSearchValid));
            }
        }
        private string _SearchPhrase;



        public string ReplaceText
        {
            get { return _ReplaceText; }
            set
            {
                if (value == _ReplaceText) return;
                _ReplaceText = value;
                OnPropertyChanged(nameof(ReplaceText));
            }
        }
        private string _ReplaceText;
        
        public string SearchFolder
        {
            get { return _SearchFolder; }
            set
            {
                if (value == _SearchFolder) return;
                _SearchFolder = value;
                OnPropertyChanged(nameof(SearchFolder));
                OnPropertyChanged(nameof(IsSearchValid));
            }
        }
        private string _SearchFolder;



        public bool SearchSubFolders
        {
            get { return _SearchSubFolders; }
            set
            {
                if (value == _SearchSubFolders) return;
                _SearchSubFolders = value;
                OnPropertyChanged(nameof(SearchSubFolders));
            }
        }
        private bool _SearchSubFolders = true;


        public bool SearchContent
        {
            get { return _SearchContent; }
            set
            {
                if (value == _SearchContent) return;
                _SearchContent = value;
                OnPropertyChanged(nameof(SearchContent));
            }
        }

        private bool _SearchContent = true;



        public bool SearchChildFolders
        {
            get { return _SearchChildFolders; }
            set
            {
                if (value == _SearchChildFolders) return;
                _SearchChildFolders = value;
                OnPropertyChanged(nameof(SearchChildFolders));
            }
        }
        private bool _SearchChildFolders;


        /// <summary>
        /// Comma delimited list of search items
        /// </summary>
        public string FileFilters
        {
            get { return _FileFilters; }
            set
            {
                if (value == _FileFilters) return;
                _FileFilters = value;
                OnPropertyChanged(nameof(FileFilters));
            }
        }
        private string _FileFilters = "*.md";




        public bool IsSearchValid
        {
            get
            {
                if (!string.IsNullOrEmpty(SearchPhrase) &&
                    Directory.Exists(SearchFolder) )
                    return true;

                return false;
            }
        }
        

        public ObservableCollection<SearchFileResult> SearchResults
        {
            get { return _SearchResults; }
            set
            {
                if (value == _SearchResults) return;
                _SearchResults = value;
                OnPropertyChanged(nameof(SearchResults));
            }
        }
        private ObservableCollection<SearchFileResult> _SearchResults;

        

        public FileSearchModel()
        {
            AppModel = mmApp.Model;
            Window = AppModel.Window;

            if (AppModel.ActiveProject != null && !AppModel.ActiveProject.IsEmpty)
            {
                SearchFolder = Path.GetDirectoryName(AppModel.ActiveProject.Filename);
            }
            else
            {
                SearchFolder = AppModel.ActiveDocument?.Filename;
                if (!string.IsNullOrEmpty(SearchFolder))
                    SearchFolder = Path.GetDirectoryName(SearchFolder);
            }

            FindFinder = new FindInFilesFinder(SearchFolder, FileFilters);
        }


        public async Task SearchAsync()
        {
            Window.ShowStatusProgress($"Searching files for '{SearchPhrase}'");

            var finder = new FindInFilesFinder(SearchFolder,FileFilters);
            finder.SearchSubFolders = SearchSubFolders;
            finder.SearchContent = SearchContent;

            var result = await finder.SearchFilesAsync(SearchPhrase);

            if (result != null)
                SearchResults = new ObservableCollection<SearchFileResult>(result.OrderBy( r=> r.FilePath + "!" + r.FileOnly) );
            else
                SearchResults = new ObservableCollection<SearchFileResult>();

            if(string.IsNullOrEmpty(SearchPhrase))
                Window.ShowStatus();  // clear
            else if (SearchResults.Count < 1)
                Window.ShowStatusError($"{SearchPhrase}: No matching files found.");
            else
                Window.ShowStatusSuccess($"{SearchPhrase}: {result.Count} file{(result.Count > 1 ? "s" : "")} found.");
        }


        public FindInFilesFinder FindFinder { get; set; } 


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
