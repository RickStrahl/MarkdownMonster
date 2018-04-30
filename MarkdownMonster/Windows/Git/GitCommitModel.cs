using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LibGit2Sharp;
using MarkdownMonster.Annotations;
using MarkdownMonster.Utilities;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class GitCommitModel : INotifyPropertyChanged
    {
        
        

        public GitCommitModel(string fileOrFolder, bool commitRepository = false)
        {
            CommitRepository = commitRepository;
            Filename = fileOrFolder;
            AppModel = mmApp.Model;
            Window = AppModel.Window;

            GitHelper = new GitHelper();
            GitHelper.OpenRepository(Filename);
        }
        

        public string Filename
        {
            get { return _Filename; }
            set
            {
                if (value == _Filename) return;
                _Filename = value;
                OnPropertyChanged(nameof(Filename));
                
            }
        }
        private string _Filename;


        public List<RepositoryStatusItem>  StatusItems { get; set; }

        public bool CommitRepository
        {
            get { return _CommitRepository; }
            set
            {
                if (value == _CommitRepository) return;
                _CommitRepository = value;
                OnPropertyChanged(nameof(CommitRepository));

                GetRepositoryChanges();
            }
        }
        private bool _CommitRepository;

        
        public bool IncludeIgnoredFiles
        {
            get => _includeIgnoredFiles;
            set
            {
                if (value == _includeIgnoredFiles) return;
                _includeIgnoredFiles = value;
                OnPropertyChanged();

                GetRepositoryChanges();
            }
        }
        private bool _includeIgnoredFiles;


      
        public string CommitMessage
        {
            get { return _CommitMessage; }
            set
            {
                if (value == _CommitMessage) return;
                _CommitMessage = value;
                OnPropertyChanged(nameof(CommitMessage));
            }
        }
        private string _CommitMessage;


        public bool CommitAndPush
        {
            get { return _CommitAndPush; }
            set
            {
                if (value == _CommitAndPush) return;
                _CommitAndPush = value;
                OnPropertyChanged(nameof(CommitAndPush));
            }
        }
        private bool _CommitAndPush;

       

        public List<RepositoryStatusItem> RepositoryStatusItems
        {
            get => _repositoryStatusItems;
            set
            {
                if (Equals(value, _repositoryStatusItems)) return;
                _repositoryStatusItems = value;
                OnPropertyChanged();
            }
        }
        private List<RepositoryStatusItem> _repositoryStatusItems;

        public AppModel AppModel { get; set; }

        public MainWindow Window { get; set; }

        public Repository Repository { get; set; }

        public GitHelper GitHelper { get; }


        #region Helpers


        public void GetRepositoryChanges()
        {
            var statuses = GitHelper.DefaultStatusesToDisplay;

            if (IncludeIgnoredFiles)
                statuses |= FileStatus.Ignored;
            
            if (CommitRepository)
                RepositoryStatusItems = GitHelper.GetRepositoryChanges(Filename, selectAll: true, includedStatuses: statuses);
            else
                RepositoryStatusItems = GitHelper.GetRepositoryChanges(Filename, Filename,includedStatuses: statuses);
        }


        public bool CommitAndPushRepository()
        {
            Window.ShowStatus("Committing files into local Git repository...");

            var files = RepositoryStatusItems.Where(it => it.Selected).Select(it => it.Filename).ToList();
            if (!GitHelper.Commit(files, CommitMessage) )
            {
                Window.ShowStatus(GitHelper.ErrorMessage, 6000, FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
                return false;
            }

            return true;
        }
        
        #endregion



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public enum GitCommitFormModes
    {
        ActiveDocument,
        Folder
    }

    
}
