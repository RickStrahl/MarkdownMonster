using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.Annotations;
using MarkdownMonster.Controls;
using MarkdownMonster.Controls.ContextMenus;
using MarkdownMonster.Utilities;
using Westwind.Utilities;
using UserControl = System.Windows.Controls.UserControl;


namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for FolderBrowerSidebar.xaml
    /// </summary>
    public partial class FolderBrowerSidebar : UserControl, INotifyPropertyChanged
    {
        public MainWindow Window { get; set; }
        public AppModel AppModel { get; set; }

        public FolderBrowserContextMenu FolderBrowserContextMenu { get; set; }

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                if (value == _folderPath) return;

                string filename = null;
                if (!Directory.Exists(value))
                {
                    if (!File.Exists(value))
                        value = null;
                    else
                    {
                        filename = value;
                        value = Path.GetDirectoryName(value);
                    }
                }

                var previousFolder = _folderPath;

                _folderPath = value;
                OnPropertyChanged(nameof(FolderPath));

                if (Window == null) return;

                SearchText = null;
                SearchSubTrees = false;
                SearchPanel.Visibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(_folderPath))
                    ActivePathItem = new PathItem();  // empty the folderOrFilePath browser
                else
                {
                    if (filename != null && previousFolder == _folderPath && string.IsNullOrEmpty(SearchText))
                        SelectFileInSelectedFolderBrowserFolder(filename);
                    else
                        SetTreeFromFolder(filename ?? _folderPath, filename != null, SearchText);
                }


                if (ActivePathItem != null)
                {
                    _folderPath = value;
                    mmApp.Configuration.FolderBrowser.AddRecentFolder(_folderPath);

                    OnPropertyChanged(nameof(FolderPath));
                    OnPropertyChanged(nameof(ActivePathItem));
                }
            }
        }
        private string _folderPath;


        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;



        public bool SearchSubTrees
        {
            get { return _searchSubTrees; }
            set
            {
                if (value == _searchSubTrees) return;
                _searchSubTrees = value;
                OnPropertyChanged();
            }
        }

        private bool _searchSubTrees;


        public PathItem ActivePathItem
        {
            get { return _activePath; }
            set
            {
                if (Equals(value, _activePath)) return;
                _activePath = value;
                OnPropertyChanged(nameof(ActivePathItem));
            }
        }
        private PathItem _activePath;


        /// <summary>
        /// Internal value
        /// </summary>
        public  FolderStructure FolderStructure { get; } = new FolderStructure();


        /// <summary>
        /// Filewatcher used to detect changes to files in the active folder
        /// including subdirectories.
        /// </summary>
        private FileSystemWatcher FileWatcher = null;


        #region Initialization

        public FolderBrowerSidebar()
        {

            InitializeComponent();
            Focusable = true;


            Loaded += FolderBrowerSidebar_Loaded;
            Unloaded += (s, e) => ReleaseFileWatcher();

        }

        /// <summary>
        /// Updates the Git status of the files currently active
        /// in the tree.
        /// </summary>
        /// <param name="pathItem"></param>
        public void UpdateGitStatus(PathItem pathItem = null)
        {
            if (pathItem == null)
                pathItem = ActivePathItem;

            FolderStructure.UpdateGitFileStatus(pathItem);
        }

        private void FolderBrowerSidebar_Loaded(object sender, RoutedEventArgs e)
        {
            AppModel = mmApp.Model;
            Window = AppModel.Window;
            DataContext = this;

            FolderBrowserContextMenu = new FolderBrowserContextMenu(this);

            // Load explicitly here to fire *after* behavior has attached
            ComboFolderPath.PreviewKeyUp += ComboFolderPath_PreviewKeyDown;

            TreeFolderBrowser.GotFocus += TreeFolderBrowser_GotFocus;
            ComboFolderPath.GotFocus += TreeFolderBrowser_GotFocus;
        }


        private void TreeFolderBrowser_GotFocus(object sender, RoutedEventArgs e)
        {
            // ensure that directory wasn't deleted under us
            if (!Directory.Exists(FolderPath))
                FolderPath = null;
        }
        #endregion

        #region FileWatcher

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (mmApp.Model == null || mmApp.Model.Window == null)
                return;

            mmApp.Model.Window.Dispatcher.Invoke(() =>
            {

                var file = e.FullPath;
                var oldFile = e.OldFullPath;

                var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, oldFile);
                if (pi == null)
                    return;

                pi.FullPath = file;
                pi.Parent.Files.Remove(pi);

                FolderStructure.InsertPathItemInOrder(pi, pi.Parent);
            },DispatcherPriority.ApplicationIdle);
        }

        private void FileWatcher_CreateOrDelete(object sender, FileSystemEventArgs e)
        {

            if (mmApp.Model == null || mmApp.Model.Window == null)
                return;

            if (!Directory.Exists(FolderPath))
            {
                FolderPath = null;
                return;
            }

            var file = e.FullPath;
            if (string.IsNullOrEmpty(file))
                return;

            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                mmApp.Model.Window.Dispatcher.Invoke(() =>
                {
                    var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, file);
                    if (pi == null)
                        return;

                    pi.Parent.Files.Remove(pi);

                    //Debug.WriteLine("After: " + pi.Parent.Files.Count + " " + file);
                },DispatcherPriority.ApplicationIdle);
            }

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                mmApp.Model.Window.Dispatcher.Invoke(() =>
                {
                    // Skip ignored Extensions
                    string[] extensions = null;
                    if (!string.IsNullOrEmpty(mmApp.Model.Configuration.FolderBrowser.IgnoredFileExtensions))
                        extensions = mmApp.Model.Configuration.FolderBrowser.IgnoredFileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (extensions != null && extensions.Any(ext => file.EndsWith(ext,StringComparison.InvariantCultureIgnoreCase)))
                       return;

                    var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, file);
                    if (pi != null) // Already exists in the tree
                        return;

                    // does the path exist?
                    var parentPathItem =
                        FolderStructure.FindPathItemByFilename(ActivePathItem, Path.GetDirectoryName(file));

                    // Path either doesn't exist or is not expanded yet so don't attach - opening will trigger
                    if (parentPathItem == null ||
                        (parentPathItem.Files.Count == 1 && parentPathItem.Files[0] == PathItem.Empty) )
                        return;

                    bool isFolder = Directory.Exists(file);
                    pi = new PathItem()
                    {
                        FullPath = file,
                        IsFolder = isFolder,
                        IsFile = !isFolder,
                        Parent = parentPathItem
                    };
                    pi.SetIcon();

                    FolderStructure.InsertPathItemInOrder(pi, parentPathItem);
                }, DispatcherPriority.ApplicationIdle);
            }

        }


        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (mmApp.Model == null || mmApp.Model.Window == null)
                return;

            Dispatcher.Invoke(() =>
            {
                var file = e.FullPath;

                var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, file);
                if (pi == null)
                    return;

                var gh = new GitHelper();
                pi.FileStatus = gh.GetGitStatusForFile(pi.FullPath);

            }, DispatcherPriority.ApplicationIdle);
        }

        private void AttachFileWatcher(string fullPath)
        {
            if (fullPath == null) return;

            ReleaseFileWatcher();

            // no file watcher for root paths
            var di = new DirectoryInfo(fullPath);
            if (di.Root.FullName == fullPath)
            {
                AppModel.Window.ShowStatusProgress("Drive root selected: Files are not updated in root folders.",mmApp.Configuration.StatusMessageTimeout,spin: false, icon: FontAwesomeIcon.Circle);
                return;
            }

            if(FileWatcher != null)
                ReleaseFileWatcher();

            if (string.IsNullOrEmpty(fullPath))
                return;

            if (!Directory.Exists(fullPath))
            {
                FolderPath = null;
                return;
            }

            FileWatcher = new FileSystemWatcher(fullPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            FileWatcher.Created += FileWatcher_CreateOrDelete;
            FileWatcher.Deleted += FileWatcher_CreateOrDelete;
            FileWatcher.Renamed += FileWatcher_Renamed;
            FileWatcher.Changed += FileWatcher_Changed;
        }


        public void ReleaseFileWatcher()
        {
            if (FileWatcher != null)
            {
                FileWatcher.Changed -= FileWatcher_Changed;
                FileWatcher.Deleted -= FileWatcher_CreateOrDelete;
                FileWatcher.Created -= FileWatcher_CreateOrDelete;
                FileWatcher.Renamed -= FileWatcher_Renamed;

                // TODO: MAKE SURE THIS DOESN'T HAVE SIDE EFFECTS (Hanging on shutdown)
                // This can hang in weird ways when application is shutting down
                // Since this is the only time this should go away this is probably safe but ugly
                //FileWatcher.Dispose();

                FileWatcher = null;
            }
        }
        #endregion

        #region Folder Button and Text Handling


        /// <summary>
        /// Sets the tree's content from a folderOrFilePath or filename.
        ///
        /// This method is also called from the FolderPath property Getter
        /// after some pre-processing.
        /// </summary>
        /// <param name="folderOrFilePath">Folder or File path to load. If File folder is loaded and file selected</param>
        /// <param name="setFocus">Optional - determines on whether focus is set to the TreeView Item</param>
        /// <param name="searchText">Optional - search text filter that is applied to the file names</param>
        public void SetTreeFromFolder(string folderOrFilePath, bool setFocus = false, string searchText = null)
        {
            if (Window == null)
                return;

            string fileName = null;
            if (File.Exists(folderOrFilePath))
            {
                fileName = folderOrFilePath;
                folderOrFilePath = Path.GetDirectoryName(folderOrFilePath);
            }

            Window.ShowStatusProgress($"Retrieving files for folderOrFilePath {folderOrFilePath}...");

            Dispatcher.InvokeAsync(() =>
            {
                // just get the top level folderOrFilePath first
                ActivePathItem = null;
                WindowUtilities.DoEvents();

                var items = FolderStructure.GetFilesAndFolders(folderOrFilePath, nonRecursive: true, ignoredFolders: ".git");
                ActivePathItem = items;

                WindowUtilities.DoEvents();
                Window.ShowStatus();

                if (TreeFolderBrowser.HasItems)
                    SetTreeViewSelectionByIndex(0);

                if (setFocus)
                    TreeFolderBrowser.Focus();


                AttachFileWatcher(folderOrFilePath);

                FolderStructure.UpdateGitFileStatus(items);

                if (!string.IsNullOrEmpty(fileName))
                    SelectFileInSelectedFolderBrowserFolder(fileName);

            }, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Selects a file in the top level folder browser folder
        /// by file name.
        /// </summary>
        /// <param name="fileName">filename with full path - must match case</param>
        public void SelectFileInSelectedFolderBrowserFolder(string fileName, bool setFocus = true)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                foreach (var file in ActivePathItem.Files)
                {
                    if (file.FullPath == fileName)
                    {
                        if (setFocus)
                            TreeFolderBrowser.Focus();

                        file.IsSelected = true;
                    }
                }
            }

        }


        //private void ButtonUseCurrentFolder_Click(object sender, RoutedEventArgs e)
        //{
        //    var doc = AppModel?.ActiveDocument;
        //    if (doc == null)
        //        return;

        //    SetTreeFromFolder(doc.Filename, true);
        //}


        public void SetTreeViewSelectionByIndex(int index)
        {
            TreeViewItem item = TreeFolderBrowser
                .ItemContainerGenerator
                .ContainerFromIndex(index) as TreeViewItem;

            if (item != null)
                item.IsSelected = true;
        }

        public void SetTreeViewSelectionByItem(PathItem item, TreeViewItem parentTreeViewItem = null)
        {
            TreeViewItem treeitem = GetNestedTreeviewItem(item);

            if (treeitem != null)
            {
                treeitem.BringIntoView();

                if (treeitem.Parent != null && treeitem.Parent is TreeViewItem)
                    ((TreeViewItem) treeitem.Parent).IsExpanded = true;

                treeitem.IsSelected = true;

                // show edited filename with file stem selected
                if (item.IsEditing &&  item.IsFile &&
                    item.DisplayName != null && item.DisplayName.Contains('.'))
                {
                    Dispatcher.Invoke(() =>
                    {
                        var tb = WindowUtilities.FindVisualChild<TextBox>(treeitem);
                        if (tb == null)
                            return;
                        var idx = item.DisplayName.IndexOf(".");
                        if (idx > 0)
                        {
                            tb.SelectionStart = 0;
                            tb.SelectionLength = idx;
                        }
                    }, DispatcherPriority.ApplicationIdle);
                }
            }
        }

        /// <summary>
        /// Retrieves a nested TreeViewItem by walking the hierarchy.
        /// Specify a root treeview or treeviewitem and it then walks
        /// the hierarchy to find the item
        /// </summary>
        /// <param name="item">Item to find</param>
        /// <param name="treeItem">Parent item to start search from</param>
        /// <returns></returns>
        public TreeViewItem GetNestedTreeviewItem(object item, ItemsControl treeItem = null)
        {
            if (treeItem == null)
                treeItem = TreeFolderBrowser;

            return WindowUtilities.GetNestedTreeviewItem(item, treeItem);
        }

        private void ComboFolderPath_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter
            ) // || e.Key == Key.Tab && (Keyboard.Modifiers & ModifierKeys.Shift) != (ModifierKeys.Shift) )
            {
                ((ComboBox) sender).IsDropDownOpen = false;
                TreeFolderBrowser.Focus();
                e.Handled = true;
            }
        }

        private void ButtonRecentFolders_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonRecentFolders.ContextMenu == null)
                ButtonRecentFolders.ContextMenu = new ContextMenu();

            mmApp.Configuration.FolderBrowser.UpdateRecentFolderContextMenu(ButtonRecentFolders.ContextMenu);
            if (ButtonRecentFolders.ContextMenu.Items.Count > 0)
                ButtonRecentFolders.ContextMenu.IsOpen = true;

        }

        private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            string folder = FolderPath;

            if (string.IsNullOrEmpty(folder))
            {
                folder = AppModel.ActiveDocument?.Filename;
                if (string.IsNullOrEmpty(folder))
                    folder = Path.GetDirectoryName(folder);
                else
                    folder = KnownFolders.GetPath(KnownFolder.Libraries);
            }

            folder = mmWindowsUtils.ShowFolderDialog(folder, "Select folderOrFilePath to open in the Folder Browser");
            if (folder == null)
                return;

            FolderPath = folder;
            TreeFolderBrowser.Focus();
        }

        private void ButtonRefreshFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ActivePathItem != null)
            {
                ActivePathItem.Files.Clear();
                ActivePathItem.FullPath = FolderPath;
                ActivePathItem.IsFolder = true;
            }

            SetTreeFromFolder(FolderPath, true);
        }

        #endregion


#region TreeView Selection Handling

public PathItem GetSelectedPathItem()
{
    return TreeFolderBrowser.SelectedItem as PathItem;
}

private string searchFilter = string.Empty;
        private DateTime searchFilterLast = DateTime.MinValue;


        private void TreeView_Keydown(object sender, KeyEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;

            // this works without a selection
            if (e.Key == Key.F2 && Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (selected == null || !selected.IsEditing)
                {
                    FolderBrowserContextMenu.MenuAddFile_Click(sender, null);
                    e.Handled = true;
                }
                return;
            }

            if (e.Key == Key.F8)
            {
                if (selected == null || !selected.IsEditing)
                {
                    FolderBrowserContextMenu.MenuAddDirectory_Click(sender, null);
                    e.Handled = true;
                }
            }

            if (selected == null)
                return;

            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (!selected.IsEditing)
                    HandleItemSelection(forceEditorFocus: true);
                else
                    RenameOrCreateFileOrFolder();
                

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (selected.IsEditing)
                {
                    selected.IsEditing = false;
                    if (!string.IsNullOrEmpty(selected.OriginalRenamePath))
                    {
                        selected.FullPath = selected.OriginalRenamePath;
                        selected.OriginalRenamePath = null;
                    }
                    else
                        selected.Parent?.Files?.Remove(selected);
                }
            }
            else if (e.Key == Key.F2)
            {
                if (!selected.IsEditing)
                    FolderBrowserContextMenu.MenuRenameFile_Click(sender, null);
            }
            else if (e.Key == Key.Delete)
            {
                if (!selected.IsEditing)
                    FolderBrowserContextMenu.MenuDeleteFile_Click(sender, null);
            }
            else if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!selected.IsEditing)
                {
                    FolderBrowserContextMenu.MenuCommitGit_Click(null, null);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!selected.IsEditing)
                {
                    FolderBrowserContextMenu.MenuUndoGit_Click(null, null);
                    e.Handled = true;
                }
            }

            if (selected.IsEditing)
                return;

            // search key
            if (e.Key >= Key.A && e.Key <= Key.Z ||
                e.Key >= Key.D0 && e.Key <= Key.D9 ||
                e.Key == Key.OemPeriod ||
                e.Key == Key.Space ||
                e.Key == Key.Separator ||
                e.Key == Key.OemMinus &&
                (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt)))
            {
                //Debug.WriteLine("Treeview TreeDown: " + e.Key + " shfit: " + Keyboard.IsKeyDown(Key.LeftShift));
                var keyConverter = new KeyConverter();

                string k;

                if (e.Key == Key.OemPeriod)
                    k = ".";
                else if (e.Key == Key.OemMinus && Keyboard.IsKeyDown(Key.LeftShift))
                    k = "_";
                else if (e.Key == Key.OemMinus)
                    k = "-";
                else if (e.Key == Key.Space)
                    k = " ";
                else
                    k = keyConverter.ConvertToString(e.Key);

                if (searchFilterLast > DateTime.Now.AddSeconds(-1.2))
                    searchFilter += k.ToLower();
                else
                    searchFilter = k.ToLower();

                Window.ShowStatus("File search filter: " + searchFilter, 2000);

                var lowerFilter = searchFilter.ToLower();

                var parentPath = selected.Parent;
                if (parentPath == null)
                    parentPath = ActivePathItem; // root

                var item = parentPath.Files.FirstOrDefault(sf => sf.DisplayName.ToLower().StartsWith(lowerFilter));
                if (item != null)
                    item.IsSelected = true;


                searchFilterLast = DateTime.Now;
            }

        }

        private void FolderBrowserGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;

            if (e.Key == Key.F1)
            {
                AppModel.Commands.HelpCommand.Execute("_4xs10gaui.htm");
                e.Handled = true;
            }
            else if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (SearchPanel.Visibility == Visibility.Collapsed)
                {
                    SearchPanel.Visibility = Visibility.Visible;
                    TextSearch.Focus();
                }
                else
                {
                    SearchPanel.Visibility = Visibility.Collapsed;
                    TextSearch.Text = string.Empty;
                }

                e.Handled = true;
            }
             // Copy File to 'clipboard'
            else if ((e.Key == Key.C || e.Key == Key.X) && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var menu = new FolderBrowserContextMenu(this);
                menu.FileBrowserCopyFile( e.Key == Key.X);
                e.Handled = true;
            }
            // Paste Files(s) from clipboard
            else if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                var menu = new FolderBrowserContextMenu(this);
                menu.FileBrowserPasteFile();
                e.Handled = true;
            }


        }

        private void TreeViewItem_MouseDownClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // double-click
            {
                LastClickTime = DateTime.MinValue;
                HandleItemSelection(forceEditorFocus:true);
            }
        }

        private void TreeViewItem_MouseUpClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                return;

            // single click - image preview  MUST BE ON MOUSEUP so we can still drag
            dynamic s = sender;
            var selected = s.DataContext as PathItem;

            if (selected == null)
                return;

            var filePath = selected.FullPath;

            if (string.IsNullOrEmpty(filePath))
                return;

            var ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
            {
                Window.OpenBrowserTab(filePath, isImageFile: true);
                return;
            }


            var tab = AppModel.Window.GetTabFromFilename(filePath);
            if(tab != null)
            {
                AppModel.Window.TabControl.SelectedItem = tab;
                return;
            }

            if (ext == ".md" || ext == ".markdown")
                Window.RefreshTabFromFile(filePath, isPreview: true, noFocus: true);
            else if (ext == ".html" || ext == ".htm")
                Window.OpenBrowserTab(filePath);


        }

        private void TreeFolderBrowser_Expanded(object sender, RoutedEventArgs e)
        {
            var tvi = e.OriginalSource as TreeViewItem;
            if (tvi == null)
                return;

            tvi.IsSelected = true;

            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null || selected.IsFile || selected.FullPath == "..")
                return;

            if (selected.Files != null && selected.Files.Count == 1 && selected.Files[0] == PathItem.Empty)
            {
                var subfolder = FolderStructure.GetFilesAndFolders(selected.FullPath, nonRecursive: true, parentPathItem: selected);
                FolderStructure.UpdateGitFileStatus(subfolder);
            }
        }

        private void Files_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            return;
        }

        private void TreeViewItem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = ElementHelper.FindVisualTreeParent<TreeViewItem>(e.OriginalSource as FrameworkElement);
            if (item != null)
                item.IsSelected = true;
        }



        public void HandleItemSelection(bool forceEditorFocus = false)
        {
            var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
            if (fileItem == null)
                return;

            if (fileItem.FullPath == "..")
                FolderPath = Path.GetDirectoryName(FolderPath.Trim('\\'));
            else if (fileItem.IsFolder)
                FolderPath = fileItem.FullPath;
            else
                OpenFile(fileItem.FullPath, forceEditorFocus);
        }

        void RenameOrCreateFileOrFolder()
        {
            var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
            if (fileItem == null)
                return;

            Fix if (string.IsNullOrEmpty(fileItem?.EditName) || fileItem.DisplayName == fileItem.EditName)
            {
                fileItem.IsEditing = false;
                return;
            }

            string oldFile = fileItem.FullPath;
            string oldPath = Path.GetDirectoryName(fileItem.FullPath);
            string newPath = Path.Combine(oldPath, fileItem.EditName);

            if (fileItem.IsFolder)
            {
                try
                {
                    if (Directory.Exists(fileItem.FullPath))
                        Directory.Move(fileItem.FullPath, newPath);
                    else
                    {
                        if (Directory.Exists(newPath))
                        {
                            AppModel.Window.ShowStatusError($"Can't create folderOrFilePath {newPath} because it exists already.");
                            return;
                        }

                        fileItem.IsEditing = false;
                        var parent = fileItem.Parent;
                        parent.Files.Remove(fileItem);

                        fileItem.FullPath = newPath;
                        FolderStructure.InsertPathItemInOrder(fileItem, parent);


                       

                        Dispatcher.Invoke(() => {
                            Directory.CreateDirectory(newPath);
                            fileItem.UpdateGitFileStatus();
                        },DispatcherPriority.ApplicationIdle);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to rename or create folderOrFilePath:\r\n" +
                                    newPath, "Path Creation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                try
                {
                    if (File.Exists(fileItem.FullPath))
                    {
                        if (!File.Exists(newPath))
                            File.Move(fileItem.FullPath, newPath);
                        else
                            File.Copy(fileItem.FullPath, newPath, true);
                    }
                    else
                    {
                        if (File.Exists(newPath))
                        {
                            AppModel.Window.ShowStatusError($"Can't create file {newPath} because it exists already.");
                            return;
                        }

                        fileItem.IsEditing = false;
                        fileItem.FullPath = newPath; // force assignment so file watcher doesn't add another

                        File.WriteAllText(newPath, "");
                        fileItem.UpdateGitFileStatus();


                        var parent = fileItem.Parent;
                        fileItem.Parent.Files.Remove(fileItem);

                        FolderStructure.InsertPathItemInOrder(fileItem, parent);
                    }

                    // If tab was open - close it and re-open new file
                    var tab = Window.GetTabFromFilename(oldFile);
                    if (tab != null)
                    {
                        Window.CloseTab(oldFile);
                        WindowUtilities.DoEvents();
                        Window.OpenFile(newPath, isPreview: true) ;
                        WindowUtilities.DoEvents();
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to rename or create file:\r\n" +
                                    newPath + "\r\n" + ex.Message, "File Creation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            fileItem.FullPath = newPath;
            fileItem.IsEditing = false;
        }

        public void OpenFile(string file, bool forceEditorFocus = false)
        {
            Window.OpenFile(file, noFocus: !forceEditorFocus );
        }

#endregion

#region Search Textbox

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        private void TextSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            debounceTimer.Debounce(500, (p) =>
            {
                Window.ShowStatusProgress("Filtering files...");
                WindowUtilities.DoEvents();
                FolderStructure.SetSearchVisibility(SearchText, ActivePathItem, SearchSubTrees);
                Window.ShowStatus(null);
            });


        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Window.ShowStatusProgress("Filtering files...");
            WindowUtilities.DoEvents();
            FolderStructure.SetSearchVisibility(SearchText, ActivePathItem, SearchSubTrees);
            Window.ShowStatus(null);
        }

        private void Button_CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            TreeFolderBrowser.Focus();
        }

        internal void MenuFindFiles_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Visible;
            TextSearch.Focus();
        }

#endregion

#region Context Menu Actions

        private void TreeFolderBrowser_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FolderBrowserContextMenu.ShowContextMenu();
        }


#endregion

#region Items and Item Selection

        private DateTime LastClickTime;
        private PathItem LastItem;

        /// <summary>
        /// Handle edit click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextFileOrFolderName_MouseUpToEdit(object sender, MouseButtonEventArgs e)
        {
            // only
            if (e.ChangedButton == MouseButton.Left)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;
                var t = DateTime.Now;

                if (LastItem == selected)
                {
                    if (t >= LastClickTime.AddMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime + 200) &&
                        t <= LastClickTime.AddMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime * 2 + 200))
                    {
                        FolderBrowserContextMenu.MenuRenameFile_Click(null, null);
                    }
                }

                LastItem = selected;
                LastClickTime = t;
            }
        }


        private string OrigEditValue;

        /// <summary>
        /// Special intercepts for New File and Folder handling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextEditFileItem_LostFocus(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected != null)
            {
                
                if (selected.DisplayName == "NewFile.md" || selected.DisplayName == "NewFolder")
                {
                    selected.Parent.Files.Remove(selected);
                    return;
                }

                if (selected.IsEditing) // this should be handled by Key ops in treeview
                {
                    RenameOrCreateFileOrFolder();
                }

                selected.IsEditing = false;
                selected.SetIcon();
            }
        }

        /// <summary>
        /// Handle Text Selection for the filename only
        /// </summary>
        private void TextEditFileItem_GotFocus(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected != null)
            {
                var tb = sender as TextBox;
                if (tb == null)
                    return;

                if(!selected.DisplayName.Contains('.') && tb.SelectionLength > 0)  // already selected
                    return;

                var at = selected.DisplayName.IndexOf('.');
                tb.SelectionStart = 0;
                tb.SelectionLength = at > 1 ? at : selected.DisplayName.Length;
            }
        }

#endregion

#region Drag Operations

        private System.Windows.Point startPoint;

        private void TreeFolderBrowser_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Window.PreviewTab != null)
            {
                var filename = (Window.PreviewTab.Tag as MarkdownDocumentEditor)?.MarkdownDocument?.Filename;
                if (filename != null)
                {
                    var ext = Path.GetExtension(filename)?.ToLower();
                    if (ext == ".jpg" || ext == ".png" || ext == ".gif" || ext == ".jpeg")
                        Window.CloseTab(Window.PreviewTab);
                }
            }

            startPoint = e.GetPosition(null);
        }


        private void TreeFolderBrowser_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var selected = TreeFolderBrowser.SelectedItem as PathItem;

                // Only allow the items to be dragged
                var src = e.OriginalSource as TextBlock;
                if (src == null)
                    return;

                // only drag image files
                if (selected == null || selected.IsFolder)
                    return;

                var mousePos = e.GetPosition(null);
                var diff = startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var treeView = sender as TreeView;
                    var treeViewItem = WindowUtilities.FindAnchestor<TreeViewItem>((DependencyObject) e.OriginalSource) ;
                    if (treeView == null || treeViewItem == null)
                        return;

                    var files = new[] {selected.FullPath};
                    var dragData = new DataObject(DataFormats.FileDrop, files);

                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Copy);
                }
            }
        }

        private void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            PathItem targetItem = ActivePathItem;


            var formats = e.Data.GetFormats();

            if (sender is TreeView)
            {
                // dropped into treeview open space
            }
            else
            {
                targetItem = (e.OriginalSource as FrameworkElement)?.DataContext as PathItem;
                if (targetItem == null)
                    return;
            }
            e.Handled = true;


            if (formats.Contains("FileDrop"))
            {
                HandleDroppedFiles(e.Data.GetData("FileDrop") as string[]);
                return;
            }


            if (!targetItem.IsFolder)
                targetItem = targetItem.Parent;

            var path = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(path))
            {
               return;
            }

            string newPath;
            var sourceItem = FolderStructure.FindPathItemByFilename(ActivePathItem, path);
            if (sourceItem == null)
            {
                // Handle dropped new files (from Explorer perhaps)
                if (File.Exists(path))
                {
                    newPath = Path.Combine(targetItem.FullPath, Path.GetFileName(path));
                    File.Copy(path, newPath);
                    AppModel.Window.ShowStatusSuccess($"File copied.");
                }

                return;
            }

            newPath = Path.Combine(targetItem.FullPath, sourceItem.DisplayName);

            if (sourceItem.FullPath.Equals(newPath, StringComparison.InvariantCultureIgnoreCase))
            {
                AppModel.Window.ShowStatusError($"File not moved.",
                    mmApp.Configuration.StatusMessageTimeout);
                return;
            }

            try
            {
                File.Move(sourceItem.FullPath, newPath);
            }
            catch (Exception ex)
            {
                AppModel.Window.ShowStatusError($"Couldn't move file: {ex.Message}",
                    mmApp.Configuration.StatusMessageTimeout);
                return;
            }
            targetItem.IsExpanded = true;

            // wait for file watcher to pick up the file
            Dispatcher.Delay(200,(p) =>
            {
                var srceItem = FolderStructure.FindPathItemByFilename(ActivePathItem, p as string);
                if (srceItem == null)
                    return;
                srceItem.IsSelected = true;
            },newPath);

            AppModel.Window.ShowStatus($"File moved to: {newPath}",mmApp.Configuration.StatusMessageTimeout);
        }

        /// <summary>
        /// Handles files that were dropped on the tree view
        /// </summary>
        /// <param name="files">array of files</param>
        void HandleDroppedFiles(string[] files)
        {
            if (files == null)
                return;

            Window.ShowStatusProgress("Copying files and folders...");
            WindowUtilities.DoEvents();

            string errors = "";

            Task.Run(() =>
            {
                foreach (var file in files)
                {
                    var isFile = File.Exists(file);
                    var isDir = Directory.Exists(file);
                    if (!isDir && !isFile)
                        continue;

                    string nPath;
                    if (isFile)
                    {
                        nPath = Path.Combine(ActivePathItem.FullPath, Path.GetFileName(file));
                        if (!LanguageUtils.IgnoreErrors(() => File.Copy(file, nPath, overwrite: true)))
                            errors += $"{nPath},";
                    }
                    else
                    {
                        nPath = Path.Combine(ActivePathItem.FullPath, Path.GetFileName(file));
                        if (!LanguageUtils.IgnoreErrors(() => FileUtils.CopyDirectory(file, nPath)))
                            errors += $"{nPath},";
                    }
                }
                Dispatcher.InvokeAsync(() =>
                {
                    if (string.IsNullOrEmpty(errors))
                        Window.ShowStatusSuccess($"Files and Folders copied.");
                    else
                        Window.ShowStatusError($"There were errors copying files and folders.");
                });
            });
        }
#endregion

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
