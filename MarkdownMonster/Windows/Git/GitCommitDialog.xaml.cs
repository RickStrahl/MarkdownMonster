using System;
using System.Collections.Generic;
using System.ComponentModel;
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



        #region Startup and Shutdown
        public GitCommitDialog(string fileOrPath, bool commitRepo = false)
        {
            InitializeComponent();
            AppModel = mmApp.Model;

            CommitModel = new GitCommitModel(fileOrPath,commitRepo);
            if (!commitRepo)    
                CommitModel.CommitMessage = $"Update {System.IO.Path.GetFileName(fileOrPath)}";

            CommitModel.CommitWindow = this;

            mmApp.SetThemeWindowOverride(this);

            Owner = AppModel.Window;
            Loaded += GitCommitDialog_Loaded;
        }

        
        private void GitCommitDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CommitModel.GitHelper.OpenRepository(CommitModel.Filename);

            // Check if a remote exists and disable push if not
            CommitModel.Remote = CommitModel.GitHelper.Repository.Network.Remotes?.FirstOrDefault()?.Name;
            if (CommitModel.Remote == null)
            {
                ButtonCommitAndPush.IsEnabled = false;
                ButtonOpenRemote.IsEnabled = false;
                AppModel.Configuration.Git.GitCommitBehavior = GitCommitBehaviors.Commit;
            }

            // get the main branch
            CommitModel.Branch = CommitModel.GitHelper.Repository?.Head?.FriendlyName;
            
            string defaultText = null;
            if (AppModel.Configuration.Git.GitCommitBehavior == GitCommitBehaviors.CommitAndPush)
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            AppModel.Window.Activate();
        }
        #endregion

        #region Button  Handlers

        private async void ButtonCommit_Click(object sender, RoutedEventArgs e)
        {
            if (await CommitModel.CommitChangesToRepository())
            {
                if (string.IsNullOrEmpty(CommitModel.GitUsername))
                    mmApp.Configuration.Git.GitName = CommitModel.GitUsername;
                if (string.IsNullOrEmpty(CommitModel.GitEmail))
                    mmApp.Configuration.Git.GitEmail = CommitModel.GitEmail;
                
                if (AppModel.Configuration.Git.CloseAfterCommit)
                {
                    Close();
                    AppModel.Window.ShowStatus("Files have been committed in the local repository.",
                        mmApp.Configuration.StatusMessageTimeout,
                        FontAwesomeIcon.CheckCircleOutline);
                }
                else
                {                    
                    CommitModel.GetRepositoryChanges();
                    if (CommitModel.RepositoryStatusItems.Count < 1)
                    {
                        Close();
                        AppModel.Window.ShowStatus("Files have been committed and pushed to the remote.",
                            mmApp.Configuration.StatusMessageTimeout);
                    }
                    else
                        ShowStatus("Files have been committed in the local repository.",
                            mmApp.Configuration.StatusMessageTimeout,
                            FontAwesomeIcon.CheckCircleOutline);

                }

                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    await Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                
                mmApp.Configuration.Git.GitCommitBehavior = GitCommitBehaviors.Commit;
                
            }
        }

        private async void ButtonCommitAndPush_Click(object sender, RoutedEventArgs e)
        {
            if (await CommitModel.CommitChangesToRepository(true))
            {
                if (string.IsNullOrEmpty(mmApp.Configuration.Git.GitName))
                {
                    mmApp.Configuration.Git.GitName = CommitModel.GitUsername;
                    mmApp.Configuration.Git.GitEmail = CommitModel.GitEmail;
                }

                if (AppModel.Configuration.Git.CloseAfterCommit)
                {
                    Close();
                    AppModel.Window.ShowStatusSuccess("Files have been committed and pushed to the remote.");

                }
                else
                {
                    // reload settings                    
                    CommitModel.GetRepositoryChanges();
                    if (CommitModel.RepositoryStatusItems.Count < 1)
                    {
                        Close();
                        AppModel.Window.ShowStatusSuccess("Files have been committed and pushed to the remote.");
                    }
                    else
                        ShowStatusSuccess("Files have been committed and pushed to the remote.");                    
                }

                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    await Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(),System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                mmApp.Configuration.Git.GitCommitBehavior = GitCommitBehaviors.CommitAndPush;               
            }            
        }

        private async void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            ShowStatusProgress("Pulling changes from the remote origin...");

            if (await CommitModel.PullChangesAsync())
            {
                // refresh the file model
                CommitModel.GetRepositoryChanges();

                // Refresh the folder browser Git status icons
                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    await Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                ShowStatusSuccess("Changes have been pulled from the remote origin.");
                return;
            }

            ShowStatusError("Failed to pull changes from the server: " + CommitModel.GitHelper.ErrorMessage);
        }

        private void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            ShowStatusProgress("Pushing changes to the remote origin...");

            if (CommitModel.PushChanges())
            {
                // refresh the file model
                CommitModel.GetRepositoryChanges();

                // Refresh the folder browser Git status icons
                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                ShowStatusSuccess("Changes pushed to the remote origin.");
                return;
            }

            ShowStatusError("Failed to pull changes from the server: " + CommitModel.GitHelper.ErrorMessage);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckCommitRepository_Click(object sender, RoutedEventArgs e)
        {
            var gh = new GitHelper();
            var repo = gh.OpenRepository(CommitModel.Filename);
            if (repo == null)            
                ShowStatus("Couldn't open repository.");            
        }


        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            CommitModel.GetRepositoryChanges();
        }

        private void ButtonShowUserInfo_Click(object sender, RoutedEventArgs e)
        {
            CommitModel.ShowUserInfo = true;
        }

        private void CheckBoxListItemChecked_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null)
                return;            
            
            var isChecked = checkBox.IsChecked;

            var checkItem = checkBox.DataContext as RepositoryStatusItem;
            if (checkItem != null)
            {
                ListBoxItem selectedListBoxItem =
                    ListChangedItems.ItemContainerGenerator.ContainerFromItem(checkItem) as ListBoxItem;
                if (selectedListBoxItem != null)
                    selectedListBoxItem.IsSelected = true;
            }


            var selList = new List<RepositoryStatusItem>();
            foreach (var item in ListChangedItems.SelectedItems)
                selList.Add(item as RepositoryStatusItem);

            foreach (RepositoryStatusItem item in selList)
            {                
                item.Selected = isChecked.Value;
            }
        }

        #endregion

        #region Item Context Menu

        private void MenuOpenDiffTool_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            if (!CommitModel.GitHelper.OpenDiffTool(selected.FullPath))
                ShowStatusError(CommitModel.GitHelper.ErrorMessage);
        }

        private void ButtonOpenRemote_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
            {
                var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                if (remoteUrl == null)
                    return;

                ShowStatus("Opening Url: " + remoteUrl);
                ShellUtils.GoUrl(remoteUrl.Replace(".git", ""));
            }
        }


        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            if (!CommitModel.GitHelper.OpenDiffTool(selected.FullPath))
                ShowStatusError(CommitModel.GitHelper.ErrorMessage);
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
                selected.FileStatus = FileStatus.Ignored;
            }
        }


        private void MenuUndoFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            CommitModel.GitHelper.UndoChanges(selected.FullPath);
            var stat = CommitModel.GitHelper.GetGitStatusForFile(selected.FullPath);
            if (stat == FileStatus.Unaltered)
                CommitModel.RepositoryStatusItems.Remove(selected);
            else
                selected.FileStatus = stat;            
        }

        private void MenuOpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            ShellUtils.OpenFileInExplorer(selected.FullPath);
        }

        private void MenuDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            if (File.Exists(selected.FullPath))
            {
                File.Delete(selected.FullPath);
                var stat = CommitModel.GitHelper.GetGitStatusForFile(selected.FullPath);
                if (stat == FileStatus.Nonexistent)
                    CommitModel.RepositoryStatusItems.Remove(selected);
                else
                    selected.FileStatus = stat;
            }

        }

        private void MenuOpenInRemote_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
            {
                var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                if (remoteUrl == null)
                    return;
                remoteUrl = remoteUrl.Replace(".git", "");

                remoteUrl += "/blob/master/" + selected.Filename;

                ShowStatus("Opening Url: " + remoteUrl);
                ShellUtils.GoUrl(remoteUrl);
            }


        }

        #endregion

        #region StatusBar Display

        DebounceDispatcher debounce = new DebounceDispatcher();
        public void ShowStatus(string message = null, int milliSeconds = 0,
            FontAwesomeIcon icon = FontAwesomeIcon.None,
            Color color = default(Color),
            bool spin = false)
        {            
            if (color == default(Color))
                color = Colors.Green;

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
        /// Displays an error message using common defaults for a timeout milliseconds
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
        /// Shows a success message with a green check icon for the timeout
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        public void ShowStatusSuccess(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.CheckCircle, Color color = default(Color))
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.Green;

            ShowStatus(message, timeout, icon, color);
        }


        /// <summary>
        /// Displays an Progress message using common defaults including a spinning icon
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="icon">optional icon (warning)</param>
        /// <param name="color">optional color (firebrick)</param>
        /// <param name="spin"></param>
        public void ShowStatusProgress(string message, int timeout = -1, FontAwesomeIcon icon = FontAwesomeIcon.CircleOutlineNotch, Color color = default(Color), bool spin = true)
        {
            if (timeout == -1)
                timeout = mmApp.Configuration.StatusMessageTimeout;

            if (color == default(Color))
                color = Colors.Goldenrod;

            ShowStatus(message, timeout, icon, color, spin);
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
                StatusIcon.SpinDuration = 2;

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

        #region Context Menus
        private void ListChangedItems_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;
            
            //<MenuItem Header="Ignore File"  Click="MenuIgnoreFile_Click"/>
            //<MenuItem Header="Undo Changes" Click="MenuUndoFile_Click" />
            //<MenuItem Header="Delete File" Click="MenuDeleteFile_Click" />
            //<Separator />
            //<MenuItem Header="Open in external Diff tool" Click="MenuOpenDiffTool_Click"/>
            //<MenuItem Header="Open in Explorer" Click="MenuOpenInExplorer_Click" />

            var ctx = new ContextMenu();

            e.Handled = true;

            var mi = new MenuItem
            {
                Header = "Ignore File"
            };
            mi.Click += MenuIgnoreFile_Click;
            ctx.Items.Add(mi);

            if (selected.FileStatus != FileStatus.NewInWorkdir &&
                selected.FileStatus != FileStatus.NewInIndex)
            {
                mi = new MenuItem
                {
                    Header = "Undo Changes"
                };
                mi.Click += MenuUndoFile_Click;
                ctx.Items.Add(mi);
            }
            if (selected.FileStatus != FileStatus.DeletedFromWorkdir &&
                selected.FileStatus != FileStatus.DeletedFromIndex)
            {
                mi = new MenuItem
                {
                    Header = "Delete File"
                };
                mi.Click += MenuDeleteFile_Click;
                ctx.Items.Add(mi);
            }

            ctx.Items.Add(new Separator());


            if (selected.FileStatus != FileStatus.NewInIndex &&
                selected.FileStatus != FileStatus.NewInWorkdir)
            {
                mi = new MenuItem
                {
                    Header = "Open in external Diff Tool"
                };
                mi.Click += MenuOpenDiffTool_Click;
                ctx.Items.Add(mi);
            }

            mi = new MenuItem
            {
                Header = "Open in Explorer"
            };
            mi.Click += MenuOpenInExplorer_Click;
            ctx.Items.Add(mi);

            if (!string.IsNullOrEmpty(CommitModel.Remote))
            {
                using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
                {
                    var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                    if (remoteUrl != null && remoteUrl.Contains("github.com"))
                    {
                        mi = new MenuItem
                        {
                            Header = "Open on Github"
                        };
                        mi.Click += MenuOpenInRemote_Click;
                        ctx.Items.Add(mi);
                    }
                }
            }

            ListChangedItems.ContextMenu = ctx;
            ListChangedItems.ContextMenu.IsOpen = true;

        }

        private void ButtonGitToolsContext_Click(object sender, RoutedEventArgs e)
        {
            var ctx = new ContextMenu();
            ButtonGitToolsContext.ContextMenu = ctx;

            var mi = new MenuItem
            {
                Header = "Clone Repository",
                Command = AppModel.Commands.OpenFromGitRepoCommand,                
            };            
            ctx.Items.Add(mi);

            mi = new MenuItem
            {
                Header = "Create Repository",
                Command = AppModel.Commands.OpenFromGitRepoCommand,
                CommandParameter="Create"
            };
            ctx.Items.Add(mi);
            mi = new MenuItem
            {
                Header = "Add Git Remote",
                Command = AppModel.Commands.OpenFromGitRepoCommand,
                CommandParameter = "AddRemote"
            };
            ctx.Items.Add(mi);            
            ctx.IsOpen = true;
        }

        #endregion
    }

}
