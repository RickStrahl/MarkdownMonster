using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.Annotations;
using MarkdownMonster.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using Westwind.Utilities;


namespace MarkdownMonster.Windows
{
    /// <summary>
    /// Interaction logic for FolderBrowerSidebar.xaml
    /// </summary>
    public partial class FolderBrowerSidebar : UserControl, INotifyPropertyChanged
    {

        public MainWindow Window { get; set; }
        public AppModel AppModel { get; set; }


        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                if (value == _folderPath) return;

                SearchText = null;
                SearchSubTrees = false;
                SearchPanel.Visibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(value))
                    ActivePathItem = new PathItem();
                else if (value != _folderPath)
                    SetTreeFromFolder(value, _folderPath != null, SearchText);

                _folderPath = value;
                mmApp.Configuration.FolderBrowser.AddRecentFolder(_folderPath);
                
                OnPropertyChanged(nameof(FolderPath));
                OnPropertyChanged(nameof(ActivePathItem));
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

        public string SelectedFile
        {
            get { return (string) GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register("SelectedFile", typeof(string), typeof(FolderBrowerSidebar),
                new PropertyMetadata(null));

        /// <summary>
        /// Internal value
        /// </summary>
        private FolderStructure FolderStructure { get; } = new FolderStructure();
        public object WindowUtilties { get; private set; }

        private FileSystemWatcher FileWatcher = null;


        #region Initialization

        public FolderBrowerSidebar()
        {

            InitializeComponent();
            Focusable = true;
            DataContext = this;
            Loaded += FolderBrowerSidebar_Loaded;           
        }



        private void FolderBrowerSidebar_Loaded(object sender, RoutedEventArgs e)
        {
            AppModel = mmApp.Model;
            Window = AppModel.Window;
            
            // Load explicitly here to fire *after* behavior has attached
            ComboFolderPath.PreviewKeyUp += ComboFolderPath_PreviewKeyDown;
        }
        #endregion

        #region FileWatcher

        private void FileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine("Rename: " + e.FullPath + " from " + e.OldFullPath);
            var file = e.FullPath;
            var oldFile = e.OldFullPath;

            var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, oldFile);
            if (pi == null)
                return;

            pi.FullPath = file;
            Dispatcher.Invoke(() => pi.Parent.Files.Remove(pi));

            FolderStructure.InsertPathItemInOrder(pi, pi.Parent);
        }

        private void FileWatcher_CreateOrDelete(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Create Or Delete: " + e.FullPath + " " + e.ChangeType);
            var file = e.FullPath;
            
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                // TODO:  this is not working
                
                mmApp.Model.Window.Dispatcher.Invoke(() =>
                {
                    var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, file);
                    if (pi == null)
                        return;
                    
                    pi.Parent.Files.Remove(pi);

                    Debug.WriteLine("After: " + pi.Parent.Files.Count + " " + file);
                },DispatcherPriority.ApplicationIdle);
            }

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                var pi = FolderStructure.FindPathItemByFilename(ActivePathItem, file);
                if (pi != null) // Already exists in the tree
                    return;

                // does the path exist?
                var parentPathItem =
                    FolderStructure.FindPathItemByFilename(ActivePathItem, Path.GetDirectoryName(file));
                if (parentPathItem == null) // path is not expanced yet
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
            }

        }

        private void AttachFileWatcher(string fullPath)
        {
            if(FileWatcher != null)
                ReleaseFileWatcher();

            FileWatcher = new FileSystemWatcher(fullPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            FileWatcher.Created += FileWatcher_CreateOrDelete;
            FileWatcher.Deleted += FileWatcher_CreateOrDelete;
            FileWatcher.Renamed += FileWatcher_Renamed;
        }

        private void ReleaseFileWatcher()
        {
            if (FileWatcher != null)
            {
                FileWatcher.Created -= FileWatcher_CreateOrDelete;
                FileWatcher.Deleted -= FileWatcher_CreateOrDelete;
                FileWatcher.Renamed -= FileWatcher_Renamed;
                FileWatcher.Dispose();
            }
        }
        #endregion

        #region Folder Button and Text Handling

        private void SetTreeFromFolder(string folder, bool setFocus = false, string searchText = null)
        {
            Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Spinner, Colors.Orange, true);
            Window.ShowStatus($"Retrieving files for folder {folder}...");

            Dispatcher.InvokeAsync(() =>
            {
                // just get the top level folder first
                ActivePathItem = null;
                WindowUtilities.DoEvents();

                var items = FolderStructure.GetFilesAndFolders(folder, nonRecursive: false, ignoredFolders: ".git");
                ActivePathItem = items;
                
                WindowUtilities.DoEvents();
                Window.ShowStatus();

                if (TreeFolderBrowser.HasItems)
                    SetTreeViewSelectionByIndex(0);

                if (setFocus)
                    TreeFolderBrowser.Focus();

                AttachFileWatcher(folder);

            }, DispatcherPriority.ApplicationIdle);
        }

        private void ButtonUseCurrentFolder_Click(object sender, RoutedEventArgs e)
        {
            var doc = AppModel.ActiveDocument;
            if (doc != null)
                FolderPath = Path.GetDirectoryName(doc.Filename);

            SetTreeFromFolder(FolderPath, true);
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

            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select folder to open in the Folder Browser";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = folder;
            dlg.RestoreDirectory = true;
            dlg.ShowHiddenItems = true;
            dlg.ShowPlacesList = true;
            dlg.EnsurePathExists = true;

            var result = dlg.ShowDialog();

            if (result != CommonFileDialogResult.Ok)
                return;

            FolderPath = dlg.FileName;

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

        private void ButtonClosePanel_Click(object sender, RoutedEventArgs e)
        {
            Window.ShowFolderBrowser(hide: true);
            AppModel.ActiveEditor?.SetEditorFocus();
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

        private void SetTreeViewSelectionByIndex(int index)
        {
            TreeViewItem item = (TreeViewItem) TreeFolderBrowser
                .ItemContainerGenerator
                .ContainerFromIndex(index);
            item.IsSelected = true;
        }

        private void SetTreeViewSelectionByItem(PathItem item)
        {
            TreeViewItem treeitem = GetTreeviewItem(item);
            if (treeitem != null)
            {
                treeitem.BringIntoView();

                if (treeitem.Parent != null && treeitem.Parent is TreeViewItem)
                    ((TreeViewItem) treeitem.Parent).IsExpanded = true;

                treeitem.IsSelected = true;
            }
        }

        private TreeViewItem GetTreeviewItem(object item)
        {
            return (TreeViewItem) TreeFolderBrowser
                .ItemContainerGenerator
                .ContainerFromItem(item);
        }

        #endregion


        #region TreeView Selection Handling

        private string searchFilter = string.Empty;
        private DateTime searchFilterLast = DateTime.MinValue;


        private void TreeView_Keydown(object sender, KeyEventArgs e)
        {            
            var selected = TreeFolderBrowser.SelectedItem as PathItem;

            // this works without a selection
            if (e.Key == Key.N && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (selected == null || !selected.IsEditing)
                {
                    MenuAddFile_Click(sender, null);
                    e.Handled = true;
                }
                return;
            }

            if (selected == null)
                return;
            
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (!selected.IsEditing)
                    HandleItemSelection();
                else
                    RenameOrCreateFileOrFolder();
                e.Handled = true;                
            }
            else if (e.Key == Key.Escape)
            {
                if (selected.IsEditing)
                    selected.IsEditing = false;
            }
            else if (e.Key == Key.F2)
            {
                if (!selected.IsEditing)
                    MenuRenameFile_Click(sender, null);
            }
            else if (e.Key == Key.Delete)
            {
                if (!selected.IsEditing)
                    MenuDeleteFile_Click(sender, null);
            }            
            else if (e.Key == Key.G && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (!selected.IsEditing)
                {
                    MenuCommitGit_Click(null, null);
                    e.Handled = true;
                }
            }

            

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
            }
            
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                LastClickTime = DateTime.MinValue;
                HandleItemSelection();
            }
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

        void HandleItemSelection()
        {
            var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
            if (fileItem == null)
                return;

            if (fileItem.FullPath == "..")
                FolderPath = Path.GetDirectoryName(FolderPath.Trim('\\'));
            else if (fileItem.IsFolder)
                FolderPath = fileItem.FullPath;
            else
                OpenFile(fileItem.FullPath);
        }

        void RenameOrCreateFileOrFolder        ()
        {
            var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
            if (string.IsNullOrEmpty(fileItem?.EditName) )
                return;
                       
            string oldFilename = Path.GetFileName(fileItem.FullPath);
            string oldPath = Path.GetDirectoryName(fileItem.FullPath);
            string newPath = Path.Combine(oldPath, fileItem.EditName);

            if (newPath != fileItem.FullPath)
            {
                if (fileItem.IsFolder)
                {
                    try
                    {
                        if (Directory.Exists(fileItem.FullPath))
                            Directory.Move(fileItem.FullPath, newPath);
                        else
                            Directory.CreateDirectory(newPath);
                    }
                    catch
                    {
                        MessageBox.Show("Unable to rename or create folder:\r\n" +
                                        newPath, "Path Creation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    try
                    {
                        if (File.Exists(fileItem.FullPath))
                        {
                            if(!File.Exists(newPath))
                                File.Move(fileItem.FullPath, newPath);
                            else
                                File.Copy(fileItem.FullPath, newPath,true);
                        }
                        else
                            File.WriteAllText(newPath, "");

                        // check if tab was open and if so rename the tab
                        var tab = Window.GetTabFromFilename(fileItem.FullPath);
                        if (tab != null)
                        {
                            Window.CloseTab(fileItem.FullPath);
                            WindowUtilities.DoEvents();
                            Window.OpenTab(newPath);
                            WindowUtilities.DoEvents();

                            //var doc = (MarkdownDocumentEditor) tab.Tag;
                            //doc.MarkdownDocument.Load(newPath);
                            //tab.Tag = doc;
                            //Window.BindTabHeaders();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Unable to rename or create file:\r\n" +
                                        newPath, "File Creation Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    // Open the document
                    // HandleItemSelection();
                }
            }

            fileItem.FullPath = newPath;
            fileItem.IsEditing = false;
        }

        public void OpenFile(string file)
        {
            string format = mmFileUtils.GetEditorSyntaxFromFileType(file);
            if (!string.IsNullOrEmpty(format))
            {
                Window.OpenTab(file, rebindTabHeaders: true);
                return;
            }

            var ext = Path.GetExtension(file).ToLower().Replace(".", "");
            if (StringUtils.Inlist(ext, "jpg", "png", "gif", "jpeg"))
            {
                if (!mmFileUtils.OpenImageInImageViewer(file))
                {
                    MessageBox.Show("Unable to launch image viewer " +
                                    Path.GetFileName(mmApp.Configuration.ImageViewer) +
                                    "\r\n\r\n" +
                                    "Most likely the image viewer configured in settings is not valid. Please check the 'ImageEditor' key in the Markdown Monster Settings." +
                                    "\r\n\r\n" +
                                    "We're opening the Settings file for you in the editor now.",
                        "Image Launching Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    Window.OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json"));
                }
            }
            else
            {
                try
                {
                    ShellUtils.GoUrl(file);
                }
                catch
                {
                    Window.ShowStatus("Unable to open file " + file, 4000);
                    Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);

                    if (MessageBox.Show(
                            "Unable to open this file. Do you want to open it as a text document in the editor?",
                            mmApp.ApplicationName,
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        Window.OpenTab(file, rebindTabHeaders: true);
                }
            }
        }

        #endregion

        #region Search Textbox        

        private DebounceDispatcher debounceTimer = new DebounceDispatcher();

        private void TextSearch_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            debounceTimer.Debounce(500, (p) =>
            {
                Window.ShowStatus("Filtering files...");
                Window.SetStatusIcon(FontAwesomeIcon.Spinner, Colors.Orange, spin: true);
                WindowUtilities.DoEvents();
                FolderStructure.SetSearchVisibility(SearchText, ActivePathItem, SearchSubTrees);
                Window.ShowStatus(null);
            });


        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Window.ShowStatus("Filtering files...");
            Window.SetStatusIcon(FontAwesomeIcon.Spinner, Colors.Orange, spin: true);
            WindowUtilities.DoEvents();
            FolderStructure.SetSearchVisibility(SearchText, ActivePathItem, SearchSubTrees);
            Window.ShowStatus(null);
        }

        private void Button_CloseSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            TreeFolderBrowser.Focus();
        }

        private void MenuFindFiles_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Visible;
            TextSearch.Focus();
        }

        #endregion

        #region Context Menu Actions

        private void MenuDeleteFile_Click(object sender, RoutedEventArgs e)
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

                var file = parent?.Files?.FirstOrDefault(fl => fl.FullPath == selected.FullPath);
                if (file != null)
                {
                    var tab = Window.GetTabFromFilename(file.FullPath);
                    if (tab != null)
                        Window.CloseTab(tab,dontPromptForSave:true);

                    selected.Parent?.Files.Remove(file);
                }

                // Delay required to overcome editor focus after MsgBox
                Dispatcher.Delay(700, s =>
                {
                    TreeFolderBrowser.Focus();
                    SetTreeViewSelectionByItem(parent);
                    TreeFolderBrowser.Focus();
                });
            }
            catch (Exception ex)
            {
                Window.ShowStatus("Delete operation failed: " + ex.Message, 6000);
                Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
            }
        }

        private void MenuAddFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
            {
                // No files/folders
                selected = new PathItem()
                {
                    IsFolder = true,
                    FullPath = FolderPath
                };
                ActivePathItem = selected;
            }

            string path;            
            if (!selected.IsFolder || selected.FullPath == "..")
                path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFile.md");
            else
            {
                var treeItem = GetTreeviewItem(selected);
                if (treeItem != null)
                    treeItem.IsExpanded = true;

                path = Path.Combine(selected.FullPath, "NewFile.md");
            }

            var item = new PathItem
            {
                FullPath = path,
                IsFolder = false,
                IsEditing = true,
                IsSelected = true
            };
            item.SetIcon();

            if (selected.FullPath == "..")
                item.Parent = ActivePathItem; // current path
            else if (!selected.IsFolder)
                item.Parent = selected.Parent;
            else
                item.Parent = selected;

            item.Parent.Files.Insert(0, item);
            SetTreeViewSelectionByItem(item);
        }


        private void MenuAddDirectory_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null || selected.Parent == null)
            {
                // No files/folders
                selected = new PathItem()
                {
                    IsFolder = true,
                    FullPath = FolderPath
                };
                ActivePathItem = selected;
            }

            string path;
            if (!selected.IsFolder)
                path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFolder");
            else
            {
                var treeItem = GetTreeviewItem(selected);
                if (treeItem != null)
                    treeItem.IsExpanded = true;

                path = Path.Combine(selected.FullPath, "NewFolder");
            }

            var item = new PathItem
            {
                FullPath = path,
                IsFolder = true,
                IsEditing = true,
                IsSelected = true
            };
            item.SetIcon();

            if (!selected.IsFolder)
                item.Parent = selected.Parent;
            else
                item.Parent = selected;

            item.Parent.Files.Insert(0, item);

            SetTreeViewSelectionByItem(item);
        }


        private void MenuRenameFile_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            // Start Editing the file name
            selected.EditName = selected.DisplayName;
            selected.IsEditing = true;


            var tvItem = GetTreeviewItem(selected);
            if (tvItem != null)
            {
                var tb = WindowUtilities.FindVisualChild<TextBox>(tvItem);
                tb?.Focus();
            }
        }


        private async void MenuCommitGit_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            var model = mmApp.Model;

            string file = selected.FullPath;

            if (string.IsNullOrEmpty(file))
                return;

            model.Window.ShowStatus("Committing and pushing to Git...");
            WindowUtilities.DoEvents();

            string error = null;

            bool pushToGit = mmApp.Configuration.GitCommitBehavior == GitCommitBehaviors.CommitAndPush;
            bool result = await Task.Run(() => mmFileUtils.CommitFileToGit(file, pushToGit, out error));

            if (result)
                model.Window.ShowStatus($"File {Path.GetFileName(file)} committed and pushed.", 6000);
            else
            {
                model.Window.ShowStatus(error, 7000);
                model.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
            }
        }

        private void TreeFolderBrowser_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var tv = sender as TreeView;
            if (tv == null)
                return;
            var cm = tv.ContextMenu;

            var pathItem = TreeFolderBrowser.SelectedItem as PathItem;
            if (pathItem == null)
                return;

            cm.Items.Clear();

            var ci = new MenuItem();
            ci.Header = "_New File";
            ci.InputGestureText = "ctrl-n";        
            ci.Click += MenuAddFile_Click;
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "New Folder";        
            ci.Click += MenuAddDirectory_Click;
            cm.Items.Add(ci);

            cm.Items.Add(new Separator());

            ci = new MenuItem();

            ci.Header = "Delete";
            ci.InputGestureText = "Del";            
            ci.Click += MenuDeleteFile_Click;
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "Rename";
            ci.InputGestureText = "F2";
            ci.Click += MenuRenameFile_Click;
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "Find Files";
            ci.InputGestureText = "ctrl-f";        
            ci.Click += MenuFindFiles_Click;
            cm.Items.Add(ci);

            cm.Items.Add(new Separator());

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
                ci.Header = "Open with Shell";            
                ci.Click += MenuOpenWithShell_Click;
                cm.Items.Add(ci);
            }

            cm.Items.Add(new Separator());

            ci = new MenuItem();            
            ci.Header = "Open Folder in Terminal";            
            ci.Click += MenuOpenTerminal_Click;
            cm.Items.Add(ci);

            ci = new MenuItem();
            ci.Header = "Open Folder in Explorer";
            ci.Click += MenuOpenInExplorer_Click;
            cm.Items.Add(ci);

            cm.Items.Add(new Separator());

            ci = new MenuItem();
            ci.Header = "Commit File to _Git and Push";
            ci.InputGestureText = "ctrl-g";        
            ci.Click += MenuCommitGit_Click;
            cm.Items.Add(ci);

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

        }

        #endregion


        #region Shell/Terminal Operations

        private void MenuOpenInExplorer_Click(object sender, RoutedEventArgs e)
        {
            string folder = FolderPath;

            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected != null)
                folder = selected.FullPath; // Path.GetDirectoryName(selected.FullPath);

            if (string.IsNullOrEmpty(folder))
                return;

            if (selected == null || selected.IsFolder)
                ShellUtils.GoUrl(folder);
            else
                mmFileUtils.OpenFileInExplorer(folder);
        }

        private void MenuOpenFolderBrowserHere_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            if (selected.FullPath == "..")
                FolderPath = Path.GetDirectoryName(FolderPath.TrimEnd('\\'));
            else
            {
                if (Directory.Exists(FolderPath))
                    FolderPath = selected.FullPath;
                else
                    FolderPath = Path.GetDirectoryName(FolderPath);
            }
              
        }

        private void MenuOpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            string folder = FolderPath;

            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected != null)
                folder = Path.GetDirectoryName(selected.FullPath);

            if (string.IsNullOrEmpty(folder))
                return;

            mmFileUtils.OpenTerminal(folder);
        }

        private void MenuOpenWithShell_Click(object sender, RoutedEventArgs e)
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
                Window.ShowStatus("Unable to open file " + selected.FullPath, 4000);
                Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
            }
        }

        private void MenuCopyPathToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;

            string clipText = null;
            if (selected == null)
                clipText = FolderPath;
            else if (selected.FullPath == "..")
                clipText = Path.GetDirectoryName(FolderPath);
            else
                clipText = selected.FullPath;

            if (!string.IsNullOrEmpty(clipText))
            {
                System.Windows.Clipboard.SetText(clipText);
                Window.ShowStatus($"Path '{clipText}' has been copied to the Clipboard.", 6000);
            }

        }


        private void MenuOpenInEditor_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            if (selected.IsFolder)
                ShellUtils.GoUrl(selected.FullPath);
            else
                Window.OpenTab(selected.FullPath, rebindTabHeaders: true);
        }

        private void MenuShowImage_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            mmFileUtils.OpenImageInImageViewer(selected.FullPath);
        }

        private void MenuEditImage_Click(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected == null)
                return;

            mmFileUtils.OpenImageInImageEditor(selected.FullPath);
        }

        #endregion

        #region Items and Item Selection

        private string overImage;

        private void TextFileOrFolderName_MouseEnter(object sender, MouseEventArgs e)
        {
            dynamic s = sender as dynamic;

            var selected = s.DataContext as PathItem;
            if (selected == null)
                return;

            overImage = selected.FullPath;

            if (string.IsNullOrEmpty(overImage))
                return;

            var ext = Path.GetExtension(overImage).ToLower();
            if (ext != ".jpg" && ext != ".png" && ext != ".gif" && ext != ".jpeg")
                return;


            Dispatcher.Delay(600, imageFile =>
            {
                if (string.IsNullOrEmpty(overImage) || overImage != (string) imageFile)
                    return;

                try
                {

                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.UriSource = new Uri((string) imageFile);
                    bmp.EndInit();

                    ImagePreview.Source = bmp;
                    PopupImagePreview.IsOpen = true;
                }
                catch
                {
                }
            }, overImage);

        }

        private void TextFileOrFolderName_MouseLeave(object sender, MouseEventArgs e)
        {
            overImage = null;

            PopupImagePreview.IsOpen = false;
            ImagePreview.Source = null;
            ImagePreviewColumn.Height = new GridLength(0);

        }

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
                    if (t >= LastClickTime.AddMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime + 100) &&
                        t <= LastClickTime.AddMilliseconds(System.Windows.Forms.SystemInformation.DoubleClickTime * 2 + 100))
                    {
                        MenuRenameFile_Click(null, null);
                    }
                }

                LastItem = selected;
                LastClickTime = t;
            }
        }

        private void TextEditFileItem_LostFocus(object sender, RoutedEventArgs e)
        {
            var selected = TreeFolderBrowser.SelectedItem as PathItem;
            if (selected != null)
            {
                 if (selected.IsEditing) // this should be ahndled by Key ops in treeview
                     RenameOrCreateFileOrFolder();

                if (selected.DisplayName == "NewFile.md" || selected.DisplayName == "NewFolder")
                {
                    selected.Parent.Files.Remove(selected);
                    return;
                }

                selected.IsEditing = false;
                selected.SetIcon();
            }
        }

        #endregion

        #region Drag Operations

        private System.Windows.Point startPoint;

        private void TreeFolderBrowser_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
                    var treeViewItem = FindCommonVisualAncestor((DependencyObject) e.OriginalSource);
                    if (treeView == null || treeViewItem == null)
                        return;

                    var dragData = new DataObject(DataFormats.UnicodeText, selected.FullPath);
                    DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Copy);
                }
            }
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
