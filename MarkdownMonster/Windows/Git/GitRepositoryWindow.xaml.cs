using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster.Utilities;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Westwind.Utilities;

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
        }

        private void OpenFromUrl_Activated(object sender, EventArgs e)
        {
            string clip = Clipboard.GetText(TextDataFormat.Text);
            if (string.IsNullOrEmpty(GitUrl) &&
                clip.StartsWith("http://") || clip.StartsWith("https://"))
                GitUrl = clip;
        }

        private void OpenFromUrl_Loaded(object sender, RoutedEventArgs e)
        {
            string clip = Clipboard.GetText(TextDataFormat.Text);
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
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select or create a folder to clone Repository to:";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = mmApp.Configuration.LastFolder;
            dlg.RestoreDirectory = true;
            dlg.ShowHiddenItems = true;
            dlg.ShowPlacesList = true;
            dlg.EnsurePathExists = true;

            var result = dlg.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return;

            LocalPath = dlg.FileName;
        }


        private async void ButtonClone_Click(object sender, RoutedEventArgs e)
        {
            WindowUtilities.FixFocus(this, TextUrl);
            Output = null;

            if (string.IsNullOrEmpty(GitUrl))
            {
                SetStatusIcon(FontAwesomeIcon.Warning, Colors.Firebrick);
                ShowStatus("Please provide a URL for the Git Repository to clone.");
                return;
            }

            if (string.IsNullOrEmpty(LocalPath))
            {
                SetStatusIcon(FontAwesomeIcon.Warning, Colors.Firebrick);
                ShowStatus("Please provide a local path to clone the Repository to.");
                return;
            }

            if (await CloneRepository())
            {
                ShowStatus("Cloning completed successfully.");
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
                SetStatusIcon(FontAwesomeIcon.Warning, Colors.Firebrick);
                ShowStatusError("Please provide a local path to clone the Git Repository to.");
                return;
            }

            if (Directory.Exists(Path.Combine(LocalPath, ".git")))
            {
                ShowStatusError("This folder already contains a Git repository.");
                return;
            }

            using (var git = new GitHelper())
            {
                if (!git.CreateRepository(LocalPath))
                {
                    ShowStatusError($"Couldn't create Git Repository: {git.ErrorMessage}");
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
                ShowStatusError("The local folder is not a Git repository. Please create a Repository first.");
                return;
            }

            using (var git = new GitHelper())
            {
                if (git.OpenRepository(LocalPath) == null)
                {
                    ShowStatusError($"Couldn't open local repository: {git.ErrorMessage}");
                    return;
                }

                if (!git.AddRemote(GitUrl,RemoteName))
                {
                    ShowStatusError($"Couldn't create Git Repository: {git.ErrorMessage}");
                    return;
                }

            }

            ShowStatus("Remote has been added to Git Repository at: " + LocalPath);
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
                SetStatusIcon(FontAwesomeIcon.CircleOutlineNotch, Colors.DarkGoldenrod, true);
                ShowStatus("Cloning Repository...");


                GitCommandResult result = await Task.Run<GitCommandResult>(() =>
                {
                    var action = new Action<object, DataReceivedEventArgs>((s, e) => { Output += e.Data; });
                    var res = git.CloneRepositoryCommandLine(GitUrl, LocalPath, action, 1);
                    return res;
                });

                if (result.HasError)
                {
                    SetStatusIcon(FontAwesomeIcon.Warning, Colors.Firebrick);
                    ShowStatus("Cloning failed.");

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

        #region StatusBar Display

        DebounceDispatcher debounce = new DebounceDispatcher();


        public void ShowStatus(string message = null, int milliSeconds = 0,
            FontAwesomeIcon icon = FontAwesomeIcon.None,
            Color color = default(Color),
            bool spin = false)
        {
            if (icon != FontAwesomeIcon.None)
                SetStatusIcon(icon, color, spin);

            if (message == null)
            {
                message = "Ready";
                SetStatusIcon();
            }

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                // debounce rather than delay so if something else displays
                // a message the delay timer is 'reset'
                debounce.Debounce(milliSeconds, (win) =>
                {
                    var window = win as GitRepositoryWindow;
                    window.ShowStatus(null, 0);
                }, this);
            }

            WindowUtilities.DoEvents();
        }


        /// <summary>
        /// Displays an error message using common defaults
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        public void ShowStatusError(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.Warning, Color color = default(Color))
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.Firebrick;

            ShowStatus(message, timeout, icon, color);
        }



        /// <summary>
        /// Status the statusbar icon on the left bottom to some indicator
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
        {
            StatusIcon.Icon = icon;
            StatusIcon.Foreground = new SolidColorBrush(color);
            if (spin)
                StatusIcon.SpinDuration = 1;

            StatusIcon.Spin = spin;
        }

        /// <summary>
        /// Resets the Status bar icon on the left to its default green circle
        /// </summary>
        public void SetStatusIcon()
        {
            StatusIcon.Icon = FontAwesomeIcon.Circle;
            StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
            StatusIcon.Spin = false;
            StatusIcon.SpinDuration = 0;
            StatusIcon.StopSpin();
        }

        #endregion

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
