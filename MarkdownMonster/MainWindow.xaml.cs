using System;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
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

        private string FileName;

        //private FileSystemWatcher openFileWatcher;

        private NamedPipeManager PipeManager { get; set; }

        public ApplicationConfiguration Configuration { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Model = new AppModel(this);
            DataContext = Model;

            Loaded += OnLoaded;
            Closing += MainWindow_Closing;

            Drop += MainWindow_Drop;
            AllowDrop = true;

            KeyUp += MainWindow_KeyUp;

            PreviewBrowser.Navigated += (sender, e) =>
            {
                // No Script Errors
                NoScriptErrors(PreviewBrowser, true);
            };
            PreviewBrowser.LoadCompleted += (sender, e) =>
            {
                if (e.Uri.ToString().Contains("about:blank"))
                    return;

                var editor = GetActiveMarkdownEditor();
                dynamic dom = PreviewBrowser.Document;                
                dom.documentElement.scrollTop = editor.MarkdownDocument.LastBrowserScrollPosition;

                if (File.Exists(editor.MarkdownDocument.HtmlRenderFilename))
                    File.Delete(editor.MarkdownDocument.HtmlRenderFilename);
            };
            PreviewBrowser.Navigate("about:blank");

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
        }


        /// <summary>
        /// Key handler used to intercept special menu hotkeys fired from
        /// editor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            bool isControlKey = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (e.Key == Key.N && isControlKey)
            {
                e.Handled = true;                
                Button_Handler(ButtonNewFile, null);
            }
            else if (e.Key == Key.O && isControlKey)
            {
                e.Handled = false;
                Button_Handler(ButtonOpenFile, null);                
            }
        }




        #region Opening and Closing

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RestoreSettings();
            RecentDocumentsContextList();
            ButtonRecentFiles.ContextMenu = Resources["ContextMenuRecentFiles"] as ContextMenu;

            AddinManager.Current.InitializeAddinsUi(this);

            // Command Line Loading of a single file
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                OpenTab(mdFile: args[1]);
                mmApp.Configuration.AddRecentFile(args[1]);
            }

            if (mmApp.Configuration.IsPreviewVisible)
            {
                ButtonHtmlPreview.IsChecked = true;
                ToolButtonPreview.IsChecked = true;
                Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
            }

            var left = Left;
            Left = 300000;

            // force controls to realign - required because of WebBrowser control weirdness            
            this.Dispatcher.InvokeAsync(() =>
            {
                TabControl.InvalidateVisual();
                Left = left;
            });            
        }


        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            this.Hide();

            bool isNewVersion = CheckForNewVersion(false, false);
            mmApp.Configuration.ApplicationUpdates.AccessCount++;
            

            SaveSettings();

            if (!CloseAllTabs())
            {
                this.Show();
                e.Cancel = true;
                return;
            }

            PipeManager?.StopServer();
            App.Mutex.Dispose();            

            if (!isNewVersion &&  
                mmApp.Configuration.ApplicationUpdates.AccessCount % 3 == 0 &&
                !UnlockKey.IsRegistered())
            {
                this.Hide();

                var rd = new RegisterDialog();
                rd.Owner = this;
                rd.ShowDialog();
            }

            

           e.Cancel = false;            
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

            if (mmApp.Configuration.RememberOpenFiles)
            {
                var selectedDoc = conf.OpenDocuments.FirstOrDefault(dc => dc.IsActive);
                MetroTabItem selectedTab = null;

                // since docs are inserted at the beginning we need to go in reverse
                foreach (var doc in conf.OpenDocuments.Reverse<MarkdownDocument>())
                {
                    if (File.Exists(doc.Filename))
                    {
                        var tab = OpenTab(doc.Filename,selectTab: false);

                        if (selectedDoc != null && selectedDoc.Filename == doc.Filename)                        
                            selectedTab = tab;                        
                    }
                }

                if (selectedTab != null)
                    TabControl.SelectedItem = selectedTab;
            }

            Model.IsPreviewBrowserVisible = mmApp.Configuration.IsPreviewVisible;
            Model.PreviewBrowserCommand.Execute(null);
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
            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                var mi = new MenuItem()
                {
                    Header = file,                    
                };

                mi.Click += (object s, RoutedEventArgs ev) => OpenTab(file);
                context.Items.Add(mi);
            }
            ToolbarButtonRecentFiles.ContextMenu = context;


            ButtonRecentFiles.Items.Clear();
            foreach (string file in mmApp.Configuration.RecentDocuments)
            {
                var mi = new MenuItem()
                {
                    Header = file,
                };
                mi.Click += (object s, RoutedEventArgs ev) => OpenTab(file);
                ButtonRecentFiles.Items.Add(mi);
            }
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

            config.OpenDocuments.Clear();

            if (mmApp.Configuration.RememberOpenFiles)
            {
                foreach (TabItem tab in TabControl.Items)
                {
                    var doc = tab.Tag as MarkdownDocumentEditor;
                    if (doc != null)
                        config.OpenDocuments.Add(doc.MarkdownDocument);
                }
            }
            config.Write();
        }
        #endregion

        #region Tab Handling
        public MetroTabItem OpenTab(string mdFile = null, MarkdownDocumentEditor editor = null, bool showPreviewIfActive = false, string syntax = "markdown", bool selectTab = true)
        {
            if (mdFile != null && mdFile!= "untitled" && !File.Exists(mdFile))
                return null;

            var tab = new MetroTabItem();

            tab.CloseButtonEnabled = true;
            tab.CloseTabCommand = Model.TabItemClosedCmd;                        
            tab.Margin = new Thickness(0, 0, 3, 0);
            tab.Padding = new Thickness(2, 0, 7, 2);
            tab.Background = this.Background;
            tab.ContextMenu = this.Resources["TabItemContextMenu"] as ContextMenu;
            
            ControlsHelper.SetHeaderFontSize(tab, 13F);

            var wb = new WebBrowser
            {
                AllowDrop = false,
                Visibility = Visibility.Hidden
            };

            tab.Content = wb;            
            
            if (editor == null)
            {
                dynamic dom = wb.Document;
                editor = new MarkdownDocumentEditor(wb)
                {
                    Window = this,
                    EditorSyntax = syntax
                };

                var doc = new MarkdownDocument
                {
                    Filename = mdFile ?? @"c:\temp\readme.md"
                };
                if (FileName != "untitled") 
                    doc.Load();

                doc.PropertyChanged += (sender, e) =>
                {                    
                    if (e.PropertyName == "IsDirty")
                        CommandManager.InvalidateRequerySuggested();
                };
                editor.MarkdownDocument = doc;

                
                var headerBinding = new Binding
                {
                    Source = doc,
                    Path = new PropertyPath("FilenameWithIndicatorNoAccellerator"),                    
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(tab, MetroTabItem.HeaderProperty, headerBinding);

                tab.ToolTip = doc.Filename;
            }

            var filename = Path.GetFileName(editor.MarkdownDocument.Filename);
            tab.Tag = editor;

            Title = filename ;

            editor.LoadDocument();
            
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

            return tab;
        }

        private bool CloseAllTabs()
        {
            for (int i = TabControl.Items.Count - 1; i > -1 ; i--)
            {
                var tab = TabControl.Items[i] as TabItem;
                if (tab != null)
                {
                    if (!CloseTab(tab))
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
        /// <returns>true if tab can close, false if it should stay open</returns>
        public bool CloseTab(TabItem tab)
        {
            if (tab == null)
                return false;

            var editor = GetActiveMarkdownEditor();
            bool returnValue = true;

            var doc = editor.MarkdownDocument;
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
                else if (res == MessageBoxResult.No)
                {
                    // close but don't save 
                }
                else
                {
                    if (doc.Filename == "untitled")
                        Model.SaveAsCommand.Execute(ButtonSaveAsFile);
                    else
                        SaveFile();

                    returnValue = true;
                }
            }

            tab.Tag = null;
            editor = null;
            TabControl.Items.Remove(tab);

            if (TabControl.Items.Count == 0)
            {
                PreviewBrowser.Visibility = Visibility.Hidden;
                PreviewBrowser.Navigate("about:blank");
            }

            return true; // close
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

            Model.ActiveDocument = editor.MarkdownDocument;

            foreach (var doc in Model.OpenDocuments)
                doc.IsActive = false;

            Model.ActiveDocument.IsActive = true;
        }

        private void TabControl_TabItemClosing(object sender, BaseMetroTabControl.TabItemClosingEventArgs e)
        {
            var tab = e.ClosingTabItem as TabItem;
            if (tab == null)
                return;

            e.Cancel = !CloseTab(tab);
        }

        #endregion

        #region Worker Functions

        public void SaveFile()
        {
            var tab = TabControl.SelectedItem as TabItem;
            if (tab == null)
                return;

            var md = tab.Content;
            var editor = tab.Tag as MarkdownDocumentEditor;
            editor.SaveDocument();            
        }


        /// <summary>
        /// Shows or hides the preview browser
        /// </summary>
        /// <param name="hide"></param>
        public void ShowPreviewBrowser(bool hide = false)
        {
            if (!hide)
            {
                PreviewBrowser.Visibility = Visibility.Visible;
                //var editor = GetActiveMarkdownEditor();
                //if (editor != null)
                //    PreviewMarkdown(editor);

                var splitterPos = mmApp.Configuration.WindowPosition.SplitterPosition;
                if (splitterPos < 50)
                    splitterPos = (int) (Width/2) - 40;

                ContentGrid.ColumnDefinitions[1].Width = new GridLength(12);
                ContentGrid.ColumnDefinitions[2].Width = new GridLength(Width / 2 - 40);
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

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false)
        {
            if (!Model.IsPreviewBrowserVisible && !showInBrowser)
                return;

            if (editor == null)
                editor = GetActiveMarkdownEditor();

            if (editor == null)
                return;

            var doc = editor.MarkdownDocument;
            var ext = Path.GetExtension(editor.MarkdownDocument.Filename).ToLower().Replace(".","");
            

            int lastPos = 0;
            dynamic dom = null;

            if (string.IsNullOrEmpty(ext) || ext == "md" || ext == "html" || ext == "htm")
            {
                this.ShowPreviewBrowser();
                
                if (keepScrollPosition)
                {
                    dom = PreviewBrowser.Document;
                    editor.MarkdownDocument.LastBrowserScrollPosition = dom.documentElement.scrollTop;
                }
                else
                    editor.MarkdownDocument.LastBrowserScrollPosition = 0;


                if (ext == "html" || ext == "htm")
                {
                    File.WriteAllText(editor.MarkdownDocument.HtmlRenderFilename, editor.MarkdownDocument.CurrentText);
                }
                else
                    editor.MarkdownDocument.RenderHtmlToFile();

                if (showInBrowser)
                {
                    ShellUtils.GoUrl(editor.MarkdownDocument.HtmlRenderFilename);
                }
                else
                {
                    PreviewBrowser.Cursor = Cursors.None;
                    PreviewBrowser.ForceCursor = true;
                    if (keepScrollPosition &&
                        PreviewBrowser.Source.ToString() == editor.MarkdownDocument.HtmlRenderFilename)
                        PreviewBrowser.Refresh(true);
                    else
                    {
                        PreviewBrowser.Navigate(editor.MarkdownDocument.HtmlRenderFilename);                        
                    }
                }
            }
            else
            {
                ShowPreviewBrowser(true);
            }
        }

        private DateTime invoked = DateTime.MinValue;
        
        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false)
        {
            if (!mmApp.Configuration.IsPreviewVisible)
                return;

            var current = DateTime.UtcNow;
                    
            // prevent multiple stacked refreshes
            if (invoked == DateTime.MinValue || current.Subtract(invoked).TotalMilliseconds > 4000)
            {
                invoked = current;
                
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() => { 
                        try
                        {
                            PreviewMarkdown(editor, keepScrollPosition);
                        }
                        catch { }                    
                    }));

                invoked = DateTime.MinValue;
            }
        }



        public MarkdownDocumentEditor GetActiveMarkdownEditor()
        {
            var tab = TabControl?.SelectedItem as TabItem;            
            return tab?.Tag as MarkdownDocumentEditor;
        }

        bool CheckForNewVersion(bool force, bool closeForm = true)
        {
            var updater = new ApplicationUpdater(typeof(MainWindow));
            bool isNewVersion = updater.IsNewVersionAvailable(!force);
            if (isNewVersion)
            {
                var res = MessageBox.Show(updater.VersionInfo.Detail + "\r\n\r\n" +
                                          "Do you want to download and install this version?",
                    updater.VersionInfo.Title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);
                
                if (res == MessageBoxResult.Yes)
                {
                    updater.DownloadProgressChanged += (sender,e) =>
                    {
                        DoEvents();
                        ShowStatus("Downloading Update: " + 
                                (e.BytesReceived / 1000).ToString("n0") + "kb  of  " +
                                (e.TotalBytesToReceive / 1000).ToString("n0") + "kb");                        
                    };
                    ShowStatus("Downloading Update...");
                    DoEvents();
                    updater.Download();

                    updater.ExecuteDownloadedFile();
                    ShowStatus("Download completed.");

                    if (closeForm)
                        Close();
                }
            }
            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;

            return isNewVersion;
        }
        public static void DoEvents()
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
        }
        private delegate void EmptyDelegate();
        #endregion



        #region Button Handlers

        public void Button_Handler(object sender, RoutedEventArgs e)
        {
            var button = sender;
            if (sender == null)
                return;

            if (button == ButtonOpenFile || button == ToolButtonOpenFile)
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.md)|*.md|" +
                             "Html files (*.htm,*.html)|*.htm;*.html|" +
                             "Javascript files (*.js)|*.js|" +
                             "Json files (*.json)|*.json|" +
                             "Css files (*.css)|*.css|" + 
                             "Xml files (*.xml,*.config)|*.xml;*.config|" +
                             "C# files (*.cs)|*.cs|" +
                             "Foxpro files (*.prg)|*.prg|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Multiselect = true,
                    Title = "Open Markdown File"
                };

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastFolder;

                var res = fd.ShowDialog();
                if (res == null || !res.Value)
                    return;
               
                OpenTab(fd.FileName);

                mmApp.Configuration.AddRecentFile(fd.FileName);
                RecentDocumentsContextList();
            }
            else if (button == ButtonNewFile)
            {
                OpenTab("untitled");
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
                ShellUtils.GoUrl( Path.Combine(Environment.CurrentDirectory,"PreviewThemes",mmApp.Configuration.RenderTheme));
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
                if (!CheckForNewVersion(true))
                    MessageBox.Show("Your version of Markdown Monster is up to date.",
                        mmApp.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
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
        }


        private void ButtonCloseTab_Click(object sender, RoutedEventArgs e)
        {
            var tab = TabControl.SelectedItem as TabItem;
            if (tab == null)
                return;

            if (CloseTab(tab))
                TabControl.Items.Remove(tab);
        }

        private void ButtonCloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            MarkdownDocumentEditor editor = null;
            var menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Name == "MenuCloseAllButThisTab")
                 editor = this.GetActiveMarkdownEditor();

            for (int i = TabControl.Items.Count-1; i > -1; i--)
            {
                var tab = TabControl.Items[i] as TabItem;
                if (tab != null)
                {
                    var ed = tab.Tag as MarkdownDocumentEditor;
                    if (ed == null || ed == editor)
                        continue;

                    if (CloseTab(tab))
                        TabControl.Items.Remove(tab);
                }
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

        private void ButtonViewInBrowser_Click(object sender, RoutedEventArgs e)
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

        #endregion

        #region Miscelleaneous Events

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (File.Exists(file))
                        OpenTab(file);
                }
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


        public void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(filesToOpen))
                {
                    TabItem lastTab = null;
                    foreach (var file in StringUtils.GetLines(filesToOpen))
                    {                        
                        if (!string.IsNullOrEmpty(file))
                            lastTab = this.OpenTab(file.Trim());
                    }
                    if (lastTab != null)
                        Dispatcher.InvokeAsync(() => TabControl.SelectedItem = lastTab);                                            
                }

                this.Topmost = true;

                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                this.Activate();

                // restor out of band
                Dispatcher.BeginInvoke(new Action(() => { this.Topmost = false; }));
            });
        }
        #endregion

        #region StatusBar Display

        public void ShowStatus(string message = null, int milliSeconds = 0)
        {
            if (message == null)
                message = "Ready";

            StatusText.Text = message;

            if (milliSeconds > 0)
            {
                var t = new Timer((object win) =>
                {
                    var window = win as MainWindow;                    
                    window.Dispatcher.Invoke(() => {  window.ShowStatus(null, 0); } );
                },this,milliSeconds,Timeout.Infinite);
            }
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
        #endregion

        #region Preview Browser No Script Errors
        /// <summary>
        /// Keep WebBrowser Preview control from firing script errors. We need this
        /// because we may be previewing HTML content that includes script content
        /// that might not work because of local file restrictions or missing 
        /// resources that can't load from the Web.
        /// 
        /// Ugh... Keep Web Browser control from showing error dialog - silent operation.
        /// this is ugly, but it works.
        /// </summary>
        /// <param name="browser"></param>
        /// <param name="silent"></param>
        static void NoScriptErrors(WebBrowser browser, bool silent)
        {
            if (browser == null)
                return;

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                dynamic webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                    webBrowser.Silent = silent;
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        #endregion
    }

}
