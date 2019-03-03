using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using LibGit2Sharp;
using MarkdownMonster.Annotations;
using MarkdownMonster.Utilities;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class GitCommitModel : INotifyPropertyChanged
    {
               
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


        public ObservableCollection<RepositoryStatusItem>  StatusItems { get; set; }

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



        public string GitUsername
        {
            get
            {
                return mmApp.Model.Configuration.Git.GitName;
            }
            set
            {
                mmApp.Model.Configuration.Git.GitName = value;
            }
        }        

        public string GitEmail
        {
            get
            {
                return mmApp.Model.Configuration.Git.GitEmail;
            }
            set
            {
                mmApp.Model.Configuration.Git.GitEmail = value;            }            
        }
        

        public bool ShowUserInfo
        {
            get { return _ShowUserInfo; }
            set
            {
                if (value == _ShowUserInfo) return;
                _ShowUserInfo = value;
                OnPropertyChanged(nameof(ShowUserInfo));
            }
        }
        private bool _ShowUserInfo;



        public string Branch
        {
            get { return _Branch; }
            set
            {
                if (value == _Branch) return;
                _Branch = value;
                OnPropertyChanged(nameof(Branch));
            }
        }
        private string _Branch;


        public List<Branch> LocalBranches
        {
            get
            {
                if (GitHelper.Repository?.Branches == null)
                    return null;

                return GitHelper.Repository.Branches
                    .Where(b => b.IsRemote == false)                 
                    .ToList();
            }
        }


        public string Remote
        {
            get { return _Remote; }
            set
            {
                if (value == _Remote) return;
                _Remote = value;
                OnPropertyChanged(nameof(Remote));
            }
        }
        private string _Remote;


        public ObservableCollection<RepositoryStatusItem> RepositoryStatusItems
        {
            get => _repositoryStatusItems;
            set
            {
                if (Equals(value, _repositoryStatusItems)) return;
                _repositoryStatusItems = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<RepositoryStatusItem> _repositoryStatusItems;
        
        public AppModel AppModel { get; set; }

        public MainWindow Window { get; set; }

        public GitCommitDialog CommitWindow { get; set; }

        public Repository Repository { get; set; }

        public GitHelper GitHelper { get; }

        
        public bool LeaveDialogOpen
        {
            get => !AppModel.Configuration.Git.CloseAfterCommit;
            set
            {
                if (!value == AppModel.Configuration.Git.CloseAfterCommit) return;
                AppModel.Configuration.Git.CloseAfterCommit = !value;
                OnPropertyChanged();
            }
        }

        


        #region Load

        public GitCommitModel(string fileOrFolder, bool commitRepository = false)
        {
            CommitRepository = commitRepository;
            Filename = fileOrFolder;
            AppModel = mmApp.Model;
            Window = AppModel.Window;

            GitHelper = new GitHelper();
            GitHelper.OpenRepository(Filename);            
            
            if (string.IsNullOrEmpty(GitEmail) && string.IsNullOrEmpty(GitUsername))
            {
                var userEmail = GitHelper.GetGitNameAndEmailFromGitConfig();
                GitUsername = userEmail[0];
                GitEmail = userEmail[1];
            }            

            ShowUserInfo = string.IsNullOrEmpty(GitUsername);

            
        }
        #endregion

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


        public async Task<bool> CommitChangesToRepository(bool pushToRemote=false)
        {
            WindowUtilities.FixFocus(CommitWindow, CommitWindow.ListChangedItems);

            CommitWindow.StatusBar.ShowStatusProgress("Committing files...");

            var files = new ObservableCollection<RepositoryStatusItem>(
                RepositoryStatusItems.Where(it => it.Selected));

            if (files.Count < 1)
            {
                Window.ShowStatusError("There are no changes in this repository.");
                return false;
            }
        
            if (!GitHelper.Commit(files, CommitMessage, GitUsername, GitEmail) )
            {                
                CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                return false;
            }

            if (!pushToRemote)
                return true;

            CommitWindow.StatusBar.ShowStatusProgress("Pushing to remote...");

            using (var repo = GitHelper.OpenRepository(files[0].FullPath))
            {
                if (repo == null)
                {
                    CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                    return false;
                }

                var branch = repo.Head?
                                      .TrackedBranch?
                                      .FriendlyName;
                branch = branch?.Substring(branch.IndexOf("/") + 1);

                if (!await GitHelper.PushAsync(repo.Info.WorkingDirectory,branch) )
                {
                    CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                    return false;
                }
            }

            return true;
        }

        public bool PushChanges()
        {            
            CommitWindow.StatusBar.ShowStatusProgress("Pushing files to the Git Remote...");

            var repo = GitHelper.OpenRepository(Filename);
            if (repo == null)
            {
                CommitWindow.StatusBar.ShowStatusError("Couldn't determine branch to commit to.");
                return false;
            }

            if (!GitHelper.Push(repo.Info.WorkingDirectory, Branch))
            {
                CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                return false;
            }

            return true;
        }

        public async Task<bool> PushChangesAsync()
        {
            CommitWindow.StatusBar.ShowStatusProgress("Pushing files to the Git Remote...");

            var repo = GitHelper.OpenRepository(Filename);
            if (repo == null)
            {
                CommitWindow.StatusBar.ShowStatusError("Couldn't determine branch to commit to.");
                return false;
            }

            bool pushResult = await GitHelper.PushAsync(repo.Info.WorkingDirectory, Branch);
            if (!pushResult)
            {
                CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                return false;
            }

            return true;
        }

        public bool PullChanges()
        {
            var repo = GitHelper.OpenRepository(Filename);
            if (repo == null)
            {
                Window.ShowStatus("Invalid repository path.",mmApp.Configuration.StatusMessageTimeout);
                return false;
            }

            if (!GitHelper.Pull(repo.Info.WorkingDirectory))
            {
                CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                return false;
            }

            return true;
        }

        public async Task<bool> PullChangesAsync()
        {
            var repo = GitHelper.OpenRepository(Filename);
            if (repo == null)
            {
                Window.ShowStatus("Invalid repository path.", mmApp.Configuration.StatusMessageTimeout);
                return false;
            }

            if (!await GitHelper.PullAsync(repo.Info.WorkingDirectory))
            {
                CommitWindow.StatusBar.ShowStatusError(GitHelper.ErrorMessage);
                return false;
            }

            return true;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
