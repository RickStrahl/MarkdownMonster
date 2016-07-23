#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Dragablz;
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

            InitializePreviewBrowser();

            TabControl.ClosingItemCallback = TabControlDragablz_TabItemClosing;            
            
            Loaded += OnLoaded;                       
            Drop += MainWindow_Drop;
            AllowDrop = true;           
            KeyUp += MainWindow_KeyUp;
                        
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
                AddRecentFile(args[1]);                
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
            Dispatcher.InvokeAsync(() =>
            {
                TabControl.InvalidateVisual();
                Left = left;

                mmApp.SetWorkingSet(10000000, 5000000);

            });            
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            mmApp.SetWorkingSet(10000000, 5000000);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            
            Hide();

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

        void AddRecentFile(string file)
        {
            mmApp.Configuration.AddRecentFile(file);
            RecentDocumentsContextList();
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
                    OpenTab(file);
                    AddRecentFile(file);
                };
                context.Items.Add(mi);

                var mi2 = new MenuItem()
                {
                    Header = file,
                };
                mi2.Click += (object s, RoutedEventArgs ev) => OpenTab(file);
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

            if (mmApp.Configuration.RememberOpenFiles)
            {
                var selectedDoc = conf.OpenDocuments.FirstOrDefault(dc => dc.IsActive);
                TabItem selectedTab = null;

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

        #endregion

        #region Tab Handling

        
        public TabItem OpenTab(string mdFile = null, MarkdownDocumentEditor editor = null, bool showPreviewIfActive = false, string syntax = "markdown", bool selectTab = true)
        {
            if (mdFile != null && mdFile!= "untitled" && (!File.Exists(mdFile) ||
                                  !AddinManager.Current.RaiseOnBeforeOpenDocument(mdFile)))
                return null;

            var tab = new TabItem();
                      
            tab.Margin = new Thickness(0, 0, 3, 0);
            tab.Padding = new Thickness(2, 0, 7, 2);
            tab.Background = Background;
            tab.ContextMenu = Resources["TabItemContextMenu"] as ContextMenu;

            
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
                    Path = new PropertyPath("FilenameWithIndicator"),                    
                    Mode = BindingMode.OneWay
                };
                BindingOperations.SetBinding(tab, HeaderedContentControl.HeaderProperty, headerBinding);

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

            // Get Tabablz control to insert at the top of the head
            //if (TabControl.Items.Count > 0)
            //    TabablzControl.AddItem(tab, TabControl.Items[0] as TabItem, AddLocationHint.First);
            //else
            //    TabControl.AddToSource(tab);
            
            if (selectTab)
            {
                TabControl.SelectedItem = tab;

                if (showPreviewIfActive && PreviewBrowser.Width > 5)
                    PreviewMarkdown(); //Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
            }

            AddinManager.Current.RaiseOnAfterOpenDocument(editor.MarkdownDocument);

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

            var editor = tab.Tag as MarkdownDocumentEditor;
            if (editor == null)
                return false;

            bool returnValue = true;

            var doc = editor.MarkdownDocument;

            if (!string.IsNullOrEmpty(doc.HtmlRenderFilename) && File.Exists(doc.HtmlRenderFilename))
                File.Delete(doc.HtmlRenderFilename);

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
            }

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
                                            
            return editor.SaveDocument();            
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
            // only render if the preview is actually visible and rendering in Preview Browser
            if (!Model.IsPreviewBrowserVisible && !showInBrowser)
                return;

            if (editor == null)
                editor = GetActiveMarkdownEditor();

            if (editor == null)
                return;

            var doc = editor.MarkdownDocument;
            var ext = Path.GetExtension(doc.Filename).ToLower().Replace(".","");

            string renderedHtml = null;

            if (string.IsNullOrEmpty(ext) || ext == "md" || ext == "html" || ext == "htm")
            {
                ShowPreviewBrowser();

                dynamic dom = null;
                if (keepScrollPosition)
                {
                    dom = PreviewBrowser.Document;
                    editor.MarkdownDocument.LastBrowserScrollPosition = dom.documentElement.scrollTop;
                }
                else
                    editor.MarkdownDocument.LastBrowserScrollPosition = 0;

                if (ext == "html" || ext == "htm")
                {
                    editor.MarkdownDocument.WriteFile(editor.MarkdownDocument.HtmlRenderFilename, editor.MarkdownDocument.CurrentText);
                }
                else
                    renderedHtml = editor.MarkdownDocument.RenderHtmlToFile();

                if (showInBrowser)
                {
                    ShellUtils.GoUrl(editor.MarkdownDocument.HtmlRenderFilename);
                }
                else
                {
                    PreviewBrowser.Cursor = Cursors.None;
                    PreviewBrowser.ForceCursor = true;
                    if (keepScrollPosition)
                    {
                        string browserUrl = PreviewBrowser.Source.ToString().ToLower();
                        string documentFile = "file:///" + editor.MarkdownDocument.HtmlRenderFilename.Replace('\\', '/').ToLower();
                        if (browserUrl == documentFile)
                        {
                            dom = PreviewBrowser.Document;
                            //var content = dom.getElementById("MainContent");
                                                        

                            renderedHtml = StringUtils.ExtractString(renderedHtml,
                                "<!-- Markdown Monster Content -->",
                                "<!-- End Markdown Monster Content -->");

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
                                }
                                catch
                                {
                                    PreviewBrowser.Refresh(true);
                                }
                            }

                            return;
                        }
                    }

                    PreviewBrowser.Navigate(editor.MarkdownDocument.HtmlRenderFilename);
                    return;
                }
            }

            ShowPreviewBrowser(true);
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
                    updater.DownloadProgressChanged += (sender, e) =>
                    {
                        WindowUtilities.DoEvents();
                        ShowStatus("Downloading Update: " +
                                   (e.BytesReceived/1000).ToString("n0") + "kb  of  " +
                                   (e.TotalBytesToReceive/1000).ToString("n0") + "kb");
                    };
                    ShowStatus("Downloading Update...");

                    WindowUtilities.DoEvents();

                    if (!updater.Download() || !updater.ExecuteDownloadedFile())
                    {
                        MessageBox.Show("Failed to download the update file. Please install the update " +
                                        "manually from http://markdownmonster.west-wind.com/.\r\n\r\n" +
                                        updater.ErrorMessage,
                            "Update Failed",
                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        ShellUtils.GoUrl("http://markdownmonster.west-wind.com/download.aspx");

                        ShowStatus("Update failed...", 4000);
                        return false;
                    }
                    
                    ShowStatus("Update download completed...");

                    if (closeForm)
                        Close();
                }
            }
            mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;

            return isNewVersion;
        }
       
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

                AddRecentFile(fd.FileName);
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
            else if (button == MenuDocumentation)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs");
            else if(button == MenuMarkdownBasics)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4ne1eu2cq.htm");
            else if (button == MenuCreateAddinDocumentation)
                ShellUtils.GoUrl("http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm");
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
                 editor = GetActiveMarkdownEditor();

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
            editor.Window.PreviewMarkdownAsync();            
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
            bool isControlKey = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            if (e.Key == Key.N && isControlKey)
            {
                e.Handled = true;
                Button_Handler(ButtonNewFile, null);
            }
            if (e.Key == Key.O && isControlKey)
            {
                e.Handled = false;
                Button_Handler(ButtonOpenFile, null);
            }
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


        private void HandleNamedPipe_OpenRequest(string filesToOpen)
        {
            Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(filesToOpen))
                {
                    TabItem lastTab = null;
                    foreach (var file in StringUtils.GetLines(filesToOpen))
                    {                        
                        if (!string.IsNullOrEmpty(file))
                            lastTab = OpenTab(file.Trim());
                    }
                    if (lastTab != null)
                        Dispatcher.InvokeAsync(() => TabControl.SelectedItem = lastTab);                                            
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
        #endregion

        #region Preview Browser Operation

        private void InitializePreviewBrowser()
        {
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

                new Timer((editr) =>
                {
                    try
                    {
                        if (File.Exists(editor.MarkdownDocument.HtmlRenderFilename))
                            File.Delete(editor.MarkdownDocument.HtmlRenderFilename);
                    }
                    catch
                    { }
                }, null, 2000, Timeout.Infinite);
            };
            PreviewBrowser.Navigate("about:blank");


        }

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
