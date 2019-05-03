
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Markdig.Parsers;
    using MarkdownMonster.Utilities;
    using MarkdownMonster.Windows;
    using MarkdownMonster.Windows.DocumentOutlineSidebar;
    using Westwind.Utilities;

    namespace MarkdownMonster.Controls.ContextMenus
    {
        /// <summary>
        /// Class that handles display and execution of the Tab Context Menu
        /// </summary>
        public class FolderBrowserContextMenu
        {
            private ContextMenu ContextMenu = new ContextMenu();

            private FolderBrowerSidebar Sidebar { get; }
            private TreeView TreeFolderBrowser { get; set; }

            private AppModel Model;

            public FolderBrowserContextMenu(FolderBrowerSidebar sidebar)
            {
                Sidebar = sidebar;
                TreeFolderBrowser = sidebar.TreeFolderBrowser;
                Model = mmApp.Model;
            }


            #region Context Menu Operations

            private void ContextMenu_Closed(object sender, RoutedEventArgs e)
            {
                ContextMenu.IsOpen = false;

                //Model.ActiveEditor?.SetEditorFocus();
                ClearMenu();
            }

            /// <summary>
            /// Clears all items off the menu
            /// </summary>
            public void ClearMenu()
            {
                if (ContextMenu == null)
                {
                    ContextMenu = new ContextMenu();
                    ContextMenu.Closed += ContextMenu_Closed;
                }
                else
                    ContextMenu?.Items.Clear();
            }

            public void Show()
            {
                if (ContextMenu != null)
                {
                    Model.ActiveEditor?.SetMarkdownMonsterWindowFocus();

                    ContextMenu.PlacementTarget = Model.Window.TabControl;
                    ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                    ContextMenu.VerticalOffset = 8;

                    ContextMenu.Focus();
                    ContextMenu.IsOpen = true;
                    TreeFolderBrowser.ContextMenu = ContextMenu;

                    var item = ContextMenu.Items[0] as MenuItem;
                    item.Focus();
                }
            }

            public void ShowContextMenu()
            {
                ClearMenu();

                var cm = ContextMenu;
                var pathItem = TreeFolderBrowser.SelectedItem as PathItem;
                if (pathItem == null)
                    return;

                cm.Items.Clear();

                var ci = new MenuItem();
                ci.Header = "_New File";
                ci.InputGestureText = "Shift-F2";
                ci.Click += MenuAddFile_Click;
                cm.Items.Add(ci);

                ci = new MenuItem();
                ci.Header = "New Folder";
                ci.Click += MenuAddDirectory_Click;
                cm.Items.Add(ci);

                cm.Items.Add(new Separator());

                ci = new MenuItem();

                if (pathItem.DisplayName != "..")
                {
                    ci.Header = "Delete";
                    ci.InputGestureText = "Del";
                    ci.Click += MenuDeleteFile_Click;
                    cm.Items.Add(ci);

                    ci = new MenuItem();
                    ci.Header = "Rename";
                    ci.InputGestureText = "F2";
                    ci.Click += MenuRenameFile_Click;
                    cm.Items.Add(ci);

                    cm.Items.Add(new Separator());
                }

                ci = new MenuItem();
                ci.Header = "Find Files";
                ci.InputGestureText = "Ctrl-F";
                ci.Click += Sidebar.MenuFindFiles_Click;
                cm.Items.Add(ci);

                ci = new MenuItem();
                ci.Header = "Add to Favorites";
                ci.Command = Model.Commands.AddFavoriteCommand;
                ci.CommandParameter = pathItem.FullPath;
                cm.Items.Add(ci);

                cm.Items.Add(new Separator());

                if (Clipboard.ContainsImage())
                {
                    cm.Items.Add(new Separator());
                    ci = new MenuItem();
                    ci.Header = "Paste Image File from Clipboard";
                    ci.Command = Model.Commands.PasteImageToFileCommand;
                    ci.CommandParameter = pathItem.FullPath;
                    cm.Items.Add(ci);
                    cm.Items.Add(new Separator());
                }


                if (pathItem.IsImage)
                {
                    ci = new MenuItem();
                    ci.Header = "Show Image";
                    ci.Click += MenuShowImage_Click;
                    cm.Items.Add(ci);

                    ci = new MenuItem();
                    ci.Header = "Edit Image";
                    ci.Click += MenuEditImage_Click;
                    cm.Items.Add(ci);

                    ci = new MenuItem();
                    ci.Header = "Optimize Image";
                    ci.Click += MenuOptimizeImage_Click;
                    cm.Items.Add(ci);

                }
                else
                {
                    if (pathItem.IsFile)
                    {
                        ci = new MenuItem();
                        ci.Header = "Open in Editor";
                        ci.Click += MenuOpenInEditor_Click;
                        cm.Items.Add(ci);
                    }

                    ci = new MenuItem();
                    ci.Header = "Open in Shell";
                    ci.Click += MenuOpenWithShell_Click;
                    cm.Items.Add(ci);
                }


                ci = new MenuItem();
                ci.Header = "Open With...";
                ci.Command = Model.Commands.OpenWithCommand;
                ci.CommandParameter = pathItem.FullPath;
                cm.Items.Add(ci);

                cm.Items.Add(new Separator());

                ci = new MenuItem();
                ci.Header = "Open in Terminal";
                ci.Click += MenuOpenTerminal_Click;
                cm.Items.Add(ci);

                ci = new MenuItem();
                ci.Header = "Show in Explorer";
                ci.Click += MenuOpenInExplorer_Click;
                cm.Items.Add(ci);

                cm.Items.Add(new Separator());


                bool isGit = false;
                var git = new GitHelper();
                string gitRemoteUrl = null;
                using (var repo = git.OpenRepository(pathItem.FullPath))
                {
                    isGit = repo != null;
                    if (isGit)
                        gitRemoteUrl = repo.Network?.Remotes.FirstOrDefault()?.Url;
                }

                if (isGit)
                {
                    ci = new MenuItem();
                    ci.Header = "Commit to _Git...";
                    ci.InputGestureText = "Ctrl-G";
                    ci.Click += MenuCommitGit_Click;
                    cm.Items.Add(ci);

                    if (pathItem.FileStatus == LibGit2Sharp.FileStatus.ModifiedInIndex ||
                        pathItem.FileStatus == LibGit2Sharp.FileStatus.ModifiedInWorkdir)
                    {
                        ci = new MenuItem();
                        ci.Header = "_Undo Changes in Git";
                        ci.InputGestureText = "Ctrl-Z";
                        ci.Click += MenuUndoGit_Click;
                        cm.Items.Add(ci);
                    }

                    ci = new MenuItem();
                    ci.Header = "Open Folder in Git Client";
                    ci.Click += MenuGitClient_Click;
                    ci.IsEnabled = Model.Configuration.Git.GitClientExecutable != null &&
                                   File.Exists(Model.Configuration.Git.GitClientExecutable);
                    cm.Items.Add(ci);

                    if (pathItem.FileStatus != LibGit2Sharp.FileStatus.Nonexistent)
                    {
                        if (gitRemoteUrl != null && gitRemoteUrl.Contains("github.com"))
                        {
                            ci = new MenuItem();
                            ci.Header = "Open on GitHub";
                            ci.Command = mmApp.Model.Commands.OpenOnGithubCommand;
                            ci.CommandParameter = pathItem.FullPath;
                            cm.Items.Add(ci);
                        }
                    }

                    cm.Items.Add(new Separator());
                }

                ci = new MenuItem();
                ci.Header = "Copy Path to Clipboard";
                ci.Click += MenuCopyPathToClipboard_Click;
                cm.Items.Add(ci);

                if (pathItem.IsFolder)
                {
                    cm.Items.Add(new Separator());

                    ci = new MenuItem();
                    ci.Header = "Open Folder Browser here";
                    ci.Click += MenuOpenFolderBrowserHere_Click;
                    cm.Items.Add(ci);
                }

                cm.IsOpen = true;

                Show();
            }

            #endregion


            #region File and Folder Operations

            public void MenuDeleteFile_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                if (MessageBox.Show(
                        "Delete:\r\n" +
                        selected.FullPath + "\r\n\r\n" +
                        "Are you sure?",
                        "Delete File",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                try
                {
                    //Directory.Delete(selected.FullPath, true);
                    //File.Delete(selected.FullPath);

                    // Recyle Bin Code can handle both files and directories
                    if (!mmFileUtils.MoveToRecycleBin(selected.FullPath))
                        return;

                    var parent = selected.Parent;

                    var index = -1;

                    var file = parent?.Files?.FirstOrDefault(fl => fl.FullPath == selected.FullPath);
                    if (file != null)
                    {
                        var tab = Model.Window.GetTabFromFilename(file.FullPath);
                        if (tab != null)
                            Model.Window.CloseTab(tab, dontPromptForSave: true);



                        if (parent != null)
                        {
                            index = parent.Files.IndexOf(selected);
                            parent.Files.Remove(file);
                            if (index > 0)
                                Sidebar.SetTreeViewSelectionByItem(parent.Files[index]);
                        }
                    }

                    // Delay required to overcome editor focus after MsgBox
                    Model.Window.Dispatcher.Delay(700, s => TreeFolderBrowser.Focus());
                }
                catch (Exception ex)
                {
                    Model.Window.ShowStatusError("Delete operation failed: " + ex.Message);
                }
            }

            public void MenuAddFile_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                {
                    // No files/folders
                    selected = new PathItem() {IsFolder = true, FullPath = Sidebar.FolderPath};
                    Sidebar.ActivePathItem = selected;
                }

                string path;
                if (selected.FullPath == "..")
                    path = Path.Combine(Sidebar.FolderPath, "NewFile.md");
                if (!selected.IsFolder)
                    path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFile.md");
                else
                {
                    var treeItem = Sidebar.GetTreeviewItem(selected);
                    if (treeItem != null)
                        treeItem.IsExpanded = true;

                    path = Path.Combine(selected.FullPath, "NewFile.md");
                }

                var item = new PathItem
                {
                    FullPath = path,
                    IsFolder = false,
                    IsFile = true,
                    IsEditing = true,
                    IsSelected = true
                };
                item.SetIcon();

                if (selected.FullPath == "..")
                    item.Parent = Sidebar.ActivePathItem; // current path
                else if (!selected.IsFolder)
                    item.Parent = selected.Parent;
                else
                    item.Parent = selected;

                item.Parent.Files.Insert(0, item);
                Sidebar.SetTreeViewSelectionByItem(item);
            }


            private void MenuAddDirectory_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null || selected.Parent == null)
                {
                    // No files/folders
                    selected = new PathItem() {IsFolder = true, FullPath = Sidebar.FolderPath};
                    Sidebar.ActivePathItem = selected;
                }

                string path;
                if (!selected.IsFolder)
                    path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFolder");
                else
                {
                    var treeItem = Sidebar.GetTreeviewItem(selected);
                    if (treeItem != null)
                        treeItem.IsExpanded = true;

                    path = Path.Combine(selected.FullPath, "NewFolder");
                }

                var item = new PathItem {FullPath = path, IsFolder = true, IsEditing = true, IsSelected = true};
                item.SetIcon();

                if (!selected.IsFolder)
                    item.Parent = selected.Parent;
                else
                    item.Parent = selected;

                item.Parent.Files.Insert(0, item);

                Sidebar.SetTreeViewSelectionByItem(item);
            }


            public void MenuRenameFile_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                // Start Editing the file name
                selected.EditName = selected.DisplayName;
                selected.IsEditing = true;


                var tvItem = Sidebar.GetTreeviewItem(selected);
                if (tvItem != null)
                {
                    var tb = WindowUtilities.FindVisualChild<TextBox>(tvItem);
                    tb?.Focus();
                }
            }

            #endregion

            #region Git Operations

            public void MenuCommitGit_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                var model = mmApp.Model;

                string file = selected.FullPath;

                if (string.IsNullOrEmpty(file))
                    return;

                bool pushToGit = mmApp.Configuration.Git.GitCommitBehavior == GitCommitBehaviors.CommitAndPush;
                model.Commands.CommitToGitCommand.Execute(file);
            }

            public void MenuUndoGit_Click(object sende, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                if (selected.FileStatus != LibGit2Sharp.FileStatus.ModifiedInIndex &&
                    selected.FileStatus != LibGit2Sharp.FileStatus.ModifiedInWorkdir)
                    return;

                var gh = new GitHelper();
                gh.UndoChanges(selected.FullPath);


                // force editors to update
                mmApp.Model.Window.CheckFileChangeInOpenDocuments();
            }

            private void MenuGitClient_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                var model = mmApp.Model;

                var path = selected.FullPath;
                if (selected.IsFile)
                    path = Path.GetDirectoryName(path);

                Model.Commands.OpenGitClientCommand.Execute(path);
            }

            #endregion

            #region Shell/Terminal Operations

            public void MenuOpenInExplorer_Click(object sender, RoutedEventArgs e)
            {
                string folder = Sidebar.FolderPath;

                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected != null)
                    folder = selected.FullPath; // Path.GetDirectoryName(selected.FullPath);

                if (string.IsNullOrEmpty(folder))
                    return;

                if (selected == null || selected.IsFolder)
                    ShellUtils.GoUrl(folder);
                else
                    ShellUtils.OpenFileInExplorer(folder);
            }

            public void MenuOpenFolderBrowserHere_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                if (selected.FullPath == "..")
                    Sidebar.FolderPath = Path.GetDirectoryName(Sidebar.FolderPath.TrimEnd('\\'));
                else
                {
                    if (Directory.Exists(Sidebar.FolderPath))
                        Sidebar.FolderPath = selected.FullPath;
                    else
                        Sidebar.FolderPath = Path.GetDirectoryName(Sidebar.FolderPath);
                }

            }

            public void MenuOpenTerminal_Click(object sender, RoutedEventArgs e)
            {
                string folder = Sidebar.FolderPath;

                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected != null)
                    folder = Path.GetDirectoryName(selected.FullPath);

                if (string.IsNullOrEmpty(folder))
                    return;

                mmFileUtils.OpenTerminal(folder);
            }

            public void MenuOpenWithShell_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                try
                {
                    ShellUtils.GoUrl(selected.FullPath);
                }
                catch
                {
                    Model.Window.ShowStatusError($"Unable to open file {selected.FullPath}");

                }
            }

            public void MenuCopyPathToClipboard_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;

                string clipText = null;
                if (selected == null)
                    clipText = Sidebar.FolderPath;
                else if (selected.FullPath == "..")
                    clipText = Path.GetDirectoryName(Sidebar.FolderPath);
                else
                    clipText = selected.FullPath;

                if (!string.IsNullOrEmpty(clipText))
                {
                    if (ClipboardHelper.SetText(clipText))
                        Model.Window.ShowStatus($"Path '{clipText}' has been copied to the Clipboard.",
                            mmApp.Configuration.StatusMessageTimeout);
                }

            }


            public void MenuOpenInEditor_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                if (selected.IsFolder)
                    ShellUtils.GoUrl(selected.FullPath);
                else
                    Model.Window.OpenTab(selected.FullPath, rebindTabHeaders: true);
            }

            #endregion


            #region Image Operations

            public void MenuShowImage_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                mmFileUtils.OpenImageInImageViewer(selected.FullPath);
            }

            public void MenuEditImage_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                mmFileUtils.OpenImageInImageEditor(selected.FullPath);
            }

            public void MenuOptimizeImage_Click(object sender, RoutedEventArgs e)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                if (selected == null)
                    return;

                long filesize = new FileInfo(selected.FullPath).Length;
                Model.Window.ShowStatusProgress("Optimizing image " + selected.FullPath, 10000);

                mmFileUtils.OptimizeImage(selected.FullPath, 0, new Action<bool>((res) =>
                {
                    Sidebar.Dispatcher.Invoke(() =>
                    {
                        var fi = new FileInfo(selected.FullPath);
                        long filesize2 = fi.Length;

                        decimal diff = 0;
                        if (filesize2 < filesize)
                            diff = (Convert.ToDecimal(filesize2) / Convert.ToDecimal(filesize)) * 100;
                        if (diff > 0)
                        {
                            mmApp.Model.Window.ShowStatusSuccess(
                                $"Image size reduced by {(100 - diff):n2}%. New size: {(Convert.ToDecimal(filesize2) / 1000):n1}kb");
                            Model.Window.OpenBrowserTab(selected.FullPath, isImageFile: true);
                        }
                        else
                            mmApp.Model.Window.ShowStatusError("Image optimization couldn't improve image size.", 5000);
                    }, DispatcherPriority.ApplicationIdle);
                }));
            }

            #endregion
        }
    }

