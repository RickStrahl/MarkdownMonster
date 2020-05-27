using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using MarkdownMonster.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for PasteHref.xaml
    /// </summary>
    public partial class GitRepositoryWindow : MetroWindow, INotifyPropertyChanged
    {
        private readonly GitRepositoryWindowMode _startupMode;

        public string GitUrl
        {
            get { return _gitUrl; }
            set
            {
                if (value == _gitUrl) return;
                _gitUrl = value;
                OnPropertyChanged(nameof(GitUrl));
            }
        }

        private string _gitUrl;


        public string RemoteName
        {
            get { return _RemoteName; }
            set
            {
                if (value == _RemoteName) return;
                _RemoteName = value;
                OnPropertyChanged(nameof(RemoteName));
            }
        }
        private string _RemoteName = "origin";


        public string LocalPath
        {
            get => _localPath;
            set
            {
                if (value == _localPath) return;
                _localPath = value;
                OnPropertyChanged(nameof(LocalPath));
            }
        }
        private string _localPath;



        public bool UseGitCredentialManager
        {
            get { return _useGitCredentialManager; }
            set
            {
                if (value == _useGitCredentialManager) return;
                _useGitCredentialManager = value;
                OnPropertyChanged(nameof(UseGitCredentialManager));
            }
        }

        private bool _useGitCredentialManager;



        public string Username
        {
            get { return _Username; }
            set
            {
                if (value == _Username) return;
                _Username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _Username;


        public string Password
        {
            get { return _Password; }
            set
            {
                if (value == _Password) return;
                _Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _Password;



        public string Output
        {
            get { return _Output; }
            set
            {
                if (value == _Output) return;
                _Output = value;
                OnPropertyChanged(nameof(Output));
            }
        }

        private string _Output;

        public AppModel AppModel { get; set; }

        StatusBarHelper StatusBar { get; set; }

        public GitRepositoryWindow(GitRepositoryWindowMode startupMode = GitRepositoryWindowMode.Clone)
        {
            _startupMode = startupMode;

            InitializeComponent();
            AppModel = mmApp.Model;

            DataContext = this;

            mmApp.SetThemeWindowOverride(this);
            if (mmApp.Configuration.ApplicationTheme == Themes.Light)
                TabControl.Background = (SolidColorBrush)Resources["LightThemeTitleBackground"];


            Loaded += OpenFromUrl_Loaded;
            Activated += OpenFromUrl_Activated;

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);
        }

        

        private void OpenFromUrl_Activated(object sender, EventArgs e)
        {
            string clip = ClipboardHelper.GetText();
            if (string.IsNullOrEmpty(GitUrl) &&
                clip.StartsWith("http://") || clip.StartsWith("https://"))
                GitUrl = clip;
        }

        private void OpenFromUrl_Loaded(object sender, RoutedEventArgs e)
        {
            string clip = ClipboardHelper.GetText();

            if (string.IsNullOrEmpty(GitUrl) &&
                clip.StartsWith("http://") || clip.StartsWith("https://"))
            {
                GitUrl = clip;
                TextPath.Focus();
            }
            else
            {
                TextUrl.Focus();
            }

            if (_startupMode == GitRepositoryWindowMode.Create)
                TabControl.SelectedItem = TabCreate;
            else if (_startupMode == GitRepositoryWindowMode.AddRemote)
                TabControl.SelectedItem = TabAddRemote;
        }

        private void BrowseForFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = mmWindowsUtils.ShowFolderDialog(mmApp.Configuration.LastFolder,"Select Git Folder");
            if (folder == null)
                return;
            LocalPath = folder;
        }




        private async void ButtonClone_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilities.FixFocus(this, TextUrl);
            Output = null;

            if (string.IsNullOrEmpty(GitUrl))
            {
                
                StatusBar.ShowStatusError("Please provide a URL for the Git Repository to clone.");
                return;
            }

            if (string.IsNullOrEmpty(LocalPath))
            {                
                StatusBar.ShowStatusError("Please provide a local path to clone the Repository to.");
                return;
            }

            if (await CloneRepository())
            {
               StatusBar.ShowStatusSuccess("Repository has been cloned.");
                DialogResult = true;
            }
            else
            {
                return; // failed - display status message
            }

            Close();
        }


        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilities.FixFocus(this, TextUrl);

            Output = null;

            if (string.IsNullOrEmpty(LocalPath))
            {

                StatusBar.ShowStatusError("Please provide a local path to clone the Git Repository to.");
                return;
            }

            if (Directory.Exists(Path.Combine(LocalPath, ".git")))
            {
                StatusBar.ShowStatusError("This folder already contains a Git repository.");
                return;
            }

            using (var git = new GitHelper())
            {
                if (!git.CreateRepository(LocalPath))
                {
                    StatusBar.ShowStatusError($"Couldn't create Git Repository: {git.ErrorMessage}");
                    return;
                }
            }

            AppModel.Window.ShowFolderBrowser(folder: LocalPath);

            var readmeFile = Path.Combine(LocalPath, "README.md");
            if(!File.Exists(readmeFile))
                File.WriteAllText(readmeFile, "# My New Repository");

            AppModel.Window.OpenTab(readmeFile);

            AppModel.Window.ShowStatus($"Repository created at {LocalPath}");
            DialogResult = true;

            Close();
        }

        private void ButtonAddRemote_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilities.FixFocus(this, TextUrl);

            Output = null;
            
            if (string.IsNullOrEmpty(LocalPath) || !Directory.Exists(Path.Combine(LocalPath, ".git")))
            {
                StatusBar.ShowStatusError("The local folder is not a Git repository. Please create a Repository first.");
                return;
            }

            using (var git = new GitHelper())
            {
                if (git.OpenRepository(LocalPath) == null)
                {
                    StatusBar.ShowStatusError($"Couldn't open local repository: {git.ErrorMessage}");
                    return;
                }

                if (!git.AddRemote(GitUrl,RemoteName))
                {
                    StatusBar.ShowStatusError($"Couldn't create Git Repository: {git.ErrorMessage}");
                    return;
                }

            }

            StatusBar.ShowStatusSuccess($"Remote has been added to Git Repository at: {LocalPath}");
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async Task<bool> CloneRepository()
        {
            using (var git = new GitHelper())
            {

                StatusBar.ShowStatusProgress("Cloning Repository...");


                GitCommandResult result = await Task.Run<GitCommandResult>(() =>
                {
                    var action = new Action<object, DataReceivedEventArgs>((s, e) => { Output += e.Data; });
                    var res = git.CloneRepositoryCommandLine(GitUrl, LocalPath, action, 1);
                    return res;
                });

                if (result.HasError)
                {

                    StatusBar.ShowStatusError("Cloning failed.");
                    return false;
                }

                var file = Path.Combine(LocalPath, "README.md");
                mmApp.Model.Window.ShowFolderBrowser(folder: LocalPath);
                if (File.Exists(file))
                    mmApp.Model.Window.OpenTab(file);

#pragma warning disable 4014
                Dispatcher.DelayAsync(1000, (p) =>
#pragma warning restore 4014
                {
                    mmApp.Model.Window.ShowStatus($"Successfully cloned Git Repository to {LocalPath}");
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);


                return true;
            }

        }


#region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

#endregion
    }

    public enum GitRepositoryWindowMode
    {
        Clone,
        Create,
        AddRemote
    }
}
