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
using System.Windows.Threading;
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

        public StatusBarHelper StatusBar { get; set; }

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

            StatusBar = new StatusBarHelper(StatusText, StatusIcon);
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
                defaultText = "commit and push";
                var panel = ButtonCommitAndPush.Parent as ToolBar;
                
                // move to first position
                panel.Items.Remove(ButtonCommitAndPush);
                panel.Items.Insert(0, ButtonCommitAndPush);
            
            }            
            else
            {
                ButtonCommit.IsDefault = true;
                ButtonCommit.FontWeight = FontWeight.FromOpenTypeWeight(600);
                defaultText = "commit";
            }
            
            CommitModel.GetRepositoryChanges();

            DataContext = CommitModel;
            
            StatusBar.ShowStatus($"Press Ctrl-Enter to quickly {defaultText}.",8000);
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
                    CommitModel.CommitMessage = string.Empty;
                    Dispatcher.Invoke(() =>
                    {
                        CommitModel.GetRepositoryChanges();
                        StatusBar.ShowStatus("Files have been committed in the local repository.",
                                mmApp.Configuration.StatusMessageTimeout,
                                FontAwesomeIcon.CheckCircleOutline);
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

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
                    CommitModel.CommitMessage = string.Empty;
                    Dispatcher.Invoke(() =>
                    {
                        // reload settings                    
                        CommitModel.GetRepositoryChanges();                        
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                    StatusBar.ShowStatusSuccess("Files have been committed and pushed to the remote.");                    
                }

                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    await Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(),System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                mmApp.Configuration.Git.GitCommitBehavior = GitCommitBehaviors.CommitAndPush;               
            }            
        }

        private async void ButtonPull_Click(object sender, RoutedEventArgs e)
        {
            StatusBar.ShowStatusProgress("Pulling changes from the remote origin...");

            if (await CommitModel.PullChangesAsync())
            {
                // refresh the file model
                CommitModel.GetRepositoryChanges();

                // Refresh the folder browser Git status icons
                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    await Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                StatusBar.ShowStatusSuccess("Changes have been pulled from the remote origin.");
                return;
            }

            StatusBar.ShowStatusError("Failed to pull changes from the server: " + CommitModel.GitHelper.ErrorMessage);
        }

        private async void ButtonPush_Click(object sender, RoutedEventArgs e)
        {
            StatusBar.ShowStatusProgress("Pushing changes to the remote origin...");

            if (await CommitModel.PushChangesAsync())
            {
                // refresh the file model
                CommitModel.GetRepositoryChanges();

                // Refresh the folder browser Git status icons
                if (AppModel.WindowLayout.IsLeftSidebarVisible)
                    Dispatcher.InvokeAsync(() => AppModel.Window.FolderBrowser.UpdateGitStatus(), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                StatusBar.ShowStatusSuccess("Changes pushed to the remote origin.");
                return;
            }

            StatusBar.ShowStatusError($"Failed to pull changes from the server: {CommitModel.GitHelper.ErrorMessage}");
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
                StatusBar.ShowStatusError("Couldn't open repository.");            
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
                StatusBar.ShowStatusError(CommitModel.GitHelper.ErrorMessage);
        }

        private void ButtonOpenRemote_Click(object sender, RoutedEventArgs e)
        {
            using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
            {
                var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                if (remoteUrl == null)
                    return;

                StatusBar.ShowStatusSuccess("Opening Url: " + remoteUrl);
                ShellUtils.GoUrl(remoteUrl.Replace(".git", ""));
            }
        }


        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            if (!CommitModel.GitHelper.OpenDiffTool(selected.FullPath))
                StatusBar.ShowStatusError(CommitModel.GitHelper.ErrorMessage);
        }

        private void MenuIgnoreFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListChangedItems.SelectedItem as RepositoryStatusItem;
            if (selected == null)
                return;

            var gh = CommitModel.GitHelper;
            if (!gh.IgnoreFile(selected.FullPath))
                StatusBar.ShowStatusError(gh.ErrorMessage);
            else
            {
                StatusBar.ShowStatusSuccess("File has been added to the .gitignore file.");
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

                StatusBar.ShowStatus("Opening Url: " + remoteUrl);
                ShellUtils.GoUrl(remoteUrl);
            }


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
