#region License

/*
 **************************************************************
 *  Author: Rick Strahl
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 *
 * Created: 04/28/2016
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************
*/

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Dragablz;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster.AddIns;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;
using Westwind.Utilities;
using Binding = System.Windows.Data.Binding;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextDataFormat = System.Windows.TextDataFormat;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace MarkdownMonster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
        //, IPreviewBrowser
    {
        public AppModel Model { get; set; }

        private NamedPipeManager PipeManager { get; set; }

        public IntPtr Hwnd
        {
            get
            {
                if (_hwnd == IntPtr.Zero)
                    _hwnd = new WindowInteropHelper(this).EnsureHandle();

                return _hwnd;
            }
        }

        private IntPtr _hwnd = IntPtr.Zero;

        private DateTime invoked = DateTime.MinValue;


        /// <summary>
        /// Manages the Preview Rendering in a WebBrowser Control
        /// </summary>
        public IPreviewBrowser PreviewBrowser { get; set; }

        //private PreviewBrowserWindow PreviewWindow;

        public PreviewBrowserWindow PreviewBrowserWindow
        {
            set { _previewBrowserWindow = value; }
            get
            {
                if (_previewBrowserWindow == null || _previewBrowserWindow.IsClosed)
                {
                    _previewBrowserWindow = new PreviewBrowserWindow()
                    {
                        Owner = this
                    };
                }

                return _previewBrowserWindow;
            }
        }


        public Grid PreviewBrowserContainer { get; set; }

        private PreviewBrowserWindow _previewBrowserWindow;

        public MainWindow()
        {
            InitializeComponent();


            Model = new AppModel(this);
            AddinManager.Current.RaiseOnModelLoaded(Model);
            AddinManager.Current.AddinsLoaded = OnAddinsLoaded;

            Model.WindowLayout = new MainWindowLayoutModel(this);

            DataContext = Model;

            TabControl.ClosingItemCallback = TabControlDragablz_TabItemClosing;

            Loaded += OnLoaded;
            Drop += MainWindow_Drop;
            AllowDrop = true;
            Activated += OnActivated;

            // Singleton App startup - server code that listens for other instances
            if (mmApp.Configuration.UseSingleWindow)
            {
                new TaskFactory().StartNew(() =>
                {
                    // Listen for other instances launching and pick up
                    // forwarded command line arguments
                    PipeManager = new NamedPipeManager("MarkdownMonster");
                    PipeManager.StartServer();
                    PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
                });
            }

            // Override some of the theme defaults (dark header specifically)
            mmApp.SetThemeWindowOverride(this);
        }


        #region Opening and Closing

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadPreviewBrowser();
            RestoreSettings();

            OpenFilesFromCommandLine();

            if (mmApp.Configuration.ApplicationUpdates.FirstRun)
            {
                if (TabControl.Items.Count == 0)
                {
                    try
                    {
                        string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                        File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"), tempFile, true);
                        OpenTab(tempFile);
                    }
                    catch (Exception ex)
                    {
                        mmApp.Log("Handled: Unable to copy file to temp folder.", ex);
                    }
                }

                // Add Snippets Addin
                Dispatcher.Delay(3000, p =>
                {
                    var url = "https://github.com/RickStrahl/Snippets-MarkdownMonster-Addin/raw/master/Build/addin.zip";
                    var addin = new AddinItem
                    {
                        id = "Snippets"
                    };
                    try
                    {
                        AddinManager.Current.DownloadAndInstallAddin(url,
                            Path.Combine(mmApp.Configuration.AddinsFolder), addin);
                    }
                    catch
                    {
                    }
                });

                mmApp.Configuration.ApplicationUpdates.FirstRun = false;
            }

            BindTabHeaders();
            SetWindowTitle();

            if (mmApp.Configuration.IsPreviewVisible)
            {
                ButtonHtmlPreview.IsChecked = true;
                ToolButtonPreview.IsChecked = true;
                //Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
            }

            Model.IsPresentationMode = mmApp.Configuration.OpenInPresentationMode;
            if (Model.IsPresentationMode)
            {
                Model.Commands.PresentationModeCommand.Execute(ToolButtonPresentationMode);
                Model.IsPreviewBrowserVisible = true;
            }

            var left = Left;
            Left = 300000;

            // force controls to realign - required because of WebBrowser control weirdness
            Dispatcher.InvokeAsync(() =>
            {
                //TabControl.InvalidateVisual();
                Left = left;

                mmApp.SetWorkingSet(10000000, 5000000);
            }, DispatcherPriority.Background);


            new TaskFactory().StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    FixMonitorPosition();
                    AddinManager.Current.InitializeAddinsUi(this);

                    AddinManager.Current.RaiseOnWindowLoaded();
                }, DispatcherPriority.ApplicationIdle);
            });
        }

        private void OnAddinsLoaded()
        {
            // Check to see if we are using another preview browser and load
            // that instead
            Dispatcher.Invoke(LoadPreviewBrowser);
        }


        /// <summary>
        /// Opens files from the command line or from an array of strings
        /// </summary>
        /// <param name="args">Array of file names. If null Command Line Args are used.</param>
        private void OpenFilesFromCommandLine(string[] args = null)
        {
            if (args == null)
            {
                // read fixed up command line args
                args = App.CommandArgs;

                if (args == null || args.Length == 0) // no args, only command line
                    return;
            }

            foreach (var fileArgs in args)
            {
                var file = fileArgs;
                if (string.IsNullOrEmpty(file))
                    continue;

                file = file.TrimEnd('\\');

                try
                {
                    // FAIL: This fails at runtime not in debugger when value is .\ trimmed to . VERY WEIRD
                    file = Path.GetFullPath(file);
                }
                catch
                {
                    mmApp.Log("Fullpath CommandLine failed: " + file);
                }

                Topmost = true;
                WindowUtilities.DoEvents();

                if (File.Exists(file))
                    OpenTab(mdFile: file, batchOpen: true);
                else if (Directory.Exists(file))
                {
                    ShowFolderBrowser(false, file);
                }
                else
                {
                    file = Path.Combine(App.InitialStartDirectory, file);
                    file = Path.GetFullPath(file);
                    if (File.Exists(file))
                        OpenTab(mdFile: file, batchOpen: true);
                    else if (Directory.Exists(file))
                        ShowFolderBrowser(false, file);
                }

                Dispatcher.Delay(800, s => { Topmost = false; });
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            WindowState = mmApp.Configuration.WindowPosition.WindowState;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            var editor = Model.ActiveEditor;
            if (editor == null) return;

            var doc = Model.ActiveDocument;
            if (doc == null) return;

            doc.IsActive = true;

            doc.LastEditorLineNumber = editor.GetLineNumber();
            if (doc.LastEditorLineNumber == -1)
                doc.LastEditorLineNumber = 0;

            base.OnDeactivated(e);

            mmApp.SetWorkingSet(10000000, 5000000);
        }

        protected void OnActivated(object sender, EventArgs e)
        {
            CheckFileChangeInOpenDocuments();
        }

        public void CheckFileChangeInOpenDocuments()
        {
            var selectedTab = TabControl.SelectedItem as TabItem;

            // check for external file changes
            for (int i = TabControl.Items.Count - 1; i > -1; i--)
            {
                var tab = TabControl.Items[i] as TabItem;

                if (tab != null)
                {
                    var editor = tab.Tag as MarkdownDocumentEditor;
                    var doc = editor?.MarkdownDocument;
                    if (doc == null)
                        continue;

                    if (doc.HasFileCrcChanged())
                    {
                        // force update to what's on disk so it doesn't fire again
                        // do here prior to dialogs so this code doesn't fire recursively
                        doc.UpdateCrc();


                        string filename = doc.FilenamePathWithIndicator.Replace("*", "");
                        string template = filename +
                                          "\r\n\r\n" +
                                          "This file has been modified by another program.\r\n" +
                                          "Do you want to reload it?";

                        if (!doc.IsDirty || MessageBox.Show(this, template,
                                "Reload",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            if (!doc.Load(doc.Filename))
                            {
                                MessageBox.Show(this, "Unable to re-load current document.",
                                    "Error re-loading file",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                continue;
                            }

                            try
                            {
                                int scroll = editor.GetScrollPosition();
                                editor.SetMarkdown(doc.CurrentText);
                                editor.AceEditor?.updateDocumentStats(false);

                                if (scroll > -1)
                                    editor.SetScrollPosition(scroll);
                            }
                            catch (Exception ex)
                            {
                                mmApp.Log("Changed file notification update failure", ex);
                                MessageBox.Show(this, "Unable to re-load current document.",
                                    "Error re-loading file",
                                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            if (tab == selectedTab)
                                PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition: true);
                        }
                    }
                }

                // Ensure that user hasn't higlighted a MenuItem so the menu doesn't lose focus
                if (!MainMenu.Items.OfType<MenuItem>().Any(item => item.IsHighlighted))
                {
                    var selectedEditor = selectedTab.Tag as MarkdownDocumentEditor;
                    if (selectedEditor != null)
                    {
                        try
                        {
                            selectedEditor.WebBrowser.Focus();
                            selectedEditor.SetEditorFocus();
                            selectedEditor.RestyleEditor();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _previewBrowserWindow?.Close();
            _previewBrowserWindow = null;

            AddinManager.Current.RaiseOnApplicationShutdown();

            bool isNewVersion = CheckForNewVersion(false, false);

            mmApp.Configuration.ApplicationUpdates.AccessCount++;

            SaveSettings();

            if (!CloseAllTabs())
            {
                //Show();
                e.Cancel = true;
                return;
            }

            Hide();

            Top -= 10000;

            if (mmApp.Configuration.UseSingleWindow)
            {
                PipeManager?.StopServer();

                if (App.Mutex != null)
                    App.Mutex.Dispose();
            }

            var displayCount = 6;
            if (mmApp.Configuration.ApplicationUpdates.AccessCount > 250)
                displayCount = 1;
            else if (mmApp.Configuration.ApplicationUpdates.AccessCount > 100)
                displayCount = 2;
            else if (mmApp.Configuration.ApplicationUpdates.AccessCount > 50)
                displayCount = 4;

            if (!isNewVersion &&
                mmApp.Configuration.ApplicationUpdates.AccessCount % displayCount == 0 &&
                !UnlockKey.IsRegistered())
            {
                Hide();
                Top += 10000;
                var rd = new RegisterDialog();
                rd.Owner = this;
                rd.ShowDialog();
            }

            mmApp.Shutdown();

            e.Cancel = false;
        }

        public void AddRecentFile(string file, bool noConfigWrite = false)
        {
            Dispatcher.InvokeAsync(() =>
            {
                mmApp.Configuration.AddRecentFile(file);

                if (!noConfigWrite)
                    mmApp.Configuration.Write();

                try
                {
                    MostRecentlyUsedList.AddToRecentlyUsedDocs(Path.GetFullPath(file));
                }
                catch
                {
                }
            }, DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// Creates/Updates the Recent Items Context list
        /// from recent file and recent folder configuration
        /// </summary>
        public void UpdateRecentDocumentsContextMenu()
        {
            var contextMenu = Resources["ContextMenuRecentFiles"] as ContextMenu;
            if (contextMenu == null)
                return;

            contextMenu.Items.Clear();
            ButtonRecentFiles.Items.Clear();

            MenuItem mi = null;
            MenuItem mi2 = null;

            List<string> badFiles = new List<string>();
            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                if (!File.Exists(file))
                {
                    badFiles.Add(file);
                    continue;
                }

                mi = new MenuItem()
                {
                    Header = file.Replace("_", "__"),
                };
                mi.Command = Model.Commands.OpenRecentDocumentCommand;
                mi.CommandParameter = file;
                contextMenu.Items.Add(mi);

                mi2 = new MenuItem()
                {
                    Header = file.Replace("_", "__")
                };
                mi2.Command = Model.Commands.OpenRecentDocumentCommand;
                mi2.CommandParameter = file;

                ButtonRecentFiles.Items.Add(mi2);
            }

            ToolbarButtonRecentFiles.ContextMenu = contextMenu;

            foreach (var file in badFiles)
                mmApp.Configuration.RecentDocuments.Remove(file);

            if (mmApp.Configuration.FolderBrowser.RecentFolders.Count > 0)
            {
                mi = new MenuItem
                {
                    IsEnabled = false,
                    Header = "——————— Recent Folders ———————"
                };
                contextMenu.Items.Add(mi);
                mi2 = new MenuItem
                {
                    IsEnabled = false,
                    Header = "——————— Recent Folders ———————"
                };
                ButtonRecentFiles.Items.Add(mi2);

                foreach (var folder in mmApp.Configuration.FolderBrowser.RecentFolders.Take(7))
                {
                    mi = new MenuItem()
                    {
                        Header = folder.Replace("_", "__"),
                        Command = Model.Commands.OpenRecentDocumentCommand,
                        CommandParameter = folder
                    };
                    contextMenu.Items.Add(mi);

                    mi2 = new MenuItem()
                    {
                        Header = folder.Replace("_", "__"),
                        Command = Model.Commands.OpenRecentDocumentCommand,
                        CommandParameter = folder
                    };
                    ButtonRecentFiles.Items.Add(mi2);
                }
            }

        }

        void RestoreSettings()
        {
            var conf = mmApp.Configuration;

            if (conf.WindowPosition.Width != 0)
            {
                Left = conf.WindowPosition.Left;
                Top = conf.WindowPosition.Top;
                Width = conf.WindowPosition.Width;
                Height = conf.WindowPosition.Height;
            }

            if (mmApp.Configuration.RememberLastDocumentsLength > 0 && mmApp.Configuration.UseSingleWindow)
            {
                //var selectedDoc = conf.RecentDocuments.FirstOrDefault();
                TabItem selectedTab = null;

                string firstDoc = conf.RecentDocuments.FirstOrDefault();


                // prevent TabSelectionChanged to fire
                batchTabAction = true;

                // since docs are inserted at the beginning we need to go in reverse
                foreach (var doc in conf.OpenDocuments.Take(mmApp.Configuration.RememberLastDocumentsLength).Reverse())
                {
                    if (doc.Filename == null)
                        continue;

                    if (File.Exists(doc.Filename))
                    {
                        var tab = OpenTab(doc.Filename, selectTab: false, batchOpen: true,
                            initialLineNumber: doc.LastEditorLineNumber);
                        if (tab == null)
                            continue;

                        if (doc.IsActive)
                        {
                            selectedTab = tab;
                            // have to explicitly notify initial activation
                            // since we surpress it on all tabs during startup
                            AddinManager.Current.RaiseOnDocumentActivated(doc);
                        }
                    }
                }

                if (selectedTab != null)
                    TabControl.SelectedItem = selectedTab;
                else
                    TabControl.SelectedIndex = 0;

                batchTabAction = false;
                BindTabHeaders();

            }

            Model.IsPreviewBrowserVisible = mmApp.Configuration.IsPreviewVisible;

            ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

            // force background so we have a little more contrast
            if (mmApp.Configuration.ApplicationTheme == Themes.Light)
            {
                ContentGrid.Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#eee");
                ToolbarPanelMain.Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#D5DAE8");
            }
            else
                ContentGrid.Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#333");
        }


        /// <summary>
        /// Save active settings of the UI that are persisted in the configuration
        /// </summary>
        public void SaveSettings()
        {
            var config = mmApp.Configuration;
            if (Model != null)
                config.IsPreviewVisible = Model.IsPreviewBrowserVisible;
            config.WindowPosition.IsTabHeaderPanelVisible = true;

            if (WindowState == WindowState.Normal)
            {
                config.WindowPosition.Left = mmFileUtils.TryConvertToInt32(Left);
                config.WindowPosition.Top = mmFileUtils.TryConvertToInt32(Top);
                config.WindowPosition.Width = mmFileUtils.TryConvertToInt32(Width, 900);
                config.WindowPosition.Height = mmFileUtils.TryConvertToInt32(Height, 700);
            }

            if (WindowState != WindowState.Minimized)
                config.WindowPosition.WindowState = WindowState;

            if (LeftSidebarColumn.Width.Value > 20)
            {
                if (LeftSidebarColumn.Width.IsAbsolute)
                    config.FolderBrowser.WindowWidth =
                        mmFileUtils.TryConvertToInt32(LeftSidebarColumn.Width.Value, 220);
                config.FolderBrowser.Visible = true;
            }
            else
                config.FolderBrowser.Visible = false;


            config.FolderBrowser.FolderPath = FolderBrowser.FolderPath;


            config.OpenDocuments.Clear();

            if (mmApp.Configuration.RememberLastDocumentsLength > 0)
            {
                foreach (var recentDocument in config.RecentDocuments.Take(mmApp.Configuration
                    .RememberLastDocumentsLength))
                {
                    var editor = this.GetTabFromFilename(recentDocument)?.Tag as MarkdownDocumentEditor;

                    var doc = editor?.MarkdownDocument;
                    if (doc == null)
                        continue;

                    doc.LastEditorLineNumber = editor.GetLineNumber();
                    if (doc.LastEditorLineNumber < 0)
                        doc.LastEditorLineNumber = 0;

                    config.OpenDocuments.Add(doc);
                }
            }

            config.Write();
        }



        public bool SaveFile(bool secureSave = false)
        {
            var tab = TabControl.SelectedItem as TabItem;
            if (tab == null)
                return false;

            var editor = tab.Tag as MarkdownDocumentEditor;
            var doc = editor?.MarkdownDocument;
            if (doc == null)
                return false;

            // prompt for password on a secure save
            if (secureSave && editor.MarkdownDocument.Password == null)
            {
                var pwdDialog = new FilePasswordDialog(editor.MarkdownDocument, false);
                pwdDialog.ShowDialog();
            }

            if (!editor.SaveDocument())
            {
                //var res = await this.ShowMessageOverlayAsync("Unable to save Document",
                //    "Unable to save document most likely due to missing permissions.");

                MessageBox.Show("Unable to save document most likely due to missing permissions.",
                    mmApp.ApplicationName);
                return false;
            }

            return true;
        }

        #endregion

        #region Tab Handling

        /// <summary>
        /// Opens a tab by a filename
        /// </summary>
        /// <param name="mdFile"></param>
        /// <param name="editor"></param>
        /// <param name="showPreviewIfActive"></param>
        /// <param name="syntax"></param>
        /// <param name="selectTab"></param>
        /// <param name="rebindTabHeaders">
        /// Rebinds the headers which should be done whenever a new Tab is
        /// manually opened and added but not when opening in batch.
        ///
        /// Checks to see if multiple tabs have the same filename open and
        /// if so displays partial path.
        ///
        /// New Tabs are opened at the front of the tab list at index 0
        /// </param>
        /// <returns></returns>
        public TabItem OpenTab(string mdFile = null,
            MarkdownDocumentEditor editor = null,
            bool showPreviewIfActive = false,
            string syntax = "markdown",
            bool selectTab = true,
            bool rebindTabHeaders = false,
            bool batchOpen = false,
            int initialLineNumber = 0,
            bool readOnly = false)
        {
            if (mdFile != null && mdFile != "untitled" &&
                (!File.Exists(mdFile) ||
                 !AddinManager.Current.RaiseOnBeforeOpenDocument(mdFile)))
                return null;

            var tab = new TabItem();
            //tab.Margin = new Thickness(0, 0, 3, 0);
            //tab.Padding = new Thickness(2, 0, 7, 2);
            tab.Background = Background;

            ControlsHelper.SetHeaderFontSize(tab, 13F);


            if (editor == null)
            {
                editor = new MarkdownDocumentEditor
                {
                    Window = this,
                    EditorSyntax = syntax,
                    InitialLineNumber = initialLineNumber,
                    IsReadOnly = readOnly
                };

                tab.Content = editor.EditorPreviewPane;

                var doc = new MarkdownDocument()
                {
                    Filename = mdFile ?? "untitled",
                    Dispatcher = Dispatcher
                };
                if (doc.Filename != "untitled")
                {
                    doc.Filename = FileUtils.GetPhysicalPath(doc.Filename);

                    if (doc.HasBackupFile())
                    {
                        try
                        {
                            ShowStatus("Auto-save recovery files have been found and opened in the editor.",
                                milliSeconds: 9000);
                            SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                            {
                                File.Copy(doc.BackupFilename, doc.BackupFilename + ".md");
                                OpenTab(doc.BackupFilename + ".md");
                                File.Delete(doc.BackupFilename + ".md");
                            }
                        }
                        catch (Exception ex)
                        {
                            string msg = "Unable to open backup file: " + doc.BackupFilename + ".md";
                            mmApp.Log(msg, ex);
                            MessageBox.Show(
                                "A backup file was previously saved, but we're unable to open it.\r\n" + msg,
                                "Cannot open backup file",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }

                    if (doc.Password == null && doc.IsFileEncrypted())
                    {
                        var pwdDialog = new FilePasswordDialog(doc, true)
                        {
                            Owner = this
                        };
                        bool? pwdResult = pwdDialog.ShowDialog();
                        if (pwdResult == false)
                        {
                            ShowStatus("Encrypted document not opened, due to missing password.",
                                mmApp.Configuration.StatusMessageTimeout);

                            return null;
                        }
                    }


                    if (!doc.Load())
                    {
                        if (!batchOpen)
                        {
                            var msg = "Most likely you don't have access to the file";
                            if (doc.Password != null && doc.IsFileEncrypted())
                                msg = "Invalid password for opening this file";
                            var file = Path.GetFileName(doc.Filename);

                            MessageBox.Show(
                                $"{msg}.\r\n\r\n{file}",
                                "Can't open File", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }

                        return null;
                    }
                }

                doc.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsDirty")
                    {
                        //CommandManager.InvalidateRequerySuggested();
                        Model.Commands.SaveCommand.InvalidateCanExecute();
                    }
                };
                editor.MarkdownDocument = doc;

                SetTabHeaderBinding(tab, doc, "FilenameWithIndicator");


                tab.ToolTip = doc.Filename;
            }

            var filename = Path.GetFileName(editor.MarkdownDocument.Filename);
            tab.Tag = editor;


            editor.LoadDocument();

            // is the tab already open?
            TabItem existingTab = null;
            if (filename != "untitled")
            {
                foreach (TabItem tb in TabControl.Items)
                {
                    var lEditor = tb.Tag as MarkdownDocumentEditor;
                    if (lEditor.MarkdownDocument.Filename == editor.MarkdownDocument.Filename)
                    {
                        existingTab = tb;
                        break;
                    }
                }
            }

            Model.OpenDocuments.Add(editor.MarkdownDocument);
            Model.ActiveDocument = editor.MarkdownDocument;

            if (existingTab != null)
                TabControl.Items.Remove(existingTab);

            tab.IsSelected = false;

            TabControl.Items.Insert(0, tab);


            if (selectTab)
            {
                TabControl.SelectedItem = tab;

                if (showPreviewIfActive && PreviewBrowserContainer.Width > 5)
                    PreviewBrowser.PreviewMarkdownAsync();

                SetWindowTitle();

                Model.OnPropertyChanged(nameof(AppModel.ActiveEditor));
            }

            AddinManager.Current.RaiseOnAfterOpenDocument(editor.MarkdownDocument);

            if (rebindTabHeaders)
                BindTabHeaders();

            // force bindings to change
            Model.OnPropertyChanged(nameof(AppModel.IsTabOpen));
            Model.OnPropertyChanged(nameof(AppModel.IsNoTabOpen));

            return tab;
        }


        /// <summary>
        /// Refreshes an already loaded tab with contents of a new (or the same file) file
        /// by just replacing the document's text.
        /// 
        /// If the tab is not found a new tab is opened.
        /// 
        /// Note: File must already be open for this to work                
        /// </summary>
        /// <param name="editorFile">File name to display int the tab</param>
        /// <param name="maintainScrollPosition">If possible preserve scroll position if refreshing</param>
        /// <param name="noPreview">If true don't refresh the preview after updating the file</param>
        /// <param name="noFocus">if true don't focus the editor</param>
        /// <param name="readOnly">if true document can't be edited</param>           
        /// <returns>selected tab item or null</returns>
        public TabItem RefreshTabFromFile(string editorFile,
            bool maintainScrollPosition = false,
            bool noPreview = false,
            bool noFocus = false,
            bool readOnly = false)
        {

            var tab = GetTabFromFilename(editorFile);
            if (tab == null)
                return OpenTab(editorFile, rebindTabHeaders: true, readOnly: readOnly);

            // load the underlying document
            var editor = tab.Tag as MarkdownDocumentEditor;
            editor.MarkdownDocument.Load(editorFile);

            if (!maintainScrollPosition)
                editor.SetCursorPosition(0, 0);

            editor.SetMarkdown(editor.MarkdownDocument.CurrentText);
            var state = editor.IsDirty(); // force refresh

            if (!noFocus)
                TabControl.SelectedItem = tab;

            if (!noPreview)
                PreviewMarkdownAsync();

            return tab;
        }


        /// <summary>
        /// Closes a tab and ask for confirmation if the tab doc
        /// is dirty.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="rebindTabHeaders">
        /// When true tab headers are rebound to handle duplicate filenames
        /// with path additions.
        /// </param>
        /// <returns>true if tab can close, false if it should stay open</returns>
        public bool CloseTab(TabItem tab, bool rebindTabHeaders = true, bool dontPromptForSave = false)
        {
            var editor = tab?.Tag as MarkdownDocumentEditor;
            if (editor == null)
                return false;

            bool returnValue = true;

            tab.Padding = new Thickness(200);

            var doc = editor.MarkdownDocument;

            doc.CleanupBackupFile();

            if (doc.IsDirty && !dontPromptForSave)
            {
                var res = MessageBox.Show(Path.GetFileName(doc.Filename) + "\r\n\r\nhas been modified.\r\n" +
                                          "Do you want to save changes?",
                    "Save Document",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
                if (res == MessageBoxResult.Cancel)
                {
                    return false; // don't close
                }

                if (res == MessageBoxResult.No)
                {
                    // close but don't save
                }
                else
                {
                    if (doc.Filename == "untitled")
                        Model.Commands.SaveAsCommand.Execute(ButtonSaveAsFile);
                    else if (!SaveFile())
                        returnValue = false;
                }
            }

            doc.LastEditorLineNumber = editor.GetLineNumber();
            if (doc.LastEditorLineNumber == -1)
                doc.LastEditorLineNumber = 0;

            tab.Tag = null;
            TabControl.Items.Remove(tab);

            if (TabControl.Items.Count == 0)
            {
                PreviewBrowser.Navigate("about:blank");

                Model.ActiveDocument = null;
                Title = "Markdown Monster" +
                        (UnlockKey.Unlocked ? "" : " (unregistered)");
            }

            if (rebindTabHeaders)
                BindTabHeaders();

            Model.OnPropertyChanged(nameof(AppModel.IsTabOpen));
            Model.OnPropertyChanged(nameof(AppModel.IsNoTabOpen));

            return returnValue; // close
        }

        /// <summary>
        /// Closes a tab and ask for confirmation if the tab doc
        /// is dirty.
        /// </summary>
        /// <param name="filename">
        /// The absolute path to the file opened in the tab that
        /// is going to be closed
        /// </param>
        /// <returns>true if tab can close, false if it should stay open or
        /// filename not opened in any tab</returns>
        public bool CloseTab(string filename)
        {
            TabItem tab = GetTabFromFilename(filename);

            if (tab != null)
            {
                return CloseTab(tab);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Flag used to let us know we don't want to perform tab selection operations
        /// </summary>
        private bool batchTabAction = false;

        public bool CloseAllTabs(TabItem allExcept = null)
        {
            batchTabAction = true;
            for (int i = TabControl.Items.Count - 1; i > -1; i--)
            {
                var tab = TabControl.Items[i] as TabItem;

                if (tab != null)
                {
                    if (allExcept != null && tab == allExcept)
                        continue;

                    if (!CloseTab(tab, rebindTabHeaders: false))
                        return false;
                }
            }

            batchTabAction = false;

            //WindowUtilities.InvalidateMenuCommands(MainMenu);
            Model.Commands.InvalidateCommands();

            return true;
        }

        /// <summary>
        /// Retrieves an open tab based on its filename.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public TabItem GetTabFromFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return null;

            TabItem tab = null;
            foreach (TabItem tabItem in TabControl.Items.Cast<TabItem>())
            {
                var markdownEditor = tabItem.Tag as MarkdownDocumentEditor;
                if (markdownEditor.MarkdownDocument.Filename.Equals(filename,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    tab = tabItem;
                    break;
                }
            }

            return tab;
        }

        /// <summary>
        /// Binds all Tab Headers
        /// </summary>
        public void BindTabHeaders()
        {
            var tabList = new List<TabItem>();
            foreach (TabItem tb in TabControl.Items)
                tabList.Add(tb);

            var tabItems = tabList
                .Select(tb => Path.GetFileName(((MarkdownDocumentEditor) tb.Tag).MarkdownDocument.Filename.ToLower()))
                .GroupBy(fn => fn)
                .Select(tbCol => new
                {
                    Filename = tbCol.Key,
                    Count = tbCol.Count()
                });

            foreach (TabItem tb in TabControl.Items)
            {
                var doc = ((MarkdownDocumentEditor) tb.Tag).MarkdownDocument;

                if (tabItems.Any(ti => ti.Filename == Path.GetFileName(doc.Filename.ToLower()) &&
                                       ti.Count > 1))

                    SetTabHeaderBinding(tb, doc, "FilenamePathWithIndicator");
                else
                    SetTabHeaderBinding(tb, doc, "FilenameWithIndicator");
            }
        }

        /// <summary>
        /// Binds the tab header to an expression
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="document"></param>
        /// <param name="propertyPath"></param>
        private void SetTabHeaderBinding(TabItem tab, MarkdownDocument document,
            string propertyPath = "FilenameWithIndicator",
            ImageSource icon = null)
        {
            if (document == null || tab == null)
                return;

            try
            {
                var grid = new Grid();
                tab.Header = grid;
                var col1 = new ColumnDefinition {Width = new GridLength(20)};
                var col2 = new ColumnDefinition {Width = GridLength.Auto};
                grid.ColumnDefinitions.Add(col1);
                grid.ColumnDefinitions.Add(col2);


                if (icon == null)
                {
                    icon = FolderStructure.IconList.GetIconFromFile(document.Filename);
                    if (icon == AssociatedIcons.DefaultIcon)
                        icon = FolderStructure.IconList.GetIconFromType(Model.ActiveEditor.EditorSyntax);
                }


                var img = new Image()
                {
                    Source = icon,
                    Height = 16,
                    Margin = new Thickness(0, 1, 5, 0)
                };
                img.SetValue(Grid.ColumnProperty, 0);
                grid.Children.Add(img);


                var textBlock = new TextBlock();
                textBlock.SetValue(Grid.ColumnProperty, 1);

                var headerBinding = new Binding
                {
                    Source = document,
                    Path = new PropertyPath(propertyPath),
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, headerBinding);

                var fontWeightBinding = new Binding
                {
                    Source = tab,
                    Path = new PropertyPath("IsSelected"),
                    Mode = BindingMode.OneWay,
                    Converter = new FontWeightFromBoolConverter()
                };
                BindingOperations.SetBinding(textBlock, TextBlock.FontWeightProperty, fontWeightBinding);


                grid.Children.Add(textBlock);

                //BindingOperations.SetBinding(tab, HeaderedContentControl.HeaderProperty, headerBinding);
            }
            catch (Exception ex)
            {
                mmApp.Log("SetTabHeaderBinding Failed. Assigning explicit path", ex);
                tab.Header = document.FilenameWithIndicator;
            }
        }


        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (batchTabAction)
                return;

            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            var tab = TabControl.SelectedItem as TabItem;

            SetWindowTitle();

            foreach (var doc in Model.OpenDocuments)
                doc.IsActive = false;

            Model.ActiveDocument = editor.MarkdownDocument;
            Model.ActiveDocument.IsActive = true;

            AddRecentFile(Model.ActiveDocument?.Filename, noConfigWrite: true);

            AddinManager.Current.RaiseOnDocumentActivated(Model.ActiveDocument);


            if (PreviewBrowserContainer.Parent != null)
                ((Grid) PreviewBrowserContainer.Parent).Children.Remove(PreviewBrowserContainer);

            editor.EditorPreviewPane.PreviewBrowserContainer.Children.Add(PreviewBrowserContainer);

            var grid = tab.Content as Grid;
            if (grid != null)
                grid.Children.Add(PreviewBrowserContainer);

            Model.WindowLayout.IsPreviewVisible = mmApp.Configuration.IsPreviewVisible;

            if (mmApp.Configuration.IsPreviewVisible)
                PreviewBrowser?.PreviewMarkdown();

            Model.ActiveEditor.RestyleEditor();

            editor.WebBrowser.Focus();
            editor.SetEditorFocus();

            Dispatcher.InvokeAsync(() => { UpdateDocumentOutline(); }, DispatcherPriority.ApplicationIdle);
        }


        private void TabControlDragablz_TabItemClosing(ItemActionCallbackArgs<TabablzControl> e)
        {
            var tab = e.DragablzItem.DataContext as TabItem;
            if (tab == null)
                return;

            if (!CloseTab(tab))
                e.Cancel();
        }

        /// <summary>
        /// Adds a new panel to the sidebar
        /// </summary>
        /// <param name="tabItem">Adds the TabItem. If null the tabs are refreshed and tabs removed if down to single tab</param>
        public void AddLeftSidebarPanelTabItem(TabItem tabItem = null)
        {
            if (tabItem != null)
            {
                ControlsHelper.SetHeaderFontSize(tabItem, 14);
                SidebarContainer.Items.Add(tabItem);
                SidebarContainer.SelectedItem = tabItem;
            }
        }

        /// <summary>
        /// Adds a new panel to the right sidebar
        /// </summary>
        /// <param name="tabItem">Adds the TabItem. If null the tabs are refreshed and tabs removed if down to single tab</param>
        public void AddRightSidebarPanelTabItem(TabItem tabItem = null)
        {
            if (tabItem != null)
            {
                ControlsHelper.SetHeaderFontSize(tabItem, 14);
                RightSidebarContainer.Items.Add(tabItem);
                RightSidebarContainer.SelectedItem = tabItem;
            }

            ShowRightSidebar();
        }


        /// <summary>
        /// Sets the Window Title followed by Markdown Monster (registration status)
        /// by default the filename is used and it's updated whenever tabs are changed.
        ///
        /// Generally just call this when you need to have the title updated due to
        /// file name change that doesn't change the active tab.
        /// </summary>
        /// <param name="title"></param>
        public void SetWindowTitle(string title = null)
        {
            if (title == null)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                title = editor.MarkdownDocument.FilenameWithIndicator.Replace("*", "");
            }

            Title = title +
                    "  - Markdown Monster" +
                    (UnlockKey.Unlocked ? "" : " (unregistered)");
        }

        #endregion

        #region Document Outline

        private void SidebarContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = SidebarContainer.SelectedItem as TabItem;
            if (selected == null)
                return;

            if (selected.Content is DocumentOutlineSidebarControl)
            {
                Dispatcher.Delay(120, p =>
                {
                    if (DocumentOutline.Model?.DocumentOutline == null)
                        UpdateDocumentOutline();
                });
            }

        }

        public void UpdateDocumentOutline(int editorLineNumber = -1)
        {
            DocumentOutline?.RefreshOutline(editorLineNumber);
        }

        #endregion


        #region Preview and UI Visibility Helpers

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
            bool showInBrowser = false, string renderedHtml = null)
        {
            PreviewBrowser?.PreviewMarkdown(editor, keepScrollPosition, showInBrowser, renderedHtml);
        }

        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
            string renderedHtml = null)
        {
            PreviewBrowser?.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml);
        }


        public void Navigate(string url)
        {
            PreviewBrowser?.Navigate(url);
        }


        /// <summary>
        /// Shows or hides the preview browser
        /// </summary>
        /// <param name="hide"></param>
        public void ShowPreviewBrowser(bool hide = false, bool refresh = false)
        {
            if (!hide && Model.Configuration.PreviewMode != PreviewModes.None)
            {
                if (Model.Configuration.PreviewMode == PreviewModes.InternalPreview)
                {

                    if (Model.Configuration.IsPreviewVisible)
                        Model.WindowLayout.IsPreviewVisible = true;
                    else
                    {
                        Model.WindowLayout.IsPreviewVisible = false;
                        return;
                    }

                    // check if we're already active - if not assign and preview immediately
                    if (!(PreviewBrowser is IPreviewBrowser))
                    {
                        LoadPreviewBrowser();
                        return;
                    }

                    // close external window if it's open
                    if (_previewBrowserWindow != null && PreviewBrowserWindow.Visibility == Visibility.Visible)
                    {
                        PreviewBrowserWindow.Close();
                        _previewBrowserWindow = null;
                        LoadPreviewBrowser();
                        return;
                    }

                    //MainWindowSeparatorColumn.Width = new GridLength(12);
                    if (!refresh)
                    {
                        if (mmApp.Configuration.WindowPosition.SplitterPosition < 100)
                            mmApp.Configuration.WindowPosition.SplitterPosition = 600;

                        //if (!Model.IsPresentationMode)
                        //    MainWindowPreviewColumn.Width =
                        //        new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);
                    }
                }
                else if (Model.Configuration.PreviewMode == PreviewModes.ExternalPreviewWindow)
                {
                    // make sure it's visible
                    //bool visible = PreviewBrowserWindow.Visibility == Visibility.Visible;
                    PreviewBrowserWindow.Show();

                    // check if we're already active - if not assign and preview immediately
                    if (!(PreviewBrowser is PreviewBrowserWindow))
                    {
                        PreviewBrowser = PreviewBrowserWindow;
                        PreviewBrowser.PreviewMarkdownAsync();
                    }


                    Model.WindowLayout.IsPreviewVisible = false;

                    // clear the preview
                    ((IPreviewBrowser) PreviewBrowserContainer.Children[0]).Navigate("about:blank");
                }
            }
            else
            {
                if (Model.Configuration.PreviewMode == PreviewModes.InternalPreview)
                {
                    Model.WindowLayout.IsPreviewVisible = false;

                    // clear the preview
                    ((IPreviewBrowser) PreviewBrowserContainer.Children[0]).Navigate("about:blank");
                }
                else if (Model.Configuration.PreviewMode == PreviewModes.ExternalPreviewWindow)
                {
                    if (_previewBrowserWindow != null)
                    {
                        PreviewBrowserWindow.Close();
                        _previewBrowserWindow = null;
                        PreviewBrowser = null;

                        // reset preview browser to internal so it's not null
                        //LoadPreviewBrowser();
                    }
                }

            }
        }

        /// <summary>
        /// Shows or hides the File Browser
        /// </summary>
        /// <param name="hide"></param>
        public void ShowFolderBrowser(bool hide = false, string folder = null)
        {
            var layoutModel = Model.WindowLayout;
            if (hide)
            {
                layoutModel.IsLeftSidebarVisible = false;
                mmApp.Configuration.FolderBrowser.Visible = false;
            }
            else
            {
                if (folder == null)
                    folder = FolderBrowser.FolderPath;
                if (folder == null)
                    folder = mmApp.Configuration.FolderBrowser.FolderPath;

                Dispatcher.InvokeAsync(() =>
                {
                    if (string.IsNullOrEmpty(folder) && Model.ActiveDocument != null)
                        folder = Path.GetDirectoryName(Model.ActiveDocument.Filename);

                    FolderBrowser.FolderPath = folder;
                }, DispatcherPriority.ApplicationIdle);

                layoutModel.IsLeftSidebarVisible = true;
                mmApp.Configuration.FolderBrowser.Visible = true;
                SidebarContainer.SelectedIndex = 0; // folder browser tab
            }
        }

        public void ShowLeftSidebar(bool hide = false)
        {
            if (!hide && SidebarContainer.Items.Count == 1)
            {
                ShowFolderBrowser();
                return;
            }

            Model.WindowLayout.IsLeftSidebarVisible = !hide;
        }

        public void ShowRightSidebar(bool hide = false)
        {
            Model.WindowLayout.IsRightSidebarVisible = !hide;
        }

        public void LoadPreviewBrowser()
        {
            var previewBrowser = AddinManager.Current.RaiseGetPreviewBrowserControl();
            if (previewBrowser == null || PreviewBrowser != previewBrowser)
            {
                if (previewBrowser == null)
                    PreviewBrowser = new IEWebBrowserControl() {Name = "PreviewBrowser"};
                else
                    PreviewBrowser = previewBrowser;

                if (PreviewBrowserContainer == null)
                    PreviewBrowserContainer = new Grid();


                PreviewBrowserContainer.Children.Clear();
                PreviewBrowserContainer.Children.Add(PreviewBrowser as UIElement);

                ShowPreviewBrowser();
            }

            // show or hide
            PreviewMarkdownAsync();
        }

        #endregion

        #region Worker Functions

        public MarkdownDocumentEditor GetActiveMarkdownEditor()
        {
            var tab = TabControl?.SelectedItem as TabItem;
            return tab?.Tag as MarkdownDocumentEditor;
        }

        bool CheckForNewVersion(bool force, bool closeForm = true, int timeout = 2000)
        {
            var updater = new ApplicationUpdater(typeof(MainWindow));
            bool isNewVersion = updater.IsNewVersionAvailable(!force, timeout: timeout);
            if (isNewVersion)
            {
                var res = MessageBox.Show(updater.VersionInfo.Detail + "\r\n\r\n" +
                                          "Do you want to download and install this version?",
                    updater.VersionInfo.Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (res == MessageBoxResult.Yes)
                {
                    ShellUtils.GoUrl(mmApp.Urls.InstallerDownloadUrl);

                    if (closeForm)
                        Close();
                }
            }

            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;

            return isNewVersion;
        }

        /// <summary>
        /// Check to see if the window is visible in the bounds of the
        /// virtual screen space. If not adjust to main monitor off 0 position.
        /// </summary>
        /// <returns></returns>
        void FixMonitorPosition()
        {
            var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
            var virtualScreenWidth = SystemParameters.VirtualScreenWidth;


            if (Left > virtualScreenWidth - 150)
                Left = 20;
            if (Top > virtualScreenHeight - 150)
                Top = 20;

            if (Left < SystemParameters.VirtualScreenLeft)
                Left = SystemParameters.VirtualScreenLeft;
            if (Top < SystemParameters.VirtualScreenTop)
                Top = SystemParameters.VirtualScreenTop;

            if (Width > virtualScreenWidth)
                Width = virtualScreenWidth - 40;
            if (Height > virtualScreenHeight)
                Height = virtualScreenHeight - 40;
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Generic button handler that handles a number of simple
        /// tasks in a single method to minimize class noise.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Button_Handler(object sender, RoutedEventArgs e)
        {
            var button = sender;
            if (button == null)
                return;

            if (button == ButtonOpenFromHtml)
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".htm",
                    Filter = "Html files (*.htm,*.html)|*.htm;*.html|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Multiselect = true,
                    Title = "Open Html as Markdown"
                };

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastFolder;

                var res = fd.ShowDialog();
                if (res == null || !res.Value)
                    return;

                var html = File.ReadAllText(fd.FileName);

                var markdown = MarkdownUtilities.HtmlToMarkdown(html);

                OpenTab("untitled");
                var editor = GetActiveMarkdownEditor();
                editor.MarkdownDocument.CurrentText = markdown;
                PreviewBrowser.PreviewMarkdown();
            }
            else if (button == ButtonRecentFiles)
            {
                var mi = button as MenuItem;
                UpdateRecentDocumentsContextMenu();
                mi.IsSubmenuOpen = true;
            }
            else if (button == ToolbarButtonRecentFiles)
            {
                var mi = button as Button;
                UpdateRecentDocumentsContextMenu();
                mi.ContextMenu.IsOpen = true;
            }
            else if (button == ButtonExit)
            {
                Close();
            }

            else if (button == MenuOpenConfigFolder)
            {
                ShellUtils.GoUrl(mmApp.Configuration.CommonFolder);
            }
            else if (button == MenuOpenPreviewFolder)
            {
                ShellUtils.GoUrl(Path.Combine(Environment.CurrentDirectory, "PreviewThemes",
                    mmApp.Configuration.PreviewTheme));
            }
            else if (button == MenuMarkdownMonsterSite)
            {
                ShellUtils.GoUrl(mmApp.Urls.WebSiteUrl);
            }
            else if (button == MenuBugReport)
            {
                ShellUtils.GoUrl(mmApp.Urls.SupportUrl);
            }
            else if (button == MenuCheckNewVersion)
            {
                ShowStatus("Checking for new version...");
                if (!CheckForNewVersion(true, timeout: 5000))
                {
                    ShowStatus("Your version of Markdown Monster is up to date.", 6000);
                    SetStatusIcon(FontAwesomeIcon.Check, Colors.Green);

                    MessageBox.Show(
                        "Your version of Markdown Monster is v" + mmApp.GetVersion() + " and you are up to date.",
                        mmApp.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else if (button == MenuRegister)
            {
                Window rf = new RegistrationForm();
                rf.Owner = this;
                rf.ShowDialog();
            }
            else if (button == ButtonAbout)
            {
                Window about = new About();
                about.Owner = this;
                about.Show();
            }
            else if (button == Button_Find)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                editor.ExecEditorCommand("find");
            }
            else if (button == Button_FindNext)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                editor.ExecEditorCommand("findnext");
            }
            else if (button == Button_Replace)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                editor.ExecEditorCommand("replace");
            }
            else if (button == ButtonScrollBrowserDown)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                editor.SpecialKey("ctrl-shift-down");
            }
            else if (button == ButtonScrollBrowserUp)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;
                editor.SpecialKey("ctrl-shift-d");
            }
            else if (button == ButtonDocumentOutlineVisible)
            {
                // Only activate/deactivate the tab
                if (Model.ActiveEditor != null && Model.ActiveEditor.EditorSyntax == "markdown" &&
                    Model.Configuration.IsDocumentOutlineVisible)
                    SidebarContainer.SelectedItem = TabDocumentOutline;
                else
                    SidebarContainer.SelectedItem = TabFolderBrowser;
            }
            else if (button == ButtonWordWrap || button == ButtonLineNumbers || button == ButtonShowInvisibles)
            {
                Model.ActiveEditor?.RestyleEditor();
            }
            else if (button == ButtonStatusEncrypted)
            {
                var dialog = new FilePasswordDialog(Model.ActiveDocument, false)
                {
                    Owner = this
                };
                dialog.ShowDialog();
            }
            //else if (button == ButtonRefreshBrowser)
            //{
            //	var editor = GetActiveMarkdownEditor();
            //	if (editor == null)
            //		return;

            //	this.PreviewMarkdownAsync();
            //}
            else if (button == MenuDocumentation)
                ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl);
            else if (button == MenuMarkdownBasics)
                ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl + "_4ne1eu2cq.htm");
            else if (button == MenuCreateAddinDocumentation)
                ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl + "_4ne0s0qoi.htm");
            else if (button == MenuShowSampleDocument)
                OpenTab(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"));
            else if (button == MenuShowErrorLog)
            {
                string logFile = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonsterErrors.txt");
                if (File.Exists(logFile))
                    ShellUtils.GoUrl(logFile);
                else
                    MessageBox.Show("There are no errors in your log file.",
                        mmApp.ApplicationName,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            else if (button == MenuResetConfiguration)
            {
                if (MessageBox.Show(
                        "This operation will reset all of your configuration settings and shut down Markdown Monster.\r\n\r\nAre you sure?",
                        "Reset Configuration Settings",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    mmApp.Configuration.Backup();
                    mmApp.Configuration.Reset();
                }
            }
            else if (button == MenuBackupConfiguration)
            {
                string filename = mmApp.Configuration.Backup();
                ShowStatus($"Configuration backed up to: {Path.GetFileName(filename)}", 6000);
                mmFileUtils.OpenFileInExplorer(filename);
            }
        }


        private void ButtonSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tab in TabControl.Items)
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                editor?.RestyleEditor();
            }
        }



        private void Button_CommandWindow(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;


            string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);
            mmFileUtils.OpenTerminal(path);
        }

        private void Button_OpenExplorer(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            mmFileUtils.OpenFileInExplorer(editor.MarkdownDocument.Filename);
        }


        private void Button_OpenFolderBrowser(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            SidebarContainer.SelectedItem = TabFolderBrowser;
            ShowFolderBrowser(folder: Path.GetDirectoryName(editor.MarkdownDocument.Filename));
        }

        internal void Button_PasteMarkdownFromHtml(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            string html = null;
            if (Clipboard.ContainsText(TextDataFormat.Html))
                html = Clipboard.GetText(TextDataFormat.Html);

            if (!string.IsNullOrEmpty(html))
                html = StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
            else
                html = Clipboard.GetText();

            if (string.IsNullOrEmpty(html))
                return;

            var markdown = MarkdownUtilities.HtmlToMarkdown(html);

            editor.SetSelection(markdown);
            editor.SetEditorFocus();

            PreviewBrowser.PreviewMarkdownAsync(editor, true);
        }

        #endregion

        #region Miscelleaneous Events

        /// <summary>
        /// Handle drag and drop of file. Note only works when dropped on the
        /// window - doesn't not work when dropped on the editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file.ToLower());
                    if (File.Exists(file) && mmApp.AllowedFileExtensions.Contains($",{ext},"))
                    {
                        OpenTab(file, rebindTabHeaders: true);
                    }
                }
            }
        }

        private void PreviewBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (e.NewSize.Width > 100)
            //{
            //	int width = Convert.ToInt32(MainWindowPreviewColumn.Width.Value);
            //	if (width > 100)
            //		mmApp.Configuration.WindowPosition.SplitterPosition = width;
            //}
        }

        private void EditorTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (TabItem tab in TabControl.Items)
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                editor?.RestyleEditor();
            }

            PreviewBrowser?.PreviewMarkdownAsync();
        }

        private void PreviewTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PreviewBrowser?.PreviewMarkdownAsync();
        }

        private void AppTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateTime.UtcNow < mmApp.Started.AddSeconds(5))
                return;

            if (mmApp.Configuration.ApplicationTheme == Themes.Default)            
                mmApp.Configuration.ApplicationTheme = Themes.Dark;


            if (MessageBox.Show(
                    "Application theme changes require that you restart.\r\n\r\nDo you want to restart Markdown Monster?",
                    "Theme Change", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) ==
                MessageBoxResult.Yes)
            {

                if (mmApp.Configuration.ApplicationTheme == Themes.Light)
                    mmApp.Configuration.EditorTheme = "visualstudio";
                else
                    mmApp.Configuration.EditorTheme = "twilight";

                mmApp.Configuration.Write();
                Close();
                mmFileUtils.ExecuteProcess(Path.Combine(Environment.CurrentDirectory, "MarkdownMonster.exe"), "");
            }
        }

        private void DocumentType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Model.ActiveEditor == null)
                return;

            Model.ActiveEditor.SetEditorSyntax(Model.ActiveEditor.EditorSyntax);
            SetTabHeaderBinding(TabControl.SelectedItem as TabItem, Model.ActiveEditor.MarkdownDocument);
        }

        private void ButtonRecentFiles_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            UpdateRecentDocumentsContextMenu();
        }

        private void LeftSidebarExpand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.Commands.OpenLeftSidebarPanelCommand.Execute(null);
        }

        private void RightSidebarExpand_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Model.WindowLayout.IsRightSidebarVisible = true;
            Model.WindowLayout.RightSidebarWidth = GridLengthHelper.FromInt(300);
        }

        private void MarkdownParserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mmApp.Configuration != null &&
                !string.IsNullOrEmpty(mmApp.Configuration.MarkdownOptions.MarkdownParserName))
            {
                MarkdownParserFactory.GetParser(parserAddinId: mmApp.Configuration.MarkdownOptions.MarkdownParserName,
                    forceLoad: true);
                PreviewBrowser?.PreviewMarkdownAsync();
            }
        }


        private void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(filesToOpen))
                {
                    var parms = StringUtils.GetLines(filesToOpen.Trim());

                    OpenFilesFromCommandLine(parms);
                    BindTabHeaders();
                }

                Topmost = true;

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                Activate();

                // restore out of band
                Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }), DispatcherPriority.ApplicationIdle);
            });
        }



        public List<MenuItem> GenerateContextMenuItemsFromOpenTabs(ContextMenu ctx = null)
        {
            var menuItems = new List<MenuItem>();
            var icons = new AssociatedIcons();
            var selectedTab = TabControl.SelectedItem as TabItem;

            var headers = TabControl.GetOrderedHeaders();
            foreach (var hd in headers)
            {
                var tab = hd.Content as TabItem;

                var doc = tab.Tag as MarkdownDocumentEditor;
                if (doc == null) continue;

                var filename = doc.MarkdownDocument.FilenamePathWithIndicator;
                var icon = icons.GetIconFromFile(doc.MarkdownDocument.Filename);

                var sp = new StackPanel {Orientation = Orientation.Horizontal};
                sp.Children.Add(new Image
                {
                    Source = icon,
                    Width = 16,
                    Height = 16,
                    Margin = new Thickness(0, 0, 20, 0)
                });
                sp.Children.Add(new TextBlock {Text = filename});

                var mi = new MenuItem();
                mi.Header = sp;
                mi.Command = Model.Commands.TabControlFileListCommand;
                mi.CommandParameter = doc.MarkdownDocument.Filename;
                if (tab == selectedTab)
                {
                    mi.FontWeight = FontWeights.Bold;
                    mi.Foreground = Brushes.SteelBlue;
                }

                menuItems.Add(mi);
            }

            return menuItems;
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
                    var window = win as MainWindow;
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

        //public void ShowStatus(string message = null, int milliSeconds = 0,
        //    FontAwesomeIcon icon = FontAwesomeIcon.None,
        //    Color color = default(Color),
        //    bool spin = false)
        //{


        //    ShowStatus(message, milliSeconds);
        //}

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

        /// <summary>
        /// Helper routine to show a Metro Dialog. Note this dialog popup is fully async!
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="style"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<MessageDialogResult> ShowMessageOverlayAsync(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative,
            MetroDialogSettings settings = null)
        {
            return await this.ShowMessageAsync(title, message, style, settings);
        }

        private void StatusZoomLevel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mmApp.Configuration.Editor.ZoomLevel = 100;
            Model.ActiveEditor?.RestyleEditor();
        }

        private void StatusZoomLevel_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var text = StatusZoomLevel.Text;
            text = text.Replace("%", "");
            if (int.TryParse(text, out int num))
            {
                Model.Configuration.Editor.ZoomLevel = num;
                Model.ActiveEditor?.RestyleEditor();
            }
        }

        private void StatusZoomLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var text = StatusZoomLevel.Text;
            text = text.Replace("%", "");

            if (int.TryParse(text, out int num))
                Model.Configuration.Editor.ZoomLevel = num;

            Model.ActiveEditor?.RestyleEditor();
        }

        #endregion


    }


    public class RecentDocumentListItem
    {
        public string Filename { get; set; }
        public string DisplayFilename { get; set; }
    }
}
