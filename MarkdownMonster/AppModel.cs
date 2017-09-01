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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Microsoft.Win32;
using Westwind.Utilities;

namespace MarkdownMonster
{
    /// <summary>
    /// Global App model for the application. Holds references to 
    /// top level components like the Window, configuration and more
    /// as well as includes a number of helper functions.
    /// 
    /// Available to Addins as `this.Model`.
    /// </summary>
    public class AppModel : INotifyPropertyChanged
    {

        #region Top Level Model Properties

        /// <summary>
        /// An instance of the main application WPF form
        /// </summary>
        public MainWindow Window { set; get; }

        /// <summary>
        /// The application's main configuration object
        /// </summary>
        public ApplicationConfiguration Configuration { get; set; }

        /// <summary>
        /// Returns an instance of the Active Editor instance. The editor contains
        /// editor behavior of the browser control as well as all interactions with
        /// the editor's event model and text selection interfaces.
        /// 
        /// Contains an `AceEditor` property that references the underlying 
        /// JavaScript editor wrapper instance.
        /// </summary>
        public MarkdownDocumentEditor ActiveEditor
        {
            get
            {
                var editor = Window.GetActiveMarkdownEditor();
                return editor;
            }
        }

        /// <summary>
        /// Returns the MarkdownDocument instance of the active editor
        /// </summary>
        public MarkdownDocument ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (value == _activeDocument)
                    return;

                _activeDocument = value;

                OnPropertyChanged(nameof(ActiveDocument));
                OnPropertyChanged(nameof(ActiveEditor));
                OnPropertyChanged(nameof(IsEditorActive));

                Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    CommandManager.InvalidateRequerySuggested();
                    //SaveCommand.InvalidateCanExecute();
                    //SaveAsHtmlCommand.InvalidateCanExecute();
                });

                Window.ToolbarEdit.IsEnabled = IsEditorActive;
            }
        }

        private MarkdownDocument _activeDocument;


        /// <summary>
        /// Gives a list of all the open documents as Markdown document instances
        /// </summary>
        public List<MarkdownDocument> OpenDocuments
        {
            get { return _openDocuments; }
            set
            {
                if (Equals(value, _openDocuments)) return;
                _openDocuments = value;
                OnPropertyChanged(nameof(OpenDocuments));
            }
        }

        private List<MarkdownDocument> _openDocuments;

        #endregion

        #region Display Modes

        /// <summary>
        /// Determines whether the preview browser is visible or not
        /// </summary>
        public bool IsPreviewBrowserVisible
        {
            get { return Configuration.IsPreviewVisible; }
            set
            {
                if (value == Configuration.IsPreviewVisible) return;
                Configuration.IsPreviewVisible = value;
                OnPropertyChanged(nameof(IsPreviewBrowserVisible));
            }
        }

        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set
            {
                if (value == _isFullScreen) return;
                _isFullScreen = value;
                OnPropertyChanged(nameof(IsFullScreen));
            }
        }

        private bool _isFullScreen = false;


        public bool IsPresentationMode
        {
            get { return _isPresentationMode; }
            set
            {
                if (_isPresentationMode == value) return;
                _isPresentationMode = value;
                OnPropertyChanged(nameof(IsPresentationMode));
            }
        }

        private bool _isPresentationMode;


        /// <summary>
        /// Determines if there's a document loaded 
        /// </summary>
        public bool IsEditorActive
        {
            get
            {
                if (ActiveDocument != null)
                    return true;

                return false;
            }
        }

        #endregion


        #region Statusbar Item Props

        /// <summary>
        /// A list of RenderThemes as retrieved based on the folder structure of hte
        /// Preview folder.
        /// </summary>
        public List<string> RenderThemeNames
        {
            get
            {
                if (_renderThemeNames == null || _renderThemeNames.Count < 1)
                {
                    var directories =
                        Directory.GetDirectories(Path.Combine(Environment.CurrentDirectory, "PreviewThemes"));
                    foreach (string dir in directories.OrderBy(d => d))
                    {
                        var dirName = Path.GetFileName(dir);

                        if (dirName.ToLower() == "scripts")
                            continue;

                        _renderThemeNames.Add(dirName);
                    }
                }

                return _renderThemeNames;
            }
        }

        private readonly List<string> _renderThemeNames = new List<string>();





        public List<PreviewSyncModeItem> PreviewSyncModeItems
        {
            get
            {
                if (_previewSyncModeItems != null)
                    return _previewSyncModeItems;

                _previewSyncModeItems = ((PreviewSyncMode[]) Enum.GetValues(typeof(PreviewSyncMode)))
                    .Select(e =>
                    {
                        var item = new PreviewSyncModeItem
                        {
                            Name = StringUtils.FromCamelCase(e.ToString()),
                            Value = e
                        };

                        if (e == PreviewSyncMode.PreviewToEditor)
                            item.Icon = FontAwesomeIcon.ArrowCircleLeft;
                        else if (e == PreviewSyncMode.EditorAndPreview)
                            item.Icon = FontAwesomeIcon.Exchange;
                        else if (e == PreviewSyncMode.None)
                            item.Icon = FontAwesomeIcon.Ban;
                        else
                            item.Icon = FontAwesomeIcon.ArrowCircleRight;

                        char c = (char) ((int) item.Icon);
                        item.IconString = c.ToString() + "   " + item.Name;

                        return item;
                    })
                    .ToList();
                return _previewSyncModeItems;
            }
        }

        private List<PreviewSyncModeItem> _previewSyncModeItems;


        /// <summary>
        /// A list of Ace Editor themes retrieved from the Editor script folder
        /// </summary>
        public List<string> EditorThemeNames
        {
            get
            {
                if (_editorThemeNames == null || _editorThemeNames.Count < 1)
                {
                    var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Editor\\Scripts\\Ace"),
                        "theme-*.js");
                    foreach (string file in files.OrderBy(d => d))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        fileName = fileName.Replace("theme-", "");

                        _editorThemeNames.Add(fileName);
                    }
                }
                return _editorThemeNames;
            }
        }

        private readonly List<string> _editorThemeNames = new List<string>();


        /// <summary>
        /// List of registered Markdown Parsers
        /// </summary>
        public List<string> MarkdownParserNames
        {
            get
            {
                if (!AddinManager.Current.AddinsLoadingComplete)
                    return null;

                var parsers = MarkdownParserFactory.GetParserNames();
                return parsers;
            }
        }

        /// <summary>
        /// Returns the width of the column containing
        /// the Markdown Parser selection combo box
        /// </summary>
        public int MarkdownParserColumnWidth
        {
            get
            {
                if (MarkdownParserFactory.GetParserNames().Count > 1)
                    return 180;

                return 0;
            }
        }

        #endregion

        #region Initialization

        public AppModel(MainWindow window)
        {

            Configuration = mmApp.Configuration;
            _openDocuments = new List<MarkdownDocument>();
            Window = window;

            CreateCommands();

            mmApp.Model = this;
        }

        #endregion


        #region Commands

        public CommandBase SaveCommand { get; set; }

        void Command_Save()
        {
            // SAVE COMMAND
            SaveCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl?.SelectedItem as TabItem;
                if (tab == null)
                    return;
                var doc = tab.Tag as MarkdownDocumentEditor;

                if (doc.MarkdownDocument.Filename == "untitled")
                    SaveAsCommand.Execute(tab);
                else if (!doc.SaveDocument())
                {
                    SaveAsCommand.Execute(tab);
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);

            }, (s, e) =>
            {
                if (ActiveDocument == null)
                    return false;

                return this.ActiveDocument.IsDirty;
            });
        }

        public CommandBase SaveAsCommand { get; set; }

        void Command_SaveAs()
        {
            SaveAsCommand = new CommandBase((parameter, e) =>
            {
                bool isEncrypted = parameter != null && parameter.ToString() == "Secure";

                var tab = Window.TabControl?.SelectedItem as TabItem;
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
                            filename = mmFileUtils.SafeFilename(filename);
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
                    OverwritePrompt = false,
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
                        Owner = Window
                    };
                    bool? pwdResult = pwdDialog.ShowDialog();
                }

                if (result != null && result.Value)
                {
                    doc.MarkdownDocument.Filename = sd.FileName;
                    if (!doc.SaveDocument())
                    {
                        MessageBox.Show(Window,
                            $"{sd.FileName}\r\n\r\nThis document can't be saved in this location. The file is either locked or you don't have permissions to save it. Please choose another location to save the file.",
                            "Unable to save Document", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SaveAsCommand.Execute(tab);
                        return;
                    }
                }

                mmApp.Configuration.LastFolder = folder;

                Window.SetWindowTitle();
                Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (ActiveDocument == null)
                    return false;

                return true;
            });
        }


        public CommandBase PreviewBrowserCommand { get; set; }

        void Command_PreviewBrowser()
        {
            PreviewBrowserCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                var editor = tab.Tag as MarkdownDocumentEditor;

                Configuration.IsPreviewVisible = IsPreviewBrowserVisible;

                if (!IsPreviewBrowserVisible && IsPresentationMode)
                    PresentationModeCommand.Execute(null);


                Window.ShowPreviewBrowser(!IsPreviewBrowserVisible);
                if (IsPreviewBrowserVisible)
                    Window.PreviewMarkdown(editor);

            }, null);


        }


        public CommandBase SaveAsHtmlCommand { get; set; }

        void Command_SaveAsHtml()
        {
            SaveAsHtmlCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl?.SelectedItem as TabItem;
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
                    OverwritePrompt = false,
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
                            MessageBox.Show(Window,
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
                            Window.Model.ViewInExternalBrowserCommand.Execute(null);
                    }
                }

                Window.PreviewMarkdown(doc, keepScrollPosition: true);
            }, (s, e) =>
            {
                if (ActiveDocument == null || ActiveEditor == null)
                    return false;
                if (ActiveDocument.Filename == "untitled")
                    return true;
                if (ActiveEditor.EditorSyntax != "markdown")
                    return false;

                return true;
            });
        }


        public CommandBase ToolbarInsertMarkdownCommand { get; set; }

        void Command_ToolbarInsertMarkdown()
        {
            ToolbarInsertMarkdownCommand = new CommandBase((s, e) =>
            {
                string action = s as string;

                var editor = Window.GetActiveMarkdownEditor();
                editor?.ProcessEditorUpdateCommand(action);
            }, null);


        }


        public CommandBase SettingsCommand { get; set; }

        void Command_Settings()
        {
            // Settings
            SettingsCommand = new CommandBase((s, e) =>
            {
                var file = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonster.json");

                // save settings first so we're looking at current setting
                Configuration.Write();

                string fileText = File.ReadAllText(file);
                if (!fileText.StartsWith("//"))
                {
                    fileText = "// Reference: http://markdownmonster.west-wind.com/docs/_4nk01yq6q.htm\r\n" +
                               fileText;
                    File.WriteAllText(file, fileText);
                }

                Window.OpenTab(file, syntax: "json");
            }, null);
        }



        public CommandBase DistractionFreeModeCommand { get; set; }

        void Command_DistractionFreeMode()
        {
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

                    IsPreviewBrowserVisible = true;
                    Window.PreviewMarkdown();

                    Window.WindowState = mmApp.Configuration.WindowPosition.WindowState;

                    IsFullScreen = false;

                    Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);
                }
                else
                {
                    var tokens = mmApp.Configuration.DistractionFreeModeHideOptions.ToLower()
                        .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

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
                        IsPreviewBrowserVisible = false;
                        Window.ShowPreviewBrowser(hide: true);
                    }

                    mmApp.Configuration.WindowPosition.WindowState = Window.WindowState;
                    if (tokens.Any(d => d == "maximized"))
                        Window.WindowState = WindowState.Maximized;

                    Window.ShowFolderBrowser(true);

                    IsFullScreen = true;
                }

                // toolbar     
                Window.MainMenuGridRow.Height = glMenu;
                Window.ToolbarGridRow.Height = glToolbar;
                Window.StatusBarGridRow.Height = glStatus;
            }, null);
        }


        public CommandBase PresentationModeCommand { get; set; }

        void Command_PresentationMode()
        {
            // PRESENTATION MODE
            PresentationModeCommand = new CommandBase((s, e) =>
            {
                if (IsFullScreen)
                    DistractionFreeModeCommand.Execute(null);

                GridLength gl = new GridLength(0);
                if (Window.WindowGrid.RowDefinitions[1].Height == gl)
                {
                    gl = GridLength.Auto; // toolbar height

                    Window.MainWindowEditorColumn.Width = new GridLength(1, GridUnitType.Star);
                    Window.MainWindowSeparatorColumn.Width = new GridLength(0);
                    Window.MainWindowPreviewColumn.Width =
                        new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);

                    Window.PreviewMarkdown();

                    Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

                    IsPresentationMode = false;
                }
                else
                {
                    Window.SaveSettings();

                    mmApp.Configuration.WindowPosition.SplitterPosition =
                        Convert.ToInt32(Window.MainWindowPreviewColumn.Width.Value);

                    // don't allow presentation mode for non-Markdown documents
                    var editor = Window.GetActiveMarkdownEditor();
                    if (editor != null)
                    {
                        var file = editor.MarkdownDocument.Filename.ToLower();
                        var ext = Path.GetExtension(file);
                        if (file != "untitled" && ext != ".md" && ext != ".htm" && ext != ".html")
                        {
                            // don't allow presentation mode for non markdown files
                            IsPresentationMode = false;
                            IsPreviewBrowserVisible = false;
                            Window.ShowPreviewBrowser(true);
                            return;
                        }
                    }

                    Window.ShowPreviewBrowser();
                    Window.ShowFolderBrowser(true);

                    Window.MainWindowEditorColumn.Width = gl;
                    Window.MainWindowSeparatorColumn.Width = gl;
                    Window.MainWindowPreviewColumn.Width = new GridLength(1, GridUnitType.Star);

                    IsPresentationMode = true;
                    IsPreviewBrowserVisible = true;
                }

                Window.WindowGrid.RowDefinitions[1].Height = gl;
                //Window.WindowGrid.RowDefinitions[3].Height = gl;  
            }, null);
        }


        public CommandBase NewDocumentCommand { get; set; }

        void Command_NewDocument()
        {

            // NEW DOCUMENT COMMAND (ctrl-n)
            NewDocumentCommand = new CommandBase((s, e) =>
            {
                Window.OpenTab("untitled");
            });
        }


        public CommandBase OpenDocumentCommand { get; set; }

        void Command_OpenDocument()
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
                    // TODO: Check AddRecentFile and make sure Tab Selection works
                    Window.OpenTab(file, rebindTabHeaders: true);
                    //Window.AddRecentFile(file);
                }
            });
        }


        public CommandBase CloseActiveDocumentCommand { get; set; }

        void Command_CloseActiveDocument()
        {

            // CLOSE ACTIVE DOCUMENT COMMAND
            CloseActiveDocumentCommand = new CommandBase((s, e) =>
            {
                var tab = Window.TabControl.SelectedItem as TabItem;
                if (tab == null)
                    return;

                if (Window.CloseTab(tab))
                    Window.TabControl.Items.Remove(tab);
            }, null)
            {
                Caption = "_Close Document",
                ToolTip = "Closes the active tab and asks to save the document."
            };


        }

        public CommandBase ViewInExternalBrowserCommand { get; set; }

        void Command_ViewInExternalBrowser()
        {
            // EXTERNAL BROWSER VIEW
            ViewInExternalBrowserCommand = new CommandBase((p, e) =>
            {
                if (ActiveDocument == null) return;
                mmFileUtils.ShowExternalBrowser(ActiveDocument.HtmlRenderFilename);
            }, (p, e) => IsPreviewBrowserVisible);
        }

        public CommandBase ViewHtmlSourceCommand { get; set; }

        void Command_ViewHtmlSource()
        {
            ViewHtmlSourceCommand = new CommandBase((p, e) =>
            {
                if (ActiveDocument == null) return;
                Window.OpenTab(ActiveDocument.HtmlRenderFilename);
            }, (p, e) => IsPreviewBrowserVisible);
        }


        public CommandBase PrintPreviewCommand { get; set; }

        void Command_PrintePreview()
        {
            // PRINT PREVIEW
            PrintPreviewCommand = new CommandBase((s, e) =>
            {
                dynamic dom = Window.PreviewBrowser.Document;
                dom.execCommand("print", true, null);
            }, (s, e) => IsPreviewBrowserVisible);

        }


        public CommandBase ShowFolderBrowserCommand { get; set; }


        void Command_ShowFolderBrowser()
        {
            // SHOW FILE BROWSER COMMAND
            ShowFolderBrowserCommand = new CommandBase((s, e) =>
            {
                mmApp.Configuration.FolderBrowser.Visible = !mmApp.Configuration.FolderBrowser.Visible;

                mmApp.Model.Window.ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

            });
        }


        public CommandBase HelpCommand { get; set; }

        void Command_Help()
        {
            // F1 Help Command - Pass option CommandParameter="TopicId"
            HelpCommand = new CommandBase((topicId, e) =>
            {
                string url = mmApp.Urls.DocumentationBaseUrl;

                if (topicId != null)
                    url = mmApp.GetDocumentionUrl(topicId as string);

                ShellUtils.GoUrl(url);
            });
        }

        public CommandBase GeneratePdfCommand { get; set; }

        void Command_GeneratePdf()
        {
            // PDF GENERATION PREVIEW
            GeneratePdfCommand = new CommandBase((s, e) =>
            {
                var form = new GeneratePdfWindow()
                {
                    Owner = mmApp.Model.Window
                };
                form.Show();
            }, (s, e) => IsPreviewBrowserVisible);
        }


        public CommandBase CommitToGitCommand { get; set; }


        void Command_CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase(async (s, e) =>
            {
                string file = ActiveDocument?.Filename;
                if (string.IsNullOrEmpty(file))
                    return;

                Window.ShowStatus("Committing and pushing to Git...");
                WindowUtilities.DoEvents();

                string error = null;

                bool pushToGit = mmApp.Configuration.GitCommitBehavior == GitCommitBehaviors.CommitAndPush;
                bool result = await Task.Run(() => mmFileUtils.CommitFileToGit(file, pushToGit, out error));

                if (result)
                    Window.ShowStatus($"File {Path.GetFileName(file)} committed and pushed.", 6000);
                else
                {
                    Window.ShowStatus(error, 7000);
                    Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
                }
            }, (s, e) => IsEditorActive);
        }


        public CommandBase WordWrapCommand { get; set; }

        void Command_WordWrap()
        {

            // WORD WRAP COMMAND
            WordWrapCommand = new CommandBase((parameter, command) =>
                {
                    //MessageBox.Show("alt-z WPF");
                    mmApp.Model.Configuration.EditorWrapText = !mmApp.Model.Configuration.EditorWrapText;
                    mmApp.Model.ActiveEditor?.SetWordWrap(mmApp.Model.Configuration.EditorWrapText);
                },
                (p, c) => IsEditorActive);
        }

        public CommandBase CopyAsHtmlCommand { get; set; }

        void Command_CopyAsHtml()
        {
            CopyAsHtmlCommand = new CommandBase((parameter, command) =>
            {
                if (ActiveEditor == null)
                    return;

                var editor = ActiveEditor;
                if (editor == null)
                    return;

                var markdown = editor.GetSelection();
                var html = editor.RenderMarkdown(markdown);

                if (!string.IsNullOrEmpty(html))
                {
                    // copy to clipboard as html
                    ClipboardHelper.CopyHtmlToClipboard(html, html);
                    Window.ShowStatus("Html has been pasted to the clipboard.", mmApp.Configuration.StatusTimeout);
                }
                editor.SetEditorFocus();
                editor.Window.PreviewMarkdownAsync();
            }, (p, c) => IsEditorActive);

        }



        private void CreateCommands()
        {
            Command_Save();
            Command_SaveAs();
            Command_PreviewBrowser();
            Command_SaveAsHtml();

            Command_DistractionFreeMode();
            Command_PresentationMode();

            Command_ToolbarInsertMarkdown();
            Command_Settings();


            Command_NewDocument();
            Command_OpenDocument();
            Command_CloseActiveDocument();

            Command_ViewInExternalBrowser();
            Command_ViewHtmlSource();
            Command_PrintePreview();

            Command_ShowFolderBrowser();
            Command_Help();

            Command_GeneratePdf();
            Command_CommitToGit();
            Command_WordWrap();
            Command_CopyAsHtml();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;


        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }

    public class PreviewSyncModeItem
    {
        public string Name { get; set; }
        public PreviewSyncMode Value { get; set; }
        public FontAwesomeIcon Icon { get; set; }
        public string IconString { get; set; }
    }
}
