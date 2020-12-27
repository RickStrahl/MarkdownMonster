using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Dragablz;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Controls.ContextMenus;
using MarkdownMonster.Favorites;
using MarkdownMonster.Services;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.ConfigurationEditor;
using Microsoft.Win32;

using Westwind.HtmlPackager;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class AppCommands
    {
        AppModel Model;

        public SpeechCommands Speech { get; }
        public GitCommands Git { get; }

        public AppCommands(AppModel model)
        {
            Model = model;

            // File Operations
            NewDocument();
            OpenDocument();
            OpenRecentDocument();
            OpenFromUrl();

            Save();
            SaveAs();
            SaveAll();

            SaveProject();
            LoadProject();
            CloseProject();

            NewWeblogPost();

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
            TogglePreviewBrowser();
            Settings();
            SettingsVisual();
            SwitchTheme();

            // Editor Commands
            ToolbarInsertMarkdown();
            CloseActiveDocument();
            CloseAllDocuments();
            CloseDocumentsToRight();
            OpenInNewWindow();
            ShowActiveTabsList();
            CopyAsHtml();
            SetDictionary();

            SpellCheck();
            SpellCheckNext();
            SpellCheckPrevious();

            CommandWindow();
            OpenInExplorer();
            OpenWith();
            PasteMarkdownFromHtml();
            MarkdownLinting();
            AddFavorite();
            EditorSplitMode();


            // Preview Browser
            EditPreviewTheme();
            PreviewSyncMode();
            ViewInExternalBrowser();
            ViewHtmlSource();
            RefreshPreview();
            RefreshBrowserContent();


            // Miscellaneous
            OpenAddinManager();
            OpenSearchSidebar();
            ShowSidebarTab();

            Help();
            CopyFolderToClipboard();
            CopyFullPathToClipboard();
            TabControlFileList();
            PasteImageToFile(); // folder browser

            // Sidebar
            CloseLeftSidebarPanel();
            CloseRightSidebarPanel();
            OpenLeftSidebarPanel();
            ToggleLeftSidebarPanel();

            ToggleFolderBrowser();
            OpenFolderBrowser();

            ToggleConsolePanel();
            ClearConsolePanel();



#if NETFULL
            Speech = new SpeechCommands(model);
#endif

            Git = new GitCommands(model);

#if DEBUG
            TestButton();
            //var mi = new MenuItem
            //{
            //    Header = "Test Item"
            //};
            //mi.Click +=  (s,ev)=> TestButtonCommand.Execute(mi.CommandParameter);
            //Model.Window.MainMenuHelp.Items.Add(mi);
#endif
        }

        #region Files And File Management

        public CommandBase NewDocumentCommand { get; set; }

        void NewDocument()
        {
            // NEW DOCUMENT COMMAND (ctrl-n)
            NewDocumentCommand = new CommandBase((parameter, e) =>
            {
                Model.Window.OpenTab("untitled");
                
                if (parameter is string)
                {
                    Model.Window.Dispatcher.InvokeAsync(() =>
                    {
                        Model.ActiveEditor.SetMarkdown(parameter as string);
                        Model.ActiveEditor.PreviewMarkdownCallback();
                    }, DispatcherPriority.ApplicationIdle);
                }
            });
        }

        public CommandBase OpenDocumentCommand { get; set; }

        void OpenDocument()
        {
            // OPEN DOCUMENT COMMAND
            OpenDocumentCommand = new CommandBase((parameter, command) =>
            {
                var file = parameter as String;
                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    Model.Window.OpenTab(file, rebindTabHeaders: true);
                    return;
                }

                WindowUtilities.DoEvents();

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

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder) &&
                    Directory.Exists(mmApp.Configuration.LastFolder))
                    fd.InitialDirectory = mmApp.Configuration.LastFolder;

                bool? res = null;
                try
                {
                    res = fd.ShowDialog();
                }
                catch (Exception ex)
                {
                    mmApp.Log($"Handled. Unable to open file in {fd.InitialDirectory}", ex);
                    MessageBox.Show(
                        $@"Unable to open file:\r\n\r\n" + ex.Message,
                        "An error occurred trying to open a file",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (res == null || !res.Value)
                    return;

                foreach (var filename in fd.FileNames)
                {
                    Model.Window.OpenTab(filename, rebindTabHeaders: true);
                }

                if (fd.FileNames.Length > 0)
                    Model.Configuration.LastFolder = Path.GetDirectoryName(fd.FileNames[0]);

            });
        }


        public CommandBase OpenFromUrlCommand { get; set; }

        void OpenFromUrl()
        {
            OpenFromUrlCommand = new CommandBase((parameter, command) =>
            {
                // If a URL is passed try to open it and display immediately
                var url = parameter as string;
                if (!string.IsNullOrEmpty(url))
                {
                    // Fix up URL for known repositories etc.
                    // to retrieve ra markdown
                    var fileSaver = new FileSaver();
                    var doc = fileSaver.OpenMarkdownDocumentFromUrl(url);

                    if (doc != null)
                    {
                        var urlTab = Model.Window.OpenTab("untitled");
                        ((MarkdownDocumentEditor) urlTab.Tag).MarkdownDocument = doc;
                        Model.Window.PreviewMarkdownAsync();
                    }

                    return;
                }

                var form = new OpenFromUrlDialog();
                form.Owner = Model.Window;
                form.Url = url;
                var result = form.ShowDialog();

                if (result == null || !result.Value || string.IsNullOrEmpty(form.Url))
                    return;

                // Fix up URL for known repositories etc.
                // to retrieve ra markdown
                url = form.Url;
                bool fixupImageLinks = form.FixupImageLinks;
                url = FileSaver.ParseMarkdownUrl(url);

                Model.Window.ShowStatusProgress("Opening document from " + url);

                string markdown;
                try
                {
                    markdown = HttpUtils.HttpRequestString(url);
                }
                catch (Exception ex)
                {
                    Model.Window.ShowStatusError($"Can't open document: {ex.Message} - Url: {url}");
                    mmApp.Log($"OpenFromUrl failed for {url}: {ex.Message}", ex, logLevel: LogLevels.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(markdown))
                {
                    Model.Window.ShowStatusError($"No content found at URL: {url}");
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

                        var newLink = $"![{text}]({linkUrl})";
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
                var editor = tab.Tag as MarkdownDocumentEditor;

                // have to know that the document has loaded before we can set Markdown
                editor.TabLoadingCompleted = OnEditorOnTabLoadingCompleted;

                void OnEditorOnTabLoadingCompleted(MarkdownDocumentEditor ed)
                {
                    ed.SetMarkdown(markdown);
                    ed.SetDirty(true);
                    Model.Window.PreviewMarkdownAsync();
                    ed.TabLoadingCompleted = null;
                }

                Model.Window.ShowStatus();
            });
        }




        public CommandBase OpenRecentDocumentCommand { get; set; }

        void OpenRecentDocument()
        {
            OpenRecentDocumentCommand = new CommandBase((parameter, command) =>
            {
                // make the context menu go away right away so there's no 'ui stutter'
                // don't know how to do the same for
                if (Model.Window.ToolbarButtonRecentFiles.ContextMenu != null)
                    Model.Window.ToolbarButtonRecentFiles.ContextMenu.Visibility = Visibility.Hidden;
                Model.Window.ButtonRecentFiles.IsSubmenuOpen = false;

                var path = parameter as string;
                if (string.IsNullOrEmpty(path))
                    return;

                if (Directory.Exists(path))
                {
                    Model.Window.FolderBrowser.FolderPath = path;
                    Model.Window.ShowFolderBrowser();
                    Model.Configuration.LastFolder = path;
                }
                else
                {
                    var ext = Path.GetExtension(path);
                    if (ext == ".mdproj")
                    {
                        Model.Commands.LoadProjectCommand.Execute(path);
                        return;
                    }

                    var tab = Model.Window.GetTabFromFilename(path);
                    if (tab == null)
                        Model.Window.OpenTab(path, rebindTabHeaders: true);
                    else
                        Model.Window.ActivateTab(tab);
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
                var editor = tab?.Tag as MarkdownDocumentEditor;
                if (editor == null)
                    return;

                if (editor.MarkdownDocument.Filename == "untitled")
                    SaveAsCommand.Execute(tab);
                else if (!editor.SaveDocument())
                {
                    Model.Window.ShowStatusError(
                        "Couldn't save document. Most likely the file is locked or the path is no longer valid.");
                    SaveAsCommand.Execute(tab);
                }

                Model.Window.PreviewMarkdown(editor, keepScrollPosition: true);
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

                if (!UnlockKey.IsUnlocked && isEncrypted)
                {
                    UnlockKey.ShowPremiumDialog("Encrypted File Saving",
                        "https://markdownmonster.west-wind.com/docs/_5fd0qopxq.htm");
                    return;
                }

                var tab = Model.Window.TabControl?.SelectedItem as TabItem;

                var doc = tab?.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                // preset folder: Current doc (if open), Last Used Folder, folder browser Path
                var filename = doc.MarkdownDocument.Filename;
                var folder = Path.GetDirectoryName(doc.MarkdownDocument.Filename);
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                {
                    folder = mmApp.Configuration.LastFolder;
                    if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                        folder = Model.Window?.FolderBrowser?.FolderPath;
                    if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
                        folder = KnownFolders.GetPath(KnownFolder.Libraries);
                }


                if (filename == "untitled")
                {
                    var match = Regex.Match(doc.GetMarkdown(), @"^# (\ *)(?<Header>.+)", RegexOptions.Multiline);

                    if (match.Success)
                    {
                        filename = match.Groups["Header"].Value;
                        if (!string.IsNullOrEmpty(filename))
                            filename = FileUtils.SafeFilename(filename);
                    }
                }



                var sd = new SaveFileDialog
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
                    var pwdDialog = new FilePasswordDialog(doc.MarkdownDocument, false) {Owner = Model.Window};
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

                    Model.Configuration.LastFolder = Path.GetDirectoryName(sd.FileName);
                }


                Model.Window.SetWindowTitle();
                Model.Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) => { return Model.IsEditorActive; });
        }

        public CommandBase SaveAllCommand { get; set; }

        void SaveAll()
        {
            SaveAllCommand = new CommandBase((parameter, command) =>
            {
                var tabs = Model.Window.TabControl.Items;

                foreach (var tabItem in tabs)
                {
                    var tab = tabItem as TabItem;

                    var doc = tab?.Tag as MarkdownDocumentEditor;
                    if (doc == null)
                        continue;

                    if (doc.MarkdownDocument.Filename == "untitled" ||
                        !doc.SaveDocument())
                        SaveAsCommand.Execute(tab);
                }
            }, (p, c) => Model.IsEditorActive);
        }


        public CommandBase NewWeblogPostCommand { get; set; }

        void NewWeblogPost()
        {
            NewWeblogPostCommand = new CommandBase((parameter, command) =>
            {
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            });
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

                var sd = new SaveFileDialog
                {
                    Filter =
                        "Raw Html Output only (Html Fragment)|*.html|" +
                        "Self contained Html File with embedded Styles and Images|*.html|" +
                        "Html Page with loose Assets in a Folder|*.html|" +
                        "Zip Archive of HTML Page  with loose Assets in a Folder|*.zip",
                    FilterIndex = 1,
                    InitialDirectory = folder,
                    FileName = Path.ChangeExtension(doc.MarkdownDocument.Filename, "html"),
                    CheckFileExists = false,
                    OverwritePrompt = true,
                    CheckPathExists = true,
                    RestoreDirectory = true,
                    Title = "Save As Html"
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
                    if (sd.FilterIndex == 1)
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

                        ShellUtils.OpenFileInExplorer(sd.FileName);
                        Model.Window.ShowStatus("Raw HTML File created.", mmApp.Configuration.StatusMessageTimeout);
                        return;
                    }


                    try
                    {
                        Model.Window.ShowStatus("Packing HTML File. This can take a little while.",
                            mmApp.Configuration.StatusMessageTimeout, FontAwesomeIcon.CircleOutlineNotch,
                            color: Colors.Goldenrod, spin: true);
                        bool packageResult = false;

                        var packager = new HtmlPackager();


                        if (sd.FilterIndex != 4 && doc.MarkdownDocument.RenderHtmlToFile(usePragmaLines: false,
                            filename: sd.FileName) == null)
                        {
                            MessageBox.Show(Model.Window,
                                $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                                "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SaveAsHtmlCommand.Execute(null);
                            return;
                        }

                        if (sd.FilterIndex == 2) // single file
                        {
                            var basePath = Path.GetDirectoryName(doc.MarkdownDocument.Filename);
                            packageResult = packager.PackageHtmlToFile(sd.FileName, sd.FileName, basePath);
                        }

                        if (sd.FilterIndex == 4)
                        {
                            // render in-place in the standard preview location
                            doc.MarkdownDocument.RenderHtmlToFile(usePragmaLines: false);

                            var basePath = Path.GetDirectoryName(doc.MarkdownDocument.Filename);
                            packageResult = packager.PackageHtmlToZipFile(doc.MarkdownDocument.HtmlRenderFilename,
                                sd.FileName, basePath);
                        }

                        else if (sd.FilterIndex == 3) // directory
                        {
                            folder = Path.GetDirectoryName(sd.FileName);
                            if (Directory.GetFiles(folder).Any() &&
                                MessageBox.Show("The Html file and resources will be generated here:\r\n\r\n" +
                                                $"Folder: {folder}\r\n" +
                                                $"File: {Path.GetFileName(sd.FileName)}\r\n\r\n" +
                                                "Files already exist in this folder." +
                                                "Are you sure you want to generate Html and resources here?",
                                    "Save As Html file with Loose Resources",
                                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) ==
                                MessageBoxResult.No)
                                return;

                            packageResult = packager.PackageHtmlToFolder(sd.FileName, sd.FileName);
                        }


                        if (packageResult)
                            ShellUtils.OpenFileInExplorer(sd.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Model.Window, "Couldn't package HTML file:\r\n\r\n" + ex.Message,
                            "Couldn't create HTML Package File");
                    }
                    finally
                    {
                        Model.Window.ShowStatus();
                        Model.Window.ShowStatus("Packaged HTML File created.",
                            mmApp.Configuration.StatusMessageTimeout);
                    }
                }
            }, (s, e) =>
            {
                if (!Model.IsEditorActive)
                    return false;
                if (Model.ActiveDocument.Filename == "untitled")
                    return true;
                if (Model.ActiveEditor?.MarkdownDocument.EditorSyntax != "markdown")
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
                if (!ClipboardHelper.CopyHtmlToClipboard(html, showStatusError: true))
                    Model.Window.ShowStatusError(
                        "Failed to copy Html to the clipboard. Check the application log for more info.");
                else
                    Model.Window.ShowStatusSuccess("Html has been copied to the clipboard.");

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
                var form = new GeneratePdfWindow() {Owner = mmApp.Model.Window};
                form.Show();
            }, (s, e) =>
            {
                if (!Model.IsEditorActive)
                    return false;
                if (Model.ActiveDocument.Filename == "untitled")
                    return true;
                if (Model.ActiveEditor.MarkdownDocument.EditorSyntax != "markdown")
                    return false;

                return true;
            });
            GeneratePdfCommand.PremiumFeatureName = "PDF Output Generation";
            GeneratePdfCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_53u1b1dsc.htm";
        }

        #endregion


        #region Projects

        public CommandBase SaveProjectCommand { get; set; }

        void SaveProject()
        {
            SaveProjectCommand = new CommandBase((parameter, command) =>
            {
                var filename = parameter as string;
                if (filename == "Edit_Project")
                {
                    EditProject();
                    return;
                }

                WindowUtilities.DoEvents();

                string folder = Model.Configuration.LastFolder;

                if (string.IsNullOrEmpty(filename))
                {
                    if (!Model.ActiveProject.IsEmpty && File.Exists(Model.ActiveProject.Filename))
                    {
                        folder = Path.GetDirectoryName(Model.ActiveProject.Filename);
                        filename = Path.GetFileName(Model.ActiveProject.Filename);
                    }

                    var sd = new SaveFileDialog
                    {
                        FilterIndex = 1,
                        InitialDirectory = folder,
                        FileName = filename,
                        CheckFileExists = false,
                        OverwritePrompt = false,
                        CheckPathExists = true,
                        RestoreDirectory = true
                    };

                    sd.Filter =
                        "Markdown files (*.mdproj)|*.mdproj|All files (*.*)|*.*";

                    bool? result = null;
                    try
                    {
                        result = sd.ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        mmApp.Log("Unable to save project file: " + filename, ex);
                        MessageBox.Show(
                            $@"Unable to open file:\r\n\r\n" + ex.Message,
                            "An error occurred trying to open a file",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }

                    if (result == null || !result.Value)
                        return;

                    filename = sd.FileName;
                }

                IEnumerable<DragablzItem> headers = null;
                try
                {
                    // Get Ordered Headers
                    var ditems = Model.Window.GetDragablzItems();
                    headers = Model.Window.TabControl.HeaderItemsOrganiser.Sort(ditems);
                    //headers = Model.Window.TabControl.GetOrderedHeaders();
                }
                catch (Exception ex)
                {
                    mmApp.Log("TabControl.GetOrderedHeaders() failed. Saving unordered.", ex,
                        logLevel: LogLevels.Warning);

                    // This works, but doesn't keep tab order intact
                    headers = new List<DragablzItem>();
                    foreach (DragablzItem tab in Model.Window.TabControl.Items)
                    {
                        ((List<DragablzItem>) headers).Add(tab);
                    }
                }

                var documents = new List<OpenFileDocument>();
                if (headers != null)
                {
                    // Important: collect all open tabs in the **original tab order**
                    foreach (var dragablzItem in headers)
                    {
                        if (dragablzItem == null)
                            continue;

                        var tab = dragablzItem.Content as TabItem;

                        var editor = tab.Tag as MarkdownDocumentEditor;
                        var doc = editor?.MarkdownDocument;
                        if (doc == null)
                            continue;

                        doc.LastEditorLineNumber = editor.GetLineNumber();
                        if (doc.LastEditorLineNumber < 1)
                            doc.LastEditorLineNumber =
                                editor.InitialLineNumber; // if document wasn't accessed line is never set
                        if (doc.LastEditorLineNumber < 0)
                            doc.LastEditorLineNumber = 0;

                        documents.Add(new OpenFileDocument(doc));
                    }
                }

                Model.ActiveProject.Filename = filename;
                Model.ActiveProject.ActiveSidebarIndex = Model.Window.SidebarContainer.SelectedIndex;

                Model.ActiveProject.ActiveFolder = Model.Window.FolderBrowser.FolderPath;
                if (string.IsNullOrEmpty(Model.ActiveProject.ActiveFolder))
                    Model.ActiveProject.ActiveFolder = Path.GetDirectoryName(filename);


                Model.ActiveProject.OpenDocuments = documents;
                if (!Model.ActiveProject.Save(filename))
                    Model.Window.ShowStatusError("Failed to save the project file.");
                else
                {
                    Model.Window.ShowStatusSuccess("Project file saved.");
                    Model.Configuration.LastFolder = Path.GetDirectoryName(filename);
                }

                // force window title to update
                Model.Window.SetWindowTitle();
            }, (p, c) => true);
        }

        private void EditProject()
        {
            if (Model.ActiveProject == null)
                return;

            if (string.IsNullOrEmpty(Model.ActiveProject.Filename))
            {
                Model.Window.ShowStatusError(
                    "Can't modify project settings on an unsaved project. Please save the project first.");
                return;
            }

            Model.ActiveProject.Save();

            Model.Window.OpenTab(Model.ActiveProject.Filename);
        }


        public CommandBase LoadProjectCommand { get; set; }

        void LoadProject()
        {
            LoadProjectCommand = new CommandBase((parameter, command) =>
            {
                var window = Model.Window;


                string filename = null;
                string folder = Model.Configuration.LastFolder;

                if (parameter != null)
                {
                    filename = parameter as string;
                }
                else
                {
                    var fd = new OpenFileDialog
                    {
                        DefaultExt = ".mdproj",
                        Filter = "Html files (*.mdproj)|*.mdproj|" +
                                 "All files (*.*)|*.*",
                        CheckFileExists = true,
                        RestoreDirectory = true,
                        Multiselect = false,
                        Title = "Open Markdown Project"
                    };

                    if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
                        fd.InitialDirectory = mmApp.Configuration.LastFolder;

                    var res = fd.ShowDialog();
                    if (res == null || !res.Value)
                        return;

                    filename = fd.FileName;
                }

                if (!File.Exists(filename))
                {
                    window.ShowStatusError("Project file doesn't exist: " + filename);
                    return;
                }

                var project = MarkdownMonsterProject.Load(filename);
                if (project == null)
                {
                    window.ShowStatusError("Project file is invalid: " + filename);
                    window.OpenTab(filename);
                    return;
                }


                window.CloseAllTabs();

                window.batchTabAction = true;

                TabItem selectedTab = null;
                foreach (var doc in project.OpenDocuments)
                {
                    if (doc.Filename == null)
                        continue;

                    if (File.Exists(doc.Filename))
                    {
                        var tab = window.OpenTab(doc.Filename,
                            selectTab: false,
                            batchOpen: true,
                            initialLineNumber: doc.LastEditorLineNumber);

                        if (tab == null)
                            continue;

                        var editor = tab.Tag as MarkdownDocumentEditor;
                        if (editor == null)
                            continue;

                        if (doc.IsActive)
                        {
                            selectedTab = tab;

                            // have to explicitly notify initial activation
                            // since we surpress it on all tabs during startup

                            AddinManager.Current.RaiseOnDocumentActivated(editor.MarkdownDocument);
                        }
                    }
                }

                window.batchTabAction = false;

                window.TabControl.SelectedIndex = -1;
                window.TabControl.SelectedItem = null;

                if (selectedTab == null)
                    window.TabControl.SelectedIndex = 0;
                else
                    window.TabControl.SelectedItem = selectedTab;

                Model.Configuration.LastFolder = Path.GetDirectoryName(filename);

                window.Dispatcher.InvokeAsync(
                    () =>
                    {
                        // force window title to update
                        window.SetWindowTitle();

                        string activeFolder = project.ActiveFolder;
                        if (string.IsNullOrEmpty(activeFolder) || !Directory.Exists(activeFolder))
                            activeFolder = Path.GetDirectoryName(project.Filename);

                        Model.Window.FolderBrowser.FolderPath = activeFolder;

                        if (project.ActiveSidebarIndex > -1)
                            Model.Window.SidebarContainer.SelectedIndex = project.ActiveSidebarIndex;
                    },
                    DispatcherPriority.ApplicationIdle);


                Model.ActiveProject = project;
                window.AddRecentFile(filename);

                window.ShowStatusSuccess("Project opened: " + filename);
            }, (p, c) => true);
        }


        public CommandBase CloseProjectCommand { get; set; }

        void CloseProject()
        {
            CloseProjectCommand = new CommandBase((parameter, command) =>
            {
                if (Model.ActiveProject.IsEmpty)
                    return;

                var filename = Model.ActiveProject.Filename;
                Model.ActiveProject = new MarkdownMonsterProject();
                Model.Window.ShowStatusSuccess("Project " + Path.GetFileName(filename) + " closed.");

                // force window title to update
                Model.Window.SetWindowTitle();
            }, (p, c) => !Model.ActiveProject.IsEmpty);
        }

        #endregion


        #region Links and External Access Commands

        public CommandBase OpenSampleMarkdownCommand { get; set; }

        void OpenSampleMarkdown()
        {
            OpenSampleMarkdownCommand = new CommandBase((parameter, command) =>
            {
                mmApp.Model.Commands.OpenFromUrlCommand.Execute(
                    "https://github.com/RickStrahl/MarkdownMonster/blob/master/SampleDocuments/SampleMarkdown.md");


                string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                File.Copy(Path.Combine(App.InitialStartDirectory, "SampleMarkdown.md"), tempFile, true);
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
            });
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
                    Model.Window.ShowStatusError(
                        "Didn't remove formatting. No selection or document is not a Markdown document.");
                }
            });
        }


        public CommandBase DistractionFreeModeCommand { get; set; }

        void DistractionFreeMode()
        {
            DistractionFreeModeCommand = new CommandBase((p, e) =>
            {
                if (p is string)
                    // toggle
                    Model.IsFullScreen = !Model.IsFullScreen;

                Model.WindowLayout.SetDistractionFreeMode(!Model.IsFullScreen);
            });
        }


        public CommandBase PresentationModeCommand { get; set; }

        void PresentationMode()
        {
            // PRESENTATION MODE
            PresentationModeCommand = new CommandBase((p, e) =>
            {
                // toggle
                if (p?.ToString() == "Toggle")
                    Model.IsPresentationMode = !Model.IsPresentationMode;

                Model.WindowLayout.SetPresentationMode(!Model.IsPresentationMode);
            });
        }


        public CommandBase TogglePreviewBrowserCommand { get; set; }

        void TogglePreviewBrowser()
        {
            var window = Model.Window;
            var config = Model.Configuration;

            TogglePreviewBrowserCommand = new CommandBase((p, e) =>
            {
                var tab = window.TabControl.SelectedItem as TabItem;
                var editor = tab?.Tag as MarkdownDocumentEditor;
                if (editor == null)
                    return;

                bool toggle = p?.ToString() == "Toggle";
                if (toggle)
                    Model.IsPreviewBrowserVisible = !Model.IsPreviewBrowserVisible;

                Model.WindowLayout.IsPreviewVisible = Model.IsPreviewBrowserVisible;

                config.IsPreviewVisible = Model.IsPreviewBrowserVisible;

                if (!Model.IsPreviewBrowserVisible && Model.IsPresentationMode)
                    PresentationModeCommand.Execute(null);

                window.ShowPreviewBrowser(!Model.IsPreviewBrowserVisible);
                if (Model.IsPreviewBrowserVisible)
                    window.PreviewMarkdown(editor);
            }, null);
        }


        public CommandBase WordWrapCommand { get; set; }

        void WordWrap()
        {
            WordWrapCommand = new CommandBase((parameter, command) =>
                {
                    Model.Configuration.Editor.WrapText = !mmApp.Model.Configuration.Editor.WrapText;
                    Model.ActiveEditor?.RestyleEditor();
                },
                (p, c) => Model.IsEditorActive);
        }

        #endregion

        #region Editor CommandsToolTip;ToolTip;

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



        public CommandBase CloseDocumentsToRightCommand { get; set; }

        void CloseDocumentsToRight()
        {
            CloseDocumentsToRightCommand = new CommandBase((parameter, command) =>
            {
                var current = Model.Window.TabControl.SelectedItem as TabItem;
                if (current == null)
                    return;

                var ditems = Model.Window.GetDragablzItems();
                var headers = Model.Window.TabControl.HeaderItemsOrganiser.Sort(ditems).ToList();

                bool thisTabFound = false;
                foreach (var dragItem in headers)
                {
                    var tab = dragItem.Content as TabItem;
                    if (current == tab)
                    {
                        thisTabFound = true;
                        continue;
                    }

                    if (thisTabFound)
                        Model.Window.CloseTab(tab);
                }

                Model.Window.BindTabHeaders();
            }, (p, c) => Model.IsEditorActive);
        }



        public CommandBase OpenInNewWindowCommand { get; set; }

        void OpenInNewWindow()
        {
            OpenInNewWindowCommand = new CommandBase((parameter, command) =>
            {
                var file = parameter as string;
                if (parameter == null)
                    file = Model.ActiveDocument?.Filename;
                if (file == null)
                    return;

                var tab = Model.Window.GetTabFromFilename(file);
                if (tab != null)
                    Model.Window.CloseTab(tab);

                var args = $"-newwindow -nosplash \"{file}\"";
                ShellUtils.ExecuteProcess("markdownmonster.exe", args);

                Model.Window.ShowStatusProgress($"Opening document {file} in a new window...", 3000);
            }, (p, c) => Model.IsEditorActive);
        }

        void MarkdownLinting()
        {
            MarkdownLintingCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                var markdown = editor.GetMarkdown();

                var errors = LintingErrorsModel.MarkdownLinting(markdown);
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
                if (tab == null)
                    return;

                tab.IsSelected = true;
            });
        }


        public CommandBase WindowMenuCommand { get; set; }


        void ShowActiveTabsList()
        {
            WindowMenuCommand = new CommandBase((parameter, command) =>
            {
                var wmMenuContextMenu = new WindowMenuContextMenu();
                wmMenuContextMenu.CreateWindowMenuContextMenu();
            });
        }



        public CommandBase SetDictionaryCommand { get; set; }

        void SetDictionary()
        {
            SetDictionaryCommand = new CommandBase((parameter, command) =>
            {
                var lang = parameter as string;
                if (string.IsNullOrEmpty(lang))
                    return;

                if (lang == "REMOVE-DICTIONARIES")
                {
                    if (string.IsNullOrEmpty(SpellChecker.ExternalDictionaryFolder) ||
                        !Directory.Exists(SpellChecker.ExternalDictionaryFolder))
                        return;

                    bool error = false;
                    foreach (var file in Directory.GetFiles(SpellChecker.ExternalDictionaryFolder))
                    {
                        if (file.EndsWith(".dic") || file.EndsWith(".aff"))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                                error = true;
                            }
                        }
                    }

                    if (error)
                        Model.Window.ShowStatusError("Some dictionaries were not deleted.");
                    else
                        Model.Window.ShowStatusSuccess("Dictionaries deleted.");

                    lang = "en-US";
                }

                if (!File.Exists(Path.Combine(SpellChecker.InternalDictionaryFolder, lang + ".dic")) &&
                    !File.Exists(Path.Combine(SpellChecker.ExternalDictionaryFolder, lang + ".dic")))
                {
                    if (!SpellChecker.AskForLicenseAcceptance(lang))
                        return;

                    Model.Window.ShowStatusProgress($"Downloading dictionary for: {lang}");

                    if (SpellChecker.DownloadDictionary(lang))
                        Model.Window.ShowStatusSuccess($"Downloaded dictionary: {lang}");
                    else
                        Model.Window.ShowStatusError("Failed to download dictionary.");
                }

                Model.Configuration.Editor.Dictionary = lang;

                try
                {
                    SpellChecker.GetSpellChecker(lang, true); // force language to reset
                }
                catch (Exception ex)
                {
                    mmApp.Model.Window.ShowStatusError(ex.Message); // couldn't set resetting to English
                }

                if (mmApp.Configuration.Editor.EnableSpellcheck)
                    Model.ActiveEditor?.SpellCheckDocument();
                else
                {
                    mmApp.Configuration.Editor.EnableSpellcheck = true;
                    Model.ActiveEditor?.RestyleEditor();
                }

                Model.Window.ShowStatusSuccess($"Spell checking dictionary changed to: {lang}.",
                    Model.Configuration.StatusMessageTimeout);
            });
        }


        public CommandBase SpellCheckCommand { get; set; }

        void SpellCheck()
        {
            SpellCheckCommand = new CommandBase((parameter, command) =>
            {
                Model.ActiveEditor?.RestyleEditor();
                Model.Window.ShowStatusSuccess(
                    $"Spell checking has been turned {(Model.Configuration.Editor.EnableSpellcheck ? "on" : "off")}.");
            }, (p, c) => true);
        }


        public CommandBase SpellCheckNextCommand { get; set; }

        void SpellCheckNext()
        {
            SpellCheckNextCommand =
                new CommandBase(
                    (parameter, command) => { Model.ActiveEditor?.AceEditor?.Invoke("spellcheckNext", false); },
                    (p, c) => Model.ActiveEditor?.MarkdownDocument.EditorSyntax == "markdown");
        }


        public CommandBase SpellCheckPreviousCommand { get; set; }

        void SpellCheckPrevious()
        {
            SpellCheckPreviousCommand =
                new CommandBase(
                    (parameter, command) => { Model.ActiveEditor?.AceEditor?.Invoke("spellcheckPrevious", false); },
                    (p, c) => Model.ActiveEditor?.MarkdownDocument.EditorSyntax == "markdown");
        }


        public CommandBase OpenWithCommand { get; set; }

        void OpenWith()
        {
            OpenWithCommand = new CommandBase((parameter, command) =>
            {
                string file = parameter as string;
                if (string.IsNullOrEmpty(file))
                    return;

                mmFileUtils.ShowOpenWithDialog(file);
            }, (p, c) => true);
        }


        public CommandBase PasteMarkdownFromHtmlCommand { get; set; }

        void PasteMarkdownFromHtml()
        {
            PasteMarkdownFromHtmlCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                string html = null;

                // Check for HTML content out of a browser/RTF/Visual Studio etc.
                if (Clipboard.ContainsText(TextDataFormat.Html))
                    html = Clipboard.GetText(TextDataFormat.Html);

                if (!string.IsNullOrEmpty(html))
                    html = StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
                else
                    html = ClipboardHelper.GetText(); // Probably just plain HTML text

                if (string.IsNullOrEmpty(html))
                    return;

                var markdown = MarkdownUtilities.HtmlToMarkdown(html);

                editor.SetSelection(markdown);
                editor.SetEditorFocus();

                Model.Window.PreviewBrowser?.PreviewMarkdownAsync(editor, true);
            }, (p, c) => true);
            PasteMarkdownFromHtmlCommand.PremiumFeatureName = "HTML to Markdown Conversion";
        }


        public CommandBase MarkdownLintingCommand { get; set; }


        public CommandBase AddFavoriteCommand { get; set; }

        void AddFavorite()
        {
            AddFavoriteCommand = new CommandBase((parameter, command) =>
            {
                var file = parameter as string;
                if (parameter == null)
                    return;

                Model.Window.OpenFavorites();

                var control = Model.Window.FavoritesTab.Content as FavoritesControl;
                var fav = control.FavoritesModel;

                var display = Path.GetFileNameWithoutExtension(file).Replace("-", " ").Replace("_", " ");
                display = StringUtils.FromCamelCase(display);

                var favorite = fav.AddFavorite(null, new FavoriteItem {File = file, Title = display});
                fav.SaveFavorites();

                fav.EditedFavorite = favorite;
                favorite.DisplayState.IsEditing = true;
                control.TextFavoriteTitle.Focus();

                Model.Window.Dispatcher.Delay(900, (p) => control.TextFavoriteTitle.Focus(), null);
            }, (p, c) => true);
        }


        public CommandBase EditorSplitModeCommand { get; set; }

        void EditorSplitMode()
        {
            EditorSplitModeCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                var mode = parameter as string;
                if (mode == null)
                    return;

                switch (mode)
                {
                    case "Beside":
                        editor.SplitEditor(EditorSplitModes.Beside);
                        break;
                    case "Below":
                        editor.SplitEditor(EditorSplitModes.Below);
                        break;
                    case "None":
                        editor.SplitEditor(EditorSplitModes.None);
                        break;
                }
            }, (p, c) => Model.IsEditorActive);
            EditorSplitModeCommand.PremiumFeatureName = "Editor Split Mode";
            EditorSplitModeCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_5ea0q9ne2.htm";
        }

        #endregion

        #region Preview

        public CommandBase EditPreviewThemeCommand { get; set; }

        void EditPreviewTheme()
        {
            EditPreviewThemeCommand = new CommandBase((parameter, command) =>
            {
                var path = Path.Combine(App.InitialStartDirectory, "PreviewThemes", Model.Configuration.PreviewTheme);
                ShellUtils.OpenFileInExplorer(path);

                mmFileUtils.ShowExternalBrowser("https://markdownmonster.west-wind.com/docs/_4nn17bfic.htm");
            });
        }


        public CommandBase PreviewSyncModeCommand { get; set; }

        void PreviewSyncMode()
        {
            PreviewSyncModeCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ComboBoxPreviewSyncModes.Focus();
                WindowUtilities.DoEvents();
                Model.Window.ComboBoxPreviewSyncModes.IsDropDownOpen = true;
            });
        }

        #endregion

        #region Open Document Operations

        public CommandBase CopyFolderToClipboardCommand { get; set; }

        void CopyFolderToClipboard()
        {
            CopyFolderToClipboardCommand = new CommandBase((parameter, command) =>
            {
                var filename = Model.ActiveTabFilename;
                if (string.IsNullOrEmpty(filename) || filename == "untitled")
                    return;

                string path = Path.GetDirectoryName(filename);

                if (ClipboardHelper.SetText(path))
                    Model.Window.ShowStatus($"Path copied to clipboard: {path}",
                        mmApp.Configuration.StatusMessageTimeout);
            });
        }


        public CommandBase CopyFullPathToClipboardCommand { get; set; }

        void CopyFullPathToClipboard()
        {
            CopyFullPathToClipboardCommand = new CommandBase((parameter, command) =>
            {
                var filename = Model.ActiveTabFilename;
                if (string.IsNullOrEmpty(filename) || filename == "untitled")
                    return;

                if (ClipboardHelper.SetText(filename))
                    Model.Window.ShowStatus($"Path copied to clipboard: {filename}",
                        mmApp.Configuration.StatusMessageTimeout);
            }, (p, c) => true);
        }


        /// <summary>
        ///  Open in Terminal Window
        /// </summary>
        public CommandBase CommandWindowCommand { get; set; }

        void CommandWindow()
        {
            CommandWindowCommand = new CommandBase((parameter, command) =>
            {
                var filename = Model.ActiveTabFilename;
                if (string.IsNullOrEmpty(filename))
                    return;

                string path = Path.GetDirectoryName(filename);
                mmFileUtils.OpenTerminal(path);
            }, (p, c) => true);
        }


        public CommandBase OpenInExplorerCommand { get; set; }

        void OpenInExplorer()
        {
            OpenInExplorerCommand = new CommandBase((parameter, command) =>
            {
                var filename = Model.ActiveTabFilename;
                if (string.IsNullOrEmpty(filename))
                    return;

                ShellUtils.OpenFileInExplorer(filename);
            }, (p, c) => true);
        }

        #endregion


        #region Miscellaneous

        public CommandBase AddinManagerCommand { get; set; }

        void OpenAddinManager()
        {
            AddinManagerCommand = new CommandBase((parameter, command) =>
            {
                var form = new AddinManagerWindow {Owner = Model.Window};
                form.Show();
            });
        }



        public CommandBase OpenSearchSidebarCommand { get; set; }

        void OpenSearchSidebar()
        {
            OpenSearchSidebarCommand = new CommandBase((parameter, command) =>
            {
                var path = parameter as string;
                if (string.IsNullOrEmpty(path))
                {
                    path = Model.ActiveProject?.Filename;
                    if (string.IsNullOrEmpty(path))
                        path = Path.GetDirectoryName(Model.ActiveDocument?.Filename);
                }

                var searchControl = Model.Window.OpenSearchPane();

                if (!string.IsNullOrEmpty(path))
                    searchControl.Model.SearchFolder = path;

                Model.Window.Dispatcher.InvokeAsync(() => searchControl.SearchPhrase.Focus(),
                    DispatcherPriority.ApplicationIdle);
            }, (p, c) => true);
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
            });
        }


        public CommandBase PasteImageToFileCommand { get; set; }

        void PasteImageToFile()
        {
            PasteImageToFileCommand = new CommandBase((parameter, command) =>
            {
                if (!Clipboard.ContainsImage())
                {
                    Model.Window.ShowStatusError("No image found on the Clipboard.");
                    return;
                }

                using (var bitMap = System.Windows.Forms.Clipboard.GetImage())
                {
                    if (bitMap == null)
                    {
                        Model.Window.ShowStatusError("An error occurred pasting an image from the clipboard.");
                        return;
                    }

                    var initialFolder = parameter as string;
                    if (initialFolder is null)
                        return;

                    if (initialFolder == "..")
                        initialFolder = Model.Window.FolderBrowser.FolderPath;

                    if (File.Exists(initialFolder))
                        initialFolder = Path.GetDirectoryName(initialFolder);

                    WindowUtilities.DoEvents();

                    var sd = new SaveFileDialog
                    {
                        Filter = "Image files (*.png;*.jpg;*.gif;)|*.png;*.jpg;*.jpeg;*.gif|All Files (*.*)|*.*",
                        FilterIndex = 1,
                        Title = "Save Image from Clipboard as",
                        InitialDirectory = initialFolder,
                        CheckFileExists = false,
                        OverwritePrompt = true,
                        CheckPathExists = true,
                        RestoreDirectory = true,
                        ValidateNames = true
                    };
                    var result = sd.ShowDialog();
                    string imagePath = null;
                    if (result != null && result.Value)
                    {
                        imagePath = sd.FileName;
                        var ext = Path.GetExtension(imagePath)?.ToLower();

                        try
                        {
                            File.Delete(imagePath);

                            if (ext == ".jpg" || ext == ".jpeg")
                            {
                                using (var bmp = new Bitmap(bitMap))
                                {
                                    mmImageUtils.SaveJpeg(bmp, imagePath,
                                        Model.Configuration.Images.JpegImageCompressionLevel);
                                }
                            }
                            else
                            {
                                var format = mmImageUtils.GetImageFormatFromFilename(imagePath);
                                bitMap.Save(imagePath, format);
                            }

                            if (ext == ".png" || ext == ".jpeg" || ext == ".jpg")
                                mmFileUtils.OptimizeImage(sd.FileName); // async

                            // need to delay as item is added by file watcher
                            Model.Window.Dispatcher.Delay(150, (p) =>
                            {
                                Model.Window.SidebarContainer.SelectedItem = mmApp.Model.Window.TabFolderBrowser;
                                Model.Window.ShowFolderBrowser(folder: imagePath);

                                Model.Window.Dispatcher.InvokeAsync(() =>
                                        Model.Window.FolderBrowser.HandleItemSelection(forceEditorFocus: true),
                                    DispatcherPriority.ApplicationIdle);
                            }, DispatcherPriority.ApplicationIdle);
                        }
                        catch (Exception ex)
                        {
                            Model.Window.ShowStatusError("Couldn't save image file: " + ex.Message);
                            return;
                        }
                    }
                }
            }, (p, c) => true);
        }


        #endregion

        #region Git

        public CommandBase OpenFromGitRepoCommand { get; set; }

        void OpenFromGitRepo()
        {
            OpenFromGitRepoCommand = new CommandBase((parameter, command) =>
            {
                GitRepositoryWindowMode startupMode = GitRepositoryWindowMode.Clone;
                var parm = parameter as string;
                if (!string.IsNullOrEmpty(parm))
                    Enum.TryParse(parm, out startupMode);

                var form = new GitRepositoryWindow(startupMode);
                form.Owner = Model.Window;

                var curPath = Model.Window.FolderBrowser.FolderPath;
                if (string.IsNullOrEmpty(curPath))
                {
                    curPath = Model.ActiveDocument?.Filename;
                    if (!string.IsNullOrEmpty(curPath))
                        curPath = Path.GetDirectoryName(curPath);
                }

                if (!string.IsNullOrEmpty(curPath))
                {
                    if (startupMode == GitRepositoryWindowMode.AddRemote)
                    {
                        var root = GitHelper.FindGitRepositoryRoot(curPath);
                        if (root != null)
                            form.LocalPath = root;
                    }
                    else if (startupMode == GitRepositoryWindowMode.Create)
                    {
                        form.LocalPath = curPath;
                    }
                }

                form.ShowDialog();
            });
            OpenFromGitRepoCommand.PremiumFeatureName = "Git Support";
            OpenFromGitRepoCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }


        public CommandBase CommitToGitCommand { get; set; }

        void CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase((parameter, e) =>
            {
                var file = parameter as string;
                if (string.IsNullOrEmpty(file))
                    file = Model.ActiveDocument?.Filename;

                if (string.IsNullOrEmpty(file))
                    return;

                var gh = new GitHelper();
                var repo = gh.OpenRepository(file);
                if (repo == null)
                {
                    Model.Window.ShowStatusError("This file or folder is not in a Git repository.");
                    return;
                }

                var changes = gh.GetRepositoryChanges(repo.Info.WorkingDirectory);
                if (changes.Count < 1)
                    Model.Window.ShowStatusError(
                        $"There are no pending changes for this Git repository: {repo.Info.WorkingDirectory}");

                if (Model.ActiveEditor != null)
                    Model.ActiveEditor.SaveDocument(Model.ActiveDocument.IsEncrypted);

                var form = new GitCommitDialog(file, false); // GitCommitFormModes.ActiveDocument);
                form.Show();
            }, (s, e) => Model.IsEditorActive);
            CommitToGitCommand.PremiumFeatureName = "Git Support";
            CommitToGitCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }

        public CommandBase OpenGitClientCommand { get; set; }

        void OpenGitClient()
        {
            OpenGitClientCommand = new CommandBase((parameter, command) =>
            {
                var path = parameter as string;
                if (path == null)
                {
                    path = Model.ActiveTabFilename;
                    if (!string.IsNullOrEmpty(path))
                        path = Path.GetDirectoryName(path);
                }

                if (string.IsNullOrEmpty(path))
                    return;

                if (!mmFileUtils.OpenGitClient(path))
                    Model.Window.ShowStatusError("Unabled to open Git client.");
                else
                    Model.Window.ShowStatus("Git client opened.", mmApp.Configuration.StatusMessageTimeout);
            }, (p, c) => !string.IsNullOrEmpty(Model.Configuration.Git.GitClientExecutable));
            OpenGitClientCommand.PremiumFeatureName = "Git Support";
            OpenGitClientCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }


        public CommandBase OpenOnGithubCommand { get; set; }

        void OpenOnGithub()
        {
            OpenOnGithubCommand = new CommandBase((parameter, command) =>
            {
                var filename = parameter as string;
                if (parameter == null)
                    return;

                var CommitModel = new GitCommitModel(filename);

                using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
                {
                    var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                    if (remoteUrl == null)
                        return;

                    var relativeFilename = FileUtils.GetRelativePath(filename, repo.Info.WorkingDirectory)
                        .Replace("\\", "/");

                    remoteUrl = remoteUrl.Replace(".git", "");
                    remoteUrl += "/blob/master/" + relativeFilename;

                    Model.Window.ShowStatus("Opening Url: " + remoteUrl);
                    ShellUtils.GoUrl(remoteUrl);
                }
            });
        }

        #endregion


        #region Static Menus Accessed from Control Templates

        public static CommandBase TabWindowListCommand { get; }

        static AppCommands()
        {
            TabWindowListCommand = new CommandBase((parameter, command) =>
            {
                var button = parameter as FrameworkElement;
                if (button == null) return;

                var menuItems = mmApp.Model.Window.GenerateContextMenuItemsFromOpenTabs();

                var contextMenu = new ContextMenu();
                foreach (var mi in menuItems)
                    contextMenu.Items.Add(mi);

                contextMenu.PlacementTarget = button;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                contextMenu.IsOpen = true;
            });
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
            });
        }


        public CommandBase OpenLeftSidebarPanelCommand { get; set; }

        void OpenLeftSidebarPanel()
        {
            OpenLeftSidebarPanelCommand = new CommandBase((parameter, command) => { Model.Window.ShowLeftSidebar(); });
        }

        public CommandBase ToggleLeftSidebarPanelCommand { get; set; }

        void ToggleLeftSidebarPanel()
        {
            ToggleLeftSidebarPanelCommand = new CommandBase((parameter, command) =>
            {
                Model.WindowLayout.IsLeftSidebarVisible = !Model.WindowLayout.IsLeftSidebarVisible;
            });
        }

        public CommandBase CloseRightSidebarPanelCommand { get; set; }

        void CloseRightSidebarPanel()
        {
            CloseRightSidebarPanelCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ShowRightSidebar(hide: true);
                Model.ActiveEditor?.SetEditorFocus();
            });
        }

        public CommandBase ToggleFolderBrowserCommand { get; set; }


        void ToggleFolderBrowser()
        {
            // SHOW FILE BROWSER COMMAND
            ToggleFolderBrowserCommand = new CommandBase((s, e) =>
            {
                mmApp.Configuration.FolderBrowser.Visible = !mmApp.Configuration.FolderBrowser.Visible;
                mmApp.Model.Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);
            });
        }


        public CommandBase OpenFolderBrowserCommand { get; set; }

        void OpenFolderBrowser()
        {
            OpenFolderBrowserCommand = new CommandBase((parameter, command) =>
            {
                string fileOrFolderPath = parameter as string;
                if (string.IsNullOrEmpty(fileOrFolderPath))
                {
                    fileOrFolderPath = Model.ActiveTabFilename;
                    if (string.IsNullOrEmpty(fileOrFolderPath))
                        return;
                }

                mmApp.Model.Window.SidebarContainer.SelectedItem = Model.Window.TabFolderBrowser;
                mmApp.Model.Window.ShowFolderBrowser(folder: fileOrFolderPath);
            }, (o, c) => Model.IsEditorActive );
        }

        public CommandBase ShowSidebarTabCommand { get; set; }

        void ShowSidebarTab()
        {
            ShowSidebarTabCommand = new CommandBase((parameter, command) =>
            {
                string action = parameter as string;


                if (action == "DocumentOutline")
                {
                    if (Model.ActiveEditor == null && Model.ActiveEditor.MarkdownDocument.EditorSyntax != "markdown")
                        return;

                    Model.Configuration.IsDocumentOutlineVisible = true;
                    Model.WindowLayout.IsLeftSidebarVisible = true;
                    Model.Window.SidebarContainer.SelectedItem = Model.Window.TabDocumentOutline;
                }
                else if (action == "FolderBrowser")
                {
                    Model.WindowLayout.IsLeftSidebarVisible = true;
                    Model.Window.SidebarContainer.SelectedItem = Model.Window.TabFolderBrowser;
                }
                else if (action == "Favorites")
                {
                    Model.WindowLayout.IsLeftSidebarVisible = true;
                    Model.Window.SidebarContainer.SelectedItem = Model.Window.FavoritesTab;
                }
                else
                    return;

                Model.WindowLayout.IsLeftSidebarVisible = true;
            }, (p, c) => Model.IsEditorActive);
        }


        public CommandBase ToggleConsolePanelCommand { get; set; }

        void ToggleConsolePanel()
        {
            ToggleConsolePanelCommand = new CommandBase((parameter, command) =>
            {
                Model.WindowLayout.IsConsolePanelVisible = !Model.WindowLayout.IsConsolePanelVisible;
                if (!Model.WindowLayout.IsConsolePanelVisible)
                    Model.Window.ConsolePanel.Hide();
                else
                    Model.Window.ConsolePanel.Show();
            }, (p, c) => true);
        }


        public CommandBase ClearConsolePanelCommand { get; set; }

        void ClearConsolePanel()
        {
            ClearConsolePanelCommand =
                new CommandBase((parameter, command) => { Model.Console.Clear(); }, (p, c) => true);
        }


        #endregion

        #region Commands


        public CommandBase SettingsVisualCommand { get; set; }

        void SettingsVisual()
        {
            SettingsVisualCommand = new CommandBase((parameter, command) =>
            {
                if (mmApp.OpenWindows.ConfigurationEditor == null || !mmApp.OpenWindows.ConfigurationEditor.IsLoaded)
                    mmApp.OpenWindows.ConfigurationEditor = new ConfigurationEditorWindow() {Owner = Model.Window};

                // optional parameter for text to jump to
                var searchFor = parameter as string;

                if (!string.IsNullOrEmpty(searchFor))
                    mmApp.OpenWindows.ConfigurationEditor.Model.SearchText = searchFor;

                mmApp.OpenWindows.ConfigurationEditor.Show();
            }, (p, c) => true);
        }



        public CommandBase SettingsCommand { get; set; }

        void Settings()
        {
            // Settings
            SettingsCommand = new CommandBase((parm, command) =>
            {
                // optional parameter for text to jump to
                var searchFor = parm as string;

                try
                {
                    if (mmApp.OpenWindows.ConfigurationEditor != null)
                        mmApp.OpenWindows.ConfigurationEditor = null;

                    var file = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json");

                    // save settings first so we're looking at current setting
                    Model.Configuration.Write();

                    string fileText = File.ReadAllText(file);
                    if (!fileText.StartsWith("//"))
                    {
                        fileText = "// Reference: https://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm\r\n" +
                                   fileText;
                        File.WriteAllText(file, fileText);
                    }

                    Model.Window.OpenTab(file, syntax: "json");

                    if (searchFor != null)
                    {
                        Model.Window.Dispatcher.InvokeAsync(() => { Model.ActiveEditor.AceEditor.FindText(searchFor); },
                            DispatcherPriority.ApplicationIdle);
                    }
                }
                catch
                {
                    if (!App.IsPortableMode &&
                        mmApp.Configuration.CommonFolder != mmApp.Configuration.InternalCommonFolder)
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
            });
        }



        public CommandBase SwitchThemeCommand { get; set; }

        void SwitchTheme()
        {


            SwitchThemeCommand = new CommandBase((parameter, command) =>
            {
                var window = mmApp.Model?.Window;
                if(window == null)
                    return;

                var selectedTheme = Themes.Dark;

                // Parameter is text for a theme or empty in which case it's toggled
                var text = parameter as string;
                if (string.IsNullOrEmpty(text))
                {
                    // toggle theme
                    if (mmApp.Configuration.ApplicationTheme == Themes.Dark)
                        selectedTheme = Themes.Light;
                }
                else
                    selectedTheme = (Themes) Enum.Parse(typeof(Themes), text);

                var oldVal = mmApp.Configuration.ApplicationTheme;

                if (oldVal != selectedTheme &&
                    MessageBox.Show(
                        "Application theme changes require that you restart.\r\n\r\nDo you want to restart Markdown Monster?",
                        "Theme Change", MessageBoxButton.YesNo,
                        MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    mmApp.Configuration.ApplicationTheme = selectedTheme;
                    if (mmApp.Configuration.ApplicationTheme == Themes.Light)
                    {
                        mmApp.Configuration.EditorTheme = "vscodelight";
                        mmApp.Configuration.PreviewTheme = "Github";
                    }
                    else
                        mmApp.Configuration.EditorTheme = "vscodedark";

                    mmApp.Configuration.Write();

                    window.PipeManager.StopServer();
                    window.ForceClose = true;
                    window.Close();

                    // execute with delay
                    ShellUtils.ExecuteProcess(Path.Combine(App.InitialStartDirectory, "MarkdownMonster.exe"), "-delay");
                    Environment.Exit(0);
                }
            }, (p, c) => true);
        }



        public CommandBase ViewInExternalBrowserCommand { get; set; }

        void ViewInExternalBrowser()
        {
            // EXTERNAL BROWSER VIEW
            ViewInExternalBrowserCommand = new CommandBase((p, e) =>
            {
                if (Model.ActiveEditor?.MarkdownDocument == null)
                {
                    if (!string.IsNullOrEmpty(Model.ActiveTabFilename))
                        mmFileUtils.ShowExternalBrowser(Model.ActiveTabFilename);

                    return;
                }

                Model.ActiveDocument.RenderHtmlToFile();
                mmFileUtils.ShowExternalBrowser(Model.ActiveDocument.HtmlRenderFilename);
            });
        }


        public CommandBase ViewHtmlSourceCommand { get; set; }

        void ViewHtmlSource()
        {
            ViewHtmlSourceCommand = new CommandBase((p, e) =>
            {
                if (Model.ActiveDocument == null) return;
                var filename = Model.ActiveDocument.HtmlRenderFilename.Replace(MarkdownDocument.PREVIEW_HTML_FILENAME,
                    MarkdownDocument.PREVIEW_HTML_SOURCE_FILENAME);
                Model.ActiveDocument.RenderHtmlToFile(filename: filename);
                Model.Window.RefreshTabFromFile(filename);
            });
        }


        public CommandBase RefreshPreviewCommand { get; set; }

        void RefreshPreview()
        {
            RefreshPreviewCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.PreviewBrowser?.Refresh(true);
            });
        }


        public CommandBase RefreshBrowserContentCommand { get; set; }

        void RefreshBrowserContent()
        {
            RefreshBrowserContentCommand = new CommandBase((parameter, command) =>
            {
                if (Model.ActiveEditor == null)
                    return;

                var mode = Model.Configuration.PreviewSyncMode;
                Model.Configuration.PreviewSyncMode = MarkdownMonster.PreviewSyncMode.EditorToPreview;
                Model.ActiveEditor.SetEditorFocus();
                Model.ActiveEditor.PreviewMarkdownCallback();
                Model.Configuration.PreviewSyncMode = mode;
            }, (p, c) => true);
        }




        public CommandBase PrintPreviewCommand { get; set; }


        void PrintPreview()
        {
            PrintPreviewCommand = new CommandBase(
                (p, e) =>
                {
                    try
                    {
                        Model.Window.PreviewBrowser.ExecuteCommand("PrintPreview");
                    }
                    catch (Exception ex)
                    {
                        mmApp.Log("Print Preview failed: " + ex.Message, ex);
                        Model.Window.ShowStatusError("Print Preview failed: " + ex.Message);
                    }
                },
                (p, e) => Model.IsEditorActive);
        }

        #endregion


        public CommandBase TestButtonCommand { get; set; }

        private WebServer WebServer = null;

        void TestButton()
        {
            TestButtonCommand = new CommandBase((parameter, command) =>
            {
                if (WebServer == null)
                {
                    WebServerLauncher.StartMarkdownMonsterWebServer();
                }
                else
                {
                    WebServerLauncher.StopMarkdownMonsterWebServer();
                }
            });
        }
    }
}
