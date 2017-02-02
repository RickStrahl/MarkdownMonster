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
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Dragablz;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public AppModel Model { get; set; }

        //private string FileName;

        //private FileSystemWatcher openFileWatcher;

        private NamedPipeManager PipeManager { get; set; }

        public ApplicationConfiguration Configuration { get; set; }

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
        /// Handles WebBrowser configuration: DPI Awareness mainly!
        /// </summary>
        private WebBrowserHostUIHandler wbHandler;

        
        public MainWindow()
        {
            InitializeComponent();

            Model = new AppModel(this);
            DataContext = Model;

            InitializePreviewBrowser();

            TabControl.ClosingItemCallback = TabControlDragablz_TabItemClosing;

            Loaded += OnLoaded;
            Drop += MainWindow_Drop;
            AllowDrop = true;
                        
            KeyUp += MainWindow_KeyUp;
            Activated += OnActivated;

            // Singleton App startup - server code that listens for other instances
            if (mmApp.Configuration.UseSingleWindow)
            {
                // Listen for other instances launching and pick up
                // forwarded command line arguments
                PipeManager = new NamedPipeManager("MarkdownMonster");
                PipeManager.StartServer();
                PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
            }
            

            // Override some of the theme defaults (dark header specifically)
            mmApp.SetThemeWindowOverride(this);

            // Forces WebBrowser to be DPI aware and not display script errors
            wbHandler = new WebBrowserHostUIHandler(PreviewBrowser);            
        }

        #region Opening and Closing

        private void OnLoaded(object sender, RoutedEventArgs e)
        {            
            RestoreSettings();
            RecentDocumentsContextList();
            ButtonRecentFiles.ContextMenu = Resources["ContextMenuRecentFiles"] as ContextMenu;

            //AddinManager.Current.InitializeAddinsUi(this);

            Dispatcher.InvokeAsync(() =>
            {
                AddinManager.Current.InitializeAddinsUi(this);
            }, DispatcherPriority.ApplicationIdle);


            // Command Line Loading multiple files
            var args = Environment.GetCommandLineArgs();

            bool first = true;
            foreach(var fileArgs in args)
            {
                if (first)
                {
                    first = false;
                    continue;                    
                }

                var file = fileArgs;                                 
                if (!File.Exists(file))
                {
                    file= mmFileUtils.FixupDocumentFilename(file);
                    if (string.IsNullOrEmpty(file))
                        continue;                    
                }
                
                OpenTab(mdFile: file, batchOpen: true);
                AddRecentFile(file);                
            }


            if (mmApp.Configuration.FirstRun)
            {
              if (TabControl.Items.Count == 0)
              {
                    string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                    File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"),tempFile,true);            
                    OpenTab(tempFile);
                }
                mmApp.Configuration.FirstRun = false;
            }

            BindTabHeaders();

            if (mmApp.Configuration.IsPreviewVisible)
            {
                ButtonHtmlPreview.IsChecked = true;
                ToolButtonPreview.IsChecked = true;
                Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
            }

            Model.IsPresentationMode = mmApp.Configuration.OpenInPresentationMode;
            if (Model.IsPresentationMode)
            {
                Model.PresentationModeCommand.Execute(ToolButtonPresentationMode);
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

            Dispatcher.InvokeAsync(() => AddinManager.Current.RaiseOnWindowLoaded(), DispatcherPriority.ApplicationIdle);
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            WindowState = mmApp.Configuration.WindowState;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            mmApp.SetWorkingSet(10000000, 5000000);
        }

        protected void OnActivated(object sender, EventArgs e)
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

                        string filename = doc.FilenamePathWithIndicator.Replace("*","");
                        string template = filename +
                                          "\r\n\r\nThis file has been modified by another program.\r\nDo you want reload it?";
                        
                        if (MessageBox.Show(this,template,
                                "Reload",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            if (!doc.Load(doc.Filename))
                            {
                                MessageBox.Show(this, "Unable to re-load current document.",
                                    "Error re-loading file",
                                    MessageBoxButton.OK,MessageBoxImage.Exclamation);                                
                                continue;
                            }

                            dynamic pos = editor.AceEditor.getscrolltop(false);
                            editor.SetMarkdown(doc.CurrentText);
                            editor.AceEditor.updateDocumentStats(false);
                            if (pos > 0)
                                editor.AceEditor.setscrolltop(pos);

                            if (tab == selectedTab)
                                PreviewMarkdown(editor, keepScrollPosition: true);
                        }
                    }                    
                }

                // Ensure that user hasn't higlighted a MenuItem so the menu doesn't lose focus
                if (!MainMenu.Items.OfType<MenuItem>().Any(item => item.IsHighlighted))
                {
                    var selectedEditor = selectedTab.Tag as MarkdownDocumentEditor;
                    if (selectedEditor != null)
                    {
                        selectedEditor.WebBrowser.Focus();
                        selectedEditor.SetEditorFocus();
                        selectedEditor.RestyleEditor();
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);            
            
            Hide();

            AddinManager.Current.RaiseOnApplicationShutdown();

            bool isNewVersion = CheckForNewVersion(false, false);

            mmApp.Configuration.ApplicationUpdates.AccessCount++;            
            SaveSettings();

            if (!CloseAllTabs())
            {
                Show();
                e.Cancel = true;
                return;
            }

            if (mmApp.Configuration.UseSingleWindow)
            {
                PipeManager?.StopServer();

                if (App.Mutex != null)
                    App.Mutex.Dispose();
            }

            if (!isNewVersion &&  
                mmApp.Configuration.ApplicationUpdates.AccessCount % 5 == 0 &&
                !UnlockKey.IsRegistered())
            {
                Hide();
                var rd = new RegisterDialog();
                rd.Owner = this;
                rd.ShowDialog();
            }

            mmApp.SendTelemetry("shutdown");
            
            e.Cancel = false;            
        }

        public void AddRecentFile(string file)
        {
            mmApp.Configuration.AddRecentFile(file);
            RecentDocumentsContextList();
            mmApp.Configuration.LastFolder = Path.GetDirectoryName(file);

            mmApp.Configuration.Write();
        }

        /// <summary>
        /// Creates the Recent Items Context list
        /// </summary>        
        private void RecentDocumentsContextList()
        {            
            var context = Resources["ContextMenuRecentFiles"] as ContextMenu;
            if (context == null)
                return;

            context.Items.Clear();
            ButtonRecentFiles.Items.Clear();

            List<string> badFiles = new List<string>();
            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                if (!File.Exists(file))
                {
                    badFiles.Add(file);
                    continue;
                }
                var mi = new MenuItem()
                {
                    Header = file,                    
                };

                mi.Click += (object s, RoutedEventArgs ev) =>
                {
                    OpenTab(file,rebindTabHeaders:true);
                    AddRecentFile(file);
                };
                context.Items.Add(mi);

                var mi2 = new MenuItem()
                {
                    Header = file,
                };
                mi2.Click += (object s, RoutedEventArgs ev) => OpenTab(file,rebindTabHeaders:true);
                ButtonRecentFiles.Items.Add(mi2);
            }
            ToolbarButtonRecentFiles.ContextMenu = context;

            foreach (var file in badFiles)
                mmApp.Configuration.RecentDocuments.Remove(file);
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

            if (mmApp.Configuration.RememberLastDocuments > 0)
            {
                var selectedDoc = conf.OpenDocuments.FirstOrDefault(dc => dc.IsActive);
                TabItem selectedTab = null;

                int counter = 0;

                // since docs are inserted at the beginning we need to go in reverse
                foreach (var doc in conf.OpenDocuments.Reverse<MarkdownDocument>())
                {                    
                    if (File.Exists(doc.Filename))
                    {
                        var tab = OpenTab(doc.Filename,selectTab: false, batchOpen: true);
                        if (tab == null)
                            continue;

                        if (selectedDoc != null && selectedDoc.Filename == doc.Filename)                        
                            selectedTab = tab;                        
                    }

                    counter++;
                    if (counter >= mmApp.Configuration.RememberLastDocuments)
                        break;
                }

                if (selectedTab != null)
                    TabControl.SelectedItem = selectedTab;
                else
                    TabControl.SelectedIndex = 0;
            }

            Model.IsPreviewBrowserVisible = mmApp.Configuration.IsPreviewVisible;
            Model.PreviewBrowserCommand.Execute(null);
        }

        void SaveSettings()
        {
            var config = mmApp.Configuration;
            config.IsPreviewVisible = Model.IsPreviewBrowserVisible;

            if (WindowState == WindowState.Normal)
            {
                config.WindowPosition.Left = Convert.ToInt32(Left);
                config.WindowPosition.Top = Convert.ToInt32(Top);
                config.WindowPosition.Width = Convert.ToInt32(Width);
                config.WindowPosition.Height = Convert.ToInt32(Height);
                config.WindowPosition.SplitterPosition = Convert.ToInt32(ContentGrid.ColumnDefinitions[2].Width.Value);
            }

            if (WindowState != WindowState.Minimized)
                config.WindowState = WindowState;

            config.OpenDocuments.Clear();

            if (mmApp.Configuration.RememberLastDocuments > 0)
            {                
                mmApp.Configuration.OpenDocuments.Clear();

                foreach (var item in TabControl.GetOrderedHeaders())
                {
                    var tab = item.Content as TabItem;
                    var doc = tab.Tag as MarkdownDocumentEditor;
                    if (doc != null)
                        config.OpenDocuments.Add(doc.MarkdownDocument);
                }
            }
            config.Write();
        }

        public bool SaveFile()
        {
            var tab = TabControl.SelectedItem as TabItem;
            if (tab == null)
                return false;

            var md = tab.Content;
            var editor = tab.Tag as MarkdownDocumentEditor;
            var doc = editor?.MarkdownDocument;
            if (doc == null)
                return false;

            if (!editor.SaveDocument())
            {
                //var res = await this.ShowMessageOverlayAsync("Unable to save Document",
                //    "Unable to save document most likely due to missing permissions.");

                MessageBox.Show("Unable to save document most likely due to missing permissions.", mmApp.ApplicationName);
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
        /// </param>
        /// <returns></returns>
        public TabItem OpenTab(string mdFile = null, 
                               MarkdownDocumentEditor editor = null,
                               bool showPreviewIfActive = false, 
                               string syntax = "markdown", 
                               bool selectTab = true, 
                               bool rebindTabHeaders = false,
                               bool batchOpen = false)
        {
            if (mdFile != null && mdFile!= "untitled" && (!File.Exists(mdFile) ||
                                  !AddinManager.Current.RaiseOnBeforeOpenDocument(mdFile)))
                return null;

            var tab = new TabItem();
                      
            tab.Margin = new Thickness(0, 0, 3, 0);
            tab.Padding = new Thickness(2, 0, 7, 2);            
            tab.Background = Background;

            ControlsHelper.SetHeaderFontSize(tab, 13F);

            var wb = new WebBrowser
            {                
                Visibility = Visibility.Hidden,
                Margin = new Thickness(-1,0,0,0)
            };
            
            tab.Content = wb;

            

            if (editor == null)
            {
                editor = new MarkdownDocumentEditor(wb)
                {
                    Window = this,
                    EditorSyntax = syntax                    
                };
                
                var doc = new MarkdownDocument()
                {
                    Filename = mdFile ?? "untitled",
                    Dispatcher = Dispatcher
                };
                if (doc.Filename != "untitled")
                {                                        
                    doc.Filename = mmFileUtils.GetPhysicalPath(doc.Filename);

                    if (doc.HasBackupFile())
                    {
                                ShowStatus("Auto-save recovery files have been found and opened in the editor.", milliSeconds: 9000);
                                SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                                {
                                    File.Copy(doc.BackupFilename, doc.BackupFilename + ".md");
                                    OpenTab(doc.BackupFilename + ".md");
                                    File.Delete(doc.BackupFilename + ".md");
                                }                     
                    }
                    
                    

                    if (!doc.Load())
                    {
                        if (!batchOpen)
                            MessageBox.Show(
                                $"Unable to load {doc.Filename}.\r\n\r\nMost likely you don't have access to the file.",
                                "File Open Error - " + mmApp.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning);

                        return null;
                    }
                }

                doc.PropertyChanged += (sender, e) =>
                {                    
                    if (e.PropertyName == "IsDirty")
                        CommandManager.InvalidateRequerySuggested();
                };
                editor.MarkdownDocument = doc;
                
                SetTabHeaderBinding(tab, doc, "FilenameWithIndicator");
                
                tab.ToolTip = doc.Filename;                
            }
            
            var filename = Path.GetFileName(editor.MarkdownDocument.Filename);
            tab.Tag = editor;

            Title = filename;

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

                if (showPreviewIfActive && PreviewBrowser.Width > 5)
                    PreviewMarkdown(); //Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
            }

            AddinManager.Current.RaiseOnAfterOpenDocument(editor.MarkdownDocument);

            if (rebindTabHeaders)
                BindTabHeaders();


            return tab;
        }


        /// <summary>
        /// Binds all Tab Headers
        /// </summary>        
        void BindTabHeaders()
        {
            var tabList = new List<TabItem>();
            foreach (TabItem tb in TabControl.Items)
                tabList.Add(tb);

            var tabItems = tabList
                .Select(tb => Path.GetFileName(((MarkdownDocumentEditor) tb.Tag).MarkdownDocument.Filename.ToLower()))
                    .GroupBy(fn => fn)
                    .Select(tbCol => new {
                        Filename = tbCol.Key,
                        Count = tbCol.Count()
                    });

            foreach (TabItem tb in TabControl.Items)
            {
                var doc = ((MarkdownDocumentEditor) tb.Tag).MarkdownDocument;

                if (tabItems.Where(
                    ti =>
                        ti.Filename == Path.GetFileName(doc.Filename.ToLower()) &&
                        ti.Count > 1).Any())

                    SetTabHeaderBinding(tb, doc, "FilenamePathWithIndicator");
                else
                    SetTabHeaderBinding(tb, doc, "FilenameWithIndicator");
            }
        }

        /// <summary>
        /// Binds the tab header to an expression
        /// </summary>
        /// <param name="tab"></param>   
        /// <param name="bindingSource"></param>     
        /// <param name="propertyPath"></param>
        private void SetTabHeaderBinding(TabItem tab, object bindingSource, string propertyPath = "FilenameWithIndicator")
        {
            var headerBinding = new Binding
            {
                Source = bindingSource,
                Path = new PropertyPath(propertyPath),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(tab, HeaderedContentControl.HeaderProperty, headerBinding);
        }

        private bool CloseAllTabs(TabItem allExcept = null)
        {            
            for (int i = TabControl.Items.Count - 1; i > -1 ; i--)
            {                
                var tab = TabControl.Items[i] as TabItem;                

                if (tab != null)
                {
                    if (allExcept != null && tab == allExcept)
                        continue;

                    if (!CloseTab(tab,rebindTabHeaders:false))
                        return false;
                }
            }
            return true;
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
        public bool CloseTab(TabItem tab, bool rebindTabHeaders = true)
        {
            if (tab == null)
                return false;

            var editor = tab.Tag as MarkdownDocumentEditor;
            if (editor == null)
                return false;

            bool returnValue = true;

            var doc = editor.MarkdownDocument;

            if (!string.IsNullOrEmpty(doc.HtmlRenderFilename) && File.Exists(doc.HtmlRenderFilename))
                File.Delete(doc.HtmlRenderFilename);

            doc.CleanupBackupFile();

            if (doc.IsDirty)
            {
                var res = MessageBox.Show(Path.GetFileName(doc.Filename) + "\r\n\r\nhas been modified.\r\n"  +
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
                        Model.SaveAsCommand.Execute(ButtonSaveAsFile);
                    else if (!SaveFile())
                        returnValue = false;
                }
            }
            
            tab.Tag = null;            
            TabControl.Items.Remove(tab);
            
            if (TabControl.Items.Count == 0)
            {
                PreviewBrowser.Visibility = Visibility.Hidden;
                PreviewBrowser.Navigate("about:blank");
                Model.ActiveDocument = null;
                Title = "Markdown Monster" + 
                        (UnlockKey.Unlocked ? "" : " (unregistered)");
            }

            if (rebindTabHeaders)
                BindTabHeaders();

            return returnValue; // close
        }
        

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            if (mmApp.Configuration.IsPreviewVisible)
                PreviewMarkdown();
            

            Title = editor.MarkdownDocument.FilenameWithIndicator.Replace("*","") + 
                "  - Markdown Monster" + 
                (UnlockKey.Unlocked ? "" : " (unregistered)");
            
            foreach (var doc in Model.OpenDocuments)
                doc.IsActive = false;

            Model.ActiveDocument = editor.MarkdownDocument;
            Model.ActiveDocument.IsActive = true;

            AddRecentFile(Model.ActiveDocument?.Filename);

            AddinManager.Current.RaiseOnDocumentActivated(Model.ActiveDocument);

            Model.ActiveEditor.RestyleEditor();

            editor.WebBrowser.Focus();
            editor.SetEditorFocus();            
        }

        //[Obsolete("This is old the code from the MetroTabControl")]
        //private void TabControl_TabItemClosing(object sender, BaseMetroTabControl.TabItemClosingEventArgs e)
        //{
        //    var tab = e.ClosingTabItem as TabItem;
        //    if (tab == null)
        //        return;

        //    e.Cancel = !CloseTab(tab);
        //}

        
        private void TabControlDragablz_TabItemClosing(ItemActionCallbackArgs<TabablzControl> e)
        {            
            var tab =  e.DragablzItem.DataContext as TabItem;
            if (tab == null)
                return;

            if (!CloseTab(tab))
                e.Cancel();            
        }

        #endregion

        #region Worker Functions

        /// <summary>
        /// Shows or hides the preview browser
        /// </summary>
        /// <param name="hide"></param>
        public void ShowPreviewBrowser(bool hide = false, bool refresh = false)
        {
            if (!hide)
            {
                PreviewBrowser.Visibility = Visibility.Visible;
               
                ContentGrid.ColumnDefinitions[1].Width = new GridLength(12);
                if (!refresh)
                {
                    if (mmApp.Configuration.WindowPosition.SplitterPosition < 100)
                        mmApp.Configuration.WindowPosition.SplitterPosition = 600;

                    if(!Model.IsPresentationMode)
                        ContentGrid.ColumnDefinitions[2].Width =
                            new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);
                }
            }
            else
            {          
                if (ContentGrid.ColumnDefinitions[2].Width.Value > 100)
                    mmApp.Configuration.WindowPosition.SplitterPosition = Convert.ToInt32(ContentGrid.ColumnDefinitions[2].Width.Value);
                
                ContentGrid.ColumnDefinitions[1].Width = new GridLength(0);
                ContentGrid.ColumnDefinitions[2].Width = new GridLength(0);

                PreviewBrowser.Navigate("about:blank");                
            }
        }

        // IMPORTANT: for browser COM CSE errors which can happen with script errors
        [HandleProcessCorruptedStateExceptions]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false)
        {            
            try
            {
                // only render if the preview is actually visible and rendering in Preview Browser
                if (!Model.IsPreviewBrowserVisible && !showInBrowser)
                    return;

                if (editor == null)
                    editor = GetActiveMarkdownEditor();

                if (editor == null)
                    return;

                var doc = editor.MarkdownDocument;
                var ext = Path.GetExtension(doc.Filename).ToLower().Replace(".", "");

                string renderedHtml = null;

                if (string.IsNullOrEmpty(ext) || ext == "md" || ext=="markdown" || ext == "html" || ext == "htm")
                {
                    dynamic dom = null;
                    if (!showInBrowser)
                    {
                        if (keepScrollPosition)
                        {
                            dom = PreviewBrowser.Document;
                            editor.MarkdownDocument.LastBrowserScrollPosition = dom.documentElement.scrollTop;
                        }
                        else
                        {
                            ShowPreviewBrowser(false, false);
                            editor.MarkdownDocument.LastBrowserScrollPosition = 0;
                        }
                    }

                    if (ext == "html" || ext == "htm")
                    {
                        if (!editor.MarkdownDocument.WriteFile(editor.MarkdownDocument.HtmlRenderFilename,
                                editor.MarkdownDocument.CurrentText))
                            // need a way to clear browser window
                            return;
                    }
                    else
                    {
                        renderedHtml = editor.MarkdownDocument.RenderHtmlToFile(usePragmaLines: !showInBrowser &&                                                                                
                                                                                                mmApp.Configuration
                                                                                                    .PreviewSyncMode !=
                                                                                                PreviewSyncMode.None,
                                                                                                renderLinksExternal: mmApp.Configuration.RenderLinksExternal);
                        if (renderedHtml == null)
                        {
                            SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red, false);
                            ShowStatus($"Access denied: {Path.GetFileName(editor.MarkdownDocument.Filename)}", 5000);
                            // need a way to clear browser window

                            return;
                        }

                        renderedHtml = StringUtils.ExtractString(renderedHtml,
                            "<!-- Markdown Monster Content -->",
                            "<!-- End Markdown Monster Content -->");
                    }

                    if (showInBrowser)
                    {
                        ShellUtils.GoUrl(editor.MarkdownDocument.HtmlRenderFilename);
                        return;
                    }
                    else
                    {
                        PreviewBrowser.Cursor = Cursors.None;
                        PreviewBrowser.ForceCursor = true;

                        // if content contains <script> tags we must do a full page refresh
                        bool forceRefresh = renderedHtml != null && renderedHtml.Contains("<script ");

                        
                        if (keepScrollPosition && !mmApp.Configuration.AlwaysUsePreviewRefresh && !forceRefresh)
                        {
                            string browserUrl = PreviewBrowser.Source.ToString().ToLower();
                            string documentFile = "file:///" +
                                                  editor.MarkdownDocument.HtmlRenderFilename.Replace('\\', '/')
                                                      .ToLower();
                            if (browserUrl == documentFile)
                            {
                                dom = PreviewBrowser.Document;
                                //var content = dom.getElementById("MainContent");


                                if (string.IsNullOrEmpty(renderedHtml))
                                    PreviewMarkdown(editor, false, false); // fully reload document
                                else
                                {
                                    try
                                    {
                                        // explicitly update the document with JavaScript code
                                        // much more efficient and non-jumpy and no wait cursor
                                        var window = dom.parentWindow;
                                        window.updateDocumentContent(renderedHtml);

                                        try
                                        {
                                            // scroll preview to selected line
                                            if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
                                                mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                                            {
                                                int lineno = editor.GetLineNumber();
                                                if (lineno > -1)
                                                    window.scrollToPragmaLine(lineno);
                                            }
                                        }
                                        catch {/* ignore scroll error */}
                                    }
                                    catch (Exception ex)
                                    {
                                        // Refresh doesn't fire Navigate event again so 
                                        // the page is not getting initiallized properly
                                        //PreviewBrowser.Refresh(true);
                                        PreviewBrowser.Tag = "EDITORSCROLL";
                                        PreviewBrowser.Navigate(editor.MarkdownDocument.HtmlRenderFilename);
                                    }
                                }

                                return;
                            }
                        }

                        PreviewBrowser.Tag = "EDITORSCROLL";
                        PreviewBrowser.Navigate(editor.MarkdownDocument.HtmlRenderFilename);
                        
                        return;
                    }
                }

                // not a markdown or HTML document to preview
                ShowPreviewBrowser(true, keepScrollPosition);
            }
            catch (Exception ex)
            {
                mmApp.Log("PreviewMarkdown failed (Exception captured - continuing)", ex);
            }
        }

        
        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false)
        {
            if (!mmApp.Configuration.IsPreviewVisible)
                return;

            var current = DateTime.UtcNow;
                    
            // prevent multiple stacked refreshes
            if (invoked == DateTime.MinValue) // || current.Subtract(invoked).TotalMilliseconds > 4000)
            {
                invoked = current;
                
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(() => {
                        try
                        {
                            PreviewMarkdown(editor, keepScrollPosition);
                        }
                        finally
                        {
                            invoked = DateTime.MinValue;
                        }                    
                    }));                
            }
        }



        public MarkdownDocumentEditor GetActiveMarkdownEditor()
        {
            var tab = TabControl?.SelectedItem as TabItem;            
            return tab?.Tag as MarkdownDocumentEditor;
        }

        bool CheckForNewVersion(bool force, bool closeForm = true, int timeout = 1500)
        {
            var updater = new ApplicationUpdater(typeof(MainWindow));
            bool isNewVersion = updater.IsNewVersionAvailable(!force,timeout: timeout);
            if (isNewVersion)
            {
                var res = MessageBox.Show(updater.VersionInfo.Detail + "\r\n\r\n" +
                                          "Do you want to download and install this version?",
                    updater.VersionInfo.Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (res == MessageBoxResult.Yes)
                {
                    ShellUtils.GoUrl(mmApp.InstallerDownloadUrl);

                    if (closeForm)
                        Close();
                }
            }
            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;

            return isNewVersion;
        }
       
        #endregion
        
        #region Button Handlers

        public async void Button_Handler(object sender, RoutedEventArgs e)
        {

            var button = sender;
            if (sender == null)
                return;

            //if (button == ButtonOpenFile || button == ToolButtonOpenFile)
            //{                
            //    var fd = new OpenFileDialog
            //    {
            //        DefaultExt = ".md",
            //        Filter = "Markdown files (*.md)|*.md|" +
            //                 "Html files (*.htm,*.html)|*.htm;*.html|" +
            //                 "Javascript files (*.js)|*.js|" +
            //                 "Typescript files (*.ts)|*.ts|" +
            //                 "Json files (*.json)|*.json|" +
            //                 "Css files (*.css)|*.css|" +
            //                 "Xml files (*.xml,*.config)|*.xml;*.config|" +
            //                 "C# files (*.cs)|*.cs|" +
            //                 "C# Razor files (*.cshtml)|*.cshtml|" +
            //                 "Foxpro files (*.prg)|*.prg|" +
            //                 "Powershell files (*.ps1)|*.ps1|" +
            //                 "Php files (*.php)|*.php|" +
            //                 "Python files (*.py)|*.py|" +
            //                 "All files (*.*)|*.*",
            //        CheckFileExists = true,
            //        RestoreDirectory = true,
            //        Multiselect = true,
            //        Title = "Open Markdown File"
            //    };

            //    if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
            //        fd.InitialDirectory = mmApp.Configuration.LastFolder;

            //    var res = fd.ShowDialog();
            //    if (res == null || !res.Value)
            //        return;

            //    OpenTab(fd.FileName, rebindTabHeaders: true);                

            //    AddRecentFile(fd.FileName);
            //}


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
                PreviewMarkdown();
            }
            //else if (button == ButtonNewFile || button == ToolButtonNewFile)
            //{
            //    OpenTab("untitled");
            //}
            else if (button == ButtonNewWeblogPost)
            {
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            }
            else if (button == ButtonExit)
            {
                Close();
            }
            else if (button == MenuAddinManager)
            {
                var form = new AddinManagerWindow();
                form.Owner = this;
                form.Show();
            }
            else if (button == MenuOpenConfigFolder)
            {
                ShellUtils.GoUrl(mmApp.Configuration.CommonFolder);
            }
            else if (button == MenuOpenPreviewFolder)
            {
                ShellUtils.GoUrl(Path.Combine(Environment.CurrentDirectory, "PreviewThemes", mmApp.Configuration.RenderTheme));
            }
            else if (button == MenuMarkdownMonsterSite)
            {
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com");
            }
            else if (button == MenuBugReport)
            {
                ShellUtils.GoUrl("https://github.com/RickStrahl/MarkdownMonster/issues");
            }
            else if (button == MenuCheckNewVersion)
            {
                ShowStatus("Checking for new version...");
                if (!CheckForNewVersion(true, timeout: 5000))
                {
                    ShowStatus("Your version of Markdown Monster is up to date.", 6000);
                    SetStatusIcon(FontAwesomeIcon.Check, Colors.Green);

                    MessageBox.Show("Your version of Markdown Monster is v" + mmApp.GetVersion() + " and you are up to date.",
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
            else if (button == ButtonRefreshBrowser)
            {
                var editor = GetActiveMarkdownEditor();
                if (editor == null)
                    return;

                this.PreviewMarkdownAsync();
            }
            else if (button == MenuDocumentation)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs");
            else if(button == MenuMarkdownBasics)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4ne1eu2cq.htm");
            else if (button == MenuCreateAddinDocumentation)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm");
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
        }


        private void ButtonCloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            TabItem except = null;

            var menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Name == "MenuCloseAllButThisTab")
                except = TabControl.SelectedItem as TabItem;

            CloseAllTabs(except);
            BindTabHeaders();
        }



        private void ButtonSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (TabItem tab in TabControl.Items)
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                editor?.RestyleEditor();
            }
        }

        public void ButtonViewInBrowser_Click(object sender, RoutedEventArgs e)
        {
            PreviewMarkdown(showInBrowser: true);                      
        }

        private void Button_CommandWindow(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);
            Process.Start("cmd.exe","/k \"cd " + path + "\"");
        }

        private void Button_OpenExplorer(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;            
            Process.Start("explorer.exe","/select,\"" +  editor.MarkdownDocument.Filename + "\"");            
        }

        internal void Button_PasteMarkdownFromHtml(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            string html = null;
            if( Clipboard.ContainsText(TextDataFormat.Html))
                html = Clipboard.GetText(TextDataFormat.Html);

            if (!string.IsNullOrEmpty(html))
                html=StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
            else
                html = Clipboard.GetText();

            if (string.IsNullOrEmpty(html))
                return;

            var markdown = MarkdownUtilities.HtmlToMarkdown(html);

            editor.SetSelection(markdown);
            editor.SetEditorFocus();            
            PreviewMarkdownAsync(editor,true);
        }

        internal void Button_CopyMarkdownAsHtml(object sender, RoutedEventArgs e)
        {
            var editor = GetActiveMarkdownEditor();
            if (editor == null)
                return;

            var markdown = editor.GetSelection();
            var html = editor.RenderMarkdown(markdown);

            if (!string.IsNullOrEmpty(html))
            {
                Clipboard.SetText(html);
                ShowStatus("Html has been pasted to the clipboard.", 4000);                
            }
            editor.SetEditorFocus();
            editor.Window.PreviewMarkdownAsync();
        }


        #endregion

        #region Miscelleaneous Events

        /// <summary>
        /// Key handler used to intercept special menu hotkeys fired from
        /// editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            //bool isControlKey = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            //if (e.Key == Key.N && isControlKey)
            //{
            //    e.Handled = true;
            //    Button_Handler(ButtonNewFile, null);
            //}
            //if (e.Key == Key.O && isControlKey)
            //{
            //    e.Handled = false;
            //    Button_Handler(ButtonOpenFile, null);
            //}

        }

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
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (File.Exists(file))
                        OpenTab(file,rebindTabHeaders:true);
                }
            }
        }

        private void PreviewBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 100)
            {
                int width = Convert.ToInt32(ContentGrid.ColumnDefinitions[2].Width.Value);
                if (width > 100)
                    mmApp.Configuration.WindowPosition.SplitterPosition = width;                                   
            }
        }

        private void EditorTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (TabItem tab in TabControl.Items)
            {
                var editor = tab.Tag as MarkdownDocumentEditor;
                editor?.RestyleEditor();
            }

            PreviewMarkdownAsync();
        }

        private void RenderTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PreviewMarkdownAsync();
        }


        private void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            Dispatcher.Invoke(() =>
            {                
                if (!string.IsNullOrEmpty(filesToOpen))
                {                    
                    var parms = StringUtils.GetLines(filesToOpen);

                    TabItem lastTab = null;
                    foreach (var file in parms)
                    {
                        if (!string.IsNullOrEmpty(file))
                           lastTab = OpenTab(file.Trim());                       
                    }
                    if (lastTab != null)
                        Dispatcher.InvokeAsync(() => TabControl.SelectedItem = lastTab);
                    BindTabHeaders();
                }

                Topmost = true;

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                Activate();

                // restore out of band
                Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }));
            });
        }
        #endregion

        #region StatusBar Display

        private Timer statusTimer;

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
            {
                message = "Ready";
                SetStatusIcon();
            }

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                statusTimer?.Dispose();
                statusTimer = new Timer((object win) =>
                {
                    var window = win as MainWindow;                    
                    window.Dispatcher.Invoke(() => {  window.ShowStatus(null, 0); } );
                },this,milliSeconds,Timeout.Infinite);
            }
            WindowUtilities.DoEvents();
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
                StatusIcon.SpinDuration = 30;                
            
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
            MetroDialogSettings settings = null )
        {
            return await this.ShowMessageAsync(title, message, style, settings);
        }

        #endregion

        #region Preview Browser Operation
        
        private void InitializePreviewBrowser()
        {
            // wbhandle has additional browser initialization code
            // using the WebBrowserHostUIHandler
            PreviewBrowser.LoadCompleted += PreviewBrowserOnLoadCompleted;
            //PreviewBrowser.Navigate("about:blank");
        }


        private void PreviewBrowserOnLoadCompleted(object sender, NavigationEventArgs e)
        {
            string url = e.Uri.ToString();
            if (url.Contains("about:blank") || !url.Contains("_MarkdownMonster_Preview") )
                return;

            bool shouldScrollToEditor = PreviewBrowser.Tag != null && PreviewBrowser.Tag.ToString() == "EDITORSCROLL";
            PreviewBrowser.Tag = null;

            dynamic window = null;
            MarkdownDocumentEditor editor = null;
            try
            {
                editor = GetActiveMarkdownEditor();
                dynamic dom = PreviewBrowser.Document;
                window = dom.parentWindow;
                dom.documentElement.scrollTop = editor.MarkdownDocument.LastBrowserScrollPosition;

                window.initializeinterop(editor);

                if (shouldScrollToEditor)
                {
                    try
                    {
                        // scroll preview to selected line
                        if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview || mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                        {
                            int lineno = editor.GetLineNumber();
                            if (lineno > -1)
                                window.scrollToPragmaLine(lineno);
                        }
                    }
                    catch
                    {
                        /* ignore scroll error */
                    }
                }
            }
            catch
            {
                // try again
                Task.Delay(500).ContinueWith(t =>
                {
                    try
                    {
                        window.initializeinterop(editor);
                    }
                    catch (Exception ex)
                    {
                        //mmApp.Log("Preview InitializeInterop failed: " + url, ex);
                    }
                });
            }
        }

        #endregion
    }

}
