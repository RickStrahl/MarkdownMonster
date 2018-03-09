using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class AppCommands
    {
        AppModel Model;

        public AppCommands(AppModel model)
        {
            Model = model;

            // File Operations
            NewDocument();
            OpenDocument();            
            Save();
            SaveAs();
            NewWeblogPost();
            OpenRecentDocument();
            SaveAsHtml();

            // Links and External
            OpenSampleMarkdown();

            // Settings
            PreviewModes();
            RemoveMarkdownFormatting();
            WordWrap();
            DistractionFreeMode();
            PresentationMode();
            PreviewBrowser();


            // Editor Commands
            ToolbarInsertMarkdown();
            CloseActiveDocument();
            CloseAllDocuments();
            ShowActiveTabsList();


            // Preview Browser
            EditPreviewTheme();
            PreviewSyncMode();

            // Miscellaneous
            OpenAddinManager();
            Help();
            CopyFolderToClipboard();
            TabControlFileList();


            


        }

        #region Files And File Management



        public CommandBase NewDocumentCommand { get; set; }

        void NewDocument()
        {

            // NEW DOCUMENT COMMAND (ctrl-n)
            NewDocumentCommand = new CommandBase((s, e) =>
            {
                Model.Window.OpenTab("untitled");
            });
        }

        public CommandBase OpenDocumentCommand { get; set; }

        void OpenDocument()
        {
            // OPEN DOCUMENT COMMAND
            OpenDocumentCommand = new CommandBase((s, e) =>
            {
                var fd = new OpenFileDialog
                {
                    DefaultExt = ".md",
                    Filter = "Markdown files (*.md,*.markdown,*.mdcrypt)|*.md;*.markdown;*.mdcrypt|" +
                             "Html files (*.htm,*.html)|*.htm;*.html|" +
                             "Javascript files (*.js)|*.js|" +
                             "Typescript files (*.ts)|*.ts|" +
                             "Json files (*.json)|*.json|" +
                             "Css files (*.css)|*.css|" +
                             "Xml files (*.xml,*.config)|*.xml;*.config|" +
                             "C# files (*.cs)|*.cs|" +
                             "C# Razor files (*.cshtml)|*.cshtml|" +
                             "Foxpro files (*.prg)|*.prg|" +
                             "Powershell files (*.ps1)|*.ps1|" +
                             "Php files (*.php)|*.php|" +
                             "Python files (*.py)|*.py|" +
                             "All files (*.*)|*.*",
                    CheckFileExists = true,
                    RestoreDirectory = true,
                    Multiselect = true,
                    Title = "Open Markdown File"
                };

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastFolder;

                bool? res = null;
                try
                {
                    res = fd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Unable to open file.", ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                if (res == null || !res.Value)
                    return;

                foreach (var file in fd.FileNames)
                {
                    Model.Window.OpenTab(file, rebindTabHeaders: true);                    
                }
                
            });
        }


        public CommandBase SaveCommand { get; set; }

        void Save()
        {
            // SAVE COMMAND
            SaveCommand = new CommandBase((s, e) =>
            {
                var tab = Model.Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;

                if (doc.MarkdownDocument.Filename == "untitled")
                    SaveAsCommand.Execute(tab);
                else if (!doc.SaveDocument())
                {
                    SaveAsCommand.Execute(tab);
                }

                Model.Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (Model.ActiveDocument == null)
                    return false;

                return Model.ActiveDocument.IsDirty;
            });
        }

        public CommandBase SaveAsCommand { get; set; }

        void SaveAs()
        {
            SaveAsCommand = new CommandBase((parameter, e) =>
            {
                bool isEncrypted = parameter != null && parameter.ToString() == "Secure";

                var tab = Model.Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                var filename = doc.MarkdownDocument.Filename;
                var folder = Path.GetDirectoryName(doc.MarkdownDocument.Filename);

                if (filename == "untitled")
                {
                    folder = mmApp.Configuration.LastFolder;

                    var match = Regex.Match(doc.GetMarkdown(), @"^# (\ *)(?<Header>.+)", RegexOptions.Multiline);

                    if (match.Success)
                    {
                        filename = match.Groups["Header"].Value;
                        if (!string.IsNullOrEmpty(filename))
                            filename = FileUtils.SafeFilename(filename);
                    }
                }

                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    folder = mmApp.Configuration.LastFolder;
                    if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                        folder = KnownFolders.GetPath(KnownFolder.Libraries);
                }


                SaveFileDialog sd = new SaveFileDialog
                {
                    FilterIndex = 1,
                    InitialDirectory = folder,
                    FileName = filename,
                    CheckFileExists = false,
                    OverwritePrompt = true,
                    CheckPathExists = true,
                    RestoreDirectory = true
                };

                var mdcryptExt = string.Empty;
                if (isEncrypted)
                    mdcryptExt = "Secure Markdown files (*.mdcrypt)|*.mdcrypt|";

                sd.Filter =
                    $"{mdcryptExt}Markdown files (*.md)|*.md|Markdown files (*.markdown)|*.markdown|All files (*.*)|*.*";

                bool? result = null;
                try
                {
                    result = sd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Unable to save file: " + doc.MarkdownDocument.Filename, ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                if (!isEncrypted)
                    doc.MarkdownDocument.Password = null;
                else
                {
                    var pwdDialog = new FilePasswordDialog(doc.MarkdownDocument, false)
                    {
                        Owner = Model.Window
                    };
                    bool? pwdResult = pwdDialog.ShowDialog();
                }

                if (result != null && result.Value)
                {
                    doc.MarkdownDocument.Filename = sd.FileName;
                    if (!doc.SaveDocument())
                    {
                        MessageBox.Show(Model.Window,
                            $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                            "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SaveAsCommand.Execute(tab);
                        return;
                    }
                    mmApp.Configuration.LastFolder = Path.GetDirectoryName(sd.FileName);
                }
                
                Model.Window.SetWindowTitle();
                Model.Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (Model.ActiveDocument == null)
                    return false;

                return true;
            });
        }



        public CommandBase NewWeblogPostCommand { get; set; }

        void NewWeblogPost()
        {
            NewWeblogPostCommand = new CommandBase((parameter, command) =>
            {
                
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            });
        }


        public CommandBase OpenRecentDocumentCommand { get; set; }

        void OpenRecentDocument()
        {
            OpenRecentDocumentCommand = new CommandBase((parameter, command) =>
            {
                // hide to avoid weird fade behavior
                var context = Model.Window.Resources["ContextMenuRecentFiles"] as ContextMenu;
                if (context != null)
                    context.Visibility = Visibility.Hidden;

                WindowUtilities.DoEvents();

                var parm = parameter as string;
                if (string.IsNullOrEmpty(parm))
                    return;

                if (Directory.Exists(parm))
                {
                    Model.Window.FolderBrowser.FolderPath = parm;
                    Model.Window.ShowFolderBrowser();
                }
                else
                    Model.Window.OpenTab(parm, rebindTabHeaders: true);

                if (context != null)
                {
                    WindowUtilities.DoEvents();
                    context.Visibility = Visibility.Visible;
                }

            }, (p, c) => true);
        }



        public CommandBase SaveAsHtmlCommand { get; set; }

        void SaveAsHtml()
        {
            SaveAsHtmlCommand = new CommandBase((s, e) =>
            {
                var tab = Model.Window.TabControl?.SelectedItem as TabItem;
                var doc = tab?.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                var folder = Path.GetDirectoryName(doc.MarkdownDocument.Filename);

                SaveFileDialog sd = new SaveFileDialog
                {
                    Filter =
                        "Html files (Html only) (*.html)|*.html|Html files (Html and dependencies in a folder)|*.html",
                    FilterIndex = 1,
                    InitialDirectory = folder,
                    FileName = Path.ChangeExtension(doc.MarkdownDocument.Filename, "html"),
                    CheckFileExists = false,
                    OverwritePrompt = true,
                    CheckPathExists = true,
                    RestoreDirectory = true
                };

                bool? result = null;
                try
                {
                    result = sd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log("Unable to save html file: " + doc.MarkdownDocument.Filename, ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                if (result != null && result.Value)
                {
                    if (sd.FilterIndex != 2)
                    {
                        var html = doc.RenderMarkdown(doc.GetMarkdown(),
                            mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal);

                        if (!doc.MarkdownDocument.WriteFile(sd.FileName, html))
                        {
                            MessageBox.Show(Model.Window,
                                $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                                "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SaveAsHtmlCommand.Execute(null);
                            return;
                        }
                    }
                    else
                    {
                        string msg = @"This feature is not available yet.

For now, you can use 'View in Web Browser' to view the document in your favorite Web Browser and use 'Save As...' to save the Html document with all CSS and Image dependencies.

Do you want to View in Browser now?
";
                        var mbResult = MessageBox.Show(msg,
                            mmApp.ApplicationName,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Asterisk,
                            MessageBoxResult.Yes);

                        if (mbResult == MessageBoxResult.Yes)
                            Model.ViewInExternalBrowserCommand.Execute(null);
                    }
                }

                Model.Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (Model.ActiveDocument == null || Model.ActiveEditor == null)
                    return false;
                if (Model.ActiveDocument.Filename == "untitled")
                    return true;
                if (Model.ActiveEditor.EditorSyntax != "markdown")
                    return false;

                return true;
            });
        }


        #endregion


        #region Links and External Access Commands

        public CommandBase OpenSampleMarkdownCommand { get; set; }

        void OpenSampleMarkdown()
        {
            OpenSampleMarkdownCommand = new CommandBase((parameter, command) =>
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"), tempFile, true);
                Model.Window.OpenTab(tempFile, rebindTabHeaders: true);
            });
        }

        #endregion


        #region Settings Commands
        public CommandBase PreviewModesCommand { get; set; }

        void PreviewModes()
        {
            PreviewModesCommand = new CommandBase((parameter, command) =>
            {
                string action = parameter as string;
                if (string.IsNullOrEmpty(action))
                    return;

                if (action == "ExternalPreviewWindow")
                    Model.Configuration.PreviewMode = MarkdownMonster.PreviewModes.ExternalPreviewWindow;
                else
                    Model.Configuration.PreviewMode = MarkdownMonster.PreviewModes.InternalPreview;

                Model.IsPreviewBrowserVisible = true;

                Model.Window.ShowPreviewBrowser();                
                Model.Window.PreviewMarkdownAsync();
            }, (p, c) => true);
        }


        public CommandBase RemoveMarkdownFormattingCommand { get; set; }

        void RemoveMarkdownFormatting()
        {
            RemoveMarkdownFormattingCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                if (!editor.RemoveMarkdownFormatting())
                {
                    Model.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, System.Windows.Media.Colors.Red);
                    Model.Window.ShowStatus("Didn't remove formatting. No selection or document is not a Markdown document.",6000);
                }
            }, (p, c) => true);
        }



        public CommandBase DistractionFreeModeCommand { get; set; }

        void DistractionFreeMode()
        {

            var Window = Model.Window;

            DistractionFreeModeCommand = new CommandBase((s, e) =>
            {
                GridLength glToolbar = new GridLength(0);
                GridLength glMenu = new GridLength(0);
                GridLength glStatus = new GridLength(0);

                GridLength glFileBrowser = new GridLength(0);

                if (Window.ToolbarGridRow.Height == glToolbar)
                {
                    Window.SaveSettings();

                    glToolbar = GridLength.Auto;
                    glMenu = GridLength.Auto;
                    glStatus = GridLength.Auto;

                    //mmApp.Configuration.WindowPosition.IsTabHeaderPanelVisible = true;
                    Window.TabControl.IsHeaderPanelVisible = true;

                    Model.IsPreviewBrowserVisible = true;
                    Window.PreviewMarkdown();

                    Window.WindowState = mmApp.Configuration.WindowPosition.WindowState;

                    Model.IsFullScreen = false;

                    Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);
                }
                else
                {
                    var tokens = mmApp.Configuration.DistractionFreeModeHideOptions.ToLower()
                        .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.All(d => d != "menu"))
                        glMenu = GridLength.Auto;

                    if (tokens.All(d => d != "toolbar"))
                        glToolbar = GridLength.Auto;

                    if (tokens.All(d => d != "statusbar"))
                        glStatus = GridLength.Auto;

                    if (tokens.Any(d => d == "tabs"))
                        Window.TabControl.IsHeaderPanelVisible = false;

                    if (tokens.Any(d => d == "preview"))
                    {
                        Model.IsPreviewBrowserVisible = false;
                        Window.ShowPreviewBrowser(hide: true);
                    }

                    mmApp.Configuration.WindowPosition.WindowState = Window.WindowState;
                    if (tokens.Any(d => d == "maximized"))
                        Window.WindowState = WindowState.Maximized;

                    Window.ShowFolderBrowser(true);

                    Model.IsFullScreen = true;
                }

                // toolbar     
                Window.MainMenuGridRow.Height = glMenu;
                Window.ToolbarGridRow.Height = glToolbar;
                Window.StatusBarGridRow.Height = glStatus;
            }, null);
        }


        public CommandBase PresentationModeCommand { get; set; }

        void PresentationMode()
        {
            var window = Model.Window;

            // PRESENTATION MODE
            PresentationModeCommand = new CommandBase((s, e) =>
            {
                if (Model.IsFullScreen)
                    DistractionFreeModeCommand.Execute(null);

                GridLength gl = new GridLength(0);
                if (window.WindowGrid.RowDefinitions[1].Height == gl)
                {
                    gl = GridLength.Auto; // toolbar height

                    window.MainWindowEditorColumn.Width = new GridLength(1, GridUnitType.Star);
                    window.MainWindowSeparatorColumn.Width = new GridLength(0);
                    //window.MainWindowPreviewColumn.Width =
                    //    new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);

                    window.PreviewMarkdown();

                    window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

                    Model.IsPresentationMode = false;
                }
                else
                {
                    window.SaveSettings();

                    //mmApp.Configuration.WindowPosition.SplitterPosition =
                    //    Convert.ToInt32(window.MainWindowPreviewColumn.Width.Value);

                    // don't allow presentation mode for non-Markdown documents
                    var editor = window.GetActiveMarkdownEditor();
                    if (editor != null)
                    {
                        var file = editor.MarkdownDocument.Filename.ToLower();
                        var ext = Path.GetExtension(file).Replace(".", "");

                        Model.Configuration.EditorExtensionMappings.TryGetValue(ext, out string mappedTo);
                        mappedTo = mappedTo ?? string.Empty;
                        if (file != "untitled" && mappedTo != "markdown" && mappedTo != "html")
                        {
                            // don't allow presentation mode for non markdown files
                            Model.IsPresentationMode = false;
                            Model.IsPreviewBrowserVisible = false;
                            window.ShowPreviewBrowser(true);
                            return;
                        }
                    }

                    window.ShowPreviewBrowser();
                    window.ShowFolderBrowser(true);

                    window.MainWindowEditorColumn.Width = gl;
                    window.MainWindowSeparatorColumn.Width = gl;
                    //window.MainWindowPreviewColumn.Width = new GridLength(1, GridUnitType.Star);

                    Model.IsPresentationMode = true;
                    Model.IsPreviewBrowserVisible = true;
                }

                window.WindowGrid.RowDefinitions[1].Height = gl;
                //Window.WindowGrid.RowDefinitions[3].Height = gl;  
            }, null);
        }



        public CommandBase PreviewBrowserCommand { get; set; }

        void PreviewBrowser()
        {
            var window = Model.Window;
            var config = Model.Configuration;

            PreviewBrowserCommand = new CommandBase((s, e) =>
            {
                var tab = window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                var editor = tab.Tag as MarkdownDocumentEditor;

                Model.WindowLayout.IsPreviewVisible = Model.IsPreviewBrowserVisible;

                config.IsPreviewVisible = Model.IsPreviewBrowserVisible;

                if (!Model.IsPreviewBrowserVisible && Model.IsPresentationMode)
                    PresentationModeCommand.Execute(null);


                window.ShowPreviewBrowser(!Model.IsPreviewBrowserVisible);
                if (Model.IsPreviewBrowserVisible)
                    window.PreviewMarkdownAsync(editor);

            }, null);


        }

        public CommandBase WordWrapCommand { get; set; }

        void WordWrap()
        {

            // WORD WRAP COMMAND
            WordWrapCommand = new CommandBase((parameter, command) =>
                {
                    //MessageBox.Show("alt-z WPF");
                    Model.Configuration.EditorWrapText = !mmApp.Model.Configuration.EditorWrapText;
                    Model.ActiveEditor?.SetWordWrap(mmApp.Model.Configuration.EditorWrapText);
                },
                (p, c) => Model.IsEditorActive);
        }



        #endregion

        #region Editor Commands

        public CommandBase ToolbarInsertMarkdownCommand { get; set; }

        void ToolbarInsertMarkdown()
        {
            ToolbarInsertMarkdownCommand = new CommandBase((s, e) =>
            {
                string action = s as string;
                var editor = Model.Window.GetActiveMarkdownEditor();
                editor?.ProcessEditorUpdateCommand(action);
            }, null);
        }

        public CommandBase CloseActiveDocumentCommand { get; set; }

        void CloseActiveDocument()
        {            
            CloseActiveDocumentCommand = new CommandBase((s, e) =>
            {
                var tab = Model.Window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                if (Model.Window.CloseTab(tab))
                    Model.Window.TabControl.Items.Remove(tab);
            }, null)
            {
                Caption = "_Close Document",
                ToolTip = "Closes the active tab and asks to save the document."
            };
        }


        public CommandBase CloseAllDocumentsCommand { get; set; }

        void CloseAllDocuments()
        {
            CloseAllDocumentsCommand = new CommandBase((parameter, command) =>
            {
                var parm = parameter as string;
                TabItem except = null;

                if (parm != null && parm == "AllBut")
                    except = Model.Window.TabControl.SelectedItem as TabItem;

                Model.Window.CloseAllTabs(except);
                Model.Window.BindTabHeaders();

            }, (p, c) => true);
        }


        /// <summary>
        /// This command handles Open Document clicks from a context
        /// menu.
        /// </summary>
        public CommandBase TabControlFileListCommand { get; set; }

        void TabControlFileList()
        {
            TabControlFileListCommand = new CommandBase((parameter, command) =>
            {
                var tab = Model.Window.GetTabFromFilename(parameter as string);
                tab.IsSelected = true;
            }, (p, c) => true);
        }


        public CommandBase WindowMenuCommand { get; set; }

        void ShowActiveTabsList()
        {
            WindowMenuCommand = new CommandBase((parameter, command) =>
            {
                var mi = Model.Window.MainMenuWindow;
                mi.Items.Clear();

                mi.Items.Add(new MenuItem { Header = "_Close Document", Command= Model.Commands.CloseActiveDocumentCommand  });
                mi.Items.Add(new MenuItem { Header = "Close _All Documents", Command = Model.Commands.CloseAllDocumentsCommand });
                mi.Items.Add(new MenuItem { Header = "Close All _But This Document", Command = Model.Commands.CloseAllDocumentsCommand, CommandParameter="AllBut" });
                
                var menuItems = Model.Window.GenerateContextMenuItemsFromOpenTabs();
                if (menuItems.Count < 1)
                    return;

                mi.Items.Add(new Separator());
                foreach (var menu in menuItems)
                {
                 
                    mi.Items.Add(menu);
                }

                mi.IsSubmenuOpen = true;
                
                mi.SubmenuClosed += (s,e) => ((MenuItem)s).Items.Clear();
            }, (p, c) => true);
        }

        #endregion

        #region Preview

        public CommandBase EditPreviewThemeCommand { get; set; }

        void EditPreviewTheme()
        {
            EditPreviewThemeCommand = new CommandBase((parameter, command) =>
            {                
                var path = Path.Combine(App.InitialStartDirectory, "PreviewThemes",Model.Configuration.RenderTheme);
                mmFileUtils.OpenFileInExplorer(path);

                mmFileUtils.ShowExternalBrowser("https://markdownmonster.west-wind.com/docs/_4nn17bfic.htm");
            }, (p, c) => true);
        }


        public CommandBase PreviewSyncModeCommand { get; set; }

        void PreviewSyncMode()
        {
            PreviewSyncModeCommand = new CommandBase((parameter, command) =>
            {
                
                Model.Window.ComboBoxPreviewSyncModes.Focus();
                WindowUtilities.DoEvents();
                Model.Window.ComboBoxPreviewSyncModes.IsDropDownOpen = true;
            }, (p, c) => true);
        }

        #endregion

        #region Open Document Operations

        public CommandBase CopyFolderToClipboardCommand { get; set; }

        void CopyFolderToClipboard()
        {
            CopyFolderToClipboardCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                if (editor.MarkdownDocument.Filename == "untitled")
                    return;

                string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);

                try
                {
                    Clipboard.SetText(path);
                    Model.Window.ShowStatus($"Path copied to clipboard: {path}", 6000);
                }
                catch
                {
                    Model.Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                    Model.Window.ShowStatus("Clipboard failure: Failed copy foldername to clipboard.", 6000);
                }
            }, (p, c) => true);
        }

        #endregion


        #region Miscellaneous

        public CommandBase AddinManagerCommand { get; set; }

        void OpenAddinManager()
        {
            AddinManagerCommand = new CommandBase((parameter, command) =>
            {
                var form = new AddinManagerWindow
                {
                    Owner = Model.Window
                };
                form.Show();
            });
        }


        public CommandBase HelpCommand { get; set; }

        void Help()
        {
            HelpCommand = new CommandBase((topicId, command) =>
            {
                string url = mmApp.Urls.DocumentationBaseUrl;

                if (topicId != null)
                    url = mmApp.GetDocumentionUrl(topicId as string);

                ShellUtils.GoUrl(url);
            }, (p, c) => true);
        }

        #endregion

        #region Static Menus Accessed from Control Templates

        public static CommandBase TabWindowListCommand { get; }

        static AppCommands()
        {
            TabWindowListCommand = new CommandBase( (parameter, command)=>
            {
                var button = parameter as FrameworkElement;
                if (button == null) return;

                var menuItems = mmApp.Model.Window.GenerateContextMenuItemsFromOpenTabs();

                button.ContextMenu = new ContextMenu();
                foreach (var mi in menuItems)
                    button.ContextMenu.Items.Add(mi);

                button.ContextMenu.IsOpen = true;
                button.ContextMenu.Closed += (o, args) => button.ContextMenu.Items.Clear();
            },
            (p, c) => true);
        }

        #endregion
    }
}
