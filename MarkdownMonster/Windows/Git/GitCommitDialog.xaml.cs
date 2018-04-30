using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FontAwesome.WPF;
using LibGit2Sharp;
using MahApps.Metro.Controls;
using MarkdownMonster.Utilities;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for GitCommitDialog.xaml
    /// </summary>
    public partial class GitCommitDialog : MetroWindow
    {
        public GitCommitModel CommitModel { get; set; }

        public AppModel AppModel { get; set; }
        

        public GitCommitDialog(string fileOrPath, bool commitRepo = false)
        {
            InitializeComponent();
            AppModel = mmApp.Model;

            CommitModel = new GitCommitModel(fileOrPath,commitRepo);
            if (!commitRepo)
                CommitModel.CommitMessage = $"Updating documentation in {System.IO.Path.GetFileName(fileOrPath)}";

            CommitModel.CommitWindow = this;

            mmApp.SetThemeWindowOverride(this);

            Owner = AppModel.Window;
            Loaded += GitCommitDialog_Loaded;
        }

        private void GitCommitDialog_Loaded(object sender, RoutedEventArgs e)
        {
            string defaultText = null;
            if (mmApp.Configuration.GitCommitBehavior == GitCommitBehaviors.CommitAndPush)
            {
                ButtonCommitAndPush.IsDefault = true;
                ButtonCommitAndPush.FontWeight = FontWeight.FromOpenTypeWeight(600);
                ButtonCommit.Opacity = 0.6;
                defaultText = "commit and push";
            }            
            else
            {
                ButtonCommit.IsDefault = true;
                ButtonCommit.FontWeight = FontWeight.FromOpenTypeWeight(600);
                ButtonCommitAndPush.Opacity = 0.6;
                defaultText = "commit";
            }
            
            CommitModel.GetRepositoryChanges();

            DataContext = CommitModel;
            
            ShowStatus($"Press Ctrl-Enter to quickly {defaultText}.",8000);
            TextCommitMessage.Focus();
        }


        #region Button  Handlers

        private void ButtonCommit_Click(object sender, RoutedEventArgs e)
        {
            if (CommitModel.CommitChangesToRepository())
            {
                if (string.IsNullOrEmpty(mmApp.Configuration.GitName))
                {
                    mmApp.Configuration.GitName = CommitModel.GitUsername;
                    mmApp.Configuration.GitEmail = CommitModel.GitEmail;
                }
                Close();

                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                
                mmApp.Configuration.GitCommitBehavior = GitCommitBehaviors.Commit;
                AppModel.Window.ShowStatus("Files have been committed in the local repository.", mmApp.Configuration.StatusMessageTimeout);
            }
        }

        private void ButtonCommitAndPush_Click(object sender, RoutedEventArgs e)
        {
            if (CommitModel.CommitChangesToRepository(true))
            {
                if (string.IsNullOrEmpty(mmApp.Configuration.GitName))
                {
                    mmApp.Configuration.GitName = CommitModel.GitUsername;
                    mmApp.Configuration.GitEmail = CommitModel.GitEmail;
                }
                Close();

                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(),System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                mmApp.Configuration.GitCommitBehavior = GitCommitBehaviors.CommitAndPush;
                AppModel.Window.ShowStatus("Files have been committed and pushed to the remote.",mmApp.Configuration.StatusMessageTimeout);
            }            
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void ButtonFileSelection_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckCommitRepository_Click(object sender, RoutedEventArgs e)
        {
            var gh = new GitHelper();
            var repo = gh.OpenRepository(CommitModel.Filename);
            if (repo == null)
            {
                ShowStatus("Couldn't open repository.");
            }

            gh.GetChanges();

        }


        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            CommitModel.GetRepositoryChanges();
        }

        private void ButtonShowUserInfo_Click(object sender, RoutedEventArgs e)
        {
            CommitModel.ShowUserInfo = true;
        }
        #endregion

        #region Item Context Menu

        private void MenuOpenDiffTool_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            if (!File.Exists(mmApp.Configuration.GitDiffExecutable))
            {
                ShowStatusError("There is no diff tool configured. Set the `GitDiffExecutable` setting to your preferred Diff tool.");
                     
                return;
            }

            var fileText = CommitModel.GitHelper.GetComittedFileTextContent(selected.FullPath);
            if (fileText == null)
            {
                ShowStatusError("Unable to load committed file: " + CommitModel.GitHelper.ErrorMessage);
                return;
            }

            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mm_diff_" + System.IO.Path.GetFileName(selected.FullPath));

            File.WriteAllText(tempFile, fileText);

            // Delete files older than 5 minutes
            FileUtils.DeleteTimedoutFiles(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mm_diff_" + "*.*"),300);

            mmFileUtils.ExecuteProcess(mmApp.Configuration.GitDiffExecutable, $"\"{tempFile}\" \"{selected.FullPath}\"");            
        }


        private void MenuIgnoreFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            var gh = CommitModel.GitHelper;
            if (!gh.IgnoreFile(selected.FullPath))
                ShowStatusError(gh.ErrorMessage);
            else
            {
                ShowStatus("File has been added to the .gitignore file.");
                CommitModel.RepositoryStatusItems.Remove(selected);
                ListChangedItems.Items.Refresh();
                //CommitModel.OnPropertyChanged(nameof(CommitModel.RepositoryStatusItems));
            }
        }

        private void MenuUndoFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            CommitModel.GitHelper.UndoChanges(selected.FullPath);
            CommitModel.GetRepositoryChanges();
        }

        #endregion

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
                    var window = win as GitCommitDialog;
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
    }

}
