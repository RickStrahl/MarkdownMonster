using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Utilities;
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
            OpenFromUrl();          
            Save();
            SaveAs();
            NewWeblogPost();
            OpenRecentDocument();
            SaveAsHtml();
            GeneratePdf();
            PrintPreview();
            

            // Links and External
            OpenSampleMarkdown();

            // Settings
            PreviewModes();
            RemoveMarkdownFormatting();
            WordWrap();
            DistractionFreeMode();
            PresentationMode();
            PreviewBrowser();
            Settings();


            // Editor Commands
            ToolbarInsertMarkdown();
            CloseActiveDocument();
            CloseAllDocuments();
            ShowActiveTabsList();
            CopyAsHtml();


            // Preview Browser
            EditPreviewTheme();
            PreviewSyncMode();
            ViewInExternalBrowser();
            ViewHtmlSource();


            // Miscellaneous
            OpenAddinManager();
            
            Help();
            CopyFolderToClipboard();
            TabControlFileList();

            // Git
            OpenGitClient();
            OpenFromGitRepo();
            CommitToGit();


            // Sidebar
            CloseLeftSidebarPanel();
            CloseRightSidebarPanel();
            OpenLeftSidebarPanel();
            ShowFolderBrowser();
        }


        private List<PropertyInfo> commandProperties;

        public void InvalidateCommands()
        {
            if (commandProperties == null)
            {
                commandProperties = typeof(AppCommands)
                    .GetProperties(BindingFlags.Public |
                                   BindingFlags.Instance |
                                   BindingFlags.GetProperty)
                    .Where(t => t.PropertyType == typeof(CommandBase))
                    .ToList();
            }

            foreach (var pi in commandProperties)
                (pi.GetValue(this) as CommandBase)?.InvalidateCanExecute();
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


        public CommandBase OpenFromUrlCommand { get; set; }

        void OpenFromUrl()
        {
            OpenFromUrlCommand = new CommandBase((parameter, command) =>
            {
                var form = new OpenFromUrlDialog();
                form.Owner = Model.Window;
                var result = form.ShowDialog();

                if (result == null || !result.Value || string.IsNullOrEmpty(form.Url))
                    return;

                var url = form.Url;
                bool fixupImageLinks = form.FixupImageLinks;

                var fs = new FileSaver();
                url = fs.ParseMarkdownUrl(url);

                string markdown;
                try
                {
                    markdown = HttpUtils.HttpRequestString(url);
                }
                catch (System.Net.WebException ex)
                {
                    Model.Window.ShowStatus($"Can't open from url: {ex.Message}", 6000, FontAwesomeIcon.Warning,
                        Colors.Firebrick);
                    return;
                }

                if (string.IsNullOrEmpty(markdown))
                {
                    Model.Window.ShowStatus($"No content found at URL: {url}", 6000, FontAwesomeIcon.Warning,
                        Colors.Firebrick);
                    return;
                }

                if (fixupImageLinks && url.EndsWith(".md", StringComparison.CurrentCultureIgnoreCase))
                {
                    var uri = new Uri(url);
                    string basePath =
                        $"{uri.Scheme}://{uri.Authority}{string.Join("", uri.Segments.Take(uri.Segments.Length - 1))}";


                    var reg = new Regex("!\\[.*?]\\(.*?\\)");

                    var matches = reg.Matches(markdown);
                    foreach (Match match in matches)
                    {
                        var link = match.Value;
                        var linkUrl = StringUtils.ExtractString(link, "](", ")");

                        if (linkUrl.StartsWith("http"))
                            continue;

                        var text = StringUtils.ExtractString(link, "![", "](");
                        linkUrl = basePath + linkUrl;

                        var newLink = $"![text]({linkUrl})";
                        markdown = markdown.Replace(link, newLink);
                    }

                    reg = new Regex("<img src=\\\".*?/>");

                    matches = reg.Matches(markdown);
                    foreach (Match match in matches)
                    {
                        var link = match.Value;
                        var linkUrl = StringUtils.ExtractString(link, " src=\"", "\"");

                        if (linkUrl.StartsWith("http"))
                            continue;

                        string newLink = basePath + linkUrl;
                        newLink = link.Replace(linkUrl, newLink);

                        markdown = markdown.Replace(link, newLink);
                    }
                }

                var tab = Model.Window.OpenTab("untitled");
                ((MarkdownDocumentEditor) tab.Tag).MarkdownDocument.CurrentText = markdown;
                Model.Window.PreviewMarkdownAsync();

            }, (p, c) => true);
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
                if (!Model.IsEditorActive)
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
                if (!Model.IsEditorActive)
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
                            Model.Commands.ViewInExternalBrowserCommand.Execute(null);
                    }
                }

                Model.Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (!Model.IsEditorActive)
                    return false;
                if (Model.ActiveDocument.Filename == "untitled")
                    return true;
                if (Model.ActiveEditor.EditorSyntax != "markdown")
                    return false;

                return true;
            });
        }




        public CommandBase CopyAsHtmlCommand { get; set; }

        void CopyAsHtml()
        {
            CopyAsHtmlCommand = new CommandBase((parameter, command) =>
            {
                if (Model.ActiveEditor == null)
                    return;

                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                var markdown = editor.GetSelection();
                if (string.IsNullOrEmpty(markdown))
                    return;

                var html = editor.RenderMarkdown(markdown);

                // copy to clipboard as html
                if (!ClipboardHelper.CopyHtmlToClipboard(html, markdown))
                    Model.Window.ShowStatus(
                        "Failed to copy Html to the clipboard. Check the application log for more info.",
                        mmApp.Configuration.StatusTimeout, FontAwesomeIcon.Warning, Colors.Firebrick);
                else
                    Model.Window.ShowStatus("Html has been copied to the clipboard.",
                        mmApp.Configuration.StatusTimeout);

                editor.SetEditorFocus();
                editor.Window.PreviewMarkdownAsync();
            }, (p, c) => Model.IsEditorActive);

        }

        public CommandBase GeneratePdfCommand { get; set; }

        void GeneratePdf()
        {
            // PDF GENERATION PREVIEW
            GeneratePdfCommand = new CommandBase((s, e) =>
            {
                var form = new GeneratePdfWindow()
                {
                    Owner = mmApp.Model.Window
                };
                form.Show();
            }, (s, e) =>
            {
                if (!Model.IsEditorActive)
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
            DistractionFreeModeCommand = new CommandBase((s, e) =>
            {
                Model.WindowLayout.SetDistractionFreeMode(!Model.IsFullScreen);
            });
        }


        public CommandBase PresentationModeCommand { get; set; }

        void PresentationMode()
        {
            // PRESENTATION MODE
            PresentationModeCommand = new CommandBase((s, e) =>
            {                
                Model.WindowLayout.SetPresentationMode(!Model.IsPresentationMode);                
            });
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
                    Model.Configuration.Editor.WrapText = !mmApp.Model.Configuration.Editor.WrapText;
                    Model.ActiveEditor?.RestyleEditor();
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
            }, (p, c) => Model.IsEditorActive);
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
            }, (p, c) => Model.IsEditorActive);
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

            }, (p, c) => Model.IsEditorActive);
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
                var path = Path.Combine(App.InitialStartDirectory, "PreviewThemes",Model.Configuration.PreviewTheme);
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

        #region Git


        public CommandBase OpenFromGitRepoCommand { get; set; }

        void OpenFromGitRepo()
        {
            OpenFromGitRepoCommand = new CommandBase((parameter, command) =>
            {
                var form = new GitRepositoryWindow();
                form.Owner = Model.Window;
                form.ShowDialog();

            }, (p, c) => true);
        }


        public CommandBase CommitToGitCommand { get; set; }

        void CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase(async (parameter, e) =>
            {
                var file = parameter as string;
                if (string.IsNullOrEmpty(file))
                file = Model.ActiveDocument?.Filename;

                if (string.IsNullOrEmpty(file))
                    return;

                var gh = new GitHelper();
                if (gh.OpenRepository(file) == null)
                {
                    Model.Window.ShowStatus("This file or folder is not in a Git repository.",6000,FontAwesomeIcon.Warning,Colors.DarkGoldenrod);
                    return;
                }

                Model.ActiveEditor.SaveDocument(Model.ActiveDocument.IsEncrypted);
                
                var form = new GitCommitDialog(file, false); // GitCommitFormModes.ActiveDocument);                
                form.Show();
            }, (s, e) => Model.IsEditorActive);
        }

        public CommandBase OpenGitClientCommand { get; set; }

        void OpenGitClient()
        {
            OpenGitClientCommand = new CommandBase((parameter, command) =>
            {
                var path = parameter as string;
                if (path == null)
                {
                    path = Model.ActiveDocument?.Filename;
                    if(!string.IsNullOrEmpty(path))
                        path = Path.GetDirectoryName(path);
                }

                if (string.IsNullOrEmpty(path))
                    return;
                                
                if (!mmFileUtils.OpenGitClient(path))
                    Model.Window.ShowStatus("Unabled to open Git client.", 6000, FontAwesomeIcon.Warning, Colors.Firebrick);
                else
                    Model.Window.ShowStatus("Git client opened.",6000);
            }, (p, c) => !string.IsNullOrEmpty(Model.Configuration.GitClientExecutable));
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

        #region Sidebar

        public CommandBase CloseLeftSidebarPanelCommand { get; set; }

        void CloseLeftSidebarPanel()
        {
            CloseLeftSidebarPanelCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ShowFolderBrowser(hide: true);
                Model.ActiveEditor?.SetEditorFocus();
            }, (p, c) => true);
        }


        public CommandBase OpenLeftSidebarPanelCommand { get; set; }

        void OpenLeftSidebarPanel()
        {
            OpenLeftSidebarPanelCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ShowLeftSidebar();                
            }, (p, c) => true);
        }

        public CommandBase CloseRightSidebarPanelCommand { get; set; }

        void CloseRightSidebarPanel()
        {
            CloseRightSidebarPanelCommand = new CommandBase((parameter, command) =>
                {
                    Model.Window.ShowRightSidebar(hide: true);
                    Model.ActiveEditor?.SetEditorFocus();
                }, (p, c) => true);
        }

        public CommandBase ShowFolderBrowserCommand { get; set; }


        void ShowFolderBrowser()
        {
            // SHOW FILE BROWSER COMMAND
            ShowFolderBrowserCommand = new CommandBase((s, e) =>
            {
                mmApp.Configuration.FolderBrowser.Visible = !mmApp.Configuration.FolderBrowser.Visible;
                mmApp.Model.Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);
            });
        }


        #endregion

        #region Commands



        public CommandBase SettingsCommand { get; set; }

        void Settings()
        {
            // Settings
            SettingsCommand = new CommandBase((s, e) =>
            {
                try
                {
                    var file = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json");

                    // save settings first so we're looking at current setting
                    Model.Configuration.Write();

                    string fileText = File.ReadAllText(file);
                    if (!fileText.StartsWith("//"))
                    {
                        fileText = "// Reference: http://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm\r\n" +
                                   fileText;
                        File.WriteAllText(file, fileText);
                    }

                    Model.Window.OpenTab(file, syntax: "json");
                }
                catch
                {
                    if (mmApp.Configuration.CommonFolder != mmApp.Configuration.InternalCommonFolder)
                    {
                        mmApp.Configuration.CommonFolder = mmApp.Configuration.InternalCommonFolder;
                        Settings();
                    }
                    else
                    {
                        var msg = $@"We couldn't load the configuration file.

Please check that the configuration folder for Markdown Monster exists. The default location is:

{FileUtils.ExpandPathEnvironmentVariables("%appdata%\\Markdown Monster")}

and that the file contains `markdownmonster.json`. You should also remove `commonfolderlocation.txt` if it exists and points at an invalid location.

If all this fails shut down Markdown Monster, rename or delete `MarkdownMonster.json` and `commonfolderlocation.txt` (if it exists) and restart Markdown Monster.

We're now shutting down the application.
";
                        MessageBox.Show(msg, mmApp.ApplicationName, MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        App.Current.Shutdown();
                    }

                }
            }, null);
        }






        public CommandBase ViewInExternalBrowserCommand { get; set; }

        void ViewInExternalBrowser()
        {
            // EXTERNAL BROWSER VIEW
            ViewInExternalBrowserCommand = new CommandBase((p, e) =>
            {
                if (Model.ActiveDocument == null) return;

                Model.ActiveDocument.RenderHtmlToFile();
                mmFileUtils.ShowExternalBrowser(Model.ActiveDocument.HtmlRenderFilename);
            }, (p, e) => Model.IsEditorActive);
        }


        public CommandBase ViewHtmlSourceCommand { get; set; }

        void ViewHtmlSource()
        {
            ViewHtmlSourceCommand = new CommandBase((p, e) =>
            {
                if (Model.ActiveDocument == null) return;
                Model.ActiveDocument.RenderHtmlToFile();
                Model.Window.OpenTab(Model.ActiveDocument.HtmlRenderFilename);
            }, (p, e) => Model.IsEditorActive);
        }


        public CommandBase PrintPreviewCommand { get; set; }

        void PrintPreview()
        {
            PrintPreviewCommand = new CommandBase(
                (p, e) => Model.Window.PreviewBrowser.ExecuteCommand("PrintPreview"),
                (p, e) => Model.IsEditorActive);

        }





        //private void CreateCommands()
        //{
        //    Command_Settings();

        //    Command_ViewInExternalBrowser();
        //    Command_ViewHtmlSource();
        //    Command_PrintePreview();

        //    Command_ShowFolderBrowser();
        //}

        #endregion
    }
}
