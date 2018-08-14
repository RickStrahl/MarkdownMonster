using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Favorites;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class 
        AppCommands
    {
        AppModel Model;

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


            // Editor Commands
            ToolbarInsertMarkdown();
            CloseActiveDocument();
            CloseAllDocuments();
            ShowActiveTabsList();
            CopyAsHtml();
            SetDictionary();
            SpellCheck();
            CommandWindow();
            OpenInExplorer();
            PasteMarkdownFromHtml();
            AddFavorite();


            // Preview Browser
            EditPreviewTheme();
            PreviewSyncMode();
            ViewInExternalBrowser();
            ViewHtmlSource();
            RefreshPreview();


            // Miscellaneous
            OpenAddinManager();

            Help();
            CopyFolderToClipboard();
            CopyFullPathToClipboard();
            TabControlFileList();

            // Git
            OpenGitClient();
            OpenFromGitRepo();
            CommitToGit();
            OpenOnGithub();


            // Sidebar
            CloseLeftSidebarPanel();
            CloseRightSidebarPanel();
            OpenLeftSidebarPanel();
            ToggleFolderBrowser();
            OpenFolderBrowser();


#if DEBUG
            TestButton();
            var mi = new MenuItem
            {
                Header = "Test Item"
            };
            mi.Click +=  (s,ev)=> TestButtonCommand.Execute(mi.CommandParameter);
            Model.Window.MainMenuHelp.Items.Add(mi);
#endif
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

                if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder) && Directory.Exists(mmApp.Configuration.LastFolder))
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
                    Model.Window.ShowStatusError($"Can't open from url: {ex.Message}");
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
                ((MarkdownDocumentEditor) tab.Tag).MarkdownDocument.CurrentText = markdown;
                Model.Window.PreviewMarkdownAsync();

            });
        }



        public CommandBase SaveCommand { get; set; }

        void Save()
        {
            // SAVE COMMAND
            SaveCommand = new CommandBase((s, e) =>
            {
                var tab = Model.Window.TabControl?.SelectedItem as TabItem;
                var doc = tab?.Tag as MarkdownDocumentEditor;
                if (doc == null)
                    return;

                if (doc.MarkdownDocument.Filename == "untitled" || !doc.SaveDocument())
                    SaveAsCommand.Execute(tab);
                
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
                
                var doc = tab?.Tag as MarkdownDocumentEditor;
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
            }, (s, e) => Model.IsEditorActive);
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
                    if (tab == null)
                        continue;

                    var doc = tab?.Tag as MarkdownDocumentEditor;
                    if (doc == null)
                        continue;

                    if (doc.MarkdownDocument.Filename == "untitled" || !doc.SaveDocument())
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

                SaveFileDialog sd = new SaveFileDialog
                {
                    Filter =
                        "Self contained Html Page with embedded dependencies (1 large file)|*.html|" +
                        "Html Page with loose assets in Folder (pick an empty folder)|*.html|" +
                        "Raw Html Fragment (generated Html only)|*.html",
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
                    if (sd.FilterIndex == 3)
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
                    }                    
                    else
                    {

                        if (doc.MarkdownDocument.RenderHtmlToFile(usePragmaLines: false,
                                renderLinksExternal: mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal,
                                filename: sd.FileName) == null)
                        {
                            MessageBox.Show(Model.Window,
                                $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                                "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                            SaveAsHtmlCommand.Execute(null);
                            return;
                        }

                        try
                        {
                            Model.Window.ShowStatus("Packing HTML File. This can take a little while.",
                                mmApp.Configuration.StatusMessageTimeout, FontAwesomeIcon.CircleOutlineNotch,
                                color: Colors.Goldenrod, spin: true);
                            
                            var packager = new HtmlPackager();

                            bool packageResult;                            
                            if(sd.FilterIndex == 1)
                                packageResult = packager.PackageHtmlToFile(sd.FileName, sd.FileName);
                            else
                            {
                                folder = Path.GetDirectoryName(sd.FileName);
                                if (Directory.GetFiles(folder).Any() &&                               
                                    MessageBox.Show("The Html file and resources will be generated here:\r\n\r\n" +
                                                    $"Folder: {folder}\r\n" +
                                                    $"File: {Path.GetFileName(sd.FileName)}\r\n\r\n" +
                                                    "Files already exist in this folder." +
                                                    "Are you sure you want to generate Html and resources here?",
                                        "Save As Html file with Loose Resources",
                                        MessageBoxButton.YesNo,MessageBoxImage.Question,MessageBoxResult.Yes) == MessageBoxResult.No)
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
                }
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

                    Model.Window.ShowStatusError("Didn't remove formatting. No selection or document is not a Markdown document.");
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

                bool toggle = p?.ToString()  == "Toggle";
                if (toggle)
                    Model.IsPreviewBrowserVisible = !Model.IsPreviewBrowserVisible;

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
                var mi = Model.Window.MainMenuWindow;
                mi.Items.Clear();

                mi.Items.Add(new MenuItem
                {
                    Header = "_Close Document",
                    Command = Model.Commands.CloseActiveDocumentCommand
                });
                mi.Items.Add(new MenuItem
                {
                    Header = "C_lose All Documents",
                    Command = Model.Commands.CloseAllDocumentsCommand,
                    CommandParameter = "All"
                });
                mi.Items.Add(new MenuItem
                {
                    Header = "Close All _But This Document",
                    Command = Model.Commands.CloseAllDocumentsCommand,
                    CommandParameter = "AllBut"
                });

                var menuItems = Model.Window.GenerateContextMenuItemsFromOpenTabs();
                if (menuItems.Count < 1)
                    return;

                mi.Items.Add(new Separator());
                foreach (var menu in menuItems)
                {
                    mi.Items.Add(menu);
                }

                mi.IsSubmenuOpen = true;
                mi.Focus();                
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
                    bool error = false;
                    foreach (var file in Directory.GetFiles(SpellChecker.ExternalDictionaryFolder))
                    {
                        if (file.EndsWith(".dic") || file.EndsWith(".aff"))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch {  error = true;}
                        }                            
                    }
                    if(error)
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
                    Model.ActiveEditor.SpellCheckDocument();
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
                Model.Window.ShowStatusSuccess($"Spell checking has been turned {(Model.Configuration.Editor.EnableSpellcheck ? "on" : "off")}.");
            }, (p, c) => true);
        }


        public CommandBase CommandWindowCommand { get; set; }

        void CommandWindow()
        {
            CommandWindowCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);
                mmFileUtils.OpenTerminal(path);
            }, (p, c) => true);
        }


        public CommandBase OpenInExplorerCommand { get; set; }

        void OpenInExplorer()
        {
            OpenInExplorerCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                ShellUtils.OpenFileInExplorer(editor.MarkdownDocument.Filename);
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

                Model.Window.PreviewBrowser.PreviewMarkdownAsync(editor, true);

            }, (p, c) => true);
        }



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

                var favorite = fav.AddFavorite(null, new FavoriteItem {File = file} );
                fav.SaveFavorites();

                fav.EditedFavorite = favorite;
                favorite.DisplayState.IsEditing = true;                
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
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                if (editor.MarkdownDocument.Filename == "untitled")
                    return;

                string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);

                if(ClipboardHelper.SetText(path))
                    Model.Window.ShowStatus($"Path copied to clipboard: {path}", mmApp.Configuration.StatusMessageTimeout);                
            });
        }



        public CommandBase CopyFullPathToClipboardCommand { get; set; }

        void CopyFullPathToClipboard()
        {
            CopyFullPathToClipboardCommand = new CommandBase((parameter, command) =>
            {
                var editor = Model.ActiveEditor;
                if (editor == null)
                    return;

                if (editor.MarkdownDocument.Filename == "untitled")
                    return;

                string path = editor.MarkdownDocument.Filename;

                if (ClipboardHelper.SetText(path))
                    Model.Window.ShowStatus($"Path copied to clipboard: {path}", mmApp.Configuration.StatusMessageTimeout);
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
            });
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
        }


        public CommandBase CommitToGitCommand { get; set; }

        void CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase( (parameter, e) =>
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
                    Model.Window.ShowStatusError($"There are no pending changes for this Git repository: {repo.Info.WorkingDirectory}");

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
                    Model.Window.ShowStatusError("Unabled to open Git client.");
                else
                    Model.Window.ShowStatus("Git client opened.",mmApp.Configuration.StatusMessageTimeout);
            }, (p, c) => !string.IsNullOrEmpty(Model.Configuration.Git.GitClientExecutable));
        }



        public CommandBase OpenOnGithubCommand { get; set; }

        void OpenOnGithub()
        {
            OpenOnGithubCommand = new CommandBase((parameter, command) =>
            {
                var filename = parameter as string;
                if(parameter == null)
                    return;

                var CommitModel = new GitCommitModel(filename);
                                
                using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
                {
                    var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                    if (remoteUrl == null)
                        return;

                    var relativeFilename = FileUtils.GetRelativePath(filename, repo.Info.WorkingDirectory).Replace("\\","/");
                    
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

                button.ContextMenu = new ContextMenu();
                foreach (var mi in menuItems)
                    button.ContextMenu.Items.Add(mi);

                button.ContextMenu.IsOpen = true;
                button.ContextMenu.Closed += (o, args) => button.ContextMenu.Items.Clear();
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
            OpenLeftSidebarPanelCommand = new CommandBase((parameter, command) =>
            {
                Model.Window.ShowLeftSidebar();
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
                string folder = parameter as string;
                if (string.IsNullOrEmpty(folder))
                {
                    var editor = mmApp.Model.ActiveEditor;
                    if (editor == null)
                        return;
                    folder = editor.MarkdownDocument.Filename;
                }

                // Is it a file instead
                if (File.Exists(folder))                
                    folder = Path.GetDirectoryName(folder);
                
                mmApp.Model.Window.SidebarContainer.SelectedItem = mmApp.Model.Window.TabFolderBrowser;
                mmApp.Model.Window.ShowFolderBrowser(folder: folder);

            }, (p, c) => true);
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
                    if (!App.IsPortableMode && mmApp.Configuration.CommonFolder != mmApp.Configuration.InternalCommonFolder)
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






        public CommandBase ViewInExternalBrowserCommand { get; set; }

        void ViewInExternalBrowser()
        {
            // EXTERNAL BROWSER VIEW
            ViewInExternalBrowserCommand = new CommandBase((p, e) =>
            {
                if (Model.ActiveDocument == null) return;

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
                Model.ActiveDocument.RenderHtmlToFile();
                Model.Window.OpenTab(Model.ActiveDocument.HtmlRenderFilename);
            });
        }



        public CommandBase RefreshPreviewCommand { get; set; }

        void RefreshPreview()
        {
            RefreshPreviewCommand = new CommandBase((parameter, command) =>
                {
                    Model.Window.PreviewBrowser.Refresh(true);
                });
        }


        public CommandBase PrintPreviewCommand { get; set; }
        

        void PrintPreview()
        {
            PrintPreviewCommand = new CommandBase(
                (p, e) => Model.Window.PreviewBrowser.ExecuteCommand("PrintPreview"),
                (p, e) => Model.IsEditorActive);

        }
#endregion


        public CommandBase TestButtonCommand { get; set; }

        void TestButton()
        {

            TestButtonCommand = new CommandBase((parameter, command) =>
            {
                var form = new BrowserMessageBox()
                {
                    Owner = Model.Window
                };
                form.ButtonClickHandler = (s, ev, f) =>
                {                    
                    var button = s as Button;
                    var selection = button.CommandParameter as string;

                    if (selection == "1")                        
                        MessageBox.Show("You clicked: " + "Accept");
                    else
                        MessageBox.Show("You clicked: " + "Cancel");

                    return true;
                };

                var btn =
                    new Button()
                    {
                        Content = "Another Button"
                    };
                btn.Click += (s, e) => MessageBox.Show("Another Button Clicked");
                form.AddButton(btn);


                //download license
                var wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                var md = wc.DownloadString(
                    "https://raw.githubusercontent.com/wooorm/dictionaries/master/dictionaries/bg/LICENSE");

                form.ShowMarkdown(md);
                form.Icon = Model.Window.Icon;
                form.ButtonOkText.Text = "Accept";
                form.SetMessage("Please accept the license for the dictionary:");
                form.ShowDialog();

            }, (p, c) => true);
        }

    }
}
